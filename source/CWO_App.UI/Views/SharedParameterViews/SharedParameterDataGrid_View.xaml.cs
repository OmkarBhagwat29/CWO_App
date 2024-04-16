using CWO_App.UI.ViewModels.SharedParametersViewModels;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Drawing;

namespace CWO_App.UI.Views.SharedParameterViews
{
    /// <summary>
    /// Interaction logic for SharedParameterDataGrid_View.xaml
    /// </summary>
    public partial class SharedParameterDataGrid_View : UserControl
    {
        // Define a dependency property for the Text property of the TextBlock
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
                typeof(SharedParameterDataGrid_View), new PropertyMetadata("Shared Parameter File"));

        // Property to get/set the value of the dependency property
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty DataGridWidthProperty =
    DependencyProperty.Register("DataGridWidth", typeof(double), typeof(SharedParameterDataGrid_View), new PropertyMetadata(400.0));

        public double DataGridWidth
        {
            get { return (double)GetValue(DataGridWidthProperty); }
            set { SetValue(DataGridWidthProperty, value); }
        }

        public static readonly DependencyProperty DataGridHeightProperty =
DependencyProperty.Register("DataGridHeight", typeof(double), typeof(SharedParameterDataGrid_View), new PropertyMetadata(400.0));

        public double DataGridHeight
        {
            get { return (double)GetValue(DataGridHeightProperty); }
            set { SetValue(DataGridHeightProperty, value); }
        }


        // Define a dependency property for the Width property of the TextBox
        public static readonly DependencyProperty TextBoxWidthProperty =
            DependencyProperty.Register("TextBoxWidth", typeof(double), typeof(SharedParameterDataGrid_View), new PropertyMetadata(200.0));

        // Property to get/set the value of the dependency property
        public double TextBoxWidth
        {
            get { return (double)GetValue(TextBoxWidthProperty); }
            set { SetValue(TextBoxWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnHeader_2Property =
            DependencyProperty.Register("ColumnHeader_2", typeof(string), typeof(SharedParameterDataGrid_View), new PropertyMetadata("Group"));

        public string ColumnHeader_2
        {
            get => (string)GetValue(ColumnHeader_2Property);
            set { SetValue(ColumnHeader_2Property, value); }
        }

        public static readonly DependencyProperty ColumnHeader_3Property =
            DependencyProperty.Register("ColumnHeader_3", typeof(string), typeof(SharedParameterDataGrid_View), new PropertyMetadata("Name"));

        public string ColumnHeader_3
        {
            get => (string)GetValue(ColumnHeader_3Property);
            set { SetValue(ColumnHeader_3Property, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SharedParameterDataGrid_View), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ButtonCommandProperty =
    DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(SharedParameterDataGrid_View), new PropertyMetadata(null));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty FileFolderPathProperty =
            DependencyProperty.Register("FileFolderPath", typeof(string), typeof(SharedParameterDataGrid_View), new PropertyMetadata(string.Empty));

        public string FileFolderPath
        {
            get { return (string)GetValue(FileFolderPathProperty); }
            set { SetValue(FileFolderPathProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
    DependencyProperty.Register("IsSelected", typeof(string), typeof(SharedParameterDataGrid_View), new PropertyMetadata(string.Empty));

        public string IsSelected
        {
            get { return (string)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

    //    public static readonly DependencyProperty CheckBoxCheckedEventProperty =
    //DependencyProperty.Register("CheckBoxCheckedEvent", typeof(RoutedEventHandler), typeof(SharedParameterDataGrid_View));

    //    public static readonly DependencyProperty CheckBoxUncheckedEventProperty =
    //        DependencyProperty.Register("CheckBoxUncheckedEvent", typeof(RoutedEventHandler), typeof(SharedParameterDataGrid_View));

    //    public event RoutedEventHandler CheckBoxCheckedEvent
    //    {
    //        add { AddHandler(CheckBox.CheckedEvent, value); }
    //        remove { RemoveHandler(CheckBox.CheckedEvent, value); }
    //    }

    //    public event RoutedEventHandler CheckBoxUncheckedEvent
    //    {
    //        add { AddHandler(CheckBox.UncheckedEvent, value); }
    //        remove { RemoveHandler(CheckBox.UncheckedEvent, value); }
    //    }

        public SharedParameterDataGrid_View()
        {
            InitializeComponent();
            this.dataGrid.BorderBrush = new SolidColorBrush(Colors.Black);
            this.dataGrid.BorderThickness = new Thickness(1);
        }

        //private void CheckBox_SelectAll_Checked(object sender, RoutedEventArgs e)
        //{
        //    // Handle the checked event
        //}

        //private void CheckBox_SelectAll_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    // Handle the unchecked event
        //}
    }
}
