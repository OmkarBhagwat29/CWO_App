using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.UI.Models
{
    public class WallTagsModel
    {
        public static IEnumerable<Dimension>AvailableDimensionTypes { get; set; }

        public IEnumerable<Dimension> SelectedDimensions { get; set; }

        public Dimension GetSelectedDimensionType(string dimensionTypeName)
        {
            if(AvailableDimensionTypes == null)
                return null;

            return AvailableDimensionTypes.FirstOrDefault(d=>d.Name == dimensionTypeName);
        }

        public void GenerateDimensionAboveTags()
        {
            foreach (Dimension dimension in SelectedDimensions) {

                dimension.Above = "Hi";
            }
        }
    }
}
