using Autodesk.Revit.UI;
using CWO_App.UI.Models;
using CWO_App.UI.Services;
using CWO_App.UI.Utils;
using CWO_App.UI.Views.SharedParameterViews;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitCore.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CWO_App.UI.ViewModels.SharedParametersViewModels
{
    public sealed partial class FamilyParameters_ViewModel: ObservableObject
    {
        private readonly ILogger _logger;
        public IWindowService WindowService;

        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private FamilyParametersModel _model;

        private static string lastOpenedFolder = "";

       // [ObservableProperty] private ObservableCollection<SharedParameterDataRow> _sharedParametersData = [];
        //[ObservableProperty] private ObservableCollection<FamilyDataGridRow> _familiesData = [];

        [ObservableProperty] private CollectionViewSource _searchStringParameterFilterCollection;
        [ObservableProperty] private CollectionViewSource _familyFilterCollection;

        [ObservableProperty] private ObservableCollection<SharedParameterDataRow> _sharedParameterDataRows = new ObservableCollection<SharedParameterDataRow>();
        [ObservableProperty] private ObservableCollection<FamilyDataGridRow> _familyDataRows = new ObservableCollection<FamilyDataGridRow>();

        [ObservableProperty] private ObservableCollection<string> _groupNames = [];
        [ObservableProperty] private string _selectedGroupName;
        [ObservableProperty] private string _parameterNameSearchString;
        [ObservableProperty] private string _familyNameSearchString;

        public FamilyParameters_ViewModel( ILogger<FamilyParameters_ViewModel> logger, IWindowService windowService)
        {
            _logger = logger;
            WindowService = windowService;

            // Subscribe to the WindowOpened event
            WindowService.WindowOpened += OnWindowOpened;
        }

        partial void OnSelectedGroupNameChanged(string oldValue, string newValue)
        {
            if (this.SearchStringParameterFilterCollection == null) return;

            this.SearchStringParameterFilterCollection.View.Refresh();
        }

        partial void OnParameterNameSearchStringChanged(string oldValue, string newValue)
        {
            if (this.SearchStringParameterFilterCollection == null) return;

            this.SearchStringParameterFilterCollection.View.Refresh();
        }

        partial void OnFamilyNameSearchStringChanged(string oldValue, string newValue)
        {
            if(this.FamilyFilterCollection == null) return;

            this.FamilyFilterCollection.View.Refresh();
        }

        private void OnWindowOpened(object sender, EventArgs e)
        {
            //get shared parameters 
            _externalHandler.Raise((uiApp) =>
            {
                var app = uiApp.Application;
                var definitionFile = app.OpenSharedParameterFile();
                if (definitionFile == null)
                {
                    Autodesk.Revit.UI.TaskDialog.Show("Message", "NO shared parameter file found in the Project");
                    WindowController.Close<FamilyParameters_Window>();
                    return;
                }
                
                _model = new FamilyParametersModel(definitionFile);
                _model.SetAllParameters();

                this.GroupNames.Clear();
                this.GroupNames.Add("");

                this.SharedParameterDataRows.Clear();
                _model.AllParameters
                .ForEach(d => {
                    SharedParameterDataRows.Add(new SharedParameterDataRow()
                    {
                        IsSelected = false,
                        ParameterGroup = d.groupName,
                        ParameterName = d.parameterName
                    });

                    if(!this.GroupNames.Contains(d.groupName))
                        this.GroupNames.Add(d.groupName);
                });


                this.SearchStringParameterFilterCollection = new CollectionViewSource
                {
                    Source = this.SharedParameterDataRows
                };

                this.SearchStringParameterFilterCollection.Filter += GetParameterSearchStringFilter;

            });
        }


        private async Task SetSelectedDataToModel()
        {
            //get selected parameters and families

            var selectedFamilyNames = this.FamilyDataRows.Where(f => f.IsSelected)
                .Select(f => f.Family).ToList();

            if (selectedFamilyNames == null || selectedFamilyNames.Count == 0)
            {
                return;
            }

            var selectedParameterData = this.SharedParameterDataRows.Where(x => x.IsSelected).ToList();

            if (selectedParameterData == null || selectedParameterData.Count == 0)
            {
                return;
            }

            _model.SetSelectedExternalDefinitions(selectedParameterData,GroupTypeId.Text);

            await _asyncExternalHandler.RaiseAsync((uiApp) => {

                uiApp.ActiveUIDocument.Document.UseTransaction(() =>
                    _model.LoadSelectedFamilies(uiApp.ActiveUIDocument.Document, selectedFamilyNames),
                    "Families loaded");
            });
        }


        [RelayCommand]
        private async Task ApplyParameters()
        {
            if (_model == null)
                return;

            //set selected data to model
            await this.SetSelectedDataToModel();

            if (_model.Definitions.Count == 0 || _model.LoadedFamilies.Count == 0)
                return;

            try
            {
               await _asyncExternalHandler.RaiseAsync((uiApp) => {
                    _model.ApplySharedParameters(uiApp.ActiveUIDocument.Document);

                    uiApp.ActiveUIDocument.Document.UseTransaction(() => uiApp.ActiveUIDocument.Document.Regenerate(), "Regenerate");

                    Autodesk.Revit.UI.TaskDialog.Show("Message", "Shared Parameters added to Families!!!");

                });
            }
            catch
            {
                Autodesk.Revit.UI.TaskDialog.Show("Error", "Unable to add Shared Parameters to Families!!!");
                
            }

            WindowController.Focus<FamilyParameters_Window>();
        }

        [RelayCommand]
        private async Task DeleteParameters()
        {
            if (_model == null)
                return;

            //set selected data to model
            await this.SetSelectedDataToModel();

            if (_model.Definitions.Count == 0 || _model.LoadedFamilies.Count == 0)
                return;

            try
            {
                await _asyncExternalHandler.RaiseAsync((uiApp) => {

                    _model.DeleteSharedParameters(uiApp.ActiveUIDocument.Document);

                    uiApp.ActiveUIDocument.Document.UseTransaction(() => uiApp.ActiveUIDocument.Document.Regenerate(), "Regenerate");

                    Autodesk.Revit.UI.TaskDialog.Show("Message", "Shared Parameters deleted from Families!!!");

                });
            }
            catch
            {

                Autodesk.Revit.UI.TaskDialog.Show("Error", "Unable to delete Shared Parameters from Families!!!");
               
            }

            WindowController.Focus<FamilyParameters_Window>();

        }

        [RelayCommand]
        private async Task SelectFamilyFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";

            if(lastOpenedFolder != string.Empty)
                dialog.InitialDirectory = lastOpenedFolder;

            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
              //  MessageBox.Show("You selected: " + dialog.FileName);
                lastOpenedFolder = dialog.FileName;
            }

            var rfaFiles = Directory.GetFiles(lastOpenedFolder, "*.rfa").ToList();
            if (rfaFiles.Count == 0)
            {
                _logger.LogInformation($"No revit family files found under {lastOpenedFolder}");
                return;
            }

            // do magic
            if (_model != null)
            {
                _model.SetAllFamilies(rfaFiles);
                await SetFamilyData(rfaFiles);
            }

        }

        private async Task SetFamilyData(List<string> rfaFiles,bool getCategories = false)
        {
            await _asyncExternalHandler.RaiseAsync((uiAPp) =>
            {
                var uiDoc = uiAPp.ActiveUIDocument;
                var doc = uiDoc.Document;

                this.FamilyDataRows.Clear();

                    foreach (var rfaFile in rfaFiles)
                    {
                        FamilyDataRows.Add(new FamilyDataGridRow()
                        {
                            Family = Path.GetFileNameWithoutExtension(rfaFile)
                        });
                    }
            });

            this.FamilyFilterCollection = new CollectionViewSource()
            {
                Source = this.FamilyDataRows
            };

            this.FamilyFilterCollection.Filter += GetFamilyCategorySearchFilter;

        }

        private void GetFamilyCategorySearchFilter(object sender, FilterEventArgs e)
        {
            var familyData = (FamilyDataGridRow)e.Item;

            if (this.FamilyNameSearchString == null || this.FamilyNameSearchString == string.Empty)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = familyData.Family.Contains(this.FamilyNameSearchString,StringComparison.CurrentCultureIgnoreCase);
            }

        }

        private void GetParameterSearchStringFilter(object sender, FilterEventArgs e)
        {
            var parameterData = (SharedParameterDataRow)e.Item;

            if (this.SelectedGroupName == null || this.SelectedGroupName == string.Empty)
            {
                if (string.IsNullOrWhiteSpace(this.ParameterNameSearchString))
                    e.Accepted = true;
                else
                    e.Accepted = parameterData.ParameterName.Contains(this.ParameterNameSearchString, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(this.ParameterNameSearchString))
                {
                    e.Accepted = parameterData.ParameterGroup == this.SelectedGroupName;
                }
                else
                {
                    e.Accepted = parameterData.ParameterGroup == this.SelectedGroupName &&
                        parameterData.ParameterName.Contains(this.ParameterNameSearchString, StringComparison.CurrentCultureIgnoreCase);
                }
            }

        }

    }
}
