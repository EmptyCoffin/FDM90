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
    public class LinkedInRepository : RepositoryBase<LinkedInCredentials>, IRepository<LinkedInCredentials>, IReadSpecific<LinkedInCredentials>
    {
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[LinkedIn]";
            }
        }

        public void Create(LinkedInCredentials objectToCreate)
        {
            string sql = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[UserId], [AccessToken], [ExpirationDate]"
                        + SQLHelper.CloseBracket + SQLHelper.Values + SQLHelper.OpenBracket
                        + "@UserID, @AccessToken, @ExpirationDate"
                        + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectToCreate.UserId),
                            new SqlParameter("@AccessToken", objectToCreate.AccessToken),
                            new SqlParameter("@ExpirationDate", objectToCreate.ExpirationDate)
                        };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(LinkedInCredentials objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LinkedInCredentials> ReadAll()
        {
            throw new NotImplementedException();
        }

        public LinkedInCredentials ReadSpecific(string identifyingItem)
        {
            string sql = SQLHelper.SelectAll
                         + _table + SQLHelper.Where + "[UserId] = @UserId" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserId", identifyingItem),
                        };

            return SendReaderCommand(sql, parameters).FirstOrDefault();
        }

        public override LinkedInCredentials SetProperties(IDataReader reader)
        {
            LinkedInCredentials creds = new LinkedInCredentials();
            creds.UserId = Guid.Parse(reader["UserId"].ToString());
            creds.AccessToken = reader["AccessToken"].ToString();
            creds.ExpirationDate = DateTime.Parse(reader["ExpirationDate"].ToString());
            return creds;
        }

        public void Update(LinkedInCredentials objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}