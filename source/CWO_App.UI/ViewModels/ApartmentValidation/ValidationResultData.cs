using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.ViewModels.ApartmentValidation
{
    public sealed partial class ValidationResultData : ObservableObject
    {
        [ObservableProperty] private string _apartmentType;
        [ObservableProperty] private double _apartmentMinimumArea;
        
        [ObservableProperty] private double _minimumWidth;
        
        [ObservableProperty] private double _minimumKLDArea;
        [ObservableProperty] private double _minimumKLDWidth;
        
        [ObservableProperty] private double _minimumBedroomArea;
        [ObservableProperty] private double _minimumBedroomWidth;

        [ObservableProperty] private double _minimumBedroom_1_Area;
        [ObservableProperty] private double _minimumBedroom_2_Area;
        [ObservableProperty] private double _minimumBedroom_3_Area;

        [ObservableProperty] private double _minimumBedroom_1_Width;
        [ObservableProperty] private double _minimumBedroom_2_Width;
        [ObservableProperty] private double _minimumBedroom_3_Width;

        [ObservableProperty] private double _minimumStorageArea;
    }
}
