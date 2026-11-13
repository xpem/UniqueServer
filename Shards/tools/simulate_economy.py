#!/usr/bin/env python3
"""
Simulação da economia do Shards.

Lê o balanceamento REAL de UniqueServer/appsettings.json (seção Shards:Balance), então os números nunca divergem do jogo.
Usa valores ESPERADOS (sem sorteio) e passos de 1 hora. Não altera nada no projeto: só imprime relatórios.

Uso:
    python Shards/tools/simulate_economy.py            # tudo
    python Shards/tools/simulate_economy.py --days 90  # horizonte da linha do tempo
    python Shards/tools/simulate_economy.py --markdown # saída em Markdown (para colar em documentos)

O que NÃO está modelado (de propósito): recompensas de missões e diárias, o mercado (não existe), depósitos (ainda não existem),
o tempo que o jogador demora para decidir. Os depósitos entram só como análise paramétrica (seção 3).
"""
import argparse
import json
import math
import os
import sys
from dataclasses import dataclass, field

CONFIG = os.path.join(os.path.dirname(__file__), '..', '..', 'UniqueServer', 'appsettings.json')
GEMS = ['Sapphire', 'Ruby', 'Emerald']  # ordem de compra das produções (do mais barato ao mais caro)


# ----------------------------------------------------------------------------- configuração
def load_config(path=CONFIG):
    with open(path, encoding='utf-8-sig') as f:
        return json.load(f)['Shards']['Balance']


class Cfg:
    def __init__(self, raw):
        e = raw['Energy']
        self.max_energy = e['MaxEnergy']
        self.energy_per_hour = 3600 / e['SecondsPerPoint']
        self.xp_first = raw['Experience']['XpFirstLevel']
        self.dd_per_level = raw['DoubleDrop']['ChancePerLevel']
        self.dd_max = raw['DoubleDrop']['MaxLevel']
        self.slots_base = raw['MineSlots']['BaseSlots']
        self.slots_max = raw['MineSlots']['MaxLevel']
        self.mines = {m['MineNumber']: m for m in raw['Mines']}
        self.cart_upgrades = sorted(raw['CartUpgrades'], key=lambda u: u['Level'])
        self.workshop = {w['Stage']: w for w in raw['Workshop']}
        self.productions = {}
        for p in raw['Productions']:
            total = sum(d['Chance'] for d in p['Drops'])
            self.productions[p['Focus']] = {
                'cost': sum(c['Quantity'] for c in p['SwitchCost']),
                'drops': [(d['Ore'], d['Chance'] / total, d['Xp']) for d in p['Drops']],
            }
        self.attack = {o['Ore']: o.get('Attack', {}) for o in raw.get('Ores', [])}
        self.defense = {o['Ore']: o.get('Defense', {}) for o in raw.get('Ores', [])}
        matrix = [e['Multiplier'] for e in raw.get('Combat', {}).get('Effectiveness', [])]
        self.advantage = max([m for m in matrix if m > 1] or [1.0])     # maior vantagem da matriz de efetividade
        self.disadvantage = min([m for m in matrix if 0 < m < 1] or [1.0])  # maior desvantagem (ignora imunidade)
        self.epc = raw.get('Combat', {}).get('EnergyPerCounterPoint', 3)  # energia de exploração por ponto de contra-ataque não absorvido
        self.tiers = sorted(raw.get('Deposits', {}).get('Tiers', []), key=lambda t: t['Slot'])
        self.stock_per_defense = raw.get('Deposits', {}).get('StockPerDefense', 2.5)
        self.energy_per_extraction = raw.get('Deposits', {}).get('EnergyPerExtraction', 1)
        self.cart_max_level = len(self.cart_upgrades)
        self.workshop_max_level = len(self.workshop)

    def xp_for_level(self, n):
        return self.xp_first * n * (n + 1) / 2

    def level_for_xp(self, xp):
        n = 0
        while self.xp_for_level(n + 1) <= xp:
            n += 1
        return n

    def expected_per_roll(self, focus):
        """(quartzo, gema) esperados por minério da produção, e o XP esperado por mineração ativa."""
        prod = self.productions[focus]
        quartz = sum(c for ore, c, _ in prod['drops'] if ore == 'Quartz')
        gem = sum(c for ore, c, _ in prod['drops'] if ore != 'Quartz')
        xp = sum(c * x for _, c, x in prod['drops'])
        return quartz, gem, xp


# ----------------------------------------------------------------------------- estado do jogador simulado
@dataclass
class Mine:
    number: int
    production: str = 'Quartz'
    cart_level: int = 0
    capacity: int = 100
    last_collect_hour: float = 0.0


@dataclass
class Player:
    quartz: float = 0.0
    raw: dict = field(default_factory=lambda: {g: 0.0 for g in GEMS})
    polished: dict = field(default_factory=lambda: {g: 0.0 for g in GEMS})
    faceted: dict = field(default_factory=lambda: {g: 0.0 for g in GEMS})
    mines: list = field(default_factory=list)
    skills: dict = field(default_factory=lambda: {'DoubleDrop': 0, 'MineSlots': 0, 'CartCapacity': 0, 'Workshop': 0})
    xp: float = 0.0
    energy: float = 100.0
    gross_quartz: float = 0.0   # quartzo que entrou (sem contar o que foi gasto)
    spent_quartz: float = 0.0
    gross_gems: float = 0.0     # gemas brutas que entraram
    log: dict = field(default_factory=dict)


def skill_points_available(cfg, p):
    return cfg.level_for_xp(p.xp) - sum(p.skills.values())


def slots(cfg, p):
    return cfg.slots_base + min(p.skills['MineSlots'], cfg.slots_max)


# ----------------------------------------------------------------------------- estratégias
@dataclass
class Strategy:
    name: str
    visits_per_day: float      # 4 = a cada 6 h, 1 = diário, 0,5 = a cada 2 dias
    gems: bool                 # troca minas para gemas?
    craft: bool = False        # lapida com o excedente?
    craft_reserve: float = 1500  # quartzo mínimo mantido antes de lapidar (e nunca abaixo da próxima compra estrutural)
    gem_mines: tuple = (2, 3, 4, 5)  # quais minas viram gema (as outras ficam em quartzo)
    active_on_gem: bool = True       # a mineração ativa usa a mina de gema (True) ou a de quartzo (False)


def allocate_skill(cfg, p, strategy):
    """Um ponto de habilidade: espaços de mina primeiro (se faltam), depois vagonete, oficina (se lapida) e drop duplo."""
    if p.skills['MineSlots'] < cfg.slots_max and len(p.mines) >= slots(cfg, p):
        return 'MineSlots'
    if p.skills['CartCapacity'] < cfg.cart_max_level:
        return 'CartCapacity'
    if strategy.craft and p.skills['Workshop'] < cfg.workshop_max_level:
        return 'Workshop'
    if p.skills['MineSlots'] < cfg.slots_max:
        return 'MineSlots'
    if p.skills['DoubleDrop'] < cfg.dd_max:
        return 'DoubleDrop'
    return None


def gem_target(strategy, mine):
    """Quais minas viram gema (strategy.gem_mines), na ordem Safira, Rubi, Esmeralda; as demais ficam em quartzo."""
    if not strategy.gems or mine.number not in strategy.gem_mines:
        return 'Quartz'
    order = sorted(strategy.gem_mines)
    return GEMS[min(order.index(mine.number), len(GEMS) - 1)]


def pending_structural_costs(cfg, p, strategy):
    """Custos das compras estruturais que o jogador já pode fazer (mina, produção, vagonete). A lapidação só usa o que sobra acima delas."""
    costs = []
    nxt = len(p.mines) + 1
    if nxt in cfg.mines and len(p.mines) < slots(cfg, p):
        costs.append(sum(c['Quantity'] for c in cfg.mines[nxt]['UnlockCost']))
    if strategy.gems:
        for mine in p.mines:
            target = gem_target(strategy, mine)
            if mine.production != target:
                costs.append(cfg.productions[target]['cost'])
                break
    cands = [m for m in p.mines if m.cart_level < min(p.skills['CartCapacity'], cfg.cart_max_level)]
    if cands:
        mine = min(cands, key=lambda m: m.cart_level)
        costs.append(sum(c['Quantity'] for c in cfg.cart_upgrades[mine.cart_level]['Cost']))
    return costs


def try_purchases(cfg, p, strategy, hour):
    """Compra o que a estratégia quer, na ordem: minas, produção, vagonete, lapidação. Devolve True se comprou algo."""
    # 1) próxima mina (precisa de espaço e quartzo)
    nxt = len(p.mines) + 1
    if nxt in cfg.mines and len(p.mines) < slots(cfg, p):
        cost = sum(c['Quantity'] for c in cfg.mines[nxt]['UnlockCost'])
        if p.quartz >= cost:
            p.quartz -= cost; p.spent_quartz += cost
            m = cfg.mines[nxt]
            p.mines.append(Mine(nxt, capacity=m['CartCapacity'], last_collect_hour=hour))
            p.log.setdefault(f'mina {nxt}', hour)
            return True

    # 2) produção em gema (estratégias de gema)
    if strategy.gems:
        for mine in p.mines:
            target = gem_target(strategy, mine)
            if mine.production != target:
                cost = cfg.productions[target]['cost']
                if p.quartz >= cost:
                    p.quartz -= cost; p.spent_quartz += cost
                    collect_cart(cfg, p, mine, hour)  # a troca esvazia o vagonete antes
                    mine.production = target
                    p.log.setdefault(f'mina {mine.number} em {target}', hour)
                    return True
                break  # espera juntar para a próxima gema (não pula a ordem)

    # 3) vagonete: a mina de menor nível primeiro (a skill é o teto)
    candidates = [m for m in p.mines if m.cart_level < min(p.skills['CartCapacity'], cfg.cart_max_level)]
    if candidates:
        mine = min(candidates, key=lambda m: m.cart_level)
        up = cfg.cart_upgrades[mine.cart_level]
        cost = sum(c['Quantity'] for c in up['Cost'])
        if p.quartz >= cost:
            p.quartz -= cost; p.spent_quartz += cost
            collect_cart(cfg, p, mine, hour)
            mine.cart_level = up['Level']; mine.capacity = up['Capacity']
            p.log.setdefault(f'vagonete nível {mine.cart_level}', hour)
            return True

    # 4) lapidação com o excedente
    if strategy.craft:
        pending = pending_structural_costs(cfg, p, strategy)
        reserve = max(strategy.craft_reserve, min(pending) if pending else 0)
        return try_craft(cfg, p, strategy, hour, reserve)
    return False


