using Sales.Domain.Models;

namespace Sales.Backend.Models
{
    public class LocalDataContext : DataContext
    {
        public new System.Data.Entity.DbSet<Sales.Common.Models.Product> Products { get; set; }

        public System.Data.Entity.DbSet<Sales.Common.Models.Category> Categories { get; set; }
    }
}