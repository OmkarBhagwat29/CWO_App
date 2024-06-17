
using Autodesk.Revit.UI;
using CWO_App.UI.Constants;
using CWO_App.UI.Requirements;
using Microsoft.Extensions.Logging;
using RevitCore.Extensions;
using RevitCore.Extensions.DefinitionExt;
using RevitCore.Extensions.PointInPoly;
using RevitCore.ResidentialApartments;
using RevitCore.ResidentialApartments.Rooms;
using RevitCore.ResidentialApartments.Validation;
using System.Text;
using System.Windows.Ink;


namespace CWO_App.UI.Models.ApartmentValidation
{
    public class ApartmentValidationModel
    {
        private readonly ILogger _logger;
        public ApartmentValidationModel(ILogger logger, UIApplication uiApp,
            ApartmentStandards _standards)
        {
            UiApp = uiApp;
            Standards = _standards;
            _logger = logger;

            CWO_Apartment.ApartmentAreaThreshold = Standards.GetTwoBedRoomApartmentAreaThreshold();
            CWO_Apartment.BedroomAreaThreshold = Standards.GetBedroomAreaThreshold();
        }

        DefinitionFile _definitionFile;

        private List<AreaRoomAssociation> _associations = [];

        public List<CWO_Apartment> Apartments = [];

        public UIApplication UiApp { get; }
        public ApartmentStandards Standards { get; }

        public void SetDefinitionFile(DefinitionFile defFile)
        {
            this._definitionFile = defFile;
        }

        public void SetAreaRoomAssociation()
        {
            _associations = AreaRoomAssociation
                    .GetCWOApartmentsInProject(this.UiApp.ActiveUIDocument.Document,
            (area) =>
            {
                if (area.Level == null)
                    return false;

                return true;
            });
        }


        /// <summary>
        /// This method also set parameter value
        /// of Apartment Types hence need to call with transaction open
        /// </summary>
        public void SetApartments()
        {
            this.Apartments = this.UiApp
                            .ActiveUIDocument
                            .Document.UseTransaction(() =>
                            {
                                return CWO_Apartment
                                        .CreateApartmentsAndSetApartmentTypeInProject(
                                        this.UiApp.ActiveUIDocument.Document,_logger,
                                        _associations, this.Standards);
                            }, "Apartments Created");
        }

        public void Validate(bool bakeValidationData = false)
        {

            //validate if value for required parameters are set
            this.Apartments.ForEach(ap =>
            {
                ap.Validate();
            }
            );

            //bake
            if (bakeValidationData)
            {
                UiApp.ActiveUIDocument.Document.UseTransaction(() =>
                {
                    this.Apartments.ForEach(ap => ap.Bake(UiApp.ActiveUIDocument.Document));

                }, "Bake Validation Data");
            }
        }

