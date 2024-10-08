﻿using CWO_App.UI.Services;
using CWO_App.UI.ViewModels.ApartmentValidation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace CWO_App.UI.Views.ApartmentValidationViews
{
    /// <summary>
    /// Interaction logic for ApartmentValidation_Window.xaml
    /// </summary>
    public partial class ApartmentValidation_Window : Window
    {
        private readonly IWindowService _windowService;
        //private readonly ApartmentValidation_ViewModel vm;
        public ApartmentValidation_Window(ApartmentValidation_ViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            _windowService = vm.WindowService;
            Loaded += ApartmentValidation_Window_Loaded;
        }

        private void ApartmentValidation_Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowService.RaiseWindowOpened();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem listViewItem)
            {
                var content = listViewItem.Content;
                if (content != null)
                {
                    var viewModel = DataContext as ApartmentValidation_ViewModel;
                    if (viewModel != null && viewModel.ItemDoubleClickCommand.CanExecute(content))
                    {
                        viewModel.ItemDoubleClickCommand.Execute(content);
                    }
                }
            }

        }
    }
}
