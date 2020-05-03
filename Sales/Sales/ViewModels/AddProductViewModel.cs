using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Sales.Common.Models;
using Sales.Helpers;
using Sales.Resources;
using Sales.Services;
using Xamarin.Forms;

namespace Sales.ViewModels
{
    public class AddProductViewModel : BaseViewModel
    {
        #region Attributes
        private MediaFile file;
        private ImageSource imageSource;
        private bool isRunning;
        private bool isEnabled;
        private ApiService apiService;
        private ObservableCollection<Category> categories;
        private Category category;
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

        public ImageSource ImageSource
        {
            get { return imageSource; }
            set { SetProperty(ref this.imageSource, value); }
        }

        public List<Category> MyCategories { get; set; }
        public ObservableCollection<Category> Categories
        {
            get
            {
                return this.categories;
            }
            set
            {
                this.SetProperty(ref this.categories, value);
            }
        }
        public Category Category
        {
            get
            {
                return this.category;
            }
            set
            {
                this.SetProperty(ref this.category, value);
            }
        }

        #endregion

        #region Constructors
        public AddProductViewModel()
        {
            apiService = new ApiService();
            this.IsEnabled = true;
            this.ImageSource = "NoProduct";
            this.LoadCategories();
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

        public ICommand ChangeImageCommand
        {
            get
            {
                return new RelayCommand(ChangeImage);
            }
        }
        #endregion

        #region Methods
        private async void LoadCategories()
        {
            this.IsRunning = true;
            this.IsEnabled = false;

            var connection = await this.apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnabled = true;
                await Application.Current.MainPage.DisplayAlert(Languages.Error, connection.Message, "Ok");
                return;
            }

            var answer = await this.LoadCategoriesFromAPI();
            if (answer)
            {
                this.RefreshList();
            }

            this.IsRunning = false;
            this.IsEnabled = true;

        }
        private void RefreshList()
        {
            this.Categories = new ObservableCollection<Category>(this.MyCategories.OrderBy(c => c.Description));
        }

        private async Task<bool> LoadCategoriesFromAPI()
        {
            var url = Application.Current.Resources["UrlApi"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlCategoriesController"].ToString();
            var response = await this.apiService.GetList<Category>(url, prefix, controller, Settings.TokenType, Settings.AccessToken);
            if (!response.IsSuccess)
            {
                return false;
            }

            this.MyCategories = (List<Category>)response.Result;
            return true;
        }
        private async void ChangeImage()
        {
            try
            {

                await CrossMedia.Current.Initialize();

                var source = await Application.Current.MainPage.DisplayActionSheet(
                    Resource.ImageSource,
                    Resource.Cancel,
                    null,
                    Resource.FromGallery,
                    Resource.NewPicture
                    );

                if (source == Resource.Cancel)
                {
                    this.file = null;
                    return;
                }
                if (source == Resource.NewPicture)
                {
                    this.file = await CrossMedia.Current.TakePhotoAsync(
                        new StoreCameraMediaOptions
                        {
                            Directory = "Sample",
                            Name = "test.jpg",
                            PhotoSize = PhotoSize.Small
                        });
                }
                else
                {
                    this.file = await CrossMedia.Current.PickPhotoAsync();
                }
                if (this.file != null)
                {
                    this.ImageSource = ImageSource.FromStream(() =>
                    {
                        var stream = this.file.GetStream();
                        return stream;
                    });
                }
            }
            catch
            { 
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
            if (this.Category == null)
            {
                await Application.Current.MainPage.DisplayAlert(Resource.Error, Resource.CategoryError, "Ok");
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
            var urlPrefix = Application.Current.Resources["UrlPrefix"].ToString();
            var urlController = Application.Current.Resources["UrlProductsController"].ToString();

            byte[] imageArray = null;
            if (this.file != null)
            {
                imageArray = FilesHelper.ReadFully(this.file.GetStream());
            }

            var location = await this.GetLocation();

            var product = new Product
            {
                Description = this.Description,
                Price = price,
                Remarks = this.Remarks,
                ImageArray = imageArray,
                CategoryId = Category.CategoryId,
                UserId = MainViewModel.GetInstance().UserASP.Id,
                Longitude = location == null ? 0 : location.Longitude,
                Latitude = location == null ? 0 : location.Latitude,
            };

            var response = await this.apiService.Post(urlBase, urlPrefix, urlController, product,Settings.TokenType,Settings.AccessToken);

            if (!response.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnabled = true;

                await Application.Current.MainPage.DisplayAlert(Resource.Error, response.Message, "Ok");
                return;
            }

            var newProduct = (Product)response.Result;
            var productsViewModel = ProductsViewModel.GetInstance();
            productsViewModel.MyProducts.Add(newProduct);
            productsViewModel.RefreshList();

            this.IsRunning = false;
            this.IsEnabled = true;
            await App.Navigator.PopAsync();
        }
        private async Task<Position> GetLocation()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;
            var location = await locator.GetPositionAsync();
            return location;
        }
        #endregion
    }
}
