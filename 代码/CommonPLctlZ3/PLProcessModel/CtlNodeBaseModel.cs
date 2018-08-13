using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using FTDataAccess.BLL;
using FTDataAccess.Model;
using DevInterface;
using DevAccess;
using LogInterface;
namespace PLProcessModel
{
    
    /// <summary>
    /// 控制节点模型基类，定义了所有控制节点对象共有的接口
    /// </summary>
    public abstract class CtlNodeBaseModel : ILogRequired
    {
        protected delegate bool DelegateUploadMes(string productBarcode, string[] mesProcessSeq, ref string reStr);
        #region 私有数据
        protected const int db1StatCheckOK = 1;
        protected const int db1StatNG = 2;
        protected const int db1StatRfidFailed = 4;
        protected const int db1StatCheckneed = 8;
        protected const int db1StatCheckNoneed = 16;
        protected const int db1ResOK = 64;
        protected bool checkEnable = true;

        protected bool checkFinished = false;//检测完成
       // protected int taskPhase = 0; //流程步号（状态机）
        protected CtlNodeStatus currentStat;
        protected ILogRecorder logRecorder = null;
        protected string nodeID = "";
        protected string nodeName = "";
        protected string mesNodeID = "";
        protected string mesNodeName = "";
        protected CtlSequenceModel currentCtlSequence = null;
        protected IPlcRW plcRW = null;//设备的plc读写接口
        protected IrfidRW rfidRW = null;//rfid读写接口
        protected IBarcodeRW barcodeRW= null; //条码枪读写接口
        
        protected string db1StartAddr = ""; //db1 开始地址
        protected string db2StartAddr = ""; //db2 开始地址
        protected IDictionary<int, PLCDataDef> dicCommuDataDB1 = null;//通信功能项字典，DB1
        protected IDictionary<int, PLCDataDef> dicCommuDataDB2 = null;//通信功能项字典，DB2
        protected int currentTaskPhase = 1;//流程步号（状态机）,从1开始
        protected string  rfidUID = ""; //读到的rfid UID
        protected string currentTaskDescribe = "";// 当前任务描述
        protected Int16[] db1ValsToSnd = null; //db1待发送数据
        protected Int16[] db1ValsReal = null; //PLC 实际DB1数据
        protected Int16[] db2Vals = null;
        /// <summary>
        /// DB1数据区的锁
        /// </summary>
        private object lockDB1 = new object();

        /// <summary>
        /// DB2数据区的锁
        /// </summary>
        private object lockDB2 = new object();

        protected OnlineProductsBll productBindBll = null;
        protected LOCAL_MES_STEP_INFOBll mesInfoBllLocal = null;
        protected LOCAL_MES_STEP_INFO_DETAILBll mesDetailBllLocal = null;
        protected ProduceRecordBll produceRecordBll = null;
        protected LOCAL_MES_STEP_INFOBll localMesBasebll = null;
        protected LOCAL_MES_STEP_INFO_DETAILBll localMesDetailbll = null;
        protected MesDA mesDA = null;
        protected OnlineProductsBll onlineProductBll = null;
        #endregion
        #region 属性
        public string NodeID { get { return nodeID; } }
        public string NodeName { get { return nodeName; } }
        public IPlcRW PlcRW
        {
            get { return this.plcRW; }
            set { this.plcRW = value; }
        }
        public IrfidRW RfidRW
        {
            get { return rfidRW; }
            set { rfidRW = value; }
        }
        public IBarcodeRW BarcodeRW
        {
            get { return barcodeRW; }
            set { barcodeRW = value; }
        }
        public IDictionary<int, PLCDataDef> DicCommuDataDB1
        {
            get { return dicCommuDataDB1; }
            set { dicCommuDataDB1 = value; }
        }
        public IDictionary<int, PLCDataDef> DicCommuDataDB2
        {
            get { return dicCommuDataDB2; }
            set { dicCommuDataDB2 = value; }
        }
        public ILogRecorder LogRecorder { get { return logRecorder; } set { logRecorder = value; } }

