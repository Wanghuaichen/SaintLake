using System.Windows;
using SaintX.Properties;

namespace SaintX
{
    /// <summary>
    /// HelpForm.xaml 的交互逻辑
    /// </summary>
    public partial class HelpForm : Window
    {
        public HelpForm()
        {
            InitializeComponent();
            lblDescription.Content = "SaintX:版本号：" + stringRes.version;
        }
    }
}
