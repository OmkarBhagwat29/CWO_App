
using Autodesk.Revit.UI;
using CWO_App.UI.Services;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External.Handlers;
using System.Collections.ObjectModel;
using RevitCore.Extensions;

namespace CWO_App.UI.ViewModels
{
    public sealed partial class BrickEvaluator_ViewModel: ObservableObject
    {
        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private readonly ILogger _logger;
        public IWindowService WindowService;

        [ObservableProperty] private string _selectedDimensionType;
        [ObservableProperty] private ObservableCollection<string> _dimensionTypes = new ObservableCollection<string>();


        public BrickEvaluator_ViewModel(ILogger<BrickEvaluator_ViewModel> logger, IWindowService windowService)
        {
            // Constructor initialization...
            _logger = logger;
            WindowService = windowService;

            // Subscribe to the WindowOpened event
            WindowService.WindowOpened += OnWindowOpened;
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            _externalHandler.Raise(uiApp=>SetLinearDimensionTypes(uiApp));
        }

        [RelayCommand]
        private void SelectDimensions()
        {
            TaskDialog.Show("Message", "Binding is working!!!");
        }


        private void SetLinearDimensionTypes(UIApplication uiApp)
        {
            List<string> linearDimensionNames = uiApp.ActiveUIDocument.Document
                .GetInstancesOfCategory(BuiltInCategory.OST_Dimensions, (e) => {

                if (!(e is Dimension d))
                    return false;

                return d.DimensionShape == DimensionShape.Linear;
            }).Select(e => e.Name).Distinct().ToList();

            if (linearDimensionNames == null || linearDimensionNames.Count == 0)
            {
#if DEBUG
                _logger.LogError("No Linear Dimensions Found in the Project.");
#else
_logger.LogError("No Linear Dimensions Found in the Project.");
TaskDialog.Show("Message", "No Linear Dimensions Found in the Project");
#endif
                return;
                
            }
            this.DimensionTypes.Clear();
            linearDimensionNames.ForEach(name=> this.DimensionTypes.Add(name));
            this.SelectedDimensionType = this.DimensionTypes.FirstOrDefault();
        }

    }
}
