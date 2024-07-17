
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.ApartmentParameters
{
    public partial class CategorizedApartmentViewModel : ObservableObject
    {

        [ObservableProperty] string _name;
        [ObservableProperty] ElementId _elementId;
        [ObservableProperty] object _parent = new();
        [ObservableProperty] ObservableCollection<CategorizedApartmentViewModel> _children = [];
    }
}
