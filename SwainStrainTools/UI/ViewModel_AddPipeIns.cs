using System;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SwainStrainTools.Utilities;
using System.Linq;
using System.Collections.Generic; // List<>


namespace SwainStrainTools.UI
{
   public class ViewModel_AddPipeIns : BaseViewModel
   {
      public RelayCommand WindowLoaded { get; set; }
      public RelayCommand WindowClosed { get; set; }
      public RelayCommand CancelCommand { get; set; }
      public RelayCommand ApplyCommand { get; set; }

      //Private properties
      private static UIApplication _uiapp = null;
      private static Document _doc = null;
      private List<PipeSystem> _PipeSystemsList;
      private string _SelectedPipeSystem;
      private List<DiameterNominal> _DNList;
      private string _SelectedDN;

      // Public Properties - Used for binding with the View

      public static ViewModel_AddPipeIns Instance { get; set; }
      public static Document Doc { get { return _doc; } set { _doc = value; } }
      public static UIApplication Uiapp { get { return _uiapp; } set { _uiapp = value; } }
      public static bool IsOpen { get; private set; } = false;

      public static string ColorSettingsFiles = string.Empty;

      public List<PipeSystem> PipeSystemsList
      {
         get { return _PipeSystemsList; }
         set
         {
            _PipeSystemsList = value;
            OnPropertyChanged("PipeSystemsList");
         }
      }

      public string SelectedPipeSystem
      {
         get { return _SelectedPipeSystem; }
         set
         {
            _SelectedPipeSystem = value;
            OnPropertyChanged("SelectedPipeSystem");
            OnPropertyChanged("AllowDNSelection"); // Trigger Enable/Disable UI element when particular system is selected
            getDNList(); // Generate a new list of DN based on a selected system
         }
      }

      public List<DiameterNominal> DNList
      {
         get { return _DNList; }
         set
         {
            _DNList = value;
            OnPropertyChanged("DNList");
         }
      }
      public string SelectedDN
      {
         get { return _SelectedDN; }
         set
         {
            _SelectedDN = value;
            OnPropertyChanged("SelectedDN");
            OnPropertyChanged("AllowInsulationSelection");
            //getTypeList(); // 
         }
      }

      public bool AllowDNSelection
      {
         get { return (SelectedPipeSystem != null); }
      }

      public bool AllowInsulationSelection
      {
         get { return (SelectedDN != null); }
      }

      public ViewModel_AddPipeIns(UIApplication uiapp)
      {
         Instance = this;
         _uiapp = uiapp;
         _doc = _uiapp.ActiveUIDocument.Document;

         // Instantiate, get a list of countries from the Model
         PipeSystem _PipeSystem = new PipeSystem();
         PipeSystemsList = _PipeSystem.getPipeSystems(uiapp);

         try
         {
            WindowLoaded = new RelayCommand(param => this.LoadedExecuted(param));
            WindowClosed = new RelayCommand(param => this.ClosedExecuted(param));
            CancelCommand = new RelayCommand(param => this.CancelExecuted(param));
         }
         catch (Exception ex)
         {
            string message = ex.Message;
         }

      }

      private void getDNList()
      {
         // Instantiate, get a list of DN based on selected pipesystem
         DiameterNominal _DiameterNominal = new DiameterNominal();
         DNList = _DiameterNominal.getDNbyPipeSystem(_uiapp, SelectedPipeSystem);
      }

      #region Public methods
      //Get all infos before show modeless form
      public bool DisplayUI()
      {
         bool result = false;
         try
         {
            result = true;
         }
         catch (Exception ex)
         {
            MessageBox.Show("Failed to display UI components\n" + ex.Message, "TD: DisplayUI", MessageBoxButton.OK, MessageBoxImage.Warning);
         }
         return result;
      }

      public class PipeSystem
      {
         public string PipeSystemName { get; set; }

         public List<PipeSystem> getPipeSystems(UIApplication app)
         {
            Document doc = app.ActiveUIDocument.Document;

            List<String> mySystems = new List<String>();

            var systems = new FilteredElementCollector(doc)
               .WhereElementIsElementType()
               .OfCategory(BuiltInCategory.OST_PipingSystem)
               .ToList<Element>();

            foreach (var s in systems)
            {
               mySystems.Add(s.Name);
            }

            mySystems.Sort();

            List<PipeSystem> returnSystems = new List<PipeSystem>();

            foreach (var m in mySystems)
            {
               returnSystems.Add(new PipeSystem() { PipeSystemName = m });
            }
            return returnSystems;
         }

      }

      public class DiameterNominal
      {
         public string PipeSystemName { get; set; }
         public string DN { get; set; }

         public List<DiameterNominal> getDNbyPipeSystem(UIApplication app, string pipeSystemName)
         {
            Document doc = app.ActiveUIDocument.Document;

            ElementParameterFilter filter = new ElementParameterFilter(ParameterFilterRuleFactory
                .CreateEqualsRule(new ElementId(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM)
                , pipeSystemName
                , false));

            IList<Element> pipes = new FilteredElementCollector(_doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .WherePasses(filter)
                .ToElements();

            List<DiameterNominal> returnDNs = new List<DiameterNominal>();

            List<string> diameters = new List<string>();

            foreach (var c in pipes)
            {
               string pipeDN = c.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsValueString();

               if(!diameters.Contains(pipeDN))
               {
                  diameters.Add(pipeDN);
                  returnDNs.Add(new DiameterNominal() { PipeSystemName = pipeSystemName, DN = pipeDN });
               }

            }

            List<DiameterNominal> SortedList = returnDNs.OrderBy(o => int.Parse(o.DN.Replace(" mm",""))).ToList();

            return SortedList;
         }

      }

      #endregion //Public methods

      private void LoadedExecuted(object param)
      {
         IsOpen = true;
      }
      private void ClosedExecuted(object param)
      {
         IsOpen = false;
      }
      private void CancelExecuted(object param)
      {
         var win = param as Window;
         win.Close();
      }

   }
}

