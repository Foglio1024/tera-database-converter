using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class SkillsParser
    {
        public static Dictionary<Class, Dictionary<uint, Skill>> Skills;
        static List<XDocument> StrSheet_UserSkillsDocs;
        static List<XDocument> SkillIconData;
        static List<XDocument> SkillDataDocs;
        static XDocument ConnectedSkillsDoc;
        static List<SkillConnection> SkillConnections;

        static void LoadFiles(string region)
        {
            int j = 1;
            int n = Directory.EnumerateFiles(Utilities.DATABASE_PATH +region+ @"/StrSheet_UserSkill").Count();
            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + region + @"/StrSheet_UserSkill"))
            {
                var d = XDocument.Load(f);
                StrSheet_UserSkillsDocs.Add(d);
                Console.Write("\rLoaded StrSheet {0}/{1}", j, n);
                j++;
            }

            j = 1;
            n = Directory.EnumerateFiles(Utilities.DATABASE_PATH+region + @"/SkillIconData").Count();
            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + region + @"/SkillIconData"))
            {
                var d = XDocument.Load(f);
                SkillIconData.Add(d);
                Console.Write("\rLoaded IconDoc {0}/{1}", j, n);
                j++;

            }

            j = 1;
            n = Directory.EnumerateFiles(Utilities.DATABASE_PATH +region+ @"/SkillData").Count();
            foreach (var f in Directory.EnumerateFiles(Utilities.DATABASE_PATH + region + "/SkillData"))
            {
                var d = XDocument.Load(f);
                SkillDataDocs.Add(d);
                Console.Write("\rLoaded SkillDataDoc {0}/{1}", j, n);
                j++;

            }

        }
        static void ParseUserSkillDoc(XDocument doc)
        {
            foreach (var s in doc.Descendants().Where(x => x.Name == "String"))
            {
                var id = Convert.ToUInt32(s.Attribute("id").Value);
                string name = string.Empty;
                if (s.Attribute("name") != null)
                {
                    name = s.Attribute("name").Value;
                }
                Enum.TryParse(s.Attribute("class").Value, out Class c);
                string toolTip = string.Empty;

                if (s.Attribute("toolTip") != null)
                {
                    toolTip = s.Attribute("toolTip").Value;
                }
                if ((s.Attribute("class").Value != "Common") && (!name.Contains("Summon: ") || name == "Summon: Party"))
                {
                    //var skill = new Skill(id, c, name, toolTip);
                    try
                    {
                        Skills[c][id].Name = name;
                        Skills[c][id].ToolTip = toolTip;
                    }
                    catch(Exception)
                    {
                        Skills[c].Add(id, new Skill(id, c, name, toolTip));
                    }
                }
            }
        }
        static void ParseSkillIconDoc(XDocument doc)
        {
            foreach (var s in doc.Descendants().Where(x => x.Name == "Icon"))
            {
                var id = Convert.ToUInt32(s.Attribute("skillId").Value);
                var iconName = s.Attribute("iconName").Value;
                Enum.TryParse(s.Attribute("class").Value, out Class c);
                if (Skills[c].TryGetValue(id, out Skill sk))
                {
                    sk.SetSkillIcon(iconName.ToLower());
                }
            }
        }
        static void ParseSkillData(XDocument doc)
        {
            foreach (var skillElement in doc.Descendants().Where(x => x.Name == "Skill"))
            {
                var id = Convert.ToUInt32(skillElement.Attribute("id").Value);
                Class c;
                try
                {
                    Enum.TryParse(skillElement.Attribute("name").Value.Split('_')[2], out c);
                }
                catch (Exception)
                {
                    continue;
                }

                List<uint> connectedSkills = new List<uint>();
                if(skillElement.Descendants().Any(x => x.Name == "ConnectSkill"))
                {
                    foreach (var addConnectSkillElement in skillElement.Descendants().Where(x => x.Name == "ConnectSkill").Descendants())
                    {
                        connectedSkills.Add(Convert.ToUInt32(addConnectSkillElement.Attribute("redirectSkill").Value));
                    }
                }
                if (skillElement.Descendants().Any(x => x.Name == "OverChargeConnectSkill"))
                {
                    foreach (var addConnectSkillElement in skillElement.Descendants().Where(x => x.Name == "OverChargeConnectSkill"))
                    {
                        connectedSkills.Add(Convert.ToUInt32(addConnectSkillElement.Attribute("redirectSkill").Value));
                    }
                }
                if (skillElement.Descendants().Any(x => x.Name == "AddAbnormalityConnectSkill"))
                {
                    foreach (var addConnectSkillElement in skillElement.Descendants().Where(x => x.Name == "AddAbnormalityConnectSkill"))
                    {
                        connectedSkills.Add(Convert.ToUInt32(addConnectSkillElement.Attribute("redirectSkill").Value));
                    }
                }
                if(!Skills.ContainsKey(c)) continue;
                Skill sk = new Skill(id, c);
                foreach (var item in connectedSkills)
                {
                    sk.AddConnectedSkill(item);
                }
                
                if (!Skills[c].ContainsKey(id))
                {
                    if(Skills[c].ContainsKey(id - id % 100))
                    {
                        Skills[c][id - id % 100].AddConnectedSkill(id);
                    }
                    else Skills[c].Add(id, sk);
                }
            }
        }

                
        static void Populate(string region)
        {
            StrSheet_UserSkillsDocs = new List<XDocument>();
            SkillIconData = new List<XDocument>();
            SkillDataDocs = new List<XDocument>();
            //init dictionaries
            Skills = new Dictionary<Class, Dictionary<uint, Skill>>();
            for (int i = 0; i <= 12; i++)
            {
                Skills.Add((Class)i, new Dictionary<uint, Skill>());
            }
            Skills.Add(Class.Common, new Dictionary<uint, Skill>());

            LoadFiles(region);
            Console.WriteLine("\nDone loading");

            //parse
            int j = 1;
            foreach (var doc in SkillDataDocs)
            {
                ParseSkillData(doc);
                Console.Write("\rParsed data doc {0}/{1}",j,SkillDataDocs.Count);
                j++;
            }
            Console.WriteLine("\nSkillData parsed");
            j = 1;
            foreach (var doc in StrSheet_UserSkillsDocs)
            {
                ParseUserSkillDoc(doc);
                Console.Write("\rParsed string sheet {0}/{1}", j, StrSheet_UserSkillsDocs.Count);
                j++;
            }
            Console.WriteLine("\nStrSheet parsed");
            j = 0;
            foreach (var doc in SkillIconData)
            {
                ParseSkillIconDoc(doc);
                Console.Write("\rParsed icon sheet {0}/{1}", j, SkillIconData.Count);
                j++;
            }

            //add hurricane
            var s = new Skill(60010, Class.Common, "Hurricane", "");
            s.SetSkillIcon("icon_skills.armorbreak_tex");
            Skills[Class.Common].Add(s.Id, s);

            //check if connected skills are in the main list and remove them
            foreach (var dict in Skills.Values)
            {
                List<uint> remove = new List<uint>();

                foreach (var skill in dict.Values)
                {
                    skill.ConnectedSkills = skill.ConnectedSkills.Distinct().ToList();
                    if (dict.Any(x => x.Value.ConnectedSkills.Contains(skill.Id)))
                    {
                        remove.Add(skill.Id);
                    }
                }

                foreach (var skillToRemove in remove)
                {
                    dict.Remove(skillToRemove);
                }
            }

        }

        static void DumpToTSV()
        {
            List<string> fileLines = new List<string>();
            foreach (var classDictPair in Skills)
            {
                foreach (var idSkillPair in classDictPair.Value)
                {
                    if (idSkillPair.Value.Name != null)
                    {
                        fileLines.Add(idSkillPair.Value.ToString());
                    }
                }
            }
            File.WriteAllLines(Environment.CurrentDirectory + "/skills.tsv", fileLines);

        }

        public static void Parse(string region)
        {
            Populate(region);
            DumpToTSV();
        }

        class SkillConnection
        {
            public Class Class;
            public int Id;
            public List<int> ConnectedSkills;

            public SkillConnection(int id, Class c)
            {
                ConnectedSkills = new List<int>();
                Id = id;
                Class = c;
            }
            public void AddConnectedSkill(int id)
            {
                ConnectedSkills.Add(id);
            }
        }
    }
    public class Skill
    {
        //Bitmap iconBitmap;
        public uint Id { get; set; }
        public Class Class { get; set; }
        public string Name { get; set; }
        public string ToolTip { get; set; }
        public string IconName { get; set; }
        public List<uint> ConnectedSkills;


        public Skill(uint id, Class c, string name, string toolTip)
        {
            Id = id;
            Class = c;
            Name = name;
            ToolTip = toolTip;
            ConnectedSkills = new List<uint>();
        }
        public Skill(uint id, Class c)
        {
            Id = id;
            Class = c;
            ConnectedSkills = new List<uint>();
        }
        public void AddConnectedSkill(uint id)
        {
            ConnectedSkills.Add(id);
        }

        public void SetSkillIcon(string iconName)
        {
            IconName = iconName;
        }
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(String.Format("{0}\t{1}\t{2}\t{3}\t{4}", Id, Class, Name, ToolTip, IconName));
            foreach (var item in ConnectedSkills)
            {
                s.Append(String.Format("\t{0}", item));
            }
            return s.ToString();
        }
    }
    public enum Class
    {
        Warrior = 0,
        Lancer = 1,
        Slayer = 2,
        Berserker = 3,
        Sorcerer = 4,
        Archer = 5,
        Priest = 6,
        Mystic = 7,
        Reaper = 8,
        Gunner = 9,
        Brawler = 10,
        Ninja = 11,
        Valkyrie = 12,
        Common = 255,
        None = 256
    }


}
