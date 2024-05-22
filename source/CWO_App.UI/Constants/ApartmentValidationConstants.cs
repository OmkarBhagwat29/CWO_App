

namespace CWO_App.UI.Constants
{
    public class ApartmentValidationConstants
    {
        public const string Studio_Name = "STUDIO";
        public const string OneBedRoom_Name = "1 BED";
        public const string TwoBedroomThreePerson_Name = "2 BED";
        public const string TwoBedroomFourPerson_Name = "2 BED(4)";
        public const string ThreeBedroom_Name = "3 BED";

        public const string SharedParameterGroupName = "01. Apartments";
        public const string CWO_APARTMENTS_TYPE = "Dwelling Type";

        public const string CWO_APARTMENTS_NUMBER = "CWO_APARTMENTS_NUMBER";
        public const string CWO_APARTMENTS_MIN_AREA = "CWO_APARTMENTS_MIN_AREA";
        public const string AreaWith10Percentage_ParamName = "AreaWith10Percentage_ParamName";
        public const string CWO_APARTMENTS_BEDS = "CWO_APARTMENTS_BEDS";
        public const string CWO_APARTMENTS_PERSON = "CWO_APARTMENTS_PERSON";

        public const string CWO_APARTMENTS_PROP_BED_AREA = "CWO_APARTMENTS_PROP_BED_AREA";
        public const string CWO_APARTMENTS_MIN_BED_AREA = "CWO_APARTMENTS_MIN_BED_AREA";

        public const string CWO_APARTMENTS_PROP_STORE_AREA = "CWO_APARTMENTS_PROP_STORE_AREA";
        public const string CWO_APARTMENTS_MIN_STORE_AREA = "CWO_APARTMENTS_MIN_STORE_AREA";

        public const string CWO_APARTMENTS_ABOVE_TEN_PERC = "CWO_APARTMENTS_ABOVE_TEN_PERC";

        public static readonly List<string> RequiredApartmentValidationParamNames = [

            CWO_APARTMENTS_MIN_AREA,
CWO_APARTMENTS_BEDS,
CWO_APARTMENTS_PERSON,
CWO_APARTMENTS_PROP_BED_AREA,
CWO_APARTMENTS_MIN_BED_AREA,
CWO_APARTMENTS_PROP_STORE_AREA,
CWO_APARTMENTS_MIN_STORE_AREA,
CWO_APARTMENTS_ABOVE_TEN_PERC,
CWO_APARTMENTS_TYPE,
CWO_APARTMENTS_NUMBER,

            ];
    }
}
