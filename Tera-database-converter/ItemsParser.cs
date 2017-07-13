using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class ItemsParser
    {
        public static Dictionary<uint, Item> Items;
        static List<XDocument> StrSheet_ItemDocs;
        static List<XDocument> ItemDataDocs;

        static void Load()
        {
            StrSheet_ItemDocs = new List<XDocument>();
            ItemDataDocs = new List<XDocument>();

            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + "/StrSheet_Item"))
            {
                var d = XDocument.Load(f);
                StrSheet_ItemDocs.Add(d);
            }

            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + "/ItemData"))
            {
                var d = XDocument.Load(f);
                ItemDataDocs.Add(d);
            }
        }
        static void ParseDocs()
        {
            foreach (var item in ItemDataDocs)
            {
                ParseItemDataDoc(item);
            }
            foreach (var item in StrSheet_ItemDocs)
            {
                ParseStrSheetDoc(item);
            }
        }
        static void ParseItemDataDoc(XDocument doc)
        {
            foreach (var item in doc.Descendants().Where(x => x.Name == "Item"))
            {
                //if (item.Attribute("obtainable").Value == "False") continue;
                var id = UInt32.Parse(item.Attribute("id").Value);
                var grade = UInt32.Parse(item.Attribute("rareGrade").Value);
                var bind = item.Attribute("boundType").Value;

                Items.Add(id, new Item(id, "", bind, grade));
            }
        }
        static void ParseStrSheetDoc(XDocument doc)
        {
            foreach (var item in doc.Descendants().Where(x => x.Name == "String"))
            {
                var id = UInt32.Parse(item.Attribute("id").Value);
                var name = item.Attribute("string").Value;
                try
                {
                    Items[id].Name = name;
                }
                catch (Exception)
                {

                    Console.WriteLine("Skipped {0}", name);
                }
            }
        }
        static void Dump()
        {
            List<string> lines = new List<string>();
            foreach (var item in Items)
            {
                if (item.Value.Name == "") continue;
                if (item.Value.Name.Contains("TBU")) continue;
                if (item.Value.Name.Contains("PC Cafe")) continue;
                if (item.Value.Name.Any(x => (ushort)x > 0xAC00 && (ushort)x < 0xD7A3)) continue;
                var sb = new StringBuilder();
                sb.Append(item.Value.Id);
                sb.Append('\t');
                sb.Append(item.Value.RareGrade);
                sb.Append('\t');
                sb.Append(item.Value.BoundType);
                sb.Append('\t');
                sb.Append(item.Value.Name);

                lines.Add(sb.ToString());
            }

            File.WriteAllLines("items.tsv", lines);
        }
        public static void Parse()
        {
            Items = new Dictionary<uint, Item>();
            Load();
            ParseDocs();
            Dump();
        }
    }

    public class Item
    {
        public uint Id;
        public string Name;
        public string BoundType;
        public uint RareGrade;
        public Item(uint id, string name, string b, uint g)
        {
            Id = id;
            Name = name;
            BoundType = b;
            RareGrade = g;
        }
    }
}
