using CWO_App.UI.Models.Keynotes;
using CWO_App.UI.ViewModels.KeynotesCreationViewModel;
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

namespace CWO_App.UI.Views.KeynotesCreationViews
{
    /// <summary>
    /// Interaction logic for KeynotesCreation_Window.xaml
    /// </summary>
    public partial class KeynotesCreation_Window : Window
    {

        public KeynotesCreation_Window(KeynotesCreation_ViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item && item.DataContext is CategorizedKeynoteViewModel keynote)
            {
                var vm = this.DataContext as KeynotesCreation_ViewModel;
                vm?.OnKeynoteDoubleClicked(keynote);
            }
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (keynoteTrv.SelectedItem != null)
            {
                var selectedItem = keynoteTrv.SelectedItem;

                // Get the details of the selected item. Assuming it has Category and Description properties.
                var category = selectedItem.GetType().GetProperty("Category")?.GetValue(selectedItem, null)?.ToString();

                // Copy the  string to clipboard
                Clipboard.SetText($"{category}");
            }
        }
    }
}
