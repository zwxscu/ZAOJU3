using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
namespace PLProcessModel
{
    public class SysCfgModel
    {
        public static bool PlcCommSynMode = false;//同步通信模式
        public static bool SimMode = false;//仿真模式
        public static bool MesTestMode = false;
        public static string DB1Start = "D2000";
        public static string DB2Start = "D3000";
        public static int DB1Len = 600;
        public static int DB2Len = 200;
       // public static bool MesInputCheck { get; set; }
        public static bool PreStationCheck { get; set; }

        public static bool MesCheckEnable { get; set; }

        public static bool MesOfflineMode { get; set; }
        public static bool PrienterEnable { get; set; }

        public static int MesTimeout { get; set; }
        public static int RfidDelayTimeout { get; set; }
        public static bool SaveCfg(ref string reStr)
        {
            try
            {
                 string xmlCfgFile = System.AppDomain.CurrentDomain.BaseDirectory + @"data/DevConfigFTzj.xml";
                if (!File.Exists(xmlCfgFile))
                {
                    reStr = "系统配置文件：" + xmlCfgFile + " 不存在!";
                    return false;
                }
                XElement root = XElement.Load(xmlCfgFile);
                XElement printerXE = root.Element("sysSet").Element("Printer");
                if(PrienterEnable)
                {
                    printerXE.Attribute("Enable").Value = "True";
                }
                else
                {
                    printerXE.Attribute("Enable").Value = "False";
                }
                XElement mesXE = root.Element("sysSet").Element("Mes");
                if( MesCheckEnable)
                {
                    mesXE.Attribute("Enable").Value="True";
                }
                else
                {
                    mesXE.Attribute("Enable").Value="False";
                }
                if(MesOfflineMode)
                {
                    mesXE.Attribute("OfflineMode").Value = "True";
                }
                else
                {
                    mesXE.Attribute("OfflineMode").Value = "False";
                }
                XElement mesTimeOutXE = root.Element("sysSet").Element("MesDownTimeout");
                mesTimeOutXE.Value = MesTimeout.ToString();
                XElement rfidTimeOutXE = root.Element("sysSet").Element("RfidTimeout");
                rfidTimeOutXE.Value = RfidDelayTimeout.ToString();
                root.Save(xmlCfgFile);
                return true;
            }
            catch (Exception ex)
            {
                reStr = ex.ToString();
                return false;
            }
        }
        public static bool LoadCfg(ref string reStr)
        {
            try
            {
                string xmlCfgFile = System.AppDomain.CurrentDomain.BaseDirectory + @"data/DevConfigFTzj.xml";
                if (!File.Exists(xmlCfgFile))
                {
                    reStr = "系统配置文件：" + xmlCfgFile + " 不存在!";
                    return false;
                }
                XElement root = XElement.Load(xmlCfgFile);
                XElement printerXE = root.Element("sysSet").Element("Printer");
                string str = printerXE.Attribute("Enable").Value.ToString().ToUpper();
                if (str == "TRUE")
                {
                    PrienterEnable = true;
                }
                else
                {
                    PrienterEnable = false;
                }
                XElement mesXE = root.Element("sysSet").Element("Mes");
                str = mesXE.Attribute("Enable").Value.ToString().ToUpper();
                if (str == "TRUE")
                {
                    MesCheckEnable = true;
                }
                else
                {
                    MesCheckEnable = false;
                }
                str = mesXE.Attribute("OfflineMode").Value.ToString().ToUpper();
                if(str== "TRUE")
                {
                    MesOfflineMode = true;
                }
                else
                {
                    MesOfflineMode = false;
                }
                XElement mesTimeOutXE = root.Element("sysSet").Element("MesDownTimeout");
                MesTimeout = int.Parse(mesTimeOutXE.Value.ToString());

                XElement rfidTimeOutXE = root.Element("sysSet").Element("RfidTimeout");
                RfidDelayTimeout=int.Parse(rfidTimeOutXE.Value.ToString());
                return true;
            }
            catch (Exception ex)
            {
                reStr = ex.ToString();
                return false;
            }
           
        }
    }
}
