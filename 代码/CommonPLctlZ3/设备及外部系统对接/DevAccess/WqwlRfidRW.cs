using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevInterface;
namespace DevAccess
{
    public enum EnumTag
    {
        TagEPCC1G2,
        Tag180006B
    }
    public class WqwlRfidRW:IrfidRW
    {
        #region 数据
        
        private EnumTag tagType = EnumTag.TagEPCC1G2;
        private string readerIP = "";
        private uint readerPort = 0;
        private byte readerID = 0x00; //读写器ID
        private int readerSocket = -1;
        public byte[] AccPaswd { get; set; }
        public string HostIP { get; set; }
        public uint HostPort { get; set; }
        public byte ReaderID
        {
            get
            {
                return readerID;
            }
            set
            {
                readerID = value;
            }
        }
        public string readerNmae { get; set; }
        public bool IsOpened { get; set; }
        #endregion
        public WqwlRfidRW(EnumTag tagType,byte readerID, string ip, uint port)
        {
            this.readerID = readerID;
            this.tagType = tagType;
            this.readerIP = ip;
            this.readerPort = port;
            AccPaswd = new byte[4] { 0, 0, 0, 0 };
        }
        public bool Connect()
        {
            
            int res = Reader.Net_ConnectScanner(ref readerSocket,readerIP,readerPort,HostIP,HostPort);
            if(res==0)
            {
                Reader.Net_SetAntenna(readerSocket, 1);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Disconnect()
        {
            int res = Reader.DisconnectScanner(readerSocket);
            if(res == Reader._OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //读TID号，有效字6个
        public string ReadUID()
        {
           
            int nCounter = 0;
            int res = 0;
            string str = "";
           if(tagType == EnumTag.Tag180006B)
           {
               byte[,] TagNumber = new byte[100, 9];
               res = Reader.Net_ISO6B_ReadLabelID(readerSocket, TagNumber, ref nCounter);
              
               //for (j = 0; j < 8; j++)
               if(res != Reader._OK)
               {
                   return string.Empty;
               }
               for (int j = 1; j <= 8; j++)
               {
                   str += TagNumber[0, j].ToString("X2");
               }
               return str;
           }
           else
           {

               byte[] epcBytes = ReadLabelID();
               if(epcBytes == null)
               {
                   Console.WriteLine("读EPC，返回字节数据为空");
                   return string.Empty;
               }
               Console.WriteLine("EPC:" + BytesToString(epcBytes));
               byte EPC_Word = epcBytes[0];
               byte epcLen = (byte)(epcBytes[0]*2);
               byte[] epcData = new byte[epcLen];
               Array.Copy(epcBytes, 1, epcData, 0, epcLen);
               byte mem= 2; //TID
               byte ptr= 0;
               byte len = 6;
               byte[] DB = new byte[len*2];
               res = Reader.Net_EPC1G2_ReadWordBlock(readerSocket, EPC_Word, epcData, mem, ptr, len, DB, AccPaswd);
               if(res != Reader._OK)
               {
                   Console.WriteLine("读TID数据失败,返回："+res.ToString());
                   return string.Empty;
               }
               str = "";
               for (int j = 0; j < len * 2; j++)
               {
                   str += DB[j].ToString("X2");
               }
               return str;
           }
           
        }
        public int ReadData()
        {
            int data = -1;
            byte[] bytesData = ReadBytesData();
            if(bytesData == null)
            {
                return data;
            }
            else
            {
                data = BitConverter.ToInt32(bytesData, 0);
            }
            return data;
            //byte mem = 1;//epc区
            //byte ptr = 2; //从第4字节开始 
            //byte len = 2;
            //byte[] DB = new byte[4];
            //byte[] EpcBytes = ReadLabelID();
            //byte[] IDTemp = null;
            //byte EPC_Word = 0;
            //if (EpcBytes != null && EpcBytes.Count() > 1)
            //{
            //    EPC_Word = EpcBytes[0];
            //    IDTemp = new byte[EpcBytes.Count() - 1];
            //    Array.Copy(EpcBytes, 1, IDTemp, 0, EpcBytes.Count() - 1);
            //}
            //else
            //{
            //    return data;
            //}

            //int res = Reader.Net_EPC1G2_ReadWordBlock(readerSocket, EPC_Word, IDTemp, mem, ptr, len, DB, AccPaswd);
            //if (res != Reader._OK)
            //{
            //    return data;
            //}
            //data = BitConverter.ToInt32(DB,0);
            
            //byte[] EpcWord = null;
            //byte[] IDBuffer = null;
            //byte mem= 1; //1:EPC
            //byte ptr = 0;
            //byte len = 2;
            //byte[] UserDataBuf=new byte[32];
            //int res = Reader.Net_EPC1G2_ReadEPCandData(readerSocket, EpcWord, IDBuffer, mem, ptr, len, UserDataBuf);
            //if(res == Reader._OK)
            //{
            //    data = BitConverter.ToInt32(UserDataBuf);
            //}
            
           
      
        }
        public bool WriteData(int val)
        {
            //byte mem = 1;//epc区
            //byte ptr = 0;
            //byte len = 2;
            //byte[] DB = new byte[8];
            //byte[] EpcBytes = ReadLabelID();
            //byte[] IDTemp = null;
            //byte EPC_Word = 0;
            //if (EpcBytes != null && EpcBytes.Count() > 8)
            //{
            //    EPC_Word = EpcBytes[0];
            //    IDTemp = new byte[EpcBytes.Count() - 1];
            //    Array.Copy(EpcBytes, 1, IDTemp, 0, EpcBytes.Count() - 1);
            //}
            //else
            //{
            //    return false;
            //}
            byte[] byteVals= BitConverter.GetBytes(val);
            return WriteBytesData(byteVals);
            //int res = Reader.Net_EPC1G2_WriteEPC(readerSocket, 2, byteVlas, AccPaswd);
            //if(res == Reader._OK)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        public Int64 ReadDataInt64()
        {
            Int64 data = -1;
            byte[] bytesData = ReadBytesData();
            //byte[] EpcWord = null;
            //byte[] IDBuffer = null;
            //byte mem = 1; //1:EPC
            //byte ptr = 2;
            //byte len = 4;
            //byte[] UserDataBuf = null;
            //int res = Reader.Net_EPC1G2_ReadEPCandData(readerSocket, EpcWord, IDBuffer, mem, ptr, len, UserDataBuf);
            //if (res == Reader._OK)
            //{
            //    data = Convert.ToInt64(UserDataBuf);
            //}
            if (bytesData == null)
            {
                return data;
            }
            else
            {
                data = BitConverter.ToInt64(bytesData, 0);
            }
            return data;
        }
        

        //读EPC字节数据，最多8个字，可写部分6个字
        public byte[] ReadBytesData()
        {
            byte mem = 1;//epc区
            byte ptr = 2; //从第4字节开始 
            byte len = 6; //6个字
            byte[] DB = new byte[12];
            byte[] EpcBytes = ReadLabelID();
            byte[] IDTemp = null;
            byte EPC_Word = 0;
            if (EpcBytes != null && EpcBytes.Count() > 1)
            {
                EPC_Word = EpcBytes[0];
                IDTemp = new byte[EpcBytes.Count() - 1];
                Array.Copy(EpcBytes, 1, IDTemp, 0, EpcBytes.Count() - 1);
            }
            else
            {
                return null;
            }
            int res = Reader.Net_EPC1G2_ReadWordBlock(readerSocket, EPC_Word, IDTemp, mem, ptr, len, DB, AccPaswd);
            if (res != Reader._OK)
            {
                return null;
            }
            return DB;
        }
        public bool WriteDataInt64(Int64 val)
        {
            byte[] byteVlas = BitConverter.GetBytes(val);
            byte len = 4; 
            int res = Reader.Net_EPC1G2_WriteEPC(readerSocket, len, byteVlas, AccPaswd);
            if (res == Reader._OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool WriteBytesData(byte[] bytesData)
        {
            byte[] bytesDataToWriten = null;
            if(bytesData == null || bytesData.Count()<0)
            {
                return false;
            }
            bytesDataToWriten = bytesData;
            byte wordLen = (byte)(bytesData.Count() / 2);
            if(wordLen*2<bytesData.Count())
            {
                wordLen++;
                bytesDataToWriten = new byte[wordLen * 2];
                bytesDataToWriten[bytesData.Count()] = 0;
                Array.Copy(bytesData, 0, bytesDataToWriten, 0, bytesData.Count());
            }
            int res = Reader.Net_EPC1G2_WriteEPC(readerSocket, wordLen, bytesDataToWriten, AccPaswd);
            if (res == Reader._OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private byte[] ReadLabelID()
        {
            int mem = 1; //只有EPC有效
            int ptr = 0; //起始位
            int len = 0;
            int nCounter = 0;
            byte[] TagNumber = new byte[13];
            byte[] mask = new byte[13];
            //EPC，总共12字节，第0字节是字的长度（一个字占2字节)
          
            int res = Reader.Net_EPC1G2_ReadLabelID(readerSocket, mem, ptr, len, mask, TagNumber, ref nCounter);
            if(res == Reader._OK)
            {
                return TagNumber;
            }
            else
            {
                return null;
            }
        }
        private string BytesToString(byte[] byteStream)
        {
            string str = "";
            for(int i=0;i<byteStream.Count();i++)
            {
                str += byteStream[i].ToString("X2");
            }
            return str;
        }
    }
}
