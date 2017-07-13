using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraDatabaseConverter
{
    public class IconCopier
    {
        static string abnormalitiesDB = Environment.CurrentDirectory + "/abnormalities.tsv";
        static string skillDB = Environment.CurrentDirectory + "/skills.tsv";

        public static Dictionary<uint, Abnormality> Abnormalities;
        public static Dictionary<Class, Dictionary<uint, Skill>> Skills;

        public static void LoadAbnormalities()
        {
            var f = File.OpenText(abnormalitiesDB);
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
        public static void LoadSkills()
        {
            var f = File.OpenText(skillDB);

            Skills = new Dictionary<Class, Dictionary<uint, Skill>>();
            for (int i = 0; i <= 12; i++)
            {
                Skills.Add((Class)i, new Dictionary<uint, Skill>());
            }
            Skills.Add(Class.Common, new Dictionary<uint, Skill>());


            while (true)
            {
                var line = f.ReadLine();
                if (line == null) break;
                var s = line.Split('\t');
                var id = Convert.ToUInt32(s[0]);
                Enum.TryParse(s[1], out Class c);
                var name = s[2];
                var tooltip = s[3];
                var iconName = s[4];

                var sk = new Skill(id, c, name, tooltip);
                sk.SetSkillIcon(iconName);
                Skills[c].Add(id, sk);

            }

        }
        public static void Copy()
        {
            var paths = Directory.GetFiles(Environment.CurrentDirectory + "/icons").ToList();
            List<string> filenames = new List<string>();
            foreach (var item in paths)
            {
                filenames.Add(Path.GetFileName(item));
            }
            foreach (var pair in IconCopier.Abnormalities)
            {
                string iconName = pair.Value.IconName.Remove(0, pair.Value.IconName.IndexOf('.') + 1) + ".tga";
                if (filenames.Contains(iconName))
                {
                    string dirName = pair.Value.IconName.Remove(pair.Value.IconName.IndexOf('.'));
                    if (!Directory.Exists(Environment.CurrentDirectory + "/used_icons/" + dirName))
                    {
                        Directory.CreateDirectory(Environment.CurrentDirectory + "/used_icons/" + dirName);
                    }
                    File.Copy(Environment.CurrentDirectory + "/icons/" + iconName, Environment.CurrentDirectory + "/used_icons/" + pair.Value.IconName.Replace('.', '/') + ".tga", true);
                }
            }
            foreach (var dict in IconCopier.Skills)
            {
                foreach (var pair in dict.Value)
                {
                    string iconName = pair.Value.IconName.Remove(0, pair.Value.IconName.IndexOf('.') + 1) + ".tga";
                    if (filenames.Contains(iconName))
                    {
                        string dirName = pair.Value.IconName.Remove(pair.Value.IconName.IndexOf('.'));
                        if (!Directory.Exists(Environment.CurrentDirectory + "/used_icons/" + dirName))
                        {
                            Directory.CreateDirectory(Environment.CurrentDirectory + "/used_icons/" + dirName);
                        }
                        File.Copy(Environment.CurrentDirectory + "/icons/" + iconName, Environment.CurrentDirectory + "/used_icons/" + pair.Value.IconName.Replace('.', '/') + ".tga", true);
                    }
                }
            }

        }
        public static void CheckIcons()
        {

            StringBuilder sb = new StringBuilder();
            foreach (var skillDict in Skills)
            {

                foreach (var skill in skillDict.Value)
                {
                    if (!File.Exists(Environment.CurrentDirectory + "/used_icons/" + skill.Value.IconName.Replace('.','/') + ".png"))
                    {
                        Console.WriteLine("Missing skill icon: {0}", skill.Value.IconName.Replace('.', '/'));
                        sb.AppendLine(skill.Value.IconName.Replace('.', '/'));
                    }
                }

            }

            foreach (var abnormality in Abnormalities)
            {
                if(!File.Exists(Environment.CurrentDirectory + "/used_icons/" + abnormality.Value.IconName.Replace('.', '/') + ".png"))
                {
                    Console.WriteLine("Missing abnormality icon: {0}", abnormality.Value.IconName.Replace('.', '/'));
                    sb.AppendLine(abnormality.Value.IconName.Replace('.', '/'));
                }
            }
            File.WriteAllText("missing_files.txt", sb.ToString());
        }
    }
}