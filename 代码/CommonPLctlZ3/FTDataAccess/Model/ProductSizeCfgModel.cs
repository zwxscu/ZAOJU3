using System;
using System.ComponentModel;
namespace FTDataAccess.Model
{
    /// <summary>
    /// ProductSizeCfgModel:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public partial class ProductSizeCfgModel : System.ComponentModel.INotifyPropertyChanged
    {
        public ProductSizeCfgModel()
        { }
        #region Model
        public event PropertyChangedEventHandler PropertyChanged;
        private int _cataseq;
        private string _productcatacode;
        private int _productheight;
        private string _mark;
        private string _packagesize;
        private string _productname;
        private string _gasname;
        private int? _basesizelevel;
        private int? _facepanelsize;
        private string _tag1;
        private string _tag2;
        private string _tag3;
        private string _tag4;
        private string _tag5;
     
        public int cataSeq
        {
            set { _cataseq = value; OnPropertyChanged("cataSeq"); }
            get { return _cataseq; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string productCataCode
        {
            set { _productcatacode = value; OnPropertyChanged("productCataCode"); }
            get { return _productcatacode; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int productHeight
        {
            set { _productheight = value; OnPropertyChanged("productHeight"); }
            get { return _productheight; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string mark
        {
            set { _mark = value; OnPropertyChanged("mark"); }
            get { return _mark; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string packageSize
        {
            set { _packagesize = value; OnPropertyChanged("packageSize"); }
            get { return _packagesize; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string productName
        {
            set { _productname = value; OnPropertyChanged("productName"); }
            get { return _productname; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string gasName
        {
            set { _gasname = value; OnPropertyChanged("gasName"); }
            get { return _gasname; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? baseSizeLevel
        {
            set { _basesizelevel = value; }
            get { return _basesizelevel; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? facePanelSize
        {
            set { _facepanelsize = value; }
            get { return _facepanelsize; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string tag1
        {
            set { _tag1 = value; OnPropertyChanged("tag1"); }
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
        /// <summary>
        /// 
        /// </summary>
        public string tag3
        {
            set { _tag3 = value; }
            get { return _tag3; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string tag4
        {
            set { _tag4 = value; }
            get { return _tag4; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string tag5
        {
            set { _tag5 = value; }
            get { return _tag5; }
        }
        #endregion Model
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

    }
}

