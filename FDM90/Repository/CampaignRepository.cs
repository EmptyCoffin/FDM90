using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using FDM90.Models.Helpers;

namespace FDM90.Repository
{
    public class CampaignRepository : RepositoryBase<Campaign>, IRepository<Campaign>, IReadMultipleSpecific<Campaign>, IReadAll<Campaign>, IReadSpecific<Campaign>
    {
        public CampaignRepository()
        {

        }

        public CampaignRepository(IDbConnection connection) : base(connection)
        {

        }

        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Campaigns]";
            }
        }

        public void Create(Campaign objectToCreate)
        {
            string sql = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[UserId], [CampaignName], [StartDate], [EndDate], [Targets], [Progress]" + SQLHelper.CloseBracket + SQLHelper.Values
                        + SQLHelper.OpenBracket + "@UserId, @CampaignName, @StartDate, @EndDate, @Targets, @Progress" + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", objectToCreate.UserId),
                            new SqlParameter("@CampaignName", objectToCreate.CampaignName),
                            new SqlParameter("@StartDate", objectToCreate.StartDate),
                            new SqlParameter("@EndDate", objectToCreate.EndDate),
                            new SqlParameter("@Targets", objectToCreate.Targets),
                            new SqlParameter("@Progress", string.IsNullOrEmpty(objectToCreate.Progress) ? (object)DBNull.Value : objectToCreate.Progress)
                        };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(Campaign objectId)
        {
            string sql = SQLHelper.Delete + _table + SQLHelper.Where + "[UserId] = @UserId" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", objectId.UserId)
                        };

            SendVoidCommand(sql, parameters);
        }

        public IEnumerable<Campaign> ReadAll()
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.EndingSemiColon;

            return SendReaderCommand(sql, new SqlParameter[0]);
        }

        public IEnumerable<Campaign> ReadMultipleSpecific(string objectId)
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.Where +
                            "[UserId] = @UserId" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", objectId)
                        };

            return SendReaderCommand(sql, parameters);
        }

        public Campaign ReadSpecific(Campaign identifyingItem)
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.Where +
                            "[UserId] = @UserId" + SQLHelper.And + "[CampaignName] = @CampaignName" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", identifyingItem.UserId),
                            new SqlParameter("@CampaignName", identifyingItem.CampaignName)
                        };

            return SendReaderCommand(sql, parameters).FirstOrDefault();
        }

        public override Campaign SetProperties(IDataReader reader)
        {
            Campaign campaign = new Campaign();
            campaign.UserId = Guid.Parse(reader["UserId"].ToString());
            campaign.CampaignName = reader["CampaignName"].ToString();
            campaign.StartDate = DateTime.Parse(reader["StartDate"].ToString());
            campaign.EndDate = DateTime.Parse(reader["EndDate"].ToString());
            campaign.Targets = reader["Targets"].ToString();
            campaign.Progress = reader["Progress"]?.ToString();
            return campaign;
        }

        public void Update(Campaign objectToUpdate)
        {
            Campaign currentDetails = ReadSpecific(objectToUpdate);
            List<SqlParameter> parameters = new List<SqlParameter>();

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                SetUpdateValues(currentDetails, objectToUpdate, out parameters)
                + SQLHelper.Where + "[UserId] = @UserId" + SQLHelper.And + "[CampaignName] = @CampaignName" + SQLHelper.EndingSemiColon;

            if (parameters.Count > 0)
            {
                parameters.AddRange(new SqlParameter[]{
                            new SqlParameter("@UserId", objectToUpdate.UserId),
                            new SqlParameter("@CampaignName", objectToUpdate.CampaignName)
                        });
                SendVoidCommand(sql, parameters.ToArray());
            }
        }
    }
}