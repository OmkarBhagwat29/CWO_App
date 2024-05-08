using Autodesk.Revit.DB.Structure;
using RevitCore.ResidentialApartments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI
{
    public class ApartmentTypeInfoWrapper
    {
        public ApartmentType ApartmentType { get; set; }
        public List<string> RoomTypeNames { get; set; } = [];

        public string ApartmentMinimumRequiredArea { get; set; }


    }
}