def try_craft(cfg, p, strategy, hour, reserve):
    pol = cfg.workshop.get('Polished'); fac = cfg.workshop.get('Faceted')
    for gem in GEMS:
        if fac and p.skills['Workshop'] >= fac['Level'] and p.polished[gem] >= fac['InputQuantity']:
            cost = sum(c['Quantity'] for c in fac['Cost'])
            if p.quartz - cost >= reserve:
                p.quartz -= cost; p.spent_quartz += cost
                p.polished[gem] -= fac['InputQuantity']; p.faceted[gem] += 1
                p.log.setdefault('primeira facetada', hour)
                return True
    for gem in GEMS:
        if pol and p.skills['Workshop'] >= pol['Level'] and p.raw[gem] >= pol['InputQuantity']:
            cost = sum(c['Quantity'] for c in pol['Cost'])
            if p.quartz - cost >= reserve:
                p.quartz -= cost; p.spent_quartz += cost
                p.raw[gem] -= pol['InputQuantity']; p.polished[gem] += 1
                p.log.setdefault('primeira polida', hour)
                return True
    return False


# ----------------------------------------------------------------------------- ações do jogador
def add_gems(p, focus, amount):
    if focus in p.raw:
        p.raw[focus] += amount
        p.gross_gems += amount


def collect_cart(cfg, p, mine, hour):
    hours = max(0.0, hour - mine.last_collect_hour)
    rate = cfg.mines[mine.number]['OresPerMinute']
    amount = math.floor(min(mine.capacity, rate * 60 * hours))
    if amount > 0:
        q, g, _ = cfg.expected_per_roll(mine.production)
        p.quartz += amount * q; p.gross_quartz += amount * q
        add_gems(p, mine.production, amount * g)
        mine.last_collect_hour = hour


def mine_active(cfg, p, strategy):
    """Gasta toda a energia na melhor mina (gema, se houver), com o drop duplo esperado."""
    n = math.floor(p.energy)
    if n <= 0:
        return
    gem_mines = [m for m in p.mines if m.production != 'Quartz']
    quartz_mines = [m for m in p.mines if m.production == 'Quartz']
    best = gem_mines[0] if ((strategy.active_on_gem and gem_mines) or not quartz_mines) else quartz_mines[0]
    q, g, xp = cfg.expected_per_roll(best.production)
    mult = 1 + cfg.dd_per_level * min(p.skills['DoubleDrop'], cfg.dd_max)
    p.quartz += n * q * mult; p.gross_quartz += n * q * mult
    add_gems(p, best.production, n * g * mult)
    p.xp += n * xp
    p.energy -= n


def visit(cfg, p, strategy, hour):
    for mine in p.mines:
        collect_cart(cfg, p, mine, hour)
    mine_active(cfg, p, strategy)
    while True:
        pts = skill_points_available(cfg, p)
        if pts <= 0:
            break
        s = allocate_skill(cfg, p, strategy)
        if s is None:
            break
        p.skills[s] += 1
    while try_purchases(cfg, p, strategy, hour):
        pass


def simulate(cfg, strategy, days):
    p = Player()
    p.mines.append(Mine(1, capacity=cfg.mines[1]['CartCapacity']))
    gap = round(24 / strategy.visits_per_day)  # intervalo entre visitas, em horas
    timeline = []
    last = {'gq': 0.0, 'gg': 0.0, 'sq': 0.0}
    for day in range(1, days + 1):
        for h in range(24):
            hour = (day - 1) * 24 + h
            p.energy = min(cfg.max_energy, p.energy + cfg.energy_per_hour)
            if (hour - 8) % gap == 0:  # primeira visita às 08:00 do dia 1 (e as anteriores no mesmo ritmo)
                visit(cfg, p, strategy, hour)
        timeline.append({
            'day': day, 'mines': len(p.mines), 'quartz': p.quartz,
            'income_q': p.gross_quartz - last['gq'], 'income_gems': p.gross_gems - last['gg'], 'spent_q': p.spent_quartz - last['sq'],
            'carts': [m.cart_level for m in p.mines], 'prod': [m.production[0] for m in p.mines],
            'raw': sum(p.raw.values()), 'polished': sum(p.polished.values()), 'faceted': sum(p.faceted.values()),
            'level': cfg.level_for_xp(p.xp), 'skills': dict(p.skills),
        })
        last = {'gq': p.gross_quartz, 'gg': p.gross_gems, 'sq': p.spent_quartz}
    return p, timeline


