using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using FDM90.Models.Helpers;
using System.Data.SqlClient;

namespace FDM90.Repository
{
    public class ConfigRepository : RepositoryBase<ConfigItem>, IReadAll<ConfigItem>
    {
        public ConfigRepository()
        {

        }

        public ConfigRepository(IDbConnection connection) : base(connection)
        {

        }

        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Configuration]";
            }
        }

        public IEnumerable<ConfigItem> ReadAll()
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.EndingSemiColon;

            return SendReaderCommand(sql, new SqlParameter[0]);
        }

        public override ConfigItem SetProperties(IDataReader reader)
        {
            ConfigItem item = new ConfigItem();
            item.Name = reader["Name"].ToString();
            item.Value = EncryptionHelper.DecryptString(reader["Value"].ToString());
            return item;
        }
    }
}