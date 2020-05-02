namespace Sales.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Models;
    using Interfaces;
    using SQLite;
    using Xamarin.Forms;

    public class DataService
    {
        private SQLiteAsyncConnection connection;

        public DataService()
        {
            this.OpenOrCreateDB();
        }

        private async Task OpenOrCreateDB()
        {
            var databasePath = DependencyService.Get<IPathService>().GetDatabasePath();
            this.connection = new SQLiteAsyncConnection(databasePath);
            await connection.CreateTableAsync<Category>().ConfigureAwait(false);
            await connection.CreateTableAsync<Product>().ConfigureAwait(false);
   
        }

        public async Task Insert<T>(T model)
        {
            await this.connection.InsertAsync(model);
        }

        public async Task Insert<T>(List<T> models)
        {
            await this.connection.InsertAllAsync(models);
        }

        public async Task Update<T>(T model)
        {
            await this.connection.UpdateAsync(model);
        }

        public async Task Update<T>(List<T> models)
        {
            await this.connection.UpdateAllAsync(models);
        }

        public async Task Delete<T>(T model)
        {
            await this.connection.DeleteAsync(model);
        }

        public async Task<List<Product>> GetAllProductsByCategory(string categoryId)
        {
            var query = await this.connection.QueryAsync<Product>($"select * from [Product] where CategoryId = {categoryId}");
            var array = query.ToArray();
            var list = array.Select(p => new Product
            {
                Description = p.Description,
                ImagePath = p.ImagePath,
                IsAvailable = p.IsAvailable,
                Price = p.Price,
                ProductId = p.ProductId,
                PublishOn = p.PublishOn,
                Remarks = p.Remarks,
            }).ToList();
            return list;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            var query = await this.connection.QueryAsync<Product>($"select * from [Product] where");
            var array = query.ToArray();
            var list = array.Select(p => new Product
            {
                Description = p.Description,
                ImagePath = p.ImagePath,
                IsAvailable = p.IsAvailable,
                Price = p.Price,
                ProductId = p.ProductId,
                PublishOn = p.PublishOn,
                Remarks = p.Remarks,
            }).ToList();
            return list;
        }

        public async Task DeleteAllProducts()
        {
            var query = await this.connection.QueryAsync<Product>("delete from [Product]");
        }

        public async Task<List<Category>> GetAllCategories()
        {
            var query = await this.connection.QueryAsync<Category>("select * from [Category]");
            var array = query.ToArray();
            var list = array.Select(p => new Category
            {
                CategoryId = p.CategoryId,
                Description = p.Description,
                ImagePath = p.ImagePath,
            }).ToList();
            return list;
        }

        public async Task DeleteAllCategories()
        {
            var query = await this.connection.QueryAsync<Category>("delete from [Category]");
        }
    }
}