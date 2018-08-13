using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PLProcessModel;
using LogInterface;
using FTDataAccess.Model;
using DevInterface;
using DevAccess;
//using FTDataAccess.Model;
using FTDataAccess.BLL;
namespace LineNodes
{
    public class NodeFaceCheck:CtlNodeBaseModel
    {
        protected delegate bool DelegateSndPrinter(string productBarcode, ref string reStr);
        //private string barcode = "";
      //  private short mesDownDisable = 64;
        protected string processName = "外观检查";
        DetectCodeDefBll detectCodeDefbll = new DetectCodeDefBll();
        private IPrinterInfoDev prienterRW = null;
        //private ThreadBaseModel printinfoSndThread = null;
    //    private Queue<string> printBuf = new Queue<string>(); //贴标机发送队列
        private List<string> printList = new List<string>();//
        private object lockPrintbuf = new object();
        private DateTime rfidFailSt ;
        private bool rfidTimeCounterBegin = false; //RFID超时计时开始
        int checkRe = 0;
        public NodeFaceCheck()
        {
           
           
        }
        public List<string> PrintList { get { return printList; } }
        public IPrinterInfoDev PrienterRW { get { return prienterRW; } set { prienterRW = value; } }
        public override bool BuildCfg(System.Xml.Linq.XElement xe, ref string reStr)
        {
            if (!base.BuildCfg(xe, ref reStr))
            {
                return false;
            }
            this.dicCommuDataDB1[1].DataDescription = "1：检查OK，放行，2：NG，4：读卡/条码失败，未投产，8：需要检测，16：不需要检测，32：前面工序有NG,64：MES禁止下线";
            for (int i = 0; i < 30; i++)
            {
                this.dicCommuDataDB1[2 + i].DataDescription = string.Format("条码[{0}]", i + 1);
            }
            this.dicCommuDataDB1[32].DataDescription = "0：允许流程开始，1:流程锁定";
            this.dicCommuDataDB1[33].DataDescription = "3：放行（只有D2400=1，D2432=3时才放行）";
            this.dicCommuDataDB2[1].DataDescription = "0:无板,1：有产品,2：空板";
            this.dicCommuDataDB2[2].DataDescription = "1：检测OK,2：检测NG";
            this.dicCommuDataDB2[3].DataDescription = "不合格项编码";
            return true;
        }
        public override bool ExeBusiness(ref string reStr)
        {
            MessLossCheck();
            if (!NodeStatParse(ref reStr))
            {
                return false;
            }

            ////临时测试
            //if (PLProcessModel.SysCfgModel.PrienterEnable)
            //{
            //    string testBarcode = System.Guid.NewGuid().ToString();
            //    SendPrinterinfo(testBarcode);//异步发送
            //}
            ////临时
            if (!checkEnable)
            {
                return true;
            }
            
            switch (currentTaskPhase)
            {
                //case 0:
                //    {
                //        db1ValsToSnd[0] = db1StatCheckNoneed; //空板进入，放行
                //        if (this.currentStat.Status != EnumNodeStatus.工位有板)
                //        {
                //            logRecorder.AddDebugLog(nodeName, "空板，放行");
                //        }
                //        this.currentStat.Status = EnumNodeStatus.工位有板;
                //        this.currentStat.ProductBarcode = "";
                //        this.currentStat.StatDescribe = "空板";
                //        rfidTimeCounterBegin = false;
                //        break;
                //    }
                case 1:
                    {
                        DevCmdReset();
                        rfidUID = string.Empty;
                        this.currentStat.Status = EnumNodeStatus.设备空闲;
                        this.currentStat.ProductBarcode = "";
                        this.currentStat.StatDescribe = "设备空闲";
                        checkRe = 0;
                        checkFinished = false;
                        currentTaskDescribe = "等待有板信号";
                        rfidTimeCounterBegin = false;
                        break;
                    }
                case 2:
                    {
                      
                        db1ValsToSnd[31] = 1;//流程锁定
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
                                db1ValsToSnd[0]= db1StatNG;
                               // this.currentTaskPhase = 5;
                                this.currentStat.StatDescribe = "未投产";
                                logRecorder.AddDebugLog(nodeName, "未投产，rfid:" + rfidUID);
                                checkEnable = false; 
                                break;
                            }
                            productBind.currentNode = this.nodeName;
                            productBindBll.Update(productBind);
                            BarcodeFillDB1(productBind.productBarcode, 1);
                            int reDetectQuery = ReDetectQuery(productBind.productBarcode);
                            if(0== reDetectQuery)
                            {
                                db1ValsToSnd[0] = db1StatCheckOK;
                                checkEnable = false;
                                logRecorder.AddDebugLog(nodeName, string.Format("{0}本地已经存在检验记录,检验结果：OK", productBind.productBarcode));
                                break;
                            }
                            else if(1== reDetectQuery)
                            {
                                db1ValsToSnd[0] = db1StatNG;
                                checkEnable = false;
                                logRecorder.AddDebugLog(nodeName, string.Format("{0}本地已经存在检验记录,检验结果：NG", productBind.productBarcode));
                                break;
                            }
                           //reDetectQuery=2,无记录，继续后面的流程
                           
                            //状态赋条码, 
                            this.currentStat.ProductBarcode = productBind.productBarcode;
                            logRecorder.AddDebugLog(this.nodeName, this.currentStat.ProductBarcode + "开始检测");
                            //查询本地数据库，之前工位是否有不合格项，若有，下线

                            if (!PreDetectCheck(productBind.productBarcode))
                            {
                                db1ValsToSnd[0] = 32;//
                                logRecorder.AddDebugLog(this.nodeName, string.Format("{0} 在前面工位有检测NG项", productBind.productBarcode));
                                checkEnable = false;
                                break;
                            }
                          
                            if (!LossCheck(productBind.productBarcode, ref reStr))
                            {
                                db1ValsToSnd[0] = 32;//
                                logRecorder.AddDebugLog(this.nodeName, string.Format("{0} 检测漏项,{1}", productBind.productBarcode, reStr));
                                checkEnable = false;
                               
                                break;
                            }
                           
                            currentTaskPhase++;
                        }
                        else
                        {
                            if (!SysCfgModel.SimMode)
                            {
                                DateTime dtEnd = DateTime.Now;
                                string recvStr = (rfidRW as SgrfidRW).GetRecvBufStr();
                                string logStr = string.Format("读RFID失败，发送读卡命令:{0},接收判断时间:{1},接收数据:{2}", dtSt.ToString("HH:mm:ss"), dtEnd.ToString("HH:mm:ss"), recvStr);
                                logRecorder.AddDebugLog(nodeName, logStr);
                            }
                         
                            if(!rfidTimeCounterBegin)
                            {
                                //logRecorder.AddDebugLog(nodeName, "读RFID卡失败");
                                rfidFailSt = System.DateTime.Now;
                            }
                            rfidTimeCounterBegin = true;
                            TimeSpan ts = System.DateTime.Now - rfidFailSt;
                           
                            if(ts.TotalSeconds>SysCfgModel.RfidDelayTimeout)
                            {
                                if (db1ValsToSnd[0] != db1StatRfidFailed)
                                {
                                    logRecorder.AddDebugLog(nodeName, "读RFID卡失败");
                                }
                                db1ValsToSnd[0] = db1StatRfidFailed;
                                
                            }
                            
                            this.currentStat.Status = EnumNodeStatus.无法识别;
                            this.currentStat.StatDescribe = "读RFID卡失败";
                            break;
                        }
                       
                        break;
                    }
                case 3:
                    {
                        currentTaskDescribe = "等待检测结果";
                        if (db2Vals[1] == 0)
                        {
                            break;
                        }
                        string detectCodes = "";
                      
                        if (db2Vals[1] == 1)
                        {
                            //合格
                        
                            checkRe = 0;
                        }
                        else
                        {
                            checkRe = 1;

                            for (int i = 0; i < 16; i++)
                            {
                                int codeIndex = i + 1;
                                DetectCodeDefModel m = detectCodeDefbll.GetModel(this.processName, codeIndex);
                                if (m != null)
                                {
                                    if ((db2Vals[2] & (1 << i)) > 0)
                                    {
                                        detectCodes += (m.detectCode + ",");
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(detectCodes))
                            {
                                break;
                            }
                            else
                            {
                                if (detectCodes[detectCodes.Count() - 1] == ',')
                                {
                                    detectCodes = detectCodes.Remove(detectCodes.Count() - 1, 1);
                                }
                                //不合格
                               
                                logRecorder.AddDebugLog(this.nodeName, this.currentStat.ProductBarcode+",故障码：" + detectCodes);
                                OutputRecord(this.currentStat.ProductBarcode);
                              //  break;
                            }

                        }

                        currentTaskDescribe = "开始保存结果到本地";
                        if (!MesDatalocalSave(this.currentStat.ProductBarcode,checkRe, detectCodes, "", 0))
                        {
                            logRecorder.AddLog(new LogModel(this.nodeName, "保存检测数据到本地数据库失败", EnumLoglevel.警告));
                            break;
                        }
                        string[] mesProcessSeq = new string[] { "RQ-ZA230", "RQ-ZA240", "RQ-ZA220", "RQ-ZA250", "RQ-ZA251", "RQ-ZA252", "RQ-ZA254", "RQ-ZA260", "RQ-ZA270" };
                       // string[] mesProcessSeq = new string[] { this.mesNodeID };
                        currentTaskDescribe = "开始上传结果到MES";
                        if (!UploadMesdata(true,this.currentStat.ProductBarcode, mesProcessSeq, ref reStr))
                        {
                            this.currentStat.StatDescribe = "上传MES失败";
                            logRecorder.AddDebugLog(this.nodeName, this.currentStat.StatDescribe);
                            break;
                        }
                        currentTaskPhase++;
                      
                        break;
                    }
                case 4:
                    {
                        currentTaskDescribe = "放行";
                        this.currentStat.StatDescribe = "外观检测完成";
                        string checkreStr = "OK";
                        if (checkRe == 1)
                        {
                            checkreStr = "NG";
                            db1ValsToSnd[0] = db1StatNG;
                        }
                        else
                        {
                            db1ValsToSnd[0] = db1StatCheckOK;//允许下线
                            db1ValsToSnd[32] = 3; //两个放行信号
                            if (PLProcessModel.SysCfgModel.PrienterEnable)
                            {
                                SendPrinterinfo(this.currentStat.ProductBarcode);//异步发送
                            }
                       
                        }
                        logRecorder.AddDebugLog(this.nodeName, this.currentStat.ProductBarcode + "检测完成," + checkreStr);
                        this.currentStat.StatDescribe = "下线";
                        checkFinished = true;
                        currentTaskPhase++;
                        break;
                    }
                case 5:
                    {
                        this.currentStat.StatDescribe = "流程完成";
                        currentTaskDescribe = "流程结束";
                        //DevCmdReset();
                       
                        //checkFinished = true;
                        break;
                    }
               
                default:
                    break;
            }
            return true;
        }
        
