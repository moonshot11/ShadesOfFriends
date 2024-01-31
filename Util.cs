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
        public static void CompressAndWrite(string data, string filename)
        {
            byte[] buf = Encoding.UTF8.GetBytes(data);
            int len = buf.Length;

            using MemoryStream ms1 = new();
            using BrotliStream bs = new(ms1, CompressionMode.Compress);

            bs.Write(buf);
            buf = [.. ms1.ToArray(), .. BitConverter.GetBytes(len)];

            File.WriteAllBytes(filename, buf);
        }

        public static string Decompress(string filename)
        {
            byte[] buf = File.ReadAllBytes(filename);
            buf = buf[..^4];

            using MemoryStream ms1 = new(buf);
            using BrotliStream bs = new(ms1, CompressionMode.Decompress);
            using MemoryStream ms2 = new();

            bs.CopyTo(ms2);
            buf = ms2.ToArray();

            return Encoding.UTF8.GetString(buf);
        }

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
