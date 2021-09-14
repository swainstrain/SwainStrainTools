using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SwainStrainTools
{

   public partial class Form_AddPipeInsulation : Window
   {
      private ExternalEvent m_ExEvent;
      private ExternalEvent_AddPipeInsulation m_Handler;

      UIApplication _uiapp;
      UIDocument _uidoc;
      Autodesk.Revit.ApplicationServices.Application _app;
      Document _doc;

      public static List<Element> pipes = new List<Element>();
      public static List<Element> pipefittings = new List<Element>();
      public static string insulation;
      public static double thickness;

      public Form_AddPipeInsulation(UIApplication uiApp, ExternalEvent exEvent, ExternalEvent_AddPipeInsulation handler, UI.ViewModel vm)
      {
         InitializeComponent();
         _uiapp = uiApp;
         _uidoc = uiApp.ActiveUIDocument;
         _app = _uiapp.Application;
         _doc = _uidoc.Document;

         m_ExEvent = exEvent;
         m_Handler = handler;

         this.DataContext = vm;

         List<string> ins = new List<string>();
         //List<string> sys = new List<string>();


         List<Element> insulations = new FilteredElementCollector(_doc)
            .WhereElementIsElementType()
            .OfCategory(BuiltInCategory.OST_PipeInsulations)
            .ToList<Element>();

         foreach (Element i in insulations)
         {
            ElementType type = i as ElementType;
            ins.Add(i.Name);
         }

         ins.Sort();

         CMB_insulations.ItemsSource = ins;
      }

      private void OKBtn_Click(object sender, RoutedEventArgs e)
      {
         string system = CMB_systems.Text;
         string diameter = CMB_DN.Text;
         insulation = CMB_insulations.Text;
         thickness = double.Parse(TXT_thickness.Text.ToString());

         if (system == "" || insulation == "")
         {
            TaskDialog.Show("Error", "Please, select the piping system and insulation");
            return;
         }

         if (thickness == 0)
         {
            TaskDialog.Show("Error", "Please, enter the insulation thickness");
            return;
         }

         ElementParameterFilter filter = new ElementParameterFilter(ParameterFilterRuleFactory
             .CreateEqualsRule(new ElementId(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM)
             , system
             , false));

         IList<Element> allpipes = new FilteredElementCollector(_doc)
             .WhereElementIsNotElementType()
             .OfCategory(BuiltInCategory.OST_PipeCurves)
             .WherePasses(filter)
             .ToElements();

         IList<Element> allfittings = new FilteredElementCollector(_doc)
             .WhereElementIsNotElementType()
             .OfCategory(BuiltInCategory.OST_PipeFitting)
             .WherePasses(filter)
             .ToElements();

         foreach (var p in allpipes) if (p.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsValueString() == diameter)
            {
               pipes.Add(p);
            }

         foreach (var p in allfittings) 
         {
            try
            {
               if (p.LookupParameter("Nominal Diameter").AsValueString() == diameter)
               {
                  pipefittings.Add(p);
               }

            }
            catch
            {

            }
         }

         m_ExEvent.Raise();
      }

      private void CancelBtn_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }
}
