using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;
using TcpFramework;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Werewolf_Node.Models;
using Message = TcpFramework.Message;

#pragma warning disable 4014

namespace Werewolf_Node
{

    class Program
    {  
        internal static SimpleTcpClient Client;
        internal static Guid ClientId;
        internal static bool Running = true;
        internal static bool nextHela = false;
        internal static bool nextJiro = false;
        internal static bool nextLara = false;
        internal static bool nextAlex = false;
        internal static HashSet<Werewolf> Games = new HashSet<Werewolf>();
        internal static TelegramBotClient Bot;
        internal static User Me;
        internal static Random R = new Random();
        internal static bool IsShuttingDown = false;
        //internal static List<long> GroupInitializing = new List<long>();
        //private static werewolfEntities DB;
        internal static readonly DateTime StartupTime = DateTime.Now;
        internal static DateTime IgnoreTime = DateTime.UtcNow.AddSeconds(10);
        internal static bool SendGifIds = false;
        internal static int CommandsReceived = 0;
        internal static int GamesStarted = 0;
        internal static int Para = 129046388;
        internal static int Jiro = 294728091;
        internal static int Hela = 322300091;
        internal static int Lara = 250353389;
        internal static int Alex = 346366020;
        internal static long ErrorGroup = -1001098399855;
        internal static int DupGamesKilled = 0;
        internal static int TotalPlayers = 0;
        internal static string APIToken;
        //internal static BotanIO.Api.Botan Analytics;

        internal static string LanguageDirectory => Path.GetFullPath(Path.Combine(RootDirectory, @"..\..\..\Languages"));

        internal static string TempLanguageDirectory => Path.GetFullPath(Path.Combine(RootDirectory, @"..\..\TempLanguageFiles"));
        internal static XDocument English;
        internal static int MessagesSent = 0;
        static void Main(string[] args)
        {
            //set up exception logging.  It appears nodes are crashing and I'm not getting any output
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                var ex = eventArgs.ExceptionObject as Exception;
                using (var sw = new StreamWriter(Path.Combine(RootDirectory, "..\\Logs\\NodeFatalError.log"), true))
                {

                    sw.WriteLine($"{DateTime.Now} - {Version} - {ex.Message}");
                    sw.WriteLine(ex.StackTrace);
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        sw.WriteLine($"{ex.Message}");
                        sw.WriteLine(ex.StackTrace);
                    }
                    sw.WriteLine("--------------------------------------------------------");
                }
            };
            English = XDocument.Load(Path.Combine(LanguageDirectory, "Spanish.xml"));




            //get api token from registry
            var key =
                    RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                        .OpenSubKey("SOFTWARE\\Werewolf");

