using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.ApartmentParameters
{
    public partial class ApartmentRoomViewModel : ObservableObject
    {
        [ObservableProperty] string _name;
        [ObservableProperty] ElementId _elementId;
    }
}
