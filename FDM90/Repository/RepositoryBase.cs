using FDM90.Models;
using FDM90.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace FDM90.Repository
{
    public abstract class RepositoryBase<T> where T : class
    {
        private static IDbConnection _connection;
        private static object _lockableObject = new object();
        protected abstract string _table { get; }

        protected RepositoryBase(IDbConnection connection)
        {
            _connection = connection;
        }

        protected RepositoryBase()
            : this(
                new SqlConnection(ConfigurationManager.ConnectionStrings[SQLHelper.DatabaseConnectionString].ToString())
            )
        {
        }

        public abstract T SetProperties(IDataReader reader);

        protected void SendVoidCommand(string sqlText, SqlParameter[] parameters)
        {
            lock (_lockableObject)
            {
                try
                {
                    _connection.Open();
                    IDbCommand command = _connection.CreateCommand();
                    command.CommandText = sqlText;

                    foreach (SqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }

                    command.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                }
                finally
                {
                    _connection.Dispose();
                }
            }
        }

        protected List<T> SendReaderCommand(string sqlText, SqlParameter[] parameters)
        {
            lock (_lockableObject)
            {
                try
                {
                    _connection.Open();
                    IDbCommand command = _connection.CreateCommand();
                    command.CommandText = sqlText;

                    foreach (SqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    var instanceList = Activator.CreateInstance<List<T>>();

                    using (var result = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (result.Read())
                        {
                            instanceList.Add(SetProperties(result));
                        }
                    }

                    return instanceList;
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    _connection.Dispose();
                }
            }
        }
    }
}
