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
        private readonly FamilyParameters_ViewModel vm;

        public bool WasParamCrtlPressed { get; private set; }

        public bool WasFamilyCtrlPressed { get; private set; }
        public FamilyParameters_Window(FamilyParameters_ViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            this.vm = vm;

            _windowService = vm.WindowService;

            Loaded += FamilyParameters_View_Loaded;

        }

        private void FamilyParameters_View_Loaded(object sender, RoutedEventArgs e)
        {
            _windowService.RaiseWindowOpened();
        }

        private void DataGrid_OnParamKeyDown(object sender, KeyEventArgs e)
        {
            this.WasParamCrtlPressed = e.IsDown && e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl;
        }

        private void DataGrid_OnFamilyKeyDown(object sender, KeyEventArgs e)
        {
            this.WasFamilyCtrlPressed = e.IsDown && e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl;
        }

        private void SelectionParamCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.WasParamCrtlPressed) return;

            var checkBox = (CheckBox)sender;

            this.vm.RowParamMultipleSelection(this.DataGridParameters.SelectedItems, checkBox);
            this.WasParamCrtlPressed = false;
        }

        private void Selected_InstanceChecked(object sender, RoutedEventArgs e)
        {
            if (!this.WasParamCrtlPressed) return;

            var checkBox = (CheckBox)sender;

            this.vm.RowInstanceMultipleSelection(this.DataGridParameters.SelectedItems, checkBox);
            this.WasParamCrtlPressed = false;
        }

        private void Selected_FamilyChecked(object sender, RoutedEventArgs e)
        {
            if (!this.WasFamilyCtrlPressed) return;

            var checkBox = (CheckBox)sender;

            this.vm.RowFamilyMultipleSelection(this.DataGridFamily.SelectedItems, checkBox);
            this.WasFamilyCtrlPressed = false;
        }
    }
}
