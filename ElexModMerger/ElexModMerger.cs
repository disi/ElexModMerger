using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ElexModMerger
{
    class ElexModMerger
    {
        static void Main(string[] args)
        {
            // get the current directory
            string workingDir = @System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (!workingDir.Contains("\\data\\packed"))
            {
                Console.WriteLine("This program needs to run in elex\\data\\packed");
                Console.WriteLine("Press any key to exit!");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // check environment
            if (!File.Exists("elexresman.exe"))
            {
                Console.WriteLine("No elexresman.exe found");
                Console.WriteLine("Press any key to exit!");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // extract original files, they should always be added first starting with 'c' instead of 'm'
            if (File.Exists("c_1_na.pak"))
            {
                Console.WriteLine("Extracting c_1_na.pak");
                RunElexResMan(inputFile: "c_1_na.pak", inputArg: "4").WaitForExit();
                RunElexResMan(inputFile: "c_1_na.pak", inputArg: "5").WaitForExit();
            }

            // extract all them mod files
            if (File.Exists("m_9_MergeMod.pak"))
            {
                RunElexResMan(inputFile: "m_9_MergeMod.pak", inputArg: "*").WaitForExit();
                File.Delete("m_9_MergeMod.pak");
            }
            var mods = Directory.GetFiles(".", "m_?_*.pak");
            foreach (string file in mods)
            {
                RunElexResMan(inputFile: file, inputArg: "*").WaitForExit();
            }
            Console.WriteLine("Extracted all mods");
            Console.WriteLine("Press any key!");
            Console.ReadKey();

            // prepare directory
            Directory.CreateDirectory("MergeMod");

            // find all them w_info.hdr and convert those, then add to wInfoResult and write new w_info.hdr
            var wInfoFiles = Directory.GetFiles(".", "w_info.hdr", SearchOption.AllDirectories);
            if (wInfoFiles.Length != 0)
            {
                Dictionary<string, List<string>> wInfoResult = new Dictionary<string, List<string>>();
                foreach (string wInfoFile in wInfoFiles)
                {
                    RunElexResMan(inputFile: wInfoFile, inputArg: "").WaitForExit();
                    Console.WriteLine("Adding current File: " + wInfoFile);
                    var wInfo = ReadInfos(infoFile: wInfoFile + "doc");
                    foreach (KeyValuePair<string, List<string>> info in wInfo)
                    {
                        if (!wInfoResult.ContainsKey(info.Key))
                        {
                            wInfoResult.Add(info.Key, info.Value);
                        }
                    }
                }
                if (wInfoResult != null)
                {
                    Directory.CreateDirectory("MergeMod\\documents\\");
                    TextWriter tw = new StreamWriter("MergeMod\\documents\\w_info.hdrdoc");
                    tw.WriteLine("class gCInfo {");
                    foreach (KeyValuePair<string, List<string>> info in wInfoResult)
                    {
                        foreach (string item in info.Value)
                        {
                            tw.WriteLine(item);
                        }
                    }
                    tw.WriteLine("}");
                    tw.Close();
                }
                RunElexResMan(inputFile: "MergeMod\\documents\\w_info.hdrdoc", inputArg: "").WaitForExit();
                File.Delete("MergeMod\\documents\\w_info.hdrdoc");
            }
            else
            {
                Console.WriteLine("No w_info.hdr found!");
            }

            // find all them World.elexwrl and convert those, then add to sectorResult and write new World.elexwrl
            var worldFiles = Directory.GetFiles(".", "World.elexwrl", SearchOption.AllDirectories);
            if (worldFiles.Length != 0)
            {
                HashSet<string> sectorResult = new HashSet<string>();
                foreach (string worldFile in worldFiles)
                {
                    RunElexResMan(inputFile: worldFile, inputArg: "").WaitForExit();
                    Console.WriteLine("Adding current File: " + worldFile);
                    sectorResult.UnionWith(ReadWorld(worldFile: worldFile + "doc"));
                }
                if (sectorResult != null)
                {
                    Directory.CreateDirectory("MergeMod\\World\\");
                    TextWriter tw = new StreamWriter("MergeMod\\World\\World.elexwrldoc");
                    tw.WriteLine("class gCTerrain {");
                    tw.WriteLine("    Version = 2;");
                    tw.WriteLine("    Properties {");
                    tw.WriteLine("        class bCString FileName = \"#G3:/data/raw/terrain3/terrain3.trn\";");
                    tw.WriteLine("    }");
                    tw.WriteLine("    ClassData {");
                    tw.WriteLine("    }");
                    tw.WriteLine("}");
                    tw.WriteLine("Sectors = [");
                    foreach (string sector in sectorResult)
                    {
                        tw.WriteLine(sector);
                    }
                    // dummy sector
                    tw.WriteLine("    \"\"");
                    tw.WriteLine("]");
                    tw.Close();
                    RunElexResMan(inputFile: "MergeMod\\World\\World.elexwrldoc", inputArg: "").WaitForExit();
                    File.Delete("MergeMod\\World\\World.elexwrldoc");
                }
            }
            else
            {
                Console.WriteLine("No World.elexwrl found!");
            }

            // find all them w_quest.hdr and convert those, then add to wInfoResult and write new w_quest.hdr
            var wQuestFiles = Directory.GetFiles(".", "w_quest.hdr", SearchOption.AllDirectories);
            if (wQuestFiles.Length != 0)
            {
                Dictionary<string, List<string>> wQuestResult = new Dictionary<string, List<string>>();
                foreach (string wQuestFile in wQuestFiles)
                {
                    Process.Start("CMD.exe", "/C elexresman.exe " + wQuestFile).WaitForExit();
                    Console.WriteLine("Adding current File: " + wQuestFile);
                    var wQuest = ReadQuests(questFile: wQuestFile + "doc");
                    foreach (KeyValuePair<string, List<string>> quest in wQuest)
                    {
                        if (!wQuestResult.ContainsKey(quest.Key))
                        {
                            wQuestResult.Add(quest.Key, quest.Value);
                        }
                    }
                }
                if (wQuestResult != null)
                {
                    Directory.CreateDirectory("MergeMod\\documents\\");
                    TextWriter tw = new StreamWriter("MergeMod\\documents\\w_quest.hdrdoc");
                    tw.WriteLine("class gCQuest {");
                    foreach (KeyValuePair<string, List<string>> quest in wQuestResult)
                    {
                        foreach (string item in quest.Value)
                        {
                            tw.WriteLine(item);
                        }
                    }
                    tw.WriteLine("}");
                    tw.Close();
                }
                Process.Start("CMD.exe", "/C elexresman.exe " + "MergeMod\\documents\\w_quest.hdrdoc").WaitForExit();
                File.Delete("MergeMod\\documents\\w_quest.hdrdoc");
            }
            else
            {
                Console.WriteLine("No w_quest.hdr found!");
            }

            // create MergeMod
            if (Directory.GetFiles("MergeMod", "*", SearchOption.AllDirectories).Length != 0)
            {
                Console.WriteLine("Create m_9_MergeMod.pak");
                Console.WriteLine("Press any key!");
                Console.ReadKey();
                Process mergeProc = RunElexResMan("MergeMod", "9");
                mergeProc.WaitForExit();
            }
            else
            {
                Console.WriteLine("No files found to create MergeMod!");
            }

            // cleanup
            CleanDirs(@workingDir);

            // done
            Console.WriteLine("");
            Console.WriteLine("All done!");
            Console.ReadKey();
        }

        private static Process RunElexResMan(string inputFile, string inputArg)
        {
            Process myProcess = new System.Diagnostics.Process();
            myProcess.StartInfo.FileName = "elexresman.exe";
            myProcess.StartInfo.Arguments = inputFile;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.Start();
            StreamWriter myStreamWriter = myProcess.StandardInput;
            myStreamWriter.WriteLine(inputArg);
            return myProcess;
        }

        private static Dictionary<string, List<string>> ReadInfos(string infoFile)
        {
            Dictionary<string, List<string>> newDict = new Dictionary<string, List<string>>();
            string[] lines = File.ReadAllLines(infoFile);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (!String.IsNullOrWhiteSpace(lines[i]) && !lines[i].StartsWith("}"))
                {
                    if (lines[i].Contains("\" {"))
                    {
                        string key = lines[i].Split('\"')[1];
                        List<string> values = new List<string> { lines[i] };
                        int y;
                        for (y = i + 1; !lines[y].Contains("\" {") && !lines[y].StartsWith("}"); y++)
                        {
                            values.Add(lines[y]);
                        }
                        newDict.Add(key, values);
                        i = y - 1;
                    }
                }
            }
            return newDict;
        }

        private static HashSet<string> ReadWorld(string worldFile)
        {
            string[] lines = File.ReadAllLines(worldFile);
            HashSet<string> sectors = new HashSet<string>();
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (lines[i].StartsWith("    \"") && !lines[i].Contains("]"))
                {
                    if (lines[i].Last<char>() == ',')
                    {
                        sectors.Add(lines[i]);
                    }
                    else
                    {
                        sectors.Add(lines[i] + ",");
                    }
                }
            }
            return sectors;
        }

        private static Dictionary<string, List<string>> ReadQuests(string questFile)
        {
            Dictionary<string, List<string>> newDict = new Dictionary<string, List<string>>();
            string[] lines = File.ReadAllLines(questFile);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                if (!String.IsNullOrWhiteSpace(lines[i]) && !lines[i].StartsWith("}"))
                {
                    if (lines[i].Contains("\" {"))
                    {
                        string key = lines[i].Split('\"')[1];
                        List<string> values = new List<string> { lines[i] };
                        int y;
                        for (y = i + 1; !lines[y].Contains("\" {") && !lines[y].StartsWith("}"); y++)
                        {
                            values.Add(lines[y]);
                        }
                        newDict.Add(key, values);
                        i = y - 1;
                    }
                }
            }
            return newDict;
        }

        private static void CleanDirs(string modDir)
        {
            Console.WriteLine("Press 'y' to remove all directories!");
            ConsoleKeyInfo result = Console.ReadKey();
            if (result.KeyChar == 'y' || result.KeyChar == 'Y')
            {
                foreach (var subDir in new DirectoryInfo(modDir).GetDirectories())
                {
                    subDir.Delete(true);
                }
            }
        }
    }
}
