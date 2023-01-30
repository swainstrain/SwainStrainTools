﻿#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
#endregion

namespace SwainStrainTools
{
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

   public class Command_Settings : IExternalCommand
   {
      public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
      {
         try
         {
            ExternalApplication.thisApp.ShowForm_LicenseKey();

            return Result.Succeeded;
         }
         catch (Exception ex)
         {
            System.Windows.Forms.MessageBox.Show("Error! " + ex);
            return Result.Failed;
         }
      }
   }

   public class Command_Settings_Availability : IExternalCommandAvailability
   {
      public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
      {
         //if (Properties.Settings.Default.LicenseKEY != "LicenseKEYValue")
         //{
         //   return false;
         //}

         return true;
      }
   }

}
