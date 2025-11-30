using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Dapper;
using Data.Exceptions;

namespace Data.Services
{
    public class AuthService : IAuthService
    {
        private PostgresSQLConnection _connection;
        public AuthService(PostgresSQLConnection connection) => _connection = connection;

        protected NpgsqlConnection DbConnection() => new NpgsqlConnection(_connection._ConnectionString);
        public async Task LogIn(LoginDTO loginDTO)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<T>> UsuarioQueryAsync<T>(string SqlQuery, object? parametros = null)
        {
            IEnumerable<T> items = [];

            using NpgsqlConnection database = DbConnection();

            try
            {
                await database.OpenAsync();

                var result = await database.QueryAsync<T>(
                    SqlQuery,
                    param: parametros
                    );
                items = result.Distinct();
                await database.CloseAsync();
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == DB_ERRORS.UNAUTHORIZED)
                    throw new HttpResponseException(StatusCodes.Status401Unauthorized, ex.MessageText);

                if (ex.SqlState == DB_ERRORS.CONFLICT)
                    throw new HttpResponseException(StatusCodes.Status409Conflict, ex.MessageText);

                if (ex.SqlState == DB_ERRORS.BAD_REQUEST)
                    throw new HttpResponseException(StatusCodes.Status400BadRequest, ex.MessageText);

                if (ex.SqlState == DB_ERRORS.CUSTOMEXCEPTION)
                    throw new HttpResponseException(StatusCodes.Status101SwitchingProtocols, ex.MessageText);

                throw new HttpResponseException(StatusCodes.Status500InternalServerError, ex.MessageText);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(StatusCodes.Status500InternalServerError, ex.Message);
            }
            return items;
        }
    }
}
