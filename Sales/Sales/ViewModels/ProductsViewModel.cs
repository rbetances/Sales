using GalaSoft.MvvmLight.Command;
using Sales.Common.Models;
using Sales.Helpers;
using Sales.Resources;
using Sales.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sales.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {

        #region Attributes
        private ApiService apiService;
        private DataService dataService;
        private ObservableCollection<ProductItemViewModel> products;
        private bool isRefreshing;
        private string filter;
        #endregion

        #region Properties
        public string Filter
        {
            get { return this.filter; }
            set
            {
                this.filter = value;
                this.RefreshList();
            }

        }
        public List<Product> MyProducts { get; set; }
        public ObservableCollection<ProductItemViewModel> Products
        {
            get { return this.products; }
            set { this.SetProperty(ref this.products, value); }
        }
        public bool IsRefreshing
        {
            get { return this.isRefreshing; }
            set { this.SetProperty(ref this.isRefreshing, value); }
        }
        private Category Category
        { 
            get;
            set; 
        }

        #endregion

        #region Constructors
        public ProductsViewModel(Category category)
        {
            instance = this;
            this.Category = category;
            this.apiService = new ApiService();
            this.dataService = new DataService();
            this.LoadProducts();
        }
        #endregion

        #region Singleton
        private static ProductsViewModel instance;

        public static ProductsViewModel GetInstance()
        {
            return instance;
        }
        #endregion

        #region Methods
        private async void LoadProducts()
        {
            this.IsRefreshing = true;

            var connection = await apiService.CheckConnection();
            await Task.Delay(1000);
            if (connection.IsSuccess)
            {
                var answer = await this.LoadProductsFromApi();
                if (answer)
                {
                    this.RefreshList();
                }
       
            }
            this.IsRefreshing = false;
        }

        private async Task LoadProductsFromDB()
        {
            this.MyProducts = await this.dataService.GetAllProducts();
        }

        private async Task SaveProductsToDB()
        {
           await this.dataService.DeleteAllProducts();
           this.dataService.Insert(MyProducts);
        }

        private async Task<bool> LoadProductsFromApi()
        {
            var urlBase = Application.Current.Resources["UrlApi"].ToString();
            var urlPrefix = Application.Current.Resources["UrlPrefix"].ToString();
            var urlController = Application.Current.Resources["UrlProductsController"].ToString();
            var response = await this.apiService.GetList<Product>(urlBase, urlPrefix, urlController,this.Category.CategoryId, Settings.TokenType, Settings.AccessToken);

            if (!response.IsSuccess)
            {
                return false;
            }

            this.MyProducts = (List<Product>)response.Result;
            return true;
        }

        public void RefreshList()
        {
            var myListProductItemViewModel = this.MyProducts.Select(x => new ProductItemViewModel
            {
                Description = x.Description,
                ImageArray = x.ImageArray,
                ImagePath = x.ImagePath,
                IsAvailable = x.IsAvailable,
                Price = x.Price,
                ProductId = x.ProductId,
                PublishOn = x.PublishOn,
                Remarks = x.Remarks
            });
            if (string.IsNullOrEmpty(this.Filter))
            {
                this.Products = new ObservableCollection<ProductItemViewModel>(myListProductItemViewModel.OrderBy(x => x.Description));
            }
            else
            {
                this.Products = new ObservableCollection<ProductItemViewModel>(myListProductItemViewModel.Where(y => y.Description.ToLower().Contains(this.Filter.ToLower())).OrderBy(x => x.Description));
            }
        }
        #endregion

        #region Commands
        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(this.RefreshList);
            }
        }
        public ICommand RefreshCommand
        {
            get
            {
                return new RelayCommand(LoadProducts);
            }
        }
        #endregion
    }
}
