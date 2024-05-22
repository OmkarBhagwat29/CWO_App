using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CWO_App.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class ACC_Command : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                // URL to open
                string url = "https://acc.autodesk.com";

                // Open the URL in the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static void LaunchACCButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            panel.AddItem(new PushButtonData(MethodBase.GetCurrentMethod().DeclaringType?.Name,
                $"To Hub", assembly.Location, MethodBase.GetCurrentMethod().DeclaringType?.FullName)
            {
                ToolTip = "Launch CWO ACC Hub",
                LargeImage = ImageUtils.LoadImage(assembly, "ACC_Hub_32x32.png")
            });

        }
    }
}
