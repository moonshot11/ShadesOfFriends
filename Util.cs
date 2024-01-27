using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadesOfFriends
{
    public static class Util
    {
        public static string RemoveNonAlpha(string value)
        {
            string result = "";
            foreach (char c in value)
                if (char.IsLetter(c) || c == ' ')
                    result += c;
            return result;
        }
    }
}
