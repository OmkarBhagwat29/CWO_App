
namespace CWO_App.UI.Services
{
    public class WindowService : IWindowService
    {
        public event EventHandler WindowOpened;

        public void RaiseWindowOpened()
        {
            WindowOpened?.Invoke(this, EventArgs.Empty);
        }
    }
}
