
using CWO_App.UI.ViewModels.SharedParametersViewModels;
using RevitCore.Extensions;
using RevitCore.Extensions.DefinitionExt;
using RevitCore.Extensions.Parameters;
using System.IO;

namespace CWO_App.UI.Models
{
    public class FamilyParametersModel
    {
        public List<(string groupName, string parameterName)> AllParameters { get;private set; }
        = new List<(string groupName, string parameterName)>();

        public List<(string FamilyName, string FilePath)> AllFamilies { get; private set; } = [];

        public List<Family> LoadedFamilies { get; private set; } = new List<Family>();

        public List<(Definition definition, bool isInstance)> Definitions { get; private set; } = [];

        public DefinitionFile SharedDefinitionFile { get; set; }

        public ForgeTypeId ParameterGroupId { get; set; }

        public FamilyParametersModel(DefinitionFile _sharedDefinitionFile)
        {
            SharedDefinitionFile = _sharedDefinitionFile;
        }

        public void SetAllParameters()
        {
          this.AllParameters = this.SharedDefinitionFile
                .GetAllParametersFromFile().SelectMany(d=>d).ToList();
        }

        public void SetSelectedExternalDefinitions(List<SharedParameterDataRow> selectedRows, ForgeTypeId parameterGroupTypeId)
        {
            //set parameter group id => forgeTypeID
            this.ParameterGroupId = parameterGroupTypeId;

            this.Definitions.Clear();
            var selectedParameterData = selectedRows.GroupBy(d => d.ParameterGroup);
            foreach (var gD in selectedParameterData)
            {
                var groupName = gD.Key;
                var definitionGroup = this.SharedDefinitionFile.Groups.get_Item(groupName) 
                    ?? throw new ArgumentNullException($"Parameter group not found! Name: {groupName}");

                foreach (var g in gD)
                {
                    var definition = definitionGroup.Definitions.get_Item(g.ParameterName)
                        ?? throw new ArgumentNullException($"Parameter Not found!\nParameter Name: {g.ParameterName} \nGroupName: {groupName}");
                    
                    this.Definitions.Add((definition,isInstance:g.IsInstance));
                }
            }
        }

        public void SetAllFamilies(List<string>revitFamilyFiles)
        {
            this.AllFamilies.Clear();
            revitFamilyFiles.ForEach((f) => this.AllFamilies.Add((Path.GetFileNameWithoutExtension(f),f)));
        }

        public void LoadSelectedFamilies(Document doc, List<string> selectedFamilyNames)
        {
            this.LoadedFamilies.Clear();
            foreach (var familyName in selectedFamilyNames)
            {
                var filePath = this.AllFamilies.FirstOrDefault(f=> f.FamilyName == familyName).FilePath;

                if (!File.Exists(filePath))
                    throw new ArgumentNullException($"Family File not found => {filePath}");

                if (!doc.FamilyExists(familyName, out Family fam))
                {
                    if (!doc.LoadFamily(filePath, out fam))
                        throw new ArgumentNullException($"Family can not be loaded, please check inspect the family => {filePath}");
                }
                
                this.LoadedFamilies.Add(fam);
            }
        }

        public void ApplySharedParameters(Document doc)
        {
            foreach (var f in LoadedFamilies)
            {
                f.TryAddSharedParametersToFamily(doc, this.Definitions, this.ParameterGroupId);
            }
        }

        public void DeleteSharedParameters(Document doc)
        {
            foreach (var f in LoadedFamilies)
            {
                f.DeleteSharedParametersFromFamily(doc, this.Definitions.Select(d=>d.definition).ToList());
            }
        }

    }
}
