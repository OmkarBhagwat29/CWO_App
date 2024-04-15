
namespace CWO_App.UI.Services
{
    public interface IWindowService
    {
        event EventHandler WindowOpened;
        void RaiseWindowOpened();
    }
}
