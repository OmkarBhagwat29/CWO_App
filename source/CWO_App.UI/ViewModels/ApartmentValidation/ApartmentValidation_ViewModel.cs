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
                    this._logger.LogError("\"Unable to read Validation Standard File!!!");
                    return;
                }
               

                //Validate
                _externalHandler.Raise((uiApp) => {

                    _model = new ApartmentValidationModel(_logger, uiApp, _standards);
                    var file = uiApp.Application.OpenSharedParameterFile();
                    if (file == null)
                    {
                        TaskDialog.Show("Message", "Shared Parameter File Not Found");
                        //WindowController.Close<ApartmentValidation_Window>();
                        return;
                    }
                    _model.SetDefinitionFile(file);

                    bool sharedParamExists = _model.CheckSharedParametersExists();
                    if (!sharedParamExists)
                    {
                        TaskDialog.Show("Message", "No Shared Parameters found for Validation.\nUse correct shared parameter file");
                        //WindowController.Close<ApartmentValidation_Window>();
                        return;
                    }

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


                    ApartmentValidationModel.CheckRequiredParametersExists(_areaElement, _roomElement, out List<string> missingParams);

                    if (missingParams.Count > 0)
                    {
                        // TaskDialog.Show("Message", String.Join(Environment.NewLine,missingParams)); 

                        uiApp.ActiveUIDocument.Document.UseTransaction(() =>
                        {

                            _model.CreateProjectParameters();

                            
                        },
                            "Assigned Project Params");
                    }

                    _parametersFound = true;
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

        private ActionEventHandler _externalHandler = new();
        private AsyncEventHandler _asyncExternalHandler = new();
        private AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private static ApartmentStandards _standards = null;

        [ObservableProperty]private ObservableCollection<ValidationResult> _widthValidationResults = [];
        [ObservableProperty]private ObservableCollection<ValidationResult> _areaValidationResults = [];

        [ObservableProperty] private double _singleDoubleBedThreshold = 10.0;

        bool _parametersFound = false;


        public ICommand ItemDoubleClickCommand { get; }

        private void OnItemDoubleClick(object validationObject)
        {
            var validationResult = validationObject as ValidationResult;
            if (validationResult != null)
            {
                _externalHandler.Raise(uiApp =>
                {
                    var uiDoc = uiApp.ActiveUIDocument;
                    var doc = uiDoc.Document;

                    var elm = doc.GetElement(validationResult.Element_Id);
                    if (elm != null)
                        uiDoc.Selection.SetElementIds([validationResult.Element_Id]);
                    else
                    {
                        //uiDoc.Selection.
                    }
                });
            }
        }


        [RelayCommand]
        public async Task Validate()
        {
            try
            {
               await _asyncExternalHandler.RaiseAsync((uiApp) => {

                   if(!double.IsNaN(this.SingleDoubleBedThreshold))
                        CWO_Apartment.BedroomAreaThreshold = this.SingleDoubleBedThreshold;

                   _model.Apartments.Clear();
                   
                   _model.SetAreaRoomAssociation();
                   _model.SetApartments(false);
                   _model.Validate();


                });

                if (_model.Apartments.Count == 0)
                {
                    TaskDialog.Show("Message", "No Apartments Found");
                    return;
                }

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

            if (_parametersFound == false)
            {
                TaskDialog.Show("Message", "Validation Parameters not found.\nCheck Correct Shared Parameter File is Used &" +
                    "Project Parameters are assigned to Areas and Rooms");
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

    }
}
