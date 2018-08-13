using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using ModuleCrossPnP;
//using PLctlPresenter;
using LineNodes;
using LogManage;
using LogInterface;
using ProductRecordView;
using ConfigManage;
using LicenceManager;
namespace CommonPL
{
    public partial class MainForm : Form, ILogDisp, IParentModule,ILicenseNotify
    {
        private string version = "1.0.0 发布版 2018-3-30";

        private int roleID = 1;
        private string userName = "";
        const int CLOSE_SIZE = 10;
       // private CtlcorePresenter corePresenter;
        NodeMonitorView nodeMonitorView = null;
        LogView logView = null;
        RecordView recordView = null;
        ConfiManageView configView = null;
        private List<string> childList = null;

        LicenseMonitor licenseMonitor = null;
        public MainForm(int roleID,string userName)
        {
            InitializeComponent();
           
          //  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;  
            //this.splitContainer1.Panel2.Height = 134;
            childList = new List<string>();
            this.roleID = roleID;
            this.userName = userName;
        }
      
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.labelVersion.Text = this.version;
            this.MainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.MainTabControl.Padding = new System.Drawing.Point(CLOSE_SIZE, CLOSE_SIZE);
            this.MainTabControl.DrawItem += new DrawItemEventHandler(this.tabControlMain_DrawItem);
            this.MainTabControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControlMain_MouseDown);
            Console.SetOut(new TextBoxWriter(this.richTextBoxLog));
           // corePresenter = new CtlcorePresenter();
            string dbSrc = ConfigurationManager.AppSettings["DBSource"];
            FTDataAccess.DBUtility.PubConstant.ConnectionString = string.Format(@"{0}Initial Catalog=FangTAIZaojuA;User ID=sa;Password=123456;", dbSrc);
            this.labelUser.Text = "当前用户：" + this.userName;
           // System.Threading.Thread.Sleep(20000);
            ModuleAttach();
            
            //string licenseFile = AppDomain.CurrentDomain.BaseDirectory + @"FTLicense.lic";
            //this.licenseMonitor = new LicenseMonitor(this, 2000, licenseFile, "zzkeyFT1");
            //if (!this.licenseMonitor.StartMonitor())
            //{
            //    throw new Exception("license 监控失败");
            //}
            //string reStr = "";
            //if (!this.licenseMonitor.IslicenseValid(ref reStr))
            //{
            //    MessageBox.Show(reStr);
            //    return;
            //}
           
        }

        /// <summary>
        /// 模块加载
        /// </summary>
        private void ModuleAttach()
        {
            
            logView = new LogView("日志");
            nodeMonitorView = new NodeMonitorView("产线监控");
            nodeMonitorView.SetParent(this);
            nodeMonitorView.RegisterMenus(this.menuBar, "产线监控");

            logView.SetParent(this);
            logView.RegisterMenus(this.menuBar, "日志查询");
            logView.SetLogDispInterface(this);

            recordView = new RecordView();
            recordView.SetParent(this);
            recordView.RegisterMenus(this.menuBar, "记录查询与管理");
            recordView.SetLoginterface(logView.GetLogrecorder());
           

           // if(this.roleID <3 && this.roleID>0)
            {
                configView = new ConfiManageView();
                configView.SetParent(this);
                configView.RegisterMenus(this.menuBar, "配置管理");
                configView.SetLoginterface(logView.GetLogrecorder());
                
            }
            
            nodeMonitorView.SetLoginterface(logView.GetLogrecorder());
            nodeMonitorView.Init();
           
            logView.SetNodeNames(nodeMonitorView.GetNodeNames());
            logView.SetDebugMode(nodeMonitorView.IsDebugMode());
            AttachModuleView(nodeMonitorView);
        }
        #region 接口实现
        public string CurUsername { get { return this.userName; } }
        public int RoleID { get { return this.roleID; } }
        private delegate void DelegateDispLog(LogModel log);//委托，显示日志
        public void DispLog(LogModel log)
        {
            if(this.richTextBoxLog.InvokeRequired)
            {
                DelegateDispLog delegateLog = new DelegateDispLog(DispLog);
                this.Invoke(delegateLog, new object[] {log });
            }
            else
            {
                if (this.richTextBoxLog.Text.Count() > 10000)
                {
                    this.richTextBoxLog.Text = "";
                }

                this.richTextBoxLog.Text += (string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}]{1},{2}", log.LogTime, log.LogSource,log.LogContent) + "\r\n");
                //this.richTextBoxLog.Focus();
                //this.richTextBoxLog.Select(this.richTextBoxLog.TextLength, 0); 

                //this.richTextBoxLog.ScrollToCaret(); 
            }
            
        }
        public void AttachModuleView(System.Windows.Forms.Form childView)
        {
            TabPage tabPage = null;
           if(this.childList.Contains(childView.Text))
           {
               tabPage = this.MainTabControl.TabPages[childView.Text];
               this.MainTabControl.SelectedTab = tabPage;
               return;
           }
          
           this.MainTabControl.TabPages.Add(childView.Text, childView.Text);
           tabPage = this.MainTabControl.TabPages[childView.Text];
           tabPage.Controls.Clear();
           this.MainTabControl.SelectedTab = tabPage;
           childView.MdiParent = this;
           
           tabPage.Controls.Add(childView);
           this.childList.Add(childView.Text);
           childView.Dock = DockStyle.Fill;
           childView.Size = this.panelCenterview.Size;
           childView.Show();
           
        }
        #endregion
        #region ILicenseNotify接口实现
        public void ShowWarninfo(string info)
        {
            //LogModel log = new LogModel("其它", info, EnumLoglevel.警告);
            //logView.GetLogrecorder().AddLog(log);

            Console.WriteLine(info);
        }
        public void LicenseInvalid(string warnInfo)
        {
            if (this.nodeMonitorView != null)
            {
                this.nodeMonitorView.SetEnabled(false);
            }
            Console.WriteLine(warnInfo);
            // LogModel log = new LogModel("其它", warnInfo, EnumLoglevel.警告);
            // logView.GetLogrecorder().AddLog(log);
        }
        public void LicenseReValid(string noteInfo)
        {
            if (this.nodeMonitorView != null)
            {
                this.nodeMonitorView.SetEnabled(true);
            }
           
            LogModel log = new LogModel("其它", noteInfo, EnumLoglevel.提示);
            logView.GetLogrecorder().AddLog(log);
        }
        #endregion
        #region UI事件

        private void panelCenterview_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                if(this.panelCenterview.Controls.Count<1)
                {
                    return;
                }
                Control child = this.panelCenterview.Controls[0];
                if (child != null)
                {
                    child.Size = this.panelCenterview.Size;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
               // throw;
            }
           
           
        }

        private void tabControlMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                Rectangle myTabRect = this.MainTabControl.GetTabRect(e.Index);

                //先添加TabPage属性   
                e.Graphics.DrawString(this.MainTabControl.TabPages[e.Index].Text
                , this.Font, SystemBrushes.ControlText, myTabRect.X + 2, myTabRect.Y + 2);

                myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2);
                myTabRect.Width = CLOSE_SIZE;
                myTabRect.Height = CLOSE_SIZE;
                //再画一个矩形框
                using (Pen p = new Pen(Color.Red))
                {

                    //  e.Graphics.DrawRectangle(p, myTabRect);
                }

                //填充矩形框
                //Color recColor = e.State == DrawItemState.Selected ? Color.Red : Color.Transparent;
                //using (Brush b = new SolidBrush(recColor))
                //{
                //    e.Graphics.FillRectangle(b, myTabRect);
                //}

                //画关闭符号
                using (Pen objpen = new Pen(Color.DarkGray, 1.0f))
                {
                    //"\"线
                    Point p1 = new Point(myTabRect.X + 3, myTabRect.Y + 3);
                    Point p2 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + myTabRect.Height - 3);
                    e.Graphics.DrawLine(objpen, p1, p2);

                    //"/"线
                    Point p3 = new Point(myTabRect.X + 3, myTabRect.Y + myTabRect.Height - 3);
                    Point p4 = new Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + 3);
                    e.Graphics.DrawLine(objpen, p3, p4);
                }

                e.Graphics.Dispose();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }
        }
        private void tabControlMain_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int x = e.X, y = e.Y;

                //计算关闭区域   
                Rectangle myTabRect = this.MainTabControl.GetTabRect(this.MainTabControl.SelectedIndex);

                myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2);
                myTabRect.Width = CLOSE_SIZE;
                myTabRect.Height = CLOSE_SIZE;

                //如果鼠标在区域内就关闭选项卡   
                bool isClose = x > myTabRect.X && x < myTabRect.Right
                 && y > myTabRect.Y && y < myTabRect.Bottom;

                if (isClose == true)
                {
                    if(this.MainTabControl.SelectedTab.Text == nodeMonitorView.Text)
                    {
                        return;
                    }
                    string tabText = this.MainTabControl.SelectedTab.Text;
                    this.childList.Remove(tabText);
                    this.MainTabControl.TabPages.Remove(this.MainTabControl.SelectedTab);
                  
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.richTextBoxLog.Text = string.Empty;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
           
        }


        #endregion

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.nodeMonitorView.OnExit())
            {
                e.Cancel = true;
            }
        }

        private void richTextBoxLog_TextChanged(object sender, EventArgs e)
        {
            this.richTextBoxLog.SelectionStart = this.richTextBoxLog.Text.Length; //Set the current caret position at the end
            this.richTextBoxLog.ScrollToCaret();
        }

        private void 切换用户ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                CommonPL.Login.LoginView2 logView2 = new Login.LoginView2();
                if (DialogResult.OK == logView2.ShowDialog())
                {
                    string tempUserName = "";
                    int tempRoleID = logView2.GetLoginRole(ref tempUserName);
                    if (tempRoleID < 1)
                    {
                        return;
                    }
                    this.roleID = tempRoleID;
                    this.userName = tempUserName;
                    this.labelUser.Text = "当前用户：" + this.userName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
        }

    }

}
