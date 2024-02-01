
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

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
                if (char.IsLetter(c) || " -".Contains(c))
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

        /// <summary>
        /// Compare first and last names ONLY (no nickname comparison) to determine
        /// whether this arrival has already been injected into the citizen manifest
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="arrival"></param>
        /// <returns></returns>
        public static bool CompareCitizenAndArrival(JToken citizen, Person arrival)
        {
            // Last names not matching is an immediately fail
            if (arrival.Last != (string)citizen["surName"])
                return false;

            // If this arrival has no user-provided first name, then a lastname match
            // is sufficient to say that this arrival is already in the city
            if (arrival.First == null)
                return true;

            return arrival.First == (string)citizen["firstName"];
        }
    }
}
