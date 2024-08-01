

namespace CWO_App.UI.Constants
{
    public class ApartmentValidationConstants
    {
        public const string Studio_Name = "STUDIO";
        public const string OneBedRoomOnePerson_Name = "1 BED(1)";
        public const string OneBedRoomTwoPerson_Name = "1 BED(2)";
        public const string TwoBedroomThreePerson_Name = "2 BED(3)";
        public const string TwoBedroomFourPerson_Name = "2 BED(4)";
        public const string ThreeBedroomFivePerson_Name = "3 BED(5)";
        public const string ThreeBedroomSixPerson_Name = "3 BED(6)";


        public const string SharedParameterGroupName = "01. APARTMENTS";
        public const string CWO_APARTMENTS_TYPE = "CWO_APARTMENTS_TYPE";

        public const string CWO_APARTMENTS_NUMBER = "CWO_APARTMENTS_NUMBER";
        public const string CWO_APARTMENTS_MIN_AREA = "CWO_APARTMENTS_MIN_AREA";
        public const string CWO_APARTMENTS_BEDS = "CWO_APARTMENTS_BEDS";
        public const string CWO_APARTMENTS_PERSON = "CWO_APARTMENTS_PERSON";

        public const string CWO_APARTMENTS_PROP_BED_AREA = "CWO_APARTMENTS_PROP_BED_AREA";
        public const string CWO_APARTMENTS_MIN_BED_AREA = "CWO_APARTMENTS_MIN_BED_AREA";

        public const string CWO_APARTMENTS_PROP_STORE_AREA = "CWO_APARTMENTS_PROP_STORE_AREA";
        public const string CWO_APARTMENTS_MIN_STORE_AREA = "CWO_APARTMENTS_MIN_STORE_AREA";

        public const string CWO_APARTMENTS_ABOVE_TEN_PERC = "CWO_APARTMENTS_ABOVE_TEN_PERC";

        public const string CWO_APARTMENTS_BLOCK = "CWO_APARTMENTS_BLOCK";

        public const string CWO_APARTMENTS_LEVELS = "CWO_APARTMENTS_LEVELS";

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
