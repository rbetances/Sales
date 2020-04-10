using GalaSoft.MvvmLight.Command;
using Sales.Common.Models;
using Sales.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            if (!connection.IsSuccess)
            {
                this.IsRefreshing = false;
                Device.BeginInvokeOnMainThread(
                    async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Alert", connection.Message, "Ok");
                    });
                return;
            }
            var urlBase = Application.Current.Resources["UrlApi"].ToString();
            var response = await this.apiService.GetList<Product>(urlBase, "/api", "/Products");

            if (!response.IsSuccess)
            {
                Device.BeginInvokeOnMainThread(
                   async () =>
                   {
                       await Application.Current.MainPage.DisplayAlert("Error", response.Message, "Ok");
                   });
                return;
            }

            var list = (List<Product>)response.Result;
            this.Products = new ObservableCollection<Product>(list);
            this.IsRefreshing = false;
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
