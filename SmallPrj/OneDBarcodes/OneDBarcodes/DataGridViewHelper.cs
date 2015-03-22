using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneDBarcodes
{
    class DataGridViewHelper
    {
        static public void InitDataGridView(DataGridView dataGridView)
        {
            //dataGridView.Columns.Add(new System.Windows.Forms.DataGridViewColumn());
            dataGridView.Columns.Clear();
            List<string> strs = new List<string>();

            int colNum = (GlobalVars.Instance.SampleCount + 15) / 16;
            for (int j = 0; j < colNum; j++)
                strs.Add("");
            int gridStartPos = GlobalVars.Instance.StartGrid;
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


        static  public void UpdateDataGridView(DataGridView dataGridView)
        {
            foreach (KeyValuePair<CellPosition, string> pair in GlobalVars.Instance.BarcodeSetting)
            {
                CellPosition cellPos = pair.Key;
               string barcode = pair.Value;
               var cell = dataGridView.Rows[cellPos.rowIndex].Cells[cellPos.colIndex];
               cell.Value = barcode;
            }
        }

    }
}
