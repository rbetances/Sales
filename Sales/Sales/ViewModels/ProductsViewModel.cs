using GalaSoft.MvvmLight.Command;
using Sales.Common.Models;
using Sales.Helpers;
using Sales.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sales.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private ApiService apiService;

        private ObservableCollection<Product> products;
        private bool isRefreshing;
        public ObservableCollection<Product> Products
        {
            get { return this.products; }
            set { this.SetProperty(ref this.products, value); }
        }
        public bool IsRefreshing
        {
            get { return this.isRefreshing; }
            set { this.SetProperty(ref this.isRefreshing, value); }
        }

        public ProductsViewModel()
        {
            this.apiService = new ApiService();
            this.LoadProducts();
        }

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
                Device.BeginInvokeOnMainThread(
                   async () =>
                   {
                       await Application.Current.MainPage.DisplayAlert(Languages.Error, response.Message, "Ok");
                   });
                return;
            }

            this.IsRefreshing = false;
            var list = (List<Product>)response.Result;
            this.Products = new ObservableCollection<Product>(list);

        }

        public ICommand RefreshCommand
        {
            get
            {
                return new RelayCommand(LoadProducts);
            }
        }
    }
}
