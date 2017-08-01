using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using FDM90.Model;
using System.Data.SqlClient;

namespace FDM90.Repository
{
    public class TwitterRepository : RepositoryBase<TwitterCredentials>, IRepository<TwitterCredentials>, IReadSpecific<TwitterCredentials>
    {
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Twitter]";
            }
        }

        public void Create(TwitterCredentials objectToCreate)
        {
            string sql = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[UserId], [AccessToken], [AccessTokenSecret], [ScreenName]"
                        + SQLHelper.CloseBracket + SQLHelper.Values + SQLHelper.OpenBracket
                        + "@UserID, @AccessToken, @AccessTokenSecret, @ScreenName"
                        + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectToCreate.UserId),
                            new SqlParameter("@AccessToken", objectToCreate.AccessToken),
                            new SqlParameter("@AccessTokenSecret", objectToCreate.AccessTokenSecret),
                            new SqlParameter("@ScreenName", objectToCreate.ScreenName)
                        };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(TwitterCredentials objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TwitterCredentials> ReadAll()
        {
            throw new NotImplementedException();
        }

        public TwitterCredentials ReadSpecific(string identifyingItem)
        {
            string sql = SQLHelper.SelectAll
                         + _table + SQLHelper.Where + "[UserId] = @UserId" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", identifyingItem)
                        };

            return SendReaderCommand(sql, parameters).FirstOrDefault();
        }

        public override TwitterCredentials SetProperties(IDataReader reader)
        {
            TwitterCredentials creds = new TwitterCredentials();
            creds.UserId = Guid.Parse(reader["UserId"].ToString());
            creds.AccessToken = reader["AccessToken"].ToString();
            creds.AccessTokenSecret = reader["AccessTokenSecret"]?.ToString();
            creds.ScreenName = reader["ScreenName"]?.ToString();
            creds.TwitterData = reader["TwitterData"]?.ToString();
            return creds;
        }

        public void Update(TwitterCredentials objectToUpdate)
        {
            TwitterCredentials currentDetails = ReadSpecific(objectToUpdate.UserId.ToString());
            List<SqlParameter> parameters = new List<SqlParameter>();

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                 SetUpdateValues(currentDetails, objectToUpdate, out parameters)
            + SQLHelper.Where + "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            parameters.Add(new SqlParameter("@UserID", objectToUpdate.UserId));

            SendVoidCommand(sql, parameters.ToArray());
        }
    }
}