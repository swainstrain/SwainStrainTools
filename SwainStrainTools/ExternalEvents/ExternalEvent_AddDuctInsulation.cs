using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
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

         double thickness = new double();

#if R2021_2022
         ForgeTypeId unitType = SpecTypeId.PipeInsulationThickness;
         Units units = doc.GetUnits();
         FormatOptions fo = units.GetFormatOptions(unitType);
         var dis = fo.GetUnitTypeId();

         thickness = UnitUtils.ConvertToInternalUnits(Form_AddDuctInsulation.thickness, dis);

#else
         UnitType unitType = UnitType.UT_PipeInsulationThickness;
         Units units = doc.GetUnits();
         FormatOptions fo = units.GetFormatOptions(unitType);
         DisplayUnitType dis = fo.DisplayUnits;

         thickness = UnitUtils.ConvertToInternalUnits(Form_AddDuctInsulation.thickness, dis);
#endif


         try
         {
            using (Transaction t = new Transaction(doc))
            {
               t.Start("Add Insulation to ducts");

               List<ElementId> dinsidtodelete = new List<ElementId>();
               List<ElementId> dtoreinsulate = new List<ElementId>();

               foreach (Duct d in Form_AddDuctInsulation.ducts)
               {
                  var ins= DuctInsulation.GetInsulationIds(doc, d.Id);

                  if(ins.Count()==0)
                  {
                     DuctInsulation ductInsulation = DuctInsulation.Create(doc, d.Id, insulation.Id, thickness);

                  }
                  else
                  {
                     foreach (var f in DuctInsulation.GetInsulationIds(doc, d.Id))
                     {
                        dinsidtodelete.Add(f);
                     }
                     dtoreinsulate.Add(d.Id);
                  }

               }

               t.Commit();

               t.Start("Add Insulation to duct fittings");
               foreach (var d in Form_AddDuctInsulation.ductfittings)
               {
                  var ins = DuctInsulation.GetInsulationIds(doc, d.Id);

                  if (ins.Count() == 0)
                  {
                     DuctInsulation ductInsulation = DuctInsulation.Create(doc, d.Id, insulation.Id, thickness);

                  }
                  else
                  {
                     foreach (var f in DuctInsulation.GetInsulationIds(doc, d.Id))
                     {
                        dinsidtodelete.Add(f);
                     }

                     dtoreinsulate.Add(d.Id);
                  }
               }

               t.Commit();

               if (dinsidtodelete.Count > 0)
               {
                  t.Start("Override pipe insulation");

                  doc.Delete(dinsidtodelete);

                  foreach (var d in dtoreinsulate)
                  {
                     try
                     {
                        DuctInsulation ductInsulation = DuctInsulation.Create(doc, d, insulation.Id, thickness);
                     }
                     catch
                     {

                     }
                  }
                  t.Commit();
               }


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
