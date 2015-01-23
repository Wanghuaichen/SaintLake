using SaintX.Navigation;
using SaintX.StageControls;
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
        StepViewModel stepViewModel = new StepViewModel();
        
        public MainWindow():base()
        {
            InitializeComponent();
            log.Info("Main window created.");
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
            lstSteps.DataContext = stepViewModel.StepsModel;
        }

        

        #region commands
        private void CommandHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HelpForm helpForm = new HelpForm();
            helpForm.ShowDialog();
        }

        private void CommandHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion


        #region event
        private void lstSteps_PreviewMouseLeftButtonUp(object sender, 
            MouseButtonEventArgs e)
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
                {
                    NavigateTo(stage2Go);
                    
                }
            }
        }


        void MainWindow_Closed(object sender, EventArgs e)
        {
            Pipeserver.Close();
            log.Info("Main window closed.");
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var userControl in stageUserControls)
                userControlHost.Children.Add(userControl);
            NavigateTo(Stage.AssayDef);
        }
        #endregion


    }



    #region  color converter
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    public class MyColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            SolidColorBrush solidColorBrush = (SolidColorBrush)value;
            return solidColorBrush.Color;
        }
    }
    #endregion endregion


}
