
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace ShadesOfFriends
{
    /// <summary>
    /// Possible genders in SoD citizen data
    /// </summary>
    public enum Gender
    {
        Male,
        Female,
        Any,
        Nonbinary
    }

    /// <summary>
    /// A custom entry provided by the user
    /// </summary>
    public struct Person
    {
        string name;
        
    }
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string user = System.Environment.UserName;

            OpenFileDialog cityOFD = new OpenFileDialog();
            cityOFD.Title = "Select city file.";
            cityOFD.InitialDirectory = @$"C:\Users\{user}\AppData\LocalLow\ColePowered Games\Shadows of Doubt\Cities\";
            cityOFD.Filter = "City files|*.cit;*.citb";
            cityOFD.ShowDialog();

            OpenFileDialog namesOFD = new OpenFileDialog();
            cityOFD.Title = "Select custom names text file.";
            cityOFD.InitialDirectory = @$"C:\Users\{user}\AppData\LocalLow\ColePowered Games\Shadows of Doubt\Cities\";
            cityOFD.Filter = "Text files|*.txt";
            cityOFD.ShowDialog();

            string cityfile = cityOFD.FileName;
            string namefile = namesOFD.FileName;

            string[] names =
            {
                "Alan Alphabet",
                "Zoey Zebra"
            };
            Dictionary<int, string> randmap = new Dictionary<int, string>();

            Console.Write("Reading save file...");
            string raw = cityfile.EndsWith('b') ?
                Util.BrotliFileToString(cityfile) :
                File.ReadAllText(cityfile);
            Console.WriteLine("done!");

            Console.Write("Parsing data...");
            JObject obj = JObject.Parse(raw);
            Console.WriteLine("done!");

            JToken[] people = obj["citizens"].ToArray();
            Random rand = new Random();

            // Pick random citizens to replace
            while (randmap.Count < names.Length)
            {
                int sel = rand.Next() % people.Length;
                if (!randmap.ContainsKey(sel))
                    randmap.Add(sel, names[randmap.Count]);
            }
        }
    }
}