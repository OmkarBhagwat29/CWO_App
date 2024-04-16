using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.SharedParametersViewModels
{
    public sealed partial class FamilyParameters_ViewModel(ILogger<FamilyParameters_ViewModel> logger) : ObservableObject
    {
        private readonly ActionEventHandler _externalHandler = new();
        private readonly AsyncEventHandler _asyncExternalHandler = new();
        private readonly AsyncEventHandler<ElementId> _asyncIdExternalHandler = new();

        private string lastOpenedFolder = "";

        [ObservableProperty] private ObservableCollection<SharedParameterDataRow> _sharedParametersData = [];
        [ObservableProperty] private ObservableCollection<FamilyDataGridRow> _familiesData = [];

        [ObservableProperty] private string _sharedParameterFilePath;


        [RelayCommand]
        private void SelectSharedParameterFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text file (*.txt)|*.txt";
            openFileDialog.Multiselect = false;

            if (!string.IsNullOrEmpty(lastOpenedFolder))
            {
                openFileDialog.InitialDirectory = lastOpenedFolder;
            }

            bool? result = openFileDialog.ShowDialog();

            if (!(bool)result)
                return;


            // Get the selected file name and store the folder path
            this.SharedParameterFilePath = openFileDialog.FileName;
            lastOpenedFolder = Path.GetDirectoryName(this.SharedParameterFilePath);

            //get shared param info
            
        }

        [RelayCommand]
        private void SelectFamilyFolder()
        {

        }
    }
}
