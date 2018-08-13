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
namespace ProductRecordView
{
    public partial class RecordView : BaseChildView
    {
        private ProduceReccordView produceRecord = null;
        private MesDatarecordView mesDataRecord = null;
        private OnlineProductView onlineProductView = null;
        public RecordView():base(string.Empty)
        {
            InitializeComponent();
            this.onlineProductView = new OnlineProductView("在线产品");
            this.produceRecord = new ProduceReccordView("生产记录");
            this.mesDataRecord = new MesDatarecordView("MES上传记录");

        }
        #region IModuleAttach接口实现
        public override void RegisterMenus(MenuStrip parentMenu, string rootMenuText)
        {

            ToolStripMenuItem rootMenuItem = new ToolStripMenuItem(rootMenuText);//parentMenu.Items.Add("仓储管理");
            //rootMenuItem.Click += LoadMainform_MenuHandler;
            parentMenu.Items.Add(rootMenuItem);
            ToolStripItem onlineProductsItem = rootMenuItem.DropDownItems.Add("在线产品");
            ToolStripItem productRecordItem = rootMenuItem.DropDownItems.Add("生产记录");
            ToolStripItem mesDataRecordItem = rootMenuItem.DropDownItems.Add("MES上传记录");

            productRecordItem.Click += LoadView_MenuHandler;
            mesDataRecordItem.Click += LoadView_MenuHandler;
            onlineProductsItem.Click += LoadView_MenuHandler;
        }
        public override void SetParent(/*Control parentContainer, Form parentForm, */IParentModule parentPnP)
        {
            this.parentPNP = parentPnP;
            
            this.produceRecord.SetParent(parentPnP);
           
        }
        public override void SetLoginterface(ILogRecorder logRecorder)
        {
            this.logRecorder = logRecorder;
            //   lineMonitorPresenter.SetLogRecorder(logRecorder);
            this.produceRecord.SetLoginterface(logRecorder);
            this.mesDataRecord.SetLoginterface(logRecorder);
        }
       
        #endregion
        private void LoadView_MenuHandler(object sender, EventArgs e)
        {
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem == null)
            {
                return;
            }
            switch (menuItem.Text)
            {
                case "在线产品":
                    {
                        this.parentPNP.AttachModuleView(this.onlineProductView);
                        break;
                    }
                case "生产记录":
                    {
                        this.parentPNP.AttachModuleView(this.produceRecord);
                        break;
                    }
                case "MES上传记录":
                    {
                        this.parentPNP.AttachModuleView(this.mesDataRecord);
                        break;
                    }
                default:
                    break;
            }


        }
    }
}
