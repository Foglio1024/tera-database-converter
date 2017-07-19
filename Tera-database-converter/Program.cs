using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraDatabaseConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            //AbnormalitiesParser.Populate();
            //AbnormalitiesParser.DumpToTSV("abnormalities");

            SkillsParser.Populate();
            SkillsParser.DumpToTSV();

            //CheckIcons();

            //SystemMessagesParser.Parse();

            //ItemsParser.Parse();

            //GuildQuestsParser.Parse();

            //AccountBenefitParser.Parse();

            //AchievementsParser.Parse();

            //QuestsParser.Parse();

            //DungeonParser.Parse();

            //AchievementGradeParser.Parse();

            //RegionParser.Parse();
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
