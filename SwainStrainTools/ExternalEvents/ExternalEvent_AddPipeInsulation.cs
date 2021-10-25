using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
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
         var version = app.Application.VersionName;

         PipeInsulationType insulation = new FilteredElementCollector(doc)
            .OfClass(typeof(PipeInsulationType))
             .First(x => x.Name == Form_AddPipeInsulation.insulation)
             as PipeInsulationType;

         double thickness = new double();

#if R2021_2022
         ForgeTypeId unitType = SpecTypeId.PipeInsulationThickness;
         Units units = doc.GetUnits();
         FormatOptions fo = units.GetFormatOptions(unitType);
         var dis = fo.GetUnitTypeId();

         thickness = UnitUtils.ConvertToInternalUnits(Form_AddPipeInsulation.thickness, dis);

#else
         UnitType unitType = UnitType.UT_PipeInsulationThickness;
         Units units = doc.GetUnits();
         FormatOptions fo = units.GetFormatOptions(unitType);
         DisplayUnitType dis = fo.DisplayUnits;

         thickness = UnitUtils.ConvertToInternalUnits(Form_AddPipeInsulation.thickness, dis);

#endif

         try
         {
            using (Transaction t = new Transaction(doc))
            {
               t.Start("Add Insulation to pipes");

               List<ElementId> pinsidtodelete = new List<ElementId>();
               List<ElementId> ptoreinsulate = new List<ElementId>();

               foreach (Pipe p in Form_AddPipeInsulation.pipes)
               {
                  var ins = PipeInsulation.GetInsulationIds(doc, p.Id);

                  if (ins.Count() == 0)
                  {
                     PipeInsulation pipeInsulation = PipeInsulation.Create(doc, p.Id, insulation.Id, thickness);
                  }
                  else
                  {
                     foreach (var f in PipeInsulation.GetInsulationIds(doc, p.Id))
                     {
                        pinsidtodelete.Add(f);
                     }
                     ptoreinsulate.Add(p.Id);
                  }
               }

               t.Commit();

               t.Start("Add Insulation to pipe fittings");
               foreach (var p in Form_AddPipeInsulation.pipefittings)
               {
                  var ins = PipeInsulation.GetInsulationIds(doc, p.Id);

                  if (ins.Count() == 0)
                  {
                     PipeInsulation pipeInsulation = PipeInsulation.Create(doc, p.Id, insulation.Id, thickness);
                  }
                  else
                  {
                     foreach (var f in PipeInsulation.GetInsulationIds(doc, p.Id))
                     {
                        pinsidtodelete.Add(f);
                     }

                     ptoreinsulate.Add(p.Id);
                  }

               }

               t.Commit();


               if (pinsidtodelete.Count > 0)
               {
                  t.Start("Override pipe insulation");

                  doc.Delete(pinsidtodelete);

                  foreach (var p in ptoreinsulate)
                  {
                     try
                     {
                        PipeInsulation pipeInsulation = PipeInsulation.Create(doc, p, insulation.Id, thickness);
                     }
                     catch
                     {

                     }
                  }
                  t.Commit();
               }

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
