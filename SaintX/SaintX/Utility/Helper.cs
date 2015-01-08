using SaintX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;

namespace SaintX.Utility
{
    class Helper
    {
        #region datagrid related
        static public System.Drawing.Color Convert2SystemDrawingColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        static public void InitDataGridView(DataGridView dataGridView)
        {
            //dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewColumn());
            dataGridView.AllowUserToAddRows = false;
            dataGridView.Columns.Clear();
            List<string> strs = new List<string>();

            int colNum = (GlobalVars.Instanse.SampleCount + 15) / 16;
            for (int j = 0; j < colNum; j++)
                strs.Add("");
            int gridStartPos = SettingsManager.Instance.ProjectSettings.startGrid;
            for (int i = 0; i < colNum; i++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = string.Format("条{0}", gridStartPos + i);
                dataGridView.Columns.Add(column);
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            dataGridView.RowHeadersWidth = 80;
            for (int i = 0; i < 16; i++)
            {
                dataGridView.Rows.Add(strs.ToArray());
                dataGridView.Rows[i].HeaderCell.Value = string.Format("行{0}", i + 1);
            }
        }

        static public void UpdateDataGridView(DataGridView dataGridView)
        {
            foreach( KeyValuePair<CellPosition, SampleInfo> pair in GlobalVars.Instanse.SampleInfos)
            {
                CellPosition cellPos = pair.Key;
                SampleInfo sampleInfo = pair.Value;
                var cell = dataGridView.Rows[cellPos.rowIndex].Cells[cellPos.colIndex];
                cell.Value = sampleInfo.colorfulAssay.Name;
                cell.Style.BackColor = Convert2SystemDrawingColor(sampleInfo.colorfulAssay.Color);
            }
        }

        #endregion
    }
}
