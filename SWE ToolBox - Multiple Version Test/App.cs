namespace SWE_Toolbox
{

    #region Namespaces
    using Autodesk.Revit.UI;
    using System;
    using System.Reflection;
    using System.Windows.Media.Imaging;
    #endregion

    /// <summary>
    /// Plugin's main entry point.
    /// </summary>
    /// <seealso cref="Autodesk.Revit.UI.IExternalApplication"/>

    public class App : IExternalApplication
    #region external application public methods
    {
        /// <summary>
        /// Called when Revit starts up.
        /// </summary>
        /// <param name="a">The application.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Result OnStartup(UIControlledApplication a)
        {
            //Plugin's main tab name.
            string tabName = "SWE Electrical";

            //Panel name hosted on ribbion tab.
            string panelElectrical = "Panel Schedule";

            //Create Tab on Revit UI.
            a.CreateRibbonTab(tabName);

            //Create first panel on ribbion tab
            var panelElec = a.CreateRibbonPanel(tabName, panelElectrical);


            //Load Names Convert Buttons and Button Data
            var ConvertLoadNames = new PushButtonData("Convert all Load Names to SWE Style", "Convert\nLoad Name", Assembly.GetExecutingAssembly().Location, "SWE_Toolbox.LoadNameConvert")
            {
                ToolTipImage = new BitmapImage(new Uri(@"C:\Users\jms\source\repos\SWE ToolBox - Multiple Version Test\LNCon.png")),
                ToolTip = "Converts Revit default load name style to SWE style"
            };
            var ConvLoadNamesButton = panelElec.AddItem(ConvertLoadNames) as PushButton;
            ConvLoadNamesButton.LargeImage = new BitmapImage(new Uri(@"C:\Users\jms\source\repos\SWE ToolBox - Multiple Version Test\LNCon_32.png"));

            return Result.Succeeded;
        }
        /// <summary>
        /// Called when Revit shutsdown
        /// </summary>
        /// <param name="a">The application.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}

#endregion

