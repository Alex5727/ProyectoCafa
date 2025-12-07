using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using Data.DataModel;

namespace Data.Interfaces
{
    public interface IAuthService
    {
        public Task<UserModel?> Login(LoginDTO loginDto);
    }
}
