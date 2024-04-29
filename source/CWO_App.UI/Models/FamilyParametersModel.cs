
using Autodesk.Revit.UI;
using CWO_App.UI.ViewModels.SharedParametersViewModels;
using RevitCore.Extensions;
using RevitCore.Extensions.DefinitionExt;
using RevitCore.Extensions.FamilyHelpers;
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

        public bool OverwriteParameterValuesOnLoad { get; set; }

        public bool SaveFamilyFile { get; set; }


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
            if (parameterGroupTypeId == null)
                throw new ArgumentNullException("Parameter group Invalid. Can not continue");

            this.ParameterGroupId = parameterGroupTypeId;

            this.Definitions.Clear();
            var selectedParameterData = selectedRows.GroupBy(d => d.SharedGroup);
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

        public bool ApplySharedParameters(Document doc)
        {
            foreach (var f in LoadedFamilies)
            {
                var familyDocument = doc.EditFamily(f) ?? throw new ArgumentNullException($"family {f.Name} failed to edit");

                using Transaction t = new Transaction(familyDocument, "Shared parameters added");
                try
                {
                    t.Start();

                    familyDocument.TryAddSharedParametersToFamily(this.Definitions, this.ParameterGroupId);

                    t.Commit();

                    familyDocument.LoadFamily(doc, new LoadFamilyOptions(this.OverwriteParameterValuesOnLoad));
                    
                    familyDocument.Close(this.SaveFamilyFile);
                    
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException e)
                {
                    t.RollBack();
                    TaskDialog.Show("Error", e.Message);
                    return false;
                }
                catch (Exception e)
                {
                    t.RollBack();
                    TaskDialog.Show("Error", "An error occurred: " + e.Message);
                    return false;
                }
            }

            return true;
        }


        public bool ApplySharedParameters_V2(Document doc)
        {
            using (var transaction = new Transaction(doc, "Apply Shared Parameters"))
            {
                try
                {
                    transaction.Start();

                    foreach (var f in LoadedFamilies)
                    {
                        var familyDocument = doc.EditFamily(f) ?? throw new ArgumentNullException($"Family {f.Name} failed to edit");

                        familyDocument.TryAddSharedParametersToFamily(this.Definitions, this.ParameterGroupId);

                        if (this.SaveFamilyFile)
                        {
                            familyDocument.Save(new SaveOptions());


                            string familyPath = familyDocument.PathName;
                            string fileName = Path.GetFileNameWithoutExtension(familyPath);

                            var versionFileName = fileName + ".0001.rfa";
                            var versionFilePath = Path.Combine(Path.GetDirectoryName(familyPath), versionFileName);
                            if (File.Exists(versionFilePath))
                                File.Delete(versionFilePath);
                        }

                        familyDocument.LoadFamily(doc, new LoadFamilyOptions(this.OverwriteParameterValuesOnLoad));
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException e)
                {
                    transaction.RollBack();
                    TaskDialog.Show("Error", e.Message);
                    return false;
                }
                catch (Exception e)
                {
                    transaction.RollBack();
                    TaskDialog.Show("Error", "An error occurred: " + e.Message);
                    return false;
                }
            }
        }

        public bool DeleteSharedParameters(Document doc, bool deleteFromProject, bool deleteFromFamilies)
        {
            bool fromProject, fromFamilies;
            if (deleteFromProject)
            {
                fromProject = this.DeleteParametersFromProject(doc);

                if (!fromProject)
                    return false;
            }


            if (deleteFromFamilies)
            {
                fromFamilies = this.DeleteParametersFromFamilies(doc);

                if(!fromFamilies)
                    return false;
            }

            return true;
            
        }

        private bool DeleteParametersFromFamilies(Document doc)
        {
            foreach (var f in LoadedFamilies)
            {
                var familyDocument = doc.EditFamily(f) ?? throw new ArgumentNullException($"family {f.Name} failed to edit");

                using Transaction t = new Transaction(familyDocument, "Shared Parameters deleted");
                try
                {
                    t.Start();

                    familyDocument.DeleteSharedParametersFromFamily(this.Definitions.Select(d => d.definition).ToList());

                    t.Commit();

                    familyDocument.LoadFamily(doc, new LoadFamilyOptions(this.OverwriteParameterValuesOnLoad));

                    familyDocument.Close(this.SaveFamilyFile);
                }
                catch
                {
                    t.RollBack();
                    TaskDialog.Show("Error", $"Shared parameters failed to delete from Family: {f.Name}\n " +
                                           $"operation will not continue for other selected families");
                    return false;
                }

            }

            return true;
        }


        private bool DeleteParametersFromProject(Document doc)
        {

            var instances = doc.GetElementsByInstances<FamilyInstance>();

            try
            {
                doc.UseTransaction(() =>
                {
                    foreach (var item in this.Definitions)
                    {
                        var def = item.definition;

                        if (def is not ExternalDefinition eD)
                            continue;

                        foreach (var elm in instances)
                        {

                            var param = elm.get_Parameter(eD.GUID);

                            if (param == null)
                                continue;

                            doc.Delete(param.Id);

                            break;
                        }
                    }

                }, "Delete shared parameters from project");
            }
            catch
            {
                TaskDialog.Show("Error", $"Shared parameters failed to delete from Project");
                return false;
            }

            return true;
        }
    }
}