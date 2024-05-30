using CWO_App.UI.Models.ApartmentValidation;
using CWO_App.UI.Requirements;
using CWO_App.UI.Services;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitCore.Extensions;
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
using CWO_App.UI.Utils;
using CWO_App.UI.Views.ApartmentValidationViews;
using Autodesk.Revit.DB.Architecture;
using System.Windows.Input;
using CWO_App.UI.Constants;

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

            ItemDoubleClickCommand = new RelayCommand<ValidationResult>(OnItemDoubleClick);
        }


        Element _roomElement;
        Element _areaElement;
        private void OnWindowOpened(object sender, EventArgs e)
        {
            try
            {
               _standards = ApartmentStandards.LoadFromJson();
                if (_standards == null)
                {
                    TaskDialog.Show("Message", "Unable to read Validation Standard File. Please Check the file!!!");
                    return;
                }

                //Validate
                _externalHandler.Raise((uiApp) => {

                    var spatialElements = uiApp.ActiveUIDocument.Document
                    .GetElements<SpatialElement>();
                    _areaElement = spatialElements.Where(e => e is Area).FirstOrDefault();

                    if (_areaElement == null)
                    {
                        TaskDialog.Show("Message", "No Areas found in the Project");

                        WindowController.Close<ApartmentValidation_Window>();

                        return;
                    }

                    _roomElement = spatialElements.Where(e => e is Room).FirstOrDefault();

                    if (_roomElement == null)
                    {
                        TaskDialog.Show("Message", "No Rooms found in the Project");

                        WindowController.Close<ApartmentValidation_Window>();

                        return;
                    }

                    _model = new ApartmentValidationModel(_logger, uiApp, _standards);

                    ApartmentValidationModel.CheckRequiredParametersExists(_areaElement, _roomElement, out List<string> missingParams);

                    if (missingParams.Count > 0)
                    {
                        TaskDialog.Show("Message", String.Join(Environment.NewLine,missingParams)); 

                       // WindowController.Close<ApartmentValidation_Window>();

                        return;
                    }
                });
            }
            catch (Exception)
            {
                WindowController.Close<ApartmentValidation_Window>();
                return;
            }
        }

        private readonly ILogger _logger;
        public IWindowService WindowService;

        private ApartmentValidationModel _model;

        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private static ApartmentStandards _standards = null;

        [ObservableProperty]private ObservableCollection<ValidationResult> _widthValidationResults = [];
        [ObservableProperty]private ObservableCollection<ValidationResult> _areaValidationResults = [];


        public ICommand ItemDoubleClickCommand { get; }

        private void OnItemDoubleClick(object validationObject)
        {
            var validationResult = validationObject as ValidationResult;
            if (validationResult != null)
            {
                _externalHandler.Raise(uiApp =>
                {
                    var uiDoc = uiApp.ActiveUIDocument;

                    uiDoc.Selection.SetElementIds([validationResult.Element_Id]);
                });
            }
        }


        [RelayCommand]
        public async Task Validate()
        {
            try
            {
               await _asyncExternalHandler.RaiseAsync((uiApp) => {

                   _model.SetAreaRoomAssociation();
                   _model.SetApartments();
                   _model.Validate();

                   if (_model.Apartments.Count == 0)
                   {
                       TaskDialog.Show("Message", "No Apartments Found");
                   }
                });

                this.WidthValidationResults.Clear();
                this.AreaValidationResults.Clear();

                _model
                    .GetFailedValidation(out List<ValidationResult> widthFailedResults,
                    out List<ValidationResult> areaFailedResults);

                foreach (var result in widthFailedResults)
                {
                    this.WidthValidationResults.Add(result);
                }

                foreach (var result in areaFailedResults)
                {
                    this.AreaValidationResults.Add(result);
                }

                TaskDialog.Show("Message", "Validation Completed!!!");

            }
            catch (Exception)
            {
                TaskDialog.Show("Error", "Validation Failed.");
            }
        }

        [RelayCommand]
        public async Task SetValidationParameters()
        {
            if (_model == null)
            {
                TaskDialog.Show("Message", "Assign Validation Parameters before setting parameters.\nUse Assign Parameters Button");
                return;
            }

            try
            {
                await _asyncExternalHandler.RaiseAsync((uiApp) => {

                    var doc = uiApp.ActiveUIDocument.Document;

                    doc.UseTransaction(() => { 
                    
                        _model.SetApartmentParameters();

                    });
                                    
                });

                TaskDialog.Show("Message", "Validation Parameters set to Areas and Rooms Successfully.");
            }
            catch (Exception)
            {
                TaskDialog.Show("Error", "Unable to set validation parameters to Areas and Rooms.");
            }

        }

        /// <summary>
        /// Use this to create shared parameters
        /// </summary>
        /// <returns></returns>
        public async Task AssignValidationParameters()
        {
            this.AreaValidationResults.Clear();
            this.WidthValidationResults.Clear();
            try
            {
                await _asyncExternalHandler.RaiseAsync((uiApp) =>
                {
                    _model = new ApartmentValidationModel(_logger, uiApp, _standards);

                    var definitionFile = uiApp.Application.OpenSharedParameterFile();
                    if (definitionFile == null)
                    {
                        TaskDialog.Show("Message", "No Shared Parameter File Found, Please create on to Procced");

                        WindowController.Close<ApartmentValidation_Window>();

                        return;
                    }
                    _model.SetDefinitionFile(definitionFile);

                    _model.CreateAndAssignRequiredParameters(_areaElement, _roomElement);

                    TaskDialog.Show("Message", "Validation Parameters Assigned to Room and Areas Successfully");
                });
            }
            catch (Exception)
            {
                TaskDialog.Show("Error", "Can not assign validation parameters to the project");
                return;
            }

        }

        [RelayCommand]
        public async Task CreateSchedules()
        {
            try
            {
                await _asyncExternalHandler.RaiseAsync((uiApp) => {

                    //get area scheme iD
                    //var scheme = uiApp.ActiveUIDocument
                    // .Document
                    // .GetElements<AreaScheme>()
                    // .FirstOrDefault(s => s.Name == this.SelectedAreaSchemeName);

                    //_model.CreateAreaValidationSchedule(scheme.Id);
                });

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
