using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using ProyectoEncriptacion.Models;

namespace Data.Interfaces
{
    public interface IAuthService
    {
        public Task<UsuarioModel?> Login(LoginDTO loginDto);
    }
}
