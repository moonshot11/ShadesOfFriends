
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace ShadesOfFriends
{
    /// <summary>
    /// Possible genders in SoD citizen data
    /// </summary>
    public enum Gender
    {
        // Corresponds to in-game integer designations
        Male = 0,
        Female = 1,
        Nonbinary = 2,
        Any = 3
    }

    /// <summary>
    /// A custom entry provided by the user
    /// </summary>
    public struct Person
    {
        public string First;
        public string Last;
        public string Nick;
        public Gender Gender;

        public string Fullname => string.Join(' ', new string[] { First, Last }.Where(x => x != null));
    }
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            string user = System.Environment.UserName;

            OpenFileDialog cityOFD = new();
            cityOFD.Title = "Select city file.";
            cityOFD.InitialDirectory = @$"C:\Users\{user}\AppData\LocalLow\ColePowered Games\Shadows of Doubt\Cities\";
            cityOFD.Filter = "City files|*.cit;*.citb";
            cityOFD.ShowDialog();

            OpenFileDialog namesOFD = new();
            namesOFD.Title = "Select custom names text file.";
            namesOFD.InitialDirectory = @$"C:\Users\{user}\AppData\LocalLow\ColePowered Games\Shadows of Doubt\Cities\";
            namesOFD.Filter = "Text files|*.txt";
            namesOFD.ShowDialog();

            string cityfile = cityOFD.FileName;
            string namefile = namesOFD.FileName;

            List<Person> arrivals = FetchNameData(namefile);

            bool enableCompression = cityfile.EndsWith('b');
            Dictionary<int, Person> randmap = new();
            List< Tuple<string, string> > postWriteNames = new();

            Console.Write("Reading save file...");
            string raw = enableCompression ?
                Util.BrotliFileToString(cityfile) :
                File.ReadAllText(cityfile);
            Console.WriteLine("done!");

            Console.Write("Parsing data...");
            JObject obj = JObject.Parse(raw);
            Console.WriteLine("done!");

            JToken[] citizens = obj["citizens"].ToArray();
            Random rand = new();

            // Pick random citizens to replace
            while (randmap.Count < arrivals.Count)
            {
                Person arrival = arrivals[randmap.Count];
                int sel = rand.Next() % citizens.Length;
                if (randmap.ContainsKey(sel))
                    continue;
                if (arrival.Gender == Gender.Any || 
                    arrival.Gender == (Gender)(int)(citizens[sel]["gender"]))
                    randmap.Add(sel, arrival);
            }

            // Report and implement primary mapping
            foreach (var kv in randmap)
            {
                Person arrival = kv.Value;
                JToken orig = citizens[kv.Key];
                string origname = orig["citizenName"].ToString();

                orig["surName"] = arrival.Last;
                if (arrival.First != null)
                    orig["firstName"] = arrival.First;
                if (arrival.Nick != null)
                    orig["casualName"] = arrival.Nick;

                orig["citizenName"] = orig["firstName"].ToString() + ' ' + orig["surName"];
                postWriteNames.Add(new(origname, orig["citizenName"].ToString()));
                Console.WriteLine(origname + " -> " + arrival.Fullname);
            }

            // Write updated city data to new file
            if (enableCompression)
                Util.StringToBrotliFire("output.json", obj.ToString());
            else
                File.WriteAllText("output.json", obj.ToString());
        }

        public static List<Person> FetchNameData(string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            Gender mode = Gender.Any;
            List<Person> result = new();

            foreach (string ll in lines)
            {
                string line = ll.Trim();

                if (line == "")
                    continue;

                switch (line)
                {
                    case "#any":
                        mode = Gender.Any;
                        continue;
                    case "#male":
                        mode = Gender.Male;
                        continue;
                    case "#female":
                        mode = Gender.Female;
                        continue;
                    case "#nonbinary":
                        mode = Gender.Nonbinary;
                        continue;
                }

                line = Util.RemoveNonAlpha(line);
                string[] words = line.Split();
                Person per = new();

                if (words.Length == 1)
                {
                    per.Last = char.ToUpper(words[0][0]) + words[0].Substring(1).ToLower();
                }
                else if (words.Length >= 2)
                {
                    per.First = char.ToUpper(words[0][0]) + words[0].Substring(1).ToLower();
                    per.Last = char.ToUpper(words[1][0]) + words[1].Substring(1).ToLower();
                }

                if (words.Length >= 3)
                {
                    per.Nick = char.ToUpper(words[2][0]) + words[2].Substring(1).ToLower();
                }

                per.Gender = mode;
                result.Add(per);
            }

            return result;
        }
    }
}