using Data.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Dapper;
using Data.Exceptions;
using ProyectoEncriptacion.Models;
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

        public async Task<UsuarioModel?> Login(LoginDTO loginDto)
        {
            using var database = DbConnection();

            try
            {
                var result = await _user.FindUserByUsername(loginDto.Username);

                var user = result.FirstOrDefault();

                if (user == null)
                    throw new HttpResponseException(
                        StatusCodes.Status401Unauthorized, 
                        "Usuario no encontrado"
                    );

                if (!BC.EnhancedVerify(loginDto.Password, user.passwd))
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

        #endregion

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
    }
}