using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class PostgresSQLConnection
    {
        public string? _ConnectionString;
        public PostgresSQLConnection(string connectionstring) => _ConnectionString = connectionstring;
    }
    public static class DB_ERRORS
    {
        public const string UNAUTHORIZED = "P0401";
        public const string BAD_REQUEST = "P0400";
        public const string CONFLICT = "P0409";
        public const string CUSTOMEXCEPTION = "P0001";
    }
}
