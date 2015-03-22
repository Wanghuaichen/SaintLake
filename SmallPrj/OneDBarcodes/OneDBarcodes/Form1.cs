using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneDBarcodes
{
    public partial class Form1 : Form
    {
        
        public delegate void UpdateViewDelegate(string text);
        UpdateViewDelegate updateDelegate;
        CellPosition curPositon = new CellPosition();
        public Form1()
        {
            InitializeComponent();
            InitSerialPort();
            updateDelegate = UpdateSingleCell;
        }

        private void InitSerialPort()
        {
            try
            {
                serialPort1.DataReceived += serialPort1_DataReceived;
                serialPort1.StopBits = System.IO.Ports.StopBits.One;
                serialPort1.BaudRate = 9600;
                serialPort1.PortName = string.Format("COM{0}", ConfigurationManager.AppSettings["comPort"]);
                serialPort1.Parity = System.IO.Ports.Parity.None;
                serialPort1.DataBits = 8;
                serialPort1.Open();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message + "请检查设备是否连接!",Color.Red);
                btnConnect.Enabled = true;   
                return;
            }
            btnConnect.Enabled = false;
            SetInfo("打开串口成功!", Color.Green);
        }

        void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] readBuffer = new byte[serialPort1.ReadBufferSize];
            serialPort1.Read(readBuffer, 0, readBuffer.Length);
            string barcode = Encoding.ASCII.GetString(readBuffer);
            if (barcode == "")
                return;
            if(barcode.Length == 2)
            {
                curPositon = GetPositon(barcode);
                return;
            }
            dataGridView1.Invoke(updateDelegate, new string[] { barcode });
           
        }

        private CellPosition GetPositon(string barcode)
        {
            int val = int.Parse(barcode);
            return new CellPosition(val - 1);
        }

      

        private void UpdateSingleCell(string barcode)
        {
            GlobalVars.Instance.BarcodeSetting[curPositon] = barcode;
            var cell = dataGridView1.Rows[curPositon.rowIndex].Cells[curPositon.colIndex];
            cell.Value = barcode;
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            SetInfo("", Color.Black);
            if(GlobalVars.Instance.BarcodeSetting.Count != 0)
            {
                var result = System.Windows.Forms.MessageBox.Show
                   ("已经设置过一些样品,您确定要继续？\r\n点击‘Yes’将会清空设置信息！", "警告",
                   MessageBoxButtons.YesNo);
                if (result != System.Windows.Forms.DialogResult.Yes)
                    return;
 
            }

            int cnt = 0; 
            bool isInt = int.TryParse(txtSampleCount.Text, out cnt);
            if(!isInt)
            {
                SetInfo("样品数量必须为数字!", Color.Red);
                return;
            }

            if( cnt <= 0 || cnt > GlobalVars.Instance.MaxSampleCount)
            {
                SetInfo(string.Format("样品数量必须介于1和{0}之间", GlobalVars.Instance.MaxSampleCount), Color.Red);
                return;
            }
            GlobalVars.Instance.BarcodeSetting.Clear();
            DataGridViewHelper.InitDataGridView(dataGridView1);
            
        }

        private void SetInfo(string txt, Color color)
        {
            txtInfo.Text = txt;
            txtInfo.ForeColor = color;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            InitSerialPort();
        }
    }
}
