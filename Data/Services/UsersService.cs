using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Data.Interfaces;
using Npgsql;
using ProyectoEncriptacion.Models;

namespace Data.Services
{
    public class UsersService : IUsersService
    {
        private PostgresSQLConnection _connectionString;

        public UsersService(PostgresSQLConnection connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connectionString._ConnectionString);

        #region FIND_USER
        public async Task<IEnumerable<UsuarioModel>> FindUserByUsername(string username)
        {
            using var dbConnection = DbConnection();

            var parameters = new
            {
                p_username = username
            };

            var sqlQuery = "SELECT * FROM encriptacion.fun_find_user(@p_username);";

            await dbConnection.OpenAsync();
            var result = await dbConnection.QueryAsync<UsuarioModel>(sql: sqlQuery, parameters);
            return result;

        }
        #endregion
    }
}
