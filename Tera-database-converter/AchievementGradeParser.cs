using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class AchievementGradeParser
    {
        static Dictionary<uint, string> Grades;
        static XDocument Doc;
        static void Load()
        {
            Doc = XDocument.Load(Utilities.DATABASE_PATH + "EU-EN/StrSheet_AchievementGradeInfo.xml");
        }
        static void ParseDoc()
        {
            Grades = new Dictionary<uint, string>();
            foreach (var item in Doc.Descendants().Where(x => x.Name == "String"))
            {
                var id = uint.Parse(item.Attribute("id").Value);
                var name = item.Attribute("string").Value;
                if (id > 105 || id < 101) continue;
                Grades.Add(id, name);
            }
        }
        static void Dump()
        {
            List<string> lines = new List<string>();
            foreach (var keyVal in Grades)
            {
                var sb = new StringBuilder();
                sb.Append(keyVal.Key);
                sb.Append("\t");
                sb.Append(keyVal.Value);

                lines.Add(sb.ToString());
            }
            File.WriteAllLines("achievement-grade-info.tsv", lines);
        }
        public static void Parse()
        {
            Load();
            ParseDoc();
            Dump();
        }

    }
}