#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
#endregion

namespace SwainStrainTools
{
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class Command_AddPipeInsulation : IExternalCommand
   {
      public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
      {
         return Execute(commandData.Application);
      }


      public Result Execute(UIApplication uiapp)
      {
         try
         {
            ExternalApplication.thisApp.ShowForm_AddPipeInsulation(uiapp);
            return Result.Succeeded;
         }
         catch (Exception ex)
         {
            System.Windows.Forms.MessageBox.Show("Error! " + ex);
            return Result.Failed;
         }

      }

   }
}



