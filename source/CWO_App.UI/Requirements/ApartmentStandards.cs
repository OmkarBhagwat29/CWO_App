using CWO_App.UI.Constants;
using Microsoft.VisualBasic;
using RevitCore.ResidentialApartments;
using RevitCore.Utils;
using System.Collections.Generic;
using System.IO;
using System.Security.RightsManagement;


namespace CWO_App.UI.Requirements
{

    public class ApartmentStandards
    {
        public ParameterNames ParameterNames { get; set; }
        public Dictionary<string, string> ApartmentTypes { get; set; }
        public Dictionary<string, string> RoomTypes { get; set; }
        public Dictionary<string, AreaWidthValidationData> ValidationInfo { get; set; }


        public ApartmentType FindApartmentType(string apartmentTypeName)
        {

            if (apartmentTypeName == this.ApartmentTypes["Studio"])
                return ApartmentType.Studio;
            if (apartmentTypeName == this.ApartmentTypes["OneBedroom"])
                return ApartmentType.One_Bedroom;
            if (apartmentTypeName == this.ApartmentTypes["TwoBedroomThreePerson"])
                return ApartmentType.Two_Bedroom_3_Person;
            if (apartmentTypeName == this.ApartmentTypes["TwoBedroomFourPerson"])
                return ApartmentType.Two_Bedroom_4_Person;
            if (apartmentTypeName == this.ApartmentTypes["ThreeBedroom"])
                return ApartmentType.Three_Bedroom;

            return ApartmentType.None;
        }

        public AreaWidthValidationData GetStandardsForApartment(string apartmentTypeName) => this.ValidationInfo[apartmentTypeName];

        public AreaWidthValidationData GetStandardsForApartment(ApartmentType type)
        {
            AreaWidthValidationData data = null;
            switch (type)
            {
                case ApartmentType.Studio:
                    data = this.ValidationInfo[ApartmentValidationConstants.Studio_Name];
                    break;
                case ApartmentType.One_Bedroom:
                    data = this.ValidationInfo[ApartmentValidationConstants.OneBedRoom_Name];
                    break;
                case ApartmentType.Two_Bedroom_3_Person:
                    data = this.ValidationInfo[ApartmentValidationConstants.TwoBedroomThreePerson_Name];
                    break;
                case ApartmentType.Two_Bedroom_4_Person:
                    data = this.ValidationInfo[ApartmentValidationConstants.TwoBedroomFourPerson_Name];
                    break;
                case ApartmentType.Three_Bedroom:
                    data = this.ValidationInfo[ApartmentValidationConstants.ThreeBedroom_Name];
                    break;
                case ApartmentType.None:
                    break;
            }

            return data;
        }

        private static ApartmentStandards FromJsonFile()
        {
            string assemblyDir = LocalDirectoryManager.AssemblyDirectory;
            string settingsPath = Path.Combine(assemblyDir, FileConstants.ApartmentStandardsJsonFile);

#if DEBUG
            settingsPath = @"C:\Users\Om\source\repos\CWOArchitects\CWO_App\source\CWO_App.UI\Resources\Standards\ApartmentStandards.json";
#endif

            string settingsStr = File.ReadAllText(settingsPath);
            return JsonUtils.FromJsonTo<ApartmentStandards>(settingsStr);
        }

        public static ApartmentStandards LoadFromJson()
        {
            try
            {
                return FromJsonFile();
            }
            catch
            {
                return null;
            }
        }
    }

    public class ParameterNames
    {

        public string ApartmentType { get; set; }
        public string RoomType { get; set; }
    }

    public class AreaWidthValidationData
    {
        public double MinimumFloorArea { get; set; }
        public int AdditionalPercentageAllowed { get; set; }
        public double MinimumLivingDinningKitchenWidth { get; set; }
        public double MinimumLivingDinningKitchenArea { get; set; }
        public List<double> MinimumAggregateBedroomAreas { get; set; }
        public double MinimumBedroomWidth { get; set; }
        public double MinimumStorageArea { get; set; }
        public double MinimumBalconyArea { get; set; }
        public double MinimumBalconyWidth { get; set; }
        public double EnclosedKitchenArea { get; set; }

        public AreaWidthValidationData()
        {
            AdditionalPercentageAllowed = 10;
            MinimumAggregateBedroomAreas = [];
        }
    }
}