        public CtlNodeStatus CurrentStat { get { return currentStat; } set { currentStat = value; } }
        public string SimRfidUID { get; set; }
        public string SimBarcode { get; set; }
        public bool SimMode { get; set; }
        #endregion
        #region 公开接口
        public CtlNodeBaseModel()
        {
           
            productBindBll = new OnlineProductsBll();
            mesInfoBllLocal = new LOCAL_MES_STEP_INFOBll();
            mesDetailBllLocal = new LOCAL_MES_STEP_INFO_DETAILBll();
            produceRecordBll = new ProduceRecordBll();
            mesDA = new MesDA();
            mesDA.SimMode = SysCfgModel.SimMode;
            if(SysCfgModel.MesTestMode)
            {
             //   mesDA.MesWS.Url = @"http://192.168.214.180:8333/soap/EventService?wsdl";
                mesDA.MesdbConnstr = @"Data Source=(DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.214.180)(PORT = 1521))
        (CONNECT_DATA =
          (SERVER = DEDICATED)
          (SERVICE_NAME = PRQMESDB)
        )
    );User Id=prqminda1;Password=prqminda1;Connection Timeout=5;";
            }
            else
            {
              //  mesDA.MesWS.Url = @"http://192.168.100.92:8123/soap/EventService?wsdl";
                mesDA.MesdbConnstr = @"Data Source=(DESCRIPTION =
    (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.100.94)(PORT = 1521))
        (CONNECT_DATA =
          (SERVER = DEDICATED)
          (SERVICE_NAME = PRQMESDB)
        )
    );User Id=prqminda1;Password=prqminda1;Connection Timeout=5;";
            }

            localMesBasebll = new LOCAL_MES_STEP_INFOBll();
            localMesDetailbll = new LOCAL_MES_STEP_INFO_DETAILBll();
            onlineProductBll = new OnlineProductsBll();
        }
        public virtual bool ReadDB1()
        {
            int blockNum = this.dicCommuDataDB1.Count();
            if (!SimMode)
            {

                if (SysCfgModel.PlcCommSynMode)
                {
                    short[] vals = null;
                    //同步通信
                    if (!plcRW.ReadMultiDB(db1StartAddr, blockNum, ref vals))
                    {
                        // refreshStatusOK = false;
                        ThrowErrorStat(this.nodeName + "读PLC数据(DB2）失败", EnumNodeStatus.设备故障);
                        return false;
                    }
                    for (int i = 0; i < blockNum; i++)
                    {
                        int commID = i + 1;
                        this.dicCommuDataDB1[commID].Val = vals[i];
                        this.db1ValsReal[i] = vals[i];
                    }
                }
                else
                {
                    int addrSt = int.Parse(this.db1StartAddr.Substring(1)) - 7000;

                    for (int i = 0; i < blockNum; i++)
                    {
                        int commID = i + 1;
                        this.db1ValsReal[i] = plcRW.Db1Vals[addrSt + i];//.db1v//plcRwMx.db1Vals[addrSt+i];
                        this.db1ValsToSnd[i] = this.db1ValsReal[i];
                        this.dicCommuDataDB1[commID].Val = this.db1ValsReal[i];

                    }
                }
            }
            else
            {
                for (int i = 0; i < blockNum; i++)
                {
                    this.db1ValsReal[i] = this.db1ValsToSnd[i];
                }
            }
            return true;


        }
        //public void WriteDB1()
        //{
        //    int blockNum = this.dicCommuDataDB1.Count();
        //    lock(lockDB1)
        //    {
        //        if (!SimMode)
        //        {
                    
        //            int addrSt = int.Parse(this.db1StartAddr.Substring(1)) - 2000;
        //            //Array.Copy(this.db1ValsToSnd,0,plcRwMx.db1Vals, addrSt, blockNum);

        //            for (int i = 0; i < blockNum; i++)
        //            {
        //                int commID = i + 1;
        //                this.dicCommuDataDB1[commID].Val = this.db1ValsToSnd[i];
        //                plcRW.Db1Vals[addrSt + i] = this.db1ValsToSnd[i];
        //                // this.db1ValsReal[i] = vals[i];
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < blockNum; i++)
        //            {
        //                this.db1ValsToSnd[i] = this.db1ValsReal[i];
        //            }
        //        }
        //    }
            
        //}
        /// <summary>
        /// 查询节点的状态数据（DB2）
        /// </summary>
        /// <param name="reStr"></param>
        /// <returns></returns>
        public bool ReadDB2(ref string reStr)
        {
          //  Console.WriteLine(mesDA.MesWS.Url);
            int blockNum = this.dicCommuDataDB2.Count();
          //  lock(lockDB2)
            {
               
                if(!SimMode)
                {
                    if (SysCfgModel.PlcCommSynMode)
                    {
                        //this.db2StartAddr = "D3000";//test
                        short[] vals = null;
                        //同步通信
                        if (!plcRW.ReadMultiDB(db2StartAddr, blockNum, ref vals))
                        {
                            // refreshStatusOK = false;
                            ThrowErrorStat(this.nodeName + "读PLC数据(DB2）失败", EnumNodeStatus.设备故障);
                           // logRecorder.AddDebugLog(this.nodeName, "读PLC数据(DB2）失败");
                            return false;
                        }
                        for (int i = 0; i < blockNum; i++)
                        {
                            int commID = i + 1;
                            this.dicCommuDataDB2[commID].Val = vals[i];
                            this.db2Vals[i] = vals[i];
                        }
                    }
                    else
                    {
                        int addrSt = int.Parse(this.db2StartAddr.Substring(1)) - int.Parse(SysCfgModel.DB2Start.Substring(1));

                        for (int i = 0; i < blockNum; i++)
                        {
                            this.db2Vals[i] = plcRW.Db2Vals[addrSt + i];
                            int commID = i + 1;
                            this.dicCommuDataDB2[commID].Val = this.db2Vals[i];
                            //this.db2Vals[i] = vals[i];
                        }
                    }
                   
                }
                else
                {
                    plcRW.ReadMultiDB(this.db2StartAddr, blockNum, ref this.db2Vals);
                    for (int i = 0; i < blockNum; i++)
                    {
                        int commID = i + 1;
                        this.db2Vals[i] = short.Parse(this.dicCommuDataDB2[commID].Val.ToString());
                    }
                }

                return true;
            }
           
        }
        public DataTable GetDB1DataDetail()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("索引");
            dt.Columns.Add("地址");
            dt.Columns.Add("内容");
            dt.Columns.Add("描述");
            int index = 1;

            for (int i = 0; i < dicCommuDataDB1.Count(); i++)
            {
                PLCDataDef commObj = dicCommuDataDB1[i + 1];
                dt.Rows.Add(index++, commObj.DataAddr, commObj.Val, commObj.DataDescription);
            }


            return dt;
        }
        public DataTable GetDB2DataDetail()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("索引");
            dt.Columns.Add("地址");
            dt.Columns.Add("内容");
            dt.Columns.Add("描述");
            int index = 1;
            for (int i = 0; i < dicCommuDataDB2.Count(); i++)
            {
                PLCDataDef commObj = dicCommuDataDB2[i + 1];

                dt.Rows.Add(index++, commObj.DataAddr, commObj.Val, commObj.DataDescription);
            }
            return dt;

        }
        /// <summary>
        /// 获取当前任务的详细信息
        /// </summary>
        /// <returns></returns>
        public string GetRunningTaskDetail()
        {
            string taskInfo = "当前任务运行到第" + currentTaskPhase.ToString() + " 步;";

            taskInfo += currentTaskDescribe;
            return taskInfo;


        }
        public void ClearErrorStat(string content)
        {
            this.currentStat.StatDescribe = content;
            this.currentStat.Status = EnumNodeStatus.设备空闲;
            LogModel log = new LogModel(this.nodeName, content, EnumLoglevel.提示);
            this.logRecorder.AddLog(log);
        }
        ///// <summary>
        ///// 根据PLC返回的数据，更新状态
        ///// </summary>
        //public void RefreshNodeStatus()
        //{
        //    //this.currentStat.ProductBarcode;
        //}
        public void ThrowErrorStat(string statDescribe, EnumNodeStatus statEnum)
        {
            if (statEnum != EnumNodeStatus.设备故障)
            {
                return;
            }
            if (this.currentStat.Status == EnumNodeStatus.设备空闲 || this.currentStat.Status == EnumNodeStatus.设备使用中)
            {
                //增加日志提示
                LogModel log = new LogModel(this.nodeName, statDescribe, EnumLoglevel.错误);
                this.logRecorder.AddLog(log);
            }
            this.currentStat.Status = statEnum;
            this.currentStat.StatDescribe = statDescribe;
        }
       
        #endregion
        #region 虚接口
        /// <summary>
        /// 业务逻辑，控制节点的流程执行
        /// </summary>
        /// <param name="reStr"></param>
        /// <returns></returns>
        public abstract bool ExeBusiness(ref string reStr);

        /// <summary>
        /// 控制流程的命令数据提交
        /// </summary>
        /// <param name="reStr"></param>
        /// <returns></returns>
        public virtual bool NodeCmdCommit(bool diffSnd, ref string reStr)
        {
            int blockNum = this.dicCommuDataDB1.Count();
            if (!SimMode)
            {
                
                if (SysCfgModel.PlcCommSynMode)
                {
                    //this.db1StartAddr = "D3000"; //test
                    if (!plcRW.WriteMultiDB(this.db1StartAddr, blockNum, this.db1ValsToSnd))
                    {
                        ThrowErrorStat("发送设备命令失败！", EnumNodeStatus.设备故障);
                        return false;
                    }
                    for (int i = 0; i < blockNum; i++)
                    {
                        int commID = i + 1;
                        this.dicCommuDataDB1[commID].Val = this.db1ValsToSnd[i];
                   
                    }
                }
                else
                {
                    int addrSt = int.Parse(this.db1StartAddr.Substring(1)) - int.Parse(SysCfgModel.DB1Start.Substring(1));
                    for (int i = 0; i < blockNum; i++)
                    {
                        int commID = i + 1;
                        this.dicCommuDataDB1[commID].Val = this.db1ValsToSnd[i];
                        plcRW.Db1Vals[addrSt + i] = this.db1ValsToSnd[i];
                        // this.db1ValsReal[i] = vals[i];
                    }
                }

            }
            else
            {
                plcRW.WriteMultiDB(this.db1StartAddr, blockNum, db1ValsToSnd);
                for (int i = 0; i < dicCommuDataDB1.Count(); i++)
                {
                    int commID = i + 1;
                    PLCDataDef commObj = dicCommuDataDB1[commID];
                    commObj.Val = db1ValsToSnd[i];

                }
            }

            return true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        public virtual bool BuildCfg(XElement xe,ref string reStr)
        {
            this.nodeID = xe.Attribute("id").Value;
            XElement baseDataXE = xe.Element("BaseDatainfo");
            if(baseDataXE == null)
            {
                reStr = this.nodeID + "，没有BaseDatainfo节点配置信息";
                return false;
            }
            this.nodeName = baseDataXE.Element("NodeName").Value;

            //mes nodeid，nodename
            XElement mesXE= baseDataXE.Element("MESDef");
            if(mesXE != null)
            {
                this.mesNodeID = mesXE.Attribute("id").Value;
                this.mesNodeName = mesXE.Attribute("name").Value;
            }
            XElement db1XE = baseDataXE.Element("DB1Addr");
            string db1StartStr = db1XE.Attribute("addrStart").Value;
            this.db1StartAddr = db1StartStr;
            int db1Start = int.Parse(db1StartStr.Substring(1));
            int db1BlockNum = int.Parse(db1XE.Attribute("blockNum").Value);
            int db1ID = 1;
            this.dicCommuDataDB1 = new Dictionary<int, PLCDataDef>();
            db1ValsReal = new Int16[db1BlockNum];
            db1ValsToSnd = new Int16[db1BlockNum];
            for (int i = 0; i < db1BlockNum; i++)
            {

                PLCDataDef commData = new PLCDataDef();
                commData.CommuID = db1ID++;
                commData.CommuMethod = EnumCommMethod.PLC_MIT_COMMU;
                commData.DataByteLen = 2;
                commData.DataDescription = "";
                commData.DataTypeDef = EnumCommuDataType.DEVCOM_short;
                commData.Val = 0;
                commData.DataAddr = "D" + (db1Start+i).ToString();
                dicCommuDataDB1[commData.CommuID] = commData;
                //db1Vals[i] = 0;
            }
            XElement db2XE = baseDataXE.Element("DB2Addr");
            string db2StartStr = db2XE.Attribute("addrStart").Value;
            this.db2StartAddr = db2StartStr;
            int db2Start = int.Parse(db2StartStr.Substring(1));
            int db2BlockNum = int.Parse(db2XE.Attribute("blockNum").Value);
            int db2ID = 1;
            this.dicCommuDataDB2 = new Dictionary<int, PLCDataDef>();
            db2Vals = new Int16[db2BlockNum];
            for (int i = 0; i < db2BlockNum; i++)
            {
                PLCDataDef commData = new PLCDataDef();
                commData.CommuID = db2ID++;
                commData.CommuMethod = EnumCommMethod.PLC_MIT_COMMU;
                commData.DataByteLen = 2;
                commData.DataDescription = "";
                commData.DataTypeDef = EnumCommuDataType.DEVCOM_short;
                commData.Val = 0;
                db2Vals[i] = 0;
                commData.DataAddr = "D" + (db2Start + i).ToString();
                dicCommuDataDB2[commData.CommuID] = commData;
            }

            this.currentStat = new CtlNodeStatus(nodeName);
            this.currentStat.Status = EnumNodeStatus.设备空闲;
            this.currentStat.StatDescribe = "空闲状态";
            return true;
        }
        public virtual void DevCmdReset()
        {
            Array.Clear(db1ValsToSnd, 0, db1ValsToSnd.Count());
            //string reStr = "";
            //NodeCmdCommit(false,ref reStr);
        }

        /// <summary>
        /// 下线记录（包括不合格检测，提前下线），若有重复，更新最新的
        /// </summary>
        /// <param name="productBarcode"></param>
        protected void OutputRecord(string productBarcode)
        {
            //按时间先后查询最后一条记录，更改记录状态，若查询结果为空，则返回
            string strWhere = string.Format("productBarcode='{0}' order by inputTime desc ",productBarcode);
            List<ProduceRecordModel> recordList = produceRecordBll.GetModelList(strWhere);
            if(recordList == null || recordList.Count()<1)
            {
                return;
            }
            recordList[0].outputTime = System.DateTime.Now;
            recordList[0].lineOuted = true;
            recordList[0].outputNode = this.nodeName;
            produceRecordBll.Update(recordList[0]);
        }
        //public void NodeLoop()
        //{
        //    try
        //    {

        //        string reStr = "";
        //        DateTime commSt = System.DateTime.Now;
        //        if (!ReadDB2(ref reStr))
        //        {
        //            return;
        //        }
        //        //DateTime commEd = System.DateTime.Now;
        //        //TimeSpan ts = commEd - commSt;
        //        //string dispCommInfo = string.Format("PLC 读通信周期:{0}毫秒", (int)ts.TotalMilliseconds);
        //        //if (ts.TotalMilliseconds > 100)
        //        //{
        //        //    node.LogRecorder.AddDebugLog(node.NodeName, dispCommInfo);
        //        //}

        //        //DateTime commEd = System.DateTime.Now;
        //        //TimeSpan ts = commEd - commSt;
        //        //string dispCommInfo = string.Format("PLC读状态周期:{0}毫秒", (int)ts.TotalMilliseconds);
        //        //if (ts.TotalMilliseconds > 500)
        //        //{
        //        //    node.LogRecorder.AddDebugLog(node.NodeName, dispCommInfo);
        //        //}

        //        if (!ExeBusiness(ref reStr))
        //        {
        //            return;
        //        }

        //        // commSt = System.DateTime.Now;
        //        if (!NodeCmdCommit(true, ref reStr))
        //        {
        //            return;
        //        }
        //        //commEd = System.DateTime.Now;
        //        //ts = commEd - commSt;
        //        //dispCommInfo = string.Format("PLC 发送通信周期:{0}毫秒", (int)ts.TotalMilliseconds);
        //        //if (ts.TotalMilliseconds > 500)
        //        //{
        //        //    node.LogRecorder.AddDebugLog(node.NodeName, dispCommInfo);
        //        //}
        //        DateTime commEd = System.DateTime.Now;
        //        TimeSpan ts = commEd - commSt;
        //        //   string dispCommInfo = string.Format("PLC控制周期:{0}毫秒", (int)ts.TotalMilliseconds);
        //        //  if (ts.TotalMilliseconds > 600)
        //        {
        //            // node.LogRecorder.AddDebugLog(node.NodeName, dispCommInfo);
        //            CurrentStat.StatDescribe = string.Format("周期:{0}毫秒", (int)ts.TotalMilliseconds);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(NodeName + ex.ToString());
        //        ThrowErrorStat(ex.ToString(), EnumNodeStatus.设备故障);
        //    }
        //}
        #endregion
        #region 内部功能接口
        

        /// <summary>
        /// db1 条码赋值
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="db1StIndex">db1地址块索引起始，从0开始编号</param>
        protected void BarcodeFillDB1(string barcode,int db1StIndex)
        {
            if(barcode.Count()>30)
            {
                barcode = barcode.Substring(0,30);
            }
            byte[] barcodeBytes = System.Text.UTF8Encoding.Default.GetBytes(barcode);
            for (int i = 0; i < barcodeBytes.Count(); i++)
            {
                db1ValsToSnd[db1StIndex + i] = barcodeBytes[i];
            }
            //string reStr="";
            //if(!NodeCmdCommit(false,ref reStr))
            //{
            //    ThrowErrorStat("PLC数据提交错误", EnumNodeStatus.设备故障);
            //}
        }

        /// <summary>
        /// 分析工位状态
        /// </summary>
        /// <param name="reStr"></param>
        /// <returns></returns>
        protected virtual bool NodeStatParse(ref string reStr)
        {
            if (db2Vals[0] == 0)
            {
                //if(this.nodeName== "产品上线")
                //{
                //    Console.WriteLine("产品上线：db2Vals[0]:{0}", db2Vals[0]);
                //}
                if (this.nodeID != "1001" && this.nodeID != "9001" && !checkFinished && checkEnable && (!string.IsNullOrWhiteSpace(this.currentStat.ProductBarcode)))
                {
                    //流程未执行完就流板，打印异常信息
                    string logInfo = string.Format(@"{0},流程未执行完“有板信号”就复位,db1[0]={1},db2[0]={2}", currentStat.ProductBarcode, db1ValsToSnd[0], db2Vals[0]);
                    logRecorder.AddDebugLog(nodeName, logInfo);
                }
                currentTaskPhase = 1; //复位
                checkEnable = true;
            }
            //else if (db2Vals[0] == 2)
            //{
            //    checkEnable = false;
            //    currentTaskPhase = 0;//

            //    this.currentStat.Status = EnumNodeStatus.工位有板;
            //    this.currentStat.ProductBarcode = "";
            //    this.currentStat.StatDescribe = "空板";
            //    reStr = "工位状态获取成功";
            //    return true;
            //}
            else if (db2Vals[0] == 1)
            {
                if (currentTaskPhase == 1 )
                {
                    currentTaskPhase = 2; //开始流程
                    checkEnable = true;
                }
              
            }
            else
            {
                ThrowErrorStat("PLC错误的状态数据", EnumNodeStatus.设备故障);
                return true;
            }
            return true;
        }

        protected virtual bool MesDatalocalSave(string productBarcode,int checkResult,string detectCodes,string dataValues,int step_mark)
        {
            string strWhere = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}' and UPLOAD_FLAG=0", productBarcode, this.nodeName);
            List<LOCAL_MES_STEP_INFOModel> unuploads = mesInfoBllLocal.GetModelList(strWhere);
            if(unuploads != null && unuploads.Count>0)
            {
                foreach(LOCAL_MES_STEP_INFOModel m in unuploads)
                {
                    if(!mesInfoBllLocal.Delete(m.RECID))
                    {
                        return false;
                    }
                }
            }
            //1 存储基本信息
            LOCAL_MES_STEP_INFOModel infoModel = new LOCAL_MES_STEP_INFOModel();
            infoModel.CHECK_RESULT = checkResult;
            infoModel.DEFECT_CODES = detectCodes;
            infoModel.LAST_MODIFY_TIME = System.DateTime.Now;
            infoModel.RECID = System.Guid.NewGuid().ToString();
            infoModel.SERIAL_NUMBER = productBarcode;
            infoModel.STEP_NUMBER = this.mesNodeID; //mes工位号
            infoModel.TRX_TIME = System.DateTime.Now;
            infoModel.UPLOAD_FLAG = false;
            infoModel.USER_NAME="";
            infoModel.STATUS = 0;
            infoModel.STEP_MARK = step_mark;
            infoModel.AutoStationName = this.nodeName;
            if(!mesInfoBllLocal.Add(infoModel))
            {
                return false;
            }
            //2存储细节数据
            List<LOCAL_MES_STEP_INFO_DETAILModel> unuploadsDetial = mesDetailBllLocal.GetModelList(strWhere);
            if(unuploadsDetial != null && unuploadsDetial.Count>0)
            {
                foreach(LOCAL_MES_STEP_INFO_DETAILModel m in unuploadsDetial)
                {
                    mesDetailBllLocal.Delete(m.RECID);
                }
            }
            LOCAL_MES_STEP_INFO_DETAILModel detailModel = new LOCAL_MES_STEP_INFO_DETAILModel();
            detailModel.DATA_NAME = this.mesNodeName; //
            detailModel.DATA_VALUE = dataValues;
            detailModel.LAST_MODIFY_TIME = System.DateTime.Now;
            detailModel.RECID = System.Guid.NewGuid().ToString();
            detailModel.SERIAL_NUMBER = currentStat.ProductBarcode;
            detailModel.STATUS = 0;
            detailModel.STEP_NUMBER = this.mesNodeID; //
            detailModel.TRX_TIME = System.DateTime.Now;
            detailModel.UPLOAD_FLAG = false;
            detailModel.AutoStationName = this.nodeName;
            
            if(!mesDetailBllLocal.Add(detailModel))
            {
                return false;
            }
           
            return true;
        }
        
        public bool UploadMesdata(bool syn,string productBarcode, string[] mesProcessSeq, ref string reStr)
        {
            if(SysCfgModel.MesOfflineMode)
            {
                reStr = "离线模式";
                return true;
            }
            if(syn)
            {
                return AsyUploadMesdata(productBarcode, mesProcessSeq, ref reStr);
            }
            else
            {
                DelegateUploadMes dlgt = new DelegateUploadMes(AsyUploadMesdata);
                IAsyncResult ar = dlgt.BeginInvoke(productBarcode, mesProcessSeq, ref reStr, null, dlgt);
                return true;
            }
           
        }
        protected bool AsyUploadMesdata(string productBarcode, string[] mesProcessSeq, ref string reStr)
        {
            try
            {
               
               // int uploadDelay = 2000;//上传延迟5秒
                List<string> strConditions = new List<string>();
                for (int i = 0; i < mesProcessSeq.Count(); i++)
                {
                    strConditions.Add(string.Format("SERIAL_NUMBER='{0}' and UPLOAD_FLAG = 0 and STEP_NUMBER='{1}' order by TRX_TIME asc", productBarcode, mesProcessSeq[i]));
                    string strWhere = strConditions[i];
                    List<LOCAL_MES_STEP_INFOModel> models = localMesBasebll.GetModelList(strWhere);
                    if (models == null || models.Count() < 1)
                    {
                        continue;
                    }
                    string process = mesProcessSeq[i];
                    if(process == "RQ-ZA240")
                    {
                        if(models.Count()>1)
                        {
                            LOCAL_MES_STEP_INFOModel model1 = models[0];
                            LOCAL_MES_STEP_INFOModel model2 = models[1];
                            LOCAL_MES_STEP_INFOModel uploadM = model1.Clone() as LOCAL_MES_STEP_INFOModel;

                            if(model1.CHECK_RESULT == 1 || model2.CHECK_RESULT == 1)
                            {
                                uploadM.CHECK_RESULT = 1;
                                if(model1.CHECK_RESULT == 1 && model2.CHECK_RESULT == 1)
                                {
                                    uploadM.DEFECT_CODES += (";" + model2.DEFECT_CODES);
                                }
                                else if (model2.CHECK_RESULT == 1)
                                {
                                    uploadM.DEFECT_CODES = model2.DEFECT_CODES;
                                }
                            }
                            
                            if(!UploadMesbasicData(uploadM))
                            {
                                return false;
                            }

                            model1.UPLOAD_FLAG = true;
                            model2.UPLOAD_FLAG = true;
                            localMesBasebll.Update(model1);
                            localMesBasebll.Update(model2);
                            //Thread.Sleep(uploadDelay);
                           
                        }
                        else if (models.Count()==1)
                        {
                            if(!UploadMesbasicData(models[0]))
                            {
                                return false;
                            }
                            models[0].UPLOAD_FLAG = true;
                            localMesBasebll.Update(models[0]);
                           // Thread.Sleep(uploadDelay);
                           
                        }
                    }
                    else
                    {
                        foreach (LOCAL_MES_STEP_INFOModel m in models)
                        {
                            if(!UploadMesbasicData(m))
                            {
                                return false;
                            }
                            m.UPLOAD_FLAG = true;
                            localMesBasebll.Update(m);
                           // Thread.Sleep(uploadDelay);
                           
                        }
                    }
                   
                }
                //for (int i = 0; i < strConditions.Count; i++)
                //{
                   

                //}
                for (int i = 0; i < strConditions.Count; i++)
                {
                    string strWhere = strConditions[i];
                    List<LOCAL_MES_STEP_INFO_DETAILModel> models = localMesDetailbll.GetModelList(strWhere);
                    if (models == null || models.Count() < 1)
                    {
                        continue;
                    }
                    string process = mesProcessSeq[i];
                    if (process == "RQ-ZA240")
                    {
                        if(models.Count()>1)
                        {
                            LOCAL_MES_STEP_INFO_DETAILModel model1 = models[0];
                            LOCAL_MES_STEP_INFO_DETAILModel model2 = models[1];
                            LOCAL_MES_STEP_INFO_DETAILModel uploadM = model1.Clone() as LOCAL_MES_STEP_INFO_DETAILModel;
                            uploadM.DATA_VALUE += (";" + model2.DATA_VALUE);
                            if(!UploadMesdetailData(uploadM))
                            {
                                return false;
                            }
                            model1.UPLOAD_FLAG = true;
                            model2.UPLOAD_FLAG = true;
                            localMesDetailbll.Update(model1);
                            localMesDetailbll.Update(model2);
                          //  Thread.Sleep(uploadDelay);
                        }
                        else if(models.Count()==1)
                        {
                            if(!UploadMesdetailData(models[0]))
                            {
                                return false;
                            }
                            models[0].UPLOAD_FLAG = true;
                            localMesDetailbll.Update(models[0]);
                           // Thread.Sleep(uploadDelay);
                        }
                    }
                    else
                    {
                        foreach (LOCAL_MES_STEP_INFO_DETAILModel m in models)
                        {
                            if(!UploadMesdetailData(m))
                            {
                                return false;
                            }
                            m.UPLOAD_FLAG = true;
                            localMesDetailbll.Update(m);
                           // Thread.Sleep(uploadDelay);
                        }
                    }
                  
                }
                return true;
            }
            catch (Exception ex)
            {
                reStr = ex.ToString();
                this.ThrowErrorStat(ex.ToString(), EnumNodeStatus.设备故障);
                return false;

            }
        }
        protected bool UploadMesbasicData(LOCAL_MES_STEP_INFOModel m)
        {
            
            //if (mesDA.MesBaseExist(m.RECID)) //？
            //{
            //    m.RECID = System.Guid.NewGuid().ToString(); 
            //}
           
            FT_MES_STEP_INFOModel ftM = new FT_MES_STEP_INFOModel();

            ftM.CHECK_RESULT = m.CHECK_RESULT;
            ftM.DEFECT_CODES = m.DEFECT_CODES;
            ftM.LAST_MODIFY_TIME =m.LAST_MODIFY_TIME;
            ftM.REASON = m.REASON;
            ftM.RECID = m.RECID;
            ftM.SERIAL_NUMBER = m.SERIAL_NUMBER;
            ftM.STATUS = m.STATUS;
            ftM.STEP_MARK = m.STEP_MARK;
            ftM.STEP_NUMBER = m.STEP_NUMBER;
            ftM.TRX_TIME = m.TRX_TIME;
            ftM.USER_NAME = m.USER_NAME;
            try
            {
                int reTryMax = 10;
                int reTryCounter = 0;
                while (!mesDA.AddMesBaseinfo(ftM))
                {
                    Thread.Sleep(1000);
                    reTryCounter++;
                    if (reTryCounter > reTryMax)
                    {
                        logRecorder.AddDebugLog(this.nodeName, string.Format("上传基本数据到MES失败,条码：{0},工位：{1}", ftM.SERIAL_NUMBER, ftM.STEP_NUMBER));
                        return false;
                    }
                }
                
                logRecorder.AddDebugLog(this.nodeName, string.Format("上传基本数据到MES成功,条码：{0},工位：{1}", ftM.SERIAL_NUMBER, ftM.STEP_NUMBER));
                if (!mesDA.MesBaseExist(ftM.RECID))
                {
                    logRecorder.AddDebugLog(this.nodeName, string.Format("MES数据未存在,条码：{0},工位：{1}", ftM.SERIAL_NUMBER, ftM.STEP_NUMBER));
                }
                return true;
            }
            catch (Exception ex)
            {
                logRecorder.AddDebugLog(this.nodeName, string.Format("上传基本数据到MES,数据库访问异常，条码：{0},工位：{1}，{2}", ftM.SERIAL_NUMBER, ftM.STEP_NUMBER, ex.Message));
                return false;
            }
        }
        protected bool UploadMesdetailData(LOCAL_MES_STEP_INFO_DETAILModel m)
        {
           
            //if (mesDA.MesDetailExist(m.RECID))
            //{
            //    m.RECID = System.Guid.NewGuid().ToString();
            //}
            FT_MES_STEP_INFO_DETAILModel ftM = new FT_MES_STEP_INFO_DETAILModel();
            ftM.DATA_NAME = m.DATA_NAME;
            ftM.DATA_VALUE = m.DATA_VALUE;
            ftM.LAST_MODIFY_TIME = m.LAST_MODIFY_TIME;
            ftM.RECID = m.RECID;
            ftM.SERIAL_NUMBER = m.SERIAL_NUMBER;
            ftM.STATUS = m.STATUS;
            ftM.STEP_NUMBER = m.STEP_NUMBER;
            ftM.TRX_TIME = m.TRX_TIME;
            try
            {
                int reTryMax = 10;
                int reTryCounter = 0;
                while (!mesDA.AddMesDetailinfo(ftM))
                {
                    Thread.Sleep(1000);
                    reTryCounter++;
                    if (reTryCounter > reTryMax)
                    {
                        logRecorder.AddDebugLog(this.nodeName, string.Format("上传详细数据到MES失败,条码：{0},工位：{1}", ftM.SERIAL_NUMBER, ftM.STEP_NUMBER));
                        return false;
                    }
                }
                logRecorder.AddDebugLog(this.nodeName, string.Format("上传详细数据到MES成功,条码：{0},工位：{1}", ftM.SERIAL_NUMBER, ftM.STEP_NUMBER));
                return true;
            }
            catch (Exception ex)
            {
                logRecorder.AddDebugLog(this.nodeName, string.Format("上传详细数据失败，MES数据库访问异常，条码：{0},工位：{1}，{2}", ftM.SERIAL_NUMBER, ftM.STEP_NUMBER, ex.Message));
                return false;
            } 
        }
        
        /// <summary>
        /// 漏项检查
        /// </summary>
        /// <param name="reStr"></param>
        /// <returns></returns>
        protected  bool LossCheck(string barCode,ref string reStr)
        {
            //List<string> strWheres = new List<string>();
            switch(this.nodeName)
            {
                case "气密检查2":
                    {
                        string strCond1 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'",barCode,"气密检查1");
                    //    strWheres.Add(strCond1);
                        string stName = "气密检查1";
                        if (!localMesBasebll.ExistByCondition(strCond1))
                        {
                            reStr = string.Format("检测数据漏项：{0}不存在",stName);
                            return false;
                        }
                        break;
                    }
                case "气密检查3":
                    {
                        string[] preStat = new string[]{ "气密检查1","气密检查2"};
                        foreach(string stName in preStat)
                        {
                            string strCond1 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'",barCode,stName);
                         //   strWheres.Add(strCond1);
                            if (!localMesBasebll.ExistByCondition(strCond1))
                            {
                                reStr = string.Format("检测数据漏项：{0}不存在", stName);
                                return false;
                            }
                        }
                        break;
                    }
                case "零秒点火":
                    {
                        string[] preStat = new string[]{ "气密检查1", "气密检查2", "气密检查3" };
                        foreach (string stName in preStat)
                        {
                            string strCond1 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'", barCode, stName);
                           // strWheres.Add(strCond1);
                             if (!localMesBasebll.ExistByCondition(strCond1))
                            {
                                reStr = string.Format("检测数据漏项：{0}不存在", stName);
                                return false;
                            }
                        }
                        break;
                    }
                case "一次试火:1":
                case "一次试火:2":
                case "一次试火:3":
                case "一次试火:4":
                    {
                        string[] preStat = new string[]{ "气密检查1", "气密检查2", "气密检查3","零秒点火" };
                        foreach (string stName in preStat)
                        {
                            string strCond1 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'", barCode, stName);
                          //  strWheres.Add(strCond1);
                            if(!localMesBasebll.ExistByCondition(strCond1))
                            {
                                reStr = string.Format("检测数据漏项：{0}不存在", stName);
                                return false;
                            }
                        }
                        break;
                    }
                case "二次试火:1":
                case "二次试火:2":
                    {
                        string[] preStat = new string[] { "气密检查1", "气密检查2", "气密检查3", "零秒点火" };
                        foreach (string stName in preStat)
                        {
                            string strCond1 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'", barCode, stName);
                        //    strWheres.Add(strCond1);
                            if (!localMesBasebll.ExistByCondition(strCond1))
                            {
                                reStr = string.Format("检测数据漏项：{0}不存在", stName);
                                return false;
                            }
                        }
                         string strCond2 = string.Format("SERIAL_NUMBER='{0}' and (AutoStationName='{1}' or AutoStationName='{2}' or AutoStationName='{3}' or AutoStationName='{4}' )",
                            barCode, "一次试火:1", "一次试火:2", "一次试火:3", "一次试火:4");
                     //   strWheres.Add(strCond2);
                        if (!localMesBasebll.ExistByCondition(strCond2))
                        {
                            reStr = string.Format("检测数据漏项：{0}不存在", "一次试火");
                            return false;
                        }
                        break;
                    }
                case "外观检查":
                    {
                        string[] preStat = new string[] { "气密检查1", "气密检查2", "气密检查3", "零秒点火" };
                        foreach (string stName in preStat)
                        {
                            string strCond1 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'", barCode, stName);
                      //      strWheres.Add(strCond1);
                            if (!localMesBasebll.ExistByCondition(strCond1))
                            {
                                reStr = string.Format("检测数据漏项：{0}不存在", stName);
                                return false;
                            }
                        }
                        string strCond2 = string.Format("SERIAL_NUMBER='{0}' and (AutoStationName='{1}' or AutoStationName='{2}' or AutoStationName='{3}' or AutoStationName='{4}' )",
                            barCode, "一次试火:1", "一次试火:2", "一次试火:3", "一次试火:4");
                    //    strWheres.Add(strCond2);
                        if (!localMesBasebll.ExistByCondition(strCond2))
                        {
                            reStr = string.Format("检测数据漏项：{0}不存在", "一次试火");
                            return false;
                        }
                        string strCond3 = string.Format("SERIAL_NUMBER='{0}' and (AutoStationName='{1}' or AutoStationName='{2}')", barCode, "二次试火:1", "二次试火:2");
                     //   strWheres.Add(strCond3);
                        if (!localMesBasebll.ExistByCondition(strCond3))
                        {
                            reStr = string.Format("检测数据漏项：{0}不存在", "二次试火");
                            return false;
                        }
                        break;
                    }
                case "装箱核对":
                    {
                        string[] preStat = new string[] { "气密检查1", "气密检查2", "气密检查3", "零秒点火" };
                        foreach (string stName in preStat)
                        {
                            string strCond1 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'", barCode, stName);
                            //      strWheres.Add(strCond1);
                            if (!localMesBasebll.ExistByCondition(strCond1))
                            {
                                reStr = string.Format("检测数据漏项：{0}不存在", stName);
                                return false;
                            }
                        }
                        string strCond2 = string.Format("SERIAL_NUMBER='{0}' and (AutoStationName='{1}' or AutoStationName='{2}' or AutoStationName='{3}' or AutoStationName='{4}' )",
                            barCode, "一次试火:1", "一次试火:2", "一次试火:3", "一次试火:4");
                        //    strWheres.Add(strCond2);
                        if (!localMesBasebll.ExistByCondition(strCond2))
                        {
                            reStr = string.Format("检测数据漏项：{0}不存在", "一次试火");
                            return false;
                        }
                        string strCond3 = string.Format("SERIAL_NUMBER='{0}' and (AutoStationName='{1}' or AutoStationName='{2}')", barCode, "二次试火:1", "二次试火:2");
                        //   strWheres.Add(strCond3);
                        if (!localMesBasebll.ExistByCondition(strCond3))
                        {
                            reStr = string.Format("检测数据漏项：{0}不存在", "二次试火");
                            return false;
                        }

                        string strCond4 = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'", barCode, "外观检查");
                        //   strWheres.Add(strCond3);
                        if (!localMesBasebll.ExistByCondition(strCond4))
                        {
                            reStr = string.Format("检测数据漏项：{0}不存在", "外观检查");
                            return false;
                        }
                        break;
                    }
                default:
                    break;
                  
            }
            return true;
           
        }
        protected bool PreDetectCheck(string barCode)
        {
           
            string strCond = string.Format("SERIAL_NUMBER='{0}' and CHECK_RESULT=1", barCode);
            
            if (localMesBasebll.ExistByCondition(strCond))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        protected int ReDetectQuery(string barcode)
        {
            int re = 0;
            string strWhere = string.Format("SERIAL_NUMBER='{0}' and AutoStationName='{1}'", barcode, this.nodeName);
            LOCAL_MES_STEP_INFOModel preCheckModel = localMesBasebll.GetLatestModel(strWhere);
            if (preCheckModel != null)
            {
                if (0 == preCheckModel.CHECK_RESULT)
                {
                    re = 0;
                }
                else
                {
                    re = 1;
                }
                
            }
            else
            {
                re = 2;//不存在记录
            }
            return re;
        }
        protected bool TryUnbind(string rfidCode, string barCode)
        {
            List<OnlineProductsModel> products = onlineProductBll.GetModelList(string.Format("rfidCode='{0}'", rfidCode));
            //   logRecorder.AddDebugLog(nodeName, "尝试解绑:" + rfidCode + "," + barCode);

            if (products != null && products.Count > 0)
            {
                foreach (OnlineProductsModel m in products)
                {
                    //  logRecorder.AddDebugLog(nodeName, "解绑rfid" + m.rfidCode);
                    if (!onlineProductBll.Delete(m.productBarcode))
                    {
                        return false;
                    }
                    else
                    {
                        logRecorder.AddDebugLog(nodeName, "解绑:" + rfidCode);
                    }
                }

            }
            if (onlineProductBll.Exists(barCode))
            {
                if (!onlineProductBll.Delete(barCode))
                {
                    return false;
                }
                else
                {
                    logRecorder.AddDebugLog(nodeName, "解绑:" + barCode);
                }
            }

            //logRecorder.AddDebugLog(nodeName, "解绑:" + rfidCode + "," + barCode);
            return true;
        }
        #endregion
    }

}
