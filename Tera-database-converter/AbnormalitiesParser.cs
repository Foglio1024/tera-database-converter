using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    class AbnormalitiesParser
    {
        public static Dictionary<uint, Abnormality> Abnormalities;

        internal static void Parse(string lang)
        {
            Populate(lang);
            DumpToTSV(lang);
        }

        static List<XDocument> StrSheet_AbnormalityDocs;
        static List<XDocument> AbnormalityIconDataDocs;
        static List<XDocument> AbnormalityDataDocs;

        static void LoadFiles(string region)
        {
            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + region + "/StrSheet_Abnormality"))
            {
                var d = XDocument.Load(f);
                StrSheet_AbnormalityDocs.Add(d);
            }

            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + region + "/AbnormalityIconData"))
            {
                var d = XDocument.Load(f);
                AbnormalityIconDataDocs.Add(d);
            }

            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + region + "/Abnormality"))
            {
                var d = XDocument.Load(f);
                AbnormalityDataDocs.Add(d);
            }
        }

        static void ParseAbnormalityDataDoc(XDocument doc)
        {
            foreach (var a in doc.Descendants().Where(x => x.Name == "Abnormal"))
            {
                uint id = Convert.ToUInt32(a.Attribute("id").Value);
                bool isShow = true;
                if (a.Attribute("isShow").Value == "False")
                {
                    isShow = false;
                }
                bool isBuff = true;
                if (a.Attribute("isBuff").Value == "False")
                {
                    isBuff = false;
                }
                bool infinity = false;
                if (a.Attribute("infinity").Value == "True")
                {
                    infinity = true;
                }
                int prop = Convert.ToInt32(a.Attribute("property").Value);

                Abnormality ab = new Abnormality(id, isShow, isBuff, infinity, (AbnormalityType)prop);
                foreach (var b in a.Descendants().Where(x => x.Name == "AbnormalityEffect"))
                {
                    try
                    {
                        Double.TryParse(b.Attribute("value").Value.Replace('.', ','), out double val);
                        var method = Convert.ToInt32(b.Attribute("method").Value);
                        var tickInterval = Convert.ToInt32(b.Attribute("tickInterval").Value);

                        ab.Effects.Add(new Effect(val, method, tickInterval));

                    }
                    catch (Exception)
                    {


                    }

                }
                Abnormalities.Add(ab.Id, ab);
            }
        }
        static void ParseStrSheetAbnormalityDoc(XDocument doc)
        {
            foreach (XElement abn in doc.Descendants().Where(x => x.Name == "String"))
            {
                var id = Convert.ToUInt32(abn.Attribute("id").Value);
                string name = String.Empty;
                string toolTip = String.Empty;
                if (abn.Attributes().Any(x => x.Name == "name"))
                {
                    name = abn.Attribute("name").Value;
                }
                if (abn.Attributes().Any(x => x.Name == "tooltip"))
                {
                    toolTip = abn.Attribute("tooltip").Value;
                }

                if (Abnormalities.TryGetValue(id, out Abnormality a))
                {
                    a.SetInfo(name, toolTip);
                }

            }
        }
        static void ParseAbnormalityIconDoc(XDocument doc)
        {
            foreach (var s in doc.Descendants().Where(x => x.Name == "Icon"))
            {
                var id = Convert.ToUInt32(s.Attribute("abnormalityId").Value);
                string iconName = string.Empty;
                if (s.Attributes().Any(x => x.Name == "iconName"))
                {
                    iconName = s.Attribute("iconName").Value;
                }

                if (Abnormalities.TryGetValue(id, out Abnormality a))
                {
                    a.SetIcon(iconName.ToLower());
                }
            }
        }

        static void Populate(string region)
        {
            Abnormalities = new Dictionary<uint, Abnormality>();
            StrSheet_AbnormalityDocs = new List<XDocument>();
            AbnormalityIconDataDocs = new List<XDocument>();
            AbnormalityDataDocs = new List<XDocument>();

            LoadFiles(region);

            foreach (var doc in AbnormalityDataDocs)
            {
                ParseAbnormalityDataDoc(doc);
            }
            foreach (var doc in StrSheet_AbnormalityDocs)
            {
                ParseStrSheetAbnormalityDoc(doc);
            }
            foreach (var doc in AbnormalityIconDataDocs)
            {
                ParseAbnormalityIconDoc(doc);
            }

            //remove items with null name or tooltip
            var toBeRemovedList = new List<Abnormality>();
            foreach (var item in Abnormalities)
            {
                if (item.Value.Name == null)
                {
                    toBeRemovedList.Add(item.Value);
                }
            }
            foreach (var item in toBeRemovedList)
            {
                //Abnormalities.Remove(item.Id);
            }
        }
        static void DumpToTSV(string region)
        {
            List<string> fileLines = new List<string>();
            foreach (var item in Abnormalities)
            {
                fileLines.Add(item.Value.ToString());
            }
            File.WriteAllLines("abnormals-" + region + ".tsv", fileLines);
        }

        public static void LoadTSV()
        {
            var f = File.OpenText("abnormalities.tsv");
            Abnormalities = new Dictionary<uint, Abnormality>();
            while (true)
            {
                var line = f.ReadLine();
                if (line == null) break;

                var s = line.Split('\t');

                var id = Convert.ToUInt32(s[0]);
                Enum.TryParse(s[1], out AbnormalityType t);
                var isShow = bool.Parse(s[2]);
                var isBuff = bool.Parse(s[3]);
                var infinity = bool.Parse(s[4]);
                var name = s[5];
                var tooltip = s[6].Replace("&#xA;", "\n");
                var iconName = s[7];

                var ab = new Abnormality(id, isShow, isBuff, infinity, t);
                ab.SetIcon(iconName);
                ab.SetInfo(name, tooltip);

                Abnormalities.Add(id, ab);
            }

        }


    }
    public class Abnormality
    {
        public uint Id { get; set; }
        public AbnormalityType Type { get; set; }
        public bool IsShow { get; set; }
        public bool IsBuff { get; set; }
        public bool Infinity { get; set; }
        public string Name { get; set; }
        public string ToolTip { get; set; }
        public string IconName { get; set; }
        public List<Effect> Effects { get; set; }

        public Abnormality(uint id, bool isShow, bool isBuff, bool infinity, AbnormalityType prop)
        {
            Effects = new List<Effect>();
            Id = id;
            IsShow = isShow;
            IsBuff = isBuff;
            Infinity = infinity;
            Type = (AbnormalityType)prop;
        }
        void ReplaceValues()
        {
            if (Effects.Count < 1 || ToolTip == null)
            {
                return;
            }
            else
            {
                for (int i = 0; i < Effects.Count; i++)
                {
                    if (i == 0)
                    {
                        string val;
                        if (Effects[i].method == 3)
                        {
                            val = Math.Abs(((Effects[i].value * 100) - 100)).ToString() + "%";
                        }
                        else
                        {
                            val = Effects[i].value.ToString();
                        }
                        string pl = "";
                        if (Effects[i].tickInterval == 1)
                        {
                            pl = "";
                        }
                        else
                        {
                            pl = "1";
                        }
                        ToolTip = ToolTip.Replace("$value$", val + "$");
                        ToolTip = ToolTip.Replace("$tickInterval$COLOR_END", (Effects[i].tickInterval).ToString() + "$COLOR_END second" + pl);
                    }
                    else
                    {
                        string val;
                        if (Effects[i].method == 3)
                        {
                            val = Math.Abs(((Effects[i].value * 100) - 100)).ToString() + "%";
                        }
                        else
                        {
                            val = Effects[i].value.ToString();
                        }
                        int index = i + 1;
                        string pl = "";
                        if (Effects[i].tickInterval == 1)
                        {
                            pl = "";
                        }
                        else
                        {
                            pl = "1";
                        }
                        ToolTip = ToolTip.Replace("$value" + index + "$", val + "$");
                        ToolTip = ToolTip.Replace("$tickInterval" + index + "$COLOR_END", (Effects[i].tickInterval).ToString() + "$COLOR_END second" + pl);


                    }
                }
            }
        }
        public void SetIcon(string iconName)
        {
            this.IconName = iconName;
        }

        public void SetInfo(string name, string toolTip)
        {
            Name = name;
            ToolTip = toolTip.Replace("\n", "&#xA;").Replace("\r", "&#xD;");
        }

        public override string ToString()
        {
            ReplaceValues();
            return String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", Id, Type, IsShow, IsBuff, Infinity, Name, ToolTip, IconName);
        }
    }
    public class Effect
    {
        public double value;
        public int method;
        public int tickInterval;
        public Effect(double v, int m, int t)
        {
            value = v;
            method = m;
            tickInterval = t;
        }
    }
    public enum AbnormalityType
    {
        WeakeningEffect = 1,
        DamageOverTime = 2,
        Stun = 3,
        Buff = 4
    }


}
