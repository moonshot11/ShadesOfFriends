
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        // Patcher-specific mode, doesn't appear in game
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
        // -- Compression parameters --
        const int COMPRESSION_QUALITY = 1;
        const int COMPRESSION_WINDOW = 19;
        const int COMPRESSION_BLOCK_SIZE = 0;
        const int COMPRESSION_MODE = 0;

        [STAThread]
        public static int Main()
        {
            string user = Environment.UserName;
            string citiesPath = @$"C:\Users\{user}\AppData\LocalLow\ColePowered Games\Shadows of Doubt\Cities\";

            // Process arguments

            // Original citizen -> Custom citizen data
            Dictionary<JToken, Person> randmap = new();

            // Preserve a (fullname -> fullname) mapping to clean up dangling reference
            // in the city file (wallets, employment data, etc.)
            List<Tuple<string, string>> postWriteNames = new();

            // -- Get files from user --

            OpenFileDialog cityOFD = new();
            OpenFileDialog namesOFD = new();

            Console.Write("Prompting for city file: ");
            cityOFD.Title = "Select city file.";
            cityOFD.InitialDirectory = citiesPath;
            cityOFD.Filter = "City files|*.cit;*.citb";
            if (cityOFD.ShowDialog() != DialogResult.OK)
                return 0;
            Console.WriteLine(Path.GetFileName(cityOFD.FileName));

            Console.Write("Prompting for custom names file: ");
            namesOFD.Title = "Select custom names text file.";
            namesOFD.InitialDirectory = Environment.CurrentDirectory;
            namesOFD.Filter = "Text files|*.txt";
            if (namesOFD.ShowDialog() != DialogResult.OK)
                return 0;
            Console.WriteLine(Path.GetFileName(namesOFD.FileName));

            string cityFilename = cityOFD.FileName;
            string namesFilename = namesOFD.FileName;
            bool enableCompression = cityFilename.EndsWith('b');

            // -- Read city data and names listfile --

            Console.WriteLine("Reading city file");
            string raw = enableCompression ?
                Encoding.UTF8.GetString(
                    brotli.decompressBuffer(File.ReadAllBytes(cityFilename), useFooter: true)
                ) :
                File.ReadAllText(cityFilename);

            Console.WriteLine("Parsing city data");
            JObject obj = JObject.Parse(raw);

            List<JToken> citizens = obj["citizens"].ToList();
            Random rand = new();

            Console.WriteLine("Reading custom names");
            List<Person> arrivals = FetchNameData(namesFilename);

            // -- Randomly choose which citizens' names to replace --

            Console.WriteLine("Choosing citizens to replace");

            // Stop once custom names are exhausted (most likely),
            // or there are no more citizens to rename
            while (randmap.Count < arrivals.Count && citizens.Count > 0)
            {
                Person arrival = arrivals[randmap.Count];

                if (arrival.Gender == Gender.Any)
                {
                    JToken citizen = citizens[rand.Next(citizens.Count)];
                    randmap.Add(citizen, arrival);
                    citizens.Remove(citizen);
                }
                else
                {
                    // Gendered selection of citizens,
                    // matching desired gender of current arrival
                    JToken[] gs = citizens.Where(x => (Gender)(int)(x["gender"]) == arrival.Gender).ToArray();

                    // If there are no more citizens of desired gender,
                    // silently switch to Any mode and try again
                    if (gs.Length == 0)
                    {
                        arrival.Gender = Gender.Any;
                        continue;
                    }

                    // Since master citizens list seeds every iteration of this loop,
                    // need to remove selected citizen from that list.
                    // gs is refreshed every iteration on its own.
                    JToken citizen = gs[rand.Next(gs.Length)];
                    randmap.Add(citizen, arrival);
                    citizens.Remove(citizen);
                }
            }

            // -- Substitute citizen names --

            Console.WriteLine("Replacing citizens");
            foreach (var kv in randmap)
            {
                JToken orig = kv.Key;
                Person arrival = kv.Value;
                string origFullname = orig["citizenName"].ToString();

                orig["surName"] = arrival.Last;

                if (arrival.First != null)
                    orig["firstName"] = arrival.First;

                if (arrival.Nick != null)
                    orig["casualName"] = arrival.Nick;
                else if (arrival.First != null)
                    orig["casualName"] = arrival.First;

                orig["citizenName"] = orig["firstName"] + " " + orig["surName"];
                postWriteNames.Add( new(origFullname, orig["citizenName"].ToString()) );
                Console.WriteLine("  " + origFullname + " -> " + arrival.Fullname);
            }

            // -- Clean up dangling mentions of original names --

            string cityOutput = JsonConvert.SerializeObject(obj, Formatting.None);

            Console.WriteLine("Cleaning stale citizen data");
            foreach ((string oldname, string newname) in postWriteNames)
                cityOutput = cityOutput.Replace(oldname, newname);

            // -- Write new data --

            // Back up original city file
            string backupFilename = Util.FindValidBackupName(cityFilename);
            Console.WriteLine("Creating backup of original data: " + Path.GetFileName(backupFilename));
            File.Move(cityFilename, backupFilename);
            if (!File.Exists(backupFilename))
            {
                Console.WriteLine("ERROR: Could not back up original file. Aborting.");
                return 1;
            }

            // Create new city file with updated names
            Console.WriteLine("Writing patched file to Cities folder");
            if (enableCompression)
            {
                byte[] cityBytes = Encoding.UTF8.GetBytes(cityOutput);
                File.WriteAllBytes(cityFilename,
                    brotli.compressBuffer(cityBytes, [0], includeSize: true,
                    quality: COMPRESSION_QUALITY,
                    lgwin: COMPRESSION_WINDOW,
                    lgblock: COMPRESSION_BLOCK_SIZE,
                    mode: COMPRESSION_MODE)
                );
            }
            else
            {
                File.WriteAllText(cityFilename, cityOutput);
            }

            if (!File.Exists(cityFilename))
            {
                Console.WriteLine("ERROR: Could not write patched city file. Aborting.");
                return 1;
            }

            Console.WriteLine("Done! Press any key to exit.");
            Console.ReadKey(true);
            return 0;
        }

        /// <summary>
        /// Parse the user-provided listfile of custom names
        /// and return a populated list of Person objects.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
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

                // If gender directive is found, change current mode
                // and skip to next line
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
                Person person = new();

                if (words.Length == 1)
                {
                    person.Last = char.ToUpper(words[0][0]) + words[0][1..].ToLower();
                }
                else if (words.Length >= 2)
                {
                    person.First = char.ToUpper(words[0][0]) + words[0][1..].ToLower();
                    person.Last = char.ToUpper(words[1][0]) + words[1][1..].ToLower();
                }

                if (words.Length >= 3)
                {
                    person.Nick = char.ToUpper(words[2][0]) + words[2][1..].ToLower();
                }

                person.Gender = mode;
                result.Add(person);
            }

            return result;
        }
    }
}