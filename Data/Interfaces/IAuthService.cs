using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;

namespace Data.Interfaces
{
    public interface IAuthService
    {
        public Task LogIn(LoginDTO loginDTO);
    }
}
