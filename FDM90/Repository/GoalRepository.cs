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
    public class GoalRepository : RepositoryBase<Goals>, IRepository<Goals>, IReadMultipleSpecific<Goals>
    {
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[Goals]";
            }
        }

        public void Create(Goals objectToCreate)
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
                            new SqlParameter("@Progress", objectToCreate.Progress)
                        };

            SendVoidCommand(sql, parameters);
        }

        public void Delete(Goals objectId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Goals> ReadAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Goals> ReadMultipleSpecific(string objectId)
        {
            string sql = SQLHelper.SelectAll + _table + SQLHelper.Where +
                            "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", objectId)
                        };

            return SendReaderCommand(sql, parameters);
        }

        public override Goals SetProperties(IDataReader reader)
        {
            Goals goal = new Goals();
            goal.UserId = Guid.Parse(reader["UserId"].ToString());
            goal.GoalName = reader["GoalName"].ToString();
            goal.StartDate = DateTime.Parse(reader["WeekStart"].ToString());
            goal.EndDate = DateTime.Parse(reader["WeekEnd"].ToString());
            goal.Targets = reader["Targets"].ToString();
            goal.Progress = reader["Progress"].ToString();
            return goal;
        }

        public void Update(Goals objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}