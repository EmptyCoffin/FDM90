using FDM90.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FDM90.Repository
{
    public class ProfanityRepository : RepositoryBase<string>, IReadAll<string>
    {
        public ProfanityRepository()
        {

        }

        public ProfanityRepository(IDbConnection connection) : base(connection)
        {

        }

        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Profanity]";
            }
        }

        public IEnumerable<string> ReadAll()
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.EndingSemiColon;

            return SendReaderCommand(sql, new SqlParameter[0]);
        }

        public override string SetProperties(IDataReader reader)
        {
            return reader["Value"].ToString();
        }
    }
}