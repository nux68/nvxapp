using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nvxapp.server.service.Helpers
{
    public static class StringHelper
    {
        public static string? RemoveSpecialCharacters(string? str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return Regex.Replace(str, "[^a-zA-Z0-9]", "");
        }
    }
}
