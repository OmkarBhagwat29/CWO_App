using ClosedXML.Excel;
using CWO_App.UI.Constants;
using DocumentFormat.OpenXml.EMMA;
using RevitCore.Excel;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.Models.Keynotes
{
    public class KeynotesCreationModel
    {
        private string uniClassExcelFile;
        private string specExcelFile;
        private string keynoteFileFolder;
        private string keynoteFileName;

        public List<string> KeynoteLines { get; private set; } = [];

        public KeynotesCreationModel(string uniClassExcelFile, string specExcelFile, string keynoteFileFolder, string keynoteFileName) {
            this.uniClassExcelFile = uniClassExcelFile;
            this.specExcelFile = specExcelFile;
            this.keynoteFileFolder = keynoteFileFolder;
            this.keynoteFileName = keynoteFileName;
        }

        public KeynotesCreationModel()
        {
            
        }

        public string GetKeynoteFilePath() => Path.Combine(this.keynoteFileFolder, this.keynoteFileName + ".txt");

        public void Set_UniclassExcelFile(string excelFile) => this.uniClassExcelFile = excelFile;

        public void Set_SpecificationExcelFile(string specificationFile) => this.specExcelFile = specificationFile;

        public string Set_KeynoteFileFolderPath(string folderPath) => this.keynoteFileFolder = folderPath;

        public string Set_KeynoteFileName(string fileName) => this.keynoteFileName = fileName;  

        private XLWorkbook GetUniClassWorkbook() => new ExcelHelper(uniClassExcelFile).GetWorkbook();

        private XLWorkbook GetSpecificationWorkbook() => new ExcelHelper(specExcelFile).GetWorkbook();

        private IEnumerable<(string code, string description, string subGroup)> GetUniClassCodeAndDescription(XLWorkbook uniClassWorkbook)
        {
            var uniClassSheets = GetUniClassSheets(uniClassWorkbook);

            if(uniClassSheets == null || uniClassSheets.Count == 0)
                throw new ArgumentNullException($"No Sheet found with default names");

            foreach (var sheet in uniClassSheets)
            {
                for (int i = KeynoteConstants.UniClassStartRowIndex; i < sheet.RowCount(); i++)
                {
                    var code = sheet.ReadCell(i, KeynoteConstants.UniClassCodeColumnIndex);
                    var description = sheet.ReadCell(i,KeynoteConstants.UniClassDescriptionColumnIndex);

                    if(code == string.Empty || description == string.Empty)
                        continue;

                    description += $" ({KeynoteConstants.UniClassKeyWord})";

                    //check if code has more than 3 underscores then continue (means it is spec nd not uniclass group)
                    if (code.Count(c => c == '_') > 3)
                        continue;

                    // subGroup
                    int last_Index = code.LastIndexOf('_');
                    if (last_Index == -1)
                        continue;

                    var subGroup = code.Substring(0, last_Index);

                    yield return (code, description, subGroup);
                }
            }
        }

        private IEnumerable<(string code, string prefix, string description, string suffix, string subGroup)>
            GetSpecificationData(XLWorkbook specWorkbook)
        {
            var worksheet = specWorkbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new ArgumentNullException($"Unable to find Specification worksheet: {specExcelFile}");

            

            for (int i = KeynoteConstants.SpecStartRowIndex; i < worksheet.RowCount(); i++)
            {
                var code = worksheet.ReadCell(i,KeynoteConstants.SpecCodeColumnIndex);

                if (code == string.Empty)
                    continue;

                var prefix = worksheet.ReadCell(i,KeynoteConstants.SpecPrefixColumnIndex);
                var description = worksheet.ReadCell(i,KeynoteConstants.SpecDescriptionColumnIndex);
                var suffix = worksheet.ReadCell(i, KeynoteConstants.SpecSuffixColumnIndex);


                // subGroup
                int last_Index = code.LastIndexOf('/');
                if (last_Index == -1)
                {
                    last_Index = code.LastIndexOf('_');

                    if (last_Index == -1)
                        continue;
                }

                var subGroup = code.Substring(0, last_Index);

                yield return (code, prefix, description, suffix, subGroup);
            }


        }

        private static string CombineUniclassCodeData(string code, string description, string subGroup, string delimiter = "\t")
        {
            return $"{code}{delimiter}{description}{delimiter}{subGroup}";
        }

        private static string CombinedSpecData(string code, string description, string subGroup, string suffix = "" )
        {
            if (suffix != "")
                return $"{code} {suffix}\t{description}\t{subGroup}";
            else
                return $"{code}\t{description}\t{subGroup}";
        }

        private List<string> CreateKeynoteFileTexts(XLWorkbook uniClassWorkbook, XLWorkbook specWorkbook)
        {

            var uniClassCodeData = GetUniClassCodeAndDescription(uniClassWorkbook).ToList();

            var specData = GetSpecificationData(specWorkbook).ToList();

            List<string> grouping = [];
            List<string> codeChecker = [];

            for (int i = 0; i < specData.Count; i++)
            {
                var specCode = specData[i].code;

                var last_backSlash_index = specCode.LastIndexOf('/');
                if (last_backSlash_index != -1)
                {
                    specCode = specCode.Substring(0, last_backSlash_index);
                }

                int _count = specCode.Count(c => c == '_');

                int _checker = 0;
                for (int j = 0; j < uniClassCodeData.Count; j++)
                {

                    if (_checker == _count)
                    {
                        _checker = 0;
                        break;
                    }

                    var uniCode = uniClassCodeData[j].code;

                    if (specCode.Contains(uniCode) && _checker != _count)
                    {
                        var data = CombineUniclassCodeData(uniCode, uniClassCodeData[j].description, uniClassCodeData[j].subGroup);
                        if (!codeChecker.Contains(uniCode))
                        {
                            grouping.Add(data);

                            codeChecker.Add(uniCode);

                            _checker++;
                        }
                    }
                }

            }

            foreach (var (code, prefix, description, suffix, subGroup) in specData)
            {
                var specString = CombinedSpecData(code, description, subGroup, suffix);

                grouping.Add(specString);
            }


            grouping.Insert(0,"Ac\tActivities(UNICLASS)");
            grouping.Insert(1,"Ss\tSystems(UNICLASS)");
            grouping.Insert(2,"Pr\tProducts(UNICLASS)");

            return grouping;
        }

        private List<IXLWorksheet> GetUniClassSheets(XLWorkbook uniClassWorkbook)
        {
            return uniClassWorkbook.GetWorksheets().Where(s => KeynoteConstants.UniClassSheetNames.Contains(s.Name)).ToList();
        }

        public string CreateKeynoteFile()
        {
            var uniClassWorkbook = GetUniClassWorkbook();

            if (uniClassWorkbook == null)
                throw new ArgumentNullException($"Unable to open Uniclass workbook: {uniClassWorkbook}");

            var specWorkbook = GetSpecificationWorkbook();
            if (specWorkbook == null) throw new ArgumentNullException($"Unable to open Specification workbook: {specWorkbook}");

            this.KeynoteLines = CreateKeynoteFileTexts(uniClassWorkbook,specWorkbook);

            string fileName = Path.Combine(this.keynoteFileFolder, this.keynoteFileName + ".txt");
            File.WriteAllLines(fileName, this.KeynoteLines);

            return fileName;
        }

        public void Set_KeynoteLines(List<string> keynoteFileTexts)
        {
            this.KeynoteLines = keynoteFileTexts;
        }
    }
}
