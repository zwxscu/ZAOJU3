using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using FTDataAccess.DBUtility;//Please add references
namespace FTDataAccess.DAL
{
    /// <summary>
    /// 数据访问类:ProduceRecordModel
    /// </summary>
    public partial class ProduceRecordDal
    {
        public ProduceRecordDal()
        { }
        #region  Method

        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(long produceRecordID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from ProduceRecord");
            strSql.Append(" where produceRecordID=" + produceRecordID + " ");
            return DbHelperSQL.Exists(strSql.ToString());
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public long Add(FTDataAccess.Model.ProduceRecordModel model)
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSql1 = new StringBuilder();
            StringBuilder strSql2 = new StringBuilder();
            if (model.productBarcode != null)
            {
                strSql1.Append("productBarcode,");
                strSql2.Append("'" + model.productBarcode + "',");
            }
            if (model.inputTime != null)
            {
                strSql1.Append("inputTime,");
                strSql2.Append("'" + model.inputTime + "',");
            }
            if (model.outputTime != null)
            {
                strSql1.Append("outputTime,");
                strSql2.Append("'" + model.outputTime + "',");
            }
            if (model.lineOuted != null)
            {
                strSql1.Append("lineOuted,");
                strSql2.Append("" + (model.lineOuted ? 1 : 0) + ",");
            }
            if (model.outputNode != null)
            {
                strSql1.Append("outputNode,");
                strSql2.Append("'" + model.outputNode + "',");
            }
            strSql.Append("insert into ProduceRecord(");
            strSql.Append(strSql1.ToString().Remove(strSql1.Length - 1));
            strSql.Append(")");
            strSql.Append(" values (");
            strSql.Append(strSql2.ToString().Remove(strSql2.Length - 1));
            strSql.Append(")");
            strSql.Append(";select @@IDENTITY");
            object obj = DbHelperSQL.GetSingle(strSql.ToString());
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt64(obj);
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(FTDataAccess.Model.ProduceRecordModel model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update ProduceRecord set ");
            if (model.productBarcode != null)
            {
                strSql.Append("productBarcode='" + model.productBarcode + "',");
            }
            if (model.inputTime != null)
            {
                strSql.Append("inputTime='" + model.inputTime + "',");
            }
            if (model.outputTime != null)
            {
                strSql.Append("outputTime='" + model.outputTime + "',");
            }
            else
            {
                strSql.Append("outputTime= null ,");
            }
            if (model.lineOuted != null)
            {
                strSql.Append("lineOuted=" + (model.lineOuted ? 1 : 0) + ",");
            }
            if (model.outputNode != null)
            {
                strSql.Append("outputNode='" + model.outputNode + "',");
            }
            else
            {
                strSql.Append("outputNode= null ,");
            }
            int n = strSql.ToString().LastIndexOf(",");
            strSql.Remove(n, 1);
            strSql.Append(" where produceRecordID=" + model.produceRecordID + "");
            int rowsAffected = DbHelperSQL.ExecuteSql(strSql.ToString());
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(long produceRecordID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from ProduceRecord ");
            strSql.Append(" where produceRecordID=" + produceRecordID + "");
            int rowsAffected = DbHelperSQL.ExecuteSql(strSql.ToString());
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }		/// <summary>
        /// 批量删除数据
        /// </summary>
        public bool DeleteList(string produceRecordIDlist)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from ProduceRecord ");
            strSql.Append(" where produceRecordID in (" + produceRecordIDlist + ")  ");
            int rows = DbHelperSQL.ExecuteSql(strSql.ToString());
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public FTDataAccess.Model.ProduceRecordModel GetModel(long produceRecordID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1  ");
            strSql.Append(" produceRecordID,productBarcode,inputTime,outputTime,lineOuted,outputNode ");
            strSql.Append(" from ProduceRecord ");
            strSql.Append(" where produceRecordID=" + produceRecordID + "");
            FTDataAccess.Model.ProduceRecordModel model = new FTDataAccess.Model.ProduceRecordModel();
            DataSet ds = DbHelperSQL.Query(strSql.ToString());
            if (ds.Tables[0].Rows.Count > 0)
            {
                return DataRowToModel(ds.Tables[0].Rows[0]);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        public FTDataAccess.Model.ProduceRecordModel DataRowToModel(DataRow row)
        {
            FTDataAccess.Model.ProduceRecordModel model = new FTDataAccess.Model.ProduceRecordModel();
            if (row != null)
            {
                if (row["produceRecordID"] != null && row["produceRecordID"].ToString() != "")
                {
                    model.produceRecordID = long.Parse(row["produceRecordID"].ToString());
                }
                if (row["productBarcode"] != null)
                {
                    model.productBarcode = row["productBarcode"].ToString();
                }
                if (row["inputTime"] != null && row["inputTime"].ToString() != "")
                {
                    model.inputTime = DateTime.Parse(row["inputTime"].ToString());
                }
                if (row["outputTime"] != null && row["outputTime"].ToString() != "")
                {
                    model.outputTime = DateTime.Parse(row["outputTime"].ToString());
                }
                if (row["lineOuted"] != null && row["lineOuted"].ToString() != "")
                {
                    if ((row["lineOuted"].ToString() == "1") || (row["lineOuted"].ToString().ToLower() == "true"))
                    {
                        model.lineOuted = true;
                    }
                    else
                    {
                        model.lineOuted = false;
                    }
                }
                if (row["outputNode"] != null)
                {
                    model.outputNode = row["outputNode"].ToString();
                }
            }
            return model;
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public DataSet GetList(string strWhere)
        {
            StringBuilder strSql = new StringBuilder();
           // strSql.Append("select produceRecordID as 记录序号,productBarcode as 主机条码,inputTime as 投产时间,outputTime as 下线时间,lineOuted as 是否已经下线,outputNode 下线工位");
            strSql.Append("select  produceRecordID,productBarcode,inputTime,outputTime,lineOuted,outputNode");
            strSql.Append(" FROM ProduceRecord ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            return DbHelperSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获得前几行数据
        /// </summary>
        public DataSet GetList(int Top, string strWhere, string filedOrder)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select ");
            if (Top > 0)
            {
                strSql.Append(" top " + Top.ToString());
            }
            strSql.Append(" produceRecordID,productBarcode,inputTime,outputTime,lineOuted,outputNode ");
            strSql.Append(" FROM ProduceRecord ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            strSql.Append(" order by " + filedOrder);
            return DbHelperSQL.Query(strSql.ToString());
        }

        /// <summary>
        /// 获取记录总数
        /// </summary>
        public int GetRecordCount(string strWhere)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) FROM ProduceRecord ");
            if (strWhere.Trim() != "")
            {
                strSql.Append(" where " + strWhere);
            }
            object obj = DbHelperSQL.GetSingle(strSql.ToString());
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
        }
        /// <summary>
        /// 分页获取数据列表
        /// </summary>
        public DataSet GetListByPage(string strWhere, string orderby, int startIndex, int endIndex)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT * FROM ( ");
            strSql.Append(" SELECT ROW_NUMBER() OVER (");
            if (!string.IsNullOrEmpty(orderby.Trim()))
            {
                strSql.Append("order by T." + orderby);
            }
            else
            {
                strSql.Append("order by T.produceRecordID desc");
            }
            strSql.Append(")AS Row, T.*  from ProduceRecord T ");
            if (!string.IsNullOrEmpty(strWhere.Trim()))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            strSql.Append(" ) TT");
            strSql.AppendFormat(" WHERE TT.Row between {0} and {1}", startIndex, endIndex);
            return DbHelperSQL.Query(strSql.ToString());
        }

        /*
        */

        #endregion  Method
        #region  MethodEx

        #endregion  MethodEx
    }
}

