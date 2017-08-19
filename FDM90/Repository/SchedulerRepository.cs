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
    public class SchedulerRepository : RepositoryBase<ScheduledPost>, IRepository<ScheduledPost>, IReadMultipleSpecific<ScheduledPost>
    {
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[ScheduledPosts]";
            }
        }

        public void Create(ScheduledPost objectToCreate)
        {
            string sql = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[PostId], [UserId], [PostText], [AttachmentPath], [PostTime], [MediaChannels]" + SQLHelper.CloseBracket + SQLHelper.Values
                        + SQLHelper.OpenBracket + "@PostId, @UserID, @PostText, @AttachmentPath, @PostTime, @MediaChannels" + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@PostId", objectToCreate.PostId),
                            new SqlParameter("@UserID", objectToCreate.UserId),
                            new SqlParameter("@PostText", (object)objectToCreate.PostText ?? DBNull.Value),
                            new SqlParameter("@AttachmentPath", (object)objectToCreate.AttachmentPath ?? DBNull.Value),
                            new SqlParameter("@PostTime", objectToCreate.PostTime),
                            new SqlParameter("@MediaChannels", objectToCreate.MediaChannels)
            };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(ScheduledPost objectId)
        {
            string sql = SQLHelper.Delete + _table + SQLHelper.Where + "[PostId] = @PostId" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@PostId", objectId.PostId)
                        };

            SendVoidCommand(sql, parameters);
        }

        public IEnumerable<ScheduledPost> ReadAll()
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.EndingSemiColon;

            return SendReaderCommand(sql, new SqlParameter[0]);
        }

        public IEnumerable<ScheduledPost> ReadMultipleSpecific(string objectId)
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.Where;
            object searchValue = null;
            string searchParameter = string.Empty;
            Guid testGuid = Guid.Empty;

            if (Guid.TryParse(objectId, out testGuid))
            {
                sql += "[UserId] = @UserId" + SQLHelper.EndingSemiColon;
                searchParameter = "@UserId";
                searchValue = testGuid;
            }
            else
            {
                sql += "[PostTime] = @PostTime" + SQLHelper.EndingSemiColon;
                searchParameter = "@PostTIme";
                searchValue = objectId;
            }

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter(searchParameter, searchValue)
                        };

            return SendReaderCommand(sql, parameters);
        }

        public override ScheduledPost SetProperties(IDataReader reader)
        {
            ScheduledPost post = new ScheduledPost();
            post.PostId = Guid.Parse(reader["PostId"].ToString());
            post.UserId = Guid.Parse(reader["UserId"].ToString());
            post.PostText = reader["PostText"].ToString();
            post.AttachmentPath = reader["AttachmentPath"].ToString();
            post.PostTime = DateTime.Parse(reader["PostTime"].ToString());
            post.MediaChannels = reader["MediaChannels"].ToString();
            return post;
        }

        public void Update(ScheduledPost objectToUpdate)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                 SetUpdateValues(new ScheduledPost(), objectToUpdate, out parameters)
            + SQLHelper.Where + "[PostId] = @PostId" + SQLHelper.EndingSemiColon;

            if (parameters.Count > 0)
            {
                if(!parameters.Any(x => x.ParameterName == "@PostId"))
                    parameters.Add(new SqlParameter("@PostId", objectToUpdate.PostId));

                SendVoidCommand(sql, parameters.ToArray());
            }
        }
    }
}