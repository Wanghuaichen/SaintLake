using System.Windows;
using Natchs.Properties;

namespace Natchs
{
    /// <summary>
    /// HelpForm.xaml 的交互逻辑
    /// </summary>
    public partial class HelpForm : Window
    {
        public HelpForm()
        {
            InitializeComponent();
            lblDescription.Content = "Natchs:版本号：" + stringRes.version;
        }
    }
}