# ----------------------------------------------------------------------------- relatórios
def table(rows, headers, md):
    widths = [max(len(str(r[i])) for r in [headers] + rows) for i in range(len(headers))]
    if md:
        out = ['| ' + ' | '.join(str(h) for h in headers) + ' |', '|' + '|'.join('---' for _ in headers) + '|']
        out += ['| ' + ' | '.join(str(c) for c in r) + ' |' for r in rows]
    else:
        out = ['  '.join(str(h).ljust(w) for h, w in zip(headers, widths)), '  '.join('-' * w for w in widths)]
        out += ['  '.join(str(c).ljust(w) for c, w in zip(r, widths)) for r in rows]
    return '\n'.join(out)


def fmt(x):
    return f'{x:,.0f}'.replace(',', '.')


def report_static(cfg, md):
    print('\n## 1. Renda por mina e por dia (valores esperados)\n' if md else '\n=== 1. Renda por mina e por dia ===')
    rows = []
    for visits in (2, 4, 6, 1, 0.5):
        gap = 24 / visits
        for level in range(0, cfg.cart_max_level + 1):
            cap = 100 if level == 0 else cfg.cart_upgrades[level - 1]['Capacity']
            if level in (0, cfg.cart_max_level) or visits in (4, 1, 0.5):
                per_collect = min(cap, cfg.mines[1]['OresPerMinute'] * 60 * gap)
                rows.append([visits, level, cap, f'{gap:.0f} h', f'{per_collect:.0f}', fmt(per_collect * visits)])
    print(table(rows, ['visitas/dia', 'nível do vagonete', 'capacidade', 'entre visitas', 'minérios por coleta', 'minérios por dia (1 mina)'], md))
    print('\nObservação: com poucas visitas, o vagonete (não a energia) limita a renda; ampliar o vagonete só rende para quem visita pouco.')
    q, g, _ = cfg.expected_per_roll('Sapphire')
    print(f'\nMina em gema: por minério, {g:.0%} de gema e {q:.0%} de quartzo (a mina de quartzo dá 100% de quartzo). '
          f'Ao trocar uma mina para gema, a renda de quartzo dela cai {1 - q:.0%}.')
    print(f'Energia: {cfg.max_energy} pontos, {cfg.energy_per_hour:.2f} por hora = {cfg.energy_per_hour * 24:.0f} minerações ativas por dia (se todas forem gastas).')


def report_timeline(cfg, days, md):
    print(f'\n## 2. Linha do tempo de um jogador ({days} dias)\n' if md else f'\n=== 2. Linha do tempo ({days} dias) ===')
    strategies = [
        Strategy('só quartzo, 4 visitas/dia', 4, gems=False),
        Strategy('só quartzo, 2 visitas/dia', 2, gems=False),
        Strategy('gemas em todas as minas 2+ (ativa na gema), lapida, 4 visitas/dia', 4, gems=True, craft=True),
        Strategy('mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 4 visitas/dia', 4, gems=True, craft=True, gem_mines=(2, 3), active_on_gem=False),
        Strategy('mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 2 visitas/dia', 2, gems=True, craft=True, gem_mines=(2, 3), active_on_gem=False),
        Strategy('só quartzo, 1 visita/dia', 1, gems=False),
        Strategy('só quartzo, 1 visita a cada 2 dias', 0.5, gems=False),
        Strategy('mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 1 visita/dia', 1, gems=True, craft=True, gem_mines=(2, 3), active_on_gem=False),
        Strategy('mista: gemas só nas minas 2 e 3 (ativa no quartzo), lapida, 1 visita a cada 2 dias', 0.5, gems=True, craft=True, gem_mines=(2, 3), active_on_gem=False),
    ]
    marks = [1, 3, 7, 14, 30, 60, days]
    results = []
    for s in strategies:
        p, tl = simulate(cfg, s, days)
        print(f'\n### {s.name}\n' if md else f'\n--- {s.name} ---')
        rows = []
        for d in marks:
            t = tl[d - 1]
            avg7 = tl[max(0, d - 7):d]
            inc = sum(x['income_q'] for x in avg7) / len(avg7)
            gem = sum(x['income_gems'] for x in avg7) / len(avg7)
            rows.append([d, t['level'], t['mines'], ''.join(map(str, t['carts'])) or '-', ''.join(t['prod']), fmt(t['quartz']),
                         fmt(inc), fmt(gem), f"{fmt(t['raw'])}/{fmt(t['polished'])}/{fmt(t['faceted'])}"])
        print(table(rows, ['dia', 'nível', 'minas', 'vagonetes', 'produção', 'quartzo guardado', 'quartzo/dia (média 7d)', 'gemas brutas/dia', 'gemas bruta/polida/facetada'], md))
        if p.log:
            ev = sorted(p.log.items(), key=lambda kv: kv[1])
            print('\nMarcos (dia em que acontece): ' + '; '.join(f'{k}: dia {int(v // 24) + 1}' for k, v in ev))
        last7 = tl[-7:]
        results.append((s, sum(x['income_q'] for x in last7) / 7, sum(x['income_gems'] for x in last7) / 7, p))
    return results


def faceted_unit(cfg):
    """Gemas brutas e quartzo gastos para 1 facetada (3 polidas + os custos de quartzo das duas receitas)."""
    pol, fac = cfg.workshop['Polished'], cfg.workshop['Faceted']
    polished = fac['InputQuantity']
    raw = polished * pol['InputQuantity']
    quartz = polished * sum(c['Quantity'] for c in pol['Cost']) + sum(c['Quantity'] for c in fac['Cost'])
    return raw, quartz


