using CWO_App.UI.Services;
using CWO_App.UI.ViewModels.SharedParametersViewModels;
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

namespace CWO_App.UI.Views.SharedParameterViews
{
    /// <summary>
    /// Interaction logic for FamilyParameters_Window.xaml
    /// </summary>
    public partial class FamilyParameters_Window : Window
    {
        private readonly IWindowService _windowService;
        public FamilyParameters_Window(FamilyParameters_ViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            _windowService = vm.WindowService;

            Loaded += FamilyParameters_View_Loaded;
        }

        private void FamilyParameters_View_Loaded(object sender, RoutedEventArgs e)
        {
            _windowService.RaiseWindowOpened();
        }
    }
}
