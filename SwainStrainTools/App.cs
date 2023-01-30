using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using SwainStrainTools.UI;
using RestSharp;
using System.Collections.Generic;
using System.Management;
using System.Security.Cryptography;
using System.Security;
using System.Collections;
using System.Text;
using System.Windows.Forms;


namespace SwainStrainTools
{
   public class ExternalApplication : IExternalApplication
   {
      public static ExternalApplication thisApp = new ExternalApplication();
      public static Form_AddPipeInsulation MyForm_AddPipeInsulation;
      public static Form_AddDuctInsulation MyForm_AddDuctInsulation;
      public static Form_Settings MyForm_Settings;
      public System.Windows.Window Window = new System.Windows.Window();

      public static ExternalEvent_AddPipeInsulation Handler { get; set; } = null;
      public static ExternalEvent ExEvent { get; set; } = null;

      public static bool VALID = false;
      public static bool FLOATING =false;
      public static bool ISACTIVE =false;


      public Result OnStartup(UIControlledApplication application)
      {
         //Program.ActivateMachine();

         if (Properties.Settings.Default.LicenseKEY != "" && Properties.Settings.Default.LicenseID != "" && result1 != "" && result2 != "")
         {
            try
            {
               Program.ValidateLicenseByKey();
               Program.ActivateMachine();
               Program.ValidateLicenseByKey();
            }
            catch
            {
               VALID = false;
               FLOATING = false;
               ISACTIVE = false;
            }
         }
         else
         {
            VALID = false;
            FLOATING = false;
            ISACTIVE = false;

         }

         //Create Ribbon Tab & Panel
         application.CreateRibbonTab("SwainStrain");
         RibbonPanel panel = application.CreateRibbonPanel("SwainStrain", "Insulation");
         string path = Assembly.GetExecutingAssembly().Location;
         var directory = Path.GetDirectoryName(path);

         //Create Buttons
         PushButtonData button1 = new PushButtonData("Button1", "Add Pipe" + Environment.NewLine + "  Insulation  ", path, "SwainStrainTools.Command_AddPipeInsulation");
         PushButtonData button2 = new PushButtonData("Button2", "Add Duct" + Environment.NewLine + "  Insulation  ", path, "SwainStrainTools.Command_AddDuctInsulation");
         PushButtonData button3 = new PushButtonData("Button3", "Settings", path, "SwainStrainTools.Command_Settings");

         BitmapSource bitmap1 = GetEmbeddedImage("SwainStrainTools.Images.insulationpipe.png");
         button1.Image = bitmap1;
         button1.LargeImage = bitmap1;
         button1.ToolTip = "Tool to add pipe insulation";
         button1.AvailabilityClassName = "SwainStrainTools.Command_AddDuctInsulation_Availability";
         panel.AddItem(button1);

         BitmapSource bitmap2 = GetEmbeddedImage("SwainStrainTools.Images.insulationduct.png");
         button2.Image = bitmap2;
         button2.LargeImage = bitmap2;
         button2.ToolTip = "Tool to add duct insulation";
         button2.AvailabilityClassName = "SwainStrainTools.Command_AddDuctInsulation_Availability";
         panel.AddItem(button2);

         button3.AvailabilityClassName= "SwainStrainTools.Command_Settings_Availability";
         panel.AddItem(button3);

         MyForm_AddPipeInsulation = null;
         MyForm_AddDuctInsulation = null;
         MyForm_Settings = null;
         thisApp = this;

         return Result.Succeeded;
      }

      public Result OnShutdown(UIControlledApplication application)
      {
         if (VALID && FLOATING)
         {
            Program.DeactivateMachine();
         }

         if (Window != null)
         {
            Window.Close();
         }
         return Result.Succeeded;
      }

      public void ShowForm_AddPipeInsulation(UIApplication uiapp)
      {
         ExternalEvent_AddPipeInsulation handler = new ExternalEvent_AddPipeInsulation();
         ExternalEvent exEvent = ExternalEvent.Create(handler);
         ViewModel_AddPipeIns vm = new ViewModel_AddPipeIns(uiapp);
         MyForm_AddPipeInsulation = new Form_AddPipeInsulation(uiapp, exEvent, handler, vm);
         MyForm_AddPipeInsulation.Show();
      }

      public void ShowForm_AddDuctInsulation(UIApplication uiapp)
      {
         ExternalEvent_AddDuctInsulation handler = new ExternalEvent_AddDuctInsulation();
         ExternalEvent exEvent = ExternalEvent.Create(handler);
         ViewModel_AddDuctIns vm = new ViewModel_AddDuctIns(uiapp);
         MyForm_AddDuctInsulation = new Form_AddDuctInsulation(uiapp, exEvent, handler, vm);
         MyForm_AddDuctInsulation.Show();
      }
      public void ShowForm_LicenseKey()
      {
         MyForm_Settings = new Form_Settings();
         MyForm_Settings.ShowDialog();
      }

      public static BitmapSource GetEmbeddedImage(string name)
      {
         try
         {
            Assembly a = Assembly.GetExecutingAssembly();
            Stream s = a.GetManifestResourceStream(name);
            return BitmapFrame.Create(s);
         }
         catch
         {
            return null;
         }
      }
   }

   public static class Program
   {
      const string KEYGEN_ACCOUNT_ID = "b611c793-06e0-4deb-94f0-8ae6984d937c";
      //public static string LICENSE_KEY = Properties.Settings.Default.LicenseKEY;
      //const string LICENSE_ID = "18a79427-f924-4c93-a05f-c165319f05c7";
      public static string FINGERPRINT = FingerPrint.Value();
      public static string licenseid="";

      public static void ValidateLicenseByKey()
      {
         var keygen = new RestClient(string.Format("https://api.keygen.sh/v1/accounts/{0}", KEYGEN_ACCOUNT_ID));
         var request = new RestRequest("licenses/actions/validate-key", Method.POST);

         request.AddHeader("Content-Type", "application/vnd.api+json");
         request.AddHeader("Accept", "application/vnd.api+json");
         request.AddJsonBody(new
         {
            meta = new
            {
               key = Properties.Settings.Default.LicenseKEY,
               scope = new
               {
                  fingerprint = FINGERPRINT
               }
            }
         });

         var response = keygen.Execute<Dictionary<string, object>>(request);
         if (response.Data.ContainsKey("errors"))
         {
            var errors = (RestSharp.JsonArray)response.Data["errors"];
            if (errors != null)
            {
               //Console.WriteLine("[ERROR] Status={0} Errors={1}", response.StatusCode, errors);
               MessageBox.Show(string.Format("[ERROR] Status={0} Errors={1} ", response.StatusCode, errors));

               Environment.Exit(1);
            }
         }

         var data = (Dictionary<string, object>)response.Data["data"];
         var meta = (Dictionary<string, object>)response.Data["meta"];
         licenseid = (string)data["id"];

         if ((bool)meta["valid"])
         {
            //MessageBox.Show(string.Format("[INFO] License={0} Valid={1} ValidationCode={2}", license["id"], meta["detail"], meta["constant"]));
            ExternalApplication.VALID = true;
            var attributes = (Dictionary<string, object>)data["attributes"];

            if ((bool)attributes["floating"] == true)
            {
               ExternalApplication.FLOATING = true;
            }
         }

         else
         {
            //MessageBox.Show(string.Format(
            //  "[INFO] License={0} Invalid={1} ValidationCode={2}",
            //  license != null ? license["id"] : "N/A",
            //  meta["detail"],
            //  meta["constant"]
            //));
            ExternalApplication.VALID = false;
            ExternalApplication.ISACTIVE = false;
         }
      }

      public static void ActivateMachine()
      {
         var keygen = new RestClient(string.Format("https://api.keygen.sh/v1/accounts/{0}", KEYGEN_ACCOUNT_ID));
         var request = new RestRequest("machines", Method.POST);

         request.AddHeader("Content-Type", "application/vnd.api+json");
         request.AddHeader("Accept", "application/vnd.api+json");
         request.AddHeader("Authorization", string.Format("License {0}", Properties.Settings.Default.LicenseKEY));

         request.AddJsonBody(new
         {
            data = new
            {
               type = "machines",
               attributes = new
               {
                  fingerprint = FINGERPRINT,
               },

               relationships = new
               {
                  license = new
                  {
                     data = new
                     {
                        type = "licenses",
                        id = Program.licenseid
                     }
                  }
               }
            }
         });

         var response = keygen.Execute(request);

         if (response.IsSuccessful)
         {
            ExternalApplication.ISACTIVE = true;
         }
         else
         {
            ExternalApplication.ISACTIVE = false;
         }
      }

      public static void DeactivateMachine()
      {
         var keygen = new RestClient(string.Format("https://api.keygen.sh/v1/accounts/{0}", KEYGEN_ACCOUNT_ID));
         var request = new RestRequest(
           string.Format("machines/{0}", FINGERPRINT),
           Method.DELETE
         );

         request.AddHeader("Accept", "application/vnd.api+json");
         request.AddHeader("Authorization", string.Format("License {0}", Properties.Settings.Default.LicenseKEY));

         var response = keygen.Execute(request);


      }

   }

