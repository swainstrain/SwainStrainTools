using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using SwainStrainTools.UI;


namespace SwainStrainTools
{
   public class ExternalEvent_AddDuctInsulation : IExternalEventHandler
   {
      public void Execute(UIApplication app)
      {
         UIDocument uidoc = app.ActiveUIDocument;
         Document doc = uidoc.Document;

         DuctInsulationType insulation = new FilteredElementCollector(doc)
            .OfClass(typeof(DuctInsulationType))
             .First(x => x.Name == Form_AddDuctInsulation.insulation)
             as DuctInsulationType;

         double thickness = UnitUtils.ConvertToInternalUnits(Form_AddDuctInsulation.thickness, UnitTypeId.Millimeters);

         try
         {
            using (Transaction t = new Transaction(doc))
            {
               t.Start("Add Insulation to ducts");
               foreach (var d in Form_AddDuctInsulation.ducts)
               {
                   DuctInsulation ductInsulation = DuctInsulation.Create(doc, d.Id, insulation.Id, thickness);
               }

               t.Commit();

               t.Start("Add Insulation to duct fittings");
               foreach (var d in Form_AddDuctInsulation.ductfittings)
               {
                  DuctInsulation ductInsulation = DuctInsulation.Create(doc, d.Id, insulation.Id, thickness);
               }

               t.Commit();

            }

            TaskDialog.Show("Success", "Duct insulation added!");

         }
         catch (Exception ex)
         {
            System.Windows.MessageBox.Show("Failed to execute the external event.\n"
                + ex.Message, "Execute Event", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
         }
      }

      public string GetName()
      {
         return "External Event - Add Duct Insulation";
      }
   }
}
