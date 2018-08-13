using System;
namespace FTDataAccess.Model
{
    /// <summary>
    /// ProduceRecordModel:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public partial class ProduceRecordModel
    {
        public ProduceRecordModel()
        { }
        #region Model
        private long _producerecordid;
        private string _productbarcode;
        private DateTime _inputtime;
        private DateTime? _outputtime;
        private bool _lineouted;
        private string _outputnode;
        /// <summary>
        /// 
        /// </summary>
        public long produceRecordID
        {
            set { _producerecordid = value; }
            get { return _producerecordid; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string productBarcode
        {
            set { _productbarcode = value; }
            get { return _productbarcode; }
        }
        /// <summary>
        /// 上线时间
        /// </summary>
        public DateTime inputTime
        {
            set { _inputtime = value; }
            get { return _inputtime; }
        }
        /// <summary>
        /// 下线时间
        /// </summary>
        public DateTime? outputTime
        {
            set { _outputtime = value; }
            get { return _outputtime; }
        }
        /// <summary>
        /// 是否已经下线（包括不合格品提前下线）
        /// </summary>
        public bool lineOuted
        {
            set { _lineouted = value; }
            get { return _lineouted; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string outputNode
        {
            set { _outputnode = value; }
            get { return _outputnode; }
        }
        #endregion Model

    }
}

