using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PLProcessModel;
using LogInterface;
using DevAccess;
using FTDataAccess.Model;
using FTDataAccess.BLL;
namespace LineNodes
{
    /// <summary>
    /// 一次试火控制节点
    /// </summary>
    public class NodeFireTryingA : CtlNodeBaseModel
    {
        protected string processName = "试火";
        GasConfigBll gasCfgBll = new GasConfigBll();
        DetectCodeDefBll detectCodeDefbll = new DetectCodeDefBll();
        ProductSizeCfgBll productCfgBll = new ProductSizeCfgBll();
        private short preCheckLoss = 32;
        private short bindUnexist = 64;
        private short gasCfgUnexist = 128; //气源配置不存在
        public override bool BuildCfg(System.Xml.Linq.XElement xe, ref string reStr)
        {
            if (!base.BuildCfg(xe, ref reStr))
            {
                return false;
            }
            this.dicCommuDataDB1[1].DataDescription = "1：检查OK，放行，2：NG，4：读卡/条码失败，未投产，8：需要检测，16：不需要检测,32：前面检测有不合格或漏检,64：产品未绑定";
            this.dicCommuDataDB1[2].DataDescription = "气源编号";
            for (int i = 0; i < 30; i++)
            {
                this.dicCommuDataDB1[3 + i].DataDescription = string.Format("条码[{0}]", i + 1);
            }
            this.dicCommuDataDB1[33].DataDescription = "0：允许流程开始，1:流程锁定";
            this.dicCommuDataDB2[1].DataDescription = "0:无板,1：有产品,2：空板";
            this.dicCommuDataDB2[2].DataDescription = "1：检测OK,2：检测NG";
            this.dicCommuDataDB2[3].DataDescription = "不合格项编码";
            return true;
        }
        public override bool ExeBusiness(ref string reStr)
        {

            if (!NodeStatParse(ref reStr))
            {
                return false;
            }
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
                //        break;
                //    }
                case 1:
                    {
                        DevCmdReset();
                        rfidUID = string.Empty;
                        this.currentStat.Status = EnumNodeStatus.设备空闲;
                        this.currentStat.ProductBarcode = "";
                        this.currentStat.StatDescribe = "设备空闲";
                        checkFinished = false;
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
                       // this.db1ValsToSnd[0] = (short)(this.db1ValsToSnd[0] | 64); //
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
                                
                               // if (this.nodeName == "二次试火")
                                if (GetType().ToString() == "LineNodes.NodeFireTryingB")
                                {
                                    db1ValsToSnd[0] |= db1StatNG;//
                                }
                                else
                                {
                                    db1ValsToSnd[0] |= bindUnexist;
                                }
                               // this.currentTaskPhase = 4;
                                this.currentStat.StatDescribe = "未投产";
                                logRecorder.AddDebugLog(nodeName, "未投产，rfid:" + rfidUID);
                                checkEnable = false; 
                                break;
                            }
                            productBind.currentNode = this.nodeName;
                            productBindBll.Update(productBind);
                            BarcodeFillDB1(productBind.productBarcode, 2);
                            int reDetectQuery = ReDetectQuery(productBind.productBarcode);
                            if (0 == reDetectQuery)
                            {
                                db1ValsToSnd[0] = db1StatCheckOK;
                                checkEnable = false;
                                logRecorder.AddDebugLog(nodeName, string.Format("{0}本地已经存在检验记录,检验结果：OK", productBind.productBarcode));
                                break;
                            }
                            else if (1 == reDetectQuery)
                            {
                                db1ValsToSnd[0] = db1StatNG;
                                checkEnable = false;
                                logRecorder.AddDebugLog(nodeName, string.Format("{0}本地已经存在检验记录,检验结果：NG", productBind.productBarcode));
                                break;
                            }

                            //状态赋条码, 
                            this.currentStat.ProductBarcode = productBind.productBarcode;
                            logRecorder.AddDebugLog(this.nodeName, this.currentStat.ProductBarcode + "开始检测");
                            //查询本地数据库，之前工位是否有不合格项，若有，下线
                           // if (LineMonitorPresenter.checkPreStation)
                            {
                                if (!PreDetectCheck(productBind.productBarcode))
                                {
                                   // if (this.nodeName == "二次试火")
                                    if (GetType().ToString() == "LineNodes.NodeFireTryingB")
                                    {
                                        db1ValsToSnd[0] |= db1StatNG;//
                                    }
                                    else
                                    {
                                        db1ValsToSnd[0] |= preCheckLoss;//
                                    }

                                    logRecorder.AddDebugLog(this.nodeName, string.Format("{0} 在前面工位有检测NG项", productBind.productBarcode));
                                    checkEnable = false;
                                    break;
                                }
                            }

                            if (!LossCheck(productBind.productBarcode, ref reStr))
                            {
                                
                               // if (this.nodeName == "二次试火")
                                if (GetType().ToString() == "LineNodes.NodeFireTryingB")
                                {
                                    db1ValsToSnd[0] |= db1StatNG;//
                                }
                                else
                                {
                                    db1ValsToSnd[0] |= preCheckLoss;//
                                }
                                logRecorder.AddDebugLog(this.nodeName, string.Format("{0} 检测漏项,{1}", productBind.productBarcode, reStr));
                                checkEnable = false;
                               
                                break;
                            }
                           

                            short gasSeq = 0;
                            if (productBind.productBarcode.Trim().Length==26)
                            {
                                //新物料，直接查询产品配置
                                string cataCode = productBind.productBarcode.Substring(0, 13);
                                ProductSizeCfgModel m = productCfgBll.GetModel(cataCode);
                                if(m == null)
                                {
                                    this.ThrowErrorStat(productBind.productBarcode + "产品型号配置不存在", EnumNodeStatus.设备故障);
                                    //if (this.nodeName == "二次试火")
                                    if (GetType().ToString() == "LineNodes.NodeFireTryingB")
                                    {
                                        db1ValsToSnd[0] |= db1StatNG;//
                                    }
                                    else
                                    {
                                        db1ValsToSnd[0] |= gasCfgUnexist;
                                    }
                                    checkEnable = false;
                                    break;
                                }
                                GasConfigModel gasCfg = gasCfgBll.GetGascfgByName(m.gasName);
                                if (gasCfg == null)
                                {
                                    this.ThrowErrorStat(productBind.productBarcode + "气源配置不存在", EnumNodeStatus.设备故障);
                                 //   if (this.nodeName == "二次试火")
                                    if (GetType().ToString() == "LineNodes.NodeFireTryingB")
                                    {
                                        db1ValsToSnd[0] |= db1StatNG;//
                                    }
                                    else
                                    {
                                        db1ValsToSnd[0] |= gasCfgUnexist;
                                    }
                                    checkEnable = false;
                                    break;
                                }
                                gasSeq = (short)gasCfg.gasSeq;
                            }
                            else
                            {
                                this.ThrowErrorStat(productBind.productBarcode + "无效的条码", EnumNodeStatus.设备故障);
                                db1ValsToSnd[0] |= db1StatNG;//
                                checkEnable = false;
                              
                                break;
                            }
                            db1ValsToSnd[1] = gasSeq;
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
                         
                            if (db1ValsToSnd[0] != db1StatRfidFailed)
                            {
                                logRecorder.AddDebugLog(nodeName, "读RFID卡失败");
                            }
                            db1ValsToSnd[0] = db1StatRfidFailed;
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
                        int checkRe = 0;
                        string detectCodes = "";
                        if (db2Vals[1] == 1)
                        {
                            checkRe = 0;
                            //合格
                            //db1ValsToSnd[0] = db1StatCheckOK; 
                        }
                        else
                        {
                            checkRe = 1;
                            for (int i = 0; i < 16;i++ )
                            {
                                int codeIndex = i + 1;
                                DetectCodeDefModel m = detectCodeDefbll.GetModel(this.processName, codeIndex);
                                if(m != null)
                                {
                                    if((db2Vals[2]&(1<<i))>0)
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
                               // db1ValsToSnd[0] = db1StatNG;
                               
                                logRecorder.AddDebugLog(this.nodeName, "故障码：" + detectCodes);
                            }
                            //检测不合格，下线
                            OutputRecord(this.currentStat.ProductBarcode);
                                
                        }
                        currentTaskDescribe = "开始保存结果到本地";
                        if (!MesDatalocalSave(this.currentStat.ProductBarcode,checkRe, detectCodes, "", 0))
                        {
                            logRecorder.AddLog(new LogModel(this.nodeName, "保存检测数据到本地数据库失败", EnumLoglevel.警告));
                            break;
                        }

                        currentTaskDescribe = "开始上传结果到MES";
                        string[] mesProcessSeq = new string[] { "RQ-ZA230", "RQ-ZA240", "RQ-ZA220", "RQ-ZA250", "RQ-ZA251", "RQ-ZA252", "RQ-ZA254", "RQ-ZA260" };
                       // string[] mesProcessSeq = new string[] { this.mesNodeID };
                        if (!UploadMesdata(true,this.currentStat.ProductBarcode, mesProcessSeq, ref reStr))
                        {
                            this.currentStat.StatDescribe = "上传MES失败";
                            logRecorder.AddDebugLog(this.nodeName, this.currentStat.StatDescribe);
                            break;
                        }
                        this.currentStat.StatDescribe = "一次试火检测完成";
                        string checkReStr = "OK";
                        currentTaskDescribe = "放行";
                        if (0 == checkRe)
                        {
                            db1ValsToSnd[0] = db1StatCheckOK;
                        }
                        else
                        {
                            db1ValsToSnd[0] = db1StatNG;
                            checkReStr = "NG";
                        }
                        logRecorder.AddDebugLog(this.nodeName, this.currentStat.ProductBarcode + "检测完成," + checkReStr);
                        checkFinished = true;
                        currentTaskPhase++;
                        break;
                    }
                case 4:
                    {
                        //流程结束
                        currentTaskDescribe = "流程结束";
                        this.currentStat.StatDescribe = "流程完成";
                       // DevCmdReset();
                        this.currentTaskPhase++;
                       
                        break; 
                    }
                case 5:
                    {
                        this.currentTaskPhase++;
                        break;
                    }
                default:
                    break;
            }
            return true;
        }
    }
}
