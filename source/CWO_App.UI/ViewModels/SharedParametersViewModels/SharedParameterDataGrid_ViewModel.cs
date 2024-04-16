using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.SharedParametersViewModels
{
    public sealed partial class SharedParameterDataGrid_ViewModel : ObservableObject
    {
        [ObservableProperty]private ObservableCollection<SharedParameterDataRow> _data = [];

        [ObservableProperty] private string _sharedParameterFilePath;

        [RelayCommand]
        private void SelectFile()
        {
            
        }
    }
}
