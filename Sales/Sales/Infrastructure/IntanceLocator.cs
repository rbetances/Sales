using Sales.ViewModels;

namespace Sales.Infrastructure
{
    public class IntanceLocator
    {
        public MainViewModel Main { get; set; }

        public IntanceLocator()
        {
            this.Main = new MainViewModel();
        }
    }
}
