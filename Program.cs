using F76CIG;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization.Json;

bool consoleDebug = false;
string dirCurrent = AppDomain.CurrentDomain.BaseDirectory;  // Path to root folder
string dirMaterials = dirCurrent + @"materials\";           // Path to materials folder (original)
string dirTextures = dirCurrent + @"textures\";             // Path to textures folder (new)
string dirRelease = dirCurrent + @"release\";               // Path to release folder (new)
string dirGame = File.ReadAllText("game.txt");              // Path to game folder
Dictionary<string, List<string>> dictCategory = new();      // [Category][Hex Color Code], [Category][Emittance Multiplier], [Category][Adaptive Exposure Offset], [Category][Texture]
Dictionary<string, List<string>> dictAdd = new();           // [Category][Path or Name of the Item]
List<string> listIgnore = new();                            // [Path or Name of the Item]
bool releaseClear = true;                                   // Clear release folder
bool releaseOverwrite = true;                               // Overwrite release folder
bool releaseUseCategories = false;                          // Divide release into categories

Init();
LoadAndSave();
Pack();

Console.WriteLine("Done!\nPress Enter to exit.");
Console.ReadLine();

void Init()
{
    Console.WriteLine("[Initialization] Please, wait...");
    if (consoleDebug) Console.WriteLine("Loading colors.txt");
    string[] fileTextLines = File.ReadAllLines("colors.txt");
    for (var i = 0; i < fileTextLines.Length; i++)
    {
        if (fileTextLines[i].Length > 2 && fileTextLines[i].Substring(0, 2) != "//")
        {
            dictCategory.Add(fileTextLines[i].Replace("--", "").Trim(), new List<string>() { fileTextLines[i + 1], fileTextLines[i + 2], fileTextLines[i + 3], fileTextLines[i + 4] });
            i += 5;
        }
    }

    if (consoleDebug) Console.WriteLine("Loading add.txt");
    fileTextLines = File.ReadAllLines("add.txt");
    string category = "";
    foreach (string fileTextLine in fileTextLines)
    {
        if (fileTextLine.Length > 2 && fileTextLine.Substring(0, 2) != "//")
        {
            if (fileTextLine.Contains("--"))
            {
                category = fileTextLine.Replace("--", "").Trim();
                dictAdd.Add(category, new());
            }
            else dictAdd[category].Add(fileTextLine.ToLower());
        }
    }

    if (consoleDebug) Console.WriteLine("Loading ignore.txt");
    fileTextLines = File.ReadAllLines("ignore.txt");
    foreach (string fileTextLine in fileTextLines)
    {
        if (fileTextLine.Length > 2 && fileTextLine.Substring(0, 2) != "//")
        {
            listIgnore.Add(fileTextLine.ToLower());
        }
    }

    if(!Directory.Exists(dirMaterials)) Unpack();
}

void LoadAndSave()
{
    Console.WriteLine("[Editing] Please, wait...");
    if (consoleDebug) Console.WriteLine("Starting...");
    if (Directory.Exists(dirMaterials))
    {
        if (!Directory.Exists(dirRelease)) Directory.CreateDirectory(dirRelease); else if (releaseClear) { Directory.Delete(dirRelease, true); Directory.CreateDirectory(dirRelease); }
        if (!releaseUseCategories) CopyDirectoryWithFiles(dirTextures, dirRelease + "textures\\");
        ProcessDirectory(dirMaterials);
    }
    else
    {
        Console.WriteLine("Error, place 'materials' folder in root folder of this utility.\bPress Enter to exit.");
        Console.ReadLine();
    }
}

void ProcessDirectory(string targetDirectory)
{
    string[] fileEntries = Directory.GetFiles(targetDirectory);
    foreach (string fileName in fileEntries) ProcessFile(fileName);
    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
    foreach (string subdirectory in subdirectoryEntries) ProcessDirectory(subdirectory);
}

void ProcessFile(string path)
{
    string pathFixed = path.ToLower();
    if (pathFixed.EndsWith(".bgsm"))
    {
        bool ignore = false;
        foreach (string name in listIgnore)
        {
            if (pathFixed.Contains(name))
            {
                ignore = true;
                if (consoleDebug) Console.WriteLine($"! Ignoring {name}");
                break;
            }
        }
        if (!ignore)
        {
            foreach (KeyValuePair<string, List<string>> addKeyVal in dictAdd)
            {
                foreach (string name in addKeyVal.Value)
                {
                    if (pathFixed.Contains(name))
                    {
                        string category = addKeyVal.Key;
                        string releasePath = dirRelease + path.Replace(dirCurrent, "");
                        //path = dirRelease + path.Replace(dirCurrent, ""); // Change directory to release
                        //Console.WriteLine("releasePath: " + releasePath);
                        if (releaseOverwrite || !File.Exists(releasePath))
                        {
                            BaseMaterialFile file = LoadFile(path);
                            SaveFile(file, releasePath, category); //dictCategory[category][0], dictCategory[category][1], dictCategory[category][2], dictCategory[category][3], category); // "Props/Glow/gwhite.dds"
                        }
                        break;
                    }
                }
            }
        }
    }
}

void CopyDirectoryWithFiles(string sourcePath, string targetPath)
{
    Directory.CreateDirectory(targetPath);
    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
    {
        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
    }
    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
    {
        File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
    }
}

