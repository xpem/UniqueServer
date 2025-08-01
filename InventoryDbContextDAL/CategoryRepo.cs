﻿using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class CategoryRepo(InventoryDbContext dbContext) : ICategoryRepo
    {
        public int Create(Category category)
        {
            dbContext.Category.Add(category);
            return dbContext.SaveChanges();
        }

        public int Delete(Category category)
        {
            dbContext.ChangeTracker?.Clear();

            dbContext.Category.Remove(category);

            return dbContext.SaveChanges();
        }

        public List<Category>? Get(int uid) => dbContext.Category.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).ToList();

        public Category? GetById(int uid, int id) => dbContext.Category.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).FirstOrDefault();

        public Category? GetByName(int uid, string name) => dbContext.Category.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Name == name).FirstOrDefault();

        public async Task<List<Category>?> GetByIdWithSubCategories(int uid, int? id = null)
        {
            if (id is null)
                return await dbContext.Category
                    .Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault))
                    .Include(x => x.SubCategories!.Where(sc => !sc.Inactive && (sc.UserId == uid || (sc.UserId == null && sc.SystemDefault))))
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            else
                return await dbContext.Category.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id)
                    .Include(x => x.SubCategories!.Where(sc => !sc.Inactive && (sc.UserId == uid || (sc.UserId == null && sc.SystemDefault))))
                    .OrderBy(x => x.Id)
                    .ToListAsync();
        }

        public int Update(Category category)
        {
            dbContext.ChangeTracker?.Clear();

            dbContext.Category.Update(category);

            return dbContext.SaveChanges();
        }
    }
}
