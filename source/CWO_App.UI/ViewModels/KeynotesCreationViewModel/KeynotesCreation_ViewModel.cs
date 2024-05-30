using Autodesk.Revit.UI;
using CWO_App.UI.Models.Keynotes;
using CWO_App.UI.Services;
using CWO_App.UI.ViewModels.SharedParametersViewModels;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External.Handlers;
using RevitCore.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


#if NET8_0_OR_GREATER
using Microsoft.Win32;
#else
using System.Windows.Forms;
#endif

namespace CWO_App.UI.ViewModels.KeynotesCreationViewModel
{
    public sealed partial class KeynotesCreation_ViewModel : ObservableObject
    {
        private readonly ILogger _logger;
        public IWindowService WindowService;

        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private KeynotesCreationModel _model;

        [ObservableProperty] private FamilyInfo_ViewModel _selectedFamily;

        [ObservableProperty] private CollectionViewSource _familyCollection;
        [ObservableProperty] private ObservableCollection<FamilyInfo_ViewModel> _familyData = [];

        [ObservableProperty] private ObservableCollection<string> _categoryNames = [];
        [ObservableProperty] private string _selectedCategoryName;

        [ObservableProperty] private ObservableCollection<string> _familyNames = [];
        [ObservableProperty] private string _selectedFamilyName;


        [ObservableProperty] private string _searchKeynoteText;
        [ObservableProperty] private ObservableCollection<CategorizedKeynoteViewModel> _keynotes = [];
        [ObservableProperty] private ObservableCollection<CategorizedKeynoteViewModel> _filteredKeynotes = [];

        private List<CategorizedKeynoteViewModel> _flattenKeynotes = [];

        [ObservableProperty] private string _keynoteFileName;

        partial void OnSearchKeynoteTextChanging(string oldValue, string newValue)
        {
            this.FilterKeynotes(newValue);
        }

        partial void OnSelectedFamilyNameChanged(string oldValue, string newValue)
        {
            if (this.FamilyCollection == null) return;
                this.FamilyCollection.View.Refresh();
        }


        public KeynotesCreation_ViewModel()
        {
            _model = new();

            Task.Run(async () =>
            {
                await _asyncExternalHandler.RaiseAsync((uiApp) => {

                    var doc = uiApp.ActiveUIDocument.Document;

                    var name = Path.GetFileNameWithoutExtension(doc.PathName);

                    this.KeynoteFileName = name;
                
                });
            });
        }

        public void OnKeynoteDoubleClicked(CategorizedKeynoteViewModel keynote)
        {
            if (this.SelectedFamily == null)
                return;

            if (this.FamilyData == null)
                return;

            this.SelectedFamily.KeynoteCode = keynote.Category;
        }

        private void FilterKeynotes(string searchString)
        {
            this.FilteredKeynotes.Clear();
            if (string.IsNullOrEmpty(searchString))
            {
                foreach (var item in this.Keynotes)
                {
                    this.FilteredKeynotes.Add(item);
                }
            }
            else
            {
                //var filtered = FilterKeynotesRecursive(this.Keynotes, this.SearchKeynoteText);

                //var flattenedKeynotes = FlattenKeynotes(this.Keynotes);
                var filtered = _flattenKeynotes
                    .Where(k => ((k.Category.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    || k.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)) &&
                    !k.Description.Contains("(UNICLASS)", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                this.FilteredKeynotes.Clear();

                foreach (var keynote in filtered)
                {
                    this.FilteredKeynotes.Add(keynote);
                }
            }
        }

        private List<CategorizedKeynoteViewModel> FilterKeynotesRecursive(IEnumerable<CategorizedKeynoteViewModel> keynotes, string searchText)
        {
            var result = new List<CategorizedKeynoteViewModel>();

            foreach (var keynote in keynotes)
            {
                var filteredChildren = FilterKeynotesRecursive(keynote.Keynotes, this.SearchKeynoteText);

                if (keynote.Category.Contains(this.SearchKeynoteText, StringComparison.CurrentCultureIgnoreCase) ||
                    keynote.Description.Contains(this.SearchKeynoteText, StringComparison.CurrentCultureIgnoreCase) ||
                    filteredChildren.Any())
                {
                    var newKeynote = new CategorizedKeynoteViewModel
                    {
                        Category = keynote.Category,
                        Description = keynote.Description,
                        Keynotes = new ObservableCollection<CategorizedKeynoteViewModel>(filteredChildren)
                    };
                    result.Add(newKeynote);
                }
            }

            return result;
        }

        private IEnumerable<CategorizedKeynoteViewModel> FlattenKeynotes(IEnumerable<CategorizedKeynoteViewModel> keynotes)
        {
            var result = new List<CategorizedKeynoteViewModel>();

            foreach (var keynote in keynotes)
            {
                result.Add(new CategorizedKeynoteViewModel()
                {
                    Category = keynote.Category,
                    Description = keynote.Description
                });
                if (keynote.Keynotes != null && keynote.Keynotes.Any())
                {
                    result.AddRange(FlattenKeynotes(keynote.Keynotes));
                }
            }

            return result;
        }


        partial void OnSelectedCategoryNameChanged(string oldValue, string newValue)
        {
            this.FamilyNames.Clear();


            this.FamilyData.Where(f => f.CategoryName == newValue)
                .GroupBy(f => f.FamilyName)
                .ToList()
                .ForEach(f => this.FamilyNames.Add(f.Key));

            this.SelectedFamilyName = this.FamilyNames.FirstOrDefault();
        }

        [RelayCommand]
        public void SelectUniclassExcelFile()
        {
#if NET8_0_OR_GREATER
            // Create an instance of the OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the filter to allow only Excel files
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            openFileDialog.Title = "Select Uniclass Excel File";

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string excelFile = openFileDialog.FileName;

                // Set the file path in the model
                _model.Set_UniclassExcelFile(excelFile);
            }
#else
      System.Windows.Forms.OpenFileDialog openFileDialog = new();

            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            openFileDialog.Title = "Select Uniclass Excel File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelFile = openFileDialog.FileName;

                _model.Set_UniclassExcelFile(excelFile);
            }
#endif
        }

