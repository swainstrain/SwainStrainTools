using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SwainStrainTools.UI
{
   /// <summary>
   /// Interaction logic for UserControl1.xaml
   /// </summary>
   public partial class Form_AddDuctInsulation : Window
   {
      private ExternalEvent m_ExEvent;
      private ExternalEvent_AddDuctInsulation m_Handler;

      UIApplication _uiapp;
      UIDocument _uidoc;
      Autodesk.Revit.ApplicationServices.Application _app;
      Document _doc;

      public static IList<Element> ducts;
      public static IList<Element> ductfittings;
      public static string insulation;
      public static double thickness;

      public Form_AddDuctInsulation(UIApplication uiApp, ExternalEvent exEvent, ExternalEvent_AddDuctInsulation handler, ViewModel_AddDuctIns vm)
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

         List<Element> insulations = new FilteredElementCollector(_doc)
            .WhereElementIsElementType()
            .OfCategory(BuiltInCategory.OST_DuctInsulations)
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
             .CreateEqualsRule(new ElementId(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM)
             , system
             , false));

         ducts = new FilteredElementCollector(_doc)
             .WhereElementIsNotElementType()
             .OfCategory(BuiltInCategory.OST_DuctCurves)
             .WherePasses(filter)
             .ToElements();

         ductfittings = new FilteredElementCollector(_doc)
             .WhereElementIsNotElementType()
             .OfCategory(BuiltInCategory.OST_DuctFitting)
             .WherePasses(filter)
             .ToElements();

         //TaskDialog.Show("ducts", ducts.Count.ToString());
         //TaskDialog.Show("ductfittings", ductfittings.Count.ToString());

         m_ExEvent.Raise();
      }

      private void CancelBtn_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }

   }
}
