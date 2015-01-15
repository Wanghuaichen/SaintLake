using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SaintX.StageControls
{
    /// <summary>
    /// BarcodeDefinition.xaml 的交互逻辑
    /// </summary>
    public partial class BarcodeDefinition : BaseUserControl
    {
        ObservableCollection<ColorfulAssay> _assays;
        PanelViewModel panelVM;
        public BarcodeDefinition(Stage stage, BaseHost host)
            : base(stage, host)
        {
            InitializeComponent();
            _assays = SettingsManager.Instance.Assays;
            this.Loaded += BarcodeDefinition_Loaded;
            
        }

        void BarcodeDefinition_Loaded(object sender, RoutedEventArgs e)
        {
            InitTreeview(_assays.Select(x => x.Name).ToList());
            Helper.InitDataGridView(dataGridView);
            
        }

        protected override void onStageChanged(object sender, EventArgs e)
        {
            base.onStageChanged(sender, e);
            if(this.Visibility != System.Windows.Visibility.Visible)
                return;
            Helper.UpdateDataGridView(dataGridView);
        }

        private void InitTreeview(List<string> assays)
        {
            panelVM = PanelViewModel.CreateViewModel(assays);
            tree.ItemsSource = new ObservableCollection<PanelViewModel>() { panelVM };
            this.tree.Focus();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NotifyFinished();
        }

        private void btnBarcodeOk_Click(object sender, RoutedEventArgs e)
        {
            if(rdbStartCount.IsChecked.Value)
            {
                string startBarCode = txtStartBarcodeApproach1.Text;
                // The bar code is assumed to be an integer
                int startBarCodeNum = int.Parse(startBarCode);
                int barCodeCount = int.Parse(txtCount.Text);
                int endBarCodeNum = startBarCodeNum + barCodeCount - 1;

                int barCode = startBarCodeNum;
                int sampleCount = GlobalVars.Instance.SampleInfos.SampleCount;
                int sampleAssignedBarcodeCount = 1;
                bool isDone = false;
                for (int col = 0; col < 20; ++col)
                {
                    for (int row = 0; row < 16; ++row)
                    {
                        if (sampleAssignedBarcodeCount <= sampleCount)
                        {
                            SampleInfo sampleInfo = GlobalVars.Instance.SampleInfos[col, row];
                            if (sampleInfo != null)
                            {
                                if (barCode <= endBarCodeNum)
                                {
                                    sampleInfo.barcode = barCode++.ToString();
                                    ++sampleAssignedBarcodeCount;
                                }
                                else
                                {
                                    // All avaliable barcodes have been used
                                    isDone = true;
                                    break;     
                                }
                            }
                        }
                        else
                        {
                            isDone = true;
                            break;   // All samples have been assigned barcode
                        }
                    }

                    if (isDone)
                        break;
                }
            }
            else
            {

            }
            
            Helper.UpdateDataGridView(dataGridView);
        }
    }

    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class BoolColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightYellow);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
