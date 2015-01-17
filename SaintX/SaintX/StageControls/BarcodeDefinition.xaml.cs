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
        SampleInfos _sampleInfos = null;
        ObservableCollection<ColorfulAssay> _assays;
        PanelViewModel panelVM;

        public SampleInfos SampleInfos
        {
            get { return this._sampleInfos; }
        }
        public BarcodeDefinition(Stage stage, BaseHost host)
            : base(stage, host)
        {
            InitializeComponent();
            _assays = SettingsManager.Instance.Assays;
            _sampleInfos = GlobalVars.Instance.SampleInfos;
            this.Loaded += BarcodeDefinition_Loaded;
            dataGridView.CellPainting += dataGridView_CellPainting;
            this.DataContext = this;
        }

     
        void BarcodeDefinition_Loaded(object sender, RoutedEventArgs e)
        {
            InitTreeview(_assays.Select(x => x.Name).ToList());
            Helper.InitDataGridView(dataGridView,CurStage);
        }

        protected override void onStageChanged(object sender, EventArgs e)
        {
            base.onStageChanged(sender, e);
            if(this.Visibility != System.Windows.Visibility.Visible)
                return;
            Helper.UpdateDataGridView(dataGridView,CurStage);
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

        #region datagridview
        void dataGridView_CellPainting(object sender, System.Windows.Forms.DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex < 0)
                return;
            if (e.RowIndex == 0)
                return;
            
            {
                System.Drawing.Rectangle newRect = new System.Drawing.Rectangle(e.CellBounds.X + 1,
                    e.CellBounds.Y + 1, e.CellBounds.Width - 4,
                    e.CellBounds.Height - 4);

                using (
                    System.Drawing.Brush gridBrush = new System.Drawing.SolidBrush(this.dataGridView.GridColor),
                    backColorBrush = new System.Drawing.SolidBrush(e.CellStyle.BackColor))
                {
                    using (System.Drawing.Pen gridLinePen = new System.Drawing.Pen(gridBrush))
                    {
                        // Erase the cell.
                        e.Graphics.FillRectangle(backColorBrush, e.CellBounds);

                        // Draw the grid lines (only the right and bottom lines;
                        // DataGridView takes care of the others).
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
                            e.CellBounds.Bottom - 1, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1,
                            e.CellBounds.Top, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom);

                        // Draw the inset highlight box.
                        //e.Graphics.DrawRectangle(System.Drawing.Pens.Blue, newRect);

                         //Draw the text content of the cell, ignoring alignment.
                        if (e.Value != null)
                        {
                            e.Graphics.DrawString((String)e.Value, e.CellStyle.Font,
                                System.Drawing.Brushes.Black, e.CellBounds.X + 2,
                                e.CellBounds.Y + 2, System.Drawing.StringFormat.GenericDefault);
                        }
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion


        private void btnBarcodeOk_Click(object sender, RoutedEventArgs e)
        {
            int startBarCodeNum = 0;
            int endBarCodeNum = 0;

            // The bar code is assumed to be an integer
            if(rdbStartCount.IsChecked.Value)
            {
                startBarCodeNum = int.Parse(txtStartBarcodeApproach1.Text);
                int barCodeCount = int.Parse(txtCount.Text);
                endBarCodeNum = startBarCodeNum + barCodeCount - 1;
            }
            else
            {
                startBarCodeNum = int.Parse(txtStartBarcodeApproach2.Text);
                endBarCodeNum = int.Parse(txtEndBarcode.Text);
            }

            // Assign barcode to samples
            int barCode = startBarCodeNum;
            int sampleCount = GlobalVars.Instance.SampleInfos.SampleCount;
            int sampleAssignedBarcodeCount = 1;
            bool isDone = false;
            for (int col = 0; col < 10; ++col)
            {
                for (int row = 0; row < 16; ++row)
                {
                    if (sampleAssignedBarcodeCount <= sampleCount)
                    {
                        SampleInfo sampleInfo = GlobalVars.Instance.SampleInfos[row, col];
                        if (sampleInfo != null)
                        {
                            if (barCode <= endBarCodeNum)
                            {
                                sampleInfo.Barcode = barCode++.ToString();
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
            Helper.UpdateDataGridView(dataGridView,CurStage);
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
