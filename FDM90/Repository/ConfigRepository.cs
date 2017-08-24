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
    public class ConfigRepository : RepositoryBase<ConfigItem>, IRepository<ConfigItem>
    {
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Configuration]";
            }
        }

        public void Create(ConfigItem objectToCreate)
        {
            throw new NotImplementedException();
        }

        public void Delete(ConfigItem objectId)
        {
            throw new NotImplementedException();
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
            item.Value = reader["Value"].ToString();
            return item;
        }

        public void Update(ConfigItem objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}