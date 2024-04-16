
using RevitCore.Extensions.Definition;

namespace CWO_App.UI.Models
{
    public class FamilyParametersModel
    {
        public List<(string groupName, string parameterName)> AllParameters { get;private set; }
        = new List<(string groupName, string parameterName)>();

        public DefinitionFile SharedDefinitionFile { get; set; }
        public FamilyParametersModel(DefinitionFile _sharedDefinitionFile)
        {
            SharedDefinitionFile = _sharedDefinitionFile;
        }

        public void SetParameters()
        {
          this.AllParameters = this.SharedDefinitionFile
                .GetAllParametersFromFile().SelectMany(d=>d).ToList();
        }
    }
}