def report_capacity(cfg, results, md):
    print('\n## 2b. Quanto de ataque o jogador consegue produzir por dia (regime estável)\n' if md else '\n=== 2b. Capacidade de ataque por dia (regime estavel) ===')
    raw_u, q_u = faceted_unit(cfg)
    atk = cfg.attack['Ruby']['Faceted']
    print(f'1 facetada = {raw_u} gemas brutas + {q_u} quartzos = {atk} pontos de ataque '
          f'({raw_u / atk:.2f} brutas e {q_u / atk:.1f} quartzos por ponto, com Rubi, o atacante). Usa a renda média dos últimos 7 dias, depois de todas as compras estruturais.')
    rows = []
    for s, q_day, g_day, p in results:
        by_q = q_day / (q_u / atk)
        by_g = g_day / (raw_u / atk)
        cap = min(by_q, by_g)
        limit = 'quartzo' if by_q < by_g else 'gemas brutas'
        rows.append([s.name, fmt(q_day), fmt(g_day), f'{cap:.0f}', limit,
                     f'{200 / cap:.1f}' if cap else '-', f'{400 / cap:.1f}' if cap else '-', f'{800 / cap:.1f}' if cap else '-'])
    print(table(rows, ['estratégia', 'quartzo/dia', 'gemas brutas/dia', 'pontos de ataque/dia', 'limitado por',
                       'dias p/ defesa 200 (neutro)', 'defesa 400', 'defesa 800'], md))
    print('\nLeitura: o que limita o ataque é o quartzo da lapidação (700 por facetada), não as gemas brutas, que sobram. '
          'Por isso o jogador que converte todas as minas em gema fica pobre em quartzo; a estratégia mista rende mais ataque.')


def ammo_cost(cfg, defense, mult, raw_value, ore='Ruby'):
    """Custo, em quartzo-equivalente, de quebrar `defense` pontos usando só facetadas ou só polidas, com o multiplicador elemental."""
    out = {}
    for stage, label in (('Faceted', 'facetadas'), ('Polished', 'polidas')):
        atk = cfg.attack[ore][stage]
        units = math.ceil(defense / (atk * mult))
        w = cfg.workshop
        if stage == 'Polished':
            raw, q = units * w['Polished']['InputQuantity'], units * sum(c['Quantity'] for c in w['Polished']['Cost'])
        else:
            pol = units * w['Faceted']['InputQuantity']
            raw = pol * w['Polished']['InputQuantity']
            q = pol * sum(c['Quantity'] for c in w['Polished']['Cost']) + units * sum(c['Quantity'] for c in w['Faceted']['Cost'])
        out[label] = (units, raw, q, q + raw * raw_value)
    return out


def report_deposits(cfg, md, raw_value, explore_max, explore_hours_per_point, amethyst_value):
    print('\n## 3. Depósitos (análise paramétrica sobre o balanceamento real)\n' if md else '\n=== 3. Depósitos (parametrico) ===')
    print(f'Valor de referência de 1 gema bruta: {raw_value} quartzos (hipótese; o mercado não existe). Munição de ataque: Rubi (polida {cfg.attack["Ruby"]["Polished"]}, '
          f'facetada {cfg.attack["Ruby"]["Faceted"]}); vantagem x{cfg.advantage}, desvantagem x{cfg.disadvantage} (da matriz de efetividade).')
    print('\n### 3.1 Custo para quebrar a defesa (quartzo-equivalente), com facetadas\n' if md else '\n-- 3.1 Custo para quebrar a defesa (so facetadas) --')
    rows = []
    for d in (100, 200, 400, 800, 1600):
        row = [d]
        for mult in (cfg.advantage, 1.0, cfg.disadvantage):
            u, raw, q, tot = ammo_cost(cfg, d, mult, raw_value)['facetadas']
            row.append(f'{u} fac. = {fmt(q)} q + {fmt(raw)} brutas ({fmt(tot)} eq.)')
        rows.append(row)
    print(table(rows, ['defesa (pontos)', 'com vantagem x1,5', 'neutro x1', 'com desvantagem x0,75'], md))

    print('\n### 3.2 Estoque e energia de exploração\n' if md else '\n-- 3.2 Estoque e energia de exploracao --')
    per_day = 24 / explore_hours_per_point
    print(f'Energia de exploração: máximo {explore_max}, 1 ponto a cada {explore_hours_per_point * 60:.0f} min = {per_day:.0f} pontos por dia '
          f'(enche em {explore_max * explore_hours_per_point:.0f} h). Supondo 1 ametista por ponto.')
    rows = []
    for days_of_stock in (2, 4, 8, 16):
        stock = days_of_stock * per_day
        row = [f'{days_of_stock} dias de energia = {fmt(stock)} ametistas']
        for d in (200, 400, 800):
            _, _, _, tot = ammo_cost(cfg, d, cfg.advantage, raw_value)['facetadas']
            row.append(f'{tot / stock:,.1f}'.replace(',', '.'))
        rows.append(row)
    print(table(rows, ['estoque', 'valor mínimo da ametista (quartzo-eq. por unidade) para defesa 200', 'defesa 400', 'defesa 800'], md))
    print('\nLeitura: o "valor mínimo" é quanto cada ametista precisa valer para o depósito empatar com o custo da munição (com vantagem elemental). '
          'Se a ametista valer menos que isso, ninguém compensa abrir o depósito.')
    report_counterattack(cfg, md, raw_value, amethyst_value)
    report_tier_counterattack(cfg, md, raw_value, amethyst_value, explore_max)
    report_loop_closure(cfg, md, raw_value, amethyst_value, 24 / explore_hours_per_point)