        /// <summary>
        /// MES漏传检查，用于MES断网再恢复时
        /// </summary>
        private void MessLossCheck()
        {
            //test
            //string reStr1="";
            //System.Data.DataTable dt= mesDA.ReadMesDataTable("FT_MES_STEP_INFO",ref reStr1);
            //if (dt == null)
            //{
            //    Console.WriteLine(reStr1);
            //    return;
            //}
            if(SysCfgModel.MesOfflineMode)
            {
                return;
            }
  
            string[] mesProcessSeq = new string[] { "RQ-ZA041", "RQ-ZA040", "RQ-ZA030", "RQ-ZA050", "RQ-ZA051", "RQ-ZA052", "RQ-ZA053", "RQ-ZA060", "RQ-ZA070" };
            string strWhere = string.Format(" AutoStationName='{0}' and UPLOAD_FLAG=0", this.nodeName);

            List<LOCAL_MES_STEP_INFOModel> unUploads = localMesBasebll.GetModelList(strWhere);
            if(unUploads != null && unUploads.Count()>0)
            {
                foreach(LOCAL_MES_STEP_INFOModel infoModel in unUploads)
                {
                    string reStr = "";
                    if (!UploadMesdata(true, infoModel.SERIAL_NUMBER, mesProcessSeq, ref reStr))
                    {
                        logRecorder.AddDebugLog(this.nodeName, infoModel.SERIAL_NUMBER+",上传MES失败");
                        
                    }
                    
                }
            }
        }
        private void SendPrinterinfo(string productBarcode)
        {
            /*
            DelegateSndPrinter dlgt = new DelegateSndPrinter(AsySndPrinterinfo);
            string reStr = "";
            IAsyncResult ar = dlgt.BeginInvoke(productBarcode, ref reStr, null, dlgt);
             */
            lock(lockPrintbuf)
            {
                this.printList.Add(productBarcode);
               
            }
            logRecorder.AddDebugLog(nodeName, string.Format("{0}添加到待发送队列", productBarcode));
           
        }
        /// <summary>
        /// 异步发送条码给贴标机
        /// </summary>
        /// <param name="productBarcode"></param>
        /// <param name="reStr"></param>
        /// <returns></returns>
        private bool AsySndPrinterinfo(string productBarcode,ref string reStr)
        {
            if (!PLProcessModel.SysCfgModel.PrienterEnable)
            {
                reStr = "贴标机已经禁用";
                return true;
            }
            int mesRe = 0;
            if(!SysCfgModel.MesOfflineMode && PLProcessModel.SysCfgModel. MesCheckEnable)
            {
                mesRe = mesDA.MesAssemDown(new string[] { productBarcode, LineMonitorPresenter.mesLineID }, ref reStr);
            }
            
            int delayTimeOut = 600;//
            int queryInterval = 100;
            DateTime mesSt = DateTime.Now;
            while (0 != mesRe)
            {
                //this.currentStat.StatDescribe = productBarcode + ":MES禁止下线:" + reStr;
               // logRecorder.AddDebugLog(this.nodeName, this.currentStat.StatDescribe);
                MesStatRecord mesStat = NodePack.GetMequeryStat(productBarcode);
                if(mesStat != null)
                {
                    if (3 == mesStat.StatVal)
                    {
                        logRecorder.AddDebugLog(this.nodeName, productBarcode + ":MES禁止下线:" + reStr);
                        return false;
                    }
                }
                TimeSpan timeElapse = System.DateTime.Now - mesSt;
                if (timeElapse.TotalMilliseconds > delayTimeOut * 1000)
                {
                    break;
                }
                Thread.Sleep(queryInterval);
                mesRe = mesDA.MesAssemDown(new string[] { productBarcode, LineMonitorPresenter.mesLineID }, ref reStr);

            }
           if (0 == mesRe)
            {
                //PushBarcodeToBuf(productBarcode);
               bool re = prienterRW.SndBarcode(productBarcode, ref reStr);
               int reTryMax=20;
               int tryCounter = 0;
               while (!re)
               {
                   tryCounter++;
                   string failInfo = string.Format("给贴标机发送条码{0} 失败,错误信息：{1}", productBarcode, reStr);
                   logRecorder.AddDebugLog(nodeName, failInfo);
                   if (tryCounter > reTryMax)
                   {
                       break;
                   }
                   Thread.Sleep(1000);
                   re = prienterRW.SndBarcode(productBarcode, ref reStr);
                  
               }
               if (re)
               {
                   logRecorder.AddDebugLog(nodeName, "成功发送贴标条码：" + productBarcode+","+reStr);
                   return true;
               }
               else
               {
                   string failInfo = string.Format("给贴标机发送条码失败:{0},错误信息：{1}", productBarcode, reStr);
                   logRecorder.AddDebugLog(nodeName, failInfo);
                   return false;
               }
            }
            else
            {
                string logStr = productBarcode + ":MES禁止下线:" + reStr;
                logRecorder.AddDebugLog(this.nodeName, logStr);
                return false;
            }
        }
       
