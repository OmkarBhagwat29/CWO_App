
using Autodesk.Revit.DB.Architecture;
using CWO_App.UI.Constants;
using CWO_App.UI.Requirements;
using RevitCore.Extensions;
using RevitCore.ResidentialApartments;
using RevitCore.ResidentialApartments.Rooms;
using RevitCore.ResidentialApartments.Validation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;



namespace CWO_App.UI.Models.ApartmentValidation
{
    public class CWO_Apartment : Apartment
    {
        public CWO_Apartment(ApartmentType _type,
        Area areaBoundary)
        {
            this.Type = _type;
            this.AreaBoundary = areaBoundary;
        }

        public override ApartmentType Type { get; }

        public override void Validate()
        {
            //apartment level validation
            this.ApartmentValidationData.ForEach(d=>d.Validate());

            //room level validation
            this.Rooms.ForEach(r=>r.RoomValidationData.ForEach(d=>d.Validate()));
        }

        public static CWO_Apartment CreateApartment(Area areaBoundary, List<Room> rooms, ApartmentStandards standards)
        {
            //TODO:: Check paramExistance outside
            //var nameParam = rooms[0].LookupParameter(standards.ParameterNames.RoomType) ??
            //    throw new ArgumentNullException($"{standards.ParameterNames.RoomType} - parameter does not exist." +
            //    $"\nThis parameter defines room type(eg. Bedroom,Kitchen)\n and is necessary to set.");

            var allBedrooms = rooms.Where(r => 
            r.LookupParameter(standards.ParameterNames.RoomType).AsString()
            .Contains(standards.RoomTypes["Bedroom"],
            StringComparison.CurrentCultureIgnoreCase)).ToList();

            //this is a studio
            CWO_Apartment apt = null;
            AreaWidthValidationData areaWidthValidationData = null;
            if (!allBedrooms.Any())
            {
                apt = new(ApartmentType.Studio, areaBoundary) {Name = ApartmentValidationConstants.Studio_Name };
                areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Studio);
            }
            else if (allBedrooms.Count() == 1)
            {
                apt = new(ApartmentType.One_Bedroom, areaBoundary) { Name = ApartmentValidationConstants.OneBedRoom_Name }; ;
                areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.One_Bedroom);
            }
            else if (allBedrooms.Count() == 2)
            {
                double minArea_3Person = standards
                     .ValidationInfo[ApartmentValidationConstants.TwoBedroomThreePerson_Name].MinimumFloorArea;

                var bedAreas_3Person = standards
                    .ValidationInfo[ApartmentValidationConstants.TwoBedroomThreePerson_Name].MinimumAggregateBedroomAreas;

                double minArea_4Person = standards
                    .ValidationInfo[ApartmentValidationConstants.TwoBedroomFourPerson_Name].MinimumFloorArea;

                var bedAreas_4Person = standards
                   .ValidationInfo[ApartmentValidationConstants.TwoBedroomFourPerson_Name].MinimumAggregateBedroomAreas;

                double achievedBoundaryArea = areaBoundary.Area.ToUnit(UnitTypeId.SquareMeters);

                var combinedBedArea_3Person = bedAreas_3Person.Sum();
                var combinedBedArea_4Person = bedAreas_4Person.Sum();

                var bed_1Area = allBedrooms.First().Area.ToUnit(UnitTypeId.SquareMeters);
                var bed_2Area = allBedrooms.Last().Area.ToUnit(UnitTypeId.SquareMeters);
                var achievedBedArea = bed_1Area + bed_2Area;

                if ((achievedBoundaryArea <= minArea_3Person || achievedBoundaryArea >= minArea_3Person)
                    && achievedBoundaryArea <= minArea_4Person)
                {
                    apt = new(ApartmentType.Two_Bedroom_3_Person, areaBoundary) { Name = ApartmentValidationConstants.TwoBedroomThreePerson_Name };
                    areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Two_Bedroom_3_Person);
                }
                else
                {
                    apt = new(ApartmentType.Two_Bedroom_4_Person, areaBoundary) { Name = ApartmentValidationConstants.TwoBedroomFourPerson_Name };
                    areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Two_Bedroom_4_Person);
                }

            }
            else if (allBedrooms.Count() == 3)
            {
                apt = new(ApartmentType.Three_Bedroom, areaBoundary) { Name = ApartmentValidationConstants.ThreeBedroom_Name };
                areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Three_Bedroom);
            }

            if (apt == null)
                return null;

            AddRoomsToApartment(apt,rooms, standards, areaWidthValidationData);
            AddValidationData(apt, areaWidthValidationData);
           
            return apt;   
        }

        public static List<CWO_Apartment> CreateApartmentsAndSetApartmentTypeInProject(
            Document doc,
            List<AreaRoomAssociation> associations,
            ApartmentStandards standards)
        {
            List<CWO_Apartment> apts = [];

            foreach (var ass in associations)
            {
                var apt = CreateApartment(ass.AreaBoundary,ass.Rooms,standards);

                if (apt == null)
                    continue;

                var element = doc.GetElement(ass.AreaBoundary.Id);
                var apartmentTypeParam = element.LookupParameter(standards.ParameterNames.ApartmentType);
                bool success = apartmentTypeParam.Set(apt.Name);
                if (!success)
                    throw new ArgumentException($"Unable to set value for parameter '{standards.ParameterNames.ApartmentType}'");

                apts.Add(apt);
            }
            return apts;
        }

        public static List<CWO_Apartment> CreateApartments(List<AreaRoomAssociation>associations,
            ApartmentStandards standards)
        {
            List<CWO_Apartment> apts = [];

            foreach (AreaRoomAssociation ass in associations)
            {
                var param = ass.AreaBoundary.LookupParameter(standards.ParameterNames.ApartmentType) ?? throw new ArgumentNullException($"Unable to find Apartment Type.\n" +
                    $"Check if {standards.ParameterNames.ApartmentType} parameter exists for Selected Apartment Area Boundary.");
                var apartmentTypeName = param.AsString();

                var apartmentType = standards.FindApartmentType(apartmentTypeName);
                if (apartmentType == ApartmentType.None)
                {
                    continue;
                    //throw new ArgumentNullException($"Unable to find Apartment Type.\n" +
                    //    $"Check if {standards.ParameterNames.ApartmentType} parameter value is set to according to standards.");
                }
                CWO_Apartment apt = new(apartmentType, ass.AreaBoundary);

                //get validation data
                var validationData = standards.GetStandardsForApartment(apartmentType) ??
                    throw new ArgumentNullException($"Unable to find Apartment Type Standards.\n" +
                        $"Check .json settings file if for apartment name.");

                AddRoomsToApartment(apt,ass.Rooms,standards,validationData);
                AddValidationData(apt,validationData);

                apts.Add(apt);
            }

            return apts;
        }

        public static void ValidateApartments(List<CWO_Apartment> apartments)
        {
            apartments.ForEach(apt=>apt.Validate());
        }

        private static void AddRoomsToApartment(CWO_Apartment apartment,
            List<Room> rooms,ApartmentStandards standards,
            AreaWidthValidationData validationData)
        {
            foreach (var rm in rooms)
            {
                var param = rm.LookupParameter(standards.ParameterNames.RoomType);
                if (param == null)
                    continue;
                var val = param.AsString();

                if (val.Equals(RoomValidationConstants.KitchenLivingDinning_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var kld = new KLD(rm)
                    {
                        Name = RoomValidationConstants.KitchenLivingDinning_Name,
                        MinimumArea = validationData.MinimumLivingDinningKitchenArea,
                        MinimumWidth = validationData.MinimumLivingDinningKitchenWidth
                    };
                    apartment.AddRoom(kld);
                }
                else if (val.Equals(RoomValidationConstants.StorageRoomName_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var storage = new Storage(rm)
                    {
                        Name = RoomValidationConstants.StorageRoomName_Name,
                        MinimumArea = validationData.MinimumStorageArea
                    };
                    apartment.AddRoom(storage);
                }
                else if (val.Equals(RoomValidationConstants.Balcony_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var balcony = new Balcony(rm)
                    {
                        Name = RoomValidationConstants.Balcony_Name,
                        MinimumArea = validationData.MinimumBalconyArea,
                        MinimumWidth = validationData.MinimumBalconyWidth
                    };
                    apartment.AddRoom(balcony);
                }
                else if (val.Equals(RoomValidationConstants.Bedroom_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm)
                    {
                        Name = RoomValidationConstants.Bedroom_Name,
                        MinimumWidth = validationData.MinimumBedroomWidth
                    };

                    apartment.AddRoom(new Bedroom(rm));
                }
                else if (val.Equals(RoomValidationConstants.Bedroom_1_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm)
                    {
                        Name = RoomValidationConstants.Bedroom_1_Name,
                        MinimumWidth = validationData.MinimumBedroomWidth
                    };

                    apartment.AddRoom(new Bedroom(rm));
                }
                else if (val.Equals(RoomValidationConstants.Bedroom_2_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm)
                    {
                        Name = RoomValidationConstants.Bedroom_2_Name,
                        MinimumWidth = validationData.MinimumBedroomWidth
                    };

                    apartment.AddRoom(new Bedroom(rm));
                }
                else if (val.Equals(RoomValidationConstants.Bedroom_3_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm)
                    {
                        Name = RoomValidationConstants.Bedroom_3_Name,
                        MinimumWidth = validationData.MinimumBedroomWidth
                    };

                    apartment.AddRoom(new Bedroom(rm));
                }
            }
        }

        private static void AddValidationData(CWO_Apartment apartment,
            AreaWidthValidationData validationData)
        {
            AreaValidation v = new(apartment.AreaBoundary,
               validationData.MinimumFloorArea);

            apartment.AddValidationData(v);

            //collect bedrooms add area validation data to apartment
            var bedrooms = apartment.Rooms.Where(r=> r is Bedroom)
                .Select(r=>r.Room as SpatialElement).ToList();

            AggregateAreaValidation agv = new(bedrooms,
    validationData.MinimumAggregateBedroomAreas);
            apartment.AddValidationData(agv);


            //collect storage and validate with area
            var storages = apartment.Rooms.Where(r => r is Storage)
                .Select(r => r.Room as SpatialElement).ToList();

            CombinedAreaValidation caV = new(storages, validationData.MinimumStorageArea);
            apartment.AddValidationData(caV);


            foreach (var room in apartment.Rooms)
            {
                if (room is not Bedroom && room is not Storage)
                {
                    AreaValidation aV = new(room.Room, room.MinimumArea);
                    room.AddValidationData(aV);
                }

                if (room is not Storage)
                {
                    DimensionValidation dV = new(room.Room, room.MinimumWidth);
                    room.AddValidationData(dV);
                }
            }

        }

    }
}
