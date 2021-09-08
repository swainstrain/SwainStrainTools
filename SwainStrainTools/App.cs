using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using SwainStrainTools.UI;



namespace SwainStrainTools
{
   public class ExternalApplication : IExternalApplication
   {
      public static ExternalApplication thisApp = new ExternalApplication();
      public static Form_AddPipeInsulation MyForm_AddPipeInsulation;
      public System.Windows.Window Window = new System.Windows.Window();

      public static ExternalEvent_AddPipeInsulation Handler { get; set; } = null;
      public static ExternalEvent ExEvent { get; set; } = null;

      public Result OnStartup(UIControlledApplication application)
      {
         //Create Ribbon Tab & Panel
         //application.CreateRibbonTab("SwainStrain");
         RibbonPanel panel = application.CreateRibbonPanel("Tools");
         string path = Assembly.GetExecutingAssembly().Location;
         var directory = Path.GetDirectoryName(path);

         //Create Buttons
         PushButtonData button1 = new PushButtonData("Button1", "Add Pipe" + Environment.NewLine + "  Insulation  ", path, "SwainStrainTools.Command_AddPipeInsulation");

         BitmapSource bitmap1 = GetEmbeddedImage("SwainStrainTools.Images.insulation.png");
         button1.Image = bitmap1;
         button1.LargeImage = bitmap1;
         button1.ToolTip = "Tool to add pipe insulation";
         panel.AddItem(button1);


         //Handler = new ExternalEvent_AddPipeInsulation();
         //ExEvent = ExternalEvent.Create(Handler);


         MyForm_AddPipeInsulation = null;
         thisApp = this;
         return Result.Succeeded;
      }

      public Result OnShutdown(UIControlledApplication application)
      {
         if (Window != null)
         {
            Window.Close();
         }
         return Result.Succeeded;
      }


      public void ShowForm_AddPipeInsulation(UIApplication uiapp)
      {
         ExternalEvent_AddPipeInsulation handler = new ExternalEvent_AddPipeInsulation();
         ExternalEvent exEvent = ExternalEvent.Create(handler);
         ViewModel vm = new ViewModel(uiapp);
         MyForm_AddPipeInsulation = new Form_AddPipeInsulation(uiapp, exEvent, handler,vm);
         MyForm_AddPipeInsulation.Show();
      }

      public static BitmapSource GetEmbeddedImage(string name)
      {
         try
         {
            Assembly a = Assembly.GetExecutingAssembly();
            Stream s = a.GetManifestResourceStream(name);
            return BitmapFrame.Create(s);
         }
         catch
         {
            return null;
         }
      }


   }
}
