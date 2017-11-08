using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class QuestsParser
    {
        static List<XDocument> Docs;
        static Dictionary<string, string> Quests;
        static void Load(string region)
        {
            Docs = new List<XDocument>();
            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + region + "/StrSheet_Quest/"))
            {
                if (f.EndsWith("-0.xml")) continue;
                Docs.Add(XDocument.Load(f));
            }
        }
        static void ParseDocs()
        {
            Quests = new Dictionary<string, string>();
            foreach (var doc in Docs)
            {
                var el = doc.Descendants().Where(x => x.Name == "StrSheet_Quest").Descendants().First();
                var id = el.Attribute("id").Value;
                var name = el.Attribute("string").Value;

                Quests.Add(id, name);
            }
        }
        static void Dump()
        {
            List<string> lines = new List<string>();
            foreach (var keyVal in Quests)
            {
                var sb = new StringBuilder();
                sb.Append(keyVal.Key);
                sb.Append("\t");
                sb.Append(keyVal.Value);

                lines.Add(sb.ToString());
            }
            File.WriteAllLines("quests.tsv", lines);
        }
        public static void Parse(string region)
        {
            Load(region);
            ParseDocs();
            Dump();
        }


    }
}
