using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class DungeonParser
    {
        static Dictionary<uint, string> Dungeons;
        static XDocument Doc;
        static void Load()
        {
            Doc = XDocument.Load(Utilities.DATABASE_PATH + "/StrSheet_Dungeon/StrSheet_Dungeon-0.xml");
        }
        static void ParseDoc()
        {
            Dungeons = new Dictionary<uint, string>();
            foreach (var item in Doc.Descendants().Where(x => x.Name == "String"))
            {
                var id = uint.Parse(item.Attribute("id").Value);
                var name = item.Attribute("string").Value;
                Dungeons.Add(id, name);
            }
        }
        static void Dump()
        {
            List<string> lines = new List<string>();
            foreach (var keyVal in Dungeons)
            {
                var sb = new StringBuilder();
                sb.Append(keyVal.Key);
                sb.Append("\t");
                sb.Append(keyVal.Value);

                lines.Add(sb.ToString());
            }
            File.WriteAllLines("dungeons.tsv", lines);
        }
        public static void Parse()
        {
            Load();
            ParseDoc();
            Dump();
        }

    }

}
