using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadesOfFriends
{
    public static class Util
    {
        /// <summary>
        /// Takes the name of a Brotli bin file; returns a decompressed ASCII string
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string BrotliFileToString(string filename)
        {
            byte[] buf;

            using (FileStream fs = new(filename, FileMode.Open, FileAccess.Read))
            using (BrotliStream bs = new(fs, CompressionMode.Decompress))
            using (MemoryStream ms = new())
            {
                bs.CopyTo(ms);
                buf = ms.ToArray();
            }
            return Encoding.ASCII.GetString(buf);
        }

        /// <summary>
        /// Write the contents of a string to a file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        public static void StringToBrotliFire(string filename, string data)
        {
            using (MemoryStream ms = new())
            using (BrotliStream bs = new(ms, CompressionMode.Compress))
            using (FileStream fs = new(filename, FileMode.CreateNew, FileAccess.Write))
            {
                byte[] buf = Encoding.ASCII.GetBytes(data);
                ReadOnlySpan<byte> span = new(buf);
                ms.Write(span);
                bs.CopyTo(fs);
            }
        }

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
