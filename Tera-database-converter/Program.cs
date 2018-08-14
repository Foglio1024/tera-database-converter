using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TeraDatabaseConverter
{
    class Program
    {
        static void Main(string[] args)
        {


            //SkillsParser.Parse("EU-EN");

            //CheckIcons();

            //SystemMessagesParser.Parse("EU-EN");

            //ItemsParser.Parse("EU-EN");
            //ItemsParser.Parse("EU-FR");
            //ItemsParser.Parse("EU-GER");
            //ItemsParser.Parse("TW");
            //ItemsParser.Parse("JP");
            //ItemsParser.Parse("KR");
            //ItemsParser.Parse("RU");
            //ItemsParser.Parse("NA");

            //GuildQuestsParser.Parse("EU-EN");

            //AccountBenefitParser.Parse();

            //AchievementsParser.Parse("EU-EN");

            //QuestsParser.Parse("EU-EN");

            //DungeonParser.Parse("EU-EN");

            //AchievementGradeParser.Parse();

            //RegionParser.Parse("EU-EN");
            //CheckIcons();
            //Process.Start("explorer.exe", Environment.CurrentDirectory);
            //CopyItemIcons();

            new IconCopier2();
        }

        private static void CopyItemIcons()
        {
            var iconsPath = @"C:\Program Files (x86)\TERA\Client\S1Game\CookedPC\Art_Data\Packages\UmodelExport\";
            var localPath = Environment.CurrentDirectory + "/icons/";

            var f = File.ReadAllLines(@"D:\Repos\TCC\TCC.Core\resources\data\hotdot\hotdot-EU-EN.tsv");

            for (int i = 0; i < f.Length; i++)
            {
                var l = f[i];
                if (l != null)
                {
                    var s = l.Split('\t');
                    var cd = s[4];
                    if (cd == "0") continue;

                    var p = s[12].Split('.');
                    if (s[12] == "") continue;
                    var folderName = p[0];
                    var iconName = p[1];
                    Directory.CreateDirectory(localPath + folderName);
                    if (!File.Exists(iconsPath + folderName + @"\Texture2D\" + iconName + ".tga")) continue;
                    File.Copy(iconsPath + folderName + @"\Texture2D\" + iconName + ".tga", localPath + folderName + "/" + iconName + ".tga", true);
                }

            }

        }

        static void CheckIcons()
        {
            IconCopier.LoadAbnormalities();
            IconCopier.LoadSkills();
            IconCopier.Copy();
            IconCopier.CheckIcons();

        }
    }

    internal class IconCopier2
    {
        private const string DataPath = @"D:\Repos\TCC\TCC.Core\resources\data";

        private const string UmodelExportPath =
            @"C:\Program Files (x86)\TERA\Client\S1Game\CookedPC\Art_Data\Packages\UmodelExport";

        private string SkillsFile => Path.Combine(DataPath, "skills", "skills-EU-EN.tsv");
        private string ItemsFile => Path.Combine(DataPath, "items", "items-EU-EN.tsv");
        private string HotdotFile => Path.Combine(DataPath, "hotdot", "hotdot-EU-EN.tsv");

        private Dictionary<uint, string> Skills = new Dictionary<uint, string>();
        private Dictionary<uint, string> Items = new Dictionary<uint, string>();
        private Dictionary<uint, string> Abnormals = new Dictionary<uint, string>();

        private void LoadSkills()
        {
            var f = File.OpenText(SkillsFile);
            while (true)
            {
                var l = f.ReadLine();
                if (l == null) break;

                var split = l.Split('\t');
                var id = Convert.ToUInt32(split[0]);
                var iconName = split[7];

                if (Skills.ContainsKey(id)) Skills[id] = iconName;
                else Skills.Add(id, iconName);
            }
        }

        private void LoadItems()
        {
            var f = File.OpenText(ItemsFile);
            while (true)
            {
                var l = f.ReadLine();
                if (l == null) break;

                var split = l.Split('\t');

                var cd = split[4];
                if (cd == "0") continue;

                var id = Convert.ToUInt32(split[0]);
                var iconName = split[5];

                if (Items.ContainsKey(id)) Skills[id] = iconName;
                else Items.Add(id, iconName);
            }
        }

        private void LoadAbnormals()
        {
            var f = File.OpenText(HotdotFile);
            while (true)
            {
                var l = f.ReadLine();
                if (l == null) break;

                var split = l.Split('\t');
                var id = Convert.ToUInt32(split[0]);
                var iconName = split[12];

                if (Abnormals.ContainsKey(id)) Skills[id] = iconName;
                else Abnormals.Add(id, iconName);
            }
        }

        public IconCopier2()
        {
            LoadAbnormals();
            LoadItems();
            LoadSkills();

            CheckIconsInExport();
            CopyAll();
        }

        private void CopyAll()
        {
            Abnormals.ToList().ForEach(Copy);
            Skills.ToList().ForEach(Copy);
            Items.ToList().ForEach(Copy);
        }

        private void Copy(KeyValuePair<uint, string> x)
        {
            if (x.Value == "") return;
            var iconFolder = x.Value.Split('.')[0];
            if (iconFolder == "icon_ep") return;
            var iconName = x.Value.Split('.')[1] + ".tga";
            var tgaPath = Path.Combine(UmodelExportPath, iconFolder, "Texture2D", iconName);
            var pngPath = Path.Combine(Environment.CurrentDirectory, "used_icons",iconFolder,iconName.ToLower().Replace(".tga",".png"));
            using (var fs = new FileStream(tgaPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(fs))
            {
                var tga = new TgaLib.TgaImage(reader);
                var png = new PNGCompression.PNGCompressor();
                var bmp = tga.GetBitmap();
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "used_icons", iconFolder)))
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "used_icons", iconFolder));
                using (var fileStream = new FileStream(pngPath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(fileStream);
                    Console.WriteLine($"Saved {pngPath}");
                }
            }


        }

        private int _found = 0;
        private int _notFound = 0;

        private void CheckIcon(KeyValuePair<uint, string> x)
        {
            if (x.Value == "") return;
            var iconFolder = x.Value.Split('.')[0];
            if (iconFolder == "icon_ep") return;
            var iconName = x.Value.Split('.')[1] + ".tga";
            var fullPath = Path.Combine(UmodelExportPath, iconFolder, "Texture2D", iconName);
            if (!File.Exists(fullPath)) _notFound++;
            else _found++;
        }

        private void CheckIconsInExport()
        {
            Abnormals.ToList().ForEach(CheckIcon);
            Skills.ToList().ForEach(CheckIcon);
            Items.ToList().ForEach(CheckIcon);

            Console.WriteLine($"Found: {_found}");
            Console.WriteLine($"Not found: {_notFound}");
        }
    }
}
