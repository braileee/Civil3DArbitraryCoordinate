using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autofac;
using Civil3DArbitraryCoordinate.Startup;
using Civil3DArbitraryCoordinate.Views;

namespace Civil3DArbitraryCoordinate
{
    public class Main
    {
        [CommandMethod("PSV", "Civil3DArbitraryCoordinate", CommandFlags.Modal)]
        public static void Start()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mainView = container.Resolve<MainView>();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModelessWindow(mainView);
        }
    }
}
