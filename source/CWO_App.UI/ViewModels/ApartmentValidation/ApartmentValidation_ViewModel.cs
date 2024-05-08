using CWO_App.UI.Models.ApartmentValidation;
using CWO_App.UI.Requirements;
using CWO_App.UI.Services;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitCore.ResidentialApartments;
using RevitCore.Extensions;
using System.Collections.ObjectModel;

//when manual mode is switched on

namespace CWO_App.UI.ViewModels.ApartmentValidation
{
    public sealed partial class ApartmentValidation_ViewModel: ObservableObject
    {
        public ApartmentValidation_ViewModel(ILogger<ApartmentValidation_ViewModel> logger,
            IWindowService windowService)
        {
            _logger = logger;
            WindowService = windowService;

            WindowService.WindowOpened += OnWindowOpened;
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            //TODO:: Check Shared Parameter File is attached

            //TODO:: FInd all necessary parameters from standards
            //if not there add there ask user if wants add those parameters
            //as it is required for validation

            //Validate
            _externalHandler.Raise((uiApp) => {

                    var element = uiApp.ActiveUIDocument.Document
                    .GetElements<SpatialElement>().First()
                    ?? throw new ArgumentNullException($"No areas or rooms found");

                    var aptTypeParam = element.LookupParameter(_standards.ParameterNames.ApartmentType)
                        ?? throw new ArgumentNullException($"Parameter {_standards.ParameterNames.ApartmentType} Not Found for Areas & Rooms");
                    var roomTypeParam = element.LookupParameter(_standards.ParameterNames.RoomType)
                        ?? throw new ArgumentNullException($"Parameter {_standards.ParameterNames.RoomType} Not Found for Rooms");

                });
            
        }

        private readonly ILogger _logger;
        public IWindowService WindowService;

        private ApartmentValidationModel _model;

        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private static readonly ApartmentStandards _standards = ApartmentStandards.LoadFromJson();

        [ObservableProperty] private bool _isStudioGridVisible = true;
        [ObservableProperty] private bool _isOneBedGridVisible;
        [ObservableProperty] private bool _isTwoBedGridVisible;

        [ObservableProperty] private ObservableCollection<string> _statusTypes = [];
        [ObservableProperty] private string _selectedStatusType;


        [RelayCommand]
        public async Task Validate()
        {
            try
            {
               await _asyncExternalHandler.RaiseAsync((uiApp) => {

                    _model = new ApartmentValidationModel(uiApp, _standards);

                    _model.RunStartToEnd();
                });

                var apartmentsGroup = _model.Apartments.GroupBy(apt => apt.Type).ToList();

                //set dataGrids here
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
