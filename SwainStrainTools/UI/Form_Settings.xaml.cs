using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SwainStrainTools.UI
{
   /// <summary>
   /// Interaction logic for Form_Settings.xaml
   /// </summary>
   public partial class Form_Settings : Window
   {
      public Form_Settings()
      {
         InitializeComponent();
         License.Text = Properties.Settings.Default.LicenseKEY;
         License_ID.Text = Properties.Settings.Default.LicenseID;
      }

      private void OKBtn_Click(object sender, RoutedEventArgs e)
      {
         Properties.Settings.Default.LicenseKEY= this.License.Text;
         Properties.Settings.Default.LicenseID= this.License_ID.Text;
         Properties.Settings.Default.Save();

         Program.ActivateMachine();
         Program.ValidateLicenseByKey();

         //MessageBox.Show(Program.VALID.ToString());

         Close();
      }

      private void CancelBtn_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }

   }
}
