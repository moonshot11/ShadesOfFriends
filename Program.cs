
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Brotli;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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

        public readonly string Fullname =>
            string.Join(' ', new string[] { First, Last }.Where(x => x != null));
    }
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            string user = Environment.UserName;
            string citiesPath = @$"C:\Users\{user}\AppData\LocalLow\ColePowered Games\Shadows of Doubt\Cities\";

            Console.WriteLine("Prompting user");

            OpenFileDialog cityOFD = new();
            cityOFD.Title = "Select city file.";
            cityOFD.InitialDirectory = citiesPath;
            cityOFD.Filter = "City files|*.cit;*.citb";
            if (cityOFD.ShowDialog() != DialogResult.OK)
                return;

            OpenFileDialog namesOFD = new();
            namesOFD.Title = "Select custom names text file.";
            namesOFD.InitialDirectory = Environment.CurrentDirectory;
            namesOFD.Filter = "Text files|*.txt";
            if (namesOFD.ShowDialog() != DialogResult.OK)
                return;

            string cityFilename = cityOFD.FileName;
            string namesFilename = namesOFD.FileName;

            List<Person> arrivals = FetchNameData(namesFilename);

            bool enableCompression = cityFilename.EndsWith('b');
            Dictionary<int, Person> randmap = new();
            List< Tuple<string, string> > postWriteNames = new();

            Console.WriteLine("Reading city file");
            string raw = enableCompression ?
                Encoding.UTF8.GetString(File.ReadAllBytes(cityFilename).DecompressFromBrotli()) :
                File.ReadAllText(cityFilename);

            Console.WriteLine("Parsing city data");
            JObject obj = JObject.Parse(raw);

            JToken[] citizens = obj["citizens"].ToArray();
            Random rand = new();

            Console.WriteLine("Choosing citizens to replace");
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

            Console.WriteLine("Replacing citizens");
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
                else if (arrival.First != null)
                    orig["casualName"] = arrival.First;

                orig["citizenName"] = orig["firstName"].ToString() + ' ' + orig["surName"];
                postWriteNames.Add(new(origname, orig["citizenName"].ToString()));
                Console.WriteLine("  " + origname + " -> " + arrival.Fullname);
            }

            string cityOutput = JsonConvert.SerializeObject(obj, Formatting.None);

            Console.WriteLine("Cleaning stale citizen data");
            foreach ((string oldname, string newname) in postWriteNames)
                cityOutput = cityOutput.Replace(oldname, newname);

            // Write updated city data to new file
            Console.WriteLine("Backing up original file");
            string backupFilename = Util.FindValidBackupName(cityFilename);
            File.Move(cityFilename, backupFilename);

            Console.WriteLine("Writing output file");
            if (enableCompression)
            {
                byte[] cityBytes = Encoding.UTF8.GetBytes(cityOutput);
                File.WriteAllBytes(cityFilename, cityBytes.CompressToBrotli());
            }
            else
            {
                File.WriteAllText(cityFilename, cityOutput);
            }
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