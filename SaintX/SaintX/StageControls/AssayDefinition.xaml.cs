using Saint.TestSetting;
using SaintX.Data;
using SaintX.Navigation;
using SaintX.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;

namespace SaintX.StageControls
{
    /// <summary>
    /// AssayDefinition.xaml 的交互逻辑
    /// </summary>
    public partial class AssayDefinition : BaseUserControl
    {
        List<ColorfulAssay> _assays;
        public AssayDefinition(Stage stage, BaseHost host)
            : base(stage, host)
        {
            InitializeComponent();
            _assays = SettingsManager.Instance.Assays;
            lstAssays.DataContext = _assays;
            this.Loaded += AssayDefinition_Loaded;
        }

        void AssayDefinition_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            Helper.InitDataGridView(dataGridView, CurStage );
        }

        private void btnSetSampleCnt_Click(object sender, RoutedEventArgs e)
        {
            int maxSampleCount = SettingsManager.Instance.PhysicalSettings.MaxSampleCount;
            lstAssays.SelectedIndex = -1;
            #region check sampleCnt valid
            int smpCnt;
            ResultIsOk = int.TryParse(txtSampleCount.Text, out smpCnt);
            if (!ResultIsOk)
                SetInfo("样品数量必须为数字！", Colors.Red);
            if (smpCnt <= 0 || smpCnt > maxSampleCount)
            {
                SetInfo(string.Format("样品数量必须介于1和{0}之间", maxSampleCount), Colors.Red);
                ResultIsOk = false;
            }
            #endregion


            if (GlobalVars.Instance.SampleLayoutInfos.AlreadyHasInfo())
            {
                var result = System.Windows.Forms.MessageBox.Show
                    ("已经设置过一些样品,您确定要继续？\r\n点击‘Yes’将会清空设置信息！", "警告",
                    MessageBoxButtons.YesNo);
                if (result != System.Windows.Forms.DialogResult.Yes)
                    return;
            }
            GlobalVars.Instance.SampleCount = smpCnt;
            GlobalVars.Instance.SampleLayoutInfos.Clear();
            lstAssays.SelectedIndex = -1;
            Helper.InitDataGridView(dataGridView, CurStage);
            Helper.UpdateDataGridView(dataGridView, CurStage);
        }

        void SetInfo(string s, Color color)
        {
            if (txtInfo == null)
                return;

            txtInfo.Background = new SolidColorBrush(Colors.White);
            txtInfo.Text = s;
            txtInfo.Foreground = new SolidColorBrush(color);
        }

        private bool CheckSettings()
        {
            int count = 0;
            bool sequent = true;
            int sampleCount = GlobalVars.Instance.SampleCount;
            int colNum = (sampleCount +15) / 16;
            List<int> rowCounts = new List<int>();
            for (int i = 0; i < colNum - 1; i++)
                rowCounts.Add(16);
            int remainCnt = sampleCount % 16;
            if ( remainCnt == 0)
                rowCounts.Add(16);
            else
                rowCounts.Add(remainCnt);


            for (int c = 0; c < colNum; c++)
            {
                for (int r = 0; r < rowCounts[c]; r++)
                {
                    if (GlobalVars.Instance.SampleLayoutInfos.Contains(r,c))
                    {
                        count++;
                    }
                    else
                    {
                        if (count < sampleCount)
                            sequent = false;
                    }
                }
            }

            if (!sequent)
            {
                SetInfo("样品没有全部设置", Colors.Red);
                return false;
            }

            if (count != sampleCount)
            {
                SetInfo(string.Format("样品总数为：{0}，有{1}个已被设置,请确保二者相等。", sampleCount, count), Colors.Red);
                return false;
            }
            return true;
        }


        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            bool bvalid = CheckSettings();
            if (!bvalid)
                return;
            NotifyFinished();
        }

        #region datagridview


        void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedCells.Count == 0 || lstAssays.SelectedIndex == -1)
                return;
            SetSelectedCell2CurrentAssay();
            dataGridView.ClearSelection();
        }

        private void SetSelectedCell2CurrentAssay()
        {
            string assayName = _assays[lstAssays.SelectedIndex].Name;
            System.Windows.Media.Color color = _assays[lstAssays.SelectedIndex].Color;
            foreach (DataGridViewCell cell in dataGridView.SelectedCells)
            {
                if (IsCellOfOutRange(cell.RowIndex, cell.ColumnIndex))
                    continue;

                cell.Style.BackColor = Helper.Convert2SystemDrawingColor(color);
                cell.Value = assayName;
                CellPosition curCellPos = new CellPosition(cell.ColumnIndex, cell.RowIndex);
                GlobalVars.Instance.SampleLayoutInfos[curCellPos] = new SampleInfo(_assays[lstAssays.SelectedIndex], "");
            }
        }

        private bool IsCellOfOutRange(int rowIndex, int columnIndex)
        {
            int sampleCount = int.Parse(txtSampleCount.Text);
            int wellID = rowIndex + columnIndex * 16 + 1;
            return wellID > sampleCount;
        }
        #endregion

    }
}
