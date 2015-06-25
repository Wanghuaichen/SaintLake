using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SaintX.StageControls
{
    /// <summary>
    /// ProtocolSelection.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolSelection : BaseUserControl
    {

        public ProtocolSelection(Stage stage, BaseHost host)
            : base(stage, host)
        {
            InitializeComponent();
            
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            GlobalVars.Instance.PanelType = (bool)rdbMagnetic.IsChecked ? "mag" : "oneStep";
            SettingsManager.Instance.UpdateProtocol();
            NotifyFinished();
        }

        private void rdbMagnetic_Checked(object sender, RoutedEventArgs e)
        {
            UpdateBackGroundImage(true);
        }

        private void rdbOneStep_Checked(object sender, RoutedEventArgs e)
        {
            UpdateBackGroundImage(false);
        }

        private void UpdateBackGroundImage(bool bMagnetic)
        {
            string file = FolderHelper.GetImageFolder() + (bMagnetic ? "magnetic.jpg" : "onestep.jpg");
            ImageSource imageSource = new BitmapImage(new Uri(file));
            layoutImg.Source = imageSource;

        }

    
    }
}
