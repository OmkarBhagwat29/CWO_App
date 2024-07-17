using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.ApartmentParameters
{
    public partial class ApartmentFamiliesViewModel : ObservableObject
    {
        [ObservableProperty] string _familyName;
        [ObservableProperty] string _familyTypeName;
        [ObservableProperty] ElementId _elementId;
    }
}
