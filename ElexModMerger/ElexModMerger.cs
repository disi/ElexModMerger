using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace ElexModMerger
{
    class ElexModMerger
    {
        static void Main(string[] args)
        {
            // extract all them mod files
            if (File.Exists("m_9_MergeMod.pak"))
            {
                Process.Start("CMD.exe", "/C echo \"*\" | elexresman.exe " + "m_9_MergeMod.pak");
            }
            var mods = Directory.GetFiles(".", "m_?_*.pak");
            foreach (string file in mods)
            {
                Process.Start("CMD.exe", "/C echo \"*\" | elexresman.exe " + file);
            }
            // here needs to be an input to wait for the files to write to disk
            Console.WriteLine("Extracted all mods!");
            Console.ReadKey();

            // find all them w_info.hdr and convert those, then add to winforesult
            string path = @System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var wInfofiles = Directory.GetFiles(path, "w_info.hdr", SearchOption.AllDirectories);
            Console.WriteLine("Found the w_info.hdr files in " + path + " \nNumber of files: " + wInfofiles.Length);
            Console.ReadKey();
            Dictionary<string, List<string>> wInfoResult = new Dictionary<string, List<string>>();
            foreach (string winfofile in wInfofiles)
            {
                Console.WriteLine("File : " + winfofile);
                Console.ReadKey();
                Process.Start("CMD.exe", "/C elexresman.exe " + winfofile);
                // here needs to be an input to wait for the file to write to disk or maybe some flush-command?
                var wInfo = readInfos(infofile: winfofile + "doc");
                wInfoResult.Concat(wInfo.Where(x => !wInfoResult.Keys.Contains(x.Key)));
            }

            /*
            Dictionary<string, List<string>> wInfo1 = readInfos(infofile: "w_info.hdrdoc");
            Dictionary<string, List<string>> wInfo2 = readInfos(infofile: "w_info1.hdrdoc");
            Console.WriteLine("Read files!");
            Console.WriteLine("Press any key to combine!");
            Console.ReadKey();
            var winforesult = wInfo1.Concat(wInfo2.Where(x => !wInfo1.Keys.Contains(x.Key))); ;
            Console.WriteLine("Combined!");
            Console.WriteLine("Press any key to write!");
            Console.ReadKey();
            TextWriter tw = new StreamWriter("w_info_new.hdrdoc");
            tw.WriteLine("class gCInfo {");
            foreach (KeyValuePair<string, List<string>> info in winforesult)
            {
                foreach (string item in info.Value)
                {
                    tw.WriteLine(item);
                }
            }
            tw.WriteLine("}");
            tw.Close();
            Console.WriteLine("Done!");
            Console.ReadKey();
            */
        }

        private static Dictionary<string, List<string>> readInfos(string infofile)
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
    }
}