        /// <summary>
        /// 贴标队列处理,周期执行
        /// </summary>
        public void PrinterListProcess()
        {
            string productBarcode="";
            lock(lockPrintbuf)
            {
                if (!PLProcessModel.SysCfgModel.PrienterEnable)
                {
                    this.printList.Clear();
                    return;
                }
                if(this.printList.Count()==0)
                {
                    return;
                }
                productBarcode = this.printList[0];
            }
            int mesRe = 0;
            string reStr = "";//!NodeFactory.SimMode && 
            DateTime mesSt = DateTime.Now;
            try
            {
                if (!SysCfgModel.MesOfflineMode && PLProcessModel.SysCfgModel.MesCheckEnable)
                {
                    mesRe = mesDA.MesAssemDown(new string[] { productBarcode, LineMonitorPresenter.mesLineID }, ref reStr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
            int queryInterval = 100;
           
            while (mesRe != 0)
            {
                MesStatRecord mesStat = NodePack.GetMequeryStat(productBarcode);
                if (mesStat != null)
                {
                    if (3 == mesStat.StatVal)
                    {
                        logRecorder.AddDebugLog(this.nodeName, productBarcode + ":MES禁止下线:" + reStr);
                        break;
                    }
                }
                TimeSpan timeElapse = System.DateTime.Now - mesSt;
                if (timeElapse.TotalSeconds > (SysCfgModel.MesTimeout+10))
                {
                    break;
                }
                Thread.Sleep(queryInterval);
                mesRe = mesDA.MesAssemDown(new string[] { productBarcode, LineMonitorPresenter.mesLineID }, ref reStr);
            }
            if(0 == mesRe)
            {
                
                bool re = prienterRW.SndBarcode(productBarcode, ref reStr);
                if(!re)
                {
                    string failInfo = string.Format("给贴标机发送条码{0} 失败,错误信息：{1}", productBarcode, reStr);
                    logRecorder.AddDebugLog(nodeName, failInfo);
                }
                else
                {
                    lock(lockPrintbuf)
                    {
                        this.printList.Remove(productBarcode);
                    }
                   
                    logRecorder.AddDebugLog(nodeName, "成功发送贴标条码：" + productBarcode + "," + reStr);
                }
            }
            else
            {
                logRecorder.AddDebugLog(this.nodeName, productBarcode + ":MES下线查询超时，" + reStr);
                lock (lockPrintbuf)
                {
                    this.printList.Remove(productBarcode);
                }
                
            }
            
        }
        //private void PushBarcodeToBuf(string barCode)
        //{
        //    lock(lockPrintbuf)
        //    {
        //        printBuf.Enqueue(barCode);
        //        string logStr = string.Format("条码添加到发送队列：{0},当前队列长度：{1}", barCode, printBuf.Count);
        //        logRecorder.AddDebugLog(nodeName, logStr);
        //    }
        //}
        //private void SendPrintcodeFromBuf()
        //{
        //    lock(lockPrintbuf)
        //    {
        //        while(printBuf.Count>0)
        //        {
        //            Thread.Sleep(1000);
        //            string barCode = printBuf.Peek();
        //            string reStr = "";
        //            if (!prienterRW.SndBarcode(barCode, ref reStr))
        //            {
        //                //ThrowErrorStat("给贴标机发送条码失败," + reStr, EnumNodeStatus.设备故障);
        //                logRecorder.AddDebugLog(nodeName,"给贴标机发送条码失败," + reStr);
        //            }
        //            else
        //            {
        //                logRecorder.AddDebugLog(nodeName, "成功发送贴标条码：" + barCode);
        //                printBuf.Dequeue();
        //            }
        //        }
        //    }
        //}
    }
}
