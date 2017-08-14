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
    public class MarketingModelRepository : RepositoryBase<MarketingModel>, IRepository<MarketingModel>
    {
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[MarketingModel]";
            }
        }

        public void Create(MarketingModel objectToCreate)
        {
            throw new NotImplementedException();
        }

        public void Delete(MarketingModel objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MarketingModel> ReadAll()
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.EndingSemiColon;

            return SendReaderCommand(sql, new SqlParameter[0]);
        }

        public override MarketingModel SetProperties(IDataReader reader)
        {
            MarketingModel model = new MarketingModel();
            model.Name = reader["Name"].ToString();
            model.Description = reader["Description"].ToString();
            model.MetricsUsed = reader["MetricsUsed"].ToString();
            model.CalculationExpression = reader["CalculationExpression"].ToString();
            return model;
        }

        public void Update(MarketingModel objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}