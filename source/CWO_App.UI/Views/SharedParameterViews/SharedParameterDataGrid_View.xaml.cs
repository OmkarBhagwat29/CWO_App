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

        public SharedParameterDataGrid_View()
        {
            InitializeComponent();
            this.dataGrid.BorderBrush = new SolidColorBrush(Colors.Black);
            this.dataGrid.BorderThickness = new Thickness(1);

        }

    }
}
