using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevInterface;

namespace DevAccess
{
    /// <summary>
    /// plc读写模拟类
    /// </summary>
    public class PlcRWSim:IPlcRW
    {
        
        /// <summary>
        /// 连接断开事件
        /// </summary>
        public event EventHandler<PlcReLinkArgs> eventLinkLost;
        public Int64 PlcStatCounter { get { return 0; } }
        public int PlcID { get; set; }
        public int StationNumber { get; set; }
        public bool IsConnect
        {
            get
            {
                return true;
            }

        }
        public void Init()
        {
            
        }
        public void Exit()
        {
           
        }
        public bool ConnectPLC( ref string reStr)
        {
            reStr = "连接成功！";
            return true;
        }
        public bool CloseConnect()
        {
            return true;
        }
        public bool ReadDB(string addr, ref int val)
        {
            //PlcDBSimModel model = dbSimBll.GetModel(addr);
            //if (model == null)
            //{
            //    return false;
            //}
            //val = model.Val;
            val = 0;
            return true;
        }
        public bool ReadMultiDB(string addr, int blockNum, ref short[] vals)
        {
        
            vals = new short[blockNum];
            return true;
        }
        public bool WriteDB(string addr, int val)
        {
            //PlcDBSimModel model = dbSimBll.GetModel(addr);
            //if (model == null)
            //{
            //    return false;
            //}
            //model.Val = val;
            //return dbSimBll.Update(model);
            
            return true;

        }
        public bool WriteMultiDB(string addr, int blockNum, short[] vals)
        {
            
            return true;
        }
    }
}
