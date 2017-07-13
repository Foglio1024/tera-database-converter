using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class SocialParser
    {
        static XDocument StrSheet_Social;
        static Dictionary<int, string> Social;
        static void LoadFile()
        {
            StrSheet_Social = XDocument.Load(Utilities.DATABASE_PATH + @"/StrSheet_Social.xml");
        }
        static void ParseSocialStrings()
        {
            foreach (var str in StrSheet_Social.Descendants().Where(x => x.Name == "String"))
            {
                if (str.Attribute("string").Value.Contains(@"{Name}"))
                {
                    var id = Int32.Parse(str.Attribute("id").Value);
                    Social.Add(id, str.Attribute("string").Value);
                }
            }
        }
        static void DumpToTSV()
        {
            List<string> lines = new List<string>();
            foreach (var item in Social)
            {
                StringBuilder s = new StringBuilder();
                s.Append(item.Key);
                s.Append("\t");
                s.Append(item.Value);
                lines.Add(s.ToString());
            }
            File.WriteAllLines("social.tsv", lines);
        }
        public static void Parse()
        {
            Social = new Dictionary<int, string>();
            LoadFile();
            ParseSocialStrings();
            DumpToTSV();
        }
    }
}
