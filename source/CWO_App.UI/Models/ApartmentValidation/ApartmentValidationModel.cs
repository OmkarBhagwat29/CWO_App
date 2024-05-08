using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using CWO_App.UI.Constants;
using CWO_App.UI.Requirements;
using RevitCore.Extensions;
using RevitCore.Extensions.PointInPoly;
using RevitCore.ResidentialApartments;
using RevitCore.ResidentialApartments.Rooms;
using RevitCore.ResidentialApartments.Validation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;


namespace CWO_App.UI.Models.ApartmentValidation
{
    public class ApartmentValidationModel
    {
        public ApartmentValidationModel(UIApplication uiApp,
            ApartmentStandards _standards)
        {
            UiApp = uiApp;
            Standards = _standards;
        }

        private List<AreaRoomAssociation> _associations = [];

        public List<CWO_Apartment> Apartments = [];

        public UIApplication UiApp { get; }
        public ApartmentStandards Standards { get; }

        public void SetAreRooAssociation()
        {
            _associations = AreaRoomAssociation
                    .GetCWOApartmentsInProject(this.UiApp.ActiveUIDocument.Document,
            (area) =>
            {
                var param = area.LookupParameter(this.Standards.ParameterNames.ApartmentType);
                if (param == null)
                    return false;
                var apartmentName = param.AsString();

                if (apartmentName.Contains(ApartmentValidationConstants.Studio_Name) ||
                    apartmentName.Contains(ApartmentValidationConstants.OneBedRoom_Name) ||
                    apartmentName.Contains(ApartmentValidationConstants.TwoBedroomThreePerson_Name) ||
                    apartmentName.Contains(ApartmentValidationConstants.TwoBedroomFourPerson_Name) ||
                    apartmentName.Contains(ApartmentValidationConstants.ThreeBedroom_Name)
                    )
                    return true;

                return false;
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
    .                                   CreateApartmentsAndSetApartmentTypeInProject(
                                        this.UiApp.ActiveUIDocument.Document,
                                        _associations, this.Standards);
                            }, "Apartments Created");
        }

        public void Validate()
        {

            CWO_Apartment.ValidateApartments(this.Apartments);
        }

        public void RunStartToEnd()
        {
            this.SetAreRooAssociation();
            this.SetApartments();
            this.Validate();
        }

    }
}
