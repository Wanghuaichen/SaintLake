using Saint.TestSetting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConfigurationTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        TestSetting testSetting = new TestSetting();
        public MainWindow()
        {
            InitializeComponent();
            host.DataContext = testSetting;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            testSetting.Assays.Add(new ColorfulAssay("test", Colors.Green));
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (testSetting.Assays.Count == 0)
            {
                return;
            }
            if (lstPanels.SelectedItem == null)
            {
                lstPanels.SelectedIndex = lstPanels.Items.Count - 1;
            }
            testSetting.Assays.RemoveAt(lstPanels.SelectedIndex);
        }

        private void btnColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            var res = colorDialog.ShowDialog();
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.A = colorDialog.Color.A;
            color.B = colorDialog.Color.B;
            color.G = colorDialog.Color.G;
            color.R = colorDialog.Color.R;
            if (res == System.Windows.Forms.DialogResult.Cancel)
                return;
            btnColor.Background = new SolidColorBrush(color);
        }
        #region folder
        private string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        private string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        private string GetDataFolder()
        {
            string sFolder =  GetExeParentFolder() + "Data\\";
            if (!Directory.Exists(sFolder))
                Directory.CreateDirectory(sFolder);
            return sFolder;
        }
        #endregion

        private string GetTestSettingFile()
        {
            string sDataFolder = GetDataFolder();
            bool DNA = (bool)rdbDNA.IsChecked;
            return sDataFolder + (DNA ? "DNA.xml" : "RNA.xml");
        }

        private void UpdateSource()
        {
            string sFile = GetTestSettingFile();
            if (File.Exists(sFile))
                testSetting = SerializeHelper.Deserialize<TestSetting>(sFile);
        }

        private void rdbDNA_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSource();
        }
    
        private void rdbRNA_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSource();
        }
    }
}
