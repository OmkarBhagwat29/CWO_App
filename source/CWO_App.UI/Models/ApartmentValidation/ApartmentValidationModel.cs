using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Events;
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

                //var param = area.LookupParameter(ApartmentValidationConstants.ApartmentType_ParamName);
                //if (param == null)
                //    return false;
                //var apartmentName = param.AsString();

                //if (apartmentName.Contains(ApartmentValidationConstants.Studio_Name) ||
                //    apartmentName.Contains(ApartmentValidationConstants.OneBedRoom_Name) ||
                //    apartmentName.Contains(ApartmentValidationConstants.TwoBedroomThreePerson_Name) ||
                //    apartmentName.Contains(ApartmentValidationConstants.TwoBedroomFourPerson_Name) ||
                //    apartmentName.Contains(ApartmentValidationConstants.ThreeBedroom_Name)
                //    )
                //    return true;

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
            this.Apartments.ForEach(ap => ap.Validate());

            //this.Apartments[apartmentIndex].Validate();

            //bake
            if (bakeValidationData)
            {
                UiApp.ActiveUIDocument.Document.UseTransaction(() =>
                {

                    this.Apartments.ForEach(ap => ap.Bake(UiApp.ActiveUIDocument.Document));

                   // this.Apartments[apartmentIndex].Bake(UiApp.ActiveUIDocument.Document);

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
                                    string bI = $"[{bedRm.Name}: Achieved Area -> {Math.Round(aaV.AchievedAreas[i], 2)}," +
                                    $" Required Area -> {aaV.RequiredAreas[i]}]";

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

        public void CreateAreaValidationSchedule(ElementId areaSchemaId)
        {

            var parameters = new List<ElementId>();

            var a = this.Apartments.FirstOrDefault().AreaBoundary;

            var dwellingTypeParam = a.LookupParameter(Standards.ParameterNames.ApartmentType);
            parameters.Add(dwellingTypeParam.Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_NUMBER).Id);
            parameters.Add(a.LookupParameter("Area").Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_AREA).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.AreaWith10Percentage_ParamName).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_BEDS).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PERSON).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_BED_AREA).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_BED_AREA).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_STORE_AREA).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_STORE_AREA).Id);
            parameters.Add(a.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_ABOVE_TEN_PERC).Id);


            UiApp.ActiveUIDocument.Document
                .UseTransaction(() =>
                {

                    UiApp.ActiveUIDocument.Document.CreateScheduleByCategory(BuiltInCategory.OST_Areas, parameters,
                        "Area Validation", areaSchemaId);

                }, "Schedule Created");

        }

        public void CreateMinWidthValidationSchedule()
        {
            var parameters = new List<ElementId>();

            var room = this.Apartments.FirstOrDefault().Rooms.FirstOrDefault().Room;

            parameters.Add(room.LookupParameter(RoomValidationConstants.RoomName_ParamName).Id);
            parameters.Add(room.LookupParameter(RoomValidationConstants.AchievedArea_ParamName).Id);
            parameters.Add(room.LookupParameter(RoomValidationConstants.CWO_ROOMS_MIN_AREA).Id);
           // parameters.Add(room.LookupParameter(RoomValidationConstants.AreaDifference_ParamName).Id);
            parameters.Add(room.LookupParameter(RoomValidationConstants.CWO_ROOMS_PROP_WIDTH).Id);
            parameters.Add(room.LookupParameter(RoomValidationConstants.CWO_ROOMS_MIN_WIDTH).Id);
            //parameters.Add(room.LookupParameter(RoomValidationConstants.WidthAccuracy_ParamName).Id);

            UiApp.ActiveUIDocument.Document
                                    .UseTransaction(() =>
                                    {

                                        UiApp.ActiveUIDocument
                                        .Document.CreateScheduleByCategory(BuiltInCategory.OST_Rooms, parameters, "Room Validation");

                                    }, "Schedule Created");

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

        public void CreateAndAssignRequiredParameters(Element areaElement, Element roomElement)
        {

            var defGroup = this._definitionFile.CreateOrGetGroup(ApartmentValidationConstants.SharedParameterGroupName);

            //all apartment level params => category : OST_Areas
            var areaExternalDefs = new List<ExternalDefinition>();

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_TYPE) == null)
            {
                var aptTypeDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_TYPE,
                    SpecTypeId.String.Text);

                areaExternalDefs.Add(aptTypeDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_NUMBER) == null)
            {
                var aptNameDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_NUMBER,
                    SpecTypeId.String.Text);

                areaExternalDefs.Add(aptNameDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_AREA) == null)
            {
                var aptAreaReqDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_MIN_AREA,
                    SpecTypeId.Number);
                areaExternalDefs.Add(aptAreaReqDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.AreaWith10Percentage_ParamName) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.AreaWith10Percentage_ParamName,
                    SpecTypeId.Number);
                areaExternalDefs.Add(paramDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_BEDS) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_BEDS,
                    SpecTypeId.Number);
                areaExternalDefs.Add(paramDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PERSON) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_PERSON,
                    SpecTypeId.Number);
                areaExternalDefs.Add(paramDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_BED_AREA) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_PROP_BED_AREA,
                    SpecTypeId.Number);
                areaExternalDefs.Add(paramDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_BED_AREA) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_MIN_BED_AREA,
                    SpecTypeId.Number);
                areaExternalDefs.Add(paramDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_STORE_AREA) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_PROP_STORE_AREA,
                    SpecTypeId.Number);
                areaExternalDefs.Add(paramDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_STORE_AREA) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_MIN_STORE_AREA,
                    SpecTypeId.Number);
                areaExternalDefs.Add(paramDef);
            }

            if (areaElement.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_ABOVE_TEN_PERC) == null)
            {
                var paramDef = defGroup.GetOrCreateDefinitionInGroup(ApartmentValidationConstants.CWO_APARTMENTS_ABOVE_TEN_PERC,
                    SpecTypeId.String.Text);
                areaExternalDefs.Add(paramDef);
            }


            var roomExternalDefs = new List<ExternalDefinition>();

            if (roomElement.LookupParameter(Standards.ParameterNames.RoomType) == null)
            {
                var roomTypeDef = defGroup.GetOrCreateDefinitionInGroup(RoomValidationConstants.RoomName_ParamName,
                SpecTypeId.String.Text);

                roomExternalDefs.Add(roomTypeDef);
            }

            if (roomElement.LookupParameter(RoomValidationConstants.CWO_ROOMS_PROP_WIDTH) == null)
            {
                var achievedWidthDef = defGroup.GetOrCreateDefinitionInGroup(RoomValidationConstants.CWO_ROOMS_PROP_WIDTH, SpecTypeId.Number);

                roomExternalDefs.Add(achievedWidthDef);
            }

            if (roomElement.LookupParameter(RoomValidationConstants.CWO_ROOMS_MIN_WIDTH) == null)
            {
                var requiredWidthDef = defGroup.GetOrCreateDefinitionInGroup(RoomValidationConstants.CWO_ROOMS_MIN_WIDTH, SpecTypeId.Number);

                roomExternalDefs.Add(requiredWidthDef);
            }

            //if (roomElement.LookupParameter(RoomValidationConstants.AreaDifference_ParamName) == null)
            //{
            //    var areaDiffDef = defGroup.GetOrCreateDefinitionInGroup(RoomValidationConstants.AreaDifference_ParamName, SpecTypeId.Number);

            //    roomExternalDefs.Add(areaDiffDef);
            //}

            //if (roomElement.LookupParameter(RoomValidationConstants.WidthAccuracy_ParamName) == null)
            //{
            //    var widthAccuracyDef = defGroup.GetOrCreateDefinitionInGroup(RoomValidationConstants.WidthAccuracy_ParamName, SpecTypeId.Number);

            //    roomExternalDefs.Add(widthAccuracyDef);
            //}


            if (areaExternalDefs.Count > 0)
            {
                UiApp.ActiveUIDocument.Document.UseTransaction(() =>
                {
                    areaExternalDefs.TryAddToDocument(this.UiApp.ActiveUIDocument.Document,
                    [BuiltInCategory.OST_Areas], BindingKind.Instance, GroupTypeId.IdentityData);
                }, "Area Shared Parameters added");

            }

            if (roomExternalDefs.Count > 0)
            {
                UiApp.ActiveUIDocument.Document.UseTransaction(() =>
                {
                    roomExternalDefs.TryAddToDocument(this.UiApp.ActiveUIDocument.Document,
                    [BuiltInCategory.OST_Rooms], BindingKind.Instance, GroupTypeId.IdentityData);
                }, "Room Shared Parameter added");

            }
        }

        public void SetApartmentParameters()
        {
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
                        p.Set(Math.Round(aV.RequiredArea, 2));
                    }

                    //This parameter to store whether or not the apartment area is above 10% of the minimum overall floor area required.
                    p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_ABOVE_TEN_PERC);
                    p?.Set(aV.IsGreaterThan(10.0));
                }

                //This parameter to store the number of bedrooms in the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_BEDS);
                if (p != null)
                {
                    var bedCount = apt.Rooms.Where(r => r is Bedroom).Count();

                    p.Set(bedCount);
                }

                //This parameter to store the number of persons in the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PERSON);
                p?.Set(apt.Occupancy);

                var cVs = apt.ApartmentValidationData.Where(v => v is CombinedAreaValidation).ToList();

                var cV = cVs.Select(c => c as CombinedAreaValidation)
                    .Where(c => c.SpatialType == typeof(Bedroom)).FirstOrDefault();

                //This parameter to store the proposed aggregate bedroom floor area of the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_BED_AREA);
                if (p != null && cV != null)
                {
                        p.Set(Math.Round(cV.CombinedArea, 2));
                }

                //This parameter to store the minimum aggregate bedroom floor area required for the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_BED_AREA);
                if (p != null && cV != null)
                {
                   p.Set(Math.Round(cV.RequiredArea,2));
                }

                //This parameter to store the proposed storage space floor area of the apartment.
                cV = cVs.Select(c => c as CombinedAreaValidation)
                    .Where(c=>c.SpatialType == typeof(Storage)).FirstOrDefault();
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_PROP_STORE_AREA);
                if (p != null && cV != null)
                {
                    p.Set(Math.Round(cV.CombinedArea, 2));
                }

                //This parameter to store the minimum storage space floor area required for the apartment.
                p = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_MIN_STORE_AREA);
                if (p != null && cV != null)
                {
                    p.Set(Math.Round(cV.RequiredArea, 2));
                }


                var aptNumParam = apt.AreaBoundary.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_NUMBER);

                #endregion

                #region Room parameters

                foreach (var rm in apt.Rooms)
                {
                    if (rm is Bedroom)
                        continue;

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

                            p.Set(dV.RequiredMinWidth);


                            //This parameter to store the proposed width of the room.
                            p = rm.Room.LookupParameter(RoomValidationConstants.CWO_ROOMS_PROP_WIDTH);

                            p?.Set(Math.Round(dV.RequiredMinWidth, 2));
                        }
                    }

                    //This parameter to store the minimum floor area required for the room.
                    p = rm.Room.LookupParameter(RoomValidationConstants.CWO_ROOMS_MIN_AREA);
                    if (p != null)
                    {
                        //var validation = apt.ApartmentValidationData.FirstOrDefault(v => v is CombinedAreaValidation);

                        var validation = rm.RoomValidationData.FirstOrDefault(v => v is AreaValidation);

                        if (validation != null)
                        {
                            var areaValidation = validation as AreaValidation;

                            p.Set(areaValidation.RequiredArea);
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
                            p.Set(aggregateValidation.RequiredAreas[i]); 
                        }
                    }
                }



                #endregion
            }
        }

    }
}