BaseMaterialFile LoadFile(string path)
{
    if (consoleDebug) Console.WriteLine($"Loading {Path.GetFileName(path)}");
    BaseMaterialFile material = new BGSM();
    using (FileStream file = new FileStream(path, FileMode.Open))
    {
        char start = Convert.ToChar(file.ReadByte());
        file.Position = 0;
        // Check for JSON
        if (start == '{' || start == '[')
        {
            try
            {
                var ser = new DataContractJsonSerializer(material.GetType());
                material = (BGSM)ser.ReadObject(file);
            }
            catch (Exception) { }
        }
        // Try binary
        else if (!material.Open(file))
        {
            Console.WriteLine($"ERROR Failed to open file '{path}'!");
        }
    }
    return material;
}

void SaveFile(BaseMaterialFile file, string path, string category)
{
    if (releaseUseCategories) // Add categories and copy textures
    {
        path = path.Replace("\\release\\", "\\release\\" + category + "\\");
        if (!Directory.Exists(dirRelease + category + "\\textures\\")) CopyDirectoryWithFiles(dirTextures, dirRelease + category + "\\textures\\");
    }
    Directory.CreateDirectory(path.Replace(Path.GetFileName(path), "")); // Create directory
    if (consoleDebug) Console.WriteLine($"Saving {Path.GetFileName(path)}");
    string color = dictCategory[category][0].Replace("#", ""); // Just in case, remove '#'
    if (color.Length != 6) color = "FF00FF"; // If something is wrong - set purple color
    if (!float.TryParse(dictCategory[category][1], out float emit)) { Console.WriteLine($"ERROR 'Emittance Multiplier' in category '{category}' is invalid, using default '0.20000'"); emit = 0.20000f; }
    if (!float.TryParse(dictCategory[category][2], out float expo)) { Console.WriteLine($"ERROR 'Adaptive Exposure Offset' in category '{category}' is invalid, using default '0.20000'"); expo = 0.20000f; }
    string texture = dictCategory[category][3] + ".dds";
    if (!File.Exists(dirTextures + texture)) { Console.WriteLine($"ERROR 'Texture' in category '{category}' is invalid, using default 'glow_white'"); texture = "glow_white.dds"; }
    // Editing BGSM params
    BGSM bgsm = (BGSM)file;
    bgsm.GlowTexture = texture;
    bgsm.EmitEnabled = true;
    bgsm.EmittanceColor = (uint)ColorTranslator.FromHtml("#" + color).ToArgb();
    bgsm.EmittanceMult = emit;
    bgsm.ExternalEmittance = false;
    bgsm.LumEmittance = 0.00000f;
    bgsm.UseAdaptativeEmissive = true;
    bgsm.AdaptativeEmissive_ExposureOffset = expo;
    bgsm.AdaptativeEmissive_FinalExposureMin = expo;
    bgsm.AdaptativeEmissive_FinalExposureMax = expo;
    bgsm.Glowmap = true;
    bgsm.Tree = false; // Fix razorgrain and other plants
    // Saving BGSM file
    try
    {
        using (var saveFile = new FileStream(path, FileMode.Create))
        {
            if (!file.Save(saveFile))
            {
                Console.WriteLine($"ERROR Failed to save file '{path}'!");
                return;
            }
        }
    }
    catch
    {
        Console.WriteLine($"ERROR Failed to save file '{path}'!");
        return;
    }
}

void Unpack()
{
    Console.WriteLine("[Unpacking] Please, wait...");
    if (consoleDebug) Console.WriteLine("Unpacking 'SeventySix - Materials.ba2'...");
    if (dirGame[dirGame.Length - 1] != '\\') dirGame += "\\";
    dirGame += "Data\\SeventySix - Materials.ba2";
    using var process = new Process();
    process.StartInfo.FileName = "bsarch.exe";
    process.StartInfo.Arguments = "unpack \"" + dirGame + "\" \".\\\"";
    process.Start();
    process.WaitForExit();
    // Clear materials directory from other files
    foreach (string filePath in Directory.GetFiles(dirMaterials, "*.*", SearchOption.AllDirectories))
    {
        if (Path.GetExtension(filePath) != ".bgsm")
        {
            File.Delete(filePath);
        }
    }
    foreach (string dirPath in Directory.GetDirectories(dirMaterials, "*", SearchOption.AllDirectories))
    {
        foreach (string filePath in Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(filePath) != ".bgsm")
            {
                File.Delete(filePath);
            }
        }
    }
}

void Pack()
{
    Console.WriteLine("[Packing] Please, wait...");
    if (consoleDebug) Console.WriteLine("Packing files into ba2 archive...");
    if (!releaseUseCategories)
    {
        using var process = new Process();
        process.StartInfo.FileName = "bsarch.exe";
        process.StartInfo.Arguments = "pack \"release\\\" \"release\\Customizable Item Glow.ba2\" -fo4 -z";
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit();
    }
    else
    {
        foreach(var category in dictCategory.Keys)
        {
            using var process = new Process();
            process.StartInfo.FileName = "bsarch.exe";
            process.StartInfo.Arguments = $"pack \"release\\{category}\\\" \"release\\Customizable Item Glow - {category}.ba2\" -fo4 -z";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
        }
    }
}