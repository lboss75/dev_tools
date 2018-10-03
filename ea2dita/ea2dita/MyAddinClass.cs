using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ea2dita.lib;
using EA;

namespace ea2dita
{
    public class MyAddinClass
    {
        // define menu constants
        const string menuHeader = "-DITA";
        const string menuExport = "Export";

        public string EA_Connect(Repository repository)
        {
            return "a string";
        }

        public object EA_GetMenuItems(Repository repository, string location, string menuName)
        {

            switch (menuName)
            {
                // defines the top level menu option
                case "":
                    return menuHeader;
                // defines the submenu options
                case menuHeader:
                    string[] subMenus = { menuExport };
                    return subMenus;
            }

            return "";
        }

        public void EA_GetMenuState(EA.Repository repository, string location, string menuName, string itemName, ref bool isEnabled, ref bool isChecked)
        {
            switch (itemName)
            {
                // define the state of the hello menu option
                case menuHeader:
                case menuExport:
                    isEnabled = true;
                    break;

                // there shouldn't be any other, but just in case disable it.
                default:
                    isEnabled = false;
                    break;
            }
        }

        public void EA_MenuClick(EA.Repository repository, string location, string menuName, string itemName)
        {
            switch (itemName)
            {
                // user has clicked the menuHello menu option
                case menuExport:
                    this.processExport(repository);
                    break;
            }
        }

        private void processExport(EA.Repository repository)
        {
            var dlg = new Export2DitaForm();
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var package = repository.GetTreeSelectedPackage();
            ExportPackage.Export(
                repository,
                package,
                dlg.DitaMapFile,
                new ExportOptions()
                {
                    HideEmptyElements =  dlg.HideEmptyElements
                });

        }

        public void EA_Disconnect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
