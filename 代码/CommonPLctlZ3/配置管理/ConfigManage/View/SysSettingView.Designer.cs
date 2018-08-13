namespace ConfigManage
{
    partial class SysSettingView
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxPrinterEnable = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxMesOffline = new System.Windows.Forms.CheckBox();
            this.checkBoxMesEnable = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxMesTimeout = new System.Windows.Forms.TextBox();
            this.textBoxRfidTimeout = new System.Windows.Forms.TextBox();
            this.buttonCancelSet = new System.Windows.Forms.Button();
            this.buttonCfgApply = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.buttonCancelSet);
            this.panel1.Controls.Add(this.buttonCfgApply);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(778, 414);
            this.panel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.flowLayoutPanel1);
            this.groupBox2.Location = new System.Drawing.Point(400, 27);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(333, 264);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "工位启用设置";
            this.groupBox2.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 17);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(327, 244);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxPrinterEnable);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkBoxMesOffline);
            this.groupBox1.Controls.Add(this.checkBoxMesEnable);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxMesTimeout);
            this.groupBox1.Controls.Add(this.textBoxRfidTimeout);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(378, 264);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "通用设置";
            // 
            // checkBoxPrinterEnable
            // 
            this.checkBoxPrinterEnable.AutoSize = true;
            this.checkBoxPrinterEnable.Font = new System.Drawing.Font("宋体", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBoxPrinterEnable.Location = new System.Drawing.Point(17, 74);
            this.checkBoxPrinterEnable.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxPrinterEnable.Name = "checkBoxPrinterEnable";
            this.checkBoxPrinterEnable.Size = new System.Drawing.Size(142, 23);
            this.checkBoxPrinterEnable.TabIndex = 0;
            this.checkBoxPrinterEnable.Text = "自动贴标使能";
            this.checkBoxPrinterEnable.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(13, 169);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 38);
            this.label2.TabIndex = 3;
            this.label2.Text = "RFID最长允许延迟\r\n（单位：秒）";
            // 
            // checkBoxMesOffline
            // 
            this.checkBoxMesOffline.AutoSize = true;
            this.checkBoxMesOffline.Font = new System.Drawing.Font("宋体", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBoxMesOffline.Location = new System.Drawing.Point(201, 32);
            this.checkBoxMesOffline.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxMesOffline.Name = "checkBoxMesOffline";
            this.checkBoxMesOffline.Size = new System.Drawing.Size(172, 23);
            this.checkBoxMesOffline.TabIndex = 0;
            this.checkBoxMesOffline.Text = "MES完全断网模式";
            this.checkBoxMesOffline.UseVisualStyleBackColor = true;
            // 
            // checkBoxMesEnable
            // 
            this.checkBoxMesEnable.AutoSize = true;
            this.checkBoxMesEnable.Font = new System.Drawing.Font("宋体", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBoxMesEnable.Location = new System.Drawing.Point(17, 32);
            this.checkBoxMesEnable.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxMesEnable.Name = "checkBoxMesEnable";
            this.checkBoxMesEnable.Size = new System.Drawing.Size(96, 23);
            this.checkBoxMesEnable.TabIndex = 0;
            this.checkBoxMesEnable.Text = "MES启用";
            this.checkBoxMesEnable.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 38);
            this.label1.TabIndex = 3;
            this.label1.Text = "MES下线查询\r\n最大延迟(单位：秒）";
            // 
            // textBoxMesTimeout
            // 
            this.textBoxMesTimeout.Location = new System.Drawing.Point(209, 127);
            this.textBoxMesTimeout.Name = "textBoxMesTimeout";
            this.textBoxMesTimeout.Size = new System.Drawing.Size(135, 21);
            this.textBoxMesTimeout.TabIndex = 2;
            // 
            // textBoxRfidTimeout
            // 
            this.textBoxRfidTimeout.Location = new System.Drawing.Point(209, 184);
            this.textBoxRfidTimeout.Name = "textBoxRfidTimeout";
            this.textBoxRfidTimeout.Size = new System.Drawing.Size(135, 21);
            this.textBoxRfidTimeout.TabIndex = 2;
            // 
            // buttonCancelSet
            // 
            this.buttonCancelSet.Font = new System.Drawing.Font("宋体", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonCancelSet.Location = new System.Drawing.Point(189, 308);
            this.buttonCancelSet.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancelSet.Name = "buttonCancelSet";
            this.buttonCancelSet.Size = new System.Drawing.Size(124, 38);
            this.buttonCancelSet.TabIndex = 1;
            this.buttonCancelSet.Text = "取消";
            this.buttonCancelSet.UseVisualStyleBackColor = true;
            this.buttonCancelSet.Click += new System.EventHandler(this.buttonCancelSet_Click);
            // 
            // buttonCfgApply
            // 
            this.buttonCfgApply.Font = new System.Drawing.Font("宋体", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonCfgApply.Location = new System.Drawing.Point(28, 308);
            this.buttonCfgApply.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCfgApply.Name = "buttonCfgApply";
            this.buttonCfgApply.Size = new System.Drawing.Size(124, 38);
            this.buttonCfgApply.TabIndex = 1;
            this.buttonCfgApply.Text = "应用";
            this.buttonCfgApply.UseVisualStyleBackColor = true;
            this.buttonCfgApply.Click += new System.EventHandler(this.buttonCfgApply_Click);
            // 
            // SysSettingView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 414);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SysSettingView";
            this.Text = "系统设置";
            this.Load += new System.EventHandler(this.SysSettingView_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBoxMesEnable;
        private System.Windows.Forms.CheckBox checkBoxPrinterEnable;
        private System.Windows.Forms.Button buttonCfgApply;
        private System.Windows.Forms.Button buttonCancelSet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxMesTimeout;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxRfidTimeout;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxMesOffline;
    }
}