using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using SwainStrainTools.UI;

namespace SwainStrainTools
{
   [Transaction(TransactionMode.Manual)]
   public class ExternalEvent_AddPipeInsulation : IExternalEventHandler
   {
      public void Execute(UIApplication app)
      {
         UIDocument uidoc = app.ActiveUIDocument;
         Document doc = uidoc.Document;

         PipeInsulationType insulation = new FilteredElementCollector(doc)
            .OfClass(typeof(PipeInsulationType))
             .First(x => x.Name == Form_AddPipeInsulation.insulation)
             as PipeInsulationType;

         double thickness = UnitUtils.ConvertToInternalUnits(Form_AddPipeInsulation.thickness, UnitTypeId.Millimeters);         

         try
         {
            using (Transaction t = new Transaction(doc))
            {
               t.Start("Add Insulation to pipes");
               foreach (Pipe p in Form_AddPipeInsulation.pipes)
               {
                  PipeInsulation pipeInsulation = PipeInsulation.Create(doc, p.Id, insulation.Id, thickness);
               }

               t.Commit();

               t.Start("Add Insulation to pipe fittings");
               foreach (var p in Form_AddPipeInsulation.pipefittings)
               {
                  PipeInsulation pipeInsulation = PipeInsulation.Create(doc, p.Id, insulation.Id, thickness);
               }

               t.Commit();

            }

            TaskDialog.Show("Success", "Pipe insulation added!");

         }
         catch (Exception ex)
         {
            System.Windows.MessageBox.Show("Failed to execute the external event.\n"
                + ex.Message, "Execute Event", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
         }
      }

      public string GetName()
      {
         return "External Event - Add Pipe Insulation";
      }
   }
}
