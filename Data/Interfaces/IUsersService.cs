using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.DataModel;


namespace Data.Interfaces
{
    public interface IUsersService
    {
        public Task<IEnumerable<UserModel>> FindUserByUsername(string username);
    }
}
