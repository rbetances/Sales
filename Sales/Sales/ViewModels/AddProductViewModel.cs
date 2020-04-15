using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Sales.Common.Models;
using Sales.Resources;
using Sales.Services;
using Xamarin.Forms;

namespace Sales.ViewModels
{
    public class AddProductViewModel : BaseViewModel
    {
        #region Attributes
        private bool isRunning;
        private bool isEnabled;
        private ApiService apiService;
        #endregion

        #region Properties

        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Remarks { get; set; }
        public bool IsRunning
        {
            get { return isRunning; }
            set { SetProperty(ref this.isRunning, value); }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref this.isEnabled, value); }
        }

        #endregion

        #region Constructors
        public AddProductViewModel()
        {
            apiService = new ApiService();
            this.IsEnabled = true;
        }
        #endregion

        #region Commands
        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(Save);
            }
        }

        private async void Save()
        {
            if (string.IsNullOrEmpty(this.Description))
            {
                await Application.Current.MainPage.DisplayAlert(Resource.Error, Resource.DescriptionError, "Ok");
                return;
            }
            decimal price;
            decimal.TryParse(this.Price.ToString(), out price);
            if (this.Price <= 0)
            {
                await Application.Current.MainPage.DisplayAlert(Resource.Error, Resource.PriceError, "Ok");
                return;
            }

            this.isRunning = true;
            this.isEnabled = false;

            var connection = await apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                this.isRunning = false;
                this.isEnabled = true;

                await Application.Current.MainPage.DisplayAlert(Resource.Error, connection.Message, "Ok");
                return;
            }

            var urlBase = Application.Current.Resources["UrlApi"].ToString();
            var urlPrefix = Application.Current.Resources["UrlPrefix"].ToString();
            var urlController = Application.Current.Resources["UrlProductsController"].ToString();

            var product = new Product
            {
                Description = this.Description,
                Price = price,
                Remarks = this.Remarks
            };

            var response = await this.apiService.Post(urlBase, urlPrefix, urlController, product);

            if (!response.IsSuccess)
            {
                this.isRunning = false;
                this.isEnabled = true;

                await Application.Current.MainPage.DisplayAlert(Resource.Error, connection.Message, "Ok");
                return;
            }
            this.isRunning = false;
            this.isEnabled = true;
            await Application.Current.MainPage.Navigation.PopAsync();

        }
        #endregion
    }
}
