using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ElexModMerger
{
    class ElexModMerger
    {
        static void Main(string[] args)
        {
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
