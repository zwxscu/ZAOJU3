using System;
using System.Data;
using System.Collections.Generic;
using FTDataAccess.Model;
namespace FTDataAccess.BLL
{
    /// <summary>
    /// ProduceRecordModel
    /// </summary>
    public partial class ProduceRecordBll
    {
        private readonly FTDataAccess.DAL.ProduceRecordDal dal = new FTDataAccess.DAL.ProduceRecordDal();
        public ProduceRecordBll()
        { }
        #region  BasicMethod
        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(long produceRecordID)
        {
            return dal.Exists(produceRecordID);
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public long Add(FTDataAccess.Model.ProduceRecordModel model)
        {
            return dal.Add(model);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(FTDataAccess.Model.ProduceRecordModel model)
        {
            return dal.Update(model);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(long produceRecordID)
        {

            return dal.Delete(produceRecordID);
        }
        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool DeleteList(string produceRecordIDlist)
        {
            return dal.DeleteList(produceRecordIDlist);
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public FTDataAccess.Model.ProduceRecordModel GetModel(long produceRecordID)
        {

            return dal.GetModel(produceRecordID);
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
        public List<FTDataAccess.Model.ProduceRecordModel> GetModelList(string strWhere)
        {
            DataSet ds = dal.GetList(strWhere);
            return DataTableToList(ds.Tables[0]);
        }
        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<FTDataAccess.Model.ProduceRecordModel> DataTableToList(DataTable dt)
        {
            List<FTDataAccess.Model.ProduceRecordModel> modelList = new List<FTDataAccess.Model.ProduceRecordModel>();
            int rowsCount = dt.Rows.Count;
            if (rowsCount > 0)
            {
                FTDataAccess.Model.ProduceRecordModel model;
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
        public void ClearHistorydata()
        {
            if (dal.GetRecordCount("") > 100)
            {
                System.TimeSpan ts = new TimeSpan(15, 0, 0, 0); //15天
                System.DateTime delDate = System.DateTime.Now - ts;
                string strWhere = string.Format("delete from ProduceRecord where inputTime<'{0}'", delDate.ToString("yyyy-MM-dd"));
                FTDataAccess.DBUtility.DbHelperSQL.ExecuteSql(strWhere);
            }
        }
        #endregion  ExtensionMethod
    }
}

