using BIA.Entity.Collections;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Threading.Tasks;

namespace BIA.DAL.DBManager
{
    public class OracleDataManagerV2
    {
        private readonly string connectionString;

        public OracleDataManagerV2()
        {
            connectionString = SettingsValues.GetConnectionString();
        }
         
        public OracleDataManagerV2(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Executes a stored procedure with expecting a return value.
        /// </summary>
        public async Task<long> CallInsertProcedure(string storedProcedureName, params OracleParameter[] parameters)
        {
            try
            {
                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = (OracleTransaction)transaction;

                if (parameters?.Length > 0)
                    command.Parameters.AddRange(parameters);

                var outputParameter = new OracleParameter("po_PKValue", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParameter);

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
                OracleDecimal oracleResult = (OracleDecimal)outputParameter.Value;
                long result = Convert.ToInt64(oracleResult.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); 
            }            
        }

        public async Task<int> CallInsertProcedureV2(string storedProcedureName, params OracleParameter[] parameters)
        {
            try
            { 
                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = (OracleTransaction)transaction;

                if (parameters?.Length > 0)
                    command.Parameters.AddRange(parameters);

                var outputParameter = new OracleParameter("po_cursor", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParameter);

                try
                {
                    await command.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();

                }
                catch
                {
                    if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                    {
                        await connection.OpenAsync();
                        await transaction.RollbackAsync();
                    }
                    throw;
                }
                OracleDecimal oracleResult = (OracleDecimal)outputParameter.Value;
                int result = Convert.ToInt32(oracleResult.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<int> CallInsertProcedureV3(string storedProcedureName, params OracleParameter[] parameters)
        {
            try
            {
                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = (OracleTransaction)transaction;

                if (parameters?.Length > 0)
                    command.Parameters.AddRange(parameters);

                var outputParameter = new OracleParameter("po_return", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParameter);

                try
                {
                    await command.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();                    
                }
                catch
                {   if(connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                    {
                        await connection.OpenAsync();
                        await transaction.RollbackAsync();
                    }                    
                    throw;
                }
                OracleDecimal oracleResult = (OracleDecimal)outputParameter.Value;
                int result = Convert.ToInt32(oracleResult.Value);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Executes a stored procedure with expecting a return value.
        /// </summary>
        public async Task<bool> CallUpdateProcedure(string storedProcedureName, params OracleParameter[] parameters)
        {
            bool isUpdate = false;
            try
            { 
                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = (OracleTransaction)transaction;

                if (parameters?.Length > 0)
                    command.Parameters.AddRange(parameters);

                var outputParameter = new OracleParameter("row_affected", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParameter);

                try
                {
                    await command.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                    {
                        await connection.OpenAsync();
                        await transaction.RollbackAsync();
                    }
                    throw;
                }
                OracleDecimal oracleResult = (OracleDecimal)outputParameter.Value;
                int result = Convert.ToInt32(oracleResult.Value);

                isUpdate = result > 0 ? true : false;
                return isUpdate;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Executes a stored procedure and returns an output parameter value.
        /// </summary>
        public async Task<object> CallSelectDataWithObjectReturn(string storedProcedureName, string outputParamName, params OracleParameter[] parameters)
        {
            try
            {
                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = (OracleTransaction)transaction;

                command.Parameters.AddRange(parameters);

                var outputParameter = new OracleParameter(outputParamName, OracleDbType.Decimal)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParameter);

                try
                {
                    await command.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();

                    // Return the value of the output parameter
                    return command.Parameters[outputParamName].Value;
                }
                catch
                {
                    if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                    {
                        await connection.OpenAsync();
                        await transaction.RollbackAsync();
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Executes a stored procedure and returns a DataTable.
        /// </summary>
        public async Task<DataTable> SelectProcedure(string storedProcedureName, params OracleParameter[] parameters)
        {
            try
            {
                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters?.Length > 0)
                    command.Parameters.AddRange(parameters);

                using var adapter = new OracleDataAdapter(command);
                var dataTable = new DataTable();

                adapter.Fill(dataTable);
                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }

        public async Task<DataTable> SelectProcedureV2(string storedProcedureName, params OracleParameter[] parameters)
        {
            try
            { 
                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters?.Length > 0)
                    command.Parameters.AddRange(parameters);

                var outputParameter = new OracleParameter("po_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParameter);

                using var adapter = new OracleDataAdapter(command);
                var dataTable = new DataTable();

                adapter.Fill(dataTable);
                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
