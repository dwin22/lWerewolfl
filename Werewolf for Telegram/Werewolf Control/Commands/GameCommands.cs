﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Werewolf_Control.Attributes;
using Werewolf_Control.Helpers;

namespace Werewolf_Control
{
    public static partial class Commands
    {
        [Command(Trigger = "startgame", Blockable = true, InGroupOnly = true)]
        public static void StartGame(Update update, string[] args)
        {
            if (!Program.MaintMode)
                StartGame(0, update);
            else
            {
                Send(Program.MaintMessage ?? "El bot está siendo actualizado para meter cosas nuevas :D Prueba a iniciar en unos minutos.",
                    update.Message.Chat.Id);
            }
        }

        [Command(Trigger = "startchaos", Blockable = true, InGroupOnly = true)]
        public static void StartChaos(Update update, string[] args)
        {
            if (!Program.MaintMode)
                StartGame(1, update);
            else
            {
                Send(Program.MaintMessage ?? "El bot está siendo actualizado para meter cosas nuevas :D Prueba a iniciar en unos minutos.",
                    update.Message.Chat.Id);
            }
        }

        [Command(Trigger = "startclumsy", Blockable = true, InGroupOnly = true)]
        public static void StartClumsy(Update update, string[] args)
        {
            if (!Program.MaintMode)
                StartGame(2, update);
            else
            {
                Send(Program.MaintMessage ?? "El bot está siendo actualizado para meter cosas nuevas :D Prueba a iniciar en unos minutos.",
                    update.Message.Chat.Id);
            }
        }

        [Command(Trigger = "superchaos", Blockable = true, InGroupOnly = true)]
        public static void SuperChaos(Update update, string[] args)
        {
            if (!Program.MaintMode)
                StartGame(3, update);
            else
            {
                Send(Program.MaintMessage ?? "El bot está siendo actualizado para meter cosas nuevas :D Prueba a iniciar en unos minutos.",
                    update.Message.Chat.Id);
            }
        }

        [Command(Trigger = "startranked", Blockable = true, InGroupOnly = true)]
        public static void StartRanked(Update update, string[] args)
        {
            if (!Program.MaintMode)
                StartGame(5, update);
            else
            {
                Send(Program.MaintMessage ?? "El bot está siendo actualizado para meter cosas nuevas :D Prueba a iniciar en unos minutos.",
                    update.Message.Chat.Id);
            }
        }

        [Command(Trigger = "startcustom", Blockable = true, InGroupOnly = true)]
        public static void StartCustom(Update update, string[] args)
        {
            if (!Program.MaintMode)
            {
                StartGame(4, update, args[1]);
            }
            else
            {
                Send(Program.MaintMessage ?? "El bot está siendo actualizado para meter cosas nuevas :D Prueba a iniciar en unos minutos.",
                    update.Message.Chat.Id);
            }
        }

        [Command(Trigger = "startinverted", Blockable = true, InGroupOnly = true)]
        public static void StartInverted(Update update, string[] args)
        {
            if (!Program.MaintMode)
                StartGame(6, update);
            else
            {
                Send(Program.MaintMessage ?? "El bot está siendo actualizado para meter cosas nuevas :D Prueba a iniciar en unos minutos.",
                    update.Message.Chat.Id);
            }
        }

        [Command(Trigger = "testeoroles", Blockable = true, InGroupOnly = true)]
        public static void TesteoRoles(Update update, string[] args)
        {
            if (args[1] == null)
                Send("Por favor, especifica el número de jugadores.", update.Message.Chat.Id);
            else
                StartGame(20, update, args[1]);
        }

        [Command(Trigger = "nextjiro", Blockable = true, InGroupOnly = true)]
        public static void NextJiro(Update update, string[] args)
        {
            if (update.Message.From.Id == 322300091)
            {
                var node = Bot.GetBestAvailableNode();
                node.nextJiro = true;
                Send("Te has registrado para enterarte de cuando Jiro se una a una partida.",
                    322300091);
            }
        }

        [Command(Trigger = "nexthela", Blockable = true, InGroupOnly = true)]
        public static void NextHela(Update update, string[] args)
        {
            if (update.Message.From.Id == 294728091)
            {
                var node = Bot.GetBestAvailableNode();
                node.nextHela = true;
                Send("Te has registrado para enterarte de cuando Hela se una a una partida.",
                    294728091);
            }
        }

        [Command(Trigger = "nextlara", Blockable = true, InGroupOnly = true)]
        public static void NextLara(Update update, string[] args)
        {
            if (update.Message.From.Id == 346366020)
            {
                var node = Bot.GetBestAvailableNode();
                node.nextLara = true;
                Send("Te has registrado para enterarte de cuando Lara se una a una partida.",
                    346366020);
            }
        }

        [Command(Trigger = "nextalex", Blockable = true, InGroupOnly = true)]
        public static void NextAlex(Update update, string[] args)
        {
            if (update.Message.From.Id == 250353389)
            {
                var node = Bot.GetBestAvailableNode();
                node.nextAlex = true;
                Send("Te has registrado para enterarte de cuando Alex se una a una partida.",
                    250353389);
            }
        }

        [Command(Trigger = "join", Blockable = true, InGroupOnly = true)]
        public static void Join(Update update, string[] args)
        {
            var id = update.Message.Chat.Id;
            using (var db = new WWContext())
            {
                if (update.Message.Chat.Type == ChatType.Private)
                {
                    //PM....  can't do that here
                    Send(GetLocaleString("JoinFromGroup", GetLanguage(update.Message.From.Id)), id);
                    return;
                }
                //check nodes to see if player is in a game
                var node = GetPlayerNode(update.Message.From.Id);
                var game = GetGroupNodeAndGame(update.Message.Chat.Id);
                if (game == null)
                {
                    Thread.Sleep(50);
                    game = GetGroupNodeAndGame(update.Message.Chat.Id);
                }
                if (game == null)
                {
                    Thread.Sleep(50);
                    game = GetGroupNodeAndGame(update.Message.Chat.Id);
                }

                if (game != null || node != null)
                {
                    //try grabbing the game again...
                    if (game == null)
                        game = node.Games.FirstOrDefault(x => x.Users.Contains(update.Message.From.Id));
                    if (game?.Users.Contains(update.Message.From.Id) ?? false)
                    {
                        if (game.GroupId != update.Message.Chat.Id)
                        {
                            //player is already in a game (in another group), and alive
                            var grp = db.Groups.FirstOrDefault(x => x.GroupId == id);
                            Send(GetLocaleString("AlreadyInGame", grp?.Language ?? "English", game.ChatGroup.ToBold()), update.Message.Chat.Id);
                            return;
                        }
                        else
                        {
                            //do nothing, player is in the game, in that group, they are just being spammy
                            return;
                        }
                    }

                    //player is not in game, they need to join, if they can
                    game?.AddPlayer(update);

                    //game?.ShowJoinButton();
                    if (game == null)
                        Program.Log($"{update.Message.From.FirstName} tried to join a game on node {node?.ClientId}, but game object was null", true);
                    return;
                }
                if (game == null)
                {
                    var grp = db.Groups.FirstOrDefault(x => x.GroupId == id);
                    if (grp == null)
                    {
                        grp = MakeDefaultGroup(id, update.Message.Chat.Title, "join");
                        db.Groups.Add(grp);
                        db.SaveChanges();
                    }
                    Send(GetLocaleString("NoGame", grp?.Language ?? "English"), id);
                }
            }
        }

        [Command(Trigger = "forcestart", Blockable = true, GroupAdminOnly = true, InGroupOnly = true)]
        public static void ForceStart(Update update, string[] args)
        {
            var id = update.Message.Chat.Id;
            using (var db = new WWContext())
            {
                var grp = db.Groups.FirstOrDefault(x => x.GroupId == id);
                if (grp == null)
                {
                    grp = MakeDefaultGroup(id, update.Message.Chat.Title, "forcestart");
                    db.Groups.Add(grp);
                    db.SaveChanges();
                }

                var game = GetGroupNodeAndGame(update.Message.Chat.Id);
                if (game != null)
                {
                    //send forcestart                                            
                    game.ForceStart();
                }
                else
                {
                    Send(GetLocaleString("NoGame", grp.Language), id);
                }
            }

        }

        [Command(Trigger = "players", Blockable = true, InGroupOnly = true)]
        public static void Players(Update update, string[] args)
        {
            var id = update.Message.Chat.Id;

            var game = GetGroupNodeAndGame(id);
            if (game == null)
            {
                Send(GetLocaleString("NoGame", GetLanguage(id)), id);
            }
            else
            {
                game.ShowPlayers();
            }

        }

        [Command(Trigger = "roles", Blockable = true, InGroupOnly = true)]
        public static void Roles(Update update, string[] args)
        {
            var id = update.Message.Chat.Id;

            var game = GetGroupNodeAndGame(id);
            if (game == null)
            {
                Send(GetLocaleString("NoGame", GetLanguage(id)), id);
            }
            else
            {
                game.ShowRoles();
            }

        }

        [Command(Trigger = "flee", Blockable = true, InGroupOnly = true)]
        public static void Flee(Update update, string[] args)
        {
            var id = update.Message.Chat.Id;
            //check nodes to see if player is in a game
            var node = GetPlayerNode(update.Message.From.Id);
            var game = GetGroupNodeAndGame(update.Message.Chat.Id);
            if (game != null || node != null)
            {
                //try grabbing the game again...
                if (node != null)
                    game =
                        node.Games.FirstOrDefault(
                            x => x.Users.Contains(update.Message.From.Id));
                if (game?.Users.Contains(update.Message.From.Id) ?? false)
                {
                    game?.RemovePlayer(update);

                    return;
                }
                if (node != null)
                {
                    //there is a game, but this player is not in it
                    Send(GetLocaleString("NotPlaying", GetLanguage(id)), id);
                }
            }
            else
            {
                Send(GetLocaleString("NoGame", GetLanguage(id)), id);
            }
        }

        [Command(Trigger = "extend", Blockable = true, InGroupOnly = true)]
        public static void Extend(Update update, string[] args)
        {
            var id = update.Message.Chat.Id;
            var isadmin = UpdateHelper.IsGroupAdmin(update) || UpdateHelper.IsGlobalAdmin(update.Message.From.Id);
            //check nodes to see if player is in a game
            var node = GetPlayerNode(update.Message.From.Id);
            var game = GetGroupNodeAndGame(update.Message.Chat.Id);
            

            if (game == null) //if this doesn't work, you'll have to get the game as node.Games.FirstOrDefault(x => x.GroupId == update.Message.Chat.Id) in the second else if block
                Send(GetLocaleString("NoGame", GetLanguage(id)), id);
            else if ((game != null && node == null) && !isadmin) //there is a game, but this player is not in it
                Send(GetLocaleString("NotPlaying", GetLanguage(id)), id);
            else if ((game != null && (node != null)) || isadmin) //player is in the game, or is an admin
            {
                int seconds = int.TryParse(args[1], out seconds) ? seconds : 30;
                if (seconds < 0 && !isadmin)
                    Send(GetLocaleString("GroupAdminOnly", GetLanguage(id)), id); //otherwise we're allowing people to /forcestart
                else
                {
                    using (var db = new WWContext())
                    {
                        var grp = db.Groups.FirstOrDefault(x => x.GroupId == update.Message.Chat.Id);
                        if (isadmin || (grp.HasFlag(GroupConfig.AllowExtend)))
                        {
                            int maxextend = grp.MaxExtend ?? Settings.MaxExtend;
                            seconds = Math.Abs(seconds) > maxextend ? maxextend * Math.Sign(seconds) : seconds;
                            game?.ExtendTime(update.Message.From.Id, isadmin, seconds);
                      }
                        else
                            Send(GetLocaleString("GroupAdminOnly", GetLanguage(id)), id);
                    }
                }
                return;
            }
        }

        [Command(Trigger = "stopwaiting", Blockable = true)]
        public static void StopWaiting(Update update, string[] args)
        {
            long groupid = 0;
            string groupname = "";
            if (update.Message.Chat.Id < 0) //it's a group
            {
                groupid = update.Message.Chat.Id;
                groupname = update.Message.Chat.Title;
            }
            else if (args.Length >= 2 && !String.IsNullOrEmpty(args[1])) {
                using (var db = new WWContext())
                {
                    var grp = GetGroup(args[1], db);
                    groupid = grp?.GroupId ?? 0;
                    groupname = grp?.Name ?? "";
                }
            }

            if (groupid == 0)
            {
                Send(GetLocaleString("GroupNotFound", GetLanguage(update.Message.From.Id)), update.Message.Chat.Id);
                return;
            }

            using (var db = new WWContext())
                db.Database.ExecuteSqlCommand($"DELETE FROM NotifyGame WHERE GroupId = {groupid} AND UserId = {update.Message.From.Id}");

            Send(GetLocaleString("DeletedFromWaitList", GetLanguage(update.Message.From.Id), groupname.ToBold()), update.Message.From.Id);
        }
    }
}
