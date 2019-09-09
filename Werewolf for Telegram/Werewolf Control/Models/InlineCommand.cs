using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Database;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Werewolf_Control.Handler;
using Werewolf_Control.Helpers;
using Newtonsoft.Json;

namespace Werewolf_Control.Models
{
    public class InlineCommand
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }

        public InlineCommand(string command, string desc)
        {
            Command = command;
            Description = desc;
        }

        public InlineCommand()
        {
            
        }
    }

    public class StatsInlineCommand : InlineCommand
    {
        public StatsInlineCommand(User u)
        {

            Description = "Get personal stats";
            Command = "stats";
            try
            {
                using (var db = new WWContext())
                {
                    Content = "";
                    //find the player
                    var p = db.Players.FirstOrDefault(x => x.TelegramId == u.Id);
                    if (p == null)
                    {
                        //remove the command
                        Command = "";
                        return;
                    }

                    // var topPlayers = db.Players.OrderByDescending(x => x.Score);
                    var gamesPlayed = p.GamePlayers.Count();
                    var won = p.GamePlayers.Count(x => x.Won);
                    var lost = gamesPlayed - won;
                    var survived = p.GamePlayers.Count(x => x.Survived);
                    var roleInfo = db.PlayerRoles(u.Id).ToList();
                    var killed = db.PlayerMostKilled(u.Id).FirstOrDefault();
                    var killedby = db.PlayerMostKilledBy(u.Id).FirstOrDefault();
                    var ach = p.Achievements == null ? new System.Collections.BitArray(200) : new System.Collections.BitArray(p.Achievements);
                    var score = p.Score;
                    var rankEmoji = "";

                    if (score > 2200)
                        rankEmoji = " 🏆";
                    else if (score > 2000)
                        rankEmoji = " 💎";
                    else if (score > 1800)
                        rankEmoji = " 🥇";
                    else if (score > 1600)
                        rankEmoji = " 🥈";
                    else if (score > 1400)
                        rankEmoji = " 🥉";
                    else if (score > 1200)
                        rankEmoji = " 🔆";
                    else if (score > 1000)
                        rankEmoji = " 🔅";

                    var count = ach.GetUniqueFlags().Count();

                    Content = $"<a href='tg://user?id={p.TelegramId}'>{p.Name.FormatHTML()}, {GetLocaleString(roleInfo.OrderByDescending(x => x.times).FirstOrDefault(x => x.role != "Villager" && x.role != "Mason" && x.role != "Police")?.role, "Spanish.xml") ?? "Noob"}</a>";
                    Content += $"\n{count.Pad()}Logros desbloqueados\n" +
                               $"{score.Pad()}Puntos{rankEmoji}\n" +
                               $"{won.Pad()}Partidas ganadas ({won*100/gamesPlayed}%)\n" +
                               $"{lost.Pad()}Partidas perdidas ({lost*100/gamesPlayed}%)\n" +
                               $"{survived.Pad()}Partidas sobrevividas ({survived*100/gamesPlayed}%)\n" +
                               $"{gamesPlayed.Pad()}Partidas totales\n" +
                               $"<code>{killed?.times}</code>\tveces he matado a {killed?.Name.FormatHTML()}\n" +
                               $"<code>{killedby?.times}</code>\tveces me ha matado {killedby?.Name.FormatHTML()}\n";

                    var json = p.CustomGifSet;
                    if (!String.IsNullOrEmpty(json))
                    {
                        var data = JsonConvert.DeserializeObject<CustomGifData>(json);
                        if (data.ShowBadge)
                        {
                            if ((p.DonationLevel ?? 0) >= 100)
                                Content += "Donation Level: 🥇";
                            else if ((p.DonationLevel ?? 0) >= 50)
                                Content += "Donation Level: 🥈";
                            else if ((p.DonationLevel ?? 0) >= 10)
                                Content += "Donation Level: 🥉";
                            if (p.Founder ?? false)
                                Content += "\n💎 FOUNDER STATUS! 💎\n<i>(This player donated at least $10USD before there was any reward for donating)</i>";
                        }
                    }
                    else
                    {
                        if ((p.DonationLevel ?? 0) >= 100)
                            Content += "Donation Level: 🥇";
                        else if ((p.DonationLevel ?? 0) >= 50)
                            Content += "Donation Level: 🥈";
                        else if ((p.DonationLevel ?? 0) >= 10)
                            Content += "Donation Level: 🥉";

                        if (p.Founder ?? false)
                            Content += "\n💎 FOUNDER STATUS! 💎\n<i>(This player donated at least $10USD before there was any reward for donating</i>";
                    }

                }
            }
            catch (Exception e)
            {
                Content = "Unable to load stats: " + e.Message;
            }
        }

        private static string GetLocaleString(string key, string language, params object[] args)
        {
            try
            {
                var files = Directory.GetFiles(Bot.LanguageDirectory);
                XDocument doc;
                var file = files.First(x => Path.GetFileNameWithoutExtension(x) == language);
                {
                    doc = XDocument.Load(file);
                }
                var strings = doc.Descendants("string").FirstOrDefault(x => x.Attribute("key").Value == key) ??
                    Bot.English.Descendants("string").FirstOrDefault(x => x.Attribute("key").Value == key);
                var values = strings.Descendants("value");
                var choice = Bot.R.Next(values.Count());
                var selected = values.ElementAt(choice);
                return String.Format(selected.Value.FormatHTML(), args).Replace("\\n", Environment.NewLine);
            }
            catch
            {
                var strings = Bot.English.Descendants("string").FirstOrDefault(x => x.Attribute("key").Value == key);
                var values = strings.Descendants("value");
                var choice = Bot.R.Next(values.Count());
                var selected = values.ElementAt(choice);
                return String.Format(selected.Value.FormatHTML(), args).Replace("\\n", Environment.NewLine);
            }
        }
    }

}
