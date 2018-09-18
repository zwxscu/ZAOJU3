using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ModuleCrossPnP;
using LogInterface;
using ConfigManage.Model;
namespace ConfigManage
{
    public partial class SysSettingView : BaseChildView
    {
        SysCfgsettingModel sysCfg = null;
        //private string captionText = "";
        //private ILogRecorder logRecorder = null;
        //private IParentModule parentPNP = null;
        #region  公有接口
       // public string CaptionText { get { return captionText; } set { captionText = value; this.Text = captionText; } }
        public SysSettingView(string captionText):base(captionText)
        {
            InitializeComponent();
            sysCfg = new SysCfgsettingModel();

            string[] nodeNames = new string[] { "气密检查1", "气密检查2", "气密检查3", "零秒点火", "一次试火:1", "一次试火:2", "一次试火:3", "一次试火:4", "二次试火:1","二次试火:2","外观检查" };
            for (int i = 0; i < nodeNames.Count();i++ )
            {
                CheckBox checkBox = new CheckBox();
            
                checkBox.Name = "checkBoxNode"+(i+1).ToString();
                checkBox.Text = nodeNames[i];
                checkBox.Checked = true;
                this.flowLayoutPanel1.Controls.Add(checkBox);
                
            }
            this.Text = captionText;
            //this.captionText = captionText;
           
            sysCfg.MesEnable = PLProcessModel.SysCfgModel. MesCheckEnable;
            sysCfg.PrienterEnable = PLProcessModel.SysCfgModel.PrienterEnable;
            sysCfg.MesDownTimeout = PLProcessModel.SysCfgModel.MesTimeout;
            this.checkBoxPrinterEnable.DataBindings.Add("Checked", sysCfg, "PrienterEnable", false, DataSourceUpdateMode.OnPropertyChanged);
            this.checkBoxMesEnable.DataBindings.Add("Checked", sysCfg, "MesEnable", false, DataSourceUpdateMode.OnPropertyChanged);
            this.checkBoxMesOffline.DataBindings.Add("Checked", sysCfg, "MesOfflineMode", false, DataSourceUpdateMode.OnPropertyChanged);
            this.textBoxMesTimeout.DataBindings.Add("Text", sysCfg, "MesDownTimeout", false, DataSourceUpdateMode.OnPropertyChanged);
            this.textBoxRfidTimeout.DataBindings.Add("Text", sysCfg, "RfidTimeout", false, DataSourceUpdateMode.OnPropertyChanged);

        }
        #endregion

        private void buttonCfgApply_Click(object sender, EventArgs e)
        {
            if(sysCfg.MesDownTimeout >20)
            {
                sysCfg.MesDownTimeout = 20;
            }
            PLProcessModel.SysCfgModel.PrienterEnable = sysCfg.PrienterEnable;
            PLProcessModel.SysCfgModel.MesTimeout = sysCfg.MesDownTimeout;
            PLProcessModel.SysCfgModel.RfidDelayTimeout = sysCfg.RfidTimeout;
            PLProcessModel.SysCfgModel.MesCheckEnable = sysCfg.MesEnable;
            PLProcessModel.SysCfgModel.MesOfflineMode = sysCfg.MesOfflineMode;
            string reStr = "";
            if(!PLProcessModel.SysCfgModel.SaveCfg(ref reStr))
            {
                MessageBox.Show(reStr);

            }
            else
            {
                MessageBox.Show("设置已保存！");
            }
        }

        private void buttonCancelSet_Click(object sender, EventArgs e)
        {
            sysCfg.PrienterEnable = PLProcessModel.SysCfgModel.PrienterEnable;
            sysCfg.MesEnable = PLProcessModel.SysCfgModel. MesCheckEnable;
            sysCfg.MesDownTimeout = PLProcessModel.SysCfgModel.MesTimeout;
            sysCfg.RfidTimeout = PLProcessModel.SysCfgModel.RfidDelayTimeout;
            sysCfg.MesOfflineMode = PLProcessModel.SysCfgModel.MesOfflineMode;
        }

        private void SysSettingView_Load(object sender, EventArgs e)
        {
            sysCfg.PrienterEnable = PLProcessModel.SysCfgModel.PrienterEnable;
            sysCfg.MesEnable = PLProcessModel.SysCfgModel. MesCheckEnable;
            sysCfg.MesDownTimeout = PLProcessModel.SysCfgModel.MesTimeout;
            sysCfg.RfidTimeout = PLProcessModel.SysCfgModel.RfidDelayTimeout;
            sysCfg.MesOfflineMode = PLProcessModel.SysCfgModel.MesOfflineMode;

            ToolTip ttpSettings = new ToolTip();
            ttpSettings.InitialDelay = 200;
            ttpSettings.AutoPopDelay = 10 * 1000;
            ttpSettings.ReshowDelay = 200;
            ttpSettings.ShowAlways = true;
            ttpSettings.IsBalloon = true;
            ttpSettings.SetToolTip(this.checkBoxMesEnable, "仅限于是否启用MES下线查询，检测数据正常上传");
            ttpSettings.SetToolTip(this.checkBoxMesOffline, "若勾选，MES下线查询和数据上传都停用");
          
        }
        //#region IModuleAttach接口实现
        //public void RegisterMenus(MenuStrip parentMenu, string rootMenuText)
        //{
        //    ToolStripMenuItem rootMenuItem = new ToolStripMenuItem(rootMenuText);//parentMenu.Items.Add("仓储管理");
        //    //rootMenuItem.Click += LoadMainform_MenuHandler;
        //    parentMenu.Items.Add(rootMenuItem);
        //}
        //public void SetParent(/*Control parentContainer, Form parentForm, */IParentModule parentPnP)
        //{
        //    this.parentPNP = parentPnP;
        //}
        //public void SetLoginterface(ILogRecorder logRecorder)
        //{
        //    this.logRecorder = logRecorder;
        //    //   lineMonitorPresenter.SetLogRecorder(logRecorder);
        //}
        //#endregion
       
    }
}
