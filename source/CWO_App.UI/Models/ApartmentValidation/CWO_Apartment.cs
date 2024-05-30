
using Autodesk.Revit.DB.Architecture;
using CWO_App.UI.Constants;
using CWO_App.UI.Requirements;
using Microsoft.Extensions.Logging;
using RevitCore.Extensions;
using RevitCore.ResidentialApartments;
using RevitCore.ResidentialApartments.Rooms;
using RevitCore.ResidentialApartments.Validation;
using System.Runtime.CompilerServices;




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


        public static CWO_Apartment CreateApartment(Area areaBoundary, List<Room> rooms,
            ApartmentStandards standards, ILogger _logger)
        {
            //var j = standards.RoomTypes["Bedroom"];
            var allBedrooms = rooms.Where(r =>
            r.LookupParameter(RoomValidationConstants.RoomName_ParamName).AsString()
            .Contains(RoomValidationConstants.Bedroom_Name,
            StringComparison.CurrentCultureIgnoreCase)).ToList();

            var bathrooms = rooms.Where(r =>
            r.LookupParameter("Name").AsString()
            .Contains(RoomValidationConstants.Bathroom_Name, StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (bathrooms.Count == 0)
                return null;

            //this is a studio
            CWO_Apartment apt = null;
            AreaWidthValidationData areaWidthValidationData = null;
            if (allBedrooms.Count == 0 && bathrooms.Count == 1)
            {
                apt = new(ApartmentType.Studio, areaBoundary) { Name = ApartmentValidationConstants.Studio_Name, Occupancy = 2 };
                areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Studio);
            }
            else if (allBedrooms.Count() == 1)
            {
                apt = new(ApartmentType.One_Bedroom, areaBoundary) { Name = ApartmentValidationConstants.OneBedRoom_Name, Occupancy = 3 }; ;
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
                    apt = new(ApartmentType.Two_Bedroom_3_Person, areaBoundary)
                    {
                        Name = ApartmentValidationConstants.TwoBedroomThreePerson_Name,
                        Occupancy = 3
                    };
                    areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Two_Bedroom_3_Person);
                }
                else
                {
                    apt = new(ApartmentType.Two_Bedroom_4_Person, areaBoundary)
                    {
                        Name = ApartmentValidationConstants.TwoBedroomFourPerson_Name,
                        Occupancy = 4
                    };
                    areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Two_Bedroom_4_Person);
                }

            }
            else if (allBedrooms.Count() == 3)
            {
                apt = new(ApartmentType.Three_Bedroom, areaBoundary) { Name = ApartmentValidationConstants.ThreeBedroom_Name };
                areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Three_Bedroom);
            }
            else
            {
                string error = $"For Area Boundary Id {areaBoundary.Id}, No apartment type found!!!\n" +
                $"Number of Spaces Contains {rooms.Count}.\n" +
                $"Check setting file if the Apartment Type exists.";

                _logger.LogError(error);

                return null;
            }


            AddRoomsToApartment(apt, rooms, standards, areaWidthValidationData);
            AddValidationData(apt, areaWidthValidationData);

            return apt;
        }

        public static List<CWO_Apartment> CreateApartmentsAndSetApartmentTypeInProject(
            Document doc, ILogger _logger,
            List<AreaRoomAssociation> associations,
            ApartmentStandards standards)
        {
            List<CWO_Apartment> apts = [];

            foreach (var ass in associations)
            {
                var apt = CreateApartment(ass.AreaBoundary, ass.Rooms, standards, _logger);

                if (apt == null)
                {
                    continue;
                }

                var element = doc.GetElement(ass.AreaBoundary.Id);
                //set apartment type
                var apartmentTypeParam = element.LookupParameter(ApartmentValidationConstants.CWO_APARTMENTS_TYPE);

                if (apartmentTypeParam != null)
                    apartmentTypeParam.Set(apt.Name);

                apts.Add(apt);
            }
            return apts;
        }

        private static void AddRoomsToApartment(CWO_Apartment apartment,
            List<Room> rooms, ApartmentStandards standards,
            AreaWidthValidationData validationData)
        {
            List<Bedroom> bdRms = [];
            foreach (var rm in rooms)
            {
                var param = rm.LookupParameter(RoomValidationConstants.RoomName_ParamName);
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
                    StringComparison.CurrentCultureIgnoreCase) || val.Equals(RoomValidationConstants.StorageRoomName_Name_2,
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

                    bdRms.Add(bedroom);
                }
                else if (val.Equals(RoomValidationConstants.Bedroom_1_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm);

                    bedroom.Name = RoomValidationConstants.Bedroom_1_Name;
                    bedroom.MinimumWidth = validationData.MinimumBedroomWidth;

                    bdRms.Add(bedroom);
                }
                else if (val.Equals(RoomValidationConstants.Bedroom_2_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm)
                    {
                        Name = RoomValidationConstants.Bedroom_2_Name,
                        MinimumWidth = validationData.MinimumBedroomWidth
                    };

                    bdRms.Add(bedroom);
                }
                else if (val.Equals(RoomValidationConstants.Bedroom_3_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm)
                    {
                        Name = RoomValidationConstants.Bedroom_3_Name,
                        MinimumWidth = validationData.MinimumBedroomWidth
                    };
                    bdRms.Add(bedroom);
                }
                else
                {
                    //it is generic room
                    var genericRoom = new GenericRoom(rm)
                    {
                        Name = rm.Name
                    };

                    apartment.AddRoom(genericRoom);
                }
            }

            //order bedrooms by area
            bdRms = bdRms.OrderBy(r => r.Room.Area).ToList();

            bdRms.ForEach(r => apartment.AddRoom(r));
        }

        private static void AddValidationData(CWO_Apartment apartment,
            AreaWidthValidationData validationData)
        {
            //apartment area validation
            AreaValidation v = new(apartment.AreaBoundary.Area.ToUnit(UnitTypeId.SquareMeters),
               validationData.MinimumFloorArea);

            apartment.AddValidationData(v);

            //collect bedrooms add area validation data to apartment
            var bedroomAreas = apartment.Rooms.Where(r => r is Bedroom)
                .Select(b => b.Room.Area.ToUnit(UnitTypeId.SquareMeters))
                .ToList();

            AggregateAreaValidation agv = new(bedroomAreas,
    validationData.MinimumAggregateBedroomAreas, typeof(CWO_Apartment));
            apartment.AddValidationData(agv);


            //collect storage and validate with area
            var storeAreas = apartment.Rooms.Where(r => r is Storage)
                .Select(r => r.Room.Area.ToUnit(UnitTypeId.SquareMeters))
                .ToList();
            CombinedAreaValidation caV = new(storeAreas, validationData.MinimumStorageArea, typeof(Storage));
            apartment.AddValidationData(caV);


            foreach (var room in apartment.Rooms)
            {
                if (room is Storage || room is GenericRoom)
                    continue;

                //for KLD add validation data
                if (room is KLD klD)
                {
                    AreaValidation aV = new(room.Room.Area.ToUnit(UnitTypeId.SquareMeters), room.MinimumArea);
                    room.AddValidationData(aV);
                }

                var sebOptions = new SpatialElementBoundaryOptions
                {
                    SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish,
                };

                var roomSolid = room.ComputeRoomSolid(sebOptions, out List<XYZ> roomBoundaryPoints);


                DimensionValidation dV = new(roomSolid, roomBoundaryPoints, room.MinimumWidth, room.GetType());
                room.AddValidationData(dV);
            }

        }

    }
}