def report_loop_closure(cfg, md, raw_value, amethyst_value, explore_per_day):
    if not cfg.tiers:
        return
    print('\n### 3.5 Fechamento do ciclo: o prêmio da extração paga a munição? (implementado)\n' if md else '\n-- 3.5 Fechamento do ciclo --')
    print(f'Cada ponto de energia de exploração ({cfg.energy_per_extraction} por unidade) rende 1 ametista, que vale ~{amethyst_value:g} quartzos-equivalentes. '
          f'Estoque = defesa x {cfg.stock_per_defense:g}. Custo = atacar com facetadas de Rubi (elemento neutro, x1; com vantagem x{cfg.advantage:g} sai mais barato) '
          f'mais a defesa que cobre o contra-ataque (ametista facetada, a melhor defensora). Dias = estoque / ({explore_per_day:.0f} pontos por dia).')
    pol, fac = cfg.workshop['Polished'], cfg.workshop['Faceted']
    pol_q = sum(c['Quantity'] for c in pol['Cost'])
    fac_cost = fac['InputQuantity'] * pol['InputQuantity'] * raw_value + fac['InputQuantity'] * pol_q + sum(c['Quantity'] for c in fac['Cost'])
    rows = []
    ratios = []
    needed = []
    for t in cfg.tiers:
        defense = t['Defense']
        stock = defense * cfg.stock_per_defense
        reward = stock / cfg.energy_per_extraction * amethyst_value
        days = stock * cfg.energy_per_extraction / explore_per_day
        counter = defense * t['CounterRatio']
        def_cost = math.ceil(counter / cfg.defense['Amethyst']['Faceted']) * fac_cost
        for mult, label in ((1.0, 'neutro'), (cfg.advantage, 'vantagem')):
            _, _, _, attack = ammo_cost(cfg, defense, mult, raw_value)['facetadas']
            cost = attack + def_cost
            ratios.append(reward / cost)
            break_even = cost / (defense * amethyst_value / cfg.energy_per_extraction)
            needed.append((label, break_even))
            rows.append([f"{t['Slot']} {t['Name']}", label, fmt(stock), f'{days:.1f}', fmt(reward), fmt(cost), fmt(reward - cost),
                         f'{reward / cost:.2f}x', f'{break_even:.2f}'])
    print(table(rows, ['faixa', 'elemento', 'estoque (ametistas)', 'dias de energia', 'valor do prêmio (q-eq.)', 'custo (atacar + defender)', 'saldo',
                       'prêmio / custo', 'estoque por ponto de defesa para empatar'], md))
    even_adv = max(b for label, b in needed if label == 'vantagem')
    even_neu = max(b for label, b in needed if label == 'neutro')
    if max(ratios) < 1:
        verdict = (f'**o prêmio NÃO paga o custo em nenhuma combinação** (de {min(ratios):.2f}x a {max(ratios):.2f}x). Para empatar seria preciso estoque de ~{even_adv:.1f} por ponto de defesa '
                   f'(com vantagem elemental) ou ~{even_neu:.1f} (neutro), contra {cfg.stock_per_defense:g} hoje; ou a ametista valer proporcionalmente mais. É decisão de balanceamento (só configuração).')
    elif min(ratios) < 1:
        verdict = f'o prêmio paga o custo só em parte das combinações (de {min(ratios):.2f}x a {max(ratios):.2f}x); sem vantagem elemental o depósito dá prejuízo.'
    else:
        verdict = f'o prêmio paga o custo em todas as combinações (de {min(ratios):.2f}x a {max(ratios):.2f}x).'
    print(f'\nLeitura: {verdict} A energia de exploração limita o ritmo da extração e o laço "ametista vira munição" só fecha se o prêmio superar a munição.')


