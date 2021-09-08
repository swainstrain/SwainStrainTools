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
            //var view = new Form_AddPipeInsulation(new UI.ViewModel(uiapp), ExternalApplication.Handler);
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



