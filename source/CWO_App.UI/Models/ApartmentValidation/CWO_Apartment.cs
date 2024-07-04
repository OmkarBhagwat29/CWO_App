
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
        public static double BedroomAreaThreshold = 10.5;
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

            var allBedrooms = rooms.Where(r =>
            r.LookupParameter(RoomValidationConstants.RoomName_ParamName).AsString()
            .Contains(RoomValidationConstants.Bedroom_Name,
            StringComparison.CurrentCultureIgnoreCase)).ToList();

            var bathrooms = rooms.Where(r =>
            r.LookupParameter(RoomValidationConstants.RoomName_ParamName).AsString()
            .Contains(RoomValidationConstants.Bathroom_Name, StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (bathrooms.Count == 0)
                return null;

            //this is a studio
            CWO_Apartment apt = null;
            ApartmentValidationData areaWidthValidationData = null;
            if (allBedrooms.Count == 0 && bathrooms.Count == 1)
            {
                apt = new(ApartmentType.Studio, areaBoundary) { Name = ApartmentValidationConstants.Studio_Name, Occupancy = 2 };
                areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Studio);
            }
            else if (allBedrooms.Count() == 1)
            {

                    apt = new(ApartmentType.One_Bedroom_1_Person, areaBoundary) {
                        Name = ApartmentValidationConstants.OneBedRoomOnePerson_Name,
                        Occupancy = 2 };
                    areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.One_Bedroom_2_Person);


            }
            else if (allBedrooms.Count() == 2)
            {
                ////if any bedroom area is less that 10 then it is apartment for 3 person
                ////if apartment area is less 68 then it is apartment for 3 person

                if (allBedrooms.Any(b => b.Area.ToUnit(UnitTypeId.SquareMeters) <= BedroomAreaThreshold))
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
                if (allBedrooms.Any(b => b.Area.ToUnit(UnitTypeId.SquareMeters) <= BedroomAreaThreshold))
                {
                    apt = new(ApartmentType.Three_Bedroom_5_Person, areaBoundary)
                    {
                        Name = ApartmentValidationConstants.ThreeBedroomFivePerson_Name,
                        Occupancy = 5
                    };

                    areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Three_Bedroom_5_Person);
                }
                else
                {
                    apt = new(ApartmentType.Three_Bedroom_5_Person, areaBoundary)
                    {
                        Name = ApartmentValidationConstants.ThreeBedroomSixPerson_Name,
                        Occupancy = 6
                    };

                    areaWidthValidationData = standards.GetStandardsForApartment(ApartmentType.Three_Bedroom_6_Person);
                }
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
            AddValidationData(apt,standards, areaWidthValidationData);

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
            ApartmentValidationData validationData)
        {
            List<Bedroom> bdRms = [];
            foreach (var rm in rooms)
            {
                var param = rm.LookupParameter(RoomValidationConstants.RoomName_ParamName);
                if (param == null)
                    continue;
                var val = param.AsString();

                if (val.Contains(RoomValidationConstants.KitchenLivingDinning_Name_1,
                    StringComparison.CurrentCultureIgnoreCase) ||
                    val.Contains(RoomValidationConstants.KitchenLivingDinning_Name_2,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var kld = new KLD(rm)
                    {
                        Name = val,
                        MinimumArea = validationData.MinimumLivingDinningKitchenArea,
                        MinimumWidth = validationData.MinimumLivingDinningKitchenWidth
                    };
                    apartment.AddRoom(kld);
                }
                else if (val.Contains(RoomValidationConstants.StorageRoomName_Name, StringComparison.CurrentCultureIgnoreCase) ||
                    val.Contains(RoomValidationConstants.StorageRoomName_Name_2,StringComparison.CurrentCultureIgnoreCase) ||
                    val.Contains(RoomValidationConstants.StorageRoomName_Name_3, StringComparison.CurrentCultureIgnoreCase))
                {
                    var storage = new Storage(rm)
                    {
                        Name = val,
                        MinimumArea = validationData.MinimumStorageArea
                    };
                    apartment.AddRoom(storage);
                }
                else if (val.Contains(RoomValidationConstants.Balcony_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var balcony = new Balcony(rm)
                    {
                        Name = val,
                        MinimumArea = validationData.MinimumBalconyArea,
                        MinimumWidth = validationData.MinimumBalconyWidth
                    };
                    apartment.AddRoom(balcony);
                }
                else if (val.Contains(RoomValidationConstants.Bedroom_Name,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    var bedroom = new Bedroom(rm)
                    {
                        Name = val,
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
            bdRms = [.. bdRms.OrderBy(r => r.Room.Area)];

            //set minimum width of bedroom
            for (int i = 0; i < bdRms.Count; i++)
            {
                //set bedroom type depending on its area
                if (bdRms[i].Room.Area.ToUnit(UnitTypeId.SquareMeters) <= BedroomAreaThreshold)
                {
                    //single bed
                    bdRms[i].SetBedRoomType(BedroomType.SingleBed);
                }
                else
                {
                    bdRms[i].SetBedRoomType(BedroomType.DoubleBed);
                }

                bdRms[i].MinimumWidth = validationData.MinimumBedroomWidths[i];

                apartment.AddRoom(bdRms[i]);
            }
        }


        private static void AddValidationData(CWO_Apartment apartment, ApartmentStandards standards,
            ApartmentValidationData validationData)
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
            var storages = apartment.Rooms.Where(r => r is Storage)
                .Select(r => r as Storage).ToList();

            var storeAreas = storages.Select(r => r.Room.Area.ToUnit(UnitTypeId.SquareMeters))
                .ToList();
            CombinedAreaValidation caV = new(storeAreas, validationData.MinimumStorageArea, typeof(Storage));
            apartment.AddValidationData(caV);


            foreach (var room in apartment.Rooms)
            {
                if (room is GenericRoom)
                    continue;

                if (room is Storage st)
                {
                    AreaValidation aGV = new(room.Room.Area.ToUnit(UnitTypeId.SquareMeters),
                        standards.AdditionalInfo.MaxStoreRoomAreaAllowed, true);

                    room.AddValidationData(aGV);

                    continue;
                }

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
