namespace UserManagementService.Model
{
    /// <summary>
    /// Formato em que User.Password está armazenado.
    /// Legacy = AES reversível (EncryptionService), mantido só para validar contas
    /// antigas e migrá-las no próximo login bem-sucedido.
    /// Pbkdf2 = hash unidirecional (PasswordHashService) — formato atual.
    /// </summary>
    public enum PasswordAlgo
    {
        Legacy = 0,
        Pbkdf2 = 1,
    }
}
