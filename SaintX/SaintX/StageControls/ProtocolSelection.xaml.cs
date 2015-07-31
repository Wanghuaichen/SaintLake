using Natchs.Data;
using Natchs.Navigation;
using Natchs.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using Natchs.Utility;

namespace Natchs.StageControls
{
    /// <summary>
    /// ProtocolSelection.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolSelection : BaseUserControl
    {
        
        List<string> allScripts = new List<string>();
        public ProtocolSelection(Stage stage, BaseHost host)
            : base(stage, host)
        {
            InitializeComponent();
            var wholePaths = EnumScripts();
            foreach(string s in wholePaths)
            {
                FileInfo fileInfo = new FileInfo(s);
                allScripts.Add(fileInfo.Name.Replace(".esc",""));
            }
            
            lstAssay.ItemsSource = ReadAssays();
            lstAssay.SelectedIndex = 0;
            chkOneStep.Click += chkOneStep_Click;
            chkMag.Click += chkMag_Click;
            //lstProtocols.ItemsSource = allScripts;
            //lstProtocols.SelectionChanged += lstProtocols_SelectionChanged;
            //lstProtocols.SelectedIndex = 0;
            OnProtocolChanged();
        }

        private System.Collections.IEnumerable ReadAssays()
        {
            string filePath = FolderHelper.GetDataFolder() + "assays.txt";
            if(!File.Exists(filePath))
            {
                SetInfo("无法找到项目定义文件！");
                return null;
            }
            return File.ReadAllLines(filePath);

        }

        void chkMag_Click(object sender, RoutedEventArgs e)
        {
            OnProtocolChanged();
        }

        void chkOneStep_Click(object sender, RoutedEventArgs e)
        {
            OnProtocolChanged();
        }

        private void OnProtocolChanged()
        {
           string imageName = GetProtocolName();
           UpdateBackGroundImage(imageName);
        }

        private IEnumerable<string> EnumScripts()
        {
            string scriptFolder = ConfigurationManager.AppSettings["scriptFolder"];
            return Directory.EnumerateFiles(scriptFolder, "*.esc");
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            SetInfo("");
            if (lstAssay.SelectedItem == null)
            {
                SetInfo("请选中一个实验！");
                return;
            }
            int smpCnt = 16;
            bool bInteger = int.TryParse(txtSampleCount.Text, out smpCnt);
            if(!bInteger)
            {
                SetInfo("样品数量必须为数字！");
                return;
            }

            if(smpCnt <16)
            {
                SetInfo("样品数量不得小于16！");
                return;
            }
            string scriptName = "";

            try
            {
               scriptName = GetScriptName((string)lstAssay.SelectedItem);
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
                return;
            }
            GlobalVars.Instance.AssayName = (string)lstAssay.SelectedItem;
            GlobalVars.Instance.ProtocolName = GetProtocolName();
            GlobalVars.Instance.SampleCount = smpCnt;
            try
            {
                GlobalVars.Instance.SampleLayoutSettings = SampleLayoutSettings.Create(smpCnt);
                SettingsManager.Instance.UpdateProtocol();
                GlobalVars.Instance.UseLastTimeSetting = (bool)chkkUseLastSettings.IsChecked;
                if(!GlobalVars.Instance.UseLastTimeSetting)
                    CreateLastRunInfos(smpCnt,(string)lstAssay.SelectedItem);
                EVOController.Instance.Start();
                NotifyFinished();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
                return;
            }
           
        }

        private void CreateLastRunInfos(int smpCnt, string assayName)
        {
            GlobalVars.Instance.LastRunInfos = new Natchs.Utility.LastRunInfos(GetProtocolName(), assayName, smpCnt, 0);
        }

      

        private string GetProtocolName()
        {
            string protocolName = (bool)chkMag.IsChecked ? "PRO" : "FAST";
            return protocolName;
        }
        private string GetScriptName(string assayName)
        {
            string protocolName = GetProtocolName();
            string scriptName = string.Format("{0}_{1}", protocolName, assayName);  //
            if (!allScripts.Contains(scriptName))
                throw new FileNotFoundException(string.Format("无法找到名为{0}的脚本！", scriptName));
            return scriptName;
        }

        private void SetInfo(string s)
        {
            txtInfo.Foreground = Brushes.Red;
            txtInfo.Text = s;
        }

    

        private void UpdateBackGroundImage(string imageName)
        {
            string file = FolderHelper.GetImageFolder() + imageName + ".jpg";
            if(!File.Exists(file))
            {
                SetInfo("无法找到相应的布局图!");
                return;
            }
            else
            {
                SetInfo("");
            }
            ImageSource imageSource = new BitmapImage(new Uri(file));
            layoutImg.Source = imageSource;

        }

        private void useLastSettings_Clicked(object sender, RoutedEventArgs e)
        {
          
            bool bChked = (bool)chkkUseLastSettings.IsChecked;
            if(bChked)
            {
                string lastRunXMLFile = FolderHelper.GetLastRunInfoFile();
                if (!File.Exists(lastRunXMLFile))
                {
                    SetInfo("无法找到上次运行定义文件！");
                    return;
                }
                string sContent = File.ReadAllText(lastRunXMLFile);
                if(sContent == "")
                {
                    SetInfo("上一次运行定义文件非法！");
                    return;
                }
                GlobalVars.Instance.LastRunInfos = SerializeHelper.Deserialize<LastRunInfos>(sContent);
            }
            bool bEnabled = !bChked;
            EnableControls(bEnabled);
       
        }

        private void EnableControls(bool bEnable)
        {
            txtSampleCount.IsEnabled = bEnable;
            if(!bEnable)
            {
                lstAssay.SelectedItem = GlobalVars.Instance.LastRunInfos.AssayName;
                bool bMag = GlobalVars.Instance.LastRunInfos.Protocol == "mag";
                chkMag.IsChecked = bMag;
                chkOneStep.IsChecked = !bMag;
                txtSampleCount.Text = GlobalVars.Instance.LastRunInfos.SampleCount.ToString();
                OnProtocolChanged();
            }
            lstAssay.IsEnabled = bEnable;
            chkMag.IsEnabled = bEnable;
            chkOneStep.IsEnabled = bEnable;
        }

     
    
    }
}
