using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.Constants
{
    public class KeynoteConstants
    {
        public const int UniClassCodeColumnIndex = 1;
        public const int UniClassDescriptionColumnIndex = 7;
        public const int UniClassStartRowIndex = 4;
        public const string UniClassKeyWord = "UNICLASS";

        public static List<string> UniClassSheetNames = ["Ac", "Ss", "Pr"];

        public const int SpecCodeColumnIndex = 1;
        public const int SpecPrefixColumnIndex = 2;
        public const int SpecDescriptionColumnIndex = 3;
        public const int SpecSuffixColumnIndex = 4;
        public const int SpecStartRowIndex = 3;
    }
}
