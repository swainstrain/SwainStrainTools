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
         try
         {
            ExternalApplication.thisApp.ShowForm_AddPipeInsulation(commandData.Application);
            return Result.Succeeded;
         }
         catch (Exception ex)
         {
            message = ex.Message;
            return Result.Failed;
         }

      }

   }
}



