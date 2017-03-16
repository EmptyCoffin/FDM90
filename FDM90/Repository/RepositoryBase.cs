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
    public abstract class RepositoryBase<T> where T:class
    {
        private static object _lockableObject = new object();
        protected abstract string _table { get; }

        public abstract T SetProperties(SqlDataReader reader);

        protected void SendVoidCommand(string sqlText, SqlParameter[] parameters)
        {
            lock (_lockableObject)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[SQLHelper.DatabaseConnectionString].ToString()))
                    {
                        conn.Open();

                        SqlCommand command = new SqlCommand(sqlText, conn);
                        command.Parameters.AddRange(parameters);
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
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[SQLHelper.DatabaseConnectionString].ToString()))
                    {
                        conn.Open();

                        var instanceList = Activator.CreateInstance<List<T>>();

                        SqlCommand command = new SqlCommand(sqlText, conn);
                        command.Parameters.AddRange(parameters);
                        command.ExecuteNonQuery();

                        using (var result = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while(result.Read())
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
    }
}
