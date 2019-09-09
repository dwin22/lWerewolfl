using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Database;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Werewolf_Control.Attributes;
using Werewolf_Control.Helpers;
using System.Threading;
using Telegram.Bot.Types.InlineKeyboardButtons;

namespace Werewolf_Control
{
    public static partial class Commands
    {
        [Command(Trigger = "grouplist")]
        public static void GroupList(Update update, string[] args)
        {
            
            //var reply = "";
            //using (var db = new WWContext())
            //{
            //    reply = Enumerable.Aggregate(db.v_PreferredGroups, "", (current, g) => current + $"{GetLanguageName(g.Language)}{(String.IsNullOrEmpty(g.Description) ? "" : $" - {g.Description}")}\n<a href=\"{g.GroupLink}\">{g.Name}</a>\n\n");
            //}
            //try
            //{
            //    var result = Bot.Api.SendTextMessageAsync(update.Message.From.Id, reply, parseMode: ParseMode.Html, disableWebPagePreview: true).Result;
            //    if (update.Message.Chat.Type != ChatType.Private)
            //        Send(GetLocaleString("SentPrivate", GetLanguage(update.Message.From.Id)), update.Message.Chat.Id);
            //}
            //catch (Exception e)
            //{
            //    Send(GetLocaleString("StartPM", GetLanguage(update.Message.Chat.Id)), update.Message.Chat.Id);
            //}

            //new method, fun times....
            //var groups = PublicGroups.GetAll();
            //now determine what languages are available in public groups.
            try
            {
                string[] disabledLangs = new string[] { "فارسی" }; // Language bases of which no grouplist is accessible
                var langs = PublicGroups.GetBaseLanguages().Where(x => !disabledLangs.Contains(x)); // do not fetch disabled langs
                //create a menu out of this
                List<InlineKeyboardCallbackButton> buttons = langs.OrderBy(x => x).Select(x => new InlineKeyboardCallbackButton(x, $"groups|{update.Message.From.Id}|{x}|null")).ToList();

                var baseMenu = new List<InlineKeyboardButton[]>();
                for (var i = 0; i < buttons.Count; i++)
                {
                    if (buttons.Count - 1 == i)
                    {
                        baseMenu.Add(new[] { buttons[i] });
                    }
                    else
                        baseMenu.Add(new[] { buttons[i], buttons[i + 1] });
                    i++;
                }

                var menu = new InlineKeyboardMarkup(baseMenu.ToArray());

                try
                {
                    var result = Bot.Api.SendTextMessageAsync(update.Message.From.Id,
                        GetLocaleString("WhatLangGroup", GetLanguage(update.Message.From.Id)),
                        replyMarkup: menu).Result;
                    if (update.Message.Chat.Type != ChatType.Private)
                        Send(GetLocaleString("SentPrivate", GetLanguage(update.Message.From.Id)), update.Message.Chat.Id);
                }
                catch
                {
                    RequestPM(update.Message.Chat.Id);
                }
            }
            catch (Exception e)
            {
                Send(e.Message + Environment.NewLine + Environment.NewLine + e.StackTrace, 133748469);
            }
        }

