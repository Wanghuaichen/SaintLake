using SaintX.Data;
using SaintX.Interfaces;
using SaintX.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SaintX.stageControls
{
    /// <summary>
    /// AssayDefinition.xaml 的交互逻辑
    /// </summary>
    public partial class AssayDefinition : BaseUserControl
    {
        ObservableCollection<ColorfulAssay> _assays;
        public AssayDefinition(Stage stage, BaseHost host):base(stage,host)
        {
            InitializeComponent();
            _assays = SettingsManager.Instance.Assays;
            lstAssays.DataContext = _assays;
        }

        private void btnSetSampleCnt_Click(object sender, RoutedEventArgs e)
        {
            //int maxSampleCount = Utility.GetMaxSampleCount();
            //int i;
            //bok = int.TryParse(txtSampleCount.Text, out i);
            //if (!bok)
            //    SetInfo("样品数量必须为数字！", Colors.Red);
            //if (i <= 0 || i > maxSampleCount)
            //{
            //    SetInfo(string.Format("样品数量必须介于1和{0}之间", maxSampleCount), Colors.Red);
            //    bok = false;
            //}

        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
