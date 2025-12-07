using Data.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Dapper;
using Data.Exceptions;
using Data.DataModel;
using BC = BCrypt.Net.BCrypt;

namespace Data.Services
{
    public class AuthService : IAuthService
    {
        private PostgresSQLConnection _connection;
        private readonly IUsersService _user;

        public AuthService(PostgresSQLConnection connection, IUsersService user)
        {
            _connection = connection;
            _user = user;
        }

        protected NpgsqlConnection DbConnection()
              => new NpgsqlConnection(_connection._ConnectionString);

        #region LOGIN

        public async Task<UserModel?> Login(LoginDTO loginDto)
        {
            using var database = DbConnection();

            try
            {
                var user = await FindUserByName(loginDto.Username);

                if (user == null)
                    throw new HttpResponseException(
                        StatusCodes.Status401Unauthorized, 
                        "Usuario no encontrado"
                    );

                if (!BC.Verify(loginDto.Password, user.passwd))  //a
                    throw new HttpResponseException(
                        StatusCodes.Status401Unauthorized, 
                        "Correo o contraseña incorrecta."
                    );

                return user;
            }
            catch (HttpResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Para otros errores, lanzar una excepción genérica
                throw new HttpResponseException(
                    StatusCodes.Status500InternalServerError, 
                    $"Error en autenticación: {"Usuario no encontrado."}"
                );
            }
            finally
            {
                if (database.State != System.Data.ConnectionState.Closed)
                    await database.CloseAsync();
            }
        }

        #region FindUserByName

        public async Task<UserModel> FindUserByName(string name)
        {
            string query = "SELECT * FROM public.fun_find_user(@p_username);";

            var param = new
            {
                p_username = name
            };

            var result = (await AuthQueryAsync<UserModel>(query, param)).FirstOrDefault();

            return result!;

        }
        #endregion
        #endregion

        public async Task<IEnumerable<T>> AuthQueryAsync<T>(string SqlQuery, object? parametros = null)
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
            }
            catch (PostgresException ex)
            {
                // Manejo de errores específicos de PostgreSQL
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
            finally
            {
                if (database.State != System.Data.ConnectionState.Closed)
                    await database.CloseAsync();
            }
            
            return items;
        }


    //     #region INTENTOS

    //      public async Task<int> ObtenerFallosRecientes(string usuario)
    // {
    //     string sql = @"
    //         SELECT COUNT(*)
    //         FROM intentos_login
    //         WHERE usuario = @usuario
    //           AND ok = false
    //           AND fecha > now() - INTERVAL '5 minutes';
    //     ";

    //     await _connection.OpenAsync();
    //     var cmd = new NpgsqlCommand(sql, _connection);
    //     cmd.Parameters.AddWithValue("@usuario", usuario);
    //     int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
    //     await _connection.CloseAsync();

    //     return result;
    // }

    // public async Task RegistrarIntento(string ip, string usuario, bool ok, string agente)
    // {
    //     string sql = @"
    //         INSERT INTO intentos_login(ip, usuario, ok, agente)
    //         VALUES(@ip, @usuario, @ok, @agente);
    //     ";

    //     await _connection.OpenAsync();
    //     var cmd = new NpgsqlCommand(sql, _connection);
    //     cmd.Parameters.AddWithValue("@ip", ip);
    //     cmd.Parameters.AddWithValue("@usuario", usuario ?? (object)DBNull.Value);
    //     cmd.Parameters.AddWithValue("@ok", ok);
    //     cmd.Parameters.AddWithValue("@agente", agente);
    //     await cmd.ExecuteNonQueryAsync();
    //     await _connection.CloseAsync();
    // }

    // public async Task ResetearIntentos(string usuario)
    // {
    //     string sql = @"DELETE FROM intentos_login WHERE usuario = @usuario;";

    //     await _connection.OpenAsync();
    //     var cmd = new NpgsqlCommand(sql, _connection);
    //     cmd.Parameters.AddWithValue("@usuario", usuario);
    //     await cmd.ExecuteNonQueryAsync();
    //     await _connection.CloseAsync();
    // }
        // #endregion
    }
}