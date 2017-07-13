using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class GuildQuestsParser
    {
        private static XDocument Data;
        private static XDocument StrSheet;
        private static XDocument BattleFieldData;
        private static XDocument StrSheet_BattleField;
        private static XDocument StrSheet_ZoneName;
        private static Dictionary<uint, GuildQuest> GuildQuests;

        private static void Load()
        {
            Data = XDocument.Load(Utilities.DATABASE_PATH + "/GuildQuest.xml");
            StrSheet = XDocument.Load(Utilities.DATABASE_PATH + "/StrSheet_GuildQuest.xml");
            BattleFieldData = XDocument.Load(Utilities.DATABASE_PATH + "/BattleFieldData.xml");
            StrSheet_BattleField = XDocument.Load(Utilities.DATABASE_PATH + "/StrSheet_BattleField.xml");
            StrSheet_ZoneName = XDocument.Load(Utilities.DATABASE_PATH + "/StrSheet_ZoneName.xml");
        }
        private static void ParseDataDoc()
        {
            foreach (var questElement in Data.Descendants().Where(x => x.Name == "Quest"))
            {
                var id = uint.Parse(questElement.Attribute("id").Value);
                var type = id / 1000;
                if (type == 2)
                {
                    //bg
                    var stringId = uint.Parse(questElement.Attribute("title").Value.Replace("@GuildQuest:", ""));
                    uint battleFieldId = uint.Parse(questElement.Descendants().FirstOrDefault(x => x.Name == "Field").Attribute("battleFieldId").Value);
                    var bgNameId = BattleFieldData.Descendants().FirstOrDefault(x => x.Name == "BattleField" && x.Attribute("id").Value == battleFieldId.ToString()).Attribute("name").Value;
                    var bgName = StrSheet_BattleField.Descendants().FirstOrDefault(x => x.Name == "String" && x.Attribute("id").Value == bgNameId).Attribute("string").Value;
                    GuildQuests[stringId].Title = GuildQuests[stringId].Title.Replace(@"{BattleField1}", bgName);
                }
                else if (type == 3)
                {
                    //gather
                }
                else if (type == 4 || type == 5)
                {
                    //rally
                }
                else if (type >= 11)
                {
                    //dg
                    var stringId = uint.Parse(questElement.Attribute("title").Value.Replace("@GuildQuest:", ""));
                    uint zoneId = uint.Parse(questElement.Descendants().FirstOrDefault(x => x.Name == "Npc").Attribute("huntingZoneId").Value);
                    var zoneName = StrSheet_ZoneName.Descendants().FirstOrDefault(x => x.Name == "String" && x.Attribute("id").Value == zoneId.ToString()).Attribute("string").Value;
                    GuildQuests[stringId].Title = GuildQuests[stringId].Title.Replace(@"{HuntingZone1}", zoneName);
                }

            }
        }
        private static void ParseStrSheetDoc()
        {
            foreach (var stringElement in StrSheet.Descendants().Where(x => x.Name == "String"))
            {
                var id = uint.Parse(stringElement.Attribute("id").Value);
                var str = stringElement.Attribute("string").Value;

                GuildQuests.Add(id, new GuildQuest(id, str));
            }
        }
        private static void Dump()
        {
            List<string> lines = new List<string>();
            foreach (var quest in GuildQuests)
            {
                if (quest.Key % 2 != 1) continue;
                if (quest.Value.Title.Contains("{")) continue;
                //if (quest.Value.ZoneId == 0) continue;
                var sb = new StringBuilder();
                sb.Append(quest.Value.Id);
                sb.Append('\t');
                sb.Append(quest.Value.Title);
                //sb.Append('\t');
                //sb.Append(quest.Value.ZoneId);
                lines.Add(sb.ToString());
            }
            File.WriteAllLines("guild-quests.tsv", lines);
        }
        public static void Parse()
        {
            GuildQuests = new Dictionary<uint, GuildQuest>();
            Load();
            ParseStrSheetDoc();
            ParseDataDoc();
            Dump();
        }
    }

    public class GuildQuest
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public uint ZoneId { get; set; }
        public uint BattleFieldId { get; set; }

        public GuildQuest(uint id, string s)
        {
            Id = id;
            Title = s;
        }
    }
}
