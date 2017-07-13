using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class AchievementsParser
    {
        static List<XDocument> StrDocs;
        static List<XDocument> DataDocs;
        static Dictionary<uint, string> Achievements;
        static void Load()
        {
            StrDocs = new List<XDocument>();
            DataDocs = new List<XDocument>();
            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + "/StrSheet_Achievement/"))
            {
                if (f.EndsWith("-1.xml")) continue;
                StrDocs.Add(XDocument.Load(f));
            }
            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + "/AchievementList/"))
            {
                if (f.EndsWith("-0.xml")) continue;
                DataDocs.Add(XDocument.Load(f));
            }
        }
        static void ParseDocs()
        {
            var nameIdtoId = new Dictionary<uint, uint>();
            Achievements = new Dictionary<uint, string>();
            foreach (var doc in DataDocs)
            {
                foreach (var item in doc.Descendants().Where(x => x.Name == "Achievement"))
                {
                    var id = UInt32.Parse(item.Attribute("id").Value);
                    var n = item.Attribute("name").Value;
                    var s = n.Substring(n.IndexOf(':') + 1);
                    var nameId = UInt32.Parse(s);
                    nameIdtoId.Add(nameId, id);
                }
            }
            foreach (var doc in StrDocs)
            {
                foreach (var str in doc.Descendants().Where(x => x.Name == "String"))
                {
                    if (str.Attribute("string").Value == "") continue;
                    if (!str.Attribute("id").Value.EndsWith("1")) continue;

                    var name = str.Attribute("string").Value;
                    var id = UInt32.Parse(str.Attribute("id").Value);

                    if (id % 1000 == 501) continue;

                    try
                    {
                        Achievements.Add(nameIdtoId[id], name);

                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }
        static void Dump()
        {
            List<string> lines = new List<string>();
            foreach (var keyVal in Achievements)
            {
                var sb = new StringBuilder();
                sb.Append(keyVal.Key);
                sb.Append("\t");
                sb.Append(keyVal.Value);

                lines.Add(sb.ToString());
            }
            File.WriteAllLines("achievements.tsv", lines);
        }
        public static void Parse()
        {
            Load();
            ParseDocs();
            Dump();
        }
    }
}
