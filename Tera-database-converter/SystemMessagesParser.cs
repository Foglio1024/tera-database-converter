using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraDatabaseConverter
{
    public static class SystemMessagesParser
    {
        static XDocument SystemMessages;
        static List<SysMessage> Messages;
        static void Load()
        {
            SystemMessages = XDocument.Load(Utilities.DATABASE_PATH + "/StrSheet_SystemMessage.xml");
        }
        static void ParseFile()
        {
            foreach (var str in SystemMessages.Descendants().Where(x => x.Name == "String"))
            {
                if (str.Attribute("string") == null) continue;

                // Parse message
                var s = str.Attribute("string").Value;
                var id = str.Attribute("readableId").Value;
                var ch = str.Attribute("chatChannel") != null ? Int32.Parse(str.Attribute("chatChannel").Value) : 302;
                var d = str.Attribute("displayChat") != null ? bool.Parse(str.Attribute("displayChat").Value) : false;

                // Manual fixes
                s = CaseOverride(s);
                if (id == "SMT_GQUEST_URGENT_APPEAR")
                {
                    s = AppearMsgOverride(s);
                }
                if (id == "SMT_FIELDNAMED_DIE")
                {
                    s = RallyDieOverride(s);
                }
                if (id == "SMT_FIELDNAMED_RANK")
                {
                    s = "<font color='#1DDB16'>" + s;
                }
                if (id == "SMT_GUILD_WAR_WITHDRAW_GUILDMONEY")
                {
                    s = s.Replace("</font></font>", "</font>");
                }

                // Check <font/> tags consistency
                if (s != "" && id != "SMT_BOOSTERENCHANT_GUIDE" && id != "SMT_CITYWAR_DEAD_MESSAGE")
                {
                    var x = Regex.Escape(s);
                    var sCount = Regex.Matches(x, "<font").Count;
                    var eCount = Regex.Matches(x, "/font>").Count;
                    if (eCount != sCount)
                    {
                        Console.WriteLine(id);
                    }
                }

                // Add message
                var sysMsg = new SysMessage(s, id, ch, d);
                Messages.Add(sysMsg);
            }
        }

        private static string RallyDieOverride(string s)
        {
            return s.Replace("npcname", "npcName");
        }

        private static string CaseOverride(string s)
        {
            s = s.Replace("NPCNAME", "npcName");
            s = s.Replace("QUESTNAME", "questName");
            s = s.Replace("ZONENAME", "zoneName");

            return s;
        }
        private static string AppearMsgOverride(string s)
        {
            return s.Replace("appeared", " appeared");
        }
        static void Dump()
        {
            List<string> lines = new List<string>();
            foreach (var item in Messages)
            {
                if (!item.displayChat) continue;
                if (item.readableId == item.Str) continue;
                var sb = new StringBuilder();
                sb.Append(item.chatChannel);
                sb.Append('\t');
                sb.Append(item.readableId);
                sb.Append('\t');
                //sb.Append(item.displayChat);
                //sb.Append('\t');
                sb.Append(item.Str.Replace("\n", "&#xA;"));

                var line = sb.ToString();
                lines.Add(line);
            }
            File.WriteAllLines("sys-messages.tsv", lines);
        }
        public static void Parse()
        {
            Messages = new List<SysMessage>();
            Load();
            ParseFile();
            Dump();
        }
        struct SysMessage
        {
            public string Str;
            public string readableId;
            public int chatChannel;
            public bool displayChat;

            public SysMessage(string s, string id, int ch, bool d)
            {
                Str = s;
                readableId = id;
                chatChannel = ch;
                displayChat = d;
            }
        }
    }
}
