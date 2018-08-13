using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using FTDataAccess.DBUtility;//Please add references
namespace FTDataAccess.DAL
{
    /// <summary>
    /// 数据访问类:ProductFacepanelSizeDefModel
    /// </summary>
    public partial class ProductFacepanelSizeDefDal
    {
        public ProductFacepanelSizeDefDal()
        { }
        #region  BasicMethod

        /// <summary>
        /// 得到最大ID
        /// </summary>
        public int GetMaxId()
        {
            return (int)DbHelperSQL.GetMaxID("facePanelSize", "ProductFacepanelSizeDef");
        }

        /// <summary>
        /// 是否存在该记录
        /// </summary>
        public bool Exists(int facePanelSize)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select count(1) from ProductFacepanelSizeDef");
            strSql.Append(" where facePanelSize=@facePanelSize ");
            SqlParameter[] parameters = {
					new SqlParameter("@facePanelSize", SqlDbType.Int,4)			};
            parameters[0].Value = facePanelSize;

            return DbHelperSQL.Exists(strSql.ToString(), parameters);
        }


        /// <summary>
        /// 增加一条数据
        /// </summary>
        public bool Add(FTDataAccess.Model.ProductFacepanelSizeDefModel model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into ProductFacepanelSizeDef(");
            strSql.Append("facePanelSize,seq,mark,tag1,tag2)");
            strSql.Append(" values (");
            strSql.Append("@facePanelSize,@seq,@mark,@tag1,@tag2)");
            SqlParameter[] parameters = {
					new SqlParameter("@facePanelSize", SqlDbType.Int,4),
					new SqlParameter("@seq", SqlDbType.Int,4),
					new SqlParameter("@mark", SqlDbType.NVarChar,255),
					new SqlParameter("@tag1", SqlDbType.NVarChar,50),
					new SqlParameter("@tag2", SqlDbType.NVarChar,50)};
            parameters[0].Value = model.facePanelSize;
            parameters[1].Value = model.seq;
            parameters[2].Value = model.mark;
            parameters[3].Value = model.tag1;
            parameters[4].Value = model.tag2;

            int rows = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
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
        /// 更新一条数据
        /// </summary>
        public bool Update(FTDataAccess.Model.ProductFacepanelSizeDefModel model)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update ProductFacepanelSizeDef set ");
            strSql.Append("seq=@seq,");
            strSql.Append("mark=@mark,");
            strSql.Append("tag1=@tag1,");
            strSql.Append("tag2=@tag2");
            strSql.Append(" where facePanelSize=@facePanelSize ");
            SqlParameter[] parameters = {
					new SqlParameter("@seq", SqlDbType.Int,4),
					new SqlParameter("@mark", SqlDbType.NVarChar,255),
					new SqlParameter("@tag1", SqlDbType.NVarChar,50),
					new SqlParameter("@tag2", SqlDbType.NVarChar,50),
					new SqlParameter("@facePanelSize", SqlDbType.Int,4)};
            parameters[0].Value = model.seq;
            parameters[1].Value = model.mark;
            parameters[2].Value = model.tag1;
            parameters[3].Value = model.tag2;
            parameters[4].Value = model.facePanelSize;

            int rows = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
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
        /// 删除一条数据
        /// </summary>
        public bool Delete(int facePanelSize)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from ProductFacepanelSizeDef ");
            strSql.Append(" where facePanelSize=@facePanelSize ");
            SqlParameter[] parameters = {
					new SqlParameter("@facePanelSize", SqlDbType.Int,4)			};
            parameters[0].Value = facePanelSize;

            int rows = DbHelperSQL.ExecuteSql(strSql.ToString(), parameters);
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
        /// 批量删除数据
        /// </summary>
        public bool DeleteList(string facePanelSizelist)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from ProductFacepanelSizeDef ");
            strSql.Append(" where facePanelSize in (" + facePanelSizelist + ")  ");
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
        public FTDataAccess.Model.ProductFacepanelSizeDefModel GetModel(int facePanelSize)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("select  top 1 facePanelSize,seq,mark,tag1,tag2 from ProductFacepanelSizeDef ");
            strSql.Append(" where facePanelSize=@facePanelSize ");
            SqlParameter[] parameters = {
					new SqlParameter("@facePanelSize", SqlDbType.Int,4)			};
            parameters[0].Value = facePanelSize;

            FTDataAccess.Model.ProductFacepanelSizeDefModel model = new FTDataAccess.Model.ProductFacepanelSizeDefModel();
            DataSet ds = DbHelperSQL.Query(strSql.ToString(), parameters);
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
        public FTDataAccess.Model.ProductFacepanelSizeDefModel DataRowToModel(DataRow row)
        {
            FTDataAccess.Model.ProductFacepanelSizeDefModel model = new FTDataAccess.Model.ProductFacepanelSizeDefModel();
            if (row != null)
            {
                if (row["facePanelSize"] != null && row["facePanelSize"].ToString() != "")
                {
                    model.facePanelSize = int.Parse(row["facePanelSize"].ToString());
                }
                if (row["seq"] != null && row["seq"].ToString() != "")
                {
                    model.seq = int.Parse(row["seq"].ToString());
                }
                if (row["mark"] != null)
                {
                    model.mark = row["mark"].ToString();
                }
                if (row["tag1"] != null)
                {
                    model.tag1 = row["tag1"].ToString();
                }
                if (row["tag2"] != null)
                {
                    model.tag2 = row["tag2"].ToString();
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
            strSql.Append("select facePanelSize,seq,mark,tag1,tag2 ");
            strSql.Append(" FROM ProductFacepanelSizeDef ");
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
            strSql.Append(" facePanelSize,seq,mark,tag1,tag2 ");
            strSql.Append(" FROM ProductFacepanelSizeDef ");
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
            strSql.Append("select count(1) FROM ProductFacepanelSizeDef ");
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
                strSql.Append("order by T.facePanelSize desc");
            }
            strSql.Append(")AS Row, T.*  from ProductFacepanelSizeDef T ");
            if (!string.IsNullOrEmpty(strWhere.Trim()))
            {
                strSql.Append(" WHERE " + strWhere);
            }
            strSql.Append(" ) TT");
            strSql.AppendFormat(" WHERE TT.Row between {0} and {1}", startIndex, endIndex);
            return DbHelperSQL.Query(strSql.ToString());
        }

        /*
        /// <summary>
        /// 分页获取数据列表
        /// </summary>
        public DataSet GetList(int PageSize,int PageIndex,string strWhere)
        {
            SqlParameter[] parameters = {
                    new SqlParameter("@tblName", SqlDbType.VarChar, 255),
                    new SqlParameter("@fldName", SqlDbType.VarChar, 255),
                    new SqlParameter("@PageSize", SqlDbType.Int),
                    new SqlParameter("@PageIndex", SqlDbType.Int),
                    new SqlParameter("@IsReCount", SqlDbType.Bit),
                    new SqlParameter("@OrderType", SqlDbType.Bit),
                    new SqlParameter("@strWhere", SqlDbType.VarChar,1000),
                    };
            parameters[0].Value = "ProductFacepanelSizeDef";
            parameters[1].Value = "facePanelSize";
            parameters[2].Value = PageSize;
            parameters[3].Value = PageIndex;
            parameters[4].Value = 0;
            parameters[5].Value = 0;
            parameters[6].Value = strWhere;	
            return DbHelperSQL.RunProcedure("UP_GetRecordByPage",parameters,"ds");
        }*/

        #endregion  BasicMethod
        #region  ExtensionMethod

        #endregion  ExtensionMethod
    }
}

