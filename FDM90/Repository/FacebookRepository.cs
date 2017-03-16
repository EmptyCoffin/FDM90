using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using FDM90.Model;

namespace FDM90.Repository
{
    public class FacebookRepository : RepositoryBase<FacebookCredentials>, IRepository<FacebookCredentials>, IReadSpecific<FacebookCredentials>
    {
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
                        "[UserId], [FacebookPageName]" 
                        + SQLHelper.CloseBracket + SQLHelper.Values + SQLHelper.OpenBracket 
                        + "@UserID, @FacebookPageName" 
                        + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectToCreate.UserId),
                            new SqlParameter("@FacebookPageName", objectToCreate.PageName),
                        };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(FacebookCredentials objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FacebookCredentials> ReadAll()
        {
            throw new NotImplementedException();
        }

        public FacebookCredentials ReadSpecific(string userId)
        {
            string sql = SQLHelper.SelectAll
                         + _table + SQLHelper.Where + "[UserId] = @UserId";

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", userId),
                        };

            return SendReaderCommand(sql, parameters).FirstOrDefault();
        }

        public override FacebookCredentials SetProperties(SqlDataReader reader)
        {
            FacebookCredentials creds = new FacebookCredentials();
            creds.UserId = Guid.Parse(reader["UserId"].ToString());
            creds.PageName = reader["FacebookPageName"].ToString();
            creds.PermanentAccessToken = reader["PermanentAccessToken"].ToString();
            return creds;
        }

        public void Update(FacebookCredentials objectToUpdate)
        {
            FacebookCredentials currentDetails = ReadSpecific(objectToUpdate.UserId.ToString());

            string valuesToSet = string.Empty;
            List<SqlParameter> parameters = new List<SqlParameter>();

            foreach (var property in currentDetails.GetType().GetProperties())
            {
                if(property.GetValue(objectToUpdate) != null && property.GetValue(objectToUpdate).ToString() != property.GetValue(currentDetails).ToString())
                {
                    valuesToSet += string.Format("[{0}] = @{0},", property.Name);
                    parameters.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(objectToUpdate)));
                }
            }

            valuesToSet = valuesToSet.Substring(0, valuesToSet.Length - 1);

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                valuesToSet
                + SQLHelper.Where + "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            parameters.Add(new SqlParameter("@UserID", objectToUpdate.UserId));

            SendVoidCommand(sql, parameters.ToArray());
        }
    }
}