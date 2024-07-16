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
        public Dictionary<string, ApartmentValidationData> ApartmentValidationInfo { get; set; }
        public BedroomValidationData BedroomInfo { get; set; }
        public MiscellaneousValidationData AdditionalInfo { get; set; }

        public ApartmentType FindApartmentType(string apartmentTypeName)
        {

            if (apartmentTypeName == this.ApartmentTypes["Studio"])
                return ApartmentType.Studio;
            if (apartmentTypeName == this.ApartmentTypes["OneBedroomOnePerson"])
                return ApartmentType.One_Bedroom_1_Person;
            if (apartmentTypeName == this.ApartmentTypes["OneBedroomTwoPerson"])
                return ApartmentType.One_Bedroom_2_Person;
            if (apartmentTypeName == this.ApartmentTypes["TwoBedroomThreePerson"])
                return ApartmentType.Two_Bedroom_3_Person;
            if (apartmentTypeName == this.ApartmentTypes["TwoBedroomFourPerson"])
                return ApartmentType.Two_Bedroom_4_Person;
            if (apartmentTypeName == this.ApartmentTypes["ThreeBedroomFivePerson"])
                return ApartmentType.Three_Bedroom_5_Person;
            if(apartmentTypeName == this.ApartmentTypes["ThreeBedroomSixPerson"])
                return ApartmentType.Three_Bedroom_6_Person;

            return ApartmentType.None;
        }

        public ApartmentValidationData GetStandardsForApartment(string apartmentTypeName) => this.ApartmentValidationInfo[apartmentTypeName];

        public ApartmentValidationData GetStandardsForApartment(ApartmentType type)
        {
            ApartmentValidationData data = null;
            switch (type)
            {
                case ApartmentType.Studio:
                    data = this.ApartmentValidationInfo[ApartmentValidationConstants.Studio_Name];
                    break;
                case ApartmentType.One_Bedroom_1_Person:
                    data = this.ApartmentValidationInfo[ApartmentValidationConstants.OneBedRoomOnePerson_Name];
                    break;
                case ApartmentType.One_Bedroom_2_Person:
                    data = this.ApartmentValidationInfo[ApartmentValidationConstants.OneBedRoomTwoPerson_Name];
                    break;
                case ApartmentType.Two_Bedroom_3_Person:
                    data = this.ApartmentValidationInfo[ApartmentValidationConstants.TwoBedroomThreePerson_Name];
                    break;
                case ApartmentType.Two_Bedroom_4_Person:
                    data = this.ApartmentValidationInfo[ApartmentValidationConstants.TwoBedroomFourPerson_Name];
                    break;
                case ApartmentType.Three_Bedroom_5_Person:
                    data = this.ApartmentValidationInfo[ApartmentValidationConstants.ThreeBedroomFivePerson_Name];
                    break;
                case ApartmentType.Three_Bedroom_6_Person:
                    data = this.ApartmentValidationInfo[ApartmentValidationConstants.ThreeBedroomSixPerson_Name];
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

            string userFolder = LocalDirectoryManager.UserProfileFolder;
#if DEBUG
            settingsPath = @$"{userFolder}\source\repos\CWOArchitects\CWO_App\source\CWO_App.UI\Resources\Standards\ApartmentStandards.json";
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

        public double GetBedroomAreaThreshold()
        {
            
            double t = (this.BedroomInfo.SingleBedMinimumArea + this.BedroomInfo.DoubleBedMinimumArea) / 2;
            return t;
        }


        public double GetTwoBedRoomApartmentAreaThreshold()
        {
            double t = (this.ApartmentValidationInfo[ApartmentValidationConstants.TwoBedroomThreePerson_Name].MinimumFloorArea +
                this.ApartmentValidationInfo[ApartmentValidationConstants.TwoBedroomFourPerson_Name].MinimumFloorArea) / 2;
            return t;
        }
    }

    public class ParameterNames
    {

        public string ApartmentType { get; set; }
        public string RoomType { get; set; }
    }

    public class ApartmentValidationData
    {
        public double MinimumFloorArea { get; set; }
       // public int AdditionalPercentageAllowed { get; set; }
        public double MinimumLivingDinningKitchenWidth { get; set; }
        public double MinimumLivingDinningKitchenArea { get; set; }
        public List<double> MinimumAggregateBedroomAreas { get; set; }
        public List<double> MinimumBedroomWidths { get; set; }
        public double MinimumStorageArea { get; set; }
        public double MinimumBalconyArea { get; set; }
        public double MinimumBalconyWidth { get; set; }
        public double EnclosedKitchenArea { get; set; }

        public ApartmentValidationData()
        {
            //AdditionalPercentageAllowed = 10;
            MinimumAggregateBedroomAreas = [];
        }
    }

    public class BedroomValidationData
    {
        public double SingleBedMinimumArea { get; set; }
        public double SingleBedMinimumWidth { get; set; } 

        public double DoubleBedMinimumArea { get; set; }

        public double DoubleBedMinimumWidth { get; set; }

    }

    public class MiscellaneousValidationData
    {
        public double AdditionalApartmentAreaPercentage { get; set; }

        public double MaxStoreRoomAreaAllowed { get; set; }
    }
}
