using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using FDM90.Models.Helpers;

namespace FDM90.Repository
{
    public class FacebookRepository : RepositoryBase<FacebookCredentials>, IRepository<FacebookCredentials>, IReadSpecific<FacebookCredentials>
    {
        public FacebookRepository()
        {
            
        }

        public FacebookRepository(IDbConnection connection):base(connection)
        {
            
        }

        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Facebook]";
            }
        }

        public void Create(FacebookCredentials objectToCreate)
        {
            string sql = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[UserId], [PageName]" 
                        + SQLHelper.CloseBracket + SQLHelper.Values + SQLHelper.OpenBracket 
                        + "@UserID, @PageName" 
                        + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectToCreate.UserId),
                            new SqlParameter("@PageName", objectToCreate.PageName),
                        };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(FacebookCredentials objectId)
        {
            string sql = SQLHelper.Delete + _table + SQLHelper.Where + "[UserId] = @UserID" + SQLHelper.And + "[PageName] = @PageName" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectId.UserId),
                            new SqlParameter("@PageName", objectId.PageName),
                        };

            SendVoidCommand(sql, parameters);
        }

        public IEnumerable<FacebookCredentials> ReadAll()
        {
            throw new NotImplementedException();
        }

        public FacebookCredentials ReadSpecific(string userId)
        {
            string sql = SQLHelper.SelectAll
                         + _table + SQLHelper.Where + "[UserId] = @UserId" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", userId),
                        };

            return SendReaderCommand(sql, parameters).FirstOrDefault();
        }

        public override FacebookCredentials SetProperties(IDataReader reader)
        {
            FacebookCredentials creds = new FacebookCredentials();
            creds.UserId = Guid.Parse(reader["UserId"].ToString());
            creds.PageName = reader["PageName"].ToString();
            creds.PermanentAccessToken = reader["PermanentAccessToken"]?.ToString();
            creds.FacebookData = reader["FacebookData"]?.ToString();
            return creds;
        }

        public void Update(FacebookCredentials objectToUpdate)
        {
            FacebookCredentials currentDetails = ReadSpecific(objectToUpdate.UserId.ToString());
            List<SqlParameter> parameters = new List<SqlParameter>();

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                 SetUpdateValues(currentDetails, objectToUpdate, out parameters)
            + SQLHelper.Where + "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            if (parameters.Count > 0)
            {
                parameters.Add(new SqlParameter("@UserID", objectToUpdate.UserId));

                SendVoidCommand(sql, parameters.ToArray());
            }
        }
    }
}