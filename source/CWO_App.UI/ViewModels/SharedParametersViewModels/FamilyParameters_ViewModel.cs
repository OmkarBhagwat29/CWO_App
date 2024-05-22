using Autodesk.Revit.UI;
using CWO_App.UI.Models;
using CWO_App.UI.Services;
using CWO_App.UI.Utils;
using CWO_App.UI.Views.SharedParameterViews;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitCore.Extensions;
using RevitCore.Extensions.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

#if NET48
using System.Windows.Forms;
#endif



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

        private static string lastOpenedFolder = "C:\\Users";
        private static readonly List<ParameterGroupInfo> ParameterGroups = ParameterExtension.GetParameterGroups().OrderBy(g=>g.Name).ToList();


        [ObservableProperty] private CollectionViewSource _searchStringParameterFilterCollection;
        [ObservableProperty] private CollectionViewSource _familyFilterCollection;

        [ObservableProperty] private ObservableCollection<SharedParameterDataRow> _sharedParameterDataRows = [];
        [ObservableProperty] private ObservableCollection<FamilyDataGridRow> _familyDataRows = [];

        [ObservableProperty] private ObservableCollection<string> _sharedGroupNames = [];
        [ObservableProperty] private string _selectedSharedGroupName;
        [ObservableProperty] private string _parameterNameSearchString;
        [ObservableProperty] private string _familyNameSearchString;

        [ObservableProperty] private ObservableCollection<string> _parameterGroupNames = [];
        [ObservableProperty] private string _selectedParameterGroupName;

        [ObservableProperty] private bool _saveFamily = true;
        [ObservableProperty] private bool _overwriteParameters = false;

        [ObservableProperty] private bool _allParamsSelected;
        [ObservableProperty] private bool _allParamInstanceSelected;
        [ObservableProperty] private bool _allFamilySelected;

        [ObservableProperty] private bool _deleteFromProject = true;
        [ObservableProperty] private bool _deleteFromFamily;

        public FamilyParameters_ViewModel( ILogger<FamilyParameters_ViewModel> logger, IWindowService windowService)
        {
            _logger = logger;
            WindowService = windowService;

            // Subscribe to the WindowOpened event
            WindowService.WindowOpened += OnWindowOpened;
        }

        partial void OnAllFamilySelectedChanged(bool oldValue, bool newValue)
        {
            foreach (var item in this.FamilyDataRows)
            {
                if (!this.FamilyFilterCollection.View.Contains(item)) continue;

                item.IsFamilySelected = newValue;
            }
        }

        partial void OnAllParamsSelectedChanged(bool oldValue, bool newValue)
        {
            foreach (var item in this.SharedParameterDataRows)
            {
                if (!this.SearchStringParameterFilterCollection.View.Contains(item)) continue;

                item.IsSelected = newValue;
            }
        }

        partial void OnAllParamInstanceSelectedChanged(bool oldValue, bool newValue)
        {
            foreach (var item in this.SharedParameterDataRows)
            {
                if (!this.SearchStringParameterFilterCollection.View.Contains(item)) continue;

                item.IsInstance = newValue;
            }
        }

        partial void OnSelectedSharedGroupNameChanged(string oldValue, string newValue)
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

                this.SharedGroupNames.Clear();
                this.SharedGroupNames.Add("");

                this.SharedParameterDataRows.Clear();
                _model.AllParameters
                .ForEach(d => {
                    SharedParameterDataRows.Add(new SharedParameterDataRow()
                    {
                        IsSelected = false,
                        SharedGroup = d.groupName,
                        ParameterName = d.parameterName
                    });

                    if(!this.SharedGroupNames.Contains(d.groupName))
                        this.SharedGroupNames.Add(d.groupName);

                    //ser parameterGroups
                    this.ParameterGroupNames.Clear();

                    ParameterGroups.ForEach(p => this.ParameterGroupNames.Add(p.Name));
                    this.SelectedParameterGroupName = this.ParameterGroupNames.FirstOrDefault();
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

            var selectedFamilyNames = this.FamilyDataRows.Where(f => f.IsFamilySelected)
                .Select(f => f.Family).ToList();


            var selectedParameterData = this.SharedParameterDataRows.Where(x => x.IsSelected).ToList();


            _model.SetSelectedExternalDefinitions(selectedParameterData,ParameterGroups.FirstOrDefault(g=>g.Name==this.SelectedParameterGroupName).ForgeTypeId);
            _model.SaveFamilyFile = this.SaveFamily;
            _model.OverwriteParameterValuesOnLoad = this.OverwriteParameters;

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
                   bool success = _model.ApplySharedParameters(uiApp.ActiveUIDocument.Document);

                    uiApp.ActiveUIDocument.Document.UseTransaction(() => uiApp.ActiveUIDocument.Document.Regenerate(), "Regenerate");

                   if(success)
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

            if (!this.DeleteFromFamily && !this.DeleteFromProject)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Message", "Delete option not selected.");
                return;
            }

            //set selected data to model
            await this.SetSelectedDataToModel();

            if (this.DeleteFromFamily)
            {
                if ( _model.LoadedFamilies.Count == 0)
                    return;
            }

            if (_model.Definitions.Count == 0)
                return;

            try
            {
                await _asyncExternalHandler.RaiseAsync((uiApp) => {

                    var deleted = _model.DeleteSharedParameters(uiApp.ActiveUIDocument.Document,this.DeleteFromProject,this.DeleteFromFamily);

                    uiApp.ActiveUIDocument.Document.UseTransaction(() => uiApp.ActiveUIDocument.Document.Regenerate(), "Regenerate");

                    if(deleted)
                        Autodesk.Revit.UI.TaskDialog.Show("Message", "Shared Parameters deleted!!!");

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
#if NET8_0_OR_GREATER
            // Create a new instance of FolderBrowserDialog
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.InitialDirectory = "lastOpenedFolder";

            if(lastOpenedFolder != string.Empty)
                dialog.InitialDirectory = lastOpenedFolder;

            if ((bool)dialog.ShowDialog())
            {
              //  MessageBox.Show("You selected: " + dialog.FileName);
                lastOpenedFolder = dialog.FolderName;
            }
#else
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
           
        // Set the initial directory (optional)
        folderBrowserDialog.SelectedPath = lastOpenedFolder;

        // Show the dialog and capture the result
        DialogResult result = folderBrowserDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            // Get the selected folder path
            lastOpenedFolder = folderBrowserDialog.SelectedPath;
        }
#endif

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

            if (this.SelectedSharedGroupName == null || this.SelectedSharedGroupName == string.Empty)
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
                    e.Accepted = parameterData.SharedGroup == this.SelectedSharedGroupName;
                }
                else
                {
                    e.Accepted = parameterData.SharedGroup == this.SelectedSharedGroupName &&
                        parameterData.ParameterName.Contains(this.ParameterNameSearchString, StringComparison.CurrentCultureIgnoreCase);
                }
            }

        }

        public void RowParamMultipleSelection(IList selectedRowItem, System.Windows.Controls.CheckBox checkBox) {

            foreach (var selectedItem in selectedRowItem)
            {
                var paramDataRow = (SharedParameterDataRow)selectedItem;
                bool isChecked = checkBox.IsChecked ?? false;

                if (!SearchStringParameterFilterCollection.View.Contains(paramDataRow)) continue;

                paramDataRow.IsSelected = isChecked;
            }
        }

        public void RowInstanceMultipleSelection(IList selectedRowItem, System.Windows.Controls.CheckBox checkBox)
        {
            foreach (var selectedItem in selectedRowItem)
            {
                var paramDataRow = (SharedParameterDataRow)selectedItem;
                bool isChecked = checkBox.IsChecked ?? false;

                if (!SearchStringParameterFilterCollection.View.Contains(paramDataRow)) continue;

                paramDataRow.IsInstance = isChecked;
            }
        }


        public void RowFamilyMultipleSelection(IList selectedRowItem, System.Windows.Controls.CheckBox checkBox)
        {
            foreach (var selectedItem in selectedRowItem)
            {
                var familyDataRow = (FamilyDataGridRow)selectedItem;
                bool isChecked = checkBox.IsChecked ?? false;

                if (!FamilyFilterCollection.View.Contains(familyDataRow)) continue;

                familyDataRow.IsFamilySelected = isChecked;
            }
        }

    }
}