            //var aToken = key.GetValue("BotanBetaAPI").ToString();

#if DEBUG
            APIToken = key.GetValue("DebugAPI").ToString();
#elif RELEASE
            APIToken = key.GetValue("DebugAPI").ToString();
#elif RELEASE2
            APIToken = key.GetValue("ProductionAPI2").ToString();
#elif BETA
            APIToken = key.GetValue("BetaAPI").ToString();
#endif
            Bot = new TelegramBotClient(APIToken);
            Me = Bot.GetMeAsync().Result;
            ClientId = Guid.NewGuid();
            new Thread(KeepAlive).Start();
            Console.Title = $"{ClientId} - {Version.FileVersion}";
            Thread.Sleep(-1);
        }

        private static void ClientOnDelimiterDataReceived(object sender, Message message)
        {
            //if (message.MessageString != "ping")
            //    Console.WriteLine(message.MessageString);
        }

        private static void ClientOnDataReceived(object sender, Message message)
        {
            try
            {
                var messages = message.MessageString.Split('\u0013');
                foreach (var msg in messages)
                {
                    if (msg == "ping" || String.IsNullOrWhiteSpace(msg)) return; //ignore

                    string t = null;
                    try
                    {
                        dynamic m = JsonConvert.DeserializeObject(msg);
                        t = m.JType?.ToString();
                    }
                    catch (Exception e)
                    {
                        //Bot.SendTextMessage(Settings.MainChatId, e.Message);
                        continue;
                    }
                    Werewolf game;
                    if (t != null)
                    {
                        switch (t)
                        {
                            case "PlayerJoinInfo":
                                var pji = JsonConvert.DeserializeObject<PlayerJoinInfo>(msg);
                                Console.WriteLine(t + " " + pji.User.FirstName);
                                game = Games.FirstOrDefault(x => x.ChatId == pji.GroupId);
                                if (pji.User.Id == Jiro)
                                {
                                    game?.AddPlayer(pji.User, true, nextJiro);
                                    nextJiro = false;
                                }
                                else if (pji.User.Id == Hela)
                                {
                                    game?.AddPlayer(pji.User, true, nextHela);
                                    nextHela = false;
                                }
                                else if (pji.User.Id == Lara)
                                {
                                    game?.AddPlayer(pji.User, true, nextLara);
                                    nextLara = false;
                                }
                                else if (pji.User.Id == Alex)
                                {
                                    game?.AddPlayer(pji.User, true, nextAlex);
                                    nextAlex = false;
                                }
                                else
                                {
                                    game?.AddPlayer(pji.User);
                                }
                                break;
                            case "GameStartInfo":
                                var gsi = JsonConvert.DeserializeObject<GameStartInfo>(msg);
                                if (gsi.nHela)
                                    nextHela = gsi.nHela;
                                if (gsi.nJiro)
                                    nextJiro = gsi.nJiro;
                                if (gsi.nLara)
                                    nextLara = gsi.nLara;
                                if (gsi.nAlex)
                                    nextAlex = gsi.nAlex;

                                //double check we don't already have a game...
                                game = Games.FirstOrDefault(x => x.ChatId == gsi.Chat.Id);
                                if (game != null)
                                {
                                    if (gsi.User.Id == Jiro)
                                    {
                                        game?.AddPlayer(gsi.User, true, nextJiro);
                                        nextJiro = false;
                                    }
                                    else if (gsi.User.Id == Hela)
                                    {
                                        game?.AddPlayer(gsi.User, true, nextHela);
                                        nextHela = false;
                                    }
                                    else if (gsi.User.Id == Lara)
                                    {
                                        game?.AddPlayer(gsi.User, true, nextLara);
                                        nextLara = false;
                                    }
                                    else if (gsi.User.Id == Alex)
                                    {
                                        game?.AddPlayer(gsi.User, true, nextAlex);
                                        nextAlex = false;
                                    }
                                    else
                                    {
                                        game?.AddPlayer(gsi.User);
                                    }
                                }
                                else
                                {
                                    game = new Werewolf(gsi.Chat.Id, gsi.User, gsi.Chat.Title,
                                        gsi.gameMode, gsi.CList);
                                    Console.WriteLine(t + " " + gsi.Chat.Title); // to see in which groups are games started
                                    Games.Add(game);
                                    GamesStarted++;
                                }
                                break;
                            case "ForceStartInfo":
                                Console.WriteLine(t);
                                var fsi = JsonConvert.DeserializeObject<ForceStartInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == fsi.GroupId);
                                game?.ForceStart();
                                break;
                            //case "ReplyInfo":
                            //    var ri = JsonConvert.DeserializeObject<ReplyInfo>(msg);
                            //    game =
                            //        Games.FirstOrDefault(
                            //            x => x.Players.Any(p => p.TeleUser.Id == ri.Update.Message.From.Id && !p.IsDead));
                            //    game?.HandleReply(ri.Update);
                            //    break;
                            case "CallbackInfo":
                                Console.WriteLine(t);
                                var ci = JsonConvert.DeserializeObject<CallbackInfo>(msg);
                                game =
                                    Games.FirstOrDefault(
                                        x => x.Players?.Any(p => p != null && (!p.IsDead || p.PlayerRole == IRole.Ghost) && p.TeleUser.Id == ci.Query.From.Id) ?? false);
                                game?.HandleReply(ci.Query);
                                break;
                            case "PlayerListRequestInfo":
                                Console.WriteLine(t);
                                var plri = JsonConvert.DeserializeObject<PlayerListRequestInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == plri.GroupId);
                                game?.OutputPlayers();
                                break;
                            case "RoleListRequestInfo":
                                Console.WriteLine(t);
                                var rlri = JsonConvert.DeserializeObject<RoleListRequestInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == rlri.GroupId);
                                game?.OutputRoles();
                                break;
                            case "PlayerFleeInfo":
                                Console.WriteLine(t);
                                var pfi = JsonConvert.DeserializeObject<PlayerFleeInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == pfi.GroupId);
                                game?.RemovePlayer(pfi.User);
                                break;
                            case "LoadLangInfo":
                                Console.WriteLine(t);
                                var lli = JsonConvert.DeserializeObject<LoadLangInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == lli.GroupId);
                                game?.LoadLanguage(lli.FileName);
                                break;
                            case "PlayerSmiteInfo":
                                Console.WriteLine(t);
                                var psi = JsonConvert.DeserializeObject<PlayerSmiteInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == psi.GroupId);
                                game?.FleePlayer(psi.UserId);
                                break;
                            case "UpdateNodeInfo":
                                Console.WriteLine(t);
                                var uni = JsonConvert.DeserializeObject<UpdateNodeInfo>(msg);
                                IsShuttingDown = true;
                                if (uni.Kill)
                                {
                                    //force kill
                                    Environment.Exit(1);
                                }
                                break;
                            case "SkipVoteInfo":
                                Console.WriteLine(t);
                                var svi = JsonConvert.DeserializeObject<SkipVoteInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == svi.GroupId);
                                game?.SkipVote();
                                break;
                            case "GameKillInfo":
                                Console.WriteLine(t);
                                var gki = JsonConvert.DeserializeObject<GameKillInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == gki.GroupId);
                                game?.Kill();
                                break;
                            case "GetGameInfo":
                                Console.WriteLine(t);
                                var ggi = JsonConvert.DeserializeObject<GetGameInfo>(msg);
                                var g = Games.FirstOrDefault(x => x.ChatId == ggi.GroupId);
                                if (g == null)
                                    message.Reply("null");
                                //build our response
                                var gi = new GameInfo(g.ChatId)
                                {
                                    Language = g.Language,
                                    ChatGroup = g.ChatGroup,
                                    //GroupId = g.ChatId,
                                    NodeId = ClientId,
                                    Guid = g.Guid,
                                    Cycle = g.Time,
                                    State = g.IsRunning ? GameState.Running : g.IsJoining ? GameState.Joining : GameState.Dead,
                                    Users = new HashSet<int>(g.Players?.Where(x => !x.IsDead)?.Select(x => x.TeleUser.Id)??new[]{0}),
                                    Players = g.Players?.Select(x => new 
                                    {
                                        Bitten = x.Bitten?"Yes":"No",
                                        x.Bullet,
                                        Choice = g.Players.FirstOrDefault(p => p.Id == x.Choice)?.Name,
                                        CurrentQuestion = x.CurrentQuestion?.QType.ToString(),
                                        x.DonationLevel,
                                        IsDead = x.IsDead?"Yes":"No",
                                        x.Name,
                                        LoverId = g.Players.FirstOrDefault(p => p.Id == x.LoverId)?.Name,
                                        PlayerRole = x.PlayerRole.ToString(),
                                        Team = x.Team.ToString(),
                                        x.Votes,
                                        x.Id
                                    })
                                };
                                message.Reply(JsonConvert.SerializeObject(gi));
                                break;
                            case "ExtendTimeInfo":
                                Console.WriteLine(t);
                                var eti = JsonConvert.DeserializeObject<ExtendTimeInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == eti.GroupId);
                                game?.ExtendTime(eti.User, eti.Admin, eti.Seconds);
                                break;
                            case "JoinButtonRequestInfo":
                                Console.WriteLine(t);
                                var jbri = JsonConvert.DeserializeObject<PlayerListRequestInfo>(msg);
                                game = Games.FirstOrDefault(x => x.ChatId == jbri.GroupId);
                                game?.ShowJoinButton();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(jbri.GroupId);
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            default:
                                Console.WriteLine(t);
                                Console.WriteLine(msg);
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + message.MessageString);
                try
                {
                    Directory.CreateDirectory(Path.Combine(RootDirectory, "ReceiveErrors"));
                    using (var sw = new StreamWriter(Path.Combine(RootDirectory, "ReceiveErrors", "error.log"), true))
                    {
                        sw.WriteLine(e.Message + Environment.NewLine + message.MessageString + Environment.NewLine +
                                     e.StackTrace);
                    }
                }
                catch
                {
                    // ignored
                }
            }

        }

        public static void RemoveGame(Werewolf werewolf)
        {
            try
            {
                if (werewolf?.Players != null)
                {
                    TotalPlayers += werewolf.Players.Count();
                }
                if (werewolf != null)
                {
                    werewolf.MessageQueueing = false; // shut off the queue to be sure
                    Games.Remove(werewolf);
                    //kill the game completely
                    werewolf.Dispose();
                    werewolf = null;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveGame: " + ex.Message);
            }
        }

        internal static async Task<Telegram.Bot.Types.Message> Send(string message, long id, bool clearKeyboard = false, InlineKeyboardMarkup customMenu = null, Werewolf game = null, bool notify = false)
        {
            MessagesSent++;
            //message = message.FormatHTML();
            //message = message.Replace("`",@"\`");
            if (clearKeyboard)
            {
                var menu = new ReplyKeyboardRemove() { RemoveKeyboard = true };
                return await Bot.SendTextMessageAsync(id, message, replyMarkup: menu, disableWebPagePreview: true, parseMode: ParseMode.Html, disableNotification: notify);
            }
            else if (customMenu != null)
            {
                return await Bot.SendTextMessageAsync(id, message, replyMarkup: customMenu, disableWebPagePreview: true, parseMode: ParseMode.Html, disableNotification: notify);
            }
            else
            {
                return await Bot.SendTextMessageAsync(id, message, disableWebPagePreview: true, parseMode: ParseMode.Html, disableNotification: notify);
            }
        }

        internal static FileVersionInfo Version
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi;
            }
        }

        internal static string RootDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal static void Connect()
        {
            Client = new SimpleTcpClient();
            Client.DataReceived += ClientOnDataReceived;
            Client.DelimiterDataReceived += ClientOnDelimiterDataReceived;
            //connection lost, let's try to reconnect
            while (Client.TcpClient == null || !Client.TcpClient.Connected)
            {
                try
                {
                    Client.Connect(Settings.ServerIP, Settings.Port);
                    var regInfo = new ClientRegistrationInfo { ClientId = ClientId };
                    var json = JsonConvert.SerializeObject(regInfo);
                    Client.WriteLine(json);
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                        ex = ex.InnerException;
                    Console.WriteLine($"Error in reconnect: {ex.Message}\n{ex.StackTrace}\n");
                }
                Thread.Sleep(100);
            }
        }

        public static void KeepAlive()
        {
            string ver = Version.FileVersion;
            
            Connect();
            while (Running)
            {
                if ((DateTime.Now - StartupTime).Hours > 10)
                    IsShuttingDown = true;

                var infoGathered = false;

                if (Games == null || (IsShuttingDown && Games.Count == 0))
                {
                    Thread.Sleep(5000);
                    //Running = false;
                    Environment.Exit(0);
                    return;
                }
                //monitor the tcp connection to keep it open
                try
                {
                    if (Games == null)
                    {
                        //uhhhhhhhhh  ok.....
                        continue;
                    }
                    var games = Games.ToList();

                    var info = new NodeInfo
                    {
                        Games = new HashSet<GameInfo>(),
                        ClientId = ClientId,
                        CurrentGames = games.Count,
                        CurrentPlayers = games.Sum(x => x?.Players?.Count ?? 0),
                        DuplicateGamesRemoved = DupGamesKilled,
                        ThreadCount = 0,//Process.GetCurrentProcess().Threads.Count,
                        //TotalGames = GamesStarted,
                        //TotalPlayers = games.Sum(x => x.Players?.Count ?? 0) + TotalPlayers,
                        Uptime = DateTime.Now - StartupTime,
                        Version = ver,
                        ShuttingDown = IsShuttingDown,
                        MessagesSent = MessagesSent
                    };

                    foreach (var g in games)
                    {
                        if (g?.Players == null)
                        {
                            try
                            {
                                Games.Remove(g);
                            }
                            catch
                            {
                                // ignored, it was already removed
                            }
                            continue;
                        }
                        var gi = new GameInfo(g.ChatId)
                        {
                            Language = g.Language,
                            ChatGroup = g.ChatGroup,
                            GroupId = g.ChatId,
                            NodeId = ClientId,
                            Guid = g.Guid,
                            State = g.IsRunning ? GameState.Running : g.IsJoining ? GameState.Joining : GameState.Dead,
                            Users = g.Players != null ? new HashSet<int>(g.Players.Where(x => !x.IsDead).Select(x => x.TeleUser.Id)) : new HashSet<int>(),
                            PlayerCount = g.Players?.Count ?? 0
                            //Players = new HashSet<IPlayer>(g.Players)
                        };
                        info.Games.Add(gi);
                    }

                    var json = JsonConvert.SerializeObject(info);
                    infoGathered = true;
                    Client.WriteLine(json);
                }
                catch (Exception e)
                {
                    Environment.Exit(69);

                    while (e.InnerException != null)
                        e = e.InnerException;
                    Console.WriteLine($"Error in KeepAlive: {e.Message}\n{e.StackTrace}\n");
                    if (infoGathered) //only disconnect if tcp error
                    {
                        if (Client != null)
                        {
                            try
                            {
                                Client.DataReceived -= ClientOnDataReceived;
                                Client.DelimiterDataReceived -= ClientOnDelimiterDataReceived;
                                Client.Disconnect();
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                        Connect();
                    }
                }
                Thread.Sleep(500);
            }
        }
    }
}
