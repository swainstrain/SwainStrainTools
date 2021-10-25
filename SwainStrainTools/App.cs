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
      public static Form_AddDuctInsulation MyForm_AddDuctInsulation;
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

         BitmapSource bitmap1 = GetEmbeddedImage("SwainStrainTools.Images.insulation2.png");
         button1.Image = bitmap1;
         button1.LargeImage = bitmap1;
         button1.ToolTip = "Tool to add pipe insulation";
         panel.AddItem(button1);

         MyForm_AddPipeInsulation = null;
         MyForm_AddDuctInsulation = null;
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
         ViewModel_AddPipeIns vm = new ViewModel_AddPipeIns(uiapp);
         MyForm_AddPipeInsulation = new Form_AddPipeInsulation(uiapp, exEvent, handler,vm);
         MyForm_AddPipeInsulation.Show();
      }

      public void ShowForm_AddDuctInsulation(UIApplication uiapp)
      {
         ExternalEvent_AddDuctInsulation handler = new ExternalEvent_AddDuctInsulation();
         ExternalEvent exEvent = ExternalEvent.Create(handler);
         ViewModel_AddDuctIns vm = new ViewModel_AddDuctIns(uiapp);
         MyForm_AddDuctInsulation = new Form_AddDuctInsulation(uiapp, exEvent, handler, vm);
         MyForm_AddDuctInsulation.Show();
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
