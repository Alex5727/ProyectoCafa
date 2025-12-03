using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
    namespace ProyectoEncriptacion.Data.Interfaces
    {
        public interface IAesService
        {
            string Encrypt(string plaintext);
        }

    }