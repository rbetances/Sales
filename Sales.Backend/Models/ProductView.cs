using System.Web;
using Sales.Common.Models;

namespace Sales.Backend.Models
{
    public class ProductView: Product
    {
        public HttpPostedFileBase ImageFile { get; set; }
    }
}