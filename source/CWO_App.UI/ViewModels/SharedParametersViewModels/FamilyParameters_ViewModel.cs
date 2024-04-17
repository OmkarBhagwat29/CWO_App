using Autodesk.Revit.UI;
using CWO_App.UI.Models;
using CWO_App.UI.Services;
using CWO_App.UI.Utils;
using CWO_App.UI.Views.SharedParameterViews;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Nice3point.Revit.Toolkit.External.Handlers;
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

        private string lastOpenedFolder = "";

       // [ObservableProperty] private ObservableCollection<SharedParameterDataRow> _sharedParametersData = [];
        //[ObservableProperty] private ObservableCollection<FamilyDataGridRow> _familiesData = [];

        [ObservableProperty] private CollectionViewSource _searchStringParameterFilterCollection;
        [ObservableProperty] private CollectionViewSource _familyFilterCollection;

        [ObservableProperty] private ObservableCollection<string> _groupNames = [];
        [ObservableProperty] private string _selectedGroupName;
        [ObservableProperty] private string _parameterNameSearchString;

        [ObservableProperty] private ObservableCollection<string> _categoryNames = [];
        [ObservableProperty] private string _selectedCategoryName;

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


        private void OnWindowOpened(object sender, EventArgs e)
        {
            //get shared parameters 
            _externalHandler.Raise((uiApp) =>
            {
                var app = uiApp.Application;
                var definitionFile = app.OpenSharedParameterFile();
                if (definitionFile == null)
                {
                    _logger.LogInformation("NO shared parameter file found in the Project");
                    WindowController.Close<FamilyParameters_Window>();
                    return;
                }
                
                _model = new FamilyParametersModel(definitionFile);
                _model.SetParameters();

                this.GroupNames.Clear();
                this.GroupNames.Add("");

                var parametersData = new List<SharedParameterDataRow>();
                _model.AllParameters
                .ForEach(d => {
                    parametersData.Add(new SharedParameterDataRow()
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
                    Source = parametersData
                };

                this.SearchStringParameterFilterCollection.Filter += GetParameterSearchStringFilter;

            });
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
                        parameterData.ParameterName.Contains(this.ParameterNameSearchString,StringComparison.CurrentCultureIgnoreCase);
                }
            }

        }

        [RelayCommand]
        private void ApplyParameters()
        {
            
        }

        [RelayCommand]
        private void DeleteParameters()
        {
            
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

            string[] rfaFiles = Directory.GetFiles(lastOpenedFolder, "*.rfa");
            if (rfaFiles.Length == 0)
            {
                _logger.LogInformation($"No revit family files found under {lastOpenedFolder}");
                return;
            }

            // do magic
            await SetFamilyData(rfaFiles);
        }

        private async Task SetFamilyData(string[] rfaFiles)
        {
            List<FamilyDataGridRow> data = [];
            await _asyncExternalHandler.RaiseAsync((uiAPp) =>
            {
                var uiDoc = uiAPp.ActiveUIDocument;
                var doc = uiDoc.Document;

                this.CategoryNames.Clear();
                this.CategoryNames.Add("");

                using (Transaction tr = new Transaction(doc, "categoryExtracted"))
                {
                    tr.Start();

                    try
                    {
                        foreach (var rfaFile in rfaFiles)
                        {
                            if (!doc.LoadFamily(rfaFile, out Family fam))
                                continue;

                            data.Add(new FamilyDataGridRow()
                            {
                                IsSelected = false,
                                Family = fam.Name,
                                Category = fam.FamilyCategory.Name
                            });

                            if (!CategoryNames.Contains(fam.FamilyCategory.Name))
                                CategoryNames.Add(fam.Category.Name);

                        }
                    }
                    catch
                    {
                        tr.RollBack();
                    }

                    tr.RollBack();
                }

            });

            this.FamilyFilterCollection = new CollectionViewSource()
            {
                Source = data
            };

            this.FamilyFilterCollection.Filter += GetFamilyCategorySearchFilter;

        }

        private void GetFamilyCategorySearchFilter(object sender, FilterEventArgs e)
        {
            var familyData = (FamilyDataGridRow)e.Item;

            if (this.SelectedCategoryName == null || this.SelectedCategoryName == string.Empty)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = familyData.Category == this.SelectedCategoryName;
            }

        }
    }
}