def report_tier_counterattack(cfg, md, raw_value, amethyst_value, explore_max):
    if not cfg.tiers:
        return
    print('\n### 3.4 Contra-ataque por faixa (implementado): quebrar a defesa inteira de uma vez\n' if md else '\n-- 3.4 Contra-ataque por faixa --')
    print(f'O depósito contra-ataca com (dano efetivo x razão da faixa) pontos; o que as gemas defensoras não absorvem tira {cfg.epc:g} de energia de exploração '
          f'por ponto (barra de {explore_max}). A gema defensora absorve defesa / multiplicador (depósito contra gema; aqui, elemento neutro, x1). '
          f'Um ponto de energia vale ~{amethyst_value:g} quartzos-equivalentes (hipótese).')
    pol, fac = cfg.workshop['Polished'], cfg.workshop['Faceted']
    pol_q = sum(c['Quantity'] for c in pol['Cost'])
    fac_cost = fac['InputQuantity'] * pol['InputQuantity'] * raw_value + fac['InputQuantity'] * pol_q + sum(c['Quantity'] for c in fac['Cost'])
    rows = []
    for t in cfg.tiers:
        defense = t['Defense']
        counter = defense * t['CounterRatio']
        energy = math.ceil(counter * cfg.epc)
        _, _, _, attack_cost = ammo_cost(cfg, defense, 1.0, raw_value)['facetadas']
        for ore in ('Sapphire', 'Amethyst'):
            per_unit = cfg.defense[ore]['Faceted']
            units = math.ceil(counter / per_unit)
            def_cost = units * fac_cost
            rows.append([f"{t['Slot']} {t['Name']}", defense, f"{t['CounterRatio'] * 100:g}%", f'{counter:.0f}',
                         f'{energy} ({energy / explore_max:.1f} barras)', f'{ore} facetada x{units}', fmt(def_cost),
                         fmt(energy * amethyst_value), f'{def_cost / attack_cost * 100:.0f}%'])
    print(table(rows, ['faixa', 'defesa', 'razão', 'contra-ataque total', 'energia se ninguém defender', 'defesa necessária',
                       'custo da defesa (q-eq.)', 'a energia equivale a (q-eq.)', 'custo da defesa / custo de atacar'], md))
    print('\nLeitura: a faixa leve cabe na barra de exploração (nem precisa de defesa); a média e a difícil custam mais energia do que a barra comporta de uma vez, '
          'então o jogador defende com gemas (barato perto do custo de atacar) ou divide o ataque em vários dias.')


def report_counterattack(cfg, md, raw_value, amethyst_value):
    print('\n### 3.3 Contra-ataque: defender com gemas ou tomar o dano na energia?\n' if md else '\n-- 3.3 Contra-ataque: defender ou tomar na energia --')
    pol, fac = cfg.workshop['Polished'], cfg.workshop['Faceted']
    pol_q = sum(c['Quantity'] for c in pol['Cost'])
    pol_cost = pol['InputQuantity'] * raw_value + pol_q
    fac_q = fac['InputQuantity'] * pol_q + sum(c['Quantity'] for c in fac['Cost'])
    fac_cost = fac['InputQuantity'] * pol['InputQuantity'] * raw_value + fac_q
    print(f'Custo de 1 gema lapidada (quartzo-equivalente, bruta a {raw_value}): polida {pol_cost:.0f}, facetada {fac_cost:.0f}. '
          f'Um ponto de energia de exploração vale ~{amethyst_value} (1 ametista de extração perdida; hipótese).')
    rows = []
    ks = []
    for ore in ('Sapphire', 'Emerald', 'Ruby', 'Amethyst'):
        for stage, cost in (('Polished', pol_cost), ('Faceted', fac_cost)):
            d = cfg.defense[ore][stage]
            per_point = cost / d
            k = per_point / amethyst_value
            ks.append(k)
            rows.append([ore, 'polida' if stage == 'Polished' else 'facetada', d, f'{cost:.0f}', f'{per_point:.1f}', f'{k:.1f}'])
    print(table(rows, ['gema', 'estágio', 'defesa', 'custo da gema', 'custo por ponto de defesa', 'energia por ponto de contra-ataque para empatar'], md))
    print(f'\nLeitura: se cada ponto de contra-ataque não absorvido custar menos energia que a coluna da direita, é mais barato tomar o dano que se defender '
          f'(ninguém usa defesa). Com defesa ~{min(ks):.1f} a {max(ks):.1f} de energia por ponto, uma conversão de ~{(min(ks) + max(ks)) / 2:.0f} por ponto '
          f'deixa as duas opções equivalentes na média.')


PROFILES = [('assíduo (4x por dia)', 6), ('diário (1x por dia)', 24), ('a cada 2 dias', 48)]