   public class FingerPrint
   {
      private static string fingerPrint = string.Empty;
      public static string Value()
      {
         if (string.IsNullOrEmpty(fingerPrint))
         {
            fingerPrint = GetHash("CPU >> " + cpuId() + "\nBIOS >> " +
              biosId() + "\nBASE >> " + baseId() +
              videoId() + "\nMAC >> " + macId()
                                 );
         }
         return fingerPrint;
      }
      private static string GetHash(string s)
      {
         MD5 sec = new MD5CryptoServiceProvider();
         ASCIIEncoding enc = new ASCIIEncoding();
         byte[] bt = enc.GetBytes(s);
         return GetHexString(sec.ComputeHash(bt));
      }
      private static string GetHexString(byte[] bt)
      {
         string s = string.Empty;
         for (int i = 0; i < bt.Length; i++)
         {
            byte b = bt[i];
            int n, n1, n2;
            n = (int)b;
            n1 = n & 15;
            n2 = (n >> 4) & 15;
            if (n2 > 9)
               s += ((char)(n2 - 10 + (int)'A')).ToString();
            else
               s += n2.ToString();
            if (n1 > 9)
               s += ((char)(n1 - 10 + (int)'A')).ToString();
            else
               s += n1.ToString();
            if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
         }
         return s;
      }
      #region Original Device ID Getting Code
      //Return a hardware identifier
      private static string identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
      {
         string result = "";
         System.Management.ManagementClass mc =
   new System.Management.ManagementClass(wmiClass);
         System.Management.ManagementObjectCollection moc = mc.GetInstances();
         foreach (System.Management.ManagementObject mo in moc)
         {
            if (mo[wmiMustBeTrue].ToString() == "True")
            {
               //Only get the first one
               if (result == "")
               {
                  try
                  {
                     result = mo[wmiProperty].ToString();
                     break;
                  }
                  catch
                  {
                  }
               }
            }
         }
         return result;
      }
      //Return a hardware identifier
      private static string identifier(string wmiClass, string wmiProperty)
      {
         string result = "";
         System.Management.ManagementClass mc =
   new System.Management.ManagementClass(wmiClass);
         System.Management.ManagementObjectCollection moc = mc.GetInstances();
         foreach (System.Management.ManagementObject mo in moc)
         {
            //Only get the first one
            if (result == "")
            {
               try
               {
                  result = mo[wmiProperty].ToString();
                  break;
               }
               catch
               {
               }
            }
         }
         return result;
      }
      private static string cpuId()
      {
         //Uses first CPU identifier available in order of preference
         //Don't get all identifiers, as it is very time consuming
         string retVal = identifier("Win32_Processor", "UniqueId");
         if (retVal == "") //If no UniqueID, use ProcessorID
         {
            retVal = identifier("Win32_Processor", "ProcessorId");
            if (retVal == "") //If no ProcessorId, use Name
            {
               retVal = identifier("Win32_Processor", "Name");
               if (retVal == "") //If no Name, use Manufacturer
               {
                  retVal = identifier("Win32_Processor", "Manufacturer");
               }
               //Add clock speed for extra security
               retVal += identifier("Win32_Processor", "MaxClockSpeed");
            }
         }
         return retVal;
      }
      //BIOS Identifier
      private static string biosId()
      {
         return identifier("Win32_BIOS", "Manufacturer")
         + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
         + identifier("Win32_BIOS", "IdentificationCode")
         + identifier("Win32_BIOS", "SerialNumber")
         + identifier("Win32_BIOS", "ReleaseDate")
         + identifier("Win32_BIOS", "Version");
      }
      //Main physical hard drive ID
      private static string diskId()
      {
         return identifier("Win32_DiskDrive", "Model")
         + identifier("Win32_DiskDrive", "Manufacturer")
         + identifier("Win32_DiskDrive", "Signature")
         + identifier("Win32_DiskDrive", "TotalHeads");
      }
      //Motherboard ID
      private static string baseId()
      {
         return identifier("Win32_BaseBoard", "Model")
         + identifier("Win32_BaseBoard", "Manufacturer")
         + identifier("Win32_BaseBoard", "Name")
         + identifier("Win32_BaseBoard", "SerialNumber");
      }
      //Primary video controller ID
      private static string videoId()
      {
         return identifier("Win32_VideoController", "DriverVersion")
         + identifier("Win32_VideoController", "Name");
      }
      //First enabled network card ID
      private static string macId()
      {
         return identifier("Win32_NetworkAdapterConfiguration",
         "MACAddress", "IPEnabled");
      }
      #endregion
   }

}
