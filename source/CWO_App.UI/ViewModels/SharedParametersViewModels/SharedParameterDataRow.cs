using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.SharedParametersViewModels
{
    public sealed partial class SharedParameterDataRow : ObservableObject
    {
        [ObservableProperty] private bool _isSelected;
        [ObservableProperty] private string _parameterGroup;
        [ObservableProperty] private string _parameterName;
        [ObservableProperty] private bool _isInstance;
    }
}
