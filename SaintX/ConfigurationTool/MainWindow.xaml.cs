using Saint.Setting;
using SaintX.Setting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace ConfigurationTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        TestSetting testSetting = new TestSetting();
        bool bLoaded = false;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            host.DataContext = testSetting;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            bLoaded = true;
            UpdateSource();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            testSetting.Assays.Add(new ColorfulAssay("test", Colors.Green));
            lstPanels.SelectedIndex = (lstPanels.Items.Count - 1);
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

        private string GetProtocolFile()
        {
            string sDataFolder = GetDataFolder();
            bool DNA = (bool)rdbDNA.IsChecked;
            return sDataFolder + (DNA ? "DNA.csv" : "RNA.csv");
        }

        private void UpdateSource()
        {
            if (!bLoaded)
                return;
            testSetting.Assays.Clear();
            testSetting.ProtocolFileName = "";
            string sFile = GetTestSettingFile();
            if (File.Exists(sFile))
            {
                testSetting = SerializeHelper.Deserialize<TestSetting>(sFile);
                host.DataContext = testSetting;
                if (testSetting.Assays.Count > 0)
                    lstPanels.SelectedIndex = 0;
            }
        }

        private void rdbDNA_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSource();
        }
    
        private void rdbRNA_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSource();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string sProtocolFile = GetProtocolFile();
            if(!File.Exists(sProtocolFile))
            {
                SetInfo(string.Format("位于{0}的文件不存在，请先定义。",sProtocolFile),Colors.Red);
                return;
            }

            testSetting.ProtocolFileName = sProtocolFile;

            if(testSetting.Assays.Count == 0)
            {
                SetInfo("未定义任何Assay！", Colors.Red);
                return;
            }
            string sFile = GetTestSettingFile();
            SerializeHelper.Serialize(sFile, testSetting);

            SetInfo("", Colors.Black);
        }

        private void SetInfo(string text, Color color)
        {
            txtInfo.Text = text;
            txtInfo.Foreground = new SolidColorBrush(color);
        }

        private void btnCreateProtocol_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