def report_visit_profiles(cfg, md, explore_options, explore_minutes_per_point, raw_value, amethyst_value):
    """Seção 3.6: o que muda por perfil de visita (a maioria dos jogadores é diária ou de 2 em 2 dias)."""
    print('\n### 3.6 Perfis de visita: vagonete, energia normal e energia de exploração\n' if md else '\n-- 3.6 Perfis de visita --')
    print('O jogador gasta **toda** a energia (normal e de exploração) em cada visita. O assíduo visita a cada 6 h; o diário, a cada 24 h; o casual, a cada 48 h. '
          'A barra enche até o teto e o que passa dele se perde.')

    rate = cfg.mines[1]['OresPerMinute'] * 60  # minérios por hora de uma mina
    print('\n**Retorno de cada upgrade do vagonete, por perfil** (mina de quartzo; ganho marginal em minérios por dia sobre o nível anterior; retorno = custo ÷ ganho, em dias):\n' if md else '\nRetorno do vagonete:')
    rows = []
    caps = [100] + [u['Capacity'] for u in cfg.cart_upgrades]
    for level in range(1, cfg.cart_max_level + 1):
        cost = sum(c['Quantity'] for c in cfg.cart_upgrades[level - 1]['Cost'])
        row = [level, caps[level], fmt(cost)]
        for _, gap in PROFILES:
            def per_day(cap, g=gap):
                return min(cap, rate * g) * 24 / g
            gain = per_day(caps[level]) - per_day(caps[level - 1])
            row.append(f'+{gain:.0f}/dia' + (f' ({cost / gain:.0f} d)' if gain > 0 else ' (nenhum)'))
        rows.append(row)
    print(table(rows, ['nível', 'capacidade', 'custo (quartzo)'] + [n for n, _ in PROFILES], md))
    print('\nLeitura: para o diário o nível 4 é o último que rende (o vagonete já enche em ~24 h); o nível 5 só ajuda quem passa mais de 1 dia fora (o casual). '
          'Para o casual todos os níveis rendem, mas o retorno é lento.')

    print('\n**Energia normal (mineração ativa) por perfil** (teto e ritmo da configuração):\n' if md else '\nEnergia normal:')
    rows = []
    for name, gap in PROFILES:
        per_visit = min(cfg.max_energy, gap * cfg.energy_per_hour)
        rows.append([name, f'{per_visit:.0f}', f'{per_visit * 24 / gap:.0f}'])
    print(table(rows, ['perfil', 'minerações ativas por visita', 'por dia'], md))
    print('\nLeitura: a mineração ativa rende 1 minério por ponto. Para o diário são ~100 por dia, contra ~475 do vagonete de UMA mina: o vagonete pesa muito mais que a energia normal para quem visita pouco.')

    per_hour = 60 / explore_minutes_per_point
    print('\n**Energia de exploração: dias para extrair um depósito inteiro** (estoque = defesa x ' + f'{cfg.stock_per_defense:g}' + f', {cfg.energy_per_extraction} ponto por unidade, 1 ponto a cada {explore_minutes_per_point:g} min):\n' if md else '\nExtracao por perfil:')
    rows = []
    for mx in explore_options:
        for name, gap in PROFILES:
            per_visit = min(mx, gap * per_hour)
            per_day = per_visit * 24 / gap / cfg.energy_per_extraction
            row = [mx, name, f'{per_visit:.0f}', f'{per_day:.0f}']
            for t in cfg.tiers:
                row.append(f'{t["Defense"] * cfg.stock_per_defense / per_day:.1f}')
            rows.append(row)
    print(table(rows, ['teto da barra', 'perfil', 'unidades por visita', 'por dia'] + [f'dias p/ faixa {t["Name"]} ({fmt(t["Defense"] * cfg.stock_per_defense)})' for t in cfg.tiers], md))

    print('\n**Contra-ataque sem defesa, em barras de exploração** (quebrar a defesa inteira de uma vez, sem gemas defensoras):\n' if md else '\nContra-ataque em barras:')
    rows = []
    for t in cfg.tiers:
        energy = math.ceil(t['Defense'] * t['CounterRatio'] * cfg.epc)
        rows.append([f"{t['Slot']} {t['Name']}", energy] + [f'{energy / mx:.2f}' for mx in explore_options])
    print(table(rows, ['faixa', 'energia perdida'] + [f'barras com teto {mx}' for mx in explore_options], md))
    print('\nLeitura: o teto da barra decide quanto o jogador extrai por visita. Com 30, o diário extrai 30 por dia; com 72 (um dia cheio de regeneração) aproveita tudo o que acumulou. '
          'O contra-ataque da faixa leve passa a caber folgado na barra maior.')


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--days', type=int, default=90)
    ap.add_argument('--markdown', action='store_true')
    ap.add_argument('--raw-gem-value', type=float, default=2.5, help='valor hipotético de 1 gema bruta em quartzos (padrão 2,5)')
    ap.add_argument('--amethyst-value', type=float, default=12, help='valor hipotético de 1 ametista (= 1 ponto de energia de exploração), em quartzos')
    ap.add_argument('--explore-max', type=int, default=30)
    ap.add_argument('--explore-minutes-per-point', type=float, default=20)
    ap.add_argument('--explore-max-options', default='30,72,144', help='tetos da barra de exploração a comparar na seção 3.6')
    ap.add_argument('--config', default=CONFIG)
    a = ap.parse_args()
    sys.stdout.reconfigure(encoding='utf-8')

    global cfg
    cfg = Cfg(load_config(a.config))
    if a.markdown:
        print(f'# Simulação da economia do Shards\n\nGerado por `Shards/tools/simulate_economy.py` a partir do `appsettings.json` ({a.days} dias).')
    report_static(cfg, a.markdown)
    results = report_timeline(cfg, a.days, a.markdown)
    report_capacity(cfg, results, a.markdown)
    report_deposits(cfg, a.markdown, a.raw_gem_value, a.explore_max, a.explore_minutes_per_point / 60, a.amethyst_value)
    report_visit_profiles(cfg, a.markdown, [int(x) for x in a.explore_max_options.split(',')], a.explore_minutes_per_point, a.raw_gem_value, a.amethyst_value)


if __name__ == '__main__':
    sys.exit(main())
