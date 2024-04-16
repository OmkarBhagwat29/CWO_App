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
        public FamilyParameters_Window(FamilyParameters_ViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
