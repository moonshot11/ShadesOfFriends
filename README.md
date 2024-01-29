# Shades of Friends

A city file patcher for [Shadows of Doubt](https://store.steampowered.com/app/986130/Shadows_of_Doubt/) which lets you insert custom names into the cities you generate.

# Installation

1. Download and unzip the latest release of this patcher.

2. Copy `libbrotli.dll` from `<Shadows of Doubt install path>\Shadows of Doubt_Data\Plugins\x86_64\` into the folder where you unzipped `ShadesOfFriends.exe`. The DLL should be in the same folder as the executable.

3. Copy Assembly-CSharp.dll

    a. Open Steam, find Shadows of Doubt in your library, right-click, and select "Properties."

    b. Select the "Betas" tab on the left.

    c. In the "Beta Participation" section, select the drop-down box and choose the "mono" option.

    d. Close the properties window, and if necessary, initiate the Shadows of Doubt update.

    e. Copy `Assembly-CSharp.dll` from `<Shadows of Doubt install path>\Shadows of Doubt_Data\Managed\` into the folder where you unzipped `ShadesOfFriends.exe`. The DLL should be in the same folder as the executable and the other DLL that you copied.

    f. Repeat steps a-d, **except** in step (c), choose the "None" option from the drop-down box to revert the installation to the main branch of the game.

# Usage

When you run `ShadesOfFriends.exe`, it will prompt you for a city file (files with a `.cit` or `.citb` extension), and a text file containing the names you want to inject into the game. The patcher will then:

1. Parse the selected city file (this may take several minutes!) and your text file of custom names.

2. Choose random citizens to rename.

3. Back up the original file with a `.bak#` extension, where`#` is the first available number for a file that doesn't exist, starting with 1.

4. Write the patched city file directly to the cities folder. (The patcher will retain the compressed/uncompressed state of the original city file when generating the output.)

If `mycity.citb.bak1` already exists, that file will be left alone, and the patcher will create a backup at `mycity.citb.bak2`. Therefore, if you accidentally patch an already-patched city file (not harmful, but not recommended), the file with the `.bak1` extension will retain the original city data produced by Shadows of Doubt, before anything was modified.

**Note:** The patcher ONLY updates the city file. It is strongly recommended to start a new save after updating a city, since otherwise certain items (employment records, wallets, etc.) captured by save files will refer to names which have been overwritten.

### Creating the names file

Before running `ShadesOfFriends.exe` you must create a text file containing the custom names you want to add. Each name belongs on a separate line. A name can take one of three formats:

```
Firstname Lastname Nickname
Firstname Lastname
Lastname
```

If only two words are found, the nickname is assumed to be the same as the provided first name. If only one word is found, it is assumed to be the last name, and the first name and nickname for that citizen will not be replaced. If there are four or more words on a line, all words after the third are ignored.

Names can contain only letters, and all spaces are considered separators. All invalid characters are removed, and names conform to Title Case (i.e. first letter capitalized, remaining letters lower case) to conform to the behavior observed in SoD's Twitch integration.

### Choosing gender

You can optionally specify the genders of your names. The patcher will select citizens which have already been assigned that gender. By default, the patcher is in `#any` mode, and the citizens adopting your custom names will be chosen completely at random. A gender mode can be activated by providing a pound sign followed by the gender. All names below that line will be assigned to citizens of the corresponding gender. (If there are no more citizens of that gender, the patcher will silently switch back to `#any` mode.)

Note that nonbinary is a third gender category defined in Shadows of Doubt, and is distinct from the `#any` mode, which is means "pick any gender for this name."

To change the gender mode, provide one of the following specifiers on its own line:

```
#any
#female
#male
#nonbinary
```

A full, sample file of names might look as follows:

```
Charlie
Bosley
Alex
Dylan
Nat Cook

#male
Jacques Clouseau Jack
Encyclopedia Brown

#female
Jennifer Aniston Jen
Monica Geller
Pheebs

Cher
Nancy Drew

#male

Sherlock Holmes
Joe Hardy
Frank Hardy

#nonbinary
Pat

#any
Wishbone
Air Bud
```

Note that blank lines are allowed at any point, that a gender mode can be selected at any time, and that modes can be repeated. Since `#any` is the default mode, the first four names in the sample file will each be assigned to a random citizen with no regard for gender.

Happy Sleuthing!
