
using CWO_App.UI.ViewModels.SharedParametersViewModels;
using RevitCore.Extensions;
using RevitCore.Extensions.Definition;
using RevitCore.Extensions.Parameters;
using System.IO;

namespace CWO_App.UI.Models
{
    public class FamilyParametersModel
    {
        public List<(string groupName, string parameterName)> AllParameters { get;private set; }
        = new List<(string groupName, string parameterName)>();

        public List<(string FamilyName, string FilePath)> AllFamilies { get; private set; } =
            new List<(string FamilyName, string FilePath)>();

        public List<Family> LoadedFamilies { get; private set; } = new List<Family>();

        public List<ExternalDefinition> ExternalDefinitions { get; private set; } = new List<ExternalDefinition>();

        public DefinitionFile SharedDefinitionFile { get; set; }
        public FamilyParametersModel(DefinitionFile _sharedDefinitionFile)
        {
            SharedDefinitionFile = _sharedDefinitionFile;
        }

        public void SetAllParameters()
        {
          this.AllParameters = this.SharedDefinitionFile
                .GetAllParametersFromFile().SelectMany(d=>d).ToList();
        }

        public void SetSelectedExternalDefinitions(List<SharedParameterDataRow> selectedRows)
        {
            this.ExternalDefinitions.Clear();
            var selectedParameterData = selectedRows.GroupBy(d => d.ParameterGroup);
            foreach (var gD in selectedParameterData)
            {
                var groupName = gD.Key;
                var definitionGroup = this.SharedDefinitionFile.Groups.get_Item(groupName);

                if (definitionGroup == null) throw new ArgumentNullException($"Parameter group not found! Name: {groupName}");

                foreach (var g in gD)
                {
                    var definition = definitionGroup.Definitions.get_Item(g.ParameterName) as ExternalDefinition;
                    if (definition == null) throw new ArgumentNullException($"Parameter Not found!\nParameter Name: {g.ParameterName} \nGroupName: {groupName}");

                    this.ExternalDefinitions.Add(definition);
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

        public void ApplySharedParameters(Document doc, ForgeTypeId groupTypeId,
            bool isInstance)
        {
            foreach (var f in LoadedFamilies)
            {
                f.AddSharedParametersToFamily(doc, this.ExternalDefinitions, groupTypeId, isInstance);
            }
        }

    }
}
