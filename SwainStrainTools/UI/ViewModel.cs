using System;
using System.IO;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SwainStrainTools.Utilities;
using System.Linq;
using System.Collections.Generic; // List<>


namespace SwainStrainTools.UI
{
   public class ViewModel : BaseViewModel
   {
      public RelayCommand WindowLoaded { get; set; }
      public RelayCommand WindowClosed { get; set; }
      public RelayCommand ColorSettingsElement { get; set; }
      public RelayCommand CancelCommand { get; set; }
      public RelayCommand ApplyCommand { get; set; }

      //Private properties
      private static UIApplication _uiapp = null;
      private static Document _doc = null;
      //private Autodesk.Revit.DB.Color rvtColorElement;
      //private ComponentOption selectOption = ComponentOption.OnlyVisible;
      //private ColorHandler _handler = null;
      private List<PipeSystem> _PipeSystemsList;
      private string _SelectedPipeSystem;
      private List<DiameterNominal> _DNList;
      private string _SelectedDN;


      // Public Properties - Used for binding with the View

      //public Autodesk.Revit.DB.Color RvtColorElement { get { return rvtColorElement; } set { rvtColorElement = value; OnPropertyChanged(); } }
      //public ComponentOption SelectOption { get { return selectOption; } set { selectOption = value; OnPropertyChanged(); } }
      public static ViewModel Instance { get; set; }
      public static Document Doc { get { return _doc; } set { _doc = value; } }
      public static UIApplication Uiapp { get { return _uiapp; } set { _uiapp = value; } }
      public static bool IsOpen { get; private set; } = false;

      //public static ColorSettings _ColorSettings { get; set; }
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
            OnPropertyChanged("AllowTypeSelection"); // Trigger Enable/Disable UI element when particular state is selected
            //getTypeList(); // Generate a new list of cities based on a selected state
         }
      }

      public ViewModel(UIApplication uiapp)
      {
         Instance = this;
         _uiapp = uiapp;
         _doc = _uiapp.ActiveUIDocument.Document;
         //_handler = handler;

         // Instantiate, get a list of countries from the Model
         PipeSystem _Category = new PipeSystem();
         PipeSystemsList = _Category.getPipeSystems(uiapp);

         try
         {
            WindowLoaded = new RelayCommand(param => this.LoadedExecuted(param));
            WindowClosed = new RelayCommand(param => this.ClosedExecuted(param));
            //ColorSettingsElement = new RelayCommand(param => this.ColorElementExecuted(param));
            //ApplyCommand = new RelayCommand(param => this.ApplyExecuted(param));
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
            //CollectSettingsJson();
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

            int i = 0;
            foreach (var c in pipes)
            {
               string pipeDN = c.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsValueString();
               returnDNs.Add(new DiameterNominal() { PipeSystemName = pipeSystemName, DN = pipeDN });
               i++;
            }

            returnDNs.Sort();

            return returnDNs;
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

      ////Raise methos when click apply button
      //private void ApplyExecuted(object param)
      //{
      //   _handler.ViewModel = this;
      //   //ExternalApplication.Handler.Request.Make(ColorHandler.RequestId.SetColor);
      //   ExternalApplication.ExEvent.Raise();
      //   //ExternalApplication.SetFocusToRevit();
      //}

      ////Get colorSetting from color selected
      //private void ColorElementExecuted(object param)
      //{
      //   //Model.ColorSettings();
      //   ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog();
      //   colorSelectionDialog.Show();
      //   RvtColorElement = colorSelectionDialog.SelectedColor;
      //   _ColorSettings.ColorElement = RvtColorElement;
      //}

      //#endregion

      //#region Private method
      //private void CollectSettingsJson()
      //{
      //   ColorSettingsFiles = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ColorSettings.json";
      //   _ColorSettings = JsonUtils.Load<ColorSettings>(ColorSettingsFiles);

      //   //Get Color Revit form color defaut of wpf
      //   var cNewElement = System.Drawing.Color.Red; //defaut Red color
      //   RvtColorElement = _ColorSettings.ColorElement ?? new Autodesk.Revit.DB.Color(cNewElement.R, cNewElement.G, cNewElement.B);

      //}
      //#endregion



   }
}

