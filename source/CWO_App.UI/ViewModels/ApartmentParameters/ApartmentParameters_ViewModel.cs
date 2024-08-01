using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using CWO_App.UI.Constants;
using CWO_App.UI.Models.ApartmentValidation;
using CWO_App.UI.Requirements;
using CWO_App.UI.Services;
using CWO_App.UI.Utils;
using CWO_App.UI.Views.ApartmentParametersViews;
using CWO_App.UI.Views.ApartmentValidationViews;
using DocumentFormat.OpenXml.Vml;
using Microsoft.Extensions.Logging;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitCore.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CWO_App.UI.ViewModels.ApartmentParameters
{
    public sealed partial class ApartmentParameters_ViewModel : ObservableObject
    {
        private readonly ILogger _logger;
        public IWindowService WindowService;

        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        Element _roomElement;
        Element _areaElement;

        private ApartmentValidationModel _model;
        private static ApartmentStandards _standards = null;


        ApartmentParameters_Window _mainWindow;

        [ObservableProperty] private ObservableCollection<CategorizedApartmentViewModel> _filteredApartments = [];

        [ObservableProperty] private string _totalApartmentContent = "Apartment Count: ";

        public ApartmentParameters_ViewModel(ILogger<ApartmentParameters_ViewModel> logger,
            IWindowService windowService)
        {
            _logger = logger;
            WindowService = windowService;

            WindowService.WindowOpened += OnWindowOpened;
        }


        private void OnWindowOpened(object sender, EventArgs e)
        {
            try
            {
                this.FilteredApartments.Clear();
                
                var window = WindowController.GetWindow<ApartmentParameters_Window>();
                _mainWindow = window as ApartmentParameters_Window;

                _standards = ApartmentStandards.LoadFromJson();
                _externalHandler.Raise(uiApp =>
                {
                    _model = new ApartmentValidationModel(_logger, uiApp, _standards);

                    var file = uiApp.Application.OpenSharedParameterFile();
                    if (file == null)
                    {
                        TaskDialog.Show("Message", "Shared Parameter File Not Found");
                        //WindowController.Close<ApartmentValidation_Window>();
                        return;
                    }
                    _model.SetDefinitionFile(file);



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

                });

            }
            catch
            {

            }

        }

        [RelayCommand]
        private void Run()
        {
            try
            {
                this.TotalApartmentContent = "Apartment Count: ";

                _model.Apartments.Clear();
                _model.SetAreaRoomAssociation();
                _model.SetApartments(false);
                _model.SetApartmentEntities();

                this.CreateTreeNodes();

                this.TotalApartmentContent += _model.Apartments.Count;

            }
            catch
            {

            }
        }

        [RelayCommand]
        private void SetParameters()
        {
            try
            {
                TaskDialog.Show("Message", "Parameters Set");
            }
            catch
            {

            }
        }

        public void ApartmentTrv_SelectedItemChanged(object treeItem)
        {
            var treeVm = treeItem as CategorizedApartmentViewModel;
            if (treeVm != null)
            {

                Task.Run(async () =>
                {
                    await _asyncExternalHandler.RaiseAsync(uiApp =>
                    {
                        var elm = uiApp.ActiveUIDocument.Document.GetElement(treeVm.ElementId);

                        if (elm != null)
                        {
                            uiApp.ActiveUIDocument.Selection.SetElementIds([treeVm.ElementId]);
                        }
                    });

                });

            }
        }


        private void CreateTreeNodes()
        {
            this.FilteredApartments.Clear();

            var apartmentsGroup = _model.Apartments
                .OrderBy(a => a.AreaBoundary.Level.Elevation)
                .OrderBy(a => a.ToString())
                .GroupBy(a => a.AreaBoundary.LevelId).ToList();

            foreach (var aptsG in apartmentsGroup)
            {
                CategorizedApartmentViewModel cvLevel = new CategorizedApartmentViewModel
                {
                    ElementId = aptsG.Key,
                    Parent = _mainWindow,
                    Children = []
                };

                foreach (var apt in aptsG)
                {
                    cvLevel.Name = apt.AreaBoundary.Level.Name;

                    var aptName = apt.ToString();

                    CategorizedApartmentViewModel cvMain = new();
                    cvMain.Name = $"{aptName}";
                    cvMain.ElementId = apt.AreaBoundary.Id;
                    cvMain.Parent = cvLevel;
                    cvMain.Children = [];

                    // add rooms as a children
                    CategorizedApartmentViewModel cvRoom = new();
                    cvRoom.Name = "Rooms";
                    cvRoom.Parent = cvMain;

                    cvMain.Children.Add(cvRoom);

                    apt.Rooms.ForEach(r => {

                        CategorizedApartmentViewModel cR = new();

                        cR.Name = r.Name;
                        cR.ElementId = r.Room.Id;
                        cR.Parent = cvRoom;
                        cR.Children = [];

                        cvRoom.Children.Add(cR);
                    });

                    this.AddToTree(cvMain, apt, BuiltInCategory.OST_Doors);
                    this.AddToTree(cvMain, apt, BuiltInCategory.OST_Windows);
                    this.AddToTree(cvMain, apt, BuiltInCategory.OST_GenericModel);
                    this.AddToTree(cvMain, apt, BuiltInCategory.OST_Casework);


                    cvLevel.Children.Add(cvMain);
                }

                cvLevel.Name += $": ({cvLevel.Children.Count})";

                this.FilteredApartments.Add(cvLevel);
            }


        }

        private void AddToTree(CategorizedApartmentViewModel mainNode,CWO_Apartment apt, BuiltInCategory category)
        {
            CategorizedApartmentViewModel c = new();
            c.Parent = mainNode;

            if (category == BuiltInCategory.OST_Doors)
                c.Name = "Doors";
            else if (category == BuiltInCategory.OST_Windows)
                c.Name = "Windows";
            else if (category == BuiltInCategory.OST_GenericModel)
                c.Name = "Generics";
            else
                c.Name = "Caseworks";

            var entities = apt.GetSpecificEntities(category);
            int i = 1;
            entities.ForEach(e => {

                CategorizedApartmentViewModel cE = new CategorizedApartmentViewModel
                {
                    Name = $"{i}. Family Name: {e.Symbol.Family.Name}\nFamily Type: {e.Symbol.Name}\nElementID: {e.Id}\n",
                    Parent = c,
                    ElementId = e.Id,
                    Children = []
                };

                c.Children.Add(cE);
                i++;
            });

            mainNode.Children.Add(c);
        }
    }
}
