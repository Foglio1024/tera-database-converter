using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace TeraDatabaseConverter
{
    class Program
    {
        static void Main(string[] args)
        {

            //AbnormalitiesParser.Parse("EU-EN");
            //AbnormalitiesParser.Parse("EU-FR");
            //AbnormalitiesParser.Parse("EU-GER");
            //AbnormalitiesParser.Parse("TW");
            //AbnormalitiesParser.Parse("JP");
            //AbnormalitiesParser.Parse("NA");
            //AbnormalitiesParser.Parse("RU");
            //AbnormalitiesParser.Parse("KR");

            //SkillsParser.Parse("EU-EN");

            ////CheckIcons();

            //SystemMessagesParser.Parse("EU-EN");

            ItemsParser.Parse("EU-EN");
            ItemsParser.Parse("EU-FR");
            ItemsParser.Parse("EU-GER");
            ItemsParser.Parse("TW");
            ItemsParser.Parse("JP");
            //ItemsParser.Parse("KR");
            ItemsParser.Parse("RU");
            ItemsParser.Parse("NA");

            //GuildQuestsParser.Parse("EU-EN");

            //AccountBenefitParser.Parse();

            //AchievementsParser.Parse("EU-EN");

            //QuestsParser.Parse("EU-EN");

            //DungeonParser.Parse("EU-EN");

            //AchievementGradeParser.Parse();

            //RegionParser.Parse("EU-EN");
            //CheckIcons();
            Process.Start("explorer.exe", Environment.CurrentDirectory);
            //CopyItemIcons();
        }

        private static void CopyItemIcons()
        {
            var iconsPath = @"I:\TERA\Client\S1Game\CookedPC\Art_Data\Packages\UmodelExport\";
            var localPath = Environment.CurrentDirectory + "/icons/";

            var f = File.ReadAllLines("items-EU-EN.tsv");
            
            for (int i = 0; i < f.Length; i++)
            {
                var l = f[i];
                if (l != null)
                {
                    var s = l.Split('\t');
                    var cd = s[4];
                    if(cd == "0") continue;

                    var p = s[5].Split('.');
                    var folderName = p[0];
                    var iconName = p[1];
                    Directory.CreateDirectory(localPath + folderName);
                    File.Copy(iconsPath +folderName+@"\Texture2D\" + iconName + ".tga", localPath+folderName+"/"+iconName+".tga", true);
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
}
