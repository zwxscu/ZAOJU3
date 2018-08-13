using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PLProcessModel;
using LogInterface;
using FTDataAccess.Model;
using FTDataAccess.BLL;
using DevAccess;
using DevInterface;
namespace LineNodes
{
    /// <summary>
    /// 产品上线控制节点
    /// </summary>
    public class NodeProductInput:CtlNodeBaseModel
    {
        private IMesAccess mesDA = null;
        private ProductSizeCfgBll productCfgBll = new ProductSizeCfgBll();
       // private OnlineProductsBll onlineProductBll = null;
        public NodeProductInput()
        {
             //this.currentStat.ProductBarcode="BARCODE1234567";
           
            if(NodeFactory.SimMode)
            {
                mesDA = new MesDASim();
            }
            else
            {
                mesDA = new MesDA();
            }
           
        }

        public override bool BuildCfg(System.Xml.Linq.XElement xe, ref string reStr)
        {
            if(!base.BuildCfg(xe, ref reStr))
            {
                return false;
            }
            this.dicCommuDataDB1[1].DataDescription = "1：检查OK，放行，2：NG，4：读卡/条码失败，8：需要检测，16：不需要检测";
            for (int i = 0; i < 30;i++ )
            {
                this.dicCommuDataDB1[2+i].DataDescription = string.Format("条码[{0}]",i+1);
            }
            this.dicCommuDataDB1[32].DataDescription = "0：允许流程开始，1:流程锁定";

            this.dicCommuDataDB2[1].DataDescription = "0:无板,1：有产品,2：空板";
           
            return true;
        }
       // int tempCounter = 0;
        public override bool ExeBusiness(ref string reStr)
        {
            try
            {
                PLCRWMx plcRwObj = plcRW as PLCRWMx;

               // Console.WriteLine("A 产品上线 D3000={0}", PLCRWMx.db2Vals[0]);
                if (!NodeStatParse(ref reStr))
                {
                    return false;
                }

                switch (this.currentTaskPhase)
                {
                    //db1复位
                    //case 0:
                    //    {
                    //        db1ValsToSnd[0] = db1StatCheckNoneed; //空板进入，放行
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
                         //   this.currentStat.ProductBarcode = "";//
                            this.currentStat.StatDescribe = "设备空闲";
                            checkFinished = false;
                            //Array.Clear(db1ValsToSnd, 0, db1ValsToSnd.Count());
                            currentTaskDescribe = "等待有板信号";
                            break;
                        }
                    case 2:
                        {
                            db1ValsToSnd[31] = 1;//流程锁定
                            //if (this.currentStat.Status == EnumNodeStatus.设备故障)
                            //{
                            //    break;
                            //}
                           
                            this.currentStat.Status = EnumNodeStatus.设备使用中;
                            this.currentStat.ProductBarcode = "";
                            this.currentStat.StatDescribe = "设备使用中";

                            //读条码，rfid，绑定
                            string rfidUID = "";
                            currentTaskDescribe = "开始读RFID";
                            DateTime dtSt = System.DateTime.Now;
                            if (NodeFactory.SimMode)
                            {
                                rfidUID = this.SimRfidUID;
                            }
                            else
                            {
                                rfidUID = rfidRW.ReadUID();
                            }

                            if (string.IsNullOrEmpty(rfidUID))
                            {
                                DateTime dtEnd = DateTime.Now;
                                if(!SysCfgModel.SimMode)
                                {
                                    string recvStr = (rfidRW as SgrfidRW).GetRecvBufStr();
                                    string logRfidStr = string.Format("读RFID失败，发送读卡命令:{0},接收判断时间:{1},接收数据:{2}", dtSt.ToString("HH:mm:ss"), dtEnd.ToString("HH:mm:ss"), recvStr);
                                    logRecorder.AddDebugLog(nodeName, logRfidStr);
                                }
                                db1ValsToSnd[0] = db1StatRfidFailed;
                               
                                this.currentStat.StatDescribe = "读RFID卡失败！";
                                break;
                            }
                            currentTaskDescribe = "开始读条码";
                            string barcode = barcodeRW.ReadBarcode().Trim();
                            if (string.IsNullOrWhiteSpace(barcode))
                            {
                                db1ValsToSnd[0] = db1StatRfidFailed;
                                this.currentStat.StatDescribe = "读条码失败！";
                                this.currentStat.ProductBarcode = "读条码失败！";
                                currentTaskDescribe = "读条码失败！";
                                break;
                            }
                            if(barcode.Length!=26)
                            {
                                db1ValsToSnd[0] = db1StatRfidFailed;
                                this.currentStat.StatDescribe = "无效的条码，位数不足26位！";
                                this.currentStat.ProductBarcode = this.currentStat.StatDescribe;
                                currentTaskDescribe = this.currentStat.StatDescribe;
                                break;
                            }
                            string productTypeCode = barcode.Substring(0, 13);
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

                            //db1赋条码
                            BarcodeFillDB1(barcode, 1);
                            currentStat.ProductBarcode = barcode;
                            // 若已经存在，则解绑
                            if(!TryUnbind(rfidUID,barcode))
                            {
                                logRecorder.AddDebugLog(this.nodeName, "解绑错误");
                                currentTaskDescribe = "解绑错误";
                                break;
                            }
                            ClearLoacalMesData(barcode);
                           
                            //数据库绑定
                            if (onlineProductBll.Exists(barcode))
                            {
                                logRecorder.AddDebugLog(nodeName, "已经存在：" + barcode+",删除");
                                onlineProductBll.Delete(barcode);
                            }

                            OnlineProductsModel productBind = new OnlineProductsModel();
                            productBind.rfidCode = rfidUID;
                            productBind.productBarcode = barcode;
                            productBind.currentNode = this.nodeName;
                            productBind.inputTime = System.DateTime.Now;
                            onlineProductBll.Add(productBind);
                            //OnlineProductsModel productBind = onlineProductBll.GetModel(barcode);
                           
                            //else
                            //{
                            //    productBind.rfidCode = rfidUID;
                            //    productBind.inputTime = System.DateTime.Now;
                            //    productBind.currentNode = this.nodeName;
                            //    onlineProductBll.Update(productBind);
                            //}
                            
                            // Console.WriteLine("产品绑定，RFID UID:{0},整机条码：{1}", rfidUID, barcode);
                            if (LineMonitorPresenter.DebugMode)
                            {
                                string logStr = string.Format("产品绑定，RFID UID:{0},整机条码：{1}", rfidUID, barcode);
                                logRecorder.AddDebugLog(nodeName, logStr);
                                logRecorder.AddDebugLog(nodeName, "产品上线，绑定完成");
                            }
                            AddInputRecord(barcode);
                            currentTaskDescribe = "产品绑定完成";
                            db1ValsToSnd[0] = db1StatCheckOK;
                            checkFinished = true;
                            this.currentTaskPhase++;
                            break;
                        }
                    case 3:
                        {
                            //流程完成
                            this.currentStat.StatDescribe = "流程完成";
                            currentTaskDescribe = "流程完成";
                           // DevCmdReset();
                         //   this.currentTaskPhase = 1;
                        
                            break;
                        }
                   
                    default:
                        break;
                }
           //     this.currentStat.StatDescribe = "流程步号：" + currentTaskPhase.ToString();
                return true;
            }
            catch (Exception ex)
            {
                ThrowErrorStat(ex.ToString(), EnumNodeStatus.设备故障);
                return false;
            }
 
        }
        private void AddInputRecord(string productBarcode)
        {
            string strWhere = string.Format("productBarcode='{0}' and lineOuted = 0 order by inputTime desc ",productBarcode);
            List<ProduceRecordModel> recordList = produceRecordBll.GetModelList(strWhere);
            if (recordList == null || recordList.Count() < 1)
            {
                ProduceRecordModel record = new ProduceRecordModel();
                record.inputTime = System.DateTime.Now;
                record.lineOuted = false;
                record.productBarcode = productBarcode;
                produceRecordBll.Add(record);
            }
            else
            {
                ProduceRecordModel record = recordList[0];
                record.inputTime = System.DateTime.Now;
                record.outputNode = "";
                produceRecordBll.Update(record);
            }
        }
       
       private bool ClearLoacalMesData(string barcode)
        {
            string strWhere = string.Format("SERIAL_NUMBER='{0}'",barcode);
            bool re1 = localMesBasebll.DelByCondition(strWhere);
            bool re2 = localMesDetailbll.DelByCondition(strWhere);
            return (re1 && re2);
        }
    }
}
