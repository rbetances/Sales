namespace Sales.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Common.Models;
    using GalaSoft.MvvmLight.Command;
    using Helpers;
    using Sales.Resources;
    using Services;
    using Xamarin.Forms;

    public class CategoriesViewModel : BaseViewModel
    {
        #region Attributes
        private string filter;

        private ApiService apiService;

        private DataService dataService;

        private bool isRefreshing;

        private ObservableCollection<CategoryItemViewModel> categories;

        private List<Product> allProducts = new List<Product>();
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

        public List<Category> MyCategories { get; set; }

        public ObservableCollection<CategoryItemViewModel> Categories
        {
            get { return this.categories; }
            set { this.SetProperty(ref this.categories, value); }
        }

        public bool IsRefreshing
        {
            get { return this.isRefreshing; }
            set { this.SetProperty(ref this.isRefreshing, value); }
        }
        #endregion

        #region Constructors
        public CategoriesViewModel()
        {
            this.apiService = new ApiService();
            this.dataService = new DataService();
            this.LoadCategories();
        }
        #endregion

        #region Methods
        private async void LoadCategories()
        {
            this.IsRefreshing = true;

            var connection = await this.apiService.CheckConnection();
            await Task.Delay(1000);
            if (!connection.IsSuccess)
            {
                await this.LoadCategoriesFromDB();
                this.IsRefreshing = false;
                this.RefreshList();
                return;
            }

            var url = Application.Current.Resources["UrlApi"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlCategoriesController"].ToString();
            var response = await this.apiService.GetList<Category>(url, prefix, controller, Settings.TokenType, Settings.AccessToken);
            if (!response.IsSuccess)
            {
                await this.LoadCategoriesFromDB();
                this.IsRefreshing = false;
                if (this.MyCategories == null || this.MyCategories.Count == 0)
                    await Application.Current.MainPage.DisplayAlert(
                        Languages.Error,
                        Resource.NoProductsMessage,
                        "OK");
                else
                    this.RefreshList();

                return;
            }

            this.MyCategories = (List<Category>)response.Result;
            this.SaveCategoriesToDB();
            this.SaveAllProducts();
            this.RefreshList();
            this.IsRefreshing = false;
        }

        private void RefreshList()
        {
            if (string.IsNullOrEmpty(this.Filter))
            {
                var myListCategoriesItemViewModel = this.MyCategories.Select(c => new CategoryItemViewModel
                {
                    CategoryId = c.CategoryId,
                    Description = c.Description,
                    ImagePath = c.ImagePath
                });

                this.Categories = new ObservableCollection<CategoryItemViewModel>(
                    myListCategoriesItemViewModel.OrderBy(c => c.Description));
            }
            else
            {
                var myListCategoriesItemViewModel = this.MyCategories.Select(c => new CategoryItemViewModel
                {
                    CategoryId = c.CategoryId,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                }).Where(c => c.Description.ToLower().Contains(this.Filter.ToLower())).ToList();

                this.Categories = new ObservableCollection<CategoryItemViewModel>(
                    myListCategoriesItemViewModel.OrderBy(c => c.Description));
            }
        }
        private async Task LoadCategoriesFromDB()
        {
            this.MyCategories = await this.dataService.GetAllCategories();
        }

        private async Task SaveCategoriesToDB()
        {
            await this.dataService.DeleteAllCategories();
            await this.dataService.Insert(this.MyCategories);
        }

        private async Task SaveAllProducts()
        {
            var url = Application.Current.Resources["UrlApi"].ToString();
            var prefix = Application.Current.Resources["UrlPrefix"].ToString();
            var controller = Application.Current.Resources["UrlProductsController"].ToString();
            var response = await this.apiService.GetList<Product>(url, prefix, controller, Settings.TokenType, Settings.AccessToken);

            if (response.IsSuccess)
            {
                allProducts = (List<Product>)response.Result;
                await SaveAllProductsLocal();
            }
        }
        private async Task SaveAllProductsLocal()
        {
            await this.dataService.DeleteAllProducts();
            await this.dataService.Insert(allProducts);
        }

        #endregion

        #region Commands
        public ICommand SearchCommand
        {
            get
            {
                return new RelayCommand(RefreshList);
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new RelayCommand(LoadCategories);
            }
        }
        #endregion
    }
}