using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Data.Interfaces;
using Npgsql;
using Data.DataModel;
using Data.Exceptions;
using Microsoft.AspNetCore.Http;

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

        public async Task<IEnumerable<UserModel>> FindUserByUsername(string username)
        {
            using var dbConnection = DbConnection();

            var sql = "SELECT * FROM public.fun_find_user(@p_username);";
            var parameters = new { p_username = username };

            await dbConnection.OpenAsync();
            var result = await dbConnection.QueryAsync<UserModel>(sql, parameters);

            if (dbConnection.State != System.Data.ConnectionState.Closed)
                await dbConnection.CloseAsync();

            return result;
        }

        // Nuevo método para crear usuario usando la función fun_create_user
        public async Task CreateUserAsync(string username, string password)
        {
            using var dbConnection = DbConnection();
            var sql = "SELECT public.fun_create_user(@p_username, @p_password);";
            var parameters = new { p_username = username, p_password = password };

            try
            {
                await dbConnection.OpenAsync();
                // Execute the function (returns void) -> use ExecuteAsync or QueryAsync, both ok
                await dbConnection.ExecuteAsync(sql, parameters);
            }
            catch (PostgresException)
            {
                // rethrow para que el controlador lo maneje (o puedes mapear a HttpResponseException)
                throw;
            }
            finally
            {
                if (dbConnection.State != System.Data.ConnectionState.Closed)
                    await dbConnection.CloseAsync();
            }
        }
    }
}