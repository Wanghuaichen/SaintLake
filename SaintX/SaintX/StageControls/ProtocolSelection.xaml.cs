using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;

namespace SaintX.StageControls
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
            lstAssay.ItemsSource = new List<string> {"HBV","HCV"};
            lstAssay.SelectedIndex = 0;
            chkOneStep.Click += chkOneStep_Click;
            chkMag.Click += chkMag_Click;
            //lstProtocols.ItemsSource = allScripts;
            //lstProtocols.SelectionChanged += lstProtocols_SelectionChanged;
            //lstProtocols.SelectedIndex = 0;
            OnProtocolChanged();
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
                SetInfo(string.Format("无法找到合适的脚本，试验方法必须是{0}且用{1}试剂！",GetProtocolName(),(string)lstAssay.SelectedItem));
                return;
            }
            GlobalVars.Instance.ScriptName = scriptName;
            try
            {
                EVOController.Instance.Start();
                GlobalVars.Instance.SampleLayoutSettings = SampleLayoutSettings.Create(smpCnt);
                SettingsManager.Instance.UpdateProtocol();
                NotifyFinished();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
                return;
            }
            
        }
        private string GetProtocolName()
        {
            string protocolName = (bool)chkMag.IsChecked ? "mag" : "oneStep";
            return protocolName;
        }
        private string GetScriptName(string assayName)
        {
            string protocolName = GetProtocolName();
            return allScripts.Where(x => x.Contains(assayName) && x.Contains(protocolName)).First();
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

    
    }
}
