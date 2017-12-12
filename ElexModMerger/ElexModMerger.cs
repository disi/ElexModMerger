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
            StringComparison comp = StringComparison.OrdinalIgnoreCase;
            if (!workingDir.Contains("\\data\\packed"))
            {
                Console.WriteLine("This program needs to run in elex\\data\\packed!");
                Console.WriteLine("Press any key to exit!");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // check environment
            if (!File.Exists("elexresman.exe"))
            {
                Console.WriteLine("No elexresman.exe found!");
                Console.WriteLine("Press any key to exit!");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // extract all them mod files
            if (File.Exists("m_9_MergeMod.pak"))
            {
                Process.Start("CMD.exe", "/C echo \"*\" | elexresman.exe " + "m_9_MergeMod.pak").WaitForExit();
                File.Delete("m_9_MergeMod.pak");
            }
            var mods = Directory.GetFiles(".", "m_?_*.pak");
            foreach (string file in mods)
            {
                Process.Start("CMD.exe", "/C echo \"*\" | elexresman.exe " + @file).WaitForExit();
            }
            Console.WriteLine("Extracted all mods");
            Console.WriteLine("Press any key!");
            Console.ReadKey();

            // find all them w_info.hdr and convert those, then add to winforesult and write new w_info.hdr
            var wInfofiles = Directory.GetFiles(".", "w_info.hdr", SearchOption.AllDirectories);
            Directory.CreateDirectory("MergeMod");
            if (wInfofiles.Length != 0)
            {
                Dictionary<string, List<string>> wInfoResult = new Dictionary<string, List<string>>();
                foreach (string winfofile in wInfofiles)
                {
                    Process.Start("CMD.exe", "/C elexresman.exe " + winfofile).WaitForExit();
                    Console.WriteLine("Adding current File: " + winfofile);
                    var wInfo = ReadInfos(infofile: winfofile + "doc");
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
                Process.Start("CMD.exe", "/C elexresman.exe " + "MergeMod\\documents\\w_info.hdrdoc").WaitForExit();
                File.Delete("MergeMod\\documents\\w_info.hdrdoc");
            }
            else
            {
                Console.WriteLine("No w_info.hdr found!");
            }

            // create MergeMod
            if (Directory.GetFiles("MergeMod", "*", SearchOption.AllDirectories).Length != 0)
            {
                Console.WriteLine("Create m_9_MergeMod.pak");
                Console.WriteLine("Press any key!");
                Console.ReadKey();
                Process myProcess = new System.Diagnostics.Process();
                myProcess.StartInfo.FileName = "elexresman.exe";
                myProcess.StartInfo.Arguments = "MergeMod";
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardInput = true;
                myProcess.Start();
                StreamWriter myStreamWriter = myProcess.StandardInput;
                myStreamWriter.WriteLine("9");
                myProcess.WaitForExit();
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

        private static Dictionary<string, List<string>> ReadInfos(string infofile)
        {
            Dictionary<string, List<string>> newDict = new Dictionary<string, List<string>>();
            string[] lines = File.ReadAllLines(infofile);
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

        private static void CleanDirs(string moddir)
        {
            Console.WriteLine("Press 'y' to remove all directories!");
            ConsoleKeyInfo result = Console.ReadKey();
            if (result.KeyChar == 'y' || result.KeyChar == 'Y')
            {
                foreach (var subDir in new DirectoryInfo(@moddir).GetDirectories())
                {
                    subDir.Delete(true);
                }
            }
        }
    }
}
