using SaintX.Navigation;
using System;
using System.Collections.Generic;
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

namespace SaintX
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : BaseHost
    {
       
        public MainWindow()
        {
            InitializeComponent();
        }

        
        

        #region commands
        private void CommandHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Help help = new Help();
            //help.ShowDialog();
        }

        private void CommandHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

        private void lstSteps_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (preventUI)
                return;
            var item = ItemsControl.ContainerFromElement(lstSteps, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                Stage stage2Go = ((StepDesc)item.Content).CorrespondingStage;
                if (stage2Go > farthestStage)
                {
                    e.Handled = true;
                }
                else
                    NavigateTo(stage2Go);
            }
        }

        private void userControlHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

    }
}
