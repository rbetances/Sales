using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Sales.Common.Models;
using Sales.Resources;
using Sales.Views;
using Xamarin.Forms;

namespace Sales.ViewModels
{
    public class MainViewModel
    {
        #region Properties
        public ProductsViewModel Products { get; set; }
        public AddProductViewModel AddProduct { get; set; }
        public EditProductViewModel EditProduct { get; set; }
        public LoginViewModel Login { get; set; }
        public RegisterViewModel Register { get; set; }
        public MyUserASP UserASP { get; set; }
        public ObservableCollection<MenuItemViewModel> Menu { get; set; }
        public string UserFullName
        {
            get
            {
                if (this.UserASP != null && this.UserASP.Claims != null && this.UserASP.Claims.Count > 1)
                {
                    return $"{this.UserASP.Claims[0].ClaimValue} {this.UserASP.Claims[1].ClaimValue}";
                }

                return null;
            }
        }
        public string UserImageFullPath
        {
            get
            {
                foreach (var claim in this.UserASP.Claims)
                {
                    if (claim.ClaimType == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri")
                    {
                        if (claim.ClaimValue.StartsWith("~"))
                        {
                            return $"http://10.0.0.22:8080{claim.ClaimValue.Substring(1)}";
                        }

                        return claim.ClaimValue;
                    }
                }

                return null;
            }
        }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            instance = this;
            this.LoadMenu();
        }
        #endregion

        #region Commands
        public ICommand AddProductCommand
        {
            get
            {
                return new RelayCommand(GoToAddProduct);
            }
        }
        #endregion

        #region Methods
        private void LoadMenu()
        {
            this.Menu = new ObservableCollection<MenuItemViewModel>();

            this.Menu.Add(new MenuItemViewModel
            {
                Icon = "info",
                PageName = "AboutPage",
                Title = Resource.About,
            });

            this.Menu.Add(new MenuItemViewModel
            {
                Icon = "settings",
                PageName = "SetupPage",
                Title = Resource.Setup,
            });

            this.Menu.Add(new MenuItemViewModel
            {
                Icon = "exit",
                PageName = "LoginPage",
                Title = Resource.Exit,
            });
        }
        private async void GoToAddProduct()
        {
            this.AddProduct = new AddProductViewModel();
            await App.Navigator.PushAsync(new AddProductPage());
        }
        #endregion

        #region Singleton
        private static MainViewModel instance;
        public static MainViewModel GetInstance()
        {
            if (instance == null)
            {
                return new MainViewModel();
            }

            return instance;

        }
        #endregion
    }
}
