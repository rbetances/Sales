using GalaSoft.MvvmLight.Command;
using Sales.Common.Models;
using Sales.Helpers;
using Sales.Services;
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
        private ObservableCollection<ProductItemViewModel> products;
        private bool isRefreshing;
        #endregion

        #region Properties
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
        #endregion

        #region Constructors
        public ProductsViewModel()
        {
            instance = this;
            this.apiService = new ApiService();
            this.LoadProducts();
        }
        #endregion

        #region Singleton
        private static ProductsViewModel instance;

        public static ProductsViewModel GetInstance()
        {
            if (instance == null)
            {
                return new ProductsViewModel();
            }

            return instance;

        }
        #endregion

        #region Methods
        private async void LoadProducts()
        {
            this.IsRefreshing = true;

            var connection = await apiService.CheckConnection();
            await Task.Delay(1000);
            if (!connection.IsSuccess)
            {
                this.IsRefreshing = false;
                // Device.BeginInvokeOnMainThread(
                //   async () =>
                // {
                await Application.Current.MainPage.DisplayAlert(Languages.Error, connection.Message, "Ok");
                //});
                return;
            }
            var urlBase = Application.Current.Resources["UrlApi"].ToString();
            var urlPrefix = Application.Current.Resources["UrlPrefix"].ToString();
            var urlController = Application.Current.Resources["UrlProductsController"].ToString();
            var response = await this.apiService.GetList<Product>(urlBase, urlPrefix, urlController);

            if (!response.IsSuccess)
            {
                await Application.Current.MainPage.DisplayAlert(Languages.Error, response.Message, "Ok");

                return;
            }

            this.IsRefreshing = false;
            var list = (List<Product>)response.Result;

            var myList = list.Select(x => new ProductItemViewModel
            {
                Description = x.Description,
                ImageArray = x.ImageArray,
                ImagePath = x.ImagePath,
                IsAvailable = x.IsAvailable,
                Price = x.Price,
                ProductId = x.ProductId,
                PublishOn = x.PublishOn,
                Remarks = x.Remarks
,
            });

            this.Products = new ObservableCollection<ProductItemViewModel>(myList);

        }
        #endregion

        #region Commands
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
