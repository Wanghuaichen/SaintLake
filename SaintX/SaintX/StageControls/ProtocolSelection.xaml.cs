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
            lstProtocols.ItemsSource = allScripts;
            lstProtocols.SelectionChanged += lstProtocols_SelectionChanged;
            lstProtocols.SelectedIndex = 0;
        }

        void lstProtocols_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(lstProtocols.Items != null && lstProtocols.SelectedItem != null)
            {
                UpdateBackGroundImage((string)lstProtocols.SelectedItem);
            }
        }

        private IEnumerable<string> EnumScripts()
        {
            string scriptFolder = ConfigurationManager.AppSettings["scriptFolder"];
            return Directory.EnumerateFiles(scriptFolder, "*.esc");
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //GlobalVars.Instance.PanelType = (bool)rdbMagnetic.IsChecked ? "mag" : "oneStep";
            if(lstProtocols.SelectedItem == null)
            {
                SetInfo("请选中一个实验！");
                return;
            }
            GlobalVars.Instance.ScriptName = (string)lstProtocols.SelectedItem;
            SettingsManager.Instance.UpdateProtocol();
            NotifyFinished();
        }

        private void SetInfo(string s)
        {
            txtInfo.Foreground = Brushes.Red;
            txtInfo.Text = s;
        }

    

        private void UpdateBackGroundImage(string imageName)
        {
            string file = FolderHelper.GetImageFolder() + imageName + ".jpg";
            ImageSource imageSource = new BitmapImage(new Uri(file));
            layoutImg.Source = imageSource;

        }

    
    }
}
