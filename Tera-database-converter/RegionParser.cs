using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class RegionParser
    {
        static XDocument StrSheet_Region;
        static Dictionary<int, string> Regions;
        static void LoadFile(string region)
        {
            StrSheet_Region = XDocument.Load(Utilities.DATABASE_PATH + region + @"/StrSheet_Region.xml");
        }
        static void ParseRegionStrings()
        {
            foreach (var str in StrSheet_Region.Descendants().Where(x => x.Name == "String"))
            {

                var id = Int32.Parse(str.Attribute("id").Value);
                Regions.Add(id, str.Attribute("string").Value);
            }
        }
        static void DumpToTSV()
        {
            List<string> lines = new List<string>();
            foreach (var item in Regions)
            {
                StringBuilder s = new StringBuilder();
                s.Append(item.Key);
                s.Append("\t");
                s.Append(item.Value);
                lines.Add(s.ToString());
            }
            File.WriteAllLines("regions.tsv", lines);
        }
        public static void Parse(string region)
        {
            Regions = new Dictionary<int, string>();
            LoadFile(region);
            ParseRegionStrings();
            DumpToTSV();
        }

    }
}
