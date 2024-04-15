
using CWO_App.UI.Services;
using CWO_App.UI.ViewModels;
using System.Windows;



namespace CWO_App.UI.Views
{
    /// <summary>
    /// Interaction logic for BrickEvaluator_View.xaml
    /// </summary>
    public partial class BrickEvaluator_View
    {
        private readonly IWindowService _windowService;
        public BrickEvaluator_View(BrickEvaluator_ViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();

            _windowService = vm.WindowService;

            Loaded += BrickEvaluator_View_Loaded;
        }

        private void BrickEvaluator_View_Loaded(object sender, EventArgs e)
        {
            // Raise the WindowOpened event when the window is loaded
            _windowService.RaiseWindowOpened();
        }
    }
}
