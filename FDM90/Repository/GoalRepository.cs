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
    public class GoalRepository : RepositoryBase<Goal>, IRepository<Goal>, IReadMultipleSpecific<Goal>
    {
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Goals]";
            }
        }

        public void Create(Goal objectToCreate)
        {
            string sql = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[UserId], [GoalName], [WeekStart], [WeekEnd], [Targets], [Progress]" + SQLHelper.CloseBracket + SQLHelper.Values
                        + SQLHelper.OpenBracket + "@UserID, @GoalName, @StartDate, @EndDate, @Targets, @Progress" + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectToCreate.UserId),
                            new SqlParameter("@GoalName", objectToCreate.GoalName),
                            new SqlParameter("@StartDate", objectToCreate.StartDate),
                            new SqlParameter("@EndDate", objectToCreate.EndDate),
                            new SqlParameter("@Targets", objectToCreate.Targets),
                            new SqlParameter("@Progress", string.IsNullOrEmpty(objectToCreate.Progress) ? (object)DBNull.Value : objectToCreate.Progress)
                        };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(Goal objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Goal> ReadAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Goal> ReadMultipleSpecific(string objectId)
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.Where +
                            "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectId)
                        };

            return SendReaderCommand(sql, parameters);
        }

        public override Goal SetProperties(IDataReader reader)
        {
            Goal goal = new Goal();
            goal.UserId = Guid.Parse(reader["UserId"].ToString());
            goal.GoalName = reader["GoalName"].ToString();
            goal.StartDate = DateTime.Parse(reader["WeekStart"].ToString());
            goal.EndDate = DateTime.Parse(reader["WeekEnd"].ToString());
            goal.Targets = reader["Targets"].ToString();
            goal.Progress = reader["Progress"].ToString();
            return goal;
        }

        public void Update(Goal objectToUpdate)
        {
            Goal currentDetails = ReadMultipleSpecific(objectToUpdate.UserId.ToString()).Where(x => x.GoalName == objectToUpdate.GoalName).First();
            List<SqlParameter> parameters = new List<SqlParameter>();

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                SetUpdateValues(currentDetails, objectToUpdate, out parameters)
                + SQLHelper.Where + "[UserId] = @UserID and [GoalName] = @GoalName" + SQLHelper.EndingSemiColon;

            parameters.AddRange(new SqlParameter[]{
                            new SqlParameter("@UserID", objectToUpdate.UserId),
                            new SqlParameter("@GoalName", objectToUpdate.GoalName)
                        });
            SendVoidCommand(sql, parameters.ToArray());
        }
    }
}