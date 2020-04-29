using GalaSoft.MvvmLight.Command;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Sales.Common.Models;
using Sales.Helpers;
using Sales.Resources;
using Sales.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sales.ViewModels
{
    public class EditProductViewModel : BaseViewModel
    {
        #region Attributes
        private MediaFile file;
        private ImageSource imageSource;
        private bool isRunning;
        private bool isEnabled;
        private ApiService apiService;
        private Product product;
        private ObservableCollection<Category> categories;
        private Category category;
        #endregion

        #region Constructor
        public EditProductViewModel(Product product)
        {
            apiService = new ApiService();
            this.IsEnabled = true;
            this.product = product;
            this.ImageSource = product.ImageFullPath;
            this.LoadCategories();
        }
        #endregion

        #region Properties
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
        public Product Product
        {
            get { return this.product; }
            set { this.SetProperty(ref this.product, value); }
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

        public ICommand DeleteProductCommand
        {
            get
            {
                return new RelayCommand(DeleteProduct);
            }
        }
        #endregion

        #region Methods
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
           
            if (string.IsNullOrEmpty(this.Product.Description))
            {
                await Application.Current.MainPage.DisplayAlert(Resource.Error, Resource.DescriptionError, "Ok");
                return;
            }
            if (this.Product.Price <= 0)
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
                this.isRunning = false;
                this.isEnabled = true;

                await Application.Current.MainPage.DisplayAlert(Resource.Error, connection.Message, "Ok");
                return;
            }

            this.Product.CategoryId = this.Category.CategoryId;
            var urlBase = Application.Current.Resources["UrlApi"].ToString();
            var urlPrefix = Application.Current.Resources["UrlPrefix"].ToString();
            var urlController = Application.Current.Resources["UrlProductsController"].ToString();

            if (this.file != null)
            {
                this.product.ImageArray = FilesHelper.ReadFully(this.file.GetStream());
            }

            var response = await this.apiService.Put<Product>(urlBase, urlPrefix, urlController, this.Product, this.Product.ProductId,Settings.TokenType,Settings.AccessToken);

            if (!response.IsSuccess)
            {
                this.IsRunning = false;
                this.IsEnabled = true;

                await Application.Current.MainPage.DisplayAlert(Resource.Error, response.Message, "Ok");
                return;
            }

            var updatedProduct = (Product)response.Result;
            var productsViewModel = ProductsViewModel.GetInstance();
            var oldProduct = productsViewModel.MyProducts.Where(p => p.ProductId == this.Product.ProductId).FirstOrDefault();
            if (oldProduct != null)
                productsViewModel.MyProducts.Remove(oldProduct);

            productsViewModel.MyProducts.Add(updatedProduct);
            productsViewModel.RefreshList();

            this.IsRunning = false;
            this.IsEnabled = true;

            await App.Navigator.PopAsync();


        }
        private async void DeleteProduct()
        {
            var answer = await Application.Current.MainPage.DisplayAlert(
                Resource.Confirm,
                Resource.DeleteConfirmation,
                Resource.Yes,
                Resource.No);

            if (!answer)
            {
                return;
            }
            var connection = await apiService.CheckConnection();
            if (!connection.IsSuccess)
            {
                await Application.Current.MainPage.DisplayAlert(Resource.Error, connection.Message, "Ok");
                return;
            }

            var urlBase = Application.Current.Resources["UrlApi"].ToString();
            var urlPrefix = Application.Current.Resources["UrlPrefix"].ToString();
            var urlController = Application.Current.Resources["UrlProductsController"].ToString();

            this.isEnabled = false;
            this.isRunning = true;

            var response = await this.apiService.Delete(urlBase, urlPrefix, urlController, this.Product.ProductId,Settings.TokenType,Settings.AccessToken);
            if (!response.IsSuccess)
            {
                this.isEnabled = true;
                this.isRunning = false;
                await Application.Current.MainPage.DisplayAlert(Resource.Error, response.Message, "Ok");
                return;
            }

            var productsViewModel = ProductsViewModel.GetInstance();
            var deletedProduct = productsViewModel.MyProducts.Where(p => p.ProductId == this.Product.ProductId).FirstOrDefault();
            if (deletedProduct != null)
            {
                productsViewModel.MyProducts.Remove(deletedProduct);
            }
            productsViewModel.RefreshList();
            this.isEnabled = true;
            this.isRunning = false;

            await App.Navigator.PopAsync();
        }
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

            this.Category = this.MyCategories.Where(x => x.CategoryId == this.product.CategoryId).FirstOrDefault();

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
        #endregion
    }
}
