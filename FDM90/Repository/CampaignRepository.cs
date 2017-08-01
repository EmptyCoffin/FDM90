using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using FDM90.Models.Helpers;

namespace FDM90.Repository
{
    public class CampaignRepository : RepositoryBase<Campaign>, IRepository<Campaign>, IReadMultipleSpecific<Campaign>
    {
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
                        "[UserId], [CampaignName], [WeekStart], [WeekEnd], [Targets], [Progress]" + SQLHelper.CloseBracket + SQLHelper.Values
                        + SQLHelper.OpenBracket + "@UserID, @CampaignName, @StartDate, @EndDate, @Targets, @Progress" + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectToCreate.UserId),
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
            throw new NotImplementedException();
        }

        public IEnumerable<Campaign> ReadAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Campaign> ReadMultipleSpecific(string objectId)
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.Where +
                            "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectId)
                        };

            return SendReaderCommand(sql, parameters);
        }

        public override Campaign SetProperties(IDataReader reader)
        {
            Campaign campaign = new Campaign();
            campaign.UserId = Guid.Parse(reader["UserId"].ToString());
            campaign.CampaignName = reader["CampaignName"].ToString();
            campaign.StartDate = DateTime.Parse(reader["WeekStart"].ToString());
            campaign.EndDate = DateTime.Parse(reader["WeekEnd"].ToString());
            campaign.Targets = reader["Targets"].ToString();
            campaign.Progress = reader["Progress"].ToString();
            return campaign;
        }

        public void Update(Campaign objectToUpdate)
        {
            Campaign currentDetails = ReadMultipleSpecific(objectToUpdate.UserId.ToString()).Where(x => x.CampaignName == objectToUpdate.CampaignName).First();
            List<SqlParameter> parameters = new List<SqlParameter>();

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                SetUpdateValues(currentDetails, objectToUpdate, out parameters)
                + SQLHelper.Where + "[UserId] = @UserID and [CampaignName] = @CampaignName" + SQLHelper.EndingSemiColon;

            parameters.AddRange(new SqlParameter[]{
                            new SqlParameter("@UserID", objectToUpdate.UserId),
                            new SqlParameter("@CampaignName", objectToUpdate.CampaignName)
                        });
            SendVoidCommand(sql, parameters.ToArray());
        }
    }
}