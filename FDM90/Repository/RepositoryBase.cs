using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using FDM90.Models.Helpers;

namespace FDM90.Repository
{
    public abstract class RepositoryBase<T> where T : class
    {
        private static IDbConnection _connection;
        private static object _lockableObject = new object();
        protected abstract string _table { get; }
        public IDbConnection Connection
        {
            get { return _connection == null ? new SqlConnection(WebConfigurationManager.ConnectionStrings[SQLHelper.DatabaseConnectionString].ConnectionString) : _connection; }
        }

    protected RepositoryBase(IDbConnection connection)
        {
            _connection = connection;
        }

        protected RepositoryBase()
        {
        }

        public abstract T SetProperties(IDataReader reader);

        protected void SendVoidCommand(string sqlText, SqlParameter[] parameters)
        {
            lock (_lockableObject)
            {
                try
                {
                    using (IDbConnection connection = Connection)
                    {
                        IDbCommand command = connection.CreateCommand();
                        command.CommandText = sqlText;

                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                        connection.Open();

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        protected List<T> SendReaderCommand(string sqlText, SqlParameter[] parameters)
        {
            lock (_lockableObject)
            {
                try
                {
                    using (IDbConnection connection = Connection)
                    {

                        IDbCommand command = connection.CreateCommand();
                        command.CommandText = sqlText;

                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                        var instanceList = Activator.CreateInstance<List<T>>();
                        connection.Open();

                        using (var result = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (result.Read())
                            {
                                instanceList.Add(SetProperties(result));
                            }
                        }

                        return instanceList;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        protected string SetUpdateValues(T existingObject, T updatedObject, out List<SqlParameter> parameters)
        {
            string valuesToSet = string.Empty;
            List<SqlParameter> param = new List<SqlParameter>();

            foreach (var property in existingObject.GetType().GetProperties())
            {
                if (property.GetValue(updatedObject) != null && property.GetValue(updatedObject).ToString() != property.GetValue(existingObject)?.ToString())
                {
                    valuesToSet += string.Format("[{0}] = @{0},", property.Name);
                    param.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(updatedObject)));
                }
            }

            parameters = param;
            return string.IsNullOrEmpty(valuesToSet) ? valuesToSet : valuesToSet.Substring(0, valuesToSet.Length - 1);
        }
    }
}