        public void GetFailedValidation(out List<ValidationResult> widthFailedResults,
            out List<ValidationResult> areaFailedResults)
        {
            widthFailedResults = [];
            areaFailedResults = [];

            var tempWVs = new List<ValidationResult>();

            var tempAVs = new List<ValidationResult>();

            this.Apartments.ForEach(apt =>
            {

                var aptNumberParam = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_NUMBER);

                var aptNumber = aptNumberParam?.AsValueString();

                aptNumber ??= apt.Name;

                //apartment validation
                apt.ApartmentValidationData.ForEach(aV =>
                {


                    if (!aV.ValidationSuccess)
                    {
                        string message = aV.GetValidationReport();

                        if (aV is AggregateAreaValidation aaV)
                        {
                            // this for bedrooms
                            StringBuilder sB = new StringBuilder();

                            var bedrooms = apt.Rooms.Where(r => r is Bedroom)
                            .ToList();

                            string info = "Bedroom Data: ";
                            int i = 0;
                            bool added = false;
                            foreach (var bedRm in bedrooms)
                            {
                                if (!aaV.ValidationResults[i])
                                {
                                    string bI = $"[{bedRm.Name}: Achieved Area: {Math.Round(aaV.AchievedAreas[i], 2)}," +
                                    $" Required Area: {aaV.RequiredAreas[i]}]\n";

                                    info += bI;

                                    added = true;
                                }

                                i++;
                            }
                            if (added)
                            {
                                string title = $"Apartment Number: {aptNumber}\n" +
                                $"Apartment Element Id: {apt.AreaBoundary.Id}";
                                sB.AppendLine(title);
                                sB.AppendLine(info);
                                sB.AppendLine(message);

                                tempAVs.Add(new ValidationResult()
                                {
                                    Message = sB.ToString(),
                                    Element_Id = apt.AreaBoundary.Id
                                });
                            }

                        }
                        else if (aV is AreaValidation areaV)
                        {
                            StringBuilder sB = new StringBuilder();

                            string title = $"Apartment Number: {aptNumber}\n" +
                            $"Apartment Element ID: {apt.AreaBoundary.Id}\n" +
                            $"Apartment Achieved Area: {Math.Round(areaV.AchievedArea, 2)}\n" +
                            $"Apartment Required Area: {areaV.RequiredArea}";

                            sB.AppendLine(title);

                            sB.AppendLine(message);

                            ValidationResult wR = new()
                            {
                                Message = sB.ToString(),

                                Element_Id = apt.AreaBoundary.Id
                            };

                            tempAVs.Add(wR);
                        }
                        else if (aV is CombinedAreaValidation cV)
                        {
                            StringBuilder sB = new StringBuilder();
                            // this is for storage
                            string title = $"Apartment Number: {aptNumber}\n" +
                               $"Apartment Element Id: {apt.AreaBoundary.Id}";

                            sB.AppendLine(title);
                            List<RoomBase> rooms = [];
                            if (cV.SpatialType == typeof(Storage))
                            {
                                rooms = apt.Rooms.Where(r => r is Storage).ToList();
                            }
                            else
                            {

                                //its beds
                                rooms = apt.Rooms.Where(r => r is Bedroom).ToList();
                            }

                            for (int i = 0; i < rooms.Count; i++)
                            {
                                var rm = rooms[i];

                                string info = $"{rm.Name}: Achieved Area -> {Math.Round(rm.Room.Area.ToUnit(UnitTypeId.SquareMeters), 2)}";

                                sB.AppendLine(info);
                            }

                            sB.AppendLine($"Achieved Combined Area: {Math.Round(cV.CombinedArea,2)}");
                            sB.AppendLine($"Required Combined Area: {cV.RequiredArea}");

                            sB.AppendLine(message);

                            tempAVs.Add(new ValidationResult()
                            {
                                Message = sB.ToString(),
                                Element_Id = apt.AreaBoundary.Id
                            });
                        }

                    }
                });

                //room validation
                apt.Rooms.ForEach(r =>
                {
                    r.RoomValidationData.ForEach(v => {

                        if (!v.ValidationSuccess)
                        {
                            if (v is DimensionValidation dV)
                            {
                                StringBuilder sB = new StringBuilder();

                                string tilte =
                                $"Apartment Number: {aptNumber}\n" +
                                $"Room Name: {r.Name}\n" +
                                $"Room Element ID: {r.Room.Id}\n" +
                                $"Achieved Width: {Math.Round(dV.AchievedMinWidth, 2)}\n" +
                                $"Required Width: {dV.RequiredMinWidth}";

                                sB.AppendLine(tilte);

                                var message = dV.GetValidationReport();

                                sB.AppendLine(message);
                                ValidationResult wR = new()
                                {
                                    Message = sB.ToString(),

                                    Element_Id = r.Room.Id
                                };

                                tempWVs.Add(wR);
                            }
                            else
                            {
                                //it's area validation for rooms

                                var message = v.GetValidationReport();

                                if (v is AreaValidation aV)
                                {
                                    StringBuilder sB = new StringBuilder();

                                    string title = $"Apartment Number: {aptNumber}\n" +
                                    $"Room Name: {r.Name}\n" +
                                    $"Room Element ID: {r.Room.Id}\n" +
                                    $"Achieved Area: {Math.Round(aV.AchievedArea, 2)}\n" +
                                    $"Required Area: {aV.RequiredArea}";

                                    sB.AppendLine(title);

                                    sB.AppendLine(message);

                                    ValidationResult wR = new()
                                    {
                                        Message = sB.ToString(),

                                        Element_Id = r.Room.Id
                                    };

                                    tempAVs.Add(wR);
                                }

                            }
                        }

                    });
                });
            });

            if (tempWVs.Count > 0)
            {
                widthFailedResults.AddRange(tempWVs);
            }
            else
            {
                // success message
                widthFailedResults.Add(new ValidationResult()
                {
                    Message = "All the apartment rooms in the project are according to the Minimum Width Requirements. Validation Successful!!!"
                });
            }

            if (tempAVs.Count > 0)
            {
                areaFailedResults.AddRange(tempAVs);
            }
            else
            {
                areaFailedResults.Add(new ValidationResult()
                {
                    Message = "All the apartments and rooms in the project are according to the Minimum Area Requirements. Validation Successful!!!"
                });
            }
        }

        public void RunStartToEnd()
        {
            this.SetAreaRoomAssociation();
            this.SetApartments();

           this.Validate(true);
        }

        public bool CheckSharedParametersExists()
        {
            //check apartment params
            
            if (!this._definitionFile.GroupExists(ApartmentValidationConstants.SharedParameterGroupName))
            {
                return false;
            }

            if (!this._definitionFile.GroupExists(RoomValidationConstants.SharedParameterGroupName))
            {
                return false;
            }

            var aptGroup = this._definitionFile.Groups.FirstOrDefault(g => g.Name == ApartmentValidationConstants.SharedParameterGroupName);

            foreach (var pName in ApartmentValidationConstants.RequiredApartmentValidationParamNames)
            {
                var p = aptGroup.GetDefinitionInGroup(pName);
                if (p == null)
                {
                    return false;
                }
            }

            var roomGroup = this._definitionFile.Groups.FirstOrDefault(g => g.Name == RoomValidationConstants.SharedParameterGroupName);
            foreach (var pName in RoomValidationConstants.RequiredRoomValidationParamNames)
            {
                var p = roomGroup.GetDefinitionInGroup(pName);
                if (p == null)
                {
                    return false;
                }
            }

            return true;
        }

        public void CreateProjectParameters()
        {
            var group = this._definitionFile.Groups.First(g => g.Name == ApartmentValidationConstants.SharedParameterGroupName);

            List<ExternalDefinition> extDefAreas = [];
            foreach (var pName in ApartmentValidationConstants.RequiredApartmentValidationParamNames)
            {
               var p = group.GetDefinitionInGroup(pName);

                if (p is ExternalDefinition extDef)
                {
                    extDefAreas.Add(extDef);
                }
            }

            extDefAreas.
            TryAddToDocument(UiApp.ActiveUIDocument.Document,
            new List<BuiltInCategory>() { BuiltInCategory.OST_Areas },
            BindingKind.Instance, GroupTypeId.IdentityData);

            group = this._definitionFile.Groups.First(g => g.Name == RoomValidationConstants.SharedParameterGroupName);

            List<ExternalDefinition> extDefRooms = [];
            foreach (var pName in RoomValidationConstants.RequiredRoomValidationParamNames)
            {
                var p = group.GetDefinitionInGroup(pName);

                if (p is ExternalDefinition extDef)
                {
                    extDefRooms.Add(extDef);
                }
            }

            extDefRooms.
            TryAddToDocument(UiApp.ActiveUIDocument.Document,
            new List<BuiltInCategory>() { BuiltInCategory.OST_Rooms },
            BindingKind.Instance, GroupTypeId.IdentityData);

            //return;
            //set it to group instance
            BindingMap bM = UiApp.ActiveUIDocument.Document.ParameterBindings;
            DefinitionBindingMapIterator it = bM.ForwardIterator();
            it.Reset();
            while (it.MoveNext())
            {
                Definition definition = it.Key;

                if (RoomValidationConstants.RequiredRoomValidationParamNames.Contains(definition.Name) ||
                    ApartmentValidationConstants.RequiredApartmentValidationParamNames.Contains(definition.Name))
                {

                    // TODO:  Verify the GUID matches the one for the shared parameter we just added

                    InternalDefinition internalDef = definition as InternalDefinition;

                    if (internalDef != null)
                    {
                        internalDef.SetAllowVaryBetweenGroups(UiApp.ActiveUIDocument.Document, true);
                    }
                }
            }

        }

        public static void CheckRequiredParametersExists(Element areaElement, Element roomElement,
            out List<string> messages)
        {
            messages = [];

            //check apartment params
            foreach (var pName in ApartmentValidationConstants.RequiredApartmentValidationParamNames)
            {
                if (areaElement.LookupParameter(pName) == null)
                {
                    messages.Add($"Shared Parameter *{pName}* not found for Area Boundary.");
                }
            }

            foreach (var pName in RoomValidationConstants.RequiredRoomValidationParamNames)
            {
                if (roomElement.LookupParameter(pName) == null)
                {
                    messages.Add($"Shared Parameter *{pName}* not found for Room.");
                }

            }

            if (messages.Count > 0)
            {
                messages.Insert(0, $"Validation will not function.\nAdd following Shared Parameters for Area and Room.");
            }
        }

        public void SetApartmentParameters()
        {
            var doc = UiApp.ActiveUIDocument.Document;
            var lengthUnitType = SpecTypeId.Length;
            var areaUnitType = SpecTypeId.Area;
            var desiredLengthDisplayUnits = doc.GetUnits().GetFormatOptions(lengthUnitType).GetUnitTypeId();
            var desiredAreaDisplayUnits = doc.GetUnits().GetFormatOptions(areaUnitType).GetUnitTypeId();


            foreach (var apt in this.Apartments)
            {

                #region Apartment Parameteres
                //This parameter to store the minimum overall apartment floor area required.
                var p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_AREA);
                if (p != null)
                {
                    AreaValidation aV = apt.ApartmentValidationData.Where(v=>v is AreaValidation).FirstOrDefault() as AreaValidation;

                    if (aV != null)
                    {
                        double sqFt = aV.RequiredArea.FromUnit(UnitTypeId.SquareMeters); 
                        bool success = p.Set(sqFt);
                    }

                    //This parameter to store whether or not the apartment area is above 10% of the minimum overall floor area required.
                    p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_ABOVE_TEN_PERC);
                    p?.Set(aV.IsGreaterThan(Standards.AdditionalInfo.AdditionalApartmentAreaPercentage).ToString());
                }

                //This parameter to store the number of bedrooms in the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_BEDS);
                if (p != null)
                {
                    var bedCount = apt.Rooms.Where(r => r is Bedroom).Count();

                    p.Set(bedCount.ToString());
                }

                //This parameter to store the number of persons in the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PERSON);
                p?.Set(apt.Occupancy.ToString());

                var aG = apt.ApartmentValidationData.FirstOrDefault(v=>v is AggregateAreaValidation) as AggregateAreaValidation;


                //This parameter to store the proposed aggregate bedroom floor area of the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_BED_AREA);
                if (p != null && aG != null)
                {
                    double sqFt = aG.AchievedAreas.Sum().FromUnit(UnitTypeId.SquareMeters);
                    
                    p.Set(sqFt);
                }

                //This parameter to store the minimum aggregate bedroom floor area required for the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_BED_AREA);
                if (p != null && aG != null)
                {
                    double sqFt = aG.RequiredAreas.Sum().FromUnit(UnitTypeId.SquareMeters); 

                    p.Set(sqFt);
                }

                //This parameter to store the proposed storage space floor area of the apartment.
                var cvs = apt.ApartmentValidationData.Where(v => v is CombinedAreaValidation).ToList();

                var cV = cvs.Select(c => c as CombinedAreaValidation).FirstOrDefault();
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_STORE_AREA);
                if (p != null && aG != null)
                {
                    double sqFt = cV.CombinedArea.FromUnit(UnitTypeId.SquareMeters);

                    p.Set(sqFt);
                }

                //This parameter to store the minimum storage space floor area required for the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_STORE_AREA);
                if (p != null && aG != null)
                {
                    double sqFt = cV.RequiredArea.FromUnit(UnitTypeId.SquareMeters); 
                    p.Set(sqFt);
                }


                var aptNumParam = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_NUMBER);

                #endregion

                #region Room parameters

                foreach (var rm in apt.Rooms)
                {

                    //This parameter to store the full apartment number where the room is located.
                    p = rm.Room.LookupParameter(RoomValidationConstants.CWO_ROOMS_APT_NUM);

                    if (aptNumParam != null && p != null)
                        p.Set(aptNumParam.AsValueString());

                    //This parameter to store the minimum width required for the room.
                    p = rm.Room.LookupParameter(RoomValidationConstants.CWO_ROOMS_MIN_WIDTH);

                    if (p != null)
                    {
                        var validation = rm.RoomValidationData.FirstOrDefault(v => v is DimensionValidation);

                        if (validation != null)
                        {
                            var dV = validation as DimensionValidation;

                            //var unt = Math.Round(UnitUtils.Convert(dV.RequireundMinWidth, UnitTypeId.Meters, desiredLengthDisplayUnits),2).ToString();
                            double fT = dV.RequiredMinWidth.FromUnit(UnitTypeId.Meters);

                            p.Set(fT);

                            //This parameter to store the proposed width of the room.
                            p = rm.Room.LookupParameter(RoomValidationConstants.CWO_ROOMS_PROP_WIDTH);

                            //unt = Math.Round(UnitUtils.Convert(dV.AchievedMinWidth, UnitTypeId.Meters, desiredLengthDisplayUnits),2).ToString();
                            fT = dV.AchievedMinWidth.FromUnit(UnitTypeId.Meters); 

                            p?.Set(fT);
                        }
                    }

                    //This parameter to store the minimum floor area required for the room.
                    p = rm.Room.LookupParameter(RoomValidationConstants.CWO_ROOMS_MIN_AREA);
                    if (p != null)
                    {

                        var validation = rm.RoomValidationData.FirstOrDefault(v => v is AreaValidation);

                        if (validation != null)
                        {
                            var areaValidation = validation as AreaValidation;

                            var sF = areaValidation.RequiredArea.FromUnit(UnitTypeId.SquareMeters);

                            p.Set(sF);
                        }

                    }
                }

                //only bedrooms
                var aaV = apt.ApartmentValidationData.FirstOrDefault(v => v is AggregateAreaValidation);

                if (aaV != null)
                {
                    var aggregateValidation = aaV as AggregateAreaValidation;

                    var bedRooms = apt.Rooms.Where(r => r is Bedroom).Select(r => r as Bedroom).ToList();

                    for (int i = 0; i < bedRooms.Count; i++)
                    {
                        var bed = bedRooms[i];

                        p = bed.Room.LookupParameter(RoomValidationConstants.CWO_ROOMS_MIN_AREA);
                        if (p != null)
                        {
                            var sF = aggregateValidation.RequiredAreas[i].FromUnit(UnitTypeId.SquareMeters);
                            p.Set(sF); 
                        }
                    }
                }

                #endregion
            }
        }

    }
}
