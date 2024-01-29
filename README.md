# Shades of Friends

A city file patcher for [Shadows of Doubt](https://store.steampowered.com/app/986130/Shadows_of_Doubt/) which lets you insert custom names into the cities you generate.

# Installation

None! Just download the latest ZIP file and unzip its contents to anywhere. Just make sure all the files stay together.

# Usage

When you run ShadesOfFriends.exe, it will prompt you for a city file (either the `.cit` or `.citb` extension), and a text file containing the names you want to inject into the game. The patcher will then:

1. Parse the selected city file (this may take several minutes!).
2. Choose random citizens to rename.
3. Back up the original file with a `.bak#` extension. `#` is the first available number for a file that doesn't exist, e.g. if `mycity.bak1` already exists, that file will be left alone, and the patcher will create a backup at `mycity.bak2`.
4. Write the patched city file directly to the cities folder.

**Note:** The patcher ONLY updates the city file. It is strongly recommended to start a new save after updating a city, since otherwise certain items (employment records, wallets, etc.) captured by save files will refer to old name.

### Creating the names file

You must make the text file beforehand, by placing each name on a separate line. A name can take one of three formats:

```
Firstname Lastname Nickname
Firstname Lastname
Lastname
```

If only two words are found, the nickname is assumed to be the same as the provided first name. If only one word is found, it is assumed to be the last name, and the first name and nickname for that citizen will not be replaced. If there are four or more words on a line, all words after the third are ignored.

Names can contain only letters, and all spaces are considered separators. All invalid characters are removed, and names conform to Title Case (i.e. first letter capitalized, remaining letters lower case) to conform to the behavior observed in SoD's Twitch integration.

### Choosing gender

In addition, you can optionally specify the gender of your names. The patcher will select citizens which have already been assigned that gender. By default, the patcher is in `#any` mode and will randomly select citizens to replace. A different mode can be activated by providing a pound sign followed by the gender. Note that nonbinary is a third gender category defined in Shadows of Doubt, and is different from the `#any` mode.

To change the gender mode, provide the specifier on its own line:

```
#any
#female
#male
#nonbinary
```

A full, sample file of names might look as follows:

```
Bosley
Alex Munday
Dylan Sanders
Natalie Cook Nat

#male
Jacques Clouseau Jack
Encyclopedia Brown

#female
Jennifer Aniston Jen
Monica Geller Mon
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

Note that blank lines are allowed at any point, and that a gender mode can be selected at any time, and even repeated. Since `#any` is the default mode, the first four names in the sample file will each be assigned to a random citizen with no regard for gender.
