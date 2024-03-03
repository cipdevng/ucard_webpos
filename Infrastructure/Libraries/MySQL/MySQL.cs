using Core.Model.DTO.Configuration;
using Infrastructure.Abstraction.Database;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using NetCore.AutoRegisterDi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Libraries.MySQL {
    [RegisterAsScoped]
    public class MySQLDB : IDBCommand, IDisposable {
        private string connectionString;
        private string statement;
        private MySqlConnection connection;
        MySqlCommand command;
        private MySqlTransaction transaction;
        public int lastAffectedRows { get; set; }
        public MySQLDB(IOptionsMonitor<SystemVariables> config) {
            DBConfig param = config.CurrentValue.MySQL;
            this.connectionString = string.Concat($"server={param.server};username={param.username};password={param.password};database={param.database};Max Pool Size=300;SslMode={param.sslMode};");
            this._connect();
        }
        private void _connect() {
            MySqlConnection ms;
            try {
                ms = new MySqlConnection(connectionString);
            } catch (Exception) {
                ms = null;
            }
            connection = ms;
        }

        public async Task beginTransaction() {
            await connect();
            _generateCommand();
            transaction = await connection.BeginTransactionAsync();
            command.Transaction = transaction;
        }

        public void prepare(string sql, bool isAt = true) {
            sql = isAt ? sql.Replace(':', '@') : sql;
            string[] strings = sql.Split('?');
            string nstr = "";
            if (strings.Length == 1) {
                nstr = sql;
            } else {
                for (int i = 0; i < strings.Length; i++) {
                    if (!string.IsNullOrWhiteSpace(strings[i])) {
                        string trimmed = strings[i].TrimEnd();
                        if (i + 1 != strings.Length) {
                            nstr += strings[i] + " @bind" + i.ToString();
                        } else {
                            nstr += strings[i];
                        }
                    }
                }
            }
            statement = nstr;
            try {
                if (command == null)
                    _generateCommand();
                command.CommandText = statement;
                command.CommandType = System.Data.CommandType.Text;
                clearParams();
            } catch {
                closeCommand();
                throw;
            }
        }

        public void bindValue(string key, object value) {
            command.Parameters.AddWithValue(key, value);
        }

        public void bindValues(IEnumerable<object> valueObj) {
            clearParams();
            List<object> values = (List<object>)valueObj;
            for (int i = 0; i < values.Count; i++) {
                command.Parameters.AddWithValue("@bind" + i, values[i]);
            }
        }

        public bool clearParams() {
            command.Parameters.Clear();
            return true;
        }
        private async Task<bool> connect() {
            if (connection == null) {
                _connect();
            }
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();
            return true;
        }
        public async Task<bool> execute() {
            try {
                await connect();
                lastAffectedRows = await command.ExecuteNonQueryAsync();
                return true;
            } catch {
                try {
                    await transaction.RollbackAsync();
                } catch { }
                closeCommand();
                throw;
            }
        }
        public async Task<IEnumerable<Dictionary<string, object>>> fetchAllAsObj() {
            try {
                await connect();
                DataTable data = new DataTable();
                var reader = await command.ExecuteReaderAsync();
                data.Load(reader);
                reader.Close();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in data.Rows) {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in data.Columns) {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                await reader.DisposeAsync();
                return rows;
            } catch {
                throw;
            } finally {
                closeCommand();
            }
        }

        public async Task<string> fetchAllAsStr() {
            List<Dictionary<string, object>> res = (List<Dictionary<string, object>>)await fetchAllAsObj();
            res = res ?? new List<Dictionary<string, object>>();
            return JsonConvert.SerializeObject(res, Formatting.Indented);
        }

        public async Task<bool> commit() {
            try {
                await transaction.CommitAsync();
            } catch {
                await transaction.RollbackAsync();
                throw;
            } finally {
                closeCommand();
            }
            return true;
        }

        private void _generateCommand() {
            MySqlCommand command;
            try {
                command = connection.CreateCommand();
                command.Connection = this.connection;
                this.command = command;
            } catch {
                closeCommand();
            }
        }

        public void Dispose() {
            close();
        }
        public void close() {
            try {
                closeCommand();
                connection.Close();
                connection.Dispose();
            } catch { }
        }
        private void closeCommand() {
            this.command?.Dispose();
            this.command = null;
        }
    }
}