        [RelayCommand]
        public void SelectSpecificationExcelFile()
        {
#if NET8_0_OR_GREATER
            // Create an instance of the OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the filter to allow only Excel files
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            openFileDialog.Title = "Select Specification Excel File";

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string excelFile = openFileDialog.FileName;

                // Set the file path in the model
                _model.Set_SpecificationExcelFile(excelFile);
            }
#else
            System.Windows.Forms.OpenFileDialog openFileDialog = new();

            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            openFileDialog.Title = "Select Uniclass Excel File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelFile = openFileDialog.FileName;

                _model.Set_SpecificationExcelFile(excelFile);
            }
#endif
        }

        [RelayCommand]
        public void SelectKeynoteFolder()
        {
#if NET8_0_OR_GREATER
            // Create a new instance of FolderBrowserDialog
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.InitialDirectory = "lastOpenedFolder";


            if ((bool)dialog.ShowDialog())
            {
                _model.Set_KeynoteFileFolderPath(dialog.FolderName);
            }
#else
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        // Show the dialog and capture the result
        DialogResult result = folderBrowserDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            // Get the selected folder path
            _model.Set_KeynoteFileFolderPath(folderBrowserDialog.SelectedPath);
        }
#endif
        }

        [RelayCommand]
        public void CreateKeynoteFile()
        {

            try
            {
                if (this.KeynoteFileName == null || this.KeynoteFileName == string.Empty)
                {
                    TaskDialog.Show("Message", $"Keynote File Name not given");
                    return;
                }

                _model.Set_KeynoteFileName(this.KeynoteFileName);

                var file = _model.CreateKeynoteFile();

                if (File.Exists(file))
                {
                    TaskDialog.Show("Message", $"Keynote File Created => {_model.GetKeynoteFilePath()}");
                }
                else
                {
                    TaskDialog.Show("Message", $"Unable to Create Keynote File, Check the inputs");
                }

            }
            catch (Exception)
            {
                TaskDialog.Show("Message", $"Unable to Create Keynote File, Check the inputs");
            }
        }

        [RelayCommand]
        public void LoadKeynoteFile()
        {

            try
            {
                if (!File.Exists(_model.GetKeynoteFilePath()))
                    return;

                LoadTreeNodes(_model.KeynoteLines);
                _externalHandler.Raise(uiApp =>
                {
                    var doc = uiApp.ActiveUIDocument.Document;

                    doc.UseTransaction(() => doc.LoadKeynoteFile(_model.GetKeynoteFilePath()), "Keynote file loaded");

                    this.SetFamilyTypeData(doc);
                });

            }
            catch (Exception)
            {

            }

        }

        [RelayCommand]
        public async Task  ApplyKeynotes()
        { 
            var famKeynotes = this.FamilyData.Where(f => f.KeynoteCode != null && f.KeynoteCode != string.Empty).ToList();

            if (famKeynotes.Count == 0)
                return;
            try
            {

                await _asyncExternalHandler.RaiseAsync((uiApp) => {

                    var doc = uiApp.ActiveUIDocument.Document;

                    doc.UseTransaction(() => {

                        List<string> messages = [];
                        foreach (var fam in famKeynotes)
                        {
                            var ele = doc.GetElement(fam.ElementId) as ElementType;

                            if (ele == null)
                                continue;

                            var para = ele.LookupParameter("Keynote");

                            if (para == null)
                                continue;

                            bool success = para.Set(fam.KeynoteCode);

                            if (!success)
                            {
                                messages.Add($"Category: {fam.CategoryName}\n" +
                                    $"Family Name: {fam.FamilyName}\n" +
                                    $"Family Type Name: {fam.FamilyTypeName}\n\n");
                            }
                        }

                        if (messages.Count > 0)
                        {
                            messages.Insert(0, "Assigning Keynote failed for Following Families:\n");
                            TaskDialog.Show("Message", string.Join("\n", messages));
                        }
                        else
                        {
                            TaskDialog.Show("Message", "Keynotes Applied Successfully!!!");
                        }

                    }, "Keynotes Assigned");

                });
            }
            catch
            {
               

            }

        }

        [RelayCommand]
        public void ClearKeynotes()
        {
            foreach (var f in this.FamilyData)
            {
                f.KeynoteCode = string.Empty;
            }
        }

        private void SetFamilyTypeData(Document doc)
        {
            this.FamilyData.Clear();
            this.CategoryNames.Clear();
            this.FamilyNames.Clear();

            var types = doc.GetElementsByType<ElementType>((e)=>e.Category!=null)
                .ToList();
            
            foreach (var eleType in types)
            {
                var categoryName = eleType.Category.Name;

                var c = new FamilyInfo_ViewModel
                {
                    CategoryName = categoryName,
                    FamilyName = eleType.FamilyName,
                    FamilyTypeName = eleType.Name,
                    ElementId = eleType.Id,
                };

                this.FamilyData.Add(c);

                if (!this.FamilyNames.Contains(eleType.FamilyName))
                    this.FamilyNames.Add(eleType.FamilyName);

                if(!this.CategoryNames.Contains(categoryName))
                    this.CategoryNames.Add(categoryName);
            }

            this.CategoryNames.Insert(0, string.Empty);
            this.FamilyNames.Insert(0, string.Empty);

            this.SelectedCategoryName = string.Empty;
            this.SelectedFamilyName = string.Empty;

            var cNames = this.CategoryNames.OrderBy(c => c).ToList();

            this.CategoryNames.Clear();

            cNames.ForEach(cN => this.CategoryNames.Add(cN));

            this.FamilyCollection = new CollectionViewSource()
            {
                Source = this.FamilyData
            };

            this.FamilyCollection.Filter += FamilyCollection_Filter;

            this.FamilyCollection.View.Refresh();
        }

        private void FamilyCollection_Filter(object sender, FilterEventArgs e)
        {
            var familyData = (FamilyInfo_ViewModel)e.Item;

            if (this.SelectedCategoryName == null || this.SelectedCategoryName == string.Empty || this.SelectedFamilyName == string.Empty)
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = familyData.CategoryName.Contains(this.SelectedCategoryName, StringComparison.CurrentCultureIgnoreCase) &&
                    familyData.FamilyName.Contains(this.SelectedFamilyName, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private void LoadTreeNodes(List<string> keynoteLines)
        {

            this.Keynotes.Clear();
            this.FilteredKeynotes.Clear();

            Dictionary<string, CategorizedKeynoteViewModel> items = [];

            // First, create all nodes
            foreach (string line in keynoteLines)
            {
                string[] parts = line.Split('\t');
                if (parts.Length < 2) continue;

                string key = parts[0];
                string text = $" {parts[1]}";

                CategorizedKeynoteViewModel newItem = new CategorizedKeynoteViewModel { Category = key, Description = text };
                items[key] = newItem;
            }

            // Then, assign children to parents
            foreach (var item in items)
            {
                string key = item.Key;
                CategorizedKeynoteViewModel node = item.Value;

                int lastUnderscoreIndex = key.LastIndexOf('_');

                if (lastUnderscoreIndex == -1)
                {
                    lastUnderscoreIndex = key.LastIndexOf("/");
                }

                if (lastUnderscoreIndex != -1)
                {
                    string parentKey = key.Substring(0, lastUnderscoreIndex);
                    if (items.ContainsKey(parentKey))
                    {
                        items[parentKey].Keynotes.Add(node);
                    }
                    else
                    {
                        this.Keynotes.Add(node);
                    }
                }
                else
                {
                    this.Keynotes.Add(node);
                }
            }

            foreach (var item in this.Keynotes)
            {
                this.FilteredKeynotes.Add(item);
            }

            _flattenKeynotes = FlattenKeynotes(this.Keynotes).ToList();
        }
    }
}
