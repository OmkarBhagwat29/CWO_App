using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.Constants
{
    public class RoomValidationConstants
    {
        public const string KitchenLivingDinning_Name = "KLD";
        public const string Kitchen_Name = "KITCHEN";
        public const string LivingDinning_Name = "LD";
        public const string StorageRoomName_Name = "STORAGE";
        public const string Bedroom_Name = "BEDROOM";
        public const string Bedroom_1_Name = "BEDROOM 1";
        public const string Bedroom_2_Name = "BEDROOM 2";
        public const string Bedroom_3_Name = "BEDROOM 3";
        public const string Balcony_Name = "BALCONY";

        public const string RoomName_ParamName = "Name";
        public const string CWO_ROOMS_APT_NUM = "CWO_ROOMS_APT_NUM";
        public const string CWO_ROOMS_PROP_WIDTH = "AchWidth";
        public const string CWO_ROOMS_MIN_WIDTH = "ReqWidth";
        //public const string WidthAccuracy_ParamName = "Width Accuracy";

        public const string AchievedArea_ParamName = "Area";
        public const string CWO_ROOMS_MIN_AREA = "ReqArea";
        //public const string AreaDifference_ParamName = "DiffArea";


        public static readonly List<string> RequiredRoomValidationParamNames =
        [   CWO_ROOMS_APT_NUM,
            CWO_ROOMS_MIN_AREA,
            CWO_ROOMS_MIN_WIDTH,
            CWO_ROOMS_PROP_WIDTH];

    }
}
