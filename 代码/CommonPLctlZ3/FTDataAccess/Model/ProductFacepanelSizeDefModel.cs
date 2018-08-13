using System;
namespace FTDataAccess.Model
{
    /// <summary>
    /// ProductFacepanelSizeDefModel:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public partial class ProductFacepanelSizeDefModel
    {
        public ProductFacepanelSizeDefModel()
        { }
        #region Model
        private int _facepanelsize;
        private int _seq;
        private string _mark;
        private string _tag1;
        private string _tag2;
        /// <summary>
        /// 
        /// </summary>
        public int facePanelSize
        {
            set { _facepanelsize = value; }
            get { return _facepanelsize; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int seq
        {
            set { _seq = value; }
            get { return _seq; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string mark
        {
            set { _mark = value; }
            get { return _mark; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string tag1
        {
            set { _tag1 = value; }
            get { return _tag1; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string tag2
        {
            set { _tag2 = value; }
            get { return _tag2; }
        }
        #endregion Model

    }
}

