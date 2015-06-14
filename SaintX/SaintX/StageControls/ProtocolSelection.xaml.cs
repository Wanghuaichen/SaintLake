using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System.Windows;

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
            GlobalVars.Instance.PanelType = (bool)rdbMagnetic.IsChecked ? "DNA" : "RNA";
            SettingsManager.Instance.UpdateProtocol();
            NotifyFinished();
        }

    
    }
}