        [Command(Trigger = "rolelist")]
        public static void RoleList(Update update, string[] args)
        {
            var lang = GetLanguage(update.Message.Chat.Id);
            // var reply =
            //    "/AboutVG - Villager\n/AboutSeer - Seer\n/AboutWw - Werewolf\n/AboutHarlot - Harlot\n/AboutDrunk - Drunk\n/AboutCursed - Cursed\n/AboutTraitor - Traitor\n/AboutGA - Guardian Angel\n/AboutDetective - Detective\n/AboutGunner - Gunner\n/AboutTanner - Tanner\n/AboutFool - Fool\n/AboutCult - Cultist\n/AboutCH - Cultist Hunter\n/AboutWC - Wild Child\n/AboutAppS - Apprentice seer\n/AboutBH - Beholder\n/AboutMason - Mason\n/AboutDG - Doppelgänger\n/AboutCupid - Cupid\n/AboutHunter - Hunter\n/AboutSK - Serial Killer";

            var reply = "";
            reply += "Roles clásicos:\n";
            reply.ToBold();
            reply += "/aboutVG - Aldeano 👱\n";
            reply += "/aboutWW - Hombre Lobo 🐺\n";
            reply += "/aboutDrunk - Borracho 🍻\n";
            reply += "/aboutSeer - Vidente 👳\n";
            reply += "/aboutCursed - Maldito 😾\n";
            reply += "/aboutHarlot - Ramera 💋\n";
            reply += "/aboutBH - Observadora 👁\n";
            reply += "/aboutGunner - Justiciero 🔫\n";
            reply += "/aboutTraitor - Traidor 🖕\n";
            try
            {
                var result = Bot.Api.SendTextMessageAsync(update.Message.From.Id, reply).Result;
                if (update.Message.Chat.Type != ChatType.Private)
                    Send(GetLocaleString("SentPrivate", GetLanguage(update.Message.From.Id)), update.Message.Chat.Id);
            }
            catch (Exception e)
            {
                RequestPM(update.Message.Chat.Id);
                return;
            }
            Thread.Sleep(300);
            reply = "/aboutGA - Ángel de la guarda 👼\n";
            reply += "/aboutDetective - Detective 🕵\n";
            reply += "/aboutAppS - Aprendiz de vidente 🙇\n";
            reply += "/aboutCult - Cultista 👤\n";
            reply += "/aboutCH - Cazadora de cultistas 💂\n";
            reply += "/aboutWC - Niño salvaje 👶\n";
            reply += "/aboutFool - Necio 🃏\n";
            reply += "/aboutMason - Masón 👷\n";
            reply += "/aboutDG - Doppelgänger 🎭\n";
            reply += "/aboutCupid - Cupido 🏹\n";
            Send(reply, update.Message.From.Id);
            Thread.Sleep(300);
            reply = "/aboutHunter - Cazador 🎯\n";
            reply += "/aboutSK - Asesina en serie 🔪\n";
            reply += "/aboutTanner - Veterano 👺\n";
            reply += "/aboutMayor - Alcalde 🎖\n";
            reply += "/aboutPrince - Princesa 👑\n";
            reply += "/aboutSorcerer - Hechicera 🔮\n";
            reply += "/aboutClumsy - Torpe 🤕\n";
            reply += "/aboutBlacksmith - Herrero ⚒\n";
            reply += "/aboutAlphaWolf - Lobo Alfa ⚡️\n";
            Send(reply, update.Message.From.Id);
            Thread.Sleep(300);
            //reply = "/aboutThief - Thief 😈\n";
            reply = "/aboutWolfCub - Lobezno 🐶\n";
            reply += "/aboutSandman - Arenero 💤\n";
            reply += "/aboutOracle - Oráculo 🌀\n";
            reply += "/aboutWolfMan - Guardabosques 👱‍🌚\n";
            reply += "/aboutLycan - Licántropo 🐺🌝\n";
            reply += "/aboutPacifist - Pacifista ☮️\n";
            reply += "/aboutWiseElder - Anciana sabia 📚\n";
            reply += "/aboutSnowWolf - Lobo de nieve 🐺❄️\n";
            Send(reply, update.Message.From.Id);
            Thread.Sleep(300);
            reply = "Roles nuevos:\n";
            reply.ToBold();
            reply += "/aboutSurvivor - Superviviente ⛺️\n";
            reply += "/aboutAtheist - Ateo 👦\n";
            reply += "/aboutSheriff - Sheriff 🤠\n";
            reply += "/aboutPolice - Policía 👮\n";
            reply += "/aboutHungryWolf - Lobo Voraz 🍽\n";
            reply += "/aboutPyro - Pirómano 🔥\n";
            reply += "/aboutImposter - Impostor ❌\n";
            reply += "/aboutBaker - Panadero 🍞\n";
            reply += "/aboutHealer - Curandero 🌟\n";
            reply += "/aboutRabidWolf - Lobo Rabioso 🐺🤢\n";
            Send(reply, update.Message.From.Id);
            Thread.Sleep(300);
            reply = "/aboutSleepwalker - Sonámbulo 😴\n";
            reply += "/aboutHerbalist - Herborista 🍃\n";
            reply += "/aboutNinja - Ninja 💨\n";
            reply += "/aboutSnooper - Lobo Husmeador 🐾\n";
            reply += "/aboutSpeedWolf - Lobo Veloz 🐺💨\n";
            reply += "/aboutLookout - Vigía 🔭\n";
            reply += "/aboutGuard - Guardia 🛡\n";
            reply += "/aboutHowlingWolf - Lobo Aullador 🐺🌕\n";
            reply += "/aboutFirefighter - Bombero 👨‍🚒\n";
            reply += "/aboutGhost - Fantasma 👻\n";
            Send(reply, update.Message.From.Id);
        }
    }
}
