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


namespace SwainStrainTools
{
   public class ExternalApplication : IExternalApplication
   {
      public static ExternalApplication thisApp = new ExternalApplication();
      public static Form_AddPipeInsulation MyForm_AddPipeInsulation;
      public static Form_AddDuctInsulation MyForm_AddDuctInsulation;
      public System.Windows.Window Window = new System.Windows.Window();

      public static ExternalEvent_AddPipeInsulation Handler { get; set; } = null;
      public static ExternalEvent ExEvent { get; set; } = null;

      public Result OnStartup(UIControlledApplication application)
      {
         Program.ActivateMachine();

         Program.ValidateLicense();

         if (Program.VALID)
         {
            //Create Ribbon Tab & Panel
            application.CreateRibbonTab("SwainStrain");
            RibbonPanel panel = application.CreateRibbonPanel("SwainStrain", "Insulation");
            string path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);

            //Create Buttons
            PushButtonData button1 = new PushButtonData("Button1", "Add Pipe" + Environment.NewLine + "  Insulation  ", path, "SwainStrainTools.Command_AddPipeInsulation");
            PushButtonData button2 = new PushButtonData("Button2", "Add Duct" + Environment.NewLine + "  Insulation  ", path, "SwainStrainTools.Command_AddDuctInsulation");

            BitmapSource bitmap1 = GetEmbeddedImage("SwainStrainTools.Images.insulationpipe.png");
            button1.Image = bitmap1;
            button1.LargeImage = bitmap1;
            button1.ToolTip = "Tool to add pipe insulation";
            panel.AddItem(button1);

            BitmapSource bitmap2 = GetEmbeddedImage("SwainStrainTools.Images.insulationduct.png");
            button2.Image = bitmap2;
            button2.LargeImage = bitmap2;
            button2.ToolTip = "Tool to add duct insulation";
            panel.AddItem(button2);

         }

         else
         {
            Program.DeactivateMachine();
         }

         MyForm_AddPipeInsulation = null;
         MyForm_AddDuctInsulation = null;
         thisApp = this;

         return Result.Succeeded;
      }

      public Result OnShutdown(UIControlledApplication application)
      {
         Program.DeactivateMachine();

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

   class Program
   {
      // This is your Keygen account ID.
      //
      // Available at: https://app.keygen.sh/settings
      const string KEYGEN_ACCOUNT_ID = "b611c793-06e0-4deb-94f0-8ae6984d937c";
      const string LICENSE_KEY = "CC3F8C-4244CC-B53E25-9CA6EE-46A8C7-V3";
      const string LICENSE_ID = "18a79427-f924-4c93-a05f-c165319f05c7";
      public static string FINGERPRINT = FingerPrint.Value();
      public static bool VALID = false;

      public static void ValidateLicense()
      {
         var keygen = new RestClient(string.Format("https://api.keygen.sh/v1/accounts/{0}", KEYGEN_ACCOUNT_ID));
         var request = new RestRequest("licenses/actions/validate-key", Method.POST);

         request.AddHeader("Content-Type", "application/vnd.api+json");
         request.AddHeader("Accept", "application/vnd.api+json");
         request.AddJsonBody(new
         {
            meta = new
            {
               key = LICENSE_KEY,
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
               Console.WriteLine("[ERROR] Status={0} Errors={1}", response.StatusCode, errors);

               Environment.Exit(1);
            }
         }

         var license = (Dictionary<string, object>)response.Data["data"];
         var meta = (Dictionary<string, object>)response.Data["meta"];

         if ((bool)meta["valid"])
         {
            //Console.WriteLine("[INFO] License={0} Valid={1} ValidationCode={2}", license["id"], meta["detail"], meta["constant"]);
            VALID = true;
         }
         else
         {
            //Console.WriteLine(
            //  "[INFO] License={0} Invalid={1} ValidationCode={2}",
            //  license != null ? license["id"] : "N/A",
            //  meta["detail"],
            //  meta["constant"]
            //);
            VALID = false;

         }
      }

      public static void ActivateMachine()
      {
         var keygen = new RestClient(string.Format("https://api.keygen.sh/v1/accounts/{0}", KEYGEN_ACCOUNT_ID));
         var request = new RestRequest("machines", Method.POST);

         request.AddHeader("Content-Type", "application/vnd.api+json");
         request.AddHeader("Accept", "application/vnd.api+json");
         request.AddHeader("Authorization", string.Format("License {0}", LICENSE_KEY));

         request.AddJsonBody(new
         {
            data = new
            {
               type = "machines",
               attributes = new
               {
                  fingerprint = FINGERPRINT,
                  platform = "macOS",
                  name = "Office MacBook Pro"
               },
               relationships = new
               {
                  license = new
                  {
                     data = new
                     {
                        type = "licenses",
                        id = LICENSE_ID
                     }
                  }
               }
            }
         });

         var response = keygen.Execute(request);

      }

      public static void DeactivateMachine()
      {
         var keygen = new RestClient(string.Format("https://api.keygen.sh/v1/accounts/{0}", KEYGEN_ACCOUNT_ID));
         var request = new RestRequest(
           string.Format("machines/{0}", FINGERPRINT),
           Method.DELETE
         );

         request.AddHeader("Accept", "application/vnd.api+json");
         request.AddHeader("Authorization", string.Format("License {0}", LICENSE_KEY));

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
      private static string identifier    (string wmiClass, string wmiProperty, string wmiMustBeTrue)
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
