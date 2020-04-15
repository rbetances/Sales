using GalaSoft.MvvmLight.Command;
using Sales.ViewModels;
using Sales.Views;
using System;
using System.Windows.Input;
using Xamarin.Forms;

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
