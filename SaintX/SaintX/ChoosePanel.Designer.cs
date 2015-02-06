namespace SaintX
{
    partial class ChoosePanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rdbDNA = new System.Windows.Forms.RadioButton();
            this.rdbRNA = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rdbDNA
            // 
            this.rdbDNA.AutoSize = true;
            this.rdbDNA.Checked = true;
            this.rdbDNA.Location = new System.Drawing.Point(25, 20);
            this.rdbDNA.Name = "rdbDNA";
            this.rdbDNA.Size = new System.Drawing.Size(41, 16);
            this.rdbDNA.TabIndex = 0;
            this.rdbDNA.TabStop = true;
            this.rdbDNA.Text = "DNA";
            this.rdbDNA.UseVisualStyleBackColor = true;
            // 
            // rdbRNA
            // 
            this.rdbRNA.AutoSize = true;
            this.rdbRNA.Location = new System.Drawing.Point(25, 43);
            this.rdbRNA.Name = "rdbRNA";
            this.rdbRNA.Size = new System.Drawing.Size(41, 16);
            this.rdbRNA.TabIndex = 1;
            this.rdbRNA.Text = "RNA";
            this.rdbRNA.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdbDNA);
            this.groupBox1.Controls.Add(this.rdbRNA);
            this.groupBox1.Location = new System.Drawing.Point(22, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 81);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "试验类型";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(197, 117);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 35);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // ChoosePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 188);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ChoosePanel";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton rdbDNA;
        private System.Windows.Forms.RadioButton rdbRNA;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnConfirm;
    }
}