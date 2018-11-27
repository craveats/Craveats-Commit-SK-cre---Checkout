using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace WebApplication.DAL.DBCommon
{
    public class DBCommonUtility
    {
        public SqlParameter GetSqlParameter(object value, string pName = null)
        {
            return new SqlParameter()
            {
                ParameterName = pName ?? "@p0",
                Value = value != null ? value : DBNull.Value
            }; 
        }

        public SqlParameter[] GetSqlParameters(IDictionary dictionary)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            if (dictionary != null)
            {
                IDictionaryEnumerator dictionaryEnumerator = dictionary.GetEnumerator();
                while (dictionaryEnumerator.MoveNext())
                {
                    sqlParameters.Add(GetSqlParameter(dictionaryEnumerator.Value, dictionaryEnumerator.Key.ToString()));
                }
            }
            return sqlParameters.ToArray();
        }

        public SqlParameter[] GetSqlParameters(ICollection collection)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            if (collection != null && collection.Count > 0)
            {
                int i = 0;
                foreach (object o in collection)
                {
                    sqlParameters.Add(GetSqlParameter(value: o, pName: string.Format("@p{0}", i)));
                }
            }
            return sqlParameters.ToArray();
        }

        public SqlParameter[] GetSqlParameters(ICollection collection, out string sqlCmdParams, bool appendEnding = false)
        {
            sqlCmdParams = "";
            StringBuilder sbCmdTextBuilder = new StringBuilder();  
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            if (collection != null && collection.Count > 0)
            {
                int i = 0;
                foreach (object o in collection)
                {
                    sqlParameters.Add(GetSqlParameter(value: o, pName: string.Format("@p{0}", i)));
                    sbCmdTextBuilder.AppendFormat(" {0},", string.Format("@p{0}", i));
                    i++;
                }
                sqlCmdParams = sbCmdTextBuilder.Length > 0 
                    ? (sbCmdTextBuilder.Remove(sbCmdTextBuilder.Length - 1, 1).ToString() + (appendEnding ? ";" : ""))
                    : sbCmdTextBuilder.ToString();
            }
            return sqlParameters.ToArray();
        }
    }
}