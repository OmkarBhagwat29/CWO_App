using CWO_App.UI.Models;
using CWO_App.UI.Services;
using CWO_App.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CWO_App.UI.Views
{
    /// <summary>
    /// Interaction logic for BrickWindow.xaml
    /// </summary>
    public partial class BrickWindow : Window
    {
        // BrickViewModel brickViewModel;
        private readonly IWindowService _windowService;
        private BrickEvaluator_ViewModel _vm;
        public BrickWindow(BrickEvaluator_ViewModel vm)
        {
            _vm = vm;
            DataContext = vm;
            _windowService = vm.WindowService;
            Loaded += BrickWindow_Loaded;

            InitializeComponent();
        }

        private void BrickWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _windowService.RaiseWindowOpened();
        }

        //private void OnRunClick(object sender, RoutedEventArgs e)
        //{
        //    brickViewModel.Run(mask.Text);
        //}

        //private void OnClearClick(object sender, RoutedEventArgs e)
        //{
        //    brickViewModel.Clear();
        //}

        //private void OnCheckClick(object sender, RoutedEventArgs e)
        //{
        //    brickViewModel.CheckNumber(calcValue.Text);
        //}

        private void listItemClick(object sender, RoutedEventArgs e)
        {
            var item = sender as ListViewItem;
            BrickSpecial b = item.Content as BrickSpecial;
            //calcValue.Text = b.Value.ToString();
            _vm.WallLength = b.Value;
            _vm.CheckNumber();
        }
        private void historyItemClick(object sender, RoutedEventArgs e)
        {
            var item = sender as ListViewItem;
            BrickSpecial b = item.Content as BrickSpecial;
            //calcValue.Text = b.Value.ToString();
            _vm.WallLength = b.Value;
            _vm.CheckNumber();
        }
    }
}
