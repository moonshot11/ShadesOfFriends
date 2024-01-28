using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadesOfFriends
{
    public static class Util
    {
        /// <summary>
        /// Preserve only letters and spaces
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveNonAlpha(string value)
        {
            string result = "";
            foreach (char c in value)
                if (char.IsLetter(c) || c == ' ')
                    result += c;
            return result;
        }

        /// <summary>
        /// Find the first available filename with the extension .bak#,
        /// where # increments sequentially from 1 until a filename is found
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FindValidBackupName(string filename)
        {
            int i = 1;

            while (File.Exists(filename + ".bak" + i))
                i++;

            return filename + ".bak" + i;
        }
    }
}
