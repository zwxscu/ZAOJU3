using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DevAccess;
using PLProcessModel;
using LogInterface;
using FTDataAccess.Model;
using FTDataAccess.BLL;
namespace LineNodes
{
    public class MesStatRecord
    {
        public string ProductBarcode{get;set;}
        public DateTime StatModifyTime{get;set;}
        public int StatVal{get;set;}
        public MesStatRecord()
        {
            StatVal = 0;
            ProductBarcode="";
            StatModifyTime=System.DateTime.Now;
        }
    }
    /// <summary>
    /// 装箱校验控制节点
    /// </summary>
    public class NodePack:CtlNodeBaseModel
    {
        public static Dictionary<string, MesStatRecord> mesQueryStat = new Dictionary<string, MesStatRecord>(); //MES预下线状态,1:查询中，2：可以下线，3：禁止下线，按NG处理
        private bool graspBegin = false; //是否已经开始抓取
        private bool boxPrepareOK = false; //纸箱就绪
       // private short mesDownDisable = 8;
        private short barcodeCompareFailed = 16;
        private short mesDownedFlag = 64;
        private DateTime detectStartTime;//启动检测后开始计时
        private ProductSizeCfgBll productCfgBll = new ProductSizeCfgBll();
        private ProductHeightDefBll productHeightBll = new ProductHeightDefBll();
        public static object mesQueryLock = new object();
        public override bool BuildCfg(System.Xml.Linq.XElement xe, ref string reStr)
        {
            if (!base.BuildCfg(xe, ref reStr))
            {
                return false;
            }
            this.dicCommuDataDB1[1].DataDescription = "1：检查OK，放行，2：NG，4：读卡失败，未投产，8：MES禁止下线，16：纸箱条码校验错误";
            this.dicCommuDataDB1[2].DataDescription = "高度编号（从1开始）";
            for (int i = 0; i < 30; i++)
            {
                this.dicCommuDataDB1[3 + i].DataDescription = string.Format("条码[{0}]", i + 1);
            }
            this.dicCommuDataDB1[33].DataDescription = "0：允许流程开始，1:流程锁定";
            this.dicCommuDataDB1[34].DataDescription = "1:大底盘，2：小底盘";

            this.dicCommuDataDB2[1].DataDescription = "0:无板,1：有产品,2：空板";
            this.dicCommuDataDB2[2].DataDescription = "0:无纸箱,1：有纸箱";
            return true;
        }
        protected override bool NodeStatParse(ref string reStr)
        {
            //if(!base.NodeStatParse(ref reStr))
            //{
            //    return false;
            //}
            if(db2Vals[0] == 1)
            {
                if(!graspBegin)
                {
                    if (currentTaskPhase == 1)
                    {
                        currentTaskPhase = 2; //开始流程
                        checkEnable = true;
                    }
                    
                }
            }
            else if(db2Vals[0] == 0)
            {
                if(!graspBegin) //非抓取状态
                {
                    this.currentTaskPhase = 1;
                    checkEnable = true;
                    //DevCmdReset();
                    //rfidUID = string.Empty;
                    //this.currentStat.Status = EnumNodeStatus.设备空闲;
                    //this.currentStat.ProductBarcode = "";
                    //this.currentStat.StatDescribe = "设备空闲";
                }
            }
            return true;
        }
        public override bool ExeBusiness(ref string reStr)
        {
            DateTime commSt = System.DateTime.Now;
           // Console.WriteLine("P1");
            MessLossCheck();
          //  Console.WriteLine("P2");
            DateTime commEd = System.DateTime.Now;
            TimeSpan ts = commEd - commSt;
            if (ts.TotalMilliseconds >1000)
            {
                // node.LogRecorder.AddDebugLog(node.NodeName, dispCommInfo);
                CurrentStat.StatDescribe = string.Format("漏项检查周期:{0}毫秒", (int)ts.TotalMilliseconds);
            }

            if (!NodeStatParse(ref reStr))
            {
                return false;
            }
           // Console.WriteLine("P3");
            if (this.db2Vals[1] == 0 && boxPrepareOK)
            {
                //抓取完毕后，纸箱复位（主要是非正常流程下人工取走纸箱）
                DevCmdReset();
                this.currentStat.StatDescribe = "流程复位";
                currentTaskDescribe = "流程复位";
                graspBegin = false;//准备新的流程
                currentTaskPhase = 1;
            }
          
            //清理MES查询记录字典
            List<string> removeList = new List<string>();
            foreach (string key in NodePack.mesQueryStat.Keys)
            {
                MesStatRecord mesStat = NodePack.mesQueryStat[key];
                TimeSpan tmSpan = System.DateTime.Now - mesStat.StatModifyTime;
                if (tmSpan.TotalSeconds > 60)
                {
                    
                    removeList.Add(key);
                }
            }
         //   Console.WriteLine("P4");
            foreach(string key in removeList)
            {
                NodePack.mesQueryStat.Remove(key);
            }
           // Console.WriteLine("P5");
            if (!checkEnable)
            {
                return true;
            }
           
            switch (currentTaskPhase)
            {
                
                case 1:
                    {
                        DevCmdReset();
                        rfidUID = string.Empty;
                        this.currentStat.Status = EnumNodeStatus.设备空闲;
                        this.currentStat.ProductBarcode = "";
                        this.currentStat.StatDescribe = "设备空闲";
                        checkFinished = false;
                        boxPrepareOK = false;
                        graspBegin = false;
                        currentTaskDescribe = "等待有板信号";
                        break;
                    }
                case 2:
                    {
                        db1ValsToSnd[32] = 1;//流程锁定
                        
                        
                        if (this.currentStat.Status == EnumNodeStatus.设备故障)
                        {
                            break;
                        }
                        this.currentStat.Status = EnumNodeStatus.设备使用中;
                        this.currentStat.ProductBarcode = "";
                        this.currentStat.StatDescribe = "设备使用中";
                        //开始读卡
                        DateTime dtSt = System.DateTime.Now;
                        if (!SimMode)
                        {
                            rfidUID = rfidRW.ReadUID();
                        }
                        else
                        {
                            rfidUID = SimRfidUID;
                        }
                        currentTaskDescribe = "开始读RFID";
                        if (!string.IsNullOrWhiteSpace(rfidUID))
                        {
                            db1ValsToSnd[0] = 0;
                            this.currentStat.StatDescribe = "RFID识别完成";
                            //根据绑定，查询条码，赋条码
                            OnlineProductsModel productBind = productBindBll.GetModelByrfid(rfidUID);
                            if (productBind == null)
                            {
                                db1ValsToSnd[0] = db1StatNG;
                                this.currentStat.StatDescribe = "未投产";
                                currentTaskDescribe = "未投产";
                                //mesQueryStat[this.currentStat.ProductBarcode] = 3;//MES禁止下线
                                SetMesQueryStat(this.currentStat.ProductBarcode, 3);
                                checkEnable = false;
                                break;
                            }
                            productBind.currentNode = this.nodeName;
                            productBindBll.Update(productBind);

                            //状态赋条码, 
                            this.currentStat.ProductBarcode = productBind.productBarcode;
                            BarcodeFillDB1(productBind.productBarcode, 2);
                            SetMesQueryStat(this.currentStat.ProductBarcode, 1);

                            //检查是否已经下线,MES离线模式下不判断
                            if(!SysCfgModel.MesOfflineMode)
                            {
                                int mesDown = mesDA.MesDowned(this.currentStat.ProductBarcode,mesNodeID, ref reStr);
                                if (mesDown == 1)
                                {
                                    string logInfo = string.Format("条码重复，{0} 已经下线，请检查", this.currentStat.ProductBarcode);
                                    logRecorder.AddDebugLog(nodeName, logInfo);
                                    db1ValsToSnd[0] = mesDownedFlag;
                                    checkEnable = false;
                                    break;
                                }
                                else if (mesDown == 3)
                                {
                                    string logInfo = string.Format("MES数据库访问失败,无法查询是否已经下线，{0},{1}", this.currentStat.ProductBarcode, reStr);
                                    logRecorder.AddDebugLog(nodeName, logInfo);
                                }
                            }
                           
                            if (!PreDetectCheck(productBind.productBarcode))
                            {
                                if(db1ValsToSnd[0] != db1StatNG)
                                {
                                    logRecorder.AddDebugLog(this.nodeName, string.Format("{0} 在前面工位有检测NG项", productBind.productBarcode));
                                }
                                db1ValsToSnd[0] = db1StatNG;
                                currentTaskDescribe = string.Format("{0} 在前面工位有检测NG项", productBind.productBarcode);
                                SetMesQueryStat(this.currentStat.ProductBarcode, 3);//MES禁止下线
                              //  mesQueryStat.Remove(this.currentStat.ProductBarcode);
                                checkEnable = false;
                                break;
                            }
                            if (!LossCheck(productBind.productBarcode, ref reStr))
                            {
                                if (db1ValsToSnd[0] != db1StatNG)
                                {
                                    logRecorder.AddDebugLog(this.nodeName, string.Format("{0} 检测漏项,{1}", productBind.productBarcode, reStr));
                                }
                                currentTaskDescribe = string.Format("{0} 检测漏项,{1}", productBind.productBarcode, reStr);
                                db1ValsToSnd[0] = db1StatNG;
                                SetMesQueryStat(this.currentStat.ProductBarcode, 3);//MES禁止下线
                                //mesQueryStat.Remove(this.currentStat.ProductBarcode);
                                checkEnable = false;
                                break;
                            }
                            this.detectStartTime = System.DateTime.Now;
                            logRecorder.AddDebugLog(nodeName, "MES下线查询开始:" + this.currentStat.ProductBarcode);
                            currentTaskDescribe = "MES下线查询开始:" + this.currentStat.ProductBarcode;
                            currentTaskPhase++;
                        }
                        else
                        {
                            currentTaskDescribe = "读RFID卡失败";
                            if (!SysCfgModel.SimMode)
                            {
                                DateTime dtEnd = DateTime.Now;
                                string recvStr = (rfidRW as SgrfidRW).GetRecvBufStr();
                                string logStr = string.Format("读RFID失败，发送读卡命令:{0},接收判断时间:{1},接收数据:{2}", dtSt.ToString("HH:mm:ss"), dtEnd.ToString("HH:mm:ss"), recvStr);
                                logRecorder.AddDebugLog(nodeName, logStr);
                            }
                           
                            //if (db1ValsToSnd[0] != db1StatRfidFailed)
                            //{
                            //    logRecorder.AddDebugLog(nodeName, "读RFID卡失败");
                            //}
                            db1ValsToSnd[0] = db1StatRfidFailed;
                            this.currentStat.Status = EnumNodeStatus.无法识别;
                            this.currentStat.StatDescribe = "读RFID卡失败";

                            break;
                        }
                        
                        break;
                    }
                case 3:
                    {
                       
                        currentTaskDescribe = "开始查询MES下线是否允许";
                        int mesRe = 0;
                        if (!SysCfgModel.MesOfflineMode && PLProcessModel.SysCfgModel. MesCheckEnable)
                        {
                            mesRe = mesDA.MesAssemDown(new string[] { this.currentStat.ProductBarcode, LineMonitorPresenter.mesLineID }, ref reStr);
                        }
                        //int mesRe = mesDA.MesAssemDown(new string[] { this.currentStat.ProductBarcode, LineMonitorPresenter.mesLineID }, ref reStr);
                        if(0 != mesRe)
                        {
                            int delayTimeOut = SysCfgModel.MesTimeout;//20;//最多允许延迟10秒
                            TimeSpan timeElapse = System.DateTime.Now - detectStartTime;
                            if (timeElapse.TotalMilliseconds > delayTimeOut * 1000)
                            {
                                SetMesQueryStat(this.currentStat.ProductBarcode, 3);//MES禁止下线
                                this.currentStat.StatDescribe = string.Format("{0} :MES预下线查询超时({1}秒)，{2}", this.currentStat.ProductBarcode, delayTimeOut,reStr);
                                logRecorder.AddDebugLog(this.nodeName, currentStat.StatDescribe);
                                currentTaskDescribe = "MES下线允许超时";
                                //超时,通知外观检测工位
                                db1ValsToSnd[0] = db1StatNG;
                                checkEnable = false;
                                break;
                            }
                        }
                        else
                        {
                            logRecorder.AddDebugLog(this.nodeName, string.Format("{0} 下线允许", this.currentStat.ProductBarcode));
                            currentTaskDescribe = "MES下线允许";
                            SetMesQueryStat(this.currentStat.ProductBarcode, 2);

                            //下线，高度配方发完,MES入库
                            if (!MesDatalocalSave(this.currentStat.ProductBarcode, 0, "", "", 1))
                            {
                                logRecorder.AddLog(new LogModel(this.nodeName, "保存检测数据到本地数据库失败", EnumLoglevel.警告));
                                break;
                            }
                            string[] mesProcessSeq = new string[] { "RQ-ZA280" };
                            if (!UploadMesdata(true, this.currentStat.ProductBarcode, mesProcessSeq, ref reStr))
                            {
                                this.currentStat.StatDescribe = "上传MES失败";
                                logRecorder.AddDebugLog(this.nodeName, this.currentStat.StatDescribe);
                                currentTaskDescribe = "上传MES失败";
                                break;
                            }

                            //查询产品高度参数
                            string productTypeCode = "";

                            if (this.currentStat.ProductBarcode.Count() == 26)
                            {
                                productTypeCode = this.currentStat.ProductBarcode.Substring(0, 13);
                            }
                            else
                            {
                                productTypeCode = "";
                            }
                            ProductSizeCfgModel cfg = productCfgBll.GetModel(productTypeCode);
                            if (cfg == null)
                            {
                                db1ValsToSnd[0] = 32;
                                currentTaskDescribe = string.Format("产品未配置,物料码{0}", productTypeCode);
                                this.currentStat.StatDescribe = currentTaskDescribe;
                                this.currentStat.Status = EnumNodeStatus.设备故障;
                                checkEnable = false;

                                break;
                            }
                            //产品高度信息
                          //  ProductHeightDefModel heightDef = productHeightBll.GetModel(cfg.productHeight);
                            if (cfg.baseSizeLevel == null)
                            {
                                cfg.baseSizeLevel = 0;
                            }
                            db1ValsToSnd[33]=(short)cfg.baseSizeLevel;
                            db1ValsToSnd[1] = short.Parse(cfg.tag1);//(short)heightDef.heightSeq;
                            graspBegin = true;
                            currentTaskPhase++;
                        }
                        break;
                    }
                case 4:
                    {
                        currentTaskDescribe = "等待有板信号复位";
                        if(db2Vals[0] != 0)
                        {
                            //等待抓起
                            break;
                        }
                        //解绑
                        TryUnbind(this.rfidUID,this.currentStat.ProductBarcode);

                        DevCmdReset(); //
                       
  						currentTaskDescribe = "等待纸箱信号";
                        currentTaskPhase++;
                        break;
                    }
                case 5:
                    {
                        //读条码，做校验
                        
                        if (db2Vals[1] != 1) //纸箱到位后才启动条码枪
                        {
                            break;
                        }
                        boxPrepareOK = true;
                        //启用自动贴标功能
                        if(PLProcessModel.SysCfgModel.PrienterEnable)
                        {
                            string boxBarcode = "";
                            if(SysCfgModel.SimMode)
                            {
                                boxBarcode = SimBarcode;
                            }
                            else
                            {
                                boxBarcode = barcodeRW.ReadBarcode();
                            }
  
                            if (boxBarcode == string.Empty || boxBarcode.Length < 26)
                            {
                                db1ValsToSnd[0] = barcodeCompareFailed;
                                this.currentStat.StatDescribe = "无效的条码，位数不足26位！";
                                currentTaskDescribe = string.Format("纸箱条码校验错误，无效的条码，位数不足26位{0}", boxBarcode);
                                break;
                            }
                            //校验纸箱条码跟主机条码是否一致
                            if (boxBarcode != this.currentStat.ProductBarcode)
                            {
                                db1ValsToSnd[0] = barcodeCompareFailed;
                                this.currentStat.StatDescribe = "纸箱跟主机条码不同";
                                currentTaskDescribe = string.Format("纸箱条码校验错误，主机：{0},纸箱{1}", this.currentStat.ProductBarcode, boxBarcode);
                                break;
                            }
                            logRecorder.AddDebugLog(nodeName, "纸箱条码验证通过：" + boxBarcode);
                            currentTaskDescribe = "纸箱条码验证通过：" + boxBarcode;
                        }
                       
                        currentTaskPhase++;
                        break;
                    }
                case 6:
                    {
                        db1ValsToSnd[0] = db1StatCheckOK; //核对正确，允许搬运
                        this.currentStat.StatDescribe = "装箱核对检测完成";
                        currentTaskDescribe = "装箱核对检测完成";
                        checkFinished = true;
                        currentTaskPhase++;
                        break;
                    }
                case 7:
                    {
                        currentTaskDescribe = "等待纸箱信号复位";
                        if(db2Vals[1]!=0)
                        {
                            break;
                        }
                        logRecorder.AddDebugLog(nodeName, "入箱完成:" + this.currentStat.ProductBarcode);
                        DevCmdReset(); 
                        this.currentStat.StatDescribe = "流程完成";
                        currentTaskDescribe = "入箱完成";
                        graspBegin = false;//准备新的流程
                        boxPrepareOK = false;
                        currentTaskPhase = 1;
                        break;
                    }
              
                default:
                    break;
            }
            return true;
        }
        public static void SetMesQueryStat(string productBarcode,int val)
        {
            lock (mesQueryStat)
            {
                if(NodePack.mesQueryStat.Keys.Contains(productBarcode))
                {
                    NodePack.mesQueryStat[productBarcode].StatVal = val;
                    NodePack.mesQueryStat[productBarcode].ProductBarcode = productBarcode;
                    NodePack.mesQueryStat[productBarcode].StatModifyTime = System.DateTime.Now;
                }
                else
                {
                    MesStatRecord stat= new MesStatRecord();
                    stat.StatVal = val;
                    stat.StatModifyTime = System.DateTime.Now;
                    stat.ProductBarcode = productBarcode;
                    NodePack.mesQueryStat[productBarcode] = stat;

                }
                
            }
        }
        public static MesStatRecord GetMequeryStat(string productBarcode)
        {
            lock (mesQueryStat)
            {
                 if(NodePack.mesQueryStat.Keys.Contains(productBarcode))
                 {
                     return NodePack.mesQueryStat[productBarcode];
                 }
                 else
                 {
                     return null;
                 }

            }
        }

        private void MessLossCheck()
        {
            if (SysCfgModel.MesOfflineMode)
            {
                return;
            }
            string[] mesProcessSeq = new string[] { "RQ-ZA230", "RQ-ZA240", "RQ-ZA220", "RQ-ZA250", "RQ-ZA251", "RQ-ZA252", "RQ-ZA254", "RQ-ZA260", "RQ-ZA270", "RQ-ZA280" };
            string strWhere = string.Format(" AutoStationName='{0}' and UPLOAD_FLAG=0", this.nodeName);

            List<LOCAL_MES_STEP_INFOModel> unUploads = localMesBasebll.GetModelList(strWhere);
            if (unUploads != null && unUploads.Count() > 0)
            {
                foreach (LOCAL_MES_STEP_INFOModel infoModel in unUploads)
                {
                    string reStr = "";
                    if (!UploadMesdata(true, infoModel.SERIAL_NUMBER, mesProcessSeq, ref reStr))
                    {
                        logRecorder.AddDebugLog(this.nodeName, infoModel.SERIAL_NUMBER + ",上传MES失败");

                    }
                }
            }
        }
    }
}
