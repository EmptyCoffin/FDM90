using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using FDM90.Models.Helpers;

namespace FDM90.Repository
{
    public class UserRepository : RepositoryBase<User>, IRepository<User>, IReadSpecific<User>
    {
        #region Private Properties
        protected override string _table
        {
            get
            {
                return "[FDM90].[dbo].[User]";
            }
        }

        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public UserRepository()
        {
        }

        public UserRepository(IDbConnection connection):base(connection)
        {
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a object
        /// to be stored in database
        /// </summary>
        /// <param name="objectToCreate"></param>
        public void Create(User newUser)
        {
            string sql = SQLHelper.Insert + _table + SQLHelper.OpenBracket +
                        "[UserId], [UserName], [EmailAddress], [Password], [Facebook], [Twitter], [Campaigns]" + SQLHelper.CloseBracket + SQLHelper.Values
                        + SQLHelper.OpenBracket + "@UserID, @UserName, @Email, @Password, @Facebook, @Twitter, @Campaigns" + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", newUser.UserId),
                            new SqlParameter("@UserName", newUser.UserName),
                            new SqlParameter("@Email", newUser.EmailAddress),
                            new SqlParameter("@Password", newUser.Password),
                            new SqlParameter("@Facebook", false),
                            new SqlParameter("@Twitter", false),
                            new SqlParameter("@Campaigns", (object)0)
                        };

            SendVoidCommand(sql, parameters);
        }

        /// <summary>
        /// Gets specific object
        /// based on user name
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>specific object from database</returns>
        public User ReadSpecific(string userName)
        {
            string sql = SQLHelper.SelectAll
                + _table + SQLHelper.Where;

            Guid test;
            sql += !Guid.TryParse(userName, out test) ? "[UserName] = @SpecificUser" + SQLHelper.EndingSemiColon 
                                                        : "[UserId] = @SpecificUser" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@SpecificUser", userName),
                        };

            return SendReaderCommand(sql, parameters).FirstOrDefault();
        }

        /// <summary>
        /// Reads all of current object
        /// with database table
        /// </summary>
        /// <returns>current objects in database</returns>
        public IEnumerable<User> ReadAll()
        {
            List<User> _currentUsersList = new List<User>();
            string sql = SQLHelper.SelectAll + _table + SQLHelper.EndingSemiColon;

            return SendReaderCommand(sql, new SqlParameter[0]);
        }

        /// <summary>
        /// Updates single object within database table
        /// basing properties on passed in object
        /// </summary>
        /// <param name="objectToUpdate"></param>
        public void Update(User updatedUser)
        {
            User currentDetails = ReadSpecific(updatedUser.UserId.ToString());
            List<SqlParameter> parameters = new List<SqlParameter>();

            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                SetUpdateValues(currentDetails, updatedUser, out parameters)
                + SQLHelper.Where + "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            if (parameters.Count > 0)
            {
                parameters.Add(new SqlParameter("@UserID", updatedUser.UserId));

                SendVoidCommand(sql, parameters.ToArray());
            }
        }

        /// <summary>
        /// Deletes relevant object within
        /// database table based on ID of object
        /// </summary>
        /// <param name="objectId"></param>
        public void Delete(User userToBeDeleted)
        {
            string sql = SQLHelper.Delete + _table + SQLHelper.Where + "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", userToBeDeleted.UserId),
                        };

            SendVoidCommand(sql, parameters);
        }

        #endregion

        #region Private Methods#
        /// <summary>
        /// Set Properties for an object
        /// </summary>
        /// <param name="reader">Database reader value</param>
        /// <param name="stockRequest">Object to populate</param>
        public override User SetProperties(IDataReader reader)
        {
            User user = new User();
            user.UserId = Guid.Parse(reader["UserId"].ToString());
            user.EmailAddress = reader["EmailAddress"].ToString();
            user.UserName = reader["UserName"].ToString();
            user.Password = reader["Password"].ToString();
            user.Facebook = reader["Facebook"] != null ? bool.Parse(reader["Facebook"].ToString()) : false;
            user.Twitter = reader["Twitter"] != null ? bool.Parse(reader["Twitter"].ToString()) : false;
            user.Campaigns = int.Parse(reader["Campaigns"].ToString());
            return user;
        }

        #endregion
    }
}