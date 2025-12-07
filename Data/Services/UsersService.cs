using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Data.Interfaces;
using Npgsql;
using Data.DataModel;

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

            Console.WriteLine("SQL ejecutado: " + sql);

                var parameters = new { p_username = username };

                    await dbConnection.OpenAsync();

            var result = await dbConnection.QueryAsync<UserModel>(
                sql,
                parameters
            );

            if (dbConnection.State != System.Data.ConnectionState.Closed)
                await dbConnection.CloseAsync();

            return result;
        }

    }
}



