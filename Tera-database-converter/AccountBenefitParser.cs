using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
namespace TeraDatabaseConverter
{
    public static class AccountBenefitParser
    {
        static XDocument Doc;

        public static void Parse()
        {
            Doc = XDocument.Load(Utilities.DATABASE_PATH + "EU-EN/StrSheet_AccountBenefit.xml");
            List<string> lines = new List<string>();
            foreach (var item in Doc.Descendants().Where(x => x.Name == "String"))
            {
                var sb = new StringBuilder();
                sb.Append(item.Attribute("id").Value);
                sb.Append('\t');
                sb.Append(item.Attribute("string").Value);
                lines.Add(sb.ToString());
            }

            File.WriteAllLines("account-benefit.tsv", lines);
        }
    }
}
