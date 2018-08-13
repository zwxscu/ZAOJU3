using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using DevInterface;
using System.Threading;
namespace DevAccess
{
    public class BarcodeRWHonevor : IBarcodeRW
    {
        private const byte CR = 0x0D;
        private const byte SYN = 0x16;
        private const byte T = 0x54;
        private const byte U = 0x55;
        private int readerID = 1;
        private string recvBarcode = "";
        private Thread recvThread;
        private bool recvExit = false;
        private bool pauseFlag = false;
        public int recvInterval = 10;

        private List<byte> saveBuf = new List<byte>();
        public SerialPort ComPortObj { get; set; }
        public int ReaderID { get { return readerID; } }
        public BarcodeRWHonevor(int id)
        {
            this.readerID = id;
            recvExit = false;
            recvThread = new Thread(new ThreadStart(ComRecvProc));
            recvThread.IsBackground = true;
            recvThread.Priority = ThreadPriority.Highest;
            recvThread.Name = string.Format("条码枪{0}接收线程", this.readerID);


        }
        public bool StartMonitor(ref  string reStr)
        {
            try
            {
                if (this.ComPortObj != null)
                {
                    string[] ports = System.IO.Ports.SerialPort.GetPortNames();
                    if (!ports.Contains(this.ComPortObj.PortName))
                    {
                        reStr = string.Format("{0} 口不存在", this.ComPortObj.PortName);
                        return false;
                    }
                    if (!this.ComPortObj.IsOpen)
                    {
                        this.ComPortObj.Open();
                    }
                    this.pauseFlag = false;
                    if (this.recvThread.ThreadState == (ThreadState.Background | ThreadState.Unstarted))
                    {
                        recvThread.Start();
                    }

                    return true;
                }
                else
                {
                    reStr = "对象未创建";
                    return false;
                }
            }
            catch (Exception ex)
            {
                reStr = ex.ToString();
                return false;
            }

        }
        public bool StopMonitor()
        {
            this.pauseFlag = true;
            return true;
        }
        void SetScanTimeout(int timeOutMax)
        {

        }
        public string ReadBarcode()
        {
            saveBuf.Clear();
            //先清空上次条码
            recvBarcode = string.Empty;
            byte[] sndBuf = new byte[3] { SYN, T, CR };
            ComPortObj.Write(sndBuf, 0, 3);
            int timeCounter = 0;
            int reTryInterval = 100;
            int timeOut = 1000;
            while (timeCounter < timeOut)
            {
                if (!string.IsNullOrEmpty(recvBarcode))
                {
                    return recvBarcode;
                }
                Thread.Sleep(reTryInterval);

                timeCounter += reTryInterval;
            }
            return string.Empty;
        }
        private void ComRecvProc()
        {
            //string strRecv = serialPort.ReadExisting();

            byte[] buf = new byte[128];
            while (!recvExit)
            {
                Thread.Sleep(recvInterval);
                if (pauseFlag)
                {
                    continue;
                }
                if (!ComPortObj.IsOpen)
                {
                    continue;
                }
                // int recvLen = 0;
                //recvLen = ComPortObj.Read(buf, 0, 128);
                //this.recvBarcode = this.ComPortObj.ReadLine();
                int readNum = this.ComPortObj.Read(buf, 0, 128);
                if (readNum > 0)
                {
                    for (int i = 0; i < readNum; i++)
                    {
                        if (buf[i] == CR)
                        {
                            break;
                        }
                        if (saveBuf.Count() > 1024)
                        {
                            saveBuf.Clear();
                            break;
                        }
                        saveBuf.Add(buf[i]);
                    }
                    if (saveBuf.Count() > 0)
                    {
                        this.recvBarcode = System.Text.Encoding.UTF8.GetString(saveBuf.ToArray());
                    }

                }

            }
        }
    }
}
