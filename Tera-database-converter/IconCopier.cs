using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace TeraDatabaseConverter
{

    public class IconCopier
    {
        static string abnormalitiesDB = @"D:\Repos\TCC\TCC.Core\resources\data\hotdot\hotdot-EU-EN.tsv";
        static string skillDB = @"D:\Repos\TCC\TCC.Core\resources\data\skills\skills-EU-EN.tsv";
        static List<Tuple<string, string>> filenames = new List<Tuple<string, string>>();

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
                if (Abnormalities.ContainsKey(id)) continue;
                //Enum.TryParse(s[1], out AbnormalityType t);
                //var isShow = bool.Parse(s[2]);
                //var isBuff = bool.Parse(s[3]);
                //var infinity = bool.Parse(s[4]);
                var name = s[5];
                //var tooltip = s[6].Replace("&#xA;", "\n");
                var iconName = s[13];

                var ab = new Abnormality(id, true, true, true, AbnormalityType.Buff);
                ab.SetIcon(iconName);
                ab.SetInfo(name, "");

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
                Enum.TryParse(s[3], out Class c);
                if(Skills[c].ContainsKey((id))) continue;
                
                var name = s[4];
                var tooltip ="";
                var iconName = s[7];

                var sk = new Skill(id, c, name, tooltip);
                sk.SetSkillIcon(iconName);
                Skills[c].Add(id, sk);

            }

        }
        public static void Copy()
        {
            var paths = Directory.GetDirectories(Utilities.ICONS_PATH).ToList();
            foreach (var item in paths)
            {
                var sp = Directory.GetFiles(Path.Combine(Utilities.ICONS_PATH,item, "Texture2D"));
                foreach (var i in sp)
                {
                    var cat = Path.GetFileName(item);
                    var fn = i.Replace(item, "").Replace("\\Texture2D\\", "");
                    filenames.Add(new Tuple<string, string>(cat, fn));
                    Console.Write($"\rAdded {cat}.{fn}");
                }
            }

            int c = 0;
            foreach (var pair in Abnormalities)
            {
                string iconName = pair.Value.IconName.Remove(0, pair.Value.IconName.IndexOf('.') + 1) + ".tga";
                var tuple = filenames.FirstOrDefault(x => string.Equals(x.Item2, iconName, StringComparison.CurrentCultureIgnoreCase));
                if (tuple != null)
                {
                    string dirName = pair.Value.IconName.Remove(pair.Value.IconName.IndexOf('.'));
                    if (!Directory.Exists(Environment.CurrentDirectory + "/used_icons/" + dirName))
                    {
                        Directory.CreateDirectory(Environment.CurrentDirectory + "/used_icons/" + dirName);
                    }
                    var tgaPath = Path.Combine(Utilities.ICONS_PATH, tuple.Item1, "Texture2D", iconName);
                    var pngPath = Environment.CurrentDirectory + "/used_icons/" + pair.Value.IconName.Replace('.', '/').ToLower() + ".png";
                    using (var fs = new FileStream(tgaPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new BinaryReader(fs))
                    {
                        var tga = new TgaLib.TgaImage(reader);
                        var png = new PNGCompression.PNGCompressor();
                        var bmp = tga.GetBitmap();
                        using (var fileStream = new FileStream(pngPath, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bmp));
                            encoder.Save(fileStream);
                            Console.WriteLine($"[{c}]Saved {pngPath}");
                            c++;
                        }
                    }

                    //File.Copy(Path.Combine(Utilities.ICONS_PATH, tuple.Item1, "Texture2D", iconName), Environment.CurrentDirectory + "/used_icons/" + pair.Value.IconName.Replace('.', '/').ToLower() + ".tga", true);
                }
            }
            foreach (var dict in Skills)
            {
                foreach (var pair in dict.Value)
                {
                    c = 0;
                    string iconName = pair.Value.IconName.Remove(0, pair.Value.IconName.IndexOf('.') + 1) + ".tga";
                    var tuple = filenames.FirstOrDefault(x => x.Item2 == iconName);
                    if (tuple != null)
                    {
                        string dirName = pair.Value.IconName.Remove(pair.Value.IconName.IndexOf('.'));
                        if (!Directory.Exists(Environment.CurrentDirectory + "/used_icons/" + dirName))
                        {
                            Directory.CreateDirectory(Environment.CurrentDirectory + "/used_icons/" + dirName);
                        }
                        var tgaPath = Path.Combine(Utilities.ICONS_PATH, tuple.Item1, "Texture2D", iconName);
                        var pngPath = Environment.CurrentDirectory + "/used_icons/" + pair.Value.IconName.Replace('.', '/').ToLower() + ".png";
                        using (var fs = new FileStream(tgaPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new BinaryReader(fs))
                        {
                            var tga = new TgaLib.TgaImage(reader);
                            var png = new PNGCompression.PNGCompressor();
                            var bmp = tga.GetBitmap();
                            using (var fileStream = new FileStream(pngPath, FileMode.Create))
                            {
                                BitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(bmp));
                                encoder.Save(fileStream);
                                Console.WriteLine($"[{c}]Saved {pngPath}");
                                c++;
                            }
                        }

                        //File.Copy(Path.Combine(Utilities.ICONS_PATH, tuple.Item1 ,"Texture2D", iconName), Environment.CurrentDirectory + "/used_icons/" + pair.Value.IconName.Replace('.', '/').ToLower() + ".tga", true);
                    }
                }
            }

        }
        public static void CheckIcons()
        {
            var files = new List<string>();
            foreach (var skillDict in Skills)
            {

                foreach (var skill in skillDict.Value)
                {
                    if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "used_icons", skill.Value.IconName.ToLower().Replace('.','\\')+ ".png")))
                    {
                        Console.WriteLine("Missing skill icon: {0}", skill.Value.IconName.Replace('.', '/'));
                        var f = skill.Value.IconName.Replace('.', '/');
                        if(!string.IsNullOrEmpty(f) && files.All(x => x != f)) files.Add(f);
                    }
                }

            }

            foreach (var abnormality in Abnormalities)
            {
                if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "used_icons", abnormality.Value.IconName.Replace('.', '\\') + ".png")))
                {
                    Console.WriteLine("Missing abnormality icon: {0}", abnormality.Value.IconName.Replace('.', '/'));
                    var f = abnormality.Value.IconName.Replace('.', '/');
                    if (!string.IsNullOrEmpty(f) && files.All(x => x != f)) files.Add(f);
                }
            }
            File.WriteAllLines("missing_files.txt", files);
        }
    }
}