using System;
using System.Data;
using System.Collections.Generic;
using FTDataAccess.Model;
namespace FTDataAccess.BLL
{
    /// <summary>
    /// ProductFacepanelSizeDefModel
    /// </summary>
    public partial class ProductFacepanelSizeDefBll
    {
        private readonly FTDataAccess.DAL.ProductFacepanelSizeDefDal dal = new FTDataAccess.DAL.ProductFacepanelSizeDefDal();
        public ProductFacepanelSizeDefBll()
        { }
        #region  BasicMethod

        /// <summary>
        /// 得到最大ID
        /// </summary>
        public int GetMaxId()
        {
            return dal.GetMaxId();
        }

        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(int facePanelSize)
        {
            return dal.Exists(facePanelSize);
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public bool Add(FTDataAccess.Model.ProductFacepanelSizeDefModel model)
        {
            return dal.Add(model);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(FTDataAccess.Model.ProductFacepanelSizeDefModel model)
        {
            return dal.Update(model);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(int facePanelSize)
        {

            return dal.Delete(facePanelSize);
        }
        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool DeleteList(string facePanelSizelist)
        {
            return dal.DeleteList(facePanelSizelist);
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public FTDataAccess.Model.ProductFacepanelSizeDefModel GetModel(int facePanelSize)
        {

            return dal.GetModel(facePanelSize);
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataSet GetList(string strWhere)
        {
            return dal.GetList(strWhere);
        }
        /// <summary>
        /// 获得前几行数据
        /// </summary>
        public DataSet GetList(int Top, string strWhere, string filedOrder)
        {
            return dal.GetList(Top, strWhere, filedOrder);
        }
        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<FTDataAccess.Model.ProductFacepanelSizeDefModel> GetModelList(string strWhere)
        {
            DataSet ds = dal.GetList(strWhere);
            return DataTableToList(ds.Tables[0]);
        }
        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<FTDataAccess.Model.ProductFacepanelSizeDefModel> DataTableToList(DataTable dt)
        {
            List<FTDataAccess.Model.ProductFacepanelSizeDefModel> modelList = new List<FTDataAccess.Model.ProductFacepanelSizeDefModel>();
            int rowsCount = dt.Rows.Count;
            if (rowsCount > 0)
            {
                FTDataAccess.Model.ProductFacepanelSizeDefModel model;
                for (int n = 0; n < rowsCount; n++)
                {
                    model = dal.DataRowToModel(dt.Rows[n]);
                    if (model != null)
                    {
                        modelList.Add(model);
                    }
                }
            }
            return modelList;
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataSet GetAllList()
        {
            return GetList("");
        }

        /// <summary>
        /// 分页获取数据列表
        /// </summary>
        public int GetRecordCount(string strWhere)
        {
            return dal.GetRecordCount(strWhere);
        }
        /// <summary>
        /// 分页获取数据列表
        /// </summary>
        public DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex)
        {
            return dal.GetListByPage(strWhere, orderby, startIndex, endIndex);
        }
        /// <summary>
        /// 分页获取数据列表
        /// </summary>
        //public DataSet GetList(int PageSize,int PageIndex,string strWhere)
        //{
        //return dal.GetList(PageSize,PageIndex,strWhere);
        //}

        #endregion  BasicMethod
        #region  ExtensionMethod

        #endregion  ExtensionMethod
    }
}

