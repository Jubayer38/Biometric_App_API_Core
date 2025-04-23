using BIA.Entity.Collections;
using BIA.Entity.Connectivities;
using BIA.Entity.Utility;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Data.Common;

namespace BIA.DAL.DBManager
{
    /// <summary>
    /// 
    /// </summary>
    public class OracleDataManager
    {
        readonly OracleConnection connection;
        readonly OracleCommand command;
        private List<OracleParameter> parameters = new List<OracleParameter>();
        string connectionString = String.Empty;
        public OracleDataManager()
        {
            LoadConnectionString();
            connection = new OracleConnection(connectionString);
            command = new OracleCommand();
        }

        public OracleDataManager(string ConnectionString)
        {
            LoadConnectionString(ConnectionString);
            connection = new OracleConnection(connectionString);
            command = new OracleCommand();
        }

        private void LoadConnectionString()
        {
            try
            {
                connectionString = SettingsValues.GetConnectionString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void LoadConnectionString(string name)
        {
            try
            {
                connectionString = SettingsValues.GetConnectionWithName(name);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddParameter(OracleParameter param)
        {
            //command.Parameters.Add(param);
            parameters.Add(param);
        }

        public void CallStoredProcedure(string storedProcedureName)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (OracleTransaction transaction = connection.BeginTransaction())
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = storedProcedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Transaction = transaction;

                    // Attach all parameters
                    command.Parameters.AddRange(parameters.ToArray());

                    try
                    {
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        parameters.Clear(); // Clear parameters after execution
                    }
                }
            }
        }

        public async Task CallStoredProcedureV2(string storedProcedureName)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                await connection.OpenAsync();

                using (OracleTransaction transaction = connection.BeginTransaction())
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = storedProcedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Transaction = transaction;

                    // Add parameters if provided
                    if (parameters != null && parameters.Count > 0)
                        command.Parameters.AddRange(parameters.ToArray());

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        //public async Task CallStoredProcedureV2(string storedProcedureName)
        //{
        //    using (connection)
        //    {
        //        //long rowAffected = 0;
        //        try
        //        {
        //            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

        //            command.Connection = connection;
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.CommandText = storedProcedureName;
        //            OracleTransaction transaction = connection.BeginTransaction();
        //            command.Transaction = transaction;
        //            try
        //            {
        //                command.Parameters.AddRange(parameters.ToArray());
        //                await command.ExecuteNonQueryAsync();
        //                await transaction.CommitAsync();
        //            }
        //            catch (Exception ex)
        //            {
        //                await transaction.RollbackAsync();
        //                throw;
        //            }

        //        }
        //        catch (Exception ex)
        //        {

        //            throw;
        //        }
        //        finally
        //        {
        //            if (connection.State != ConnectionState.Closed) connection.Close();
        //        }
        //    }
        //}


        /// <summary>
        /// This function helps to get outoput paremeter from procedure as an object.
        /// </summary>
        /// <param name="param"></param>
        /// <returns>Output Parameter</returns>
        public object GetValueOfOutputParameter(string outputParam)
        {
            return command.Parameters[outputParam].Value;
        }



        /// <summary>
        /// This function takes a produre name as string and 
        /// returns an object which determines weather the procedure exceute successsfully or not.  
        /// </summary>
        /// <returns>int</returns>
        //public object CallStoredProcedure_1(string storedProcedureName, string outputParam)
        //{
        //    using (connection)
        //    {
        //        try
        //        {
        //            if (connection.State != ConnectionState.Open) connection.Open();

        //            command.Connection = connection;
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.CommandText = storedProcedureName;
        //            OracleTransaction transaction = connection.BeginTransaction();
        //            command.Transaction = transaction;
        //            try
        //            {
        //                command.Parameters.AddRange(parameters.ToArray());
        //                command.ExecuteNonQuery();
        //                transaction.Commit();
        //                return command.Parameters[outputParam].Value;
        //            }
        //            catch (Exception)
        //            {
        //                transaction.Rollback();
        //                throw;
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //            throw;
        //        }
        //        finally
        //        {
        //            if (connection.State != ConnectionState.Closed) connection.Close();
        //        }
        //    }
        //}

        public object CallStoredProcedure_1(string storedProcedureName, string outputParam)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (OracleTransaction transaction = connection.BeginTransaction())
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = storedProcedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Transaction = transaction;

                    // Attach all parameters
                    command.Parameters.AddRange(parameters.ToArray());

                    try
                    {
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        return command.Parameters[outputParam].Value;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        parameters.Clear(); // Clear parameters after execution
                    }
                }
            }
        }

        //public async Task<object> CallStoredProcedure(string storedProcedureName, string outputParam)
        //{
        //    using (connection)
        //    {
        //        try
        //        {
        //            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

        //            command.Connection = connection;
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.CommandText = storedProcedureName;
        //            OracleTransaction transaction = connection.BeginTransaction();
        //            command.Transaction = transaction;
        //            try
        //            {
        //                command.Parameters.AddRange(parameters.ToArray());
        //                await command.ExecuteNonQueryAsync();
        //                transaction.Commit();
        //                return command.Parameters[outputParam].Value;
        //            }
        //            catch (Exception)
        //            {
        //                transaction.Rollback();
        //                throw;
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //            throw;
        //        }
        //        finally
        //        {
        //            if (connection.State != ConnectionState.Closed) connection.Close();
        //        }
        //    }
        //}

        public async Task<object> CallStoredProcedure(string storedProcedureName, string outputParam)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                await connection.OpenAsync();

                using (OracleTransaction transaction = connection.BeginTransaction())
                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = storedProcedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Transaction = transaction;

                    // Add parameters if provided
                    if (parameters != null && parameters.Count > 0)
                        command.Parameters.AddRange(parameters.ToArray());

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                        return command.Parameters[outputParam].Value;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public DateTime CallStoredProcedure_DBDate()
        {
            DateTime pkValue = DateTime.Now;

            OracleParameter param = new OracleParameter("po_PKValue", OracleDbType.Date)
            {
                Direction = ParameterDirection.Output
            };
            AddParameter(param);

            try
            {
                CallStoredProcedure("SSP_GetDBDate");

                if (param.Value != DBNull.Value)
                    pkValue = ((Oracle.ManagedDataAccess.Types.OracleDate)param.Value).Value;
            }
            catch (Exception)
            {

                throw;
            }


            return pkValue;
        }
              

        public DataSet CallStoredProcedure_SelectDS(string storedProcedureName)
        {

            using (connection)
            {
                try
                {
                    if (connection.State != ConnectionState.Open) connection.Open();

                    command.Connection = connection;
                    command.CommandText = storedProcedureName;
                    command.CommandType = CommandType.StoredProcedure;

                    DataSet dt = new DataSet(storedProcedureName);

                    command.Parameters.AddRange(parameters.ToArray());

                    using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                    {
                        //adapter.SelectCommand = command;
                        adapter.Fill(dt);
                    }
                    return dt;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed) connection.Close();
                }
            }
        }

        public DataTable CallStoredProcedure_Select(string storedProcedureName)
        {
            using (OracleConnection connection = new OracleConnection(connectionString)) // Fresh connection
            {
                try
                {
                    connection.Open();

                    using (OracleCommand command = new OracleCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        DataTable dt = new DataTable();
                        
                        command.Parameters.AddRange(parameters.ToArray());
                        
                        using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }

                        return dt;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error executing stored procedure {storedProcedureName}: {ex.Message}", ex);
                }
            } // Connection is automatically closed when exiting the 'using' block
        }


        /// <summary>
        /// This method directly call the stored procedure where procedure name is mention in argument.It must return inserted primary key value. If need any parameter passing, just do it before call this method by AddParameter()
        /// Primary key value[Developmer must implement code in SP for return]
        /// </summary>
        /// <param name="storedProcedureName">Enter Stored Procedure Name</param>
        /// <returns>Primary key value[Developmer must implement code in SP for return]</returns>
        public long? CallStoredProcedure_Insert(string storedProcedureName)
        {
            long? pkValue = null;
            OracleParameter param = new OracleParameter("po_PKValue", OracleDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };

            AddParameter(param);
            try
            {
                CallStoredProcedure(storedProcedureName);

                if (param.Value != DBNull.Value)
                    pkValue = (long)((Oracle.ManagedDataAccess.Types.OracleDecimal)param.Value).Value;
                return pkValue;
            }
            catch (Exception ex)
            {
                throw;
            }
            //return pkValue;
        }

        public async Task<long?> CallStoredProcedure_InsertV3(string storedProcedureName)
        {
            long? pkValue = null;
            OracleParameter param = new OracleParameter("po_PKValue", OracleDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };

            AddParameter(param);
            try
            {
                await CallStoredProcedureV2(storedProcedureName);

                if (param.Value != DBNull.Value)
                    pkValue = (long)((Oracle.ManagedDataAccess.Types.OracleDecimal)param.Value).Value;
                return pkValue;
            }
            catch (Exception ex)
            {
                throw;
            }
            //return pkValue;
        }

        public DataTable CallStoredProcedure_InsertV2(string storedProcedureName)
        {
            OracleParameter param = new OracleParameter("po_cursor", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            AddParameter(param);
            using (connection)
            {
                try
                {
                    if (connection.State != ConnectionState.Open) connection.Open();

                    command.Connection = connection;
                    command.CommandText = storedProcedureName;
                    command.CommandType = CommandType.StoredProcedure;

                    DataTable dt = new DataTable(storedProcedureName);

                    command.Parameters.AddRange(parameters.ToArray());

                    using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                    {
                        //adapter.SelectCommand = command;
                        adapter.Fill(dt);
                    }
                    return dt;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                finally
                {
                    if (connection.State != ConnectionState.Closed) connection.Close();
                }
            }

        }
        public long? CallStoredProcedure_Update(string storedProcedureName)
        {
            long? pkValue = null;
            OracleParameter param = new OracleParameter("po_PKValue", OracleDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };

            AddParameter(param);
            //command.Parameters.AddRange(parameters.ToArray());
            try
            {
                CallStoredProcedure(storedProcedureName);

                if (param.Value != DBNull.Value)
                    pkValue = (long)((Oracle.ManagedDataAccess.Types.OracleDecimal)param.Value).Value;
                return pkValue;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool CallStoredProcedure_UpdateV2(string storedProcedureName)
        {
            bool isUpdate = false;
            long? value = null;
            OracleParameter param = new OracleParameter("row_affected", OracleDbType.Int16)
            {
                Direction = ParameterDirection.Output
            };
            AddParameter(param);
            try
            {
                CallStoredProcedure(storedProcedureName);
                if (param.Value != DBNull.Value)
                    value = (long)
                        ((Oracle.ManagedDataAccess.Types.OracleDecimal)param.Value).Value;
                isUpdate = value > 0 ? true : false;

            }
            catch (Exception)
            {

                throw;
            }

            return isUpdate;
        }

        public void CallStoredProcedure_Delete(string storedProcedureName)
        {
            try
            {
                CallStoredProcedure(storedProcedureName);


            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
