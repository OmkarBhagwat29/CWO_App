using CWO_App.UI.Services;
using CWO_App.UI.ViewModels.ApartmentParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CWO_App.UI.Views.ApartmentParametersViews
{
    /// <summary>
    /// Interaction logic for ApartmentParameters_Window.xaml
    /// </summary>
    public partial class ApartmentParameters_Window : Window
    {
        private readonly IWindowService _windowService;
        public ApartmentParameters_Window(ApartmentParameters_ViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _windowService = vm.WindowService;
            Loaded += ApartmentParameters_Window_Loaded;
        }

        private void ApartmentParameters_Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowService.RaiseWindowOpened();
        }


        private void ApartmentTrv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView item)
            {
                var vm = this.DataContext as ApartmentParameters_ViewModel;

                vm?.ApartmentTrv_SelectedItemChanged(item.SelectedItem);
            }
        }
    }
}
