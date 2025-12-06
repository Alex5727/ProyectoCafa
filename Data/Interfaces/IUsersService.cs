using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProyectoEncriptacion.Models;

namespace Data.Interfaces
{
    public interface IUsersService
    {
        public Task<IEnumerable<UsuarioModel>> FindUserByUsername(string username);
    }
}
