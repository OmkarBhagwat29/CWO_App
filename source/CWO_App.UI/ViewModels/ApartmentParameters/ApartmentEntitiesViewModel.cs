using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.ApartmentParameters
{
    public partial class ApartmentEntitiesViewModel : ObservableObject
    {
        [ObservableProperty] ObservableCollection<ApartmentRoomViewModel> _rooms = [];
        [ObservableProperty] ObservableCollection<ApartmentDoorVM> _doors = [];
        [ObservableProperty] ObservableCollection<ApartmentWindowVM> _windows = [];
        [ObservableProperty] ObservableCollection<ApartmentGenericVM> _generics = [];
        [ObservableProperty] ObservableCollection<ApartmentCaseworkVM> _caseworks = [];
    }
}
