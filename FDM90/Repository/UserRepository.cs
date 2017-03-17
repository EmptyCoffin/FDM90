using FDM90.Models;
using FDM90.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

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
                        "[UserId], [UserName], [EmailAddress], [Password]" + SQLHelper.CloseBracket + SQLHelper.Values
                        + SQLHelper.OpenBracket + "@UserID, @UserName, @Email, @Password" + SQLHelper.CloseBracket + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", newUser.UserId),
                            new SqlParameter("@Email", newUser.EmailAddress),
                            new SqlParameter("@UserName", newUser.UserName),
                            new SqlParameter("@Password", newUser.Password),
                            new SqlParameter("@FacebookLinked", newUser.FacebookLinked)
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
            sql += !Guid.TryParse(userName, out test) ? " [UserName] = @UserName " + SQLHelper.EndingSemiColon : " [UserId] = @UserName " + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserName", userName),
                        };

            return SendReaderCommand(sql, parameters).First();
        }

        /// <summary>
        /// Reads all of current object
        /// with database table
        /// </summary>
        /// <returns>current objects in database</returns>
        public IEnumerable<User> ReadAll()
        {
            List<User> _currentUsersList = new List<User>();
            string sql = SQLHelper.SelectAll
                + SQLHelper.From + _table + SQLHelper.EndingSemiColon;

            return SendReaderCommand(sql, new SqlParameter[0]);
        }

        /// <summary>
        /// Updates single object within database table
        /// basing properties on passed in object
        /// </summary>
        /// <param name="objectToUpdate"></param>
        public void Update(User updatedUser)
        {
            string sql = SQLHelper.Update + _table + SQLHelper.Set +
                "[UserName] = @UserName, [EmailAddress] = @EmailAddress, [Password] = @Password, [FacebookLinked] = @FacebookLinked"
                + SQLHelper.Where + "[UserId] = @UserID" + SQLHelper.EndingSemiColon;

            SqlParameter[] parameters = new SqlParameter[]{
                            new SqlParameter("@UserID", updatedUser.UserId),
                            new SqlParameter("@EmailAddress", updatedUser.EmailAddress),
                            new SqlParameter("@UserName", updatedUser.UserName),
                            new SqlParameter("@Password", updatedUser.Password),
                            new SqlParameter("@FacebookLinked", updatedUser.FacebookLinked)
                        };

            SendVoidCommand(sql, parameters);
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
            user.FacebookLinked = bool.Parse(reader["FacebookLinked"].ToString());
            return user;
        }

        #endregion
    }
}