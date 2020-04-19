using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Sales.Helpers;
using Sales.Resources;
using Sales.Services;
using Sales.Views;
using Xamarin.Forms;

namespace Sales.ViewModels
{
    public class LoginViewModel:BaseViewModel
    {
        #region Attibutes
        private bool isRunning;
        private bool isEnabled;
        private ApiService apiService;
        #endregion

        #region Constructor
        public LoginViewModel()
        {
            this.apiService = new ApiService();
            this.IsEnabled = true;
            this.IsRemembered = true;
        }
        #endregion

        #region Properties
        public string Email { get; set; }
        public string Password { get; set; }
        public  bool IsRemembered { get; set; }
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

        #region Commands
        public ICommand LoginCommand 
        {
            get
            {
                return new RelayCommand(Login);
            }
        }
        #endregion

        #region Methods
        private async void Login()
        {
            if (string.IsNullOrEmpty(this.Email))
            {
                await Application.Current.MainPage.DisplayAlert(Resource.Error,Resource.EMailError,"Ok");
                return;
            }
            if (string.IsNullOrEmpty(this.Password))
            {
                await Application.Current.MainPage.DisplayAlert(Resource.Error, Resource.PasswordError, "Ok");
                return;
            }
            this.IsRunning = true;
            this.IsEnabled = false;

            var connection = await apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnabled = true;

                await Application.Current.MainPage.DisplayAlert(Resource.Error, connection.Message, "Ok");
                return;
            }

            var urlBase = Application.Current.Resources["UrlApi"].ToString();
            var token = await apiService.GetToken(urlBase,this.Email,this.Password);
            if (token == null || string.IsNullOrEmpty(token.AccessToken))
            {
                this.IsRunning = false;
                this.IsEnabled = true;
                await Application.Current.MainPage.DisplayAlert(Resource.Error, Resource.SomethingWrong, "Ok");
                return;
            }
            Settings.TokenType = token.TokenType;
            Settings.AccessToken = token.AccessToken;
            Settings.IsRemembered = this.IsRemembered;
            
            MainViewModel.GetInstance().Products = new ProductsViewModel();
            Application.Current.MainPage = new ProductsPage();

            this.IsRunning = false;
            this.IsEnabled = true;
        }
        #endregion
    }
}
