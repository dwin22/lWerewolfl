using Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using Werewolf_Node.Helpers;
using Werewolf_Node.Models;

// ReSharper disable PossibleMultipleEnumeration warning
#pragma warning disable 4014

namespace Werewolf_Node
{
    public class Werewolf : IDisposable
    {
        public long ChatId;
        public int GameDay, GameId, gameMode;
        private int _secondsToAdd = 0;
        public List<IPlayer> Players = new List<IPlayer>();
        public bool IsRunning,
            IsJoining = true,
            KillTimer,
            IsInitializing,
            MessageQueueing = true,
            Chaos, WolfCubKilled,
            RabidKilled,
            NoOneCastLynch;
        public int Jiro = 294728091;
        public int Teste = 291476121;
        public int Hela = 322300091;
        public int Lara = 250353389;
        public int Alex = 346366020;
        public Guid Guid = Guid.NewGuid();
        private readonly InlineKeyboardMarkup _requestPMButton;
        public DateTime LastPlayersOutput = DateTime.Now;
        public DateTime LastRolesOutput = DateTime.Now;
        public GameTime Time;
        public string Language = "English SFW", ChatGroup;
        public Locale Locale;
        public Group DbGroup = new Group() { ShowRolesEnd = "All", ShowRolesDeath = true };
        private bool _playerListChanged = true, _silverSpread, _sandmanSleep, _bakerDied, _pacifistUsed;
        private DateTime _timeStarted;
        private Nullable<TimeSpan> _timePlayed = null;
        public readonly IRole[] WolfRoles = { IRole.Wolf, IRole.AlphaWolf, IRole.WolfCub, IRole.Lycan, IRole.HungryWolf, IRole.RabidWolf, IRole.SpeedWolf, IRole.HowlingWolf };
        public readonly IRole[] SupportWolves = { IRole.SnowWolf, IRole.Snooper };
        public readonly IRole[] AllWolves = { IRole.Wolf, IRole.AlphaWolf, IRole.WolfCub, IRole.Lycan, IRole.HungryWolf, IRole.RabidWolf, IRole.SpeedWolf, IRole.SnowWolf, IRole.Snooper, IRole.HowlingWolf };
        public readonly IRole[] BadRoles = { IRole.Wolf, IRole.AlphaWolf, IRole.WolfCub, IRole.Lycan, IRole.HungryWolf, IRole.RabidWolf, IRole.SerialKiller, IRole.Pyro, IRole.Sorcerer, IRole.Imposter, IRole.Cultist, IRole.SnowWolf, IRole.Snooper, IRole.SpeedWolf, IRole.HowlingWolf };
        public readonly IRole[] RepeatableRoles = { IRole.Villager, IRole.Police, IRole.Mason, IRole.Cultist, IRole.Wolf, IRole.Guard };
        public readonly IRole[] NeutralRoles = { IRole.Tanner, IRole.Survivor, IRole.Doppelgänger };
        public readonly IRole[] AllyRoles = { IRole.WildChild, IRole.Cursed, IRole.Traitor };
        public readonly IRole[] WolfHelpers = { IRole.Sorcerer, IRole.Imposter, IRole.Traitor };
        public readonly IRole[] Roles120 = { IRole.WildChild, IRole.Doppelgänger, IRole.Cupid, IRole.Imposter };
        public readonly IRole[] LowCultables = { IRole.Pacifist, IRole.Blacksmith, IRole.Cursed, IRole.Detective, IRole.Healer, IRole.Herbalist, IRole.Hunter, IRole.Ninja, IRole.Oracle, IRole.Sandman, IRole.Seer, IRole.Sheriff, IRole.Sleepwalker, IRole.WiseElder, IRole.Lookout, IRole.Firefighter, IRole.Ghost };
        public readonly IRole[] Cultables = { IRole.ApprenticeSeer, IRole.Baker, IRole.Beholder, IRole.ClumsyGuy, IRole.Cupid, IRole.Drunk, IRole.Fool, IRole.Mason, IRole.Mayor, IRole.Police, IRole.Prince, IRole.Survivor, IRole.Villager, IRole.WildChild, IRole.WolfMan, IRole.Guard };
        public readonly IRole[] SafeRoles = { IRole.Seer, IRole.GuardianAngel, IRole.Detective, IRole.Harlot, IRole.Herbalist };
        public List<long> HaveExtended = new List<long>();
        private List<IPlayer> _joined = new List<IPlayer>(); 
        private int _joinMsgId;
        private string FirstMessage = "";
        private DateTime LastJoinButtonShowed = DateTime.MinValue;
        private InlineKeyboardMarkup _joinButton;
        private List<int> _joinButtons = new List<int>();
        private int _playerListId = 0;
        private int _roleListId = 0;
        public bool RandomMode = false;
        public bool ShowRolesOnDeath, AllowTanner, AllowFool, AllowCult, SecretLynch, ShowIDs, AllowNSFW, AllowThief, RankPerks, DisableVillager, StrongMode;
        public string ShowRolesEnd;
        public int totalplayers = 0;
        public int startingwolves = 0;
        public int WolfScore = 0;
        public int VillageScore = 0;
        public int WolfTarget1 = 0;
        public int WolfTarget2 = 0;
        public int CultTarget = 0;
        public bool Inverted = false;
        public int SpecialCount = 0;
        public List<string> customList = new List<string>();
        public bool SomeoneSmited = false;
        public bool HowledTonight = false;

        public List<string> VillagerDieImages,
            WolfWin,
            WolvesWin,
            VillagersWin,
            NoWinner,
            StartGame,
            StartChaosGame,
            TannerWin,
            CultWins,
            SerialKillerWins,
            PyroWins,
            ClumsyGame,
            LoversWin,
            SuperChaos,
            StartCustom,
            SkStab,
            PyroBurn,
            StartInverted;
        
        #region Constructor
        /// <summary>
        /// Starts a new instance of a werewolf game
        /// </summary>
        /// <param name="chatid">Id of the group starting the game</param>
        /// <param name="u">User that started the game</param>
        /// <param name="chatGroup">Name of the group starting the game</param>
        /// <param name="chaos">Chaos mode yes or no</param>
        public Werewolf(long chatid, User u, string chatGroup, int gmode, List<string> cList)
        {
            try
            {
                /*VillagerDieImages = Settings.VillagerDieImages.ToList();
                WolfWin = Settings.WolfWin.ToList();
                WolvesWin = Settings.WolvesWin.ToList();
                VillagersWin = Settings.VillagersWin.ToList();
                NoWinner = Settings.NoWinner.ToList();
                StartGame = Settings.StartGame.ToList();
                StartChaosGame = Settings.StartChaosGame.ToList();
                TannerWin = Settings.TannerWin.ToList();
                CultWins = Settings.CultWins.ToList();
                SerialKillerWins = Settings.SerialKillerWins.ToList();
                LoversWin = Settings.LoversWin.ToList();*/

                VillagerDieImages = new List<string> { "CgADBAADKgMAAoMbZAcDMzzvBLtkDgI", "CgADBAADWAMAAt4cZAca-4d_WgIvAwI", "CgADAwADggADdBexB2dfz7NFUvhUAg" };
                WolfWin = new List<string> { "CgADAwADgAADdBexB3RS8Ngm9Qk-Ag", "CgADAwADgQADdBexB4nNsYbJdwQcAg" };
                WolvesWin = new List<string> { "CgADBAADlwMAAtgaZAcGis70fHk5_wI", "CgADBAADcAMAAn8ZZAe4INcCBiINwQI" };
                VillagersWin = new List<string> { "CgADAwADgwADdBexBzd2zefLWqdCAg" };
                NoWinner = new List<string> { "CgADBAADuAMAAlUXZAcr9cAAAfH9XIEC", "CgADBAAD8QgAAqIeZAfXa3s4HDi0VAI" };
                StartGame = new List<string> { "CgADBAADwg0AAu0XZAeJYaNtQl8otgI", "CgADAwADhAADdBexBzhVGklBymDqAg" };
                StartChaosGame = new List<string> { "CgADBAAD7wYAAgcYZAfZ1dhbwjxX5QI", "CgADBAAD_wcAAiUYZAfPZzx5cHb5RwI" };
                TannerWin = new List<string> { "CgADBAAD_gMAAtgaZAczTl8FGKfMbQI", "CgADBAADQwgAAuQaZAcQOHeUv7YwVgI" };
                CultWins = new List<string> { "CgADBAADWAMAAosYZAe-7g6vJNEKnwI", "CgADBAADHwsAAgUYZAf97T2hRLDVegI" };
                SerialKillerWins = new List<string> { "CgADBAADOAQAAqUXZAe78wwTRk8L4wI", "CgADBAADdQMAAsEcZAdHFMBLTBk-7gI", "CgADBAADmgMAArgcZAfvhgTxjBBWjAI", "CgADBAADKwMAAsQZZAfLl-elnd1eZwI" };
                LoversWin = new List<string> { "CgADBAADYAMAAkMdZAfJH7O_SbVC_gI", "CgADBAAD8hUAAhYYZAd3a0TjAwaT9AI" };
                PyroWins = new List<string> { "CgADBAADxqIAAuYXZAc5ogGUFZJjuAI" };
                ClumsyGame = new List<string> { "CgADBAAD0JgAAsAcZAeGq4YcSBjJSQI" };
                SuperChaos = new List<string> { "CgADBAADHwADXfCgUYo8PN0peNCNAg" };
                StartCustom = new List<string> { "CgADBAADoAADwhSMU58OfazPxYIMAg" };
                SkStab = new List<string> { "CgADBAADBwADnjokUeYfwyvDdWJsAg" };
                PyroBurn = new List<string> { "CgADBAADnwAD0XvcU80nnXzfQIZaAg" };
                StartInverted = new List<string> { "CgADBAADKgEAAkGuFFEgG7lRwazpFAI" };

                new Thread(GroupQueue).Start();

                ChatGroup = chatGroup;
                ChatId = chatid;
                gameMode = gmode;
                customList = cList;
                if (gameMode == 1)
                {
                    Chaos = true;
                }
                else
                {
                    Chaos = false;
                }

                if (gameMode == 6)
                    Inverted = true;

                if (gameMode == 20)
                {
                    SpecialCount = int.Parse(cList[0]);
                    for (int i = 0; i < SpecialCount; i++)
                    {
                        var p = new IPlayer
                        {
                            Name = "TestBoi " + i
                        };
                        p.Id = i + 1;
                        p.Score = 1000;
                        Players.Add(p);
                    }
                    LoadLanguage("Spanish");
                    AssignRoles();
                    var msg = $"{GetLocaleString("PlayersAlive")}: {Players.Count(x => !x.IsDead)} / {Players.Count}\n" + Players.OrderBy(x => x.TimeDied).Aggregate("", (current, p) => current + ($"{p.Name}: {(p.IsDead ? (p.Fled ? GetLocaleString("RanAway") : GetLocaleString("Dead")) : GetLocaleString("Alive")) + " - " + GetEndDescription(p)}\n"));
                    SendWithQueue(msg);

                    Thread.Sleep(10000);
                    Program.RemoveGame(this);
                    return;
                }

                using (var db = new WWContext())

                {
                    //ChatGroup = chatGroup;
                    //ChatId = chatid;
                    DbGroup = db.Groups.FirstOrDefault(x => x.GroupId == ChatId);

                    if (DbGroup == null)
                    {
                        MessageQueueing = false;
                        Program.RemoveGame(this);
                        return;
                    }
                    try
                    {
                        var memberCount = Program.Bot.GetChatMembersCountAsync(chatid).Result;
                        DbGroup.MemberCount = memberCount;

                        db.SaveChanges();
                    }
                    catch
                    {
                        // ignored
                    }
#if !BETA
                    var player = db.Players.FirstOrDefault(x => x.TelegramId == u.Id);
                    if (player?.CustomGifSet != null)
                    {
                        var gifset = JsonConvert.DeserializeObject<CustomGifData>(player.CustomGifSet);
                        if (gifset.Approved == true)
                        {
                            if (gifset.StartChaosGame != null)
                            {
                                StartChaosGame.Clear();
                                StartChaosGame.Add(gifset.StartChaosGame);
                            }
                            if (gifset.StartGame != null)
                            {
                                StartGame.Clear();
                                StartGame.Add(gifset.StartGame);
                            }
                        }
                    }
#endif
                    DbGroup.UpdateFlags();
                    ShowIDs = DbGroup.HasFlag(GroupConfig.ShowIDs);
                    RandomMode = DbGroup.HasFlag(GroupConfig.RandomMode);
                    db.SaveChanges();
                    if (RandomMode)
                    {
                        //Chaos = Program.R.Next(100) < 50;
                        AllowTanner = Program.R.Next(100) < 50;
                        AllowFool = Program.R.Next(100) < 50;
                        AllowCult = Program.R.Next(100) < 50;
                        AllowThief = Program.R.Next(100) < 50;
                        RankPerks = Program.R.Next(100) < 50;
                        SecretLynch = Program.R.Next(100) < 50;
                        ShowRolesOnDeath = Program.R.Next(100) < 50;
                        DisableVillager = Program.R.Next(100) < 50;
                        StrongMode = Program.R.Next(100) < 50;
                        var r = Program.R.Next(100);
                        if (r < 33)
                            ShowRolesEnd = "None";
                        else if (r < 67)
                            ShowRolesEnd = "Living";
                        else
                            ShowRolesEnd = "All";

                    }
                    else
                    {
                        //decide if chaos or not
                        //Chaos = DbGroup.Mode == "Player" ? chaos : DbGroup.Mode == "Chaos";
                        ShowRolesEnd = DbGroup.ShowRolesEnd;
                        AllowTanner = DbGroup.HasFlag(GroupConfig.AllowTanner);
                        AllowFool = DbGroup.HasFlag(GroupConfig.AllowFool);
                        AllowCult = DbGroup.HasFlag(GroupConfig.AllowCult);
                        AllowThief = DbGroup.HasFlag(GroupConfig.AllowThief);
                        RankPerks = DbGroup.HasFlag(GroupConfig.RankPerks);
                        SecretLynch = DbGroup.HasFlag(GroupConfig.EnableSecretLynch);
                        ShowRolesOnDeath = DbGroup.HasFlag(GroupConfig.ShowRolesDeath);
                        DisableVillager = DbGroup.HasFlag(GroupConfig.DisableVillager);
                        StrongMode = DbGroup.HasFlag(GroupConfig.StrongMode);
                    }
                    AllowNSFW = DbGroup.HasFlag(GroupConfig.AllowNSFW);


                    LoadLanguage(DbGroup.Language);
                    
                    _requestPMButton = new InlineKeyboardMarkup(new[] { new InlineKeyboardUrlButton("Start Me", "http://t.me/" + Program.Me.Username) });
                    //AddPlayer(u);
                }

                var deeplink = $"{Program.ClientId.ToString("N")}{Guid.ToString("N")}";

                //create our button
                _joinButton = new InlineKeyboardMarkup(new[]
                {
                    new InlineKeyboardUrlButton(GetLocaleString("JoinButton"),$"https://t.me/{Program.Me.Username}?start=" + deeplink)
                });
                switch (gameMode)
                {
                    case 0:
                        FirstMessage = GetLocaleString("PlayerStartedGame", u.FirstName);
                        _joinMsgId = Program.Bot.SendDocumentAsync(chatid, new FileToSend(GetRandomImage(StartGame)), FirstMessage, replyMarkup: _joinButton).Result.MessageId;
                        break;
                    case 1:
                        FirstMessage = GetLocaleString("PlayerStartedChaosGame", u.FirstName);
                        _joinMsgId = Program.Bot.SendDocumentAsync(chatid, new FileToSend(GetRandomImage(StartChaosGame)), FirstMessage, replyMarkup: _joinButton).Result.MessageId;
                        break;
                    case 4:
                        FirstMessage = GetLocaleString("PlayerStartedCustomGame", u.FirstName);
                        _joinMsgId = Program.Bot.SendDocumentAsync(chatid, new FileToSend(GetRandomImage(StartCustom)), FirstMessage, replyMarkup: _joinButton).Result.MessageId;
                        break;
                    case 2:
                        FirstMessage = GetLocaleString("PlayerStartedClumsyGame", u.FirstName);
                        _joinMsgId = Program.Bot.SendDocumentAsync(chatid, new FileToSend(GetRandomImage(ClumsyGame)), FirstMessage, replyMarkup: _joinButton).Result.MessageId;
                        break;
                    case 3:
                        FirstMessage = GetLocaleString("PlayerStartedSuperChaosGame", u.FirstName);
                        _joinMsgId = Program.Bot.SendDocumentAsync(chatid, new FileToSend(GetRandomImage(SuperChaos)), FirstMessage, replyMarkup: _joinButton).Result.MessageId;
                        break;
                    case 5:
                        FirstMessage = GetLocaleString("PlayerStartedRankedGame", u.FirstName);
                        _joinMsgId = Program.Bot.SendDocumentAsync(chatid, new FileToSend(GetRandomImage(StartGame)), FirstMessage, replyMarkup: _joinButton).Result.MessageId;
                        break;
                    case 6:
                        FirstMessage = GetLocaleString("PlayerStartedInverted", u.FirstName);
                        _joinMsgId = Program.Bot.SendDocumentAsync(chatid, new FileToSend(GetRandomImage(StartInverted)), FirstMessage, replyMarkup: _joinButton).Result.MessageId;
                        break;
                    default:
                        break;
                }

                //let's keep this on for a while, then we will delete it
                //SendWithQueue(GetLocaleString("NoAutoJoin", u.Username != null ? ("@" + u.Username) : u.FirstName.ToBold()));
                SendPlayerList(true);
                if (gameMode == 4)
                {
                    SendRoleList(true);
                }

                new Thread(GameTimer).Start();

            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                Program.Send("Hmm.. something went wrong, please try starting the game again...\n" + ex.Message, chatid);
                //Send(ex.StackTrace);
                Program.Send(ex.StackTrace, chatid);
                Send(
                    Program.Version.FileVersion +
                    $"\nGroup: {ChatId} ({ChatGroup})\nLanguage: {DbGroup?.Language ?? "null"}\n{Program.ClientId}\n{ex.Message}\n{ex.StackTrace}",
                    Program.ErrorGroup);
                Program.RemoveGame(this);
            }

        }

        
        #endregion

        #region Language Helpers
        /// <summary>
        /// Caches the language file in the instance
        /// </summary>
        /// <param name="language">The language filename to load</param>
        public void LoadLanguage(string language)
        {
            try
            {
                var files = Directory.GetFiles(Program.LanguageDirectory);
                var file = files.First(x => Path.GetFileNameWithoutExtension(x) == language);
                {
                    var doc = XDocument.Load(file);
                    Locale = new Locale
                    {
                        Language = Path.GetFileNameWithoutExtension(file),
                        File = doc
                    };
                }
                Language = Locale.Language;
            }
            catch
            {
                if (language != "English")
                    LoadLanguage("English");
            }
        }
        /// <summary>
        /// Gets the matching language string and formats it with parameters
        /// </summary>
        /// <param name="key">The XML Key of the string needed</param>
        /// <param name="args">Any arguments to fill the strings {0} {n}</param>
        /// <returns></returns>
        private string GetLocaleString(string key, params object[] args)
        {
            try
            {
                var strings = Locale.File.Descendants("string").FirstOrDefault(x => x.Attribute("key")?.Value == key) ??
                              Program.English.Descendants("string").FirstOrDefault(x => x.Attribute("key")?.Value == key);
                if (strings != null)
                {
                    var values = strings.Descendants("value");
                    var choice = Program.R.Next(values.Count());
                    var selected = values.ElementAt(choice).Value;

                    //disable bluetexting /join!
                    if (selected.ToLower().Contains("/join"))
                        throw new Exception("/join found in the string, using the English file.");

                    return String.Format(selected.FormatHTML(), args).Replace("\\n", Environment.NewLine);
                }
                else
                {
                    throw new Exception($"Error getting string {key} with parameters {(args != null && args.Length > 0 ? args.Aggregate((a, b) => a + "," + b.ToString()) : "none")}");
                }
            }
            catch (Exception e)
            {
                try
                {
                    //try the english string to be sure
                    var strings =
                        Program.English.Descendants("string").FirstOrDefault(x => x.Attribute("key")?.Value == key);
                    var values = strings?.Descendants("value");
                    if (values != null)
                    {
                        var choice = Program.R.Next(values.Count());
                        var selected = values.ElementAt(choice).Value;
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return String.Format(selected.FormatHTML(), args).Replace("\\n", Environment.NewLine);
                    }
                    else
                        throw new Exception("Cannot load english string for fallback");
                }
                catch
                {
                    throw new Exception(
                        $"Error getting string {key} with parameters {(args != null && args.Length > 0 ? args.Aggregate((a, b) => a + "," + b.ToString()) : "none")}",
                        e);
                }
            }
        }
#endregion

#region Main bits
        /// <summary>
        /// The main timer for the game
        /// </summary>
        private void GameTimer()
        {
            try
            {
                //Send($"{Settings.GameJoinTime} seconds to join!");
                //start with the joining time
                var count = Players.Count;
                int secondsElapsed = 0;
                var i = 0;
                var reducedOnce = false;
                while (i < Settings.GameJoinTime)
                {
                    if (Players == null) //killed extra game
                        return;

                    if (KillTimer) //forcestart
                    {
                        KillTimer = false;
                        break;
                    }

                    _requestPlayerListUpdate = true;

                    if (count != Players.Count) //if a player joined, add time
                    {
                        i = Math.Min(i, Math.Max(1700, i - 30));
                        count = Players.Count;
                    }

                    if (secondsElapsed++ % 30 == 0 && _joined.Any()) //every 30 seconds, say in group who have joined
                    {
                        SendWithQueue(GetLocaleString("HaveJoined", _joined.Aggregate("", (cur, p) => cur + p.GetRankName(gameMode) + (ShowIDs ? $" (ID: <code>{p.TeleUser.Id}</code>)\n" : ", ")).TrimEnd(',', ' ') + (ShowIDs ? "" : " ")));
                        _joined.Clear();
                    }

                    try
                    {
                        Telegram.Bot.Types.Message r = null;

                        var importantSeconds = new[] { 10, 30, 60, 120, 300, 600, 900, 1200 }; //notify when time is running out
                        foreach (var s in importantSeconds)
                        {
                            if (i == Settings.GameJoinTime - s)
                            {
                                var str = GetLocaleString("MinuteLeftToJoin");
                                if (s > 60)
                                {
                                    var m = s / 60;
                                    str = GetLocaleString("MinutesLeftToJoin", m.ToString().ToBold());
                                }
                                else if (s < 60)
                                {
                                    str = GetLocaleString("SecondsLeftToJoin", s.ToString().ToBold());
                                }
                                r = Program.Bot.SendTextMessageAsync(ChatId, str, parseMode: ParseMode.Html, replyMarkup: _joinButton).Result;
                                break;
                            }
                        }

                        if (_secondsToAdd != 0)
                        {
                            i -= _secondsToAdd; //Math.Max(i - _secondsToAdd, Settings.GameJoinTime - Settings.MaxJoinTime);

                            if (Settings.GameJoinTime > i)
                                r = Program.Bot.SendTextMessageAsync(
                                    ChatId,
                                    GetLocaleString(
                                        _secondsToAdd > 0 ? "SecondsAdded" : "SecondsRemoved",
                                        Math.Abs(_secondsToAdd).ToString().ToBold(),
                                        TimeSpan.FromSeconds(Settings.GameJoinTime - i).ToString(@"mm\:ss").ToBold()
                                    ), parseMode: ParseMode.Html, replyMarkup: _joinButton
                                ).Result;

                            _secondsToAdd = 0;
                        }
                        if (r != null)
                        {
                            _joinButtons.Add(r.MessageId);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    i++;
                    if (Players.Count > 4 && i < 1620 && !reducedOnce)
                    {
                        i = 1680;
                        reducedOnce = true;
                    }
                    Thread.Sleep(1000);
                }
                Program.Bot.EditMessageCaptionAsync(ChatId, _joinMsgId, FirstMessage, null);
                IsJoining = false;
                IsInitializing = true;

                Thread.Sleep(2000); //wait for last second joins
                CleanupButtons();
                //check we have enough players...
                if (Players.Count < Settings.MinPlayers)
                {
                    SendWithQueue(GetLocaleString("NotEnoughPlayers"));

                    Program.RemoveGame(this);
                    return;
                }

                SendWithQueue(GetLocaleString("StartingGameWait"));
                if (gameMode == 4)
                {
                    SendRoleList();
                }
                _playerListChanged = true;

#if !BETA
                /*if (Players.Count(x => x.GifPack?.Approved ?? false) > 0)
                {
                    var cMsg = "Players with custom gif packs:\n";
                    var customs = Players.Where(x => x.GifPack?.Approved ?? false);
                    if (!AllowNSFW)
                        customs = customs.Where(x => !x.GifPack.NSFW);
                    if (customs.Any(x => x.GifPack.CultWins != null))
                        CultWins = customs.Select(x => x.GifPack.CultWins).ToList();
                    if (customs.Any(x => x.GifPack.LoversWin != null))
                        LoversWin = customs.Select(x => x.GifPack.LoversWin).ToList();
                    if (customs.Any(x => x.GifPack.NoWinner != null))
                        NoWinner = customs.Select(x => x.GifPack.NoWinner).ToList();
                    if (customs.Any(x => x.GifPack.SerialKillerWins != null))
                        SerialKillerWins = customs.Select(x => x.GifPack.SerialKillerWins).ToList();
                    if (customs.Any(x => x.GifPack.TannerWin != null))
                        TannerWin = customs.Select(x => x.GifPack.TannerWin).ToList();
                    if (customs.Any(x => x.GifPack.VillagerDieImage != null))
                        VillagerDieImages = customs.Select(x => x.GifPack.VillagerDieImage).ToList();
                    if (customs.Any(x => x.GifPack.VillagersWin != null))
                        VillagersWin = customs.Select(x => x.GifPack.VillagersWin).ToList();
                    if (customs.Any(x => x.GifPack.WolfWin != null))
                        WolfWin = customs.Select(x => x.GifPack.WolfWin).ToList();
                    if (customs.Any(x => x.GifPack.WolvesWin != null))
                        WolvesWin = customs.Select(x => x.GifPack.WolvesWin).ToList();
                    foreach (var p in customs)
                    {
                        cMsg += p.GetName() + Environment.NewLine;
                    }
                    Send(cMsg);
                }*/
#endif
                //Program.Analytics.TrackAsync("gamestart", new { players = Players, playerCount = Players.Count(), mode = Chaos ? "Chaos" : "Normal" }, "0");
                IsRunning = true;
                AssignRoles();
                //create new game for database
                using (var db = new WWContext())
                {
                    _timeStarted = DateTime.Now;
                    var game = new Database.Game
                    {
                        GroupName = ChatGroup,
                        TimeStarted = _timeStarted,
                        GroupId = ChatId,
                        GrpId = int.Parse(DbGroup.Id.ToString()),
                        Mode = Chaos ? "Chaos" : "Normal"
                    };

                    db.SaveChanges();
                    db.Games.Add(game);
                    db.SaveChanges();

                    foreach (var p in Players)
                    {
                        //make sure they have DB entries
                        var dbp = db.Players.FirstOrDefault(x => x.TelegramId == p.Id);
                        if (dbp == null)
                        {
                            dbp = new Player { TelegramId = p.Id, Language = Language, Score = 1000, ShowRank = true };
                            db.Players.Add(dbp);
                            dbp.Score = 1000;
                            dbp.ShowRank = true;
                        }
                        dbp.Name = p.Name;
                        dbp.UserName = p.TeleUser.Username;
                        p.Score = dbp.Score;
                        p.ShowRank = dbp.ShowRank ?? true;

                        p.Language = dbp.Language;

                        db.SaveChanges();
                        if (gameMode != 4)
                        {
                            var gamePlayer = new GamePlayer
                            {
                                GameId = game.Id,
                                Survived = true,
                                Role = p.PlayerRole.ToString()
                            };
                            dbp.GamePlayers.Add(gamePlayer);
                            db.SaveChanges();
                        }

                        //new Task(() => { ImageHelper.GetUserImage(p.TeleUser.Id); }).Start();
                    }

                    GameId = db.Games.Where(x => x.GroupId == ChatId).OrderByDescending(x => x.Id).FirstOrDefault()?.Id ?? 0;

                    db.Database.ExecuteSqlCommand($"DELETE FROM NotifyGame WHERE GroupId = {ChatId}");
                }
                IsInitializing = false;

                //disable Inconspicuous achievement if less than 20 players
                if (Players.Count < 20)
                {
                    foreach (var p in Players.Where(x => x != null))
                        p.HasBeenVoted = true;
                }

                NotifyRoles();

                Time = GameTime.Night;
                while (IsRunning)
                {
                    GameDay++;
                    if (!IsRunning) break;
                    CheckRoleChanges();
                    CheckLongHaul();
                    NightCycle();
                    if (!IsRunning) break;
                    CheckRoleChanges();
                    CheckLongHaul();
                    DayCycle();
                    if (!IsRunning) break;
                    CheckRoleChanges();
                    CheckLongHaul();
                    LynchCycle();
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var msg = "";
                if (ex.InnerException != null)
                    msg += ex.InnerException.Message;
                foreach (var ves in ex.EntityValidationErrors)
                {
                    foreach (var ve in ves.ValidationErrors)
                    {
                        msg += $"{ves.Entry.Entity}:{ve.ErrorMessage}\n";
                    }
                }


                Send("Something just went terribly wrong, I had to cancel the game....");
                Send(
                    Program.Version.FileVersion +
                    $"\nGroup: {ChatId} ({ChatGroup})\nLanguage: {DbGroup?.Language ?? "null"}\n{Program.ClientId}\n{ex.Message}\n{msg}\n{ex.StackTrace}",
                    Program.ErrorGroup);
            }
            catch (Exception ex)
            {
                LogAllExceptions(ex);
                Send("Something just went terribly wrong, I had to cancel the game.\n" + ex.Message);
                Send(ex.StackTrace);
                //this would be a duplicate
                //Send(Program.Version.FileVersion + $"\nGroup: {ChatId} ({ChatGroup})\nLanguage: {DbGroup?.Language ?? "null"}\n{Program.ClientId}\n{ex.Message}\n{ex.StackTrace}", Program.ErrorGroup);

            }
            finally
            {
                Program.RemoveGame(this);
            }

        }
        /// <summary>
        /// Add (Join) a player to the game
        /// </summary>
        /// <param name="u">Telegram user who is joining</param>
        /// <param name="notify">Should we announce the join?</param>
        public void AddPlayer(User u, bool notify = true, bool n = false)
        {
            try
            {
                if (!IsJoining || IsInitializing)
                {
                    //SendWithQueue(GetLocaleString("NoJoinGameRunning"));
                    return;
                }
                //first check the player hasn't already joined
                if (Players.Any(x => x.Id == u.Id))
                {
                    //SendWithQueue(GetLocaleString("AlreadyJoined"));
                    return;
                }


                var p = new IPlayer
                {
                    TeleUser = u,
                    HasPM = false,
                    Name = $"{u.FirstName} {u.LastName}"
                };
                p.Name = p.Name.Replace("\n", "").Trim();
                p.Id = p.TeleUser.Id;
                //if ()
                //{
                //    var dbuser = GetDBPlayer(p);
                //    if (dbuser != null)
                //    {
                //        dbuser.Banned = true;
                //        DB.SaveChanges();
                //        Send(GetLocaleString("BannedForExploit", p.Name));
                //    }
                //}
                if (p.Name.StartsWith("/") || String.IsNullOrEmpty(p.Name) || p.Name.Trim().ToLower() == "skip" || p.Name.Trim().ToLower() == GetLocaleString("Skip").ToLower())
                {
                    SendWithQueue(GetLocaleString("ChangeNameToJoin",
                        String.IsNullOrWhiteSpace(u.Username) ? u.FirstName + " " + u.LastName : "@" + u.Username));
                    return;
                }
                if (Players.Any(x => x.Name == p.Name))
                {
                    SendWithQueue(GetLocaleString("NameExists", p.GetName(), p.TeleUser.Username));
                    return;
                }
                if (Players.Count >= Settings.MaxPlayers)
                {
                    SendWithQueue(GetLocaleString("PlayerLimitReached"));
                    return;
                }
                //check one more time
                if (IsInitializing || !IsJoining) return;
                //add player
                if (n)
                {
                    if (u.Id == Jiro)
                    {
                        Send("¡Jiro se ha unido a una partida!", Hela);
                    }
                    if (u.Id == Hela)
                    {
                        Send("¡Hela se ha unido a una partida!", Jiro);

                    }
                    if (u.Id == Lara)
                    {
                        Send("¡Lara se ha unido a una partida!", Alex);

                    }
                    if (u.Id == Alex)
                    {
                        Send("¡Alex se ha unido a una partida!", Lara);

                    }
                }

                using (var db = new WWContext())
                {
                    var user = db.Players.FirstOrDefault(x => x.TelegramId == u.Id);
                    if (user == null)
                    {
                        user = new Player
                        {
                            TelegramId = u.Id,
                            Language = "English",
                            HasPM = false,
                            HasPM2 = false,
                            HasDebugPM = false,
                            Score = 1000,
                            ShowRank = true
                        };
                        db.Players.Add(user);
                    }
                    p.Score = user.Score;
                    p.ShowRank = user.ShowRank ?? true;
                    p.DonationLevel = user.DonationLevel ?? 0;
                    p.Founder = user.Founder ?? false;
                    user.UserName = u.Username;
                    user.Name = $"{u.FirstName} {u.LastName}".Trim();
                    if (!String.IsNullOrEmpty(user.CustomGifSet))
                        p.GifPack = JsonConvert.DeserializeObject<CustomGifData>(user.CustomGifSet);

                    db.SaveChanges();

                    var botname = "@" + Program.Me.Username;
                }

                Players.Add(p);
                _joined.Add(p);

                var groupname = String.IsNullOrWhiteSpace(DbGroup.GroupLink) ? ChatGroup : $"<a href=\"{DbGroup.GroupLink}\">{ChatGroup.FormatHTML()}</a>";
                Send(GetLocaleString("YouJoined", groupname), p.Id);

                //now, attempt to PM the player
                //try
                //{
                // ReSharper disable once UnusedVariable
                //var result = Send(GetLocaleString("YouJoined", ChatGroup.FormatHTML()), u.Id).Result;
                //}
                //catch (Exception)
                //{
                //var botname = "@" + Program.Me.Username;
                //if (!sendPM)
                //    msg = GetLocaleString("PMTheBot", p.GetName(), botname);
                ////unable to PM
                //sendPM = true;
                //}

                //SendWithQueue(msg, requestPM: sendPM);

                //if (sendPM) //don't allow them to join
                //{
                //    Players.Remove(p);
                //}
                _playerListChanged = true;
                if (gameMode == 4)
                {
                    if (Players.Count == customList.Count)
                    {
                        KillTimer = true;
                    }
                }
                else
                {
                    if (Players.Count == (DbGroup.MaxPlayers ?? Settings.MaxPlayers))
                        KillTimer = true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in AddPlayer: {e.Message}");
            }
        }
        /// <summary>
        /// Removes (flees) player from the game
        /// </summary>
        /// <param name="u">The telegram user to remove</param>
        public void RemovePlayer(User u)
        {

            try
            {
                if (IsInitializing) throw new Exception("Cannot flee while game is initializing.  Try again once game is done starting.");
                if (!DbGroup.HasFlag(GroupConfig.AllowFlee) && !IsJoining && IsRunning)
                {
                    SendWithQueue(GetLocaleString("FleeDisabled"));
                    return;
                }
                var p = Players.FirstOrDefault(x => x.Id == u.Id);
                if (p == null) return;
                if (p.IsDead)
                {
                    SendWithQueue(GetLocaleString("DeadFlee"));
                    return;
                }

                SendWithQueue(GetLocaleString("Flee", p.GetRankName(gameMode)));
                if (IsRunning)
                {
                    //kill the player
                    p.IsDead = true;
                    p.TimeDied = DateTime.Now;
                    p.Fled = true;
                    p.DayOfDeath = GameDay;
                    SomeoneSmited = true;
                    SendWithQueue(GetLocaleString("PlayerRoleWas", p.GetName(), GetDescription(p)));

                   /* if (DbGroup.HasFlag(GroupConfig.ShowRolesDeath))
                        SendWithQueue(GetLocaleString("PlayerRoleWas", p.GetName(), GetDescription(p.PlayerRole)));*/

                    CheckRoleChanges();
                    //add the 'kill'
                    DBKill(p, p, KillMthd.Flee);
                    CheckForGameEnd();
                }
                else if (IsJoining && !IsInitializing)// really, should never be both joining and initializing but....
                {
                    Players.Remove(p);
                    _playerListChanged = true;
                    SendWithQueue(GetLocaleString("CountPlayersRemain", Players.Count.ToBold()));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in RemovePlayer: {e.Message}");
            }
        }
#endregion

#region Communications
        public void HandleReply(CallbackQuery query)
        {
            try
            {
                //first off, what was I asking them?
                var args = query.Data.Split('|');
                //0 - vote
                //1 - clientid
                //2 - choiceid
                var player = Players.FirstOrDefault(x => x.Id == query.From.Id);

                if (player == null) return;

                if (player.PlayerRole == IRole.Mayor && args[2] == "reveal" && player.HasUsedAbility == false)
                {
                    player.HasUsedAbility = true;
                    SendWithQueue(GetLocaleString("MayorReveal", player.GetName()));

                    Program.MessagesSent++;
                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted"));

                    return;
                }
                else if (player.PlayerRole == IRole.Mayor && args[2] == "reveal" && player.HasUsedAbility == true)
                    return;

                if (player.PlayerRole == IRole.Pacifist && args[2] == "peace" && player.HasUsedAbility == false)
                {
                    player.HasUsedAbility = true;
                    _pacifistUsed = true;
                    SendWithQueue(GetLocaleString("PacifistNoLynch", player.GetName()));

                    Program.MessagesSent++;
                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted"));
                    return;
                }
                else if (player.PlayerRole == IRole.Pacifist && args[2] == "peace" && player.HasUsedAbility == true)
                    return;

                if (player.PlayerRole == IRole.SpeedWolf && args[2] == "runaway" && player.HasUsedAbility == false)
                {
                    player.HasUsedAbility = true;
                    player.RanAway = true;
                    SendWithQueue(GetLocaleString("SpeedWolfRunning", player.GetName()));

                    Program.MessagesSent++;
                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted"));
                    return;
                }
                else if (player.PlayerRole == IRole.SpeedWolf && args[2] == "runaway" && player.HasUsedAbility == true)
                    return;

                if (player.PlayerRole == IRole.Blacksmith && player.CurrentQuestion.QType == QuestionType.SpreadSilver)
                {
                    if (args[2] == "yes")
                    {
                        player.HasUsedAbility = true;
                        _silverSpread = true;
                        SendWithQueue(GetLocaleString("BlacksmithSpreadSilver", player.GetName()));
                    }

                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted"));
                    player.CurrentQuestion = null;
                    return;
                }

                if (player.PlayerRole == IRole.Sandman && player.CurrentQuestion.QType == QuestionType.Sandman)
                {
                    if (args[2] == "yes")
                    {
                        player.HasUsedAbility = true;
                        _sandmanSleep = true;
                        SendWithQueue(GetLocaleString("SandmanSleepAll"));
                    }

                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted"));
                    player.CurrentQuestion = null;
                    return;
                }

                if (player.PlayerRole == IRole.Herbalist && player.CurrentQuestion.QType == QuestionType.Cure)
                {
                    if (args[2] == "yes")
                    {
                        player.Choice = 1;
                    }

                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted"));
                    player.CurrentQuestion = null;
                    return;
                }

                if (player.PlayerRole == IRole.Ninja && player.CurrentQuestion.QType == QuestionType.Hide)
                {
                    if (args[2] == "yes")
                    {
                        player.Choice = 1;
                    }

                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted"));
                    player.CurrentQuestion = null;
                    return;
                }

                if (player.CurrentQuestion == null)
                {
                    return;
                }

                if (query.Data == null)
                {
                    throw new NullReferenceException("Object was null: query.Data");
                }

                if (args[2] == "-1")
                {
                    if (player.CurrentQuestion.QType == QuestionType.Kill2)
                        player.Choice2 = -1;           
                    else
                        player.Choice = -1;
                    if (player.CurrentQuestion.QType == QuestionType.Lynch)
                        player.Choice = -2;
                    Program.MessagesSent++;
                    ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted") + $" - {GetLocaleString("Skip")}");
                    player.CurrentQuestion = null;
                    return;
                }


                if (player.CurrentQuestion.QType == QuestionType.Kill2)
                    player.Choice2 = int.Parse(args[2]);
                else
                    player.Choice = int.Parse(args[2]);

                if (player.PlayerRole == IRole.ClumsyGuy && player.CurrentQuestion.QType == QuestionType.Lynch)
                {
                    if (Program.R.Next(100) < 50)
                    {
                        //pick a random target
                        var clumsy = ChooseRandomPlayerId(player, false);
                        if (clumsy == player.Choice) player.ClumsyCorrectLynchCount++;
                        player.Choice = clumsy;
                    }
                    else
                    {
                        player.ClumsyCorrectLynchCount++;
                    }
                }


                var target = Players.FirstOrDefault(x => player.CurrentQuestion.QType == QuestionType.Kill2 ? x.Id == player.Choice2 : x.Id == player.Choice);
                if (target == null)
                {
                    Send(GetLocaleString("NoPlayerName"), query.From.Id);
                    return;
                }

                if (player.PlayerRole == IRole.SnowWolf && player.CurrentQuestion.QType == QuestionType.Freeze)
                {
                    var others = Players.GetPlayersForRoles(AllWolves, exceptPlayer: player);
                    foreach (var w in others)
                    {
                        Send(GetLocaleString("WolfVotedFreeze", player.GetName(), target.GetName()), w.Id);
                    }
                }

                if (player.PlayerRole == IRole.Snooper && player.CurrentQuestion.QType == QuestionType.Snoop)
                {
                    var others = Players.GetPlayersForRoles(AllWolves, exceptPlayer: player);
                    foreach (var w in others)
                    {
                        Send(GetLocaleString("WolfVotedSnoop", player.GetName(), target.GetName()), w.Id);
                    }
                }

                if (WolfRoles.Contains(player.PlayerRole) && player.CurrentQuestion.QType == QuestionType.Kill2)
                {
                    var others = Players.GetPlayersForRoles(AllWolves, exceptPlayer: player);
                    foreach (var w in others)
                    {
                        Send(GetLocaleString("WolfVotedKill", player.GetName(), target.GetName()), w.Id);
                    }
                }

                var clearCurrent = true;
                if (WolfRoles.Contains(player.PlayerRole) && player.CurrentQuestion.QType == QuestionType.Kill)
                {
                    var others = Players.GetPlayersForRoles(AllWolves, exceptPlayer: player);
                    foreach (var w in others)
                    {
                        Send(GetLocaleString("WolfVotedKill", player.GetName(), target.GetName()), w.Id);
                    }
                    if (WolfCubKilled)
                    {
                        //need to let them have another menu for second kill
                        var targets = Players.Where(x => !AllWolves.Contains(x.PlayerRole) & !x.IsDead && x.Id != player.Choice).ToList();
                        var msg = GetLocaleString("AskEat");
                        var qtype = QuestionType.Kill2;
                        var buttons = targets.Select(x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId}|{x.Id}") }).ToList();
                        buttons.Add(new[] { new InlineKeyboardCallbackButton(GetLocaleString("Skip"), $"vote|{Program.ClientId}|-1") });
                        SendMenu(buttons, player, msg, qtype);
                        clearCurrent = false;
                    }
                }
                if (player.PlayerRole == IRole.WildChild && player.CurrentQuestion.QType == QuestionType.RoleModel)
                {
                    player.RoleModel = target.Id;
                    player.Choice = -1;
                }
                if (player.PlayerRole == IRole.Cupid && player.CurrentQuestion.QType == QuestionType.Lover1)
                {
                    var lover1 = Players.FirstOrDefault(x => x.Id == player.Choice);

                    if (lover1 != null)
                    {
                        if (lover1.Id == player.Id)
                            AddAchievement(player, Achievements.SelfLoving);
                        lover1.InLove = true;
                        //send menu for second choice....
                        var secondChoices = Players.Where(x => !x.IsDead && x.Id != lover1.Id).ToList();
                        var buttons =
                            secondChoices.Select(
                                x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId}|{x.Id}") }).ToList();
                        player.Choice = 0;
                        Program.MessagesSent++;
                        ReplyToCallback(query,
                            GetLocaleString("ChoiceAccepted") + " - " + target.Name);

                        SendMenu(buttons, player, GetLocaleString("AskCupid2"), QuestionType.Lover2);
                    }
                    return;
                }
                if (player.PlayerRole == IRole.Cupid && player.CurrentQuestion.QType == QuestionType.Lover2)
                {
                    var lover11 = Players.FirstOrDefault(x => x.InLove);
                    if (lover11 == null)
                        return;
                    if (lover11.Id == player.Id)
                        AddAchievement(player, Achievements.SelfLoving);
                    lover11.LoverId = player.Choice;
                    lover11.InLove = true;

                    var id = int.Parse(lover11.Id.ToString());
                    var lover2 = Players.FirstOrDefault(x => x.Id == player.Choice);
                    if (lover2 == null)
                        return;
                    lover2.InLove = true;
                    lover2.LoverId = id;
                    player.Choice = -1;
                }

                if (player.PlayerRole == IRole.Doppelgänger && player.CurrentQuestion.QType == QuestionType.RoleModel)
                {
                    player.RoleModel = target.Id;
                    player.Choice = -1;
                }

                if (player.PlayerRole == IRole.Imposter && player.CurrentQuestion.QType == QuestionType.RoleModel)
                {
                    player.RoleModel = target.Id;
                    player.Choice = -1;
                    Send(GetLocaleString("ImposterSees", GetDescription(target)), player.Id);
                }

                if (player.PlayerRole == IRole.Seer && player.CurrentQuestion.QType == QuestionType.See)
                {
                    if (target != null)
                    {
                        DBAction(player, target, "See");
                        var role = target.PlayerRole;
                        if (BadRoles.Contains(target.PlayerRole))
                        {
                            player.PointsToAdd += 1;
                        }
                        switch (role)
                        {
                            case IRole.Beholder:
                                AddAchievement(player, Achievements.ShouldHaveKnown);
                                break;
                            case IRole.Imposter:
                                var rm = Players.FirstOrDefault(x => x.Id == target.RoleModel);
                                role = rm.PlayerRole;
                                break;
                            case IRole.Traitor:
                                role = Program.R.Next(100) > 50 ? IRole.Wolf : IRole.Villager;
                                break;
                            case IRole.WolfCub: //seer doesn't see wolf type
                            case IRole.AlphaWolf:
                            case IRole.HungryWolf:
                            case IRole.WolfMan: //poor wolf man, is just a villager!
                            case IRole.RabidWolf:
                            case IRole.SpeedWolf:
                            case IRole.HowlingWolf:
                                role = IRole.Wolf;
                                break;
                            case IRole.Lycan: //sneaky wuff
                            case IRole.Sleepwalker:
                                role = IRole.Villager;
                                break;
                        }
                        Send(GetLocaleString("SeerSees", target.GetName(), GetRoleDescription(role)), player.Id);
                    }
                }

                if (player.PlayerRole == IRole.Sorcerer && player.CurrentQuestion.QType == QuestionType.See)
                {
                    if (target != null)
                    {
                        DBAction(player, target, "See");
                        var role = target.PlayerRole;
                        switch (role)
                        {
                            case IRole.AlphaWolf:
                            case IRole.Wolf:
                            case IRole.WolfCub:
                            case IRole.Lycan:
                            case IRole.HungryWolf:
                            case IRole.RabidWolf:
                            case IRole.SpeedWolf:
                            case IRole.HowlingWolf:
                                Send(GetLocaleString("SeerSees", target.GetName(), GetRoleDescription(IRole.Wolf)), player.Id);
                                break;
                            case IRole.Sleepwalker:
                                Send(GetLocaleString("SeerSees", target.GetName(), GetRoleDescription(IRole.Villager)), player.Id);
                                break;
                            default:
                                Send(GetLocaleString("SeerSees", target.GetName(), GetRoleDescription(role)), player.Id);
                                break;
                        }
                    }
                }

                if (player.PlayerRole == IRole.Fool && player.CurrentQuestion.QType == QuestionType.See)
                {
                    if (target != null)
                    {
                        var possibleRoles = Players.Where(x => !x.IsDead && x.Id != player.Id && x.PlayerRole != IRole.Seer).Select(x => x.PlayerRole).ToList();
                        possibleRoles.Shuffle();
                        possibleRoles.Shuffle();
                        if (possibleRoles.Any())
                        {

                            //don't see wolf type!
                            if (WolfRoles.Contains(possibleRoles[0]))
                                possibleRoles[0] = IRole.Wolf;

                            //check if it's accurate
                            try
                            {
                                if (possibleRoles[0] == target.PlayerRole || (possibleRoles[0] == IRole.Wolf && WolfRoles.Contains(target.PlayerRole)))
                                    player.FoolCorrectSeeCount++;
                            }
                            catch
                            {
                                // ignored
                            }

                            Send(GetLocaleString("SeerSees", target.GetName(), GetRoleDescription(possibleRoles[0])), player.Id);
                        }
                    }
                }

                if (player.PlayerRole == IRole.Cultist && player.CurrentQuestion.QType == QuestionType.Convert)
                {
                    var others =
                        Players.Where(
                            x => !x.IsDead && x.PlayerRole == IRole.Cultist && x.Id != player.Id);
                    foreach (var w in others)
                    {
                        Send(GetLocaleString("CultistVotedConvert", player.GetName(), target.GetName()), w.Id);
                    }
                }


                if (player.CurrentQuestion.QType == QuestionType.Lynch)
                {

                    if (!DbGroup.HasFlag(GroupConfig.EnableSecretLynch))
                    {
                        var msg = GetLocaleString("PlayerVotedLynch", player.GetName(), target.GetName());
                        SendWithQueue(msg);
                    }
                    else
                    {
                        var msg = GetLocaleString("PlayerVoteCounts", Players.Count(x => !x.IsDead && x.Choice != 0), Players.Count(x => !x.IsDead));
                        SendWithQueue(msg);
                    }

                    if (NoOneCastLynch)
                    {
                        player.FirstStone++;
                        NoOneCastLynch = false;
                    }
                    //First Stone counter does not reset its value to 0
                    //else
                    //    player.FirstStone = 0;

                    if (player.FirstStone == 5)
                    {
                        AddAchievement(player, Achievements.FirstStone);
                    }
                }
                Program.MessagesSent++;
                ReplyToCallback(query,
                        GetLocaleString("ChoiceAccepted") + " - " + target.GetName(true));
                if (clearCurrent)
                    player.CurrentQuestion = null;
            }
            catch (Exception e)
            {
                //Send(e.Message, query.From.Id);
                Console.WriteLine($"Error in HandleReply: {e.Message} \n{query.From.FirstName} {query.From.LastName} (@{query.From.Username})\n{query.Data}");
            }
        }

        private Task<Telegram.Bot.Types.Message> Send(string message, long id = 0, bool clearKeyboard = false, InlineKeyboardMarkup menu = null, bool notify = false)
        {
            if (id == 0)
                id = ChatId;
            return Program.Send(message, id, clearKeyboard, menu, game: this, notify: notify);
        }

        private void SendGif(string text, string image, long id = 0)
        {
            Program.MessagesSent++;
            if (id == 0)
                id = ChatId;
            //Log.WriteLine($"{id} -> {image} {text}");
            //Send(text, id);
            Program.Bot.SendDocumentAsync(id, new FileToSend(image), text);
        }

        private void SendWithQueue(string text, string gif = null, bool requestPM = false)
        {
            _messageQueue.Enqueue(new Message(text, gif, requestPM));
        }

        private void SendWithQueue(Message m)
        {
            _messageQueue.Enqueue(m);
        }



        //TODO move the message queue elsewhere
        class Message
        {
            public string Msg { get; }
            public string GifId { get; }

            public bool RequestPM { get; }
            public bool PlayerList { get; set; }
            public bool Joining { get; set; } = false;
            public bool Notify { get; set; }

            public Message(string msg, string gifid = null, bool requestPM = false, bool notify = false)
            {
                Msg = msg;
                GifId = gifid;
                RequestPM = requestPM;
                Notify = notify;
            }
        }

        private readonly Queue<Message> _messageQueue = new Queue<Message>();
        private bool _requestPlayerListUpdate = false;
        private void GroupQueue()
        {
            string final;
            while (MessageQueueing)
            {
                if (_requestPlayerListUpdate)
                {
                    SendPlayerList(true);
                    _requestPlayerListUpdate = false;
                }
                final = "";
                bool requestPM = false;
                bool byteMax = false;
                bool pList = false;
                var i = 0;
                while (_messageQueue.Count > 0 && !byteMax)
                {

                    i++;
                    var m = _messageQueue.Peek();

                    if (m.Joining)
                    {
                        _messageQueue.Dequeue();
                        if (_playerListId == 0)
                        {
                            try
                            {
                                _playerListId = Send(m.Msg).Result.MessageId;
                            }
                            catch
                            {
                                //ignored
                            }
                        }
                        else
                            Program.Bot.EditMessageTextAsync(ChatId, _playerListId, m.Msg, ParseMode.Html, disableWebPagePreview: true);
                        continue;
                    }

                    if (!String.IsNullOrEmpty(m.GifId))
                    {
                        if (!String.IsNullOrEmpty(final))
                            Send(final);
                        Thread.Sleep(500);

                        _messageQueue.Dequeue();
                        SendGif(m.Msg, m.GifId);
                        Thread.Sleep(500);
                        final = "";
                        /*var temp = final + m.Msg + Environment.NewLine + Environment.NewLine;
                        if (Encoding.UTF8.GetByteCount(temp) > 512 && i > 1)
                        {
                            byteMax = true; //break and send
                        }
                        else
                        {
                            _messageQueue.Dequeue(); //remove the message, we are sending it.
                            final += m.Msg + Environment.NewLine + Environment.NewLine;
                            if (m.RequestPM)
                                requestPM = true;
                            if (m.PlayerList)
                                pList = true;

                        }*/

                    }
                    else
                    {
                        var temp = final + m.Msg + Environment.NewLine + Environment.NewLine;
                        if ((Encoding.UTF8.GetByteCount(temp) > 512 && i > 1))
                        {
                            byteMax = true; //break and send
                        }
                        else
                        {
                            _messageQueue.Dequeue(); //remove the message, we are sending it.
                            final += m.Msg + Environment.NewLine + Environment.NewLine;
                            if (m.RequestPM)
                                requestPM = true;
                            if (m.PlayerList)
                                pList = true;
                        }

                    }
                }
                if (!String.IsNullOrEmpty(final))
                {
                    if (requestPM)
                    {
                        Send(final, 0, false, _requestPMButton);
                    }
                    else
                    {
                        if (pList)
                        {
                            try
                            {
                                var result = Send(final).Result;
                                _playerListId = result.MessageId;
                            }
                            catch
                            {
                                _playerListId = 0;
                            }
                        }
                        else
                            Send(final, notify: true);
                    }

                }
                Thread.Sleep(4000);

            }
            //do one last send
            final = "";
            while (_messageQueue.Count > 0)
            {
                var m = _messageQueue.Dequeue();
                if (m.GifId != null)
                {
                    if (!String.IsNullOrEmpty(final))
                        Send(final);
                    Thread.Sleep(500);
                    SendGif(m.Msg, m.GifId);
                    Thread.Sleep(500);
                    final = "";
                }
                else
                {
                    final += m.Msg + Environment.NewLine;
                }

            }
            if (!String.IsNullOrEmpty(final))
                Send(final);
        }
        
        private void SendPlayerList(bool joining = false)
        {
            if (!_playerListChanged) return;
            if (Players == null) return;
            try
            {
                new Thread(() =>
                {
                    var msg = "";
                    if (joining)
                    {
                        msg = $"#players: {Players.Count}\n" +
                        Players.Aggregate("", (current, p) => current + ($"{p.GetRankName(gameMode)}\n"));
                    }
                    else
                    {
                        //Thread.Sleep(4500); //wait a moment before sending
                        LastPlayersOutput = DateTime.Now;
                        msg =
                            $"{GetLocaleString("PlayersAlive")}: {Players.Count(x => !x.IsDead)}/{Players.Count}\n" +
                            Players.OrderBy(x => x.TimeDied)
                                .Aggregate("",
                                    (current, p) =>
                                        current +
                                        ($"{p.GetName(dead: p.IsDead)}: {(p.IsDead ? ((p.Fled ? GetLocaleString("RanAway") : GetLocaleString("Dead")) + (/*DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ?*/ " - " + GetDeathDescription(p) + (p.InLove ? "❤️" : "") /*: ""*/)) : GetLocaleString("Alive"))}\n"));
                        //{(p.HasUsedAbility & !p.IsDead && new[] { IRole.Prince, IRole.Mayor, IRole.Gunner, IRole.Blacksmith }.Contains(p.PlayerRole) ? " - " + GetDescription(p.PlayerRole) : "")}  //OLD CODE SHOWING KNOWN ROLES
                       
                    }
                    _playerListChanged = false;
                    SendWithQueue(new Message(msg) { PlayerList = true, Joining = joining });

                }).Start();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void SendWolfList(IPlayer p, bool turning = false)
        {
            var wolves = Players.Where(x => (WolfRoles.Contains(x.PlayerRole) || SupportWolves.Contains(x.PlayerRole)) && !x.IsDead);
            var amnesia = false;
            var amnesiaString = GetLocaleString("IsThisAmnesia");
            if (amnesiaString != null)
            {
                if (amnesiaString == "Yes")
                    amnesia = true;
            }
            if (wolves.Count() <= 1 && !turning) return;
            var msg = GetLocaleString("WolfList").ToBold();
            msg += "\n";
            if (amnesia)
            {
                foreach (var ww in wolves)
                {
                    msg += "- " + ww.GetName() + "\n";
                }
            }
            else
            {
                foreach (var ww in wolves)
                {
                    msg += "- " + ww.GetName() + " " + GetLocaleString(ww.PlayerRole.ToString()).ToBold() + "\n";
                }
            }
            Send(msg, p.Id);
        }

        public void OutputPlayers()
        {
            if (!((DateTime.Now - LastPlayersOutput).TotalSeconds > (10))) return;
            LastPlayersOutput = DateTime.Now;
            Program.Bot.SendTextMessageAsync(ChatId, GetLocaleString(_playerListId != 0 ? "LatestList" : "UnableToGetList"), parseMode: ParseMode.Html, replyToMessageId: _playerListId);
        }

        public void OutputRoles()
        {
            if (!((DateTime.Now - LastRolesOutput).TotalSeconds > (10))) return;
            LastRolesOutput = DateTime.Now;
            if (gameMode == 4)
            {
                Program.Bot.SendTextMessageAsync(ChatId, GetLocaleString(_roleListId != 0 ? "LatestRoleList" : "UnableToGetRoleList"), parseMode: ParseMode.Html, replyToMessageId: _roleListId);
            }
        }

        public async void ShowJoinButton()
        {
            if (!IsJoining) return;
            if (!((DateTime.Now - LastJoinButtonShowed).TotalSeconds > (15))) return;
            LastJoinButtonShowed = DateTime.Now;
            try
            {
                var r = await Program.Bot.SendTextMessageAsync(ChatId, GetLocaleString("JoinByButton"), parseMode: ParseMode.Html, replyMarkup: _joinButton);
                _joinButtons.Add(r.MessageId);
            }
            catch
            {
                // ignored
            }

        }
#endregion

#region Roles
        string GetDescription(IPlayer p)
        {
            if (p.PlayerRole == IRole.Imposter)
            {
                var rm = Players.FirstOrDefault(x => x.Id == p.RoleModel);
                return GetLocaleString(rm.PlayerRole.ToString()).ToBold();
            }
            return GetLocaleString(p.PlayerRole.ToString()).ToBold();
        }

        string GetRoleDescription(IRole en)
        {
            return GetLocaleString(en.ToString()).ToBold();
        }

        string GetEndDescription(IPlayer p)
        {
            if (p.PlayerRole != p.OriginalRole)
            {
                if (p.PlayerRole == IRole.Wolf && p.OriginalRole == IRole.SnowWolf)
                    return GetRoleDescription(p.PlayerRole) + "(❄️)";
                else
                    return GetRoleDescription(p.PlayerRole) + "(" + p.OriginalRole.GetEmoji() + ")";
            }
            else
            {
                return GetRoleDescription(p.PlayerRole);
            }
        }

        string GetDeathDescription(IPlayer pl)
        {
            if (pl.Devoured)
            {
                return GetLocaleString("WasDevoured").ToBold();
            }
            else
            {
                if (pl.PlayerRole == IRole.Imposter)
                {
                    var rm = Players.FirstOrDefault(x => x.Id == pl.RoleModel);
                    return GetLocaleString(rm.PlayerRole.ToString()).ToBold();
                }
                return GetLocaleString(pl.PlayerRole.ToString()).ToBold();
            }
        }

        private List<IRole> PlayableRoleList()
        {
            List<IRole> lista = Enum.GetValues(typeof(IRole)).Cast<IRole>().ToList();
            lista.Remove(IRole.Thief);
            lista.Remove(IRole.Miner);
            // not implemented
            return lista;
        }

        private List<IRole> GetRoleList(int playerCount)
        {
            totalplayers = playerCount;
            var rolesToAssign = new List<IRole>();
            if (gameMode != 3)
            {
                int playersPerWolf = 4;
                //need to set the max wolves so game doesn't end immediately - 25% max wolf population
                //25% was too much, max it at 5 wolves.
                var possiblewolves = new List<IRole>() { IRole.Wolf, IRole.AlphaWolf, IRole.WolfCub, IRole.Lycan, IRole.HungryWolf, IRole.SpeedWolf, IRole.RabidWolf, IRole.HowlingWolf };
                if (playerCount < 7)
                {
                    possiblewolves.Remove(IRole.AlphaWolf);
                    possiblewolves.Remove(IRole.RabidWolf);
                    possiblewolves.Remove(IRole.SpeedWolf);
                }
                if (DisableVillager || StrongMode)
                    possiblewolves.Remove(IRole.Lycan);
                var wolftoadd = possiblewolves[Program.R.Next(possiblewolves.Count())];
                possiblewolves.Add(IRole.SnowWolf);
                possiblewolves.Add(IRole.Snooper);
                for (int i = 0; i < Math.Min(Math.Max(playerCount / playersPerWolf, 1), 10); i++)
                {

                    rolesToAssign.Add(wolftoadd);
                    if (wolftoadd != IRole.Wolf)
                        possiblewolves.Remove(wolftoadd);
                    wolftoadd = possiblewolves[Program.R.Next(possiblewolves.Count())];
                }
                //add remaining roles to 'card pile'
                var fullList = Enum.GetValues(typeof(IRole)).Cast<IRole>().ToList();
                if (StrongMode && gameMode != 5 && gameMode != 2)
                {
                    fullList.Remove(IRole.Villager);
                    fullList.Remove(IRole.Sleepwalker);
                    fullList.Remove(IRole.ClumsyGuy);
                    fullList.Remove(IRole.Baker);
                    fullList.Remove(IRole.Beholder);
                    fullList.Remove(IRole.Cupid);
                    fullList.Remove(IRole.WildChild);
                    fullList.Remove(IRole.Cursed);
                    fullList.Remove(IRole.Traitor);
                    fullList.Remove(IRole.WolfMan);
                }
                foreach (var role in fullList)
                {
                    switch (role)
                    {
                        case IRole.Wolf:
                        case IRole.Lycan:
                        case IRole.WolfCub:
                        case IRole.AlphaWolf:
                        case IRole.HungryWolf:
                        case IRole.RabidWolf:
                        case IRole.SnowWolf:
                        case IRole.Snooper:
                        case IRole.SpeedWolf:
                        case IRole.HowlingWolf:
                            break;
                        case IRole.WildChild:
                        case IRole.Sorcerer:
                        case IRole.Traitor:
                        case IRole.Cupid:
                        case IRole.Doppelgänger:
                        case IRole.Imposter:
                        case IRole.Survivor:
                        case IRole.Cursed:
                            if (playerCount > 6)
                                rolesToAssign.Add(role);
                            break;
                        case IRole.Harlot:
                        case IRole.Baker:
                            if (playerCount > 4)
                                rolesToAssign.Add(role);
                            break;
                        case IRole.CultistHunter:
                        case IRole.Atheist:
                            if (playerCount > 10)
                                rolesToAssign.Add(role);
                            break;
                        case IRole.Tanner:
                            if (playerCount > 6 && playerCount < 30 && AllowTanner)
                                rolesToAssign.Add(role);
                            break;
                        case IRole.Fool:
                            if (AllowFool)
                                rolesToAssign.Add(role);
                            break;
                        case IRole.Villager:
                        case IRole.Sleepwalker:
                            if (!DisableVillager || gameMode == 5 || gameMode == 2)
                                rolesToAssign.Add(role);
                            break;
                        case IRole.Cultist:
                            // we'll add them later...
                            break;
                        case IRole.Thief:
                        case IRole.Miner:
                            //not implemented
                            break;
                        default:
                            rolesToAssign.Add(role);
                            break;
                    }
                }

                //add a couple more masons
                rolesToAssign.Add(IRole.Mason);
                rolesToAssign.Add(IRole.Mason);
                if (playerCount > 30) rolesToAssign.Add(IRole.Mason);
                //for smaller games, all roles will be available and chosen randomly.  For large games, it will be about the
                //same as it was before....


                if (rolesToAssign.Any(x => x == IRole.CultistHunter))
                {
                    rolesToAssign.Add(IRole.Cultist);
                    rolesToAssign.Add(IRole.Cultist);
                    rolesToAssign.Add(IRole.Cultist);
                    if (playerCount > 40) rolesToAssign.Add(IRole.Cultist);
                    if (playerCount > 70) rolesToAssign.Add(IRole.Cultist);
                }
                //now fill rest of the slots with villagers (for large games)

                if ((!DisableVillager && !StrongMode) || gameMode == 5)
                {
                    for (int i = 0; i < playerCount / 5; i++)
                        rolesToAssign.Add(IRole.Villager);
                }

                rolesToAssign.Add(IRole.Police);
                for (int i = 0; i < playerCount / 6; i++)
                    rolesToAssign.Add(IRole.Police);

                rolesToAssign.Add(IRole.Guard);
                for (int i = 0; i < playerCount / 6; i++)
                    rolesToAssign.Add(IRole.Guard);

                if (playerCount > 40) rolesToAssign.Add(IRole.ApprenticeSeer);
            }
            else
            {
                List<IRole> lista = PlayableRoleList();
                if (playerCount > 29)
                {
                    lista.Remove(IRole.Tanner);
                }
                if (StrongMode)
                {
                    lista.Remove(IRole.Villager);
                    lista.Remove(IRole.Sleepwalker);
                    lista.Remove(IRole.Lycan);
                    lista.Remove(IRole.ClumsyGuy);
                    lista.Remove(IRole.Baker);
                    lista.Remove(IRole.Beholder);
                    lista.Remove(IRole.Cupid);
                    lista.Remove(IRole.WildChild);
                    lista.Remove(IRole.Cursed);
                    lista.Remove(IRole.Traitor);
                    lista.Remove(IRole.WolfMan);
                }
                if (DisableVillager)
                {
                    lista.Remove(IRole.Villager);
                    lista.Remove(IRole.Sleepwalker);
                    lista.Remove(IRole.Lycan);
                }

                IRole roleToAdd = lista[Program.R.Next(lista.Count())];
                for (int i = 0; i < playerCount; i++)
                {
                    rolesToAssign.Add(roleToAdd);
                    if (roleToAdd == IRole.Mayor || roleToAdd == IRole.Cupid || roleToAdd == IRole.AlphaWolf)
                    {
                        lista.Remove(roleToAdd);
                        roleToAdd = lista[Program.R.Next(lista.Count())];
                    }
                    else
                    {
                        if(Program.R.Next(100) > 50)
                            roleToAdd = lista[Program.R.Next(lista.Count())];
                    }
                }
                int maxWolves = 0;

                for (int i = 0; i < Math.Min(Math.Max(playerCount / 4, 1), 10); i++)
                {
                    maxWolves++;
                }
                while (rolesToAssign.Count(x => WolfRoles.Contains(x) || SupportWolves.Contains(x)) > maxWolves)
                {
                    var wolfToRemove = rolesToAssign.FindIndex(x => WolfRoles.Contains(x) || SupportWolves.Contains(x));
                    rolesToAssign[wolfToRemove] = roleToAdd;
                    roleToAdd = lista[Program.R.Next(lista.Count())];
                    if (roleToAdd == IRole.Mayor || roleToAdd == IRole.Cupid || roleToAdd == IRole.AlphaWolf)
                    {
                        lista.Remove(roleToAdd);
                        roleToAdd = lista[Program.R.Next(lista.Count())];
                    }
                    else
                    {
                        if (Program.R.Next(100) > 50)
                            roleToAdd = lista[Program.R.Next(lista.Count())];
                    }
                }
            }

            return rolesToAssign;
        }
        private void AssignRoles()
        {
            try
            {
                List<IRole> rolesToAssign = new List<IRole>();
                var count = Players.Count;

                if (gameMode != 4)
                {
                    var balanced = false;
                    var attempts = 0;
                    var nonVgRoles = new[] { IRole.Cultist, IRole.SerialKiller, IRole.Tanner, IRole.Wolf, IRole.AlphaWolf, IRole.Sorcerer, IRole.WolfCub, IRole.Lycan, IRole.Thief, IRole.Pyro, IRole.HungryWolf,
                        IRole.Survivor, IRole.Imposter, IRole.RabidWolf, IRole.SnowWolf, IRole.Snooper, IRole.SpeedWolf, IRole.HowlingWolf };
                    var newRoles = new List<IRole>() { IRole.HowlingWolf, IRole.Ghost, IRole.Firefighter };
                    var gameConditions = new[] { IRole.Pyro, IRole.SerialKiller, IRole.Cultist };

                    int forcedWolves = 0; // with so many roles, wolves appear rarely. This will be used to set the amount of wolves the game will 100% have
                    int chancePerWolf = 75;

                    for (int i = 0; i < Math.Min(Math.Max(count / 5, 1), 10); i++)
                    {
                        if (Program.R.Next(100) < chancePerWolf)
                            forcedWolves++;
                    }

                    int forcedSafes = 0;

                    if (count > 10) forcedSafes = 1;
                    if (count > 20) forcedSafes = 2;
                    if (count > 30) forcedSafes = 3;
                    if (count > 40) forcedSafes = 4;

                    do
                    {
                        attempts++;
                        if (attempts >= 100000)
                        {
                            throw new IndexOutOfRangeException("Unable to create a balanced game.  Please try again.\nPlayer count: " + count);
                        }


                        //determine which roles should be assigned
                        rolesToAssign = GetRoleList(count);
                        rolesToAssign.Shuffle();
                        rolesToAssign = rolesToAssign.Take(count).ToList();

                        //clumsy mode
                        if (gameMode == 2)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                var role = rolesToAssign[i];
                                if (role.GetStrength(rolesToAssign) < 5 && Program.R.Next(100) < 90 && role != IRole.ClumsyGuy)
                                {
                                    var toclumsy = rolesToAssign.FindIndex(x => x == role);
                                    rolesToAssign[toclumsy] = IRole.ClumsyGuy;
                                }
                            }
                        }

                        //let's fix some roles that should or shouldn't be there...

                        //sorcerer, imposter, snowwolf, snooper or traitor, without wolves, are pointless. change one of them to wolf
                        if ((rolesToAssign.Contains(IRole.Sorcerer) || rolesToAssign.Contains(IRole.Traitor) || rolesToAssign.Contains(IRole.Imposter) || rolesToAssign.Contains(IRole.SnowWolf) || rolesToAssign.Contains(IRole.Snooper)) &&
                            !rolesToAssign.Any(x => WolfRoles.Contains(x)))
                        {
                            var towolf = rolesToAssign.FindIndex(x => x == IRole.Sorcerer || x == IRole.Traitor || x == IRole.Imposter || x == IRole.SnowWolf || x == IRole.Snooper); //if there are multiple, the random order of rolesToAssign will choose for us which one to substitute
                            rolesToAssign[towolf] = WolfRoles[Program.R.Next(WolfRoles.Count())]; //choose randomly from WolfRoles
                        }

                        //cult without CH -> add CH
                        if (rolesToAssign.Contains(IRole.Cultist) && !rolesToAssign.Contains(IRole.CultistHunter))
                        {
                            //just pick a vg, and turn them to CH
                            var vg = rolesToAssign.FindIndex(x => !nonVgRoles.Contains(x));
                            var cult = rolesToAssign.FindIndex(x => x == IRole.Cultist);
                            if (rolesToAssign.Any(x => !nonVgRoles.Contains(x)))
                            {
                                rolesToAssign[vg] = IRole.CultistHunter;
                            } // if there is no vg, turn one cultist into ch
                            else
                            {
                                rolesToAssign[cult] = IRole.CultistHunter;
                            }
                        }

                        //appseer without seer -> seer
                        if (rolesToAssign.Contains(IRole.ApprenticeSeer) && !rolesToAssign.Contains(IRole.Seer))
                        {
                            //substitute with seer
                            var apps = rolesToAssign.IndexOf(IRole.ApprenticeSeer);
                            rolesToAssign[apps] = IRole.Seer;
                        }

                        //rabid without alpha is useless
                        if (rolesToAssign.Contains(IRole.RabidWolf) && !rolesToAssign.Contains(IRole.AlphaWolf))
                        {
                            var wolves = rolesToAssign.Where(x => WolfRoles.Contains(x) && x != IRole.RabidWolf);
                            if (wolves.Any())
                            {
                                //just pick another wolf, and turn them to alpha
                                var wolf = rolesToAssign.FindIndex(x => WolfRoles.Contains(x) && x != IRole.RabidWolf);
                                rolesToAssign[wolf] = IRole.AlphaWolf;
                            }
                            else
                            {
                                //turn that rabid wolf into the alpha
                                var rabid = rolesToAssign.IndexOf(IRole.RabidWolf);
                                rolesToAssign[rabid] = IRole.AlphaWolf;
                            }
                        }

                        //make sure that we have at least one new role (for testing)
                        if ((rolesToAssign.Any(x => !newRoles.Contains(x)) && rolesToAssign.Any(x => newRoles.Contains(x))))
                        {
                        var neutralKiller = rolesToAssign.FirstOrDefault(x => gameConditions.Contains(x));
                        //make sure that we have at least two teams
                        if (
                        rolesToAssign.Any(x => !nonVgRoles.Contains(x)) //make sure we have VGs
                        && rolesToAssign.Any(x => gameConditions.Contains(x) || WolfRoles.Contains(x)) //make sure we have at least one enemy
                    )
                            balanced = true;
                        else if (rolesToAssign.Any(x => gameConditions.Contains(x)) && rolesToAssign.Any(x => WolfRoles.Contains(x)))
                            balanced = true; // wolves vs sk/pyro/cult works too
                        else if (rolesToAssign.Any(x => gameConditions.Contains(x) && (x != neutralKiller || x == IRole.SerialKiller)))
                            balanced = true; // sk/pyro/cult fighting each other works too. sk vs sk too
                                             //else, redo role assignment. better to rely on randomness, than trying to fix it
                        }

                        if (gameMode == 2 && !rolesToAssign.Contains(IRole.ClumsyGuy))
                        {
                            balanced = false;
                        }

                        //never have lone guard
                        if (rolesToAssign.Count(x => x == IRole.Guard) == 1)
                            balanced = false;

                        if (gameMode != 3)
                        {
                            // make sure we have a minimum amount of wolves
                            if (rolesToAssign.Count(x => WolfRoles.Contains(x) || SupportWolves.Contains(x)) < forcedWolves)
                                balanced = false;

                            // make sure we have a minimum amount of safes
                            if (rolesToAssign.Count(x => SafeRoles.Contains(x)) < forcedSafes)
                                balanced = false;

                            // make sure we have a minimum amount of vgs
                            int maxBaddies = ((count - 1) / 2) - 1;
                            if (StrongMode && gameMode != 5)
                                maxBaddies++;
                            if (count == 4)
                                maxBaddies = 1;
                            if (rolesToAssign.Count(x => nonVgRoles.Contains(x) || AllyRoles.Contains(x) || x == IRole.Cupid) > maxBaddies)
                                balanced = false;
                        }

                        var masons = rolesToAssign.Count(x => x == IRole.Mason); // masons are only allowed in strong mode if there are 3!
                        if (StrongMode && gameMode != 5 && (masons == 1 || masons == 2))
                            balanced = false;

                        //the roles to assign are good, now if it's not a chaos game we need to check if they're balanced
                        if (gameMode != 1 && gameMode != 3)
                        {
                            var villageStrength =
                                rolesToAssign.Where(x => !nonVgRoles.Contains(x)).Sum(x => x.GetStrength(rolesToAssign));
                            var enemyStrength =
                                rolesToAssign.Where(x => nonVgRoles.Contains(x)).Sum(x => x.GetStrength(rolesToAssign));

                            //big games usually have few evils
                            if (count > 10)
                            {
                                villageStrength += ((count / 3) - 3);
                            }

                            //check balance
                            var varianceAllowed = (count / 6) + 1;
                            balanced = balanced && (Math.Abs(villageStrength - enemyStrength) <= varianceAllowed);
                        }
                    } while (!balanced);
                }
                else
                {
                    var customRoleList = customList.Take(Players.Count).ToList();
                    var list = PlayableRoleList();
                    foreach (var role in customRoleList)
                    {
                        var randomList = new List<IRole>();
                        IRole roleToAdd = IRole.Villager;
                        switch (role)
                        {
                            case "Random":
                                foreach (var rol in list)
                                {
                                    if ((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || RepeatableRoles.Contains(rol))
                                        randomList.Add(rol);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomVillager":
                                foreach (var rol in list)
                                {
                                    if (((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || RepeatableRoles.Contains(rol)) && !BadRoles.Contains(rol) && !NeutralRoles.Contains(rol))
                                        randomList.Add(rol);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomVillagerNoAlly":
                                foreach (var rol in list)
                                {
                                    if (((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || RepeatableRoles.Contains(rol)) && !BadRoles.Contains(rol) && !NeutralRoles.Contains(rol) && !AllyRoles.Contains(rol))
                                        randomList.Add(rol);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomKiller":
                                foreach (var rol in list)
                                {
                                    if (((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || ((!WolfRoles.Contains(rol) && !SupportWolves.Contains(rol)) || rol == IRole.Wolf)) && (WolfRoles.Contains(rol) || SupportWolves.Contains(rol) || rol == IRole.SerialKiller || rol == IRole.Pyro))
                                        randomList.Add(rol);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomBaddie":
                                foreach (var rol in list)
                                {
                                    if (((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || ((!WolfRoles.Contains(rol) && !SupportWolves.Contains(rol)) || rol == IRole.Wolf)) && (WolfRoles.Contains(rol) || SupportWolves.Contains(rol) || rol == IRole.SerialKiller || rol == IRole.Pyro || rol == IRole.Cultist))
                                        randomList.Add(rol);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomWolf":
                                foreach (var rol in list)
                                {
                                    if (((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || RepeatableRoles.Contains(rol)) && (WolfRoles.Contains(rol) || SupportWolves.Contains(rol)))
                                        randomList.Add(rol);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomAlly":
                                foreach (var rol in list)
                                {
                                    if ((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) && WolfHelpers.Contains(rol))
                                        randomList.Add(rol);
                                }
                                if (randomList.Count == 0)
                                {
                                    randomList.Add(IRole.Sorcerer);
                                    randomList.Add(IRole.Imposter);
                                    randomList.Add(IRole.Traitor);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomNeutral":
                                foreach (var rol in list)
                                {
                                    if ((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) && NeutralRoles.Contains(rol))
                                        randomList.Add(rol);
                                }
                                if (randomList.Count == 0)
                                {
                                    randomList.Add(IRole.Survivor);
                                    randomList.Add(IRole.Tanner);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "Random120":
                                foreach (var rol in list)
                                {
                                    if ((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) && Roles120.Contains(rol))
                                        randomList.Add(rol);
                                }
                                if (randomList.Count == 0)
                                {
                                    randomList.Add(IRole.WildChild);
                                    randomList.Add(IRole.Imposter);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomSkyro":
                                randomList.Add(IRole.SerialKiller);
                                randomList.Add(IRole.Pyro);
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomCultable":
                                foreach (var rol in list)
                                {
                                    if (((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || RepeatableRoles.Contains(rol)) && Cultables.Contains(rol))
                                        randomList.Add(rol);
                                }
                                roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomLowCultable":
                                foreach (var rol in list)
                                {
                                    if (((!customRoleList.Contains(rol.ToString()) && !rolesToAssign.Contains(rol)) || RepeatableRoles.Contains(rol)) && LowCultables.Contains(rol))
                                        randomList.Add(rol);
                                }
                                if (randomList.Count == 0)
                                    roleToAdd = LowCultables[Program.R.Next(LowCultables.Count())];
                                else
                                    roleToAdd = randomList[Program.R.Next(randomList.Count())];
                                break;
                            case "RandomChoice": // shouldn't happen
                                break;
                            default:
                                roleToAdd = StringToRole(role);
                                break;
                        }
                        rolesToAssign.Add(roleToAdd);
                    }
                }

                startingwolves = rolesToAssign.Count(x => WolfRoles.Contains(x) || SupportWolves.Contains(x));

                //shuffle things
                Players.Shuffle();
                Players.Shuffle();
                Players.Shuffle();
                Players.Shuffle();
                Players.Shuffle();
                rolesToAssign.Shuffle();
                rolesToAssign.Shuffle();


                //assign the roles 
                for (var i = 0; i < Players.Count; i++)
                {
                    Players[i].PlayerRole = rolesToAssign[i];
                }

                SetRoleAttributes();

                //shuffle again
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        Players.Shuffle();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                foreach (var p in Players)
                    p.OriginalRole = p.PlayerRole;
            }
            catch (Exception ex)
            {
                Send($"Error while assigning roles: {ex.Message}\nPlease start a new game");
                LogException(ex);
                Thread.Sleep(1000);
                Program.RemoveGame(this);
            }
        }

        private void SendRoleList(bool full = false)
        {
            var msg = "Rolelist:\n\n";
            msg.ToBold();
            var orderedList = customList.Take(Players.Count).ToList();
            if (full)
                orderedList = customList.ToList();
            orderedList = orderedList.OrderBy(x => x).ToList();
            var list = orderedList.GroupBy(x => x).Select(x => new {Count = x.Count(), Name = x.Key });
            foreach (var role in list)
            {
                msg += "- " + GetLocaleString(role.Name).ToBold() + " x" + role.Count + "\n";
            }
            _roleListId = Send(msg).Result.MessageId;
        }

        private IRole StringToRole(string name)
        {
            switch (name)
            {
                case "Villager":
                    return IRole.Villager;
                case "Drunk":
                    return IRole.Drunk;
                case "Harlot":
                    return IRole.Harlot;
                case "Seer":
                    return IRole.Seer;
                case "Traitor":
                    return IRole.Traitor;
                case "GuardianAngel":
                    return IRole.GuardianAngel;
                case "Detective":
                    return IRole.Detective;
                case "Wolf":
                    return IRole.Wolf;
                case "Gunner":
                    return IRole.Gunner;
                case "Tanner":
                    return IRole.Tanner;
                case "Fool":
                    return IRole.Fool;
                case "WildChild":
                    return IRole.WildChild;
                case "Beholder":
                    return IRole.Beholder;
                case "Cupid":
                    return IRole.Cupid;
                case "ClumsyGuy":
                    return IRole.ClumsyGuy;
                case "Mayor":
                    return IRole.Mayor;
                case "Prince":
                    return IRole.Prince;
                case "Survivor":
                    return IRole.Survivor;
                case "Imposter":
                    return IRole.Imposter;
                case "Baker":
                    return IRole.Baker;
                case "Sleepwalker":
                    return IRole.Sleepwalker;
                case "Ninja":
                    return IRole.Ninja;
                case "Herbalist":
                    return IRole.Herbalist;
                case "Sorcerer":
                    return IRole.Sorcerer;
                case "Healer":
                    return IRole.Healer;
                case "Cursed":
                    return IRole.Cursed;
                case "Sandman":
                    return IRole.Sandman;
                case "Sheriff":
                    return IRole.Sheriff;
                case "Pyro":
                    return IRole.Pyro;
                case "Doppelgänger":
                    return IRole.Doppelgänger;
                case "Atheist":
                    return IRole.Atheist;
                case "Hunter":
                    return IRole.Hunter;
                case "Oracle":
                    return IRole.Oracle;
                case "Blacksmith":
                    return IRole.Blacksmith;
                case "WiseElder":
                    return IRole.WiseElder;
                case "Pacifist":
                    return IRole.Pacifist;
                case "SerialKiller":
                    return IRole.SerialKiller;
                case "AlphaWolf":
                    return IRole.AlphaWolf;
                case "WolfCub":
                    return IRole.WolfCub;
                case "Lycan":
                    return IRole.Lycan;
                case "RabidWolf":
                    return IRole.RabidWolf;
                case "SnowWolf":
                    return IRole.SnowWolf;
                case "Snooper":
                    return IRole.Snooper;
                case "HungryWolf":
                    return IRole.HungryWolf;
                case "Cultist":
                    return IRole.Cultist;
                case "WolfMan":
                    return IRole.WolfMan;
                case "Police":
                    return IRole.Police;
                case "ApprenticeSeer":
                    return IRole.ApprenticeSeer;
                case "Mason":
                    return IRole.Mason;
                case "CultistHunter":
                    return IRole.CultistHunter;
                case "SpeedWolf":
                    return IRole.SpeedWolf;
                case "Lookout":
                    return IRole.Lookout;
                case "Guard":
                    return IRole.Guard;
                case "HowlingWolf":
                    return IRole.HowlingWolf;
                case "Firefighter":
                    return IRole.Firefighter;
                case "Miner":
                    return IRole.Miner;
                case "Ghost":
                    return IRole.Ghost;
                default:
                    throw new ArgumentOutOfRangeException(name);
            }
        }

        private void SetRoleAttributes()
        {
            foreach (var p in Players)
            {
                switch (p.PlayerRole)
                {
                    case IRole.Villager:
                    case IRole.Cursed:
                    case IRole.Drunk:
                    case IRole.Beholder:
                    case IRole.ApprenticeSeer:
                    case IRole.Traitor:
                    case IRole.Mason:
                    case IRole.Hunter:
                    case IRole.Mayor:
                    case IRole.ClumsyGuy:
                    case IRole.Prince:
                    case IRole.WolfMan:
                    case IRole.Pacifist:
                    case IRole.WiseElder:
                    case IRole.Atheist:
                    case IRole.Baker:
                    case IRole.Sleepwalker:
                    case IRole.Ghost:
                        p.HasDayAction = false;
                        p.HasNightAction = false;
                        p.Team = ITeam.Village;
                        break;
                    case IRole.Fool:
                    case IRole.Harlot:
                    case IRole.CultistHunter:
                    case IRole.Seer:
                    case IRole.GuardianAngel:
                    case IRole.WildChild:
                    case IRole.Cupid:
                    case IRole.Blacksmith:
                    case IRole.Sandman:
                    case IRole.Oracle:
                    case IRole.Sheriff:
                    case IRole.Police:
                    case IRole.Healer:
                    case IRole.Herbalist:
                    case IRole.Ninja:
                    case IRole.Lookout:
                    case IRole.Guard:
                    case IRole.Firefighter:
                    case IRole.Miner:
                        p.Team = ITeam.Village;
                        p.HasNightAction = true;
                        p.HasDayAction = false;
                        break;
                    case IRole.Doppelgänger:
                    case IRole.Thief:
                    case IRole.Survivor:
                        p.Team = ITeam.Neutral;
                        p.HasNightAction = true;
                        p.HasDayAction = false;
                        break;
                    case IRole.Detective:
                    case IRole.Gunner:
                        p.Team = ITeam.Village;
                        p.HasDayAction = true;
                        p.HasNightAction = false;
                        break;
                    case IRole.Sorcerer:
                    case IRole.AlphaWolf:
                    case IRole.WolfCub:
                    case IRole.Wolf:
                    case IRole.Lycan:
                    case IRole.HungryWolf:
                    case IRole.Imposter:
                    case IRole.RabidWolf:
                    case IRole.SnowWolf:
                    case IRole.Snooper:
                    case IRole.SpeedWolf:
                    case IRole.HowlingWolf:
                        p.Team = ITeam.Wolf;
                        p.HasNightAction = true;
                        p.HasDayAction = false;
                        break;
                    case IRole.Tanner:
                        p.Team = ITeam.Tanner;
                        p.HasDayAction = false;
                        p.HasNightAction = false;
                        break;
                    case IRole.Cultist:
                        p.HasDayAction = false;
                        p.HasNightAction = true;
                        p.Team = ITeam.Cult;
                        break;
                    case IRole.SerialKiller:
                        p.HasNightAction = true;
                        p.HasDayAction = false;
                        p.Team = ITeam.SerialKiller;
                        break;
                    case IRole.Pyro:
                        p.HasNightAction = true;
                        p.HasDayAction = false;
                        p.Team = ITeam.Pyro;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void NotifyRoles()
        {
            if (Players == null) return; //how the hell?
            //notify each player
            foreach (var p in Players.ToList())
            {
                if (p?.PlayerRole == null) continue;
                var msg = GetRoleInfo(p.PlayerRole);
                try
                {
                    // ReSharper disable once UnusedVariable
                    var result = Program.Send(msg, p.Id, true).Result;
                    if (WolfRoles.Contains(p.PlayerRole) || SupportWolves.Contains(p.PlayerRole))
                    {
                        SendWolfList(p);
                    }
                }
                catch (AggregateException ae)
                {
                    LogException(ae);
                }
                catch (Exception e)
                {
                    LogAllExceptions(e);
                }
                Thread.Sleep(50);
            }
        }

        private string GetRoleInfo(IRole role)
        {
            try
            {
                string msg;
                switch (role)
                {
                    case IRole.Fool:
                        return GetLocaleString("RoleInfoSeer");
                    case IRole.Beholder:
                        msg = GetLocaleString("RoleInfoBeholder");
                        var seer = Players?.FirstOrDefault(x => x.PlayerRole == IRole.Seer);
                        if (seer != null)
                            msg += GetLocaleString("BeholderSeer", $"{seer.GetName()}");
                        else
                            msg += "  " + GetLocaleString("NoSeer");
                        return msg;
                    case IRole.Mason:
                        msg = GetLocaleString("RoleInfoMason");
                        if (Players?.Count(x => x?.PlayerRole == IRole.Mason) > 1)
                        {
                            msg += GetLocaleString("MasonTeam", Players.Where(x => x.PlayerRole == IRole.Mason).Select(x => x.GetName()).Aggregate((current, next) => current + ", " + next));
                        }
                        return msg;
                    case IRole.Doppelgänger:
                        return GetLocaleString("RoleInfoDoppelganger");
                    default:
                        return GetLocaleString($"RoleInfo{role}");
                }
            }
            catch (Exception e)
            {
                Send("Error in get role info: \n" + e.Message + "\n" + e.StackTrace, Program.ErrorGroup);
            }
            return "";
        }

        public void CheckRoleChanges(bool checkbitten = false)
        {
            if (Players == null) return;
            //check Apprentice Seer
            var aps = Players?.FirstOrDefault(x => x.PlayerRole == IRole.ApprenticeSeer & !x.IsDead);
            if (aps != null && (!checkbitten || !aps.Bitten))
            {
                //check if seer is alive
                if (!Players.Any(x => x.PlayerRole == IRole.Seer & !x.IsDead))
                {
                    //get the dead seer
                    var ds = Players.FirstOrDefault(x => x.PlayerRole == IRole.Seer && x.IsDead);

                    //seer is dead, promote app seer
                    aps.HasDayAction = false;
                    aps.HasNightAction = true;
                    aps.OriginalRole = IRole.ApprenticeSeer;
                    aps.PlayerRole = IRole.Seer;
                    aps.ChangedRolesCount++;
                    //notify
                    Send(GetLocaleString("ApprenticeNowSeer", ds?.GetName() ?? GetRoleDescription(IRole.Seer)), aps.Id);
                    var beholder = Players.FirstOrDefault(x => x.PlayerRole == IRole.Beholder & !x.IsDead);
                    if (beholder != null)
                        Send(GetLocaleString("BeholderNewSeer", $"{aps.GetName()}", ds?.GetName() ?? GetRoleDescription(IRole.Seer)), beholder.Id);
                }
            }
            CheckWildChild(checkbitten);
            CheckDoppelganger(checkbitten);

            var wolves = Players.GetPlayersForRoles(WolfRoles);
            if (wolves.Count() >= 7)
            {
                foreach (var w in wolves)
                {
                    AddAchievement(w, Achievements.PackHunter);
                }
            }

            var seers = Players.GetPlayersForRoles(new[] { IRole.Seer });
            if (seers.Count() > 1)
            {
                foreach (var s in seers)
                    AddAchievement(s, Achievements.DoubleVision);
            }
        }

        /*private void ChooseOrRandomTarget()
        {
            List<IPlayer> targets = new List<IPlayer>();
            foreach (var p in Players.Where(x => x.Choice <= 0))
            {
                switch (p.PlayerRole)
                {
                    case IRole.Healer:
                        targets = Players.Where(x => x.IsDead && x.Team == ITeam.Village).ToList();
                        break;
                    case IRole.Pyro:
                        break;
                    case IRole.SnowWolf:
                        break;
                    case IRole.Wolf:
                    case IRole.AlphaWolf:
                    case IRole.WolfCub:
                    case IRole.Lycan:
                    case IRole.HungryWolf:
                    case IRole.RabidWolf:
                    case IRole.SpeedWolf:
                    case IRole.Snooper:
                        break;
                    case IRole.Cultist:
                        break;
                    default:
                        break;
                }

                try
                {
                    targets.Shuffle();
                    targets.Shuffle();
                    p.Choice = targets[0].Id;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Send("Unable to choose random player\n" + Program.Version.FileVersion + $"\nGroup: {ChatId} ({ChatGroup})\nLanguage: {DbGroup?.Language ?? "null"}\n{Program.ClientId}\n{ex.Message}\n{ex.StackTrace}", Program.ErrorGroup);
                    p.Choice = -1;
                }
            }
        }*/

        private void ValidateSpecialRoleChoices()
        {
            if (GameDay != 1) return;
            //Wild Child
            var wcs = Players.Where(x => x.PlayerRole == IRole.WildChild);
            if (wcs.Any())
            {
                foreach (var wc in wcs)
                {
                    if (wc.RoleModel == 0)
                    {
                        var choiceid = ChooseRandomPlayerId(wc);
                        var choice = Players.FirstOrDefault(x => x.Id == choiceid);
                        if (choice != null)
                        {
                            wc.RoleModel = choice.Id;
                            Send(GetLocaleString("RoleModelChosen", choice.GetName()), wc.Id);
                        }
                    }
                }
            }

            var dg = Players.FirstOrDefault(x => x.PlayerRole == IRole.Doppelgänger);
            if (dg != null && dg.RoleModel == 0)
            {
                var choiceid = ChooseRandomPlayerId(dg);
                var choice = Players.FirstOrDefault(x => x.Id == choiceid);
                if (choice != null)
                {
                    dg.RoleModel = choice.Id;
                    Send(GetLocaleString("RoleModelChosen", choice.GetName()), dg.Id);
                }
            }

            var ims = Players.Where(x => x.PlayerRole == IRole.Imposter);
            if (ims.Any())
            {
                foreach (var im in ims)
                {
                    if (im.RoleModel == 0)
                    {
                        var choiceid = ChooseRandomPlayerId(im);
                        var choice = Players.FirstOrDefault(x => x.Id == choiceid);
                        if (choice != null)
                        {
                            im.RoleModel = choice.Id;
                            Send(GetLocaleString("RoleModelChosen", choice.GetName()), im.Id);
                            Send(GetLocaleString("ImposterSees", GetDescription(choice)), im.Id);
                        }
                    }
                }
            }

            //first, make sure there even IS a cupid
            if (Players.Any(x => x.PlayerRole == IRole.Cupid))
            {
                CreateLovers();
                NotifyLovers();
            }
        }

        private void CreateLovers()
        {
            //REDO
            //how many lovers do we have?
            var count = Players.Count(x => x.InLove);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{count} Lovers found");
            Console.ForegroundColor = ConsoleColor.Gray;
            if (count == 2)
            {
                return;
            }
            if (count > 2) //how?!?
            {
                var lovers = Players.Where(x => x.InLove).ToList(); //to list, we have broken off
                var l1 = Players.FirstOrDefault(x => x.Id == lovers[0].Id);
                var l2 = Players.FirstOrDefault(x => x.Id == lovers[1].Id);
                if (l1 == null || l2 == null)
                {
                    //WTF IS GOING ON HERE?!
                    if (l1 != null)
                        AddLover(l1);
                    if (l2 != null)
                        AddLover(l2);
                    //if both are null..
                    if (l1 == null && l2 == null)
                    {
                        //so lost....
                        l1 = AddLover();
                        l2 = AddLover(l1);
                    }
                }
                if (l1 != null && l2 != null)
                {
                    l1.LoverId = l2.Id;
                    l2.LoverId = l1.Id;
                }
                foreach (var p in lovers.Skip(2))
                {
                    var foreverAlone = Players.FirstOrDefault(x => x.Id == p.Id);
                    if (foreverAlone != null)
                    {
                        foreverAlone.InLove = false;
                        foreverAlone.LoverId = 0;
                    }
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Step 1: {Players.Count(x => x.InLove)} Lovers found");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            if (count < 2)
            {
                //ok, missing lovers.
                var exist = (Players.FirstOrDefault(x => x.InLove) ?? AddLover()) ?? AddLover();
                AddLover(exist);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Step 2: {Players.Count(x => x.InLove)} Lovers found");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            ////cupid stuffs
            //var lovers = Players.Where(x => x.InLove);
            //while (lovers.Count() != 2)
            //{
            //    //ok, missing lover, create one
            //    var choiceid = ChooseRandomPlayerId(lovers);
            //    var newLover = Players.FirstOrDefault(x => x.Id == choiceid);
            //    if (newLover != null)
            //    {
            //        newLover.InLove = true;
            //        var otherLover = lovers.FirstOrDefault(x => x.Id != newLover.Id);
            //        if (otherLover != null)
            //        {
            //            otherLover.LoverId = newLover.Id;
            //            newLover.LoverId = otherLover.Id;
            //        }
            //    }
            //}
        }

        private void NotifyLovers()
        {
            var loversNotify = Players.Where(x => x.InLove).ToList();
            while (loversNotify.Count != 2)
            {
                CreateLovers();
                loversNotify = Players.Where(x => x.InLove).ToList();
            }

            foreach (var lover in loversNotify)
            {
                if (lover.SpeedDating)
                    AddAchievement(lover, Achievements.OnlineDating);
            }

            Send(GetLocaleString("CupidChosen", loversNotify[0].GetName()), loversNotify[1].Id);
            Send(GetLocaleString("CupidChosen", loversNotify[1].GetName()), loversNotify[0].Id);
        }

        private IPlayer AddLover(IPlayer existing = null)
        {
            var lover = Players.FirstOrDefault(x => x.Id == ChooseRandomPlayerId(existing));
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"AddLover: {lover?.Name} picked");
            Console.ForegroundColor = ConsoleColor.Gray;
            if (lover == null) return null;
            lover.InLove = true;
            lover.SpeedDating = true;
            if (existing == null) return lover;
            existing.LoverId = lover.Id;
            lover.LoverId = existing.Id;
            return lover;
        }

        private void CheckWildChild(bool checkbitten = false)
        {
            var wcs = Players?.Where(x => x.PlayerRole == IRole.WildChild & !x.IsDead);

            // Check Wild Child
            if (wcs.Any())
            {
                foreach (var wc in wcs)
                {
                    if (!checkbitten || !wc.Bitten)
                    {
                        var rm = Players.FirstOrDefault(x => x.Id == wc.RoleModel);
                        if (rm != null)
                        {
                            if (rm.IsDead)
                            {
                                var wolves = Players.GetPlayersForRoles(WolfRoles);
                                //notify other wolves
                                foreach (var w in wolves)
                                {
                                    Send(GetLocaleString("WildChildToWolves", wc.GetName()), w.Id);
                                }
                                    Send(GetLocaleString("WildChildTransform", rm.GetName()), wc.Id);
                                wc.PlayerRole = IRole.Wolf;
                                wc.Team = ITeam.Wolf;
                                wc.ChangedRolesCount++;
                                wc.HasNightAction = true;
                                wc.HasDayAction = false;
                                SendWolfList(wc, true);
                            }
                        }
                    }
                }
            }
        }

        private void CheckDoppelganger(bool checkbitten = false)
        {
            var doppels = Players?.Where(x => x.PlayerRole == IRole.Doppelgänger & !x.IsDead);
            //var aps = Players.FirstOrDefault(x => x.PlayerRole == IRole.ApprenticeSeer & !x.IsDead);
            //var traitor = Players.FirstOrDefault(x => x.PlayerRole == IRole.Traitor & !x.IsDead);

            // Check DG
            if (doppels.Any())
            {
                foreach (var p in doppels)
                {
                    if (!checkbitten || !p.Bitten)
                    {
                        var rm = Players.FirstOrDefault(x => x.Id == p.RoleModel);
                        if (rm != null && rm.IsDead)
                        {
                            var teammates = "";
                            //notify other wolves
                            p.PlayerRole = rm.OriginalRole;
                            if (rm.OriginalRole == IRole.Doppelgänger)
                            {
                                p.PlayerRole = rm.PlayerRole;
                            }
                            if (rm.OriginalRole == IRole.ApprenticeSeer || rm.OriginalRole == IRole.WildChild || rm.OriginalRole == IRole.Traitor || rm.OriginalRole == IRole.Cursed)
                            {
                                //if (rm.OriginalRole == IRole.ApprenticeSeer || rm.OriginalRole == IRole.Cursed)
                                if (rm.OriginalRole == IRole.ApprenticeSeer)     //if cursed turned wolf before dying, should DG turn cursed or directly wolf? use the above line if DG should turn cursed
                                    if (rm.PlayerRole != IRole.Wolf)
                                        p.PlayerRole = rm.PlayerRole;
                                if (rm.PlayerRole != IRole.Cultist)
                                    p.PlayerRole = rm.PlayerRole;
                            }
                            p.ChangedRolesCount++;

                            if (!new[] { IRole.Mason, IRole.Wolf, IRole.AlphaWolf, IRole.WolfCub, IRole.Cultist, IRole.WildChild,
                        IRole.Lycan, IRole.HungryWolf, IRole.Imposter, IRole.RabidWolf, IRole.SnowWolf, IRole.Snooper, IRole.Doppelgänger, IRole.SpeedWolf, IRole.HowlingWolf }.Contains(p.PlayerRole))
                            {
                                //tell them their new role
                                Send(GetRoleInfo(p.PlayerRole), p.Id);
                            }
                            switch (p.PlayerRole)
                            {
                                case IRole.Villager:
                                case IRole.Cursed:
                                case IRole.Drunk:
                                case IRole.Prince:
                                case IRole.ClumsyGuy:
                                case IRole.WolfMan:
                                case IRole.WiseElder:
                                case IRole.Atheist:
                                case IRole.Baker:
                                case IRole.Sleepwalker:
                                case IRole.Ghost:
                                    p.HasDayAction = false;
                                    p.HasNightAction = false;
                                    p.Team = ITeam.Village;
                                    p.HasUsedAbility = false;
                                    break;
                                case IRole.Beholder:
                                    p.HasDayAction = false;
                                    p.HasNightAction = false;
                                    p.Team = ITeam.Village;
                                    var seer = Players.FirstOrDefault(x => x.PlayerRole == IRole.Seer);
                                    Send(
                                        seer != null
                                            ? GetLocaleString("BeholderSeer", $"{seer.GetName()}")
                                            : GetLocaleString("NoSeer"), p.Id);
                                    break;
                                case IRole.ApprenticeSeer:
                                    p.HasDayAction = false;
                                    p.HasNightAction = false;
                                    p.Team = ITeam.Village;
                                    if (Players.Count(x => !x.IsDead && x.PlayerRole == IRole.Seer) == 0)
                                    {
                                        p.PlayerRole = IRole.Seer;
                                        p.HasNightAction = true;
                                        var beholder = Players.FirstOrDefault(x => x.PlayerRole == IRole.Beholder & !x.IsDead);
                                        if (beholder != null)
                                            Send(GetLocaleString("BeholderNewSeer", $"{p.GetName()}", rm.GetName() ?? GetRoleDescription(IRole.Seer)), beholder.Id);
                                    }
                                    break;
                                case IRole.Traitor:
                                    p.HasDayAction = false;
                                    p.HasNightAction = false;
                                    p.Team = ITeam.Village;
                                    if (Players.Count(x => !x.IsDead && WolfRoles.Contains(x.PlayerRole)) == 0)
                                    {
                                        p.HasNightAction = true;
                                        p.PlayerRole = IRole.Wolf;
                                        p.Team = ITeam.Wolf;
                                    }
                                    break;
                                case IRole.Mason:
                                    p.HasDayAction = false;
                                    p.HasNightAction = false;
                                    p.Team = ITeam.Village;
                                    foreach (var w in Players.Where(x => x.PlayerRole == IRole.Mason & !x.IsDead && x.Id != p.Id))
                                    {
                                        Send(GetLocaleString("DGToMason", $"{p.GetName()}"), w.Id);
                                        teammates += $"{w.GetName()}" + ", ";
                                    }
                                    Send(GetLocaleString("DGTransformToMason", rm.GetName(), teammates), p.Id);
                                    break;
                                case IRole.Hunter:
                                    p.HasDayAction = false;
                                    p.HasNightAction = false;
                                    p.Team = ITeam.Village;
                                    break;
                                case IRole.Fool:
                                case IRole.Harlot:
                                case IRole.CultistHunter:
                                case IRole.GuardianAngel:
                                case IRole.Oracle:
                                case IRole.Sandman:
                                case IRole.Sheriff:
                                case IRole.Police:
                                case IRole.Healer:
                                case IRole.Herbalist:
                                case IRole.Ninja:
                                case IRole.Lookout:
                                case IRole.Guard:
                                case IRole.Firefighter:
                                case IRole.Miner:
                                    p.Team = ITeam.Village;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    break;
                                case IRole.Seer:
                                    p.Team = ITeam.Village;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    var bh = Players.FirstOrDefault(x => x.PlayerRole == IRole.Beholder & !x.IsDead);
                                    if (bh != null)
                                        Send(GetLocaleString("BeholderNewSeer", $"{p.GetName()}", rm.GetName() ?? GetRoleDescription(IRole.Seer)), bh.Id);
                                    break;
                                case IRole.WildChild:
                                    p.RoleModel = rm.RoleModel;
                                    p.Team = ITeam.Village;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    Send(GetLocaleString("NewWCRoleModel", Players.FirstOrDefault(x => x.Id == p.RoleModel)?.GetName() ?? "None was chosen!"), p.Id);
                                    break;
                                case IRole.Imposter:
                                    p.RoleModel = rm.RoleModel;
                                    p.Team = ITeam.Wolf;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    Send(GetLocaleString("NewIMRoleModel", Players.FirstOrDefault(x => x.Id == p.RoleModel)?.GetName() ?? "None was chosen!"), p.Id);
                                    Send(GetLocaleString("ImposterSees", GetDescription(Players.FirstOrDefault(x => x.Id == p.RoleModel))), p.Id);
                                    break;
                                case IRole.Doppelgänger:
                                    p.RoleModel = rm.RoleModel;
                                    p.Team = ITeam.Neutral;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    Send(GetLocaleString("NewDGRoleModel", Players.FirstOrDefault(x => x.Id == p.RoleModel)?.GetName() ?? "None was chosen!"), p.Id);
                                    break;
                                case IRole.Cupid:
                                    p.Team = ITeam.Village;
                                    p.HasNightAction = false;
                                    p.HasDayAction = false;
                                    break;
                                case IRole.Detective:
                                case IRole.Blacksmith:
                                case IRole.Gunner:
                                    p.Bullet = 2;
                                    p.Team = ITeam.Village;
                                    p.HasDayAction = true;
                                    p.HasNightAction = false;
                                    break;
                                case IRole.AlphaWolf:
                                case IRole.WolfCub:
                                case IRole.Wolf:
                                case IRole.Lycan:
                                case IRole.HungryWolf:
                                case IRole.RabidWolf:
                                case IRole.Snooper:
                                case IRole.HowlingWolf:
                                    var wolves = Players.Where(x => (WolfRoles.Contains(x.PlayerRole) || SupportWolves.Contains(x.PlayerRole)) && !x.IsDead && x.Id != p.Id);
                                    p.Team = ITeam.Wolf;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;

                                    foreach (var w in wolves)
                                    {
                                        Send(GetLocaleString($"DGToWolf", p.GetName()), w.Id);
                                    }
                                    SendWolfList(p, true);
                                    break;
                                case IRole.SpeedWolf:
                                    wolves = Players.Where(x => (WolfRoles.Contains(x.PlayerRole) || SupportWolves.Contains(x.PlayerRole)) && !x.IsDead && x.Id != p.Id);
                                    p.Team = ITeam.Wolf;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;

                                    var choices = new[] { new[] { new InlineKeyboardCallbackButton(GetLocaleString("RunAway"), $"vote|{Program.ClientId}|runaway") } }.ToList();
                                    SendMenu(choices, p, GetLocaleString("AskRunaway"), QuestionType.SpeedWolf);

                                    foreach (var w in wolves)
                                    {
                                        Send(GetLocaleString($"DGToWolf", p.GetName()), w.Id);
                                    }
                                    SendWolfList(p, true);
                                    break;
                                case IRole.SnowWolf:
                                    wolves = Players.Where(x => (WolfRoles.Contains(x.PlayerRole) || SupportWolves.Contains(x.PlayerRole)) && !x.IsDead && x.Id != p.Id);
                                    p.Team = ITeam.Wolf;
                                    p.HasDayAction = false;
                                    p.HasNightAction = true;
                                    foreach (var w in wolves)
                                    {
                                        Send(GetLocaleString($"DGToSnowWolf", p.GetName()), w.Id);
                                    }
                                    SendWolfList(p);
                                    break;
                                case IRole.Tanner:
                                    p.Team = ITeam.Tanner;
                                    p.HasDayAction = false;
                                    p.HasNightAction = false;
                                    break;
                                case IRole.Cultist:
                                    p.HasDayAction = false;
                                    p.HasNightAction = true;
                                    p.Team = ITeam.Cult;
                                    foreach (var w in Players.Where(x => x.PlayerRole == IRole.Cultist & !x.IsDead && x.Id != p.Id))
                                    {
                                        Send(GetLocaleString("DGToCult", $"{p.GetName()}"), w.Id);
                                        teammates += $"{w.GetName()}" + ", ";
                                    }
                                    Send(GetLocaleString("DGTransformToCult", rm.GetName(), teammates), p.Id);
                                    break;
                                case IRole.SerialKiller:
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    p.Team = ITeam.SerialKiller;
                                    break;
                                case IRole.Pyro:
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    p.Team = ITeam.Pyro;
                                    break;
                                case IRole.Sorcerer:
                                    p.Team = ITeam.Wolf;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    break;
                                case IRole.Mayor:
                                    p.HasUsedAbility = false;
                                    p.Team = ITeam.Village;
                                    p.HasNightAction = false;
                                    p.HasDayAction = false;
                                    choices = new[] { new[] { new InlineKeyboardCallbackButton(GetLocaleString("Reveal"), $"vote|{Program.ClientId}|reveal") } }.ToList();
                                    SendMenu(choices, p, GetLocaleString("AskMayor"), QuestionType.Mayor);
                                    break;
                                case IRole.Pacifist:
                                    p.HasUsedAbility = false;
                                    p.Team = ITeam.Village;
                                    p.HasNightAction = false;
                                    p.HasDayAction = false;
                                    choices = new[] { new[] { new InlineKeyboardCallbackButton(GetLocaleString("Peace"), $"vote|{Program.ClientId}|peace") } }.ToList();
                                    SendMenu(choices, p, GetLocaleString("AskPacifist"), QuestionType.Pacifist);
                                    break;
                                case IRole.Thief:
                                case IRole.Survivor:
                                    p.Team = ITeam.Neutral;
                                    p.HasNightAction = true;
                                    p.HasDayAction = false;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }
        }

        private void BitePlayer(IPlayer target, IEnumerable<IPlayer> voteWolves, string alpha, bool rabid)
        {
            if (rabid)
                target.RabidBitten = true;
            target.Bitten = true;
            foreach (var wolf in voteWolves)
                Send(
                    GetLocaleString("PlayerBittenWolves", target.GetName(),
                        alpha), wolf.Id);
        }

        private void DevourPlayer(IPlayer target, IEnumerable<IPlayer> voteWolves, string hungryww)
        {
            target.Devoured = true;
            foreach (var wolf in voteWolves)
                Send(
                    GetLocaleString("PlayerDevouredWolves", target.GetName(),
                        hungryww, GetRoleDescription(target.PlayerRole)), wolf.Id);
        }

        private void StealRole(IPlayer target)
        {
            if (target.IsDead)
            {
                //sad for you....
                Send(GetLocaleString("ThiefStealDead", target.GetName()));
                return;
            }
            //TODO
            //swap roles
            //notify both players (notify team?)
            //should a bitten player stay bitten? yes...
        }

        private void ConvertToCult(IPlayer target, IEnumerable<IPlayer> voteCult, int chance = 100)
        {
            if (Program.R.Next(100) < chance)
            {
                if (target.PlayerRole == IRole.Harlot)
                    foreach (var c in voteCult)
                        AddAchievement(c, Achievements.DontStayHome);

                target.OriginalRole = target.PlayerRole;
                target.PlayerRole = IRole.Cultist;
                target.Team = ITeam.Cult;
                target.HasDayAction = false;
                target.HasNightAction = true;
                target.DayCult = GameDay;
                Send(GetLocaleString("CultConvertYou"), target.Id);
                Send(GetLocaleString("CultTeam", voteCult.Select(x => x.GetName()).Aggregate((a, b) => a + ", " + b)), target.Id);
                foreach (var c in voteCult)
                    Send(GetLocaleString("CultJoin", $"{target.GetName()}"), c.Id);
            }
            else
            {
                foreach (var c in voteCult)
                {
                    Send(GetLocaleString("CultUnableToConvert", voteCult.OrderByDescending(x => x.DayCult).First().GetName(), target.GetName()), c.Id);
                }
                Send(GetLocaleString("CultAttempt"), target.Id);
            }
        }



#endregion

#region Cycles

        public void ForceStart()
        {
            KillTimer = true;
        }
        public void ExtendTime(long id, bool admin, int seconds)
        {
            if (!IsJoining) return;
            var p = Players.FirstOrDefault(x => x.TeleUser.Id == id);
            if (p != null)
            {
                if (HaveExtended.Contains(p.TeleUser.Id) && !admin)
                {
                    SendWithQueue(GetLocaleString("CantExtend"));
                    return;
                }
                _secondsToAdd = seconds;
                HaveExtended.Add(p.TeleUser.Id);
            }
            return;
        }
        private void LynchCycle()
        {
            if (!IsRunning) return;
            Time = GameTime.Lynch;
            if (Players == null) return;
            foreach (var p in Players)
                p.CurrentQuestion = null;

            if (CheckForGameEnd()) return;
            if (_bakerDied)
            {
                _bakerDied = false;
                SendWithQueue(GetLocaleString("BakerNoLynch"));
                return;
            }
            if (_pacifistUsed)
            {
                _pacifistUsed = false;
                SendWithQueue(GetLocaleString("PacifistNoLynchNow"));
                return;
            }
            SendWithQueue(GetLocaleString("LynchTime", /*DbGroup.LynchTime.ToBold() ?? */Settings.TimeLynch.ToBold()));
            SendPlayerList();
            SendLynchMenu();

            for (var i = 0; i < (/*DbGroup.LynchTime ?? */Settings.TimeLynch); i++)
            {
                Thread.Sleep(1000);
                if (CheckForGameEnd()) return;
                if (_pacifistUsed)
                {
                    SendWithQueue(GetLocaleString("PacifistNoLynchNow"));
                    _pacifistUsed = false;
                    foreach (var p in Players.Where(x => x.CurrentQuestion != null))
                    {
                        try
                        {
                            if (p.CurrentQuestion.MessageId != 0)
                            {
                                Program.MessagesSent++;
                                Program.Bot.EditMessageTextAsync(p.Id, p.CurrentQuestion.MessageId, GetLocaleString("LynchPeaceTimeout"));
                            }
                        }
                        catch { } // ignored
                        p.CurrentQuestion = null;
                    }
                    return;
                }
                //check if all votes are cast
                var livePlayers = Players.Where(x => !x.IsDead && !x.RanAway);
                if (livePlayers.All(x => x.Choice != 0))
                    break;
            }
            //remove menus
            try
            {
                foreach (var p in Players.Where(x => x.CurrentQuestion != null))
                {
                    try
                    {
                        if (p.CurrentQuestion.MessageId != 0)
                        {
                            Program.MessagesSent++;
                            Program.Bot.EditMessageTextAsync(p.Id, p.CurrentQuestion.MessageId, GetLocaleString("TimesUp"));
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    p.CurrentQuestion = null;
                }
            }
            catch
            {
                // ignored
            }


            //Log.WriteLine("Lynch time ended, adding up votes");
            if (CheckForGameEnd()) return;
            foreach (var p in Players.Where(x => !x.IsDead || x.PlayerRole == IRole.Ghost))
            {
                if (Inverted)
                {
                    if (p.Choice == 0 || p.Choice == -1)
                    {
                        var others = Players.Where(x => !x.IsDead && x.Id != p.Id).ToList();
                        if (others.Any())
                        {
                            others.Shuffle();
                            others.Shuffle();
                            p.Choice = others[0].Id;
                            Send(GetLocaleString("RandomTargetChosen", others[0].GetName()), p.Id);
                            var msg = GetLocaleString("PlayerVotedLynch", p.GetName(), others[0].GetName());
                            SendWithQueue(msg);
                        }
                    }
                }

                if (p.Choice != 0 && p.Choice != -1)
                {
                    var target = Players.FirstOrDefault(x => x.Id == p.Choice);
                    if (target != null)
                    {
                        if (BadRoles.Contains(target.PlayerRole) && !BadRoles.Contains(p.PlayerRole))
                        {
                            p.PointsToAdd += 1;
                        }
                        target.HasBeenVoted = true;
                        if (p.PlayerRole == IRole.Thief)
                        {
                            target.Votes += p.VotePower;
                        }
                        else if (p.PlayerRole == IRole.Mayor && p.HasUsedAbility) //Mayor counts twice
                        {
                            p.MayorLynchAfterRevealCount++;
                            target.Votes += 2;
                        }
                        else if (!p.IsStolen)
                            target.Votes++;
                        DBAction(p, target, "Lynch");
                    }
                    p.NonVote = 0;
                }
                else if (!p.IsDead && p.Choice != -2)
                {
                    if (!p.RanAway)
                        p.NonVote++;
                    if (p.NonVote < (p.Score < 1800 ? 2 : 3)) continue;
                    var idles24 = 0;
                    try
                    {
                        using (var db = new WWContext())
                        {
                            idles24 = db.GetIdleKills24Hours(p.Id).FirstOrDefault() ?? 0;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    SendWithQueue(GetLocaleString("IdleKill", p.GetName(), (/*DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ?*/ $"{p.GetName()} {GetLocaleString("Was")} {GetRoleDescription(p.PlayerRole)}\n" /*: ""*/) + GetLocaleString("IdleCount", p.GetName() + $"(id: <code>{p.TeleUser.Id}</code>)", idles24 + 1)));

                    //if hunter has died from AFK, too bad....
                    if (p.PlayerRole == IRole.Baker)
                    {
                        _bakerDied = true;
                    }
                    p.PointsToAdd -= 3 + idles24;
                    p.IsDead = true;
                    p.DiedAFK = true;
                    p.TimeDied = DateTime.Now;
                    p.DayOfDeath = GameDay;
                    CheckRoleChanges();
                    //update the database
                    DBKill(p, p, KillMthd.Idle);
                }
            }


            try
            {
                var maxVotes = Players.Max(x => x.Votes);
                var choices = Players.Where(x => x.Votes == maxVotes).ToList();
                IPlayer lynched = new IPlayer() { Votes = -1 };
                if (choices.Count > 1)
                {
                    //Log.WriteLine("Lynch tie");
                    if (Settings.RandomLynch)
                    {
                        //select one at random now
                        choices.Shuffle();
                        choices.Shuffle();
                        lynched = Players.First(x => x.Id == choices.First().Id);
                    }
                }
                else
                {
                    lynched = Players.First(x => x.Id == choices.First().Id);
                }

                //Log.WriteLine("lynched Votes = " + lynched.Votes);

                if (lynched.Votes > 0)
                {
                    if (lynched.PlayerRole == IRole.Prince & !lynched.HasUsedAbility) //can only do this once
                    {
                        SendWithQueue(GetLocaleString("PrinceLynched", lynched.GetName()));
                        lynched.HasUsedAbility = true;
                    }
                    else if (lynched.PlayerRole == IRole.SpeedWolf & lynched.RanAway)
                    {
                        SendWithQueue(GetLocaleString("SpeedWolfLynched", lynched.GetName()));
                        lynched.RanAway = false;
                        lynched.HasUsedAbility = true;
                    }
                    else
                    {
                        lynched.IsDead = true;
                        lynched.TimeDied = DateTime.Now;
                        lynched.DayOfDeath = GameDay;
                        if (lynched.PlayerRole == IRole.Seer && GameDay == 1)
                            AddAchievement(lynched, Achievements.LackOfTrust);
                        if (lynched.PlayerRole == IRole.Prince && lynched.HasUsedAbility)
                            AddAchievement(lynched, Achievements.SpoiledRichBrat);
                        SendWithQueue(GetLocaleString("LynchKill", lynched.GetName(), /*!DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ?*/ $"{lynched.GetName()} {GetLocaleString("Was")} {GetDescription(lynched)}" /*: ""*/));

                        if (lynched.InLove)
                            KillLover(lynched);

                        //update the database
                        DBKill(Players.Where(x => x.Choice == lynched.Id), lynched, KillMthd.Lynch);

                        foreach (var player in Players.Where(x => x.LynchedWolf))
                            player.LynchedWolf = false;

                        //effects on game depending on the lynched's role
                        switch (lynched.PlayerRole)
                        {
                            case IRole.WolfCub:
                                WolfCubKilled = true;
                                break;
                            case IRole.RabidWolf:
                                RabidKilled = true;
                                break;
                            case IRole.Baker:
                                _bakerDied = true;
                                break;
                            case IRole.Tanner:
                                //check for overkill
                                if (Players.Where(x => !x.IsDead).All(x => x.Choice == lynched.Id))
                                    AddAchievement(lynched, Achievements.TannerOverkill);
                                //end game
                                lynched.DiedLastNight = true; //store the tanner who should win (DG is too complicated to handle)
                                DoGameEnd(ITeam.Tanner);
                                return;
                            case IRole.Hunter:
                                HunterFinalShot(lynched, KillMthd.Lynch);
                                break;
                        }

                        if (WolfRoles.Contains(lynched.PlayerRole) || SupportWolves.Contains(lynched.PlayerRole))
                            lynched.LynchedWolf = true;

                        CheckRoleChanges(true);
                    }
                }
                else if (lynched.Votes == -1)
                {
                    SendWithQueue(GetLocaleString("LynchTie"));
                    var t = choices.FirstOrDefault(x => x.PlayerRole == IRole.Tanner);
                    if (t != null)
                        AddAchievement(t, Achievements.SoClose);
                }
                else
                {
                    SendWithQueue(GetLocaleString("NoLynchVotes"));
                }

                foreach (var player in Players.Where(x => x.RanAway))
                {
                    player.RanAway = false;
                }

                if (CheckForGameEnd(true)) return;
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                    e = e.InnerException;


                Send("Oh no, something went wrong :( Error report is being sent to the developers\n" + e.Message);
                Send(e.StackTrace);
                Program.RemoveGame(this);
            }
        }

        private void DayCycle()
        {
            if (!IsRunning) return;
            Time = GameTime.Day;

            //see who died over night
            if (Players == null) return;
            foreach (var p in Players)
                p.CurrentQuestion = null;
            var timeToAdd = Math.Max(((Players.Count(x => !x.IsDead) / 5) - 1) * 30, 60);
            if (timeToAdd > 120)
                timeToAdd = 120;
            SendWithQueue(GetLocaleString("DayTime", ((/*DbGroup.DayTime ?? */Settings.TimeDay) + timeToAdd).ToBold()));
            SendWithQueue(GetLocaleString("Day", GameDay.ToBold()));
            SendPlayerList();

            SendDayActions();
            //incremental sleep time for large players....
            Thread.Sleep(TimeSpan.FromSeconds((/*DbGroup.DayTime ?? */Settings.TimeDay) + timeToAdd));

            if (!IsRunning) return;
            try
            {
                foreach (var p in Players.Where(x => x.CurrentQuestion != null))
                {
                    try
                    {
                        if (p.CurrentQuestion.MessageId != 0 && p.CurrentQuestion.QType != QuestionType.Mayor && p.CurrentQuestion.QType != QuestionType.Pacifist && p.CurrentQuestion.QType != QuestionType.SpeedWolf)
                        {
                            Program.MessagesSent++;
                            Program.Bot.EditMessageTextAsync(p.Id, p.CurrentQuestion.MessageId, GetLocaleString("TimesUp"));
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    p.CurrentQuestion = null;
                }
            }
            catch
            {
                // ignored
            }
            //check detective
            if (Players == null) return;
            var detectives = Players.Where(x => x.PlayerRole == IRole.Detective & !x.IsDead && x.Choice != 0 && x.Choice != -1);
            if (detectives.Any())
            {
                foreach (var detect in detectives)
                {
                    //first off, chance to tell wolves
                    if (Program.R.Next(100) < Settings.ChanceDetectiveCaught)
                    {
                        foreach (var w in Players.Where(x => !x.IsDead && (x.PlayerRole == IRole.Wolf || x.PlayerRole == IRole.AlphaWolf || x.PlayerRole == IRole.Snooper)))
                        {
                            Send(GetLocaleString("DetectiveCaught", $"{detect.GetName()}"), w.Id);
                        }
                    }

                    var check = Players.FirstOrDefault(x => x.Id == detect.Choice);
                    if (check != null)
                    {
                        DBAction(detect, check, "Detect");
                        Send(GetLocaleString("DetectiveSnoop", check.GetName(), GetDescription(check)), detect.Id);

                        //if snooped non-bad-roles:
                        if (!BadRoles.Contains(check.PlayerRole))
                            detect.CorrectSnooped.Clear();     //clear correct snoop list
                        else
                        {
                            detect.PointsToAdd += 1;
                            if (detect.CorrectSnooped.Contains(check.Id))     //check if it is a re-snoop of correct roles
                                detect.CorrectSnooped.Clear();             //clear the correct snoop list
                            detect.CorrectSnooped.Add(check.Id);              //add the current snoop to list

                            //if snooped 4 times correct continously
                            if (detect.CorrectSnooped.Count() >= 4)
                            {
                                AddAchievement(detect, Achievements.Streetwise);
                                detect.CorrectSnooped.Clear();
                            }
                        }
                    }
                }
            }

            var pyros = Players.Where(x => x.PlayerRole == IRole.Pyro & !x.IsDead && x.Choice != 0 && x.Choice != -1);
            var alivePlayers = Players.Where(x => !x.IsDead);
            if (pyros.Any() && alivePlayers.All(x => !WolfRoles.Contains(x.PlayerRole) && x.PlayerRole != IRole.SerialKiller))
            {
                foreach (var pyro in pyros)
                {
                    var dousedT = Players.FirstOrDefault(x => x.Id == pyro.Choice);
                    if (dousedT != null)
                    {
                        dousedT.IsDoused = true;
                    }
                }
            }


            var thieves = Players.Where(x => x.PlayerRole == IRole.Thief & !x.IsDead && x.Choice != 0 && x.Choice != -1);
            if (thieves.Any())
            {
                foreach (var thief in thieves)
                {
                    var check = Players.FirstOrDefault(x => x.Id == thief.Choice);
                    if (check != null)
                    {
                        check.IsStolen = true;
                        thief.VotePower += 1;
                    }
                }
            }

            var gunners = Players.Where(x => x.PlayerRole == IRole.Gunner & !x.IsDead && x.Choice != 0 && x.Choice != -1);
            if (gunners.Any())
            {
                foreach (var gunner in gunners)
                {
                    var check = Players.FirstOrDefault(x => x.Id == gunner.Choice);
                    if (check != null)
                    {
                        //kill them
                        gunner.Bullet--;
                        gunner.HasUsedAbility = true;
                        check.IsDead = true;
                        if (check.PlayerRole == IRole.WolfCub)
                            WolfCubKilled = true;
                        if (check.PlayerRole == IRole.RabidWolf)
                            RabidKilled = true;
                        if (check.PlayerRole == IRole.Baker)
                            _bakerDied = true;
                        if (!BadRoles.Contains(check.PlayerRole))
                            gunner.BulletHitVillager = true;
                        else
                        {
                            gunner.PointsToAdd += 3;
                        }
                        check.TimeDied = DateTime.Now;
                        check.DayOfDeath = GameDay;
                        //update database
                        DBKill(gunner, check, KillMthd.Shoot);
                        DBAction(gunner, check, "Shoot");
                        switch (check.PlayerRole)
                        {
                            case IRole.Harlot:
                                SendWithQueue(/*DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ?*/ GetLocaleString("HarlotShot", gunner.GetName(), check.GetName()) /*: GetLocaleString("DefaultShot", gunner.GetName(), check.GetName(), "")*/);
                                gunner.PointsToAdd -= 3;
                                break;
                            case IRole.Hunter:
                                SendWithQueue(GetLocaleString("DefaultShot", gunner.GetName(), check.GetName(), /*!DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ? "" :*/ $"{check.GetName()} {GetLocaleString("Was")} {GetDescription(check)}"));
                                HunterFinalShot(check, KillMthd.Shoot);
                                break;
                            case IRole.WiseElder:
                                SendWithQueue(GetLocaleString("DefaultShot", gunner.GetName(), check.GetName(), /*!DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ? "" :*/ $"{check.GetName()} {GetLocaleString("Was")} {GetDescription(check)}"));
                                SendWithQueue(GetLocaleString("GunnerShotWiseElder", gunner.GetName(), check.GetName()));
                                gunner.PointsToAdd -= 3;
                                gunner.PlayerRole = IRole.Villager;
                                gunner.ChangedRolesCount++;
                                gunner.HasDayAction = false;
                                gunner.Bullet = 0;
                                break;
                            default:
                                SendWithQueue(GetLocaleString("DefaultShot", gunner.GetName(), check.GetName(), /*!DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ? "" :*/ $"{check.GetName()} {GetLocaleString("Was")} {GetDescription(check)}"));
                                break;
                        }
                        //check if dead was in love
                        if (check.InLove)
                            KillLover(check);
                    }
                }
            }
            CheckRoleChanges();
        }

        private void NightCycle()
        {
            if (!IsRunning) return;
            //FUN!
            Time = GameTime.Night;
            var nightStart = DateTime.Now;
            if (CheckForGameEnd(true)) return;
            foreach (var p in Players)
            {
                p.Choice = 0;
                p.Choice2 = 0;
                p.CurrentQuestion = null;
                p.Votes = 0;
                p.DiedLastNight = false;
                p.BeingVisitedSameNightCount = 0;
                if (p.Bitten && !p.IsDead && !WolfRoles.Contains(p.PlayerRole))
                {
                    if (p.PlayerRole == IRole.Mason)
                        foreach (var m in Players.Where(x => x.PlayerRole == IRole.Mason & !x.IsDead && x.Id != p.Id))
                            Send(GetLocaleString("MasonConverted", p.GetName()), m.Id);

                    p.Bitten = false;
                    if (p.RabidBitten)
                    {
                        p.PlayerRole = IRole.RabidWolf;
                        p.RabidBitten = false;
                        RabidKilled = false;
                    }
                    else if (p.PlayerRole == IRole.Detective)
                        p.PlayerRole = IRole.Snooper;
                    else if (p.PlayerRole == IRole.Ninja)
                        p.PlayerRole = IRole.SpeedWolf;
                    else
                        p.PlayerRole = IRole.Wolf;
                    p.Team = ITeam.Wolf;
                    p.HasDayAction = false;
                    p.HasNightAction = true;
                    p.RoleModel = 0;
                    p.ChangedRolesCount++;  //add count for double-shifter achv after converting to wolf
                    var msg = GetLocaleString("BittenTurned");
                    Players.GetPlayerForRole(IRole.AlphaWolf, false).AlphaConvertCount++;

                    Send(msg, p.Id);

                    SendWolfList(p, true);

                }
            }
            CheckRoleChanges();     //so maybe if seer got converted to wolf, appseer will promote here
            if (CheckForGameEnd()) return;
            var nightTime = (/*DbGroup.NightTime ?? */Settings.TimeNight);
            if (GameDay == 1)
                if (Players.Any(x => new[] { IRole.Cupid, IRole.Doppelgänger, IRole.WildChild, IRole.Imposter }.Contains(x.PlayerRole)))
                    nightTime = Math.Max(nightTime, 120);
            if (_sandmanSleep)
            {
                _sandmanSleep = false;
                _silverSpread = false; //reset blacksmith
                WolfCubKilled = false; //reset double kill
                SendWithQueue(GetLocaleString("SandmanNight"));
                return;
            }
            SendWithQueue(GetLocaleString("NightTime", nightTime.ToBold()));
            var moonMsg = GetLocaleString("NightPhase", GameDay.ToBold()) + " - ";
            var moonType = GameDay % 4;
            switch (moonType)
            {
                case 1:
                    moonMsg += GetLocaleString("FullMoon").ToBold();
                    break;
                case 2:
                    moonMsg += GetLocaleString("LastQuarter");
                    break;
                case 3:
                    moonMsg += GetLocaleString("NewMoon");
                    break;
                case 0:
                    moonMsg += GetLocaleString("FirstQuarter");
                    break;
            }
            SendWithQueue(moonMsg);
            SendPlayerList();


            // check if howlingwolf howls at the moon
            if (Players.Any(x => !x.IsDead && x.PlayerRole == IRole.HowlingWolf) && GameDay % 4 == 1)
            {
                HowledTonight = true;
                SendWithQueue(GetLocaleString("HowlingNight"));
            }

            SendNightActions();

            if (Inverted)
            {
                if (GameDay == 1)
                    SendWithQueue(GetLocaleString("StartingInverse"));
            }

            var nightPlayers = Players.Where(x => !x.IsDead & !x.Drunk && x.HasNightAction);
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            for (var i = 0; i < nightTime; i++)
            {
                Thread.Sleep(1000);
                if (CheckForGameEnd()) return;
                //check if all votes are cast

                if (nightPlayers.All(x => x.CurrentQuestion == null))
                    break;
            }

            try
            {
                foreach (var p in Players.Where(x => x.CurrentQuestion != null))
                {
                    try
                    {
                        if (p.CurrentQuestion.MessageId != 0)
                        {
                            Program.MessagesSent++;
                            Program.Bot.EditMessageTextAsync(p.Id, p.CurrentQuestion.MessageId, GetLocaleString("TimesUp"));
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    p.CurrentQuestion = null;
                }
            }
            catch
            {
                // ignored
            }

            //if first night, make sure cupid / wc / dg have picked
            ValidateSpecialRoleChoices();

            /* Role priority:
             * Seer / Fool / Sorcerer
             * Snow wolf / Snooper
             * Sleepwalker / Healer
             * GA / Guard
             * Ninja / Herbalist
             * Wolf
             * Pyro
             * Serial Killer
             * Cultist Hunter
             * Sheriff / Police
             * Cult
             * Harlot / Oracle / Lookout
             */

            #region Wolf Night

            #region Snow Wolf
            var snowwolves = Players.Where(x => x.PlayerRole == IRole.SnowWolf & !x.IsDead);
            if (snowwolves.Any())
            {
                foreach (var snowwolf in snowwolves)
                {
                    if (snowwolf != null && snowwolf.Choice != -1 && snowwolf.Choice != 0)
                    {
                        var target = Players.FirstOrDefault(x => x.Id == snowwolf.Choice);
                        if (target != null)
                        {
                            target.BeingVisitedSameNightCount++;
                            if (target.PlayerRole == IRole.SerialKiller)
                            {
                                if (Program.R.Next(100) < 20) // snowwolf has 20% chance of successfully freezing SK
                                {
                                    target.Frozen = true;
                                    Send(GetLocaleString("SKFrozen"), target.Id);
                                    Send(GetLocaleString("SuccessfulFreeze", target.GetName()), snowwolf.Id);
                                }
                                else
                                {
                                    snowwolf.IsDead = true;
                                    snowwolf.DiedLastNight = true;
                                    snowwolf.TimeDied = DateTime.Now;
                                    snowwolf.DiedByVisitingKiller = true;
                                    snowwolf.KilledByRole = IRole.SerialKiller;
                                    DBKill(target, snowwolf, KillMthd.SerialKilled);
                                }
                            }
                            else if (target.PlayerRole == IRole.Hunter)
                            {
                                if (Program.R.Next(100) < 50)
                                {
                                    target.Frozen = true;
                                    Send(GetLocaleString("DefaultFrozen"), target.Id);
                                    Send(GetLocaleString("SuccessfulFreeze", target.GetName()), snowwolf.Id);
                                }
                                else
                                {
                                    snowwolf.IsDead = true;
                                    snowwolf.TimeDied = DateTime.Now;
                                    snowwolf.DiedByVisitingKiller = true;
                                    snowwolf.KilledByRole = IRole.Hunter;
                                    snowwolf.DiedLastNight = true;
                                    DBKill(target, snowwolf, KillMthd.HunterShot);
                                }
                            }
                            else // go and freeze that player!
                            {
                                target.Frozen = true;
                                switch (target.PlayerRole)
                                {
                                    case IRole.Harlot:
                                        Send(GetLocaleString("HarlotFrozen"), target.Id);
                                        break;
                                    case IRole.Cultist:
                                        Send(GetLocaleString("CultistFrozen"), target.Id);
                                        break;
                                    case IRole.CultistHunter:
                                        Send(GetLocaleString("CHFrozen"), target.Id);
                                        break;
                                    case IRole.Fool:
                                    case IRole.Seer:
                                    case IRole.Sorcerer:
                                        Send(GetLocaleString("SeeingFrozenNext"), target.Id);
                                        break;
                                    case IRole.Oracle:
                                    case IRole.Healer:
                                        Send(GetLocaleString("SeeingFrozen"), target.Id);
                                        break;
                                    case IRole.GuardianAngel:
                                        Send(GetLocaleString("GAFrozen"), target.Id);
                                        break;
                                    case IRole.Pyro:
                                        Send(GetLocaleString("PyroFrozen"), target.Id);
                                        break;
                                    case IRole.Sheriff:
                                    case IRole.Police:
                                    case IRole.Herbalist:
                                    case IRole.Firefighter:
                                    case IRole.Miner:
                                        Send(GetLocaleString("VisitorFrozen"), target.Id);
                                        break;
                                    default:
                                        Send(GetLocaleString("DefaultFrozen"), target.Id);
                                        break;
                                }
                                Send(GetLocaleString("SuccessfulFreeze", target.GetName()), snowwolf.Id);
                            }
                        }
                    }
                }
            }
            #endregion

            #region Snooper
            var snoopers = Players.Where(x => x.PlayerRole == IRole.Snooper & !x.IsDead);
            if (snoopers.Any())
            {
                foreach (var snooper in snoopers)
                {
                    if (snooper != null && snooper.Choice != -1 && snooper.Choice != 0)
                    {
                        var target = Players.FirstOrDefault(x => x.Id == snooper.Choice);
                        if (target != null)
                        {
                            target.BeingVisitedSameNightCount++;
                            switch (target.PlayerRole)
                            {
                                case IRole.Drunk:
                                    snooper.PointsToAdd++;
                                    Send(GetLocaleString("SnoopDrunk", target.GetName(), GetRoleDescription(target.PlayerRole)), snooper.Id);
                                    break;
                                case IRole.Herbalist:
                                    snooper.PointsToAdd++;
                                    Send(GetLocaleString("SnoopHerbalist", target.GetName(), GetRoleDescription(target.PlayerRole)), snooper.Id);
                                    break;
                                case IRole.Baker:
                                    snooper.PointsToAdd++;
                                    Send(GetLocaleString("SnoopBaker", target.GetName(), GetRoleDescription(target.PlayerRole)), snooper.Id);
                                    break;
                                case IRole.AlphaWolf:
                                case IRole.Cultist:
                                case IRole.CultistHunter:
                                case IRole.Fool:
                                case IRole.GuardianAngel:
                                case IRole.Harlot:
                                case IRole.Healer:
                                case IRole.HungryWolf:
                                case IRole.Lycan:
                                case IRole.Ninja:
                                case IRole.Oracle:
                                case IRole.Police:
                                case IRole.Pyro:
                                case IRole.RabidWolf:
                                case IRole.Seer:
                                case IRole.SerialKiller:
                                case IRole.Sheriff:
                                case IRole.Snooper:
                                case IRole.SnowWolf:
                                case IRole.Sorcerer:
                                case IRole.Wolf:
                                case IRole.WolfCub:
                                case IRole.SpeedWolf:
                                case IRole.Lookout:
                                case IRole.Guard:
                                case IRole.HowlingWolf:
                                case IRole.Miner:
                                case IRole.Firefighter:
                                    Send(GetLocaleString("SnoopAction", target.GetName()), snooper.Id);
                                    break;
                                default:
                                    Send(GetLocaleString("SnoopNoAction", target.GetName()), snooper.Id);
                                    break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Sleepwalker / Healer
            // sleepwalker goes out
            var sleepwalkers = Players.Where(x => x.PlayerRole == IRole.Sleepwalker && !x.IsDead && !x.Frozen && !HowledTonight);
            if (sleepwalkers.Any())
            {
                foreach (var sleepwalker in sleepwalkers)
                {
                    sleepwalker.HasStayedHome = Program.R.Next(100) < 20;
                }
            }

            var healers = Players.Where(x => x.PlayerRole == IRole.Healer & !x.IsDead && !x.HasUsedAbility && !x.Frozen && !HowledTonight);
            if (healers.Any())
            {
                foreach (var healer in healers)
                {
                    var revived = Players.FirstOrDefault(x => x.Id == healer.Choice && x.IsDead);
                    if (revived != null)
                    {
                        revived.IsDead = false;
                        revived.RevivedLastNight = true;
                        revived.TimeDied = DateTime.MaxValue;
                        healer.HasUsedAbility = true;
                    }
                }
            }

            #endregion

            var gas = Players.Where(x => x.PlayerRole == IRole.GuardianAngel & !x.IsDead && x.Choice != 0 && x.Choice != -1 && !x.Frozen && !HowledTonight);

            var guards = Players.Where(x => x.PlayerRole == IRole.Guard & !x.IsDead && x.Choice != 0 && x.Choice != -1 && !x.Frozen && !HowledTonight);

            var fighters = Players.Where(x => x.PlayerRole == IRole.Firefighter & !x.IsDead && x.Choice != 0 && x.Choice != -1 && !x.Frozen && !HowledTonight);

            var ninjas = Players.Where(x => x.PlayerRole == IRole.Ninja & !x.IsDead && !x.Frozen && !HowledTonight);
            if (ninjas.Any())
            {
                foreach (var ninja in ninjas)
                {
                    if (ninja.Choice == 1) // if ninja decides to stay awake
                    {
                        ninja.HasUsedAbility = true;
                    }
                    else
                    {
                        ninja.HasUsedAbility = false;
                    }
                }
            }

            var herbalists = Players.Where(x => x.PlayerRole == IRole.Herbalist & !x.IsDead && !x.Frozen && !HowledTonight);
            if (herbalists.Any())
            {
                foreach (var herbalist in herbalists)
                {
                    if (herbalist.PoisonTarget == 0)
                    {
                        var poisoned = Players.FirstOrDefault(x => x.Id == herbalist.Choice && !x.IsDead);
                        if (poisoned != null)
                        {
                            var guarded = Program.R.Next(100) < (guards.Count(x => x.Choice == poisoned.Id) * 20);
                            poisoned.BeingVisitedSameNightCount++;
                            if (!gas.Any(x => x.Choice == poisoned.Id) && !guarded)
                            {
                                if (poisoned.PlayerRole == IRole.Ninja)
                                {
                                    if (poisoned.HasUsedAbility)
                                    {
                                        Send(GetLocaleString("NinjaHidden"), poisoned.Id);
                                    }
                                }
                                herbalist.PoisonTarget = poisoned.Id;
                                poisoned.PoisonedBy = herbalist.Id;
                                Send(GetLocaleString("HerbalistPoisonedYou"), poisoned.Id);
                            }
                            else
                            {
                                Send(GetLocaleString("GuardBlockedHerbalist", poisoned.GetName()), herbalist.Id);
                                Send(GetLocaleString("GuardSavedYou"), poisoned.Id);
                                if (guarded)
                                    poisoned.GuardedLastNight = true;
                                else
                                    poisoned.WasSavedLastNight = true;
                            }
                        }
                    }
                    else
                    {
                        var poisoned = Players.FirstOrDefault(x => x.Id == herbalist.PoisonTarget && !x.IsDead);
                        if (poisoned != null)
                        {
                            if (herbalist.Choice == 1)
                            {
                                poisoned.BeingVisitedSameNightCount++;
                                poisoned.PoisonedBy = 0;
                                Send(GetLocaleString("CurePoisoned", poisoned.GetName()), herbalist.Id);
                                Send(GetLocaleString("HerbalistCuredYou"), poisoned.Id);
                                poisoned.WasPoisoned = false;
                                herbalist.LastPoisoned = poisoned.Id; // for lookout
                            }
                        }
                        herbalist.PoisonTarget = 0;
                    }
                }
            }

            #region Other wolves

            var wolves = nightPlayers.GetPlayersForRoles(WolfRoles).ToList();

            var voteWolves = wolves.Where(x => !x.Drunk && !x.NoEat);
            var voteWolvesCount = voteWolves.Count();

            if (voteWolves.Any())
            {
                var votechoice = voteWolves.Where(x => (x.Choice != 0 && x.Choice != -1) || (x.Choice2 != 0 && x.Choice2 != -1));

                List<int> choices = new List<int>();

                //choice1
                foreach (var w in votechoice)
                {
                    var p = Players.Where(x => x.Id == w.Choice);
                    foreach (var pl in p)
                        pl.Votes++;
                }
                WolfTarget1 = Players.Where(x => x.Votes > 0).OrderByDescending(x => x.Votes).FirstOrDefault()?.Id ?? 0;
                choices.Add(WolfTarget1);


                //choice2 (will be 0 if wolfcub wasn't killed)
                foreach (var p in Players)
                    p.Votes = 0;
                foreach (var w in votechoice)
                {
                    var p = Players.Where(x => x.Id == w.Choice2 && x.Id != choices[0]);
                    foreach (var pl in p)
                        pl.Votes++;
                }
                WolfTarget2 = Players.Where(x => x.Votes > 0).OrderByDescending(x => x.Votes).FirstOrDefault()?.Id ?? 0;
                choices.Add(WolfTarget2);
                int eatCount = 0;
                int bittenCount = 0;

                if (Inverted)
                {
                    if (!choices.Any(x => x != 0 && x != -1))
                    {
                        var nonWolves = Players.Where(x => !x.IsDead && !WolfRoles.Contains(x.PlayerRole) && !SupportWolves.Contains(x.PlayerRole)).ToList();
                        if (nonWolves.Any())
                        {
                            nonWolves.Shuffle();
                            nonWolves.Shuffle();
                            choices.Add(nonWolves[0].Id);
                            foreach (var ww in voteWolves)
                            {
                                Send(GetLocaleString("RandomTargetChosen", nonWolves[0].GetName()), ww.Id);
                                ww.Choice = nonWolves[0].Id;
                            }
                            WolfTarget1 = nonWolves[0].Id;
                        }
                    }
                }

                foreach (var choice in choices.Where(x => x != 0 && x != -1))
                {
                    if (!voteWolves.Any()) break; //if wolf dies from first choice, and was alone...
                    var target = Players.FirstOrDefault(x => x.Id == choice & !x.IsDead);
                    if (target != null)
                    {
                        var guarded = Program.R.Next(100) < (guards.Count(x => x.Choice == target.Id) * 20);
                        var saved = fighters.Any(x => x.Choice == target.Id);
                        target.BeingVisitedSameNightCount++;
                        if ((gas.Any(x => x.Choice == target.Id) || guarded || saved) &&
                            !(target.PlayerRole == IRole.Harlot && (target.Choice == 0 || target.Choice == -1 || target.Frozen))) //doesn't apply to harlot not home
                        {
                            foreach (var wolf in voteWolves)
                            {
                                if (guarded)
                                {
                                    Send(GetLocaleString("GuardsStopWolf", target.GetName()), wolf.Id);
                                    target.GuardedLastNight = true;
                                }
                                else if (saved)
                                {
                                    Send(GetLocaleString("FightedWolf", target.GetName()), wolf.Id);
                                    target.FightedLastNight = true;
                                }
                                else
                                {
                                    Send(GetLocaleString("GuardBlockedWolf", target.GetName()), wolf.Id);
                                    target.WasSavedLastNight = true;
                                }
                            }
                            Send(GetLocaleString("GuardSavedYou"), target.Id);
                            if (gas.Any())
                            {
                                foreach (var ga in gas)
                                {
                                    if (ga.Choice == target.Id)
                                        ga.PointsToAdd += 5;
                                }
                            }
                            if (guards.Any())
                            {
                                foreach (var guard in guards)
                                {
                                    if (guard.Choice == target.Id)
                                        guard.PointsToAdd += 2;
                                }
                            }
                            if (fighters.Any())
                            {
                                foreach (var fighter in fighters)
                                {
                                    if (fighter.Choice == target.Id)
                                        fighter.PointsToAdd += 5;
                                }
                            }
                        }
                        else
                        {
                            var canBiteRabid = (RabidKilled && bittenCount == 0);
                            var bitten = voteWolves.Any(x => x.PlayerRole == IRole.AlphaWolf) && (Program.R.Next(100) < Settings.AlphaWolfConversionChance || canBiteRabid);
							var devoured = voteWolves.Any(x => x.PlayerRole == IRole.HungryWolf) && Program.R.Next(100) < 40;
                            var alpha = "";
                            if (bitten)
                                alpha = voteWolves.FirstOrDefault(x => x.PlayerRole == IRole.AlphaWolf).GetName();
							var hungryww = "";
                            if (devoured)
                                hungryww = voteWolves.FirstOrDefault(x => x.PlayerRole == IRole.HungryWolf).GetName();
                            //check if they are the harlot, and were home
                            switch (target.PlayerRole)
                            {
                                case IRole.Harlot:
                                    if (target.Choice == 0 || target.Choice == -1 || target.Frozen) //stayed home
                                    {
                                        if (bitten)
                                        {
                                            BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                            bittenCount++;
                                        }
                                        else
                                        {
                                            if (devoured)
                                            {
                                                DevourPlayer(target, voteWolves, hungryww);
                                            }
                                            target.DiedLastNight = true;
                                            target.IsDead = true;
                                            target.TimeDied = DateTime.Now;
                                            target.DayOfDeath = GameDay;
                                            target.KilledByRole = IRole.Wolf;
                                            DBKill(voteWolves, target, KillMthd.Eat);
                                            SendGif(GetLocaleString("WolvesEatYou"),
                                            GetRandomImage(VillagerDieImages), target.Id);
                                            //SendWithQueue(DbGroup.ShowRoles != false
                                            //    ? GetLocaleString("HarlotEaten", target.GetName())
                                            //    : GetLocaleString("GenericDeathNoReveal", target.GetName()));
                                        }
                                        foreach (var w in voteWolves)
                                            AddAchievement(w, Achievements.DontStayHome);
                                    }
                                    else
                                    {
                                        foreach (var wolf in voteWolves)
                                            Send(GetLocaleString("HarlotNotHome", target.GetName()), wolf.Id);
                                    }
                                    break;
                                case IRole.Sleepwalker:
                                    if (target.HasStayedHome)
                                    {
                                        if (bitten)
                                        {
                                            BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                            bittenCount++;
                                        }
                                        else
                                        {
                                            if (devoured)
                                            {
                                                DevourPlayer(target, voteWolves, hungryww);
                                            }
                                            target.DiedLastNight = true;
                                            target.IsDead = true;
                                            target.TimeDied = DateTime.Now;
                                            target.DayOfDeath = GameDay;
                                            target.KilledByRole = IRole.Wolf;
                                            DBKill(voteWolves, target, KillMthd.Eat);
                                            SendGif(GetLocaleString("WolvesEatYou"),
                                            GetRandomImage(VillagerDieImages), target.Id);
                                            //SendWithQueue(DbGroup.ShowRoles != false
                                            //    ? GetLocaleString("HarlotEaten", target.GetName())
                                            //    : GetLocaleString("GenericDeathNoReveal", target.GetName()));
                                        }
                                    }
                                    else
                                    {
                                        foreach (var wolf in voteWolves)
                                            Send(GetLocaleString("HarlotNotHome", target.GetName()), wolf.Id);
                                    }
                                    break;
                                case IRole.Ninja:
                                    if (!target.HasUsedAbility)
                                    {
                                        if (bitten)
                                        {
                                            BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                            bittenCount++;
                                        }
                                        else
                                        {
                                            if (devoured)
                                            {
                                                DevourPlayer(target, voteWolves, hungryww);
                                            }
                                            target.DiedLastNight = true;
                                            target.IsDead = true;
                                            target.TimeDied = DateTime.Now;
                                            target.DayOfDeath = GameDay;
                                            target.KilledByRole = IRole.Wolf;
                                            DBKill(voteWolves, target, KillMthd.Eat);
                                            SendGif(GetLocaleString("WolvesEatYou"),
                                            GetRandomImage(VillagerDieImages), target.Id);
                                            //SendWithQueue(DbGroup.ShowRoles != false
                                            //    ? GetLocaleString("HarlotEaten", target.GetName())
                                            //    : GetLocaleString("GenericDeathNoReveal", target.GetName()));
                                        }
                                    }
                                    else
                                    {
                                        foreach (var wolf in voteWolves)
                                            Send(GetLocaleString("HarlotNotHome", target.GetName()), wolf.Id);
                                        Send(GetLocaleString("NinjaHidden"), target.Id);
                                        target.PointsToAdd += 4;
                                        target.HasUsedAbility = false;
                                    }
                                    break;
                                case IRole.Pyro:
                                    foreach (var wolf in voteWolves)
                                        Send(GetLocaleString("HarlotNotHome", target.GetName()), wolf.Id);
                                    break;
                                case IRole.Cursed:
                                    target.PlayerRole = IRole.Wolf;
                                    target.Team = ITeam.Wolf;
                                    target.ChangedRolesCount++;
                                    target.HasNightAction = true;
                                    target.HasDayAction = false;
                                    var msg = GetLocaleString("CursedBitten");
                                    try
                                    {
                                        Send(msg, target.Id);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                    SendWolfList(target, true);
                                    foreach (var w in wolves)
                                        Send(GetLocaleString("CursedBittenToWolves", target.GetName()), w.Id);
                                    break;
                                case IRole.Drunk:
                                    if (bitten)
                                    {
                                        BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                        bittenCount++;
                                    }
                                    else
                                    {
                                        if (devoured)
                                        {
                                            DevourPlayer(target, voteWolves, hungryww);
                                        }
                                        target.PointsToAdd += 4;
                                        target.DiedLastNight = true;
                                        target.KilledByRole = IRole.Wolf;
                                        target.IsDead = true;
                                        target.TimeDied = DateTime.Now;
                                        target.DayOfDeath = GameDay;
                                        DBKill(voteWolves, target, KillMthd.Eat);
                                        SendGif(GetLocaleString("WolvesEatYou"),
                                            GetRandomImage(VillagerDieImages), target.Id);
                                        foreach (var w in voteWolves)
                                        {
                                            var secondvictim = Players.FirstOrDefault(x => x.Id == choices[1]);
                                            Send(
                                                target != (secondvictim ?? target) ? //if the drunk is the first victim out of two, they block the second one. let's tell wolves
                                                GetLocaleString("WolvesEatDrunkBlockSecondKill", target.GetName(), secondvictim.GetName()) :
                                                GetLocaleString("WolvesEatDrunk", target.GetName()), w.Id);
                                            w.Drunk = true;
                                        }
                                    }
                                    break;
                                case IRole.Hunter:
                                    //hunter has a chance to kill....
                                    voteWolvesCount = voteWolves.Count();
                                    //figure out what chance they have...
                                    var chance = Settings.HunterKillWolfChanceBase + ((voteWolves.Count() - 1) * 20);
                                    if (Program.R.Next(100) < chance)
                                    {
                                        //wolf dies!
                                        IPlayer shotWuff;
                                        try
                                        {
                                            shotWuff = voteWolves.ElementAt(Program.R.Next(voteWolves.Count()));
                                        }
                                        catch
                                        {
                                            shotWuff = voteWolves.FirstOrDefault();
                                        }
                                        if (shotWuff != null)
                                        {
                                            if (voteWolves.Count() > 1)
                                            {
                                                //commented out: we don't want the hunter to be bitten if he shot.
                                                //if (bitten)
                                                //{
                                                //    BitePlayer(target, voteWolves, alpha);
                                                //}
                                                //else
                                                {
                                                    SendGif(GetLocaleString("WolvesEatYou"),
                                                        GetRandomImage(VillagerDieImages), target.Id);
                                                    DBKill(voteWolves, target, KillMthd.Eat);
                                                    target.KilledByRole = IRole.Wolf;
                                                    target.IsDead = true;
                                                    target.TimeDied = DateTime.Now;
                                                    target.DayOfDeath = GameDay;
                                                    target.DiedLastNight = true;
                                                }
                                            }
                                            target.PointsToAdd += 3;
                                            shotWuff.IsDead = true;
                                            if (shotWuff.PlayerRole == IRole.WolfCub)
                                                WolfCubKilled = true;
                                            if (shotWuff.PlayerRole == IRole.RabidWolf)
                                                RabidKilled = true;
                                            shotWuff.TimeDied = DateTime.Now;
                                            shotWuff.DayOfDeath = GameDay;
                                            shotWuff.DiedByVisitingKiller = true;
                                            shotWuff.KilledByRole = IRole.Hunter;
                                            shotWuff.DiedLastNight = true;
                                            DBKill(target, shotWuff, KillMthd.HunterShot);
                                        }
                                    }
                                    else
                                    {
                                        if (bitten)
                                        {
                                            BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                            bittenCount++;
                                        }
                                        else
                                        {
                                            if (devoured)
                                            {
                                                DevourPlayer(target, voteWolves, hungryww);
                                            }
                                            SendGif(GetLocaleString("WolvesEatYou"),
                                                GetRandomImage(VillagerDieImages), target.Id);
                                            DBKill(voteWolves, target, KillMthd.Eat);
                                            target.KilledByRole = IRole.Wolf;
                                            target.IsDead = true;
                                            target.TimeDied = DateTime.Now;
                                            target.DayOfDeath = GameDay;
                                            target.DiedLastNight = true;
                                        }
                                    }
                                    break;
                                case IRole.SerialKiller:
                                    //serial killer has 80% of winning the fight....
                                    if (Program.R.Next(100) < 80)
                                    {
                                        //serial killer wins...
                                        IPlayer shotWuff;
                                        try
                                        {
                                            shotWuff = voteWolves.ElementAt(Program.R.Next(voteWolves.Count()));
                                        }
                                        catch
                                        {
                                            shotWuff = voteWolves.FirstOrDefault();
                                        }
                                        shotWuff.IsDead = true;
                                        if (shotWuff.PlayerRole == IRole.WolfCub)
                                            WolfCubKilled = true;
                                        if (shotWuff.PlayerRole == IRole.RabidWolf)
                                            RabidKilled = true;
                                        shotWuff.TimeDied = DateTime.Now;
                                        shotWuff.DayOfDeath = GameDay;
                                        shotWuff.DiedByVisitingKiller = true;
                                        shotWuff.KilledByRole = IRole.SerialKiller;
                                        shotWuff.DiedLastNight = true;
                                        //SendWithQueue(GetLocaleString("SerialKillerKilledWolf", shotWuff.GetName()));
                                        DBKill(target, shotWuff, KillMthd.SerialKilled);
                                    }
                                    else
                                    {
                                        if (bitten)
                                        {
                                            BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                            bittenCount++;
                                        }
                                        else
                                        {
                                            if (devoured)
                                            {
                                                DevourPlayer(target, voteWolves, hungryww);
                                            }
                                            target.KilledByRole = IRole.Wolf;
                                            target.IsDead = true;
                                            target.TimeDied = DateTime.Now;
                                            target.DayOfDeath = GameDay;
                                            target.DiedLastNight = true;
                                            DBKill(voteWolves, target, KillMthd.Eat);
                                            SendGif(GetLocaleString("WolvesEatYou"),
                                                GetRandomImage(VillagerDieImages), target.Id);
                                        }
                                    }
                                    break;
                                case IRole.WiseElder:
                                    if (bitten)
                                    {
                                        BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                        bittenCount++;
                                    }
                                    else
                                    {
                                        if (target.HasUsedAbility)
                                        {
                                            if (devoured)
                                            {
                                                DevourPlayer(target, voteWolves, hungryww);
                                            }
                                            target.KilledByRole = IRole.Wolf;
                                            target.IsDead = true;
                                            target.TimeDied = DateTime.Now;
                                            target.DayOfDeath = GameDay;
                                            target.DiedLastNight = true;
                                            DBKill(voteWolves, target, KillMthd.Eat);
                                            SendGif(GetLocaleString("WolvesEatYou"),
                                                GetRandomImage(VillagerDieImages), target.Id);
                                        }
                                        else
                                        {
                                            target.HasUsedAbility = true;
                                            foreach (var wolf in voteWolves)
                                                Send(GetLocaleString("WolvesTriedToEatWiseElder", target.GetName()), wolf.Id);
                                            Send(GetLocaleString("WolvesAteWiseElderPM"), target.Id);
                                        }
                                    }
                                    break;
                                default:
                                    if (bitten)
                                    {
                                        BitePlayer(target, voteWolves, alpha, canBiteRabid);
                                        bittenCount++;
                                    }
                                    else
                                    {
                                        if (devoured)
                                        {
                                            DevourPlayer(target, voteWolves, hungryww);
                                        }
                                        if (target.PlayerRole == IRole.Baker)
                                        {
                                            _bakerDied = true;
                                            target.PointsToAdd -= 1;
                                        }
                                        if (target.PlayerRole == IRole.ClumsyGuy || target.PlayerRole == IRole.WolfMan)
                                        {
                                            target.PointsToAdd += 2;
                                        }
                                        if (target.PlayerRole == IRole.Baker || target.PlayerRole == IRole.Seer || target.PlayerRole == IRole.GuardianAngel || target.PlayerRole == IRole.Healer || target.PlayerRole == IRole.Sheriff)
                                        {
                                            foreach (var w in votechoice)
                                            {
                                                if (w.Choice == target.Id || w.Choice2 == target.Id)
                                                {
                                                    w.PointsToAdd += 2;
                                                }
                                            }
                                        }
                                        target.KilledByRole = IRole.Wolf;
                                        target.IsDead = true;
                                        target.TimeDied = DateTime.Now;
                                        target.DayOfDeath = GameDay;
                                        target.DiedLastNight = true;
                                        if (target.PlayerRole == IRole.Sorcerer)
                                        {
                                            foreach (var w in voteWolves)
                                                AddAchievement(w, Achievements.NoSorcery);
                                        }
                                        DBKill(voteWolves, target, KillMthd.Eat);
                                        SendGif(GetLocaleString("WolvesEatYou"),
                                            GetRandomImage(VillagerDieImages), target.Id);
                                    }
                                    break;
                            }
                        }
                        eatCount++;
                    }
                    else
                    {
                        //no choice
                    }
                }
                if (eatCount == 2)
                {
                    var cub = Players.GetPlayersForRoles(new[] { IRole.WolfCub }, false).OrderByDescending(x => x.TimeDied).FirstOrDefault(x => x.IsDead);
                    if (cub != null)
                    {
                        AddAchievement(cub, Achievements.IHelped);
                        cub.PointsToAdd += 2;
                    }
                }
                eatCount = 0;
                bittenCount = 0;
            }
            WolfCubKilled = false;

            foreach (var ww in Players.Where(x => x.NoEat))
                ww.NoEat = false;
            #endregion

            #endregion

            #region Pyro Night

            var pyros = Players.Where(x => x.PlayerRole == IRole.Pyro & !x.IsDead && !HowledTonight);
            if (pyros.Any())
            {
                foreach (var arso in pyros)
                {
                    var doused = Players.FirstOrDefault(x => x.Id == arso.Choice && !x.IsDead);

                    if (Inverted)
                    {
                        if (doused == null)
                        {
                            var others = Players.Where(x => !x.IsDead && x.Id != arso.Id).ToList();
                            if (others.Any())
                            {
                                others.Shuffle();
                                others.Shuffle();
                                doused = others[0];
                                arso.Choice = doused.Id;
                                Send(GetLocaleString("RandomTargetChosen", others[0].GetName()), arso.Id);
                            }
                        }
                    }

                    if (doused != null)
                    {
                        var guarded = Program.R.Next(100) < (guards.Count(x => x.Choice == doused.Id) * 20);
                        var saved = fighters.Any(x => x.Choice == doused.Id);
                        if (doused.Id != arso.Id)
                        {
                            doused.BeingVisitedSameNightCount++;
                            if (!gas.Any(x => x.Choice == doused.Id) && !guarded && !saved)
                            {
                                if (doused.PlayerRole == IRole.Drunk)
                                {
                                    Send(GetLocaleString("DousedDrunkNight", doused.GetName()), arso.Id);
                                    doused.DiedLastNight = true;
                                    doused.IsDead = true;
                                    doused.TimeDied = DateTime.Now;
                                    doused.DayOfDeath = GameDay;
                                    doused.KilledByRole = IRole.Pyro;
                                    DBKill(arso, doused, KillMthd.Ignited);
                                    SendGif(GetLocaleString("PyroBurnedYou"),
                                            GetRandomImage(PyroBurn), doused.Id);
                                }
                                else if (doused.PlayerRole == IRole.Ninja)
                                {
                                    if (doused.HasUsedAbility)
                                    {
                                        Send(GetLocaleString("NinjaHidden"), doused.Id);
                                    }
                                    doused.IsDoused = true;
                                }
                                else if (doused.PlayerRole == IRole.Hunter && Program.R.Next(100) < 30)
                                {
                                    doused.IsDoused = true; // still douses hunter in case there is dg
                                    arso.DiedLastNight = true;
                                    arso.IsDead = true;
                                    arso.TimeDied = DateTime.Now;
                                    arso.DayOfDeath = GameDay;
                                    arso.KilledByRole = IRole.Hunter;
                                    DBKill(doused, arso, KillMthd.HunterShot);
                                }
                                else
                                {
                                    doused.IsDoused = true;
                                }
                            }
                            else
                            {
                                if (gas.Any())
                                {
                                    foreach (var ga in gas)
                                    {
                                        if (ga.Choice == doused.Id)
                                            ga.PointsToAdd += 5;
                                    }
                                }
                                if (fighters.Any())
                                {
                                    foreach (var fighter in fighters)
                                    {
                                        if (fighter.Choice == doused.Id)
                                            fighter.PointsToAdd += 5;
                                    }
                                }
                                if (guards.Any())
                                {
                                    foreach (var guard in guards)
                                    {
                                        if (guard.Choice == doused.Id)
                                            guard.PointsToAdd += 2;
                                    }
                                }
                                Send(GetLocaleString("GuardSavedYou"), doused.Id);
                                if (guarded)
                                {
                                    Send(GetLocaleString("GuardsStopArso", doused.GetName()), arso.Id);
                                    doused.GuardedLastNight = true;
                                }
                                else if (saved)
                                {
                                    Send(GetLocaleString("FightedArso", doused.GetName()), arso.Id);
                                    doused.FightedLastNight = true;
                                }
                                else
                                {
                                    Send(GetLocaleString("GuardBlockedArso", doused.GetName()), arso.Id);
                                    doused.WasSavedLastNight = true;
                                }
                            }
                        }
                        else // if pyro chooses themselves, kill all doused players
                        {
                            arso.IsDoused = false;
                            var ignited = Players.Where(x => x.IsDoused && !x.IsDead);
                            if (ignited.Any())
                            {
                                foreach (var igni in ignited)
                                {
                                    igni.DiedLastNight = true;
                                    igni.IsDead = true;
                                    if (igni.PlayerRole == IRole.WolfCub)
                                        WolfCubKilled = true;
                                    if (igni.PlayerRole == IRole.RabidWolf)
                                        RabidKilled = true;
                                    if (igni.PlayerRole == IRole.Baker)
                                    {
                                        igni.PointsToAdd -= 1;
                                        _bakerDied = true;
                                    }
                                    if (igni.PlayerRole == IRole.ClumsyGuy || igni.PlayerRole == IRole.WolfMan)
                                    {
                                        igni.PointsToAdd += 2;
                                    }
                                    if (igni.PlayerRole == IRole.Baker || igni.PlayerRole == IRole.Seer || igni.PlayerRole == IRole.GuardianAngel || igni.PlayerRole == IRole.Healer || igni.PlayerRole == IRole.Sheriff)
                                    {
                                        arso.PointsToAdd += 2;
                                    }
                                    igni.TimeDied = DateTime.Now;
                                    igni.DayOfDeath = GameDay;
                                    igni.KilledByRole = IRole.Pyro;
                                    DBKill(arso, igni, KillMthd.Ignited);
                                    SendGif(GetLocaleString("PyroBurnedYou"),
                                            GetRandomImage(PyroBurn), igni.Id);
                                }
                            }
                        }
                    }
                }
            }

#endregion

#region Serial Killer Night

            //give serial killer a chance!
            var sks = Players.Where(x => x.PlayerRole == IRole.SerialKiller && !x.IsDead && !x.Frozen && !HowledTonight);
            if (sks.Any())
            {
                foreach (var sk in sks)
                {
                    var skilled = Players.FirstOrDefault(x => x.Id == sk.Choice && !x.IsDead);

                    if (Inverted)
                    {
                        if (skilled == null)
                        {
                            var others = Players.Where(x => !x.IsDead && x.Id != sk.Id).ToList();
                            if (others.Any())
                            {
                                others.Shuffle();
                                others.Shuffle();
                                skilled = others[0];
                                sk.Choice = skilled.Id;
                                Send(GetLocaleString("RandomTargetChosen", others[0].GetName()), sk.Id);
                            }
                        }
                    }

                    if (skilled != null)
                    {
                        var guarded = Program.R.Next(100) < (guards.Count(x => x.Choice == skilled.Id) * 20);
                        skilled.BeingVisitedSameNightCount++;
                        if (gas.Any(x => x.Choice == skilled.Id) || guarded)
                        {
                            if (gas.Any())
                            {
                                foreach (var ga in gas)
                                {
                                    if (ga.Choice == skilled.Id)
                                        ga.PointsToAdd += 5;
                                }
                            }
                            if (guards.Any())
                            {
                                foreach (var guard in guards)
                                {
                                    if (guard.Choice == skilled.Id)
                                        guard.PointsToAdd += 2;
                                }
                            }
                            if (guarded)
                            {
                                var deadGuard = guards.FirstOrDefault(x => x.Choice == skilled.Id && !x.IsDead);
                                if (deadGuard != null)
                                {
                                    deadGuard.IsDead = true;
                                    deadGuard.TimeDied = DateTime.Now;
                                    deadGuard.DiedLastNight = true;
                                    deadGuard.DayOfDeath = GameDay;
                                    deadGuard.DiedByVisitingVictim = true;
                                    deadGuard.KilledByRole = IRole.SerialKiller;
                                    deadGuard.RoleModel = skilled.Id; //store who they visited
                                    DBKill(skilled, deadGuard, KillMthd.VisitVictim);
                                }
                                Send(GetLocaleString("GuardsStopKiller", skilled.GetName()), sk.Id);
                                skilled.GuardedLastNight = true;
                            }
                            else
                            {
                                Send(GetLocaleString("GuardBlockedKiller", skilled.GetName()), sk.Id);
                                skilled.WasSavedLastNight = true;
                            }
                            Send(GetLocaleString("GuardSavedYou"), skilled.Id);
                        }
                        else if (skilled.PlayerRole == IRole.Ninja && skilled.HasUsedAbility)
                        {
                            if (Program.R.Next(100) < 50)
                            {
                                skilled.DiedLastNight = true;
                                skilled.IsDead = true;
                                skilled.TimeDied = DateTime.Now;
                                skilled.DayOfDeath = GameDay;
                                skilled.DiedByVisitingVictim = true;
                                skilled.KilledByRole = IRole.SerialKiller;
                                DBKill(sk, skilled, KillMthd.SerialKilled);
                                Send(GetLocaleString("NinjaFailed"), skilled.Id);
                            }
                            else
                            {
                                Send(GetLocaleString("NinjaHidden"), skilled.Id);
                                skilled.PointsToAdd += 4;
                                skilled.HasUsedAbility = false;
                            }
                        }
                        else
                        {
                            skilled.DiedLastNight = true;
                            skilled.IsDead = true;
                            if (skilled.PlayerRole == IRole.WolfCub)
                                WolfCubKilled = true;
                            if (skilled.PlayerRole == IRole.RabidWolf)
                                RabidKilled = true;
                            if (skilled.PlayerRole == IRole.Baker)
                            {
                                skilled.PointsToAdd -= 1;
                                _bakerDied = true;
                            }
                            if (skilled.PlayerRole == IRole.ClumsyGuy || skilled.PlayerRole == IRole.WolfMan)
                            {
                                skilled.PointsToAdd += 2;
                            }
                            if (skilled.PlayerRole == IRole.Baker || skilled.PlayerRole == IRole.Seer || skilled.PlayerRole == IRole.GuardianAngel || skilled.PlayerRole == IRole.Healer || skilled.PlayerRole == IRole.Sheriff)
                            {
                                sk.PointsToAdd += 2;
                            }
                            skilled.TimeDied = DateTime.Now;
                            skilled.DayOfDeath = GameDay;
                            skilled.KilledByRole = IRole.SerialKiller;
                            DBKill(sk, skilled, KillMthd.SerialKilled);
                            SendGif(GetLocaleString("SKStabbedYou"),
                                            GetRandomImage(SkStab), skilled.Id);
                            if (WolfRoles.Contains(skilled.PlayerRole))
                                sk.SerialKilledWolvesCount++;
                        }
                    }
                }
            }

#endregion

            if (Players == null)
                return;

#region Cult Hunter Night

            //cult hunter
            var hunters = Players.Where(x => x.PlayerRole == IRole.CultistHunter && !x.Frozen && !HowledTonight);
            if (hunters.Any())
            {
                foreach (var hunter in hunters)
                {
                    var hunted = Players.FirstOrDefault(x => x.Id == hunter.Choice);

                    if (Inverted)
                    {
                        if (hunted == null)
                        {
                            var others = Players.Where(x => !x.IsDead && x.Id != hunter.Id).ToList();
                            if (others.Any())
                            {
                                others.Shuffle();
                                others.Shuffle();
                                hunted = others[0];
                                hunter.Choice = hunted.Id;
                                Send(GetLocaleString("RandomTargetChosen", others[0].GetName()), hunter.Id);
                            }
                        }
                    }

                    if (hunted != null)
                    {
                        hunted.BeingVisitedSameNightCount++;
                        DBAction(hunter, hunted, "Hunt");
                        if (hunted.PlayerRole == IRole.SerialKiller)
                        {
                            //awwwwww CH gets popped
                            DBKill(hunted, hunter, KillMthd.SerialKilled);
                            hunter.IsDead = true;
                            hunter.TimeDied = DateTime.Now;
                            hunter.DayOfDeath = GameDay;
                            hunter.DiedLastNight = true;
                            hunter.KilledByRole = IRole.SerialKiller;
                            hunter.DiedByVisitingKiller = true;
                        }
                        else if (hunted.IsDead)
                        {
                            Send(GetLocaleString("HunterVisitDead", hunted.GetName()), hunter.Id);
                        }
                        else if (hunted.PlayerRole == IRole.Cultist)
                        {
                            Send(GetLocaleString("HunterFindCultist", hunted.GetName()), hunter.Id);
                            hunter.PointsToAdd += 1;
                            hunted.IsDead = true;
                            hunted.TimeDied = DateTime.Now;
                            hunted.DiedLastNight = true;
                            hunted.DayOfDeath = GameDay;
                            hunter.CHHuntedCultCount++;
                            hunted.KilledByRole = IRole.CultistHunter;
                            DBKill(hunter, hunted, KillMthd.Hunt);
                        }
                        else
                        {
                            if (hunted.PlayerRole == IRole.Ninja && hunted.HasUsedAbility)
                            {
                                Send(GetLocaleString("NinjaHidden"), hunted.Id);
                            }
                            Send(GetLocaleString("HunterFailedToFind", hunted.GetName()), hunter.Id);
                        }
                    }
                }
            }
            #endregion

            #region Sheriff Night

            var sheriffs = Players.Where(x => x.PlayerRole == IRole.Sheriff && !x.Frozen && !HowledTonight);
            if (sheriffs.Any())
            {
                foreach (var sheriff in sheriffs)
                {
                    var targetS = Players.FirstOrDefault(x => x.Id == sheriff.Choice);
                    if (targetS != null)
                    {
                        targetS.IsSheriffed = true;
                        targetS.BeingVisitedSameNightCount++;
                        switch (targetS.PlayerRole)
                        {
                            case IRole.SerialKiller:
                                sheriff.PointsToAdd += 1;
                                DBKill(targetS, sheriff, KillMthd.SerialKilled);
                                sheriff.IsDead = true;
                                sheriff.TimeDied = DateTime.Now;
                                sheriff.DayOfDeath = GameDay;
                                sheriff.DiedLastNight = true;
                                sheriff.KilledByRole = IRole.SerialKiller;
                                sheriff.DiedByVisitingKiller = true;
                                break;
                            case IRole.Wolf:
                            case IRole.AlphaWolf:
                            case IRole.WolfCub:
                            case IRole.Lycan:
                            case IRole.HungryWolf:
                            case IRole.RabidWolf:
                            case IRole.Cultist:
                            case IRole.Pyro:
                            case IRole.SnowWolf:
                            case IRole.Snooper:
                            case IRole.SpeedWolf:
                            case IRole.HowlingWolf:
                                sheriff.PointsToAdd += 1;
                                if (targetS.Choice == 0 || targetS.Choice == -1 || targetS.Frozen) // stayed home
                                    Send(GetLocaleString("SheriffVisitHome", targetS.GetName()), sheriff.Id);
                                else
                                {
                                    Send(GetLocaleString("SheriffVisitNotHome", targetS.GetName()), sheriff.Id);
                                }
                                break;
                            case IRole.CultistHunter:
                            case IRole.Harlot:
                            case IRole.GuardianAngel:
                            case IRole.Sheriff:
                            case IRole.Police:
                            case IRole.Herbalist:
                            case IRole.Lookout:
                            case IRole.Guard:
                            case IRole.Firefighter:
                            case IRole.Miner:
                                if (targetS.Choice == 0 || targetS.Choice == -1 || targetS.Frozen) // stayed home
                                    Send(GetLocaleString("SheriffVisitHome", targetS.GetName()), sheriff.Id);
                                else
                                {
                                    Send(GetLocaleString("SheriffVisitNotHome", targetS.GetName()), sheriff.Id);
                                }
                                break;
                            case IRole.Sleepwalker:
                                if (targetS.HasStayedHome)
                                {
                                    Send(GetLocaleString("SheriffVisitHome", targetS.GetName()), sheriff.Id);
                                }
                                else
                                {
                                    Send(GetLocaleString("SheriffVisitNotHome", targetS.GetName()), sheriff.Id);
                                }
                                break;
                            case IRole.Ninja:
                                if (targetS.HasUsedAbility)
                                {
                                    Send(GetLocaleString("SheriffVisitNotHome", targetS.GetName()), sheriff.Id);
                                    Send(GetLocaleString("NinjaHidden"), targetS.Id);
                                }
                                else
                                {
                                    Send(GetLocaleString("SheriffVisitHome", targetS.GetName()), sheriff.Id);
                                }
                                break;
                            default:
                                Send(GetLocaleString("SheriffVisitHome", targetS.GetName()), sheriff.Id);
                                break;
                        }
                    }
                }
            }

            #endregion

#region Police Night

            var polis = Players.Where(x => x.PlayerRole == IRole.Police && !x.IsDead && !x.Frozen && !HowledTonight);
            if (polis.Any())
            {
                foreach (var poli in polis)
                {
                    var target = Players.FirstOrDefault(x => x.Id == poli.Choice);
                    if (target != null)
                    {
                        DBAction(poli, target, "PoliceSee");
                        var role = target.PlayerRole;
                        switch (role)
                        {
                            case IRole.WolfCub:
                            case IRole.AlphaWolf:
                            case IRole.HungryWolf:
                            case IRole.RabidWolf:
                            case IRole.Lycan:
                            case IRole.SerialKiller:
                            case IRole.Wolf:
                            case IRole.Cultist:
                            case IRole.Pyro:
                            case IRole.SnowWolf:
                            case IRole.Snooper:
                            case IRole.SpeedWolf:
                            case IRole.HowlingWolf:
                                poli.PointsToAdd += 1;
                                Send(GetLocaleString("PoliceGuilty", target.GetName()), poli.Id);
                                break;
                            default:
                                if (target.PlayerRole == IRole.Ninja && target.HasUsedAbility)
                                {
                                    Send(GetLocaleString("NinjaHidden"), target.Id);
                                }
                                if (Program.R.Next(100) < 50)
                                    Send(GetLocaleString("PoliceInnocent", target.GetName()), poli.Id);
                                else
                                    Send(GetLocaleString("PoliceGuilty", target.GetName()), poli.Id);
                                break;
                        }
                    }
                }
            }

            #endregion

            #region Cult Night

            //CULT
            var voteCult = Players.Where(x => x.PlayerRole == IRole.Cultist && !x.IsDead && !x.Frozen && !HowledTonight);

            if (voteCult.Any())
            {
                foreach (var c in voteCult)
                {
                    var cchoice = Players.FirstOrDefault(x => x.Id == c.Choice);
                    if (cchoice != null)
                    {
                        DBAction(c, cchoice, "Convert");
                    }
                }
                var votechoice = voteCult.Where(x => x.Choice != 0 && x.Choice != -1);
                int choice = 0;
                if (votechoice.Any())
                {
                    choice = votechoice.GroupBy(x => x.Choice).OrderByDescending(x => x.Count()).First().Key;
                }

                if (Inverted)
                {
                    if (choice == 0 || choice == -1)
                    {
                        var others = Players.Where(x => !x.IsDead && x.PlayerRole != IRole.Cultist).ToList();
                        if (others.Any())
                        {
                            others.Shuffle();
                            others.Shuffle();
                            choice = others[0].Id;
                            foreach (var cult in voteCult)
                            {
                                Send(GetLocaleString("RandomTargetChosen", others[0].GetName()), cult.Id);
                                cult.Choice = choice;
                            }
                        }
                    }
                }

                CultTarget = choice;

                if (choice != 0 && choice != -1)
                {
                    var target = Players.FirstOrDefault(x => x.Id == choice & !x.IsDead);
                    if (target != null)
                    {
                        target.BeingVisitedSameNightCount++;
                        if (target.IsSheriffed)
                        {
                            foreach (var c in voteCult)
                                Send(GetLocaleString("CultTargetSheriffed", target.GetName()), c.Id);
                            target.IsSheriffed = false;
                        }
                        else if (!target.IsDead)
                        {
                            var newbie = voteCult.OrderByDescending(x => x.DayCult).First();
                            //check if they are the hunter
                            switch (target.PlayerRole)
                            {
                                case IRole.Hunter:
                                    //first, check if they got converted....
                                    if (Program.R.Next(100) < Settings.HunterConversionChance)
                                    {
                                        ConvertToCult(target, voteCult);
                                    }
                                    else
                                    {
                                        if (Program.R.Next(100) < Settings.HunterKillCultChance)
                                        {
                                            newbie.DiedLastNight = true;
                                            newbie.IsDead = true;
                                            newbie.TimeDied = DateTime.Now;
                                            newbie.DayOfDeath = GameDay;
                                            newbie.KilledByRole = IRole.Hunter;
                                            newbie.DiedByVisitingKiller = true;
                                            DBKill(target, newbie, KillMthd.HunterCult);
                                            //notify everyone
                                            foreach (var c in voteCult)
                                            {
                                                Send(GetLocaleString("CultConvertHunter", newbie.GetName(), target.GetName()), c.Id);
                                            }
                                        }
                                        else
                                        {
                                            foreach (var c in voteCult)
                                            {
                                                Send(GetLocaleString("CultUnableToConvert", newbie.GetName(), target.GetName()), c.Id);
                                            }
                                            Send(GetLocaleString("CultAttempt"), target.Id);
                                        }
                                    }
                                    break;
                                case IRole.SerialKiller:
                                    //kill newest cult
                                    newbie.DiedLastNight = true;
                                    newbie.IsDead = true;
                                    newbie.DayOfDeath = GameDay;
                                    newbie.TimeDied = DateTime.Now;
                                    newbie.KilledByRole = IRole.SerialKiller;
                                    newbie.DiedByVisitingKiller = true;
                                    DBKill(target, newbie, KillMthd.SerialKilled);
                                    foreach (var c in voteCult)
                                    {
                                        Send(GetLocaleString("CultConvertSerialKiller", newbie.GetName(), target.GetName()), c.Id);
                                    }
                                    //SendWithQueue(GetLocaleString("DefaultKilled", newbie.GetName(), DbGroup.ShowRoles == false ? "" : $"{GetDescription(newbie)} {GetLocaleString("IsDead")}"));
                                    break;
                                case IRole.CultistHunter:
                                    //kill the newest cult member
                                    newbie.DiedLastNight = true;
                                    newbie.IsDead = true;
                                    newbie.DayOfDeath = GameDay;
                                    newbie.KilledByRole = IRole.CultistHunter;
                                    newbie.DiedByVisitingKiller = true;
                                    AddAchievement(newbie, Achievements.CultFodder);
                                    newbie.TimeDied = DateTime.Now;
                                    DBKill(target, newbie, KillMthd.Hunt);
                                    //notify everyone
                                    foreach (var c in voteCult)
                                    {
                                        Send(GetLocaleString("CultConvertCultHunter", newbie.GetName(), target.GetName()), c.Id);
                                    }
                                    Send(GetLocaleString("CultHunterKilledCultVisit", newbie.GetName(), voteCult.Count()), target.Id);
                                    break;
                                case IRole.Mason:
                                    //notify other masons....
                                    ConvertToCult(target, voteCult);
                                    foreach (var m in Players.Where(x => x.PlayerRole == IRole.Mason & !x.IsDead))
                                        Send(GetLocaleString("MasonConverted", target.GetName()), m.Id);
                                    break;
                                case IRole.Wolf:
                                case IRole.AlphaWolf:
                                case IRole.WolfCub:
                                case IRole.Lycan:
                                case IRole.HungryWolf:
                                case IRole.RabidWolf:
                                case IRole.SpeedWolf:
                                case IRole.HowlingWolf:
                                    if (voteWolves.Any(x => (x.Choice != 0 && x.Choice != -1) || (x.Choice2 != 0 && x.Choice2 != -1))) //did wolves go eating?
                                    {
                                        foreach (var c in voteCult)
                                        {
                                            Send(GetLocaleString("CultVisitEmpty", newbie.GetName(), target.GetName()), c.Id);
                                        }
                                    }
                                    else //stayed home!
                                    {
                                        //kill the newest cult member
                                        newbie.DiedLastNight = true;
                                        newbie.IsDead = true;
                                        newbie.TimeDied = DateTime.Now;
                                        newbie.DayOfDeath = GameDay;
                                        newbie.KilledByRole = IRole.Wolf;
                                        newbie.DiedByVisitingKiller = true;
                                        DBKill(target, newbie, KillMthd.Eat);

                                        foreach (var c in voteCult)
                                        {
                                            Send(GetLocaleString("CultConvertWolf", newbie.GetName(), target.GetName()), c.Id);
                                        }
                                        Send(GetLocaleString("CultAttempt"), target.Id); //only notify if they were home
                                    }
                                    break;
                                case IRole.SnowWolf:
                                case IRole.Snooper:
                                    if (target.Choice != -1 && target.Choice != 0) // did snow wolf go freezing?
                                    {
                                        foreach (var c in voteCult)
                                        {
                                            Send(GetLocaleString("CultVisitEmpty", newbie.GetName(), target.GetName()), c.Id);
                                        }
                                    }
                                    else // stayed home!
                                    {
                                        newbie.DiedLastNight = true;
                                        newbie.IsDead = true;
                                        newbie.TimeDied = DateTime.Now;
                                        newbie.KilledByRole = IRole.Wolf;
                                        newbie.DiedByVisitingKiller = true;
                                        DBKill(target, newbie, KillMthd.Eat);

                                        foreach (var c in voteCult)
                                        {
                                            Send(GetLocaleString("CultConvertWolf", newbie.GetName(), target.GetName()), c.Id);
                                        }
                                        Send(GetLocaleString("CultAttempt"), target.Id); //only notify if they were home
                                    }
                                    break;
                                case IRole.GuardianAngel:
                                    if (target.Choice == 0 || target.Choice == -1 || target.Frozen) // stayed home
                                        ConvertToCult(target, voteCult, Settings.GuardianAngelConversionChance);
                                    else
                                    {
                                        foreach (var c in voteCult)
                                            Send(GetLocaleString("CultVisitEmpty", newbie.GetName(), target.GetName()), c.Id);
                                        //Send(GetLocaleString("CultAttempt"), target.Id);
                                    }
                                    break;
                                case IRole.Harlot:
                                    if (target.Choice == 0 || target.Choice == -1 || target.Frozen) // stayed home
                                        ConvertToCult(target, voteCult, Settings.HarlotConversionChance);
                                    else
                                    {
                                        foreach (var c in voteCult)
                                            Send(GetLocaleString("CultVisitEmpty", newbie.GetName(), target.GetName()), c.Id);
                                        //Send(GetLocaleString("CultAttempt"), target.Id);
                                    }
                                    break;
                                case IRole.Pyro:
                                    if (target.Choice == 0 || target.Choice == -1) // stayed home
                                        ConvertToCult(target, voteCult, 0);
                                    else
                                    {
                                        foreach (var c in voteCult)
                                            Send(GetLocaleString("CultVisitEmpty", newbie.GetName(), target.GetName()), c.Id);
                                    }
                                    break;
                                case IRole.Sleepwalker:
                                    if (target.HasStayedHome) // stayed home
                                        ConvertToCult(target, voteCult);
                                    else
                                    {
                                        foreach (var c in voteCult)
                                            Send(GetLocaleString("CultVisitEmpty", newbie.GetName(), target.GetName()), c.Id);
                                    }
                                    break;
                                case IRole.Ninja:
                                    if (target.HasUsedAbility)
                                    {
                                        Send(GetLocaleString("NinjaHidden"), target.Id);
                                        target.PointsToAdd += 2;
                                        target.HasUsedAbility = false;
                                        foreach (var c in voteCult)
                                            Send(GetLocaleString("CultVisitEmpty", newbie.GetName(), target.GetName()), c.Id);
                                    }
                                    else
                                    {
                                        ConvertToCult(target, voteCult);
                                    }
                                    break;
                                case IRole.Seer:
                                case IRole.Herbalist:
                                case IRole.Sorcerer:
                                    ConvertToCult(target, voteCult, Settings.SeerConversionChance);
                                    break;
                                case IRole.Blacksmith:
                                    ConvertToCult(target, voteCult, Settings.BlacksmithConversionChance);
                                    break;
                                case IRole.Detective:
                                    ConvertToCult(target, voteCult, Settings.DetectiveConversionChance);
                                    break;
                                case IRole.Cursed:
                                case IRole.Lookout:
                                    ConvertToCult(target, voteCult, Settings.CursedConversionChance);
                                    break;
                                case IRole.Doppelgänger:
                                case IRole.Atheist:
                                    ConvertToCult(target, voteCult, 0);
                                    break;
                                case IRole.Oracle:
                                    ConvertToCult(target, voteCult, Settings.OracleConversionChance);
                                    break;
                                case IRole.Sandman:
                                    ConvertToCult(target, voteCult, Settings.SandmanConversionChance);
                                    break;
                                case IRole.Sheriff:
                                    ConvertToCult(target, voteCult, Settings.SheriffConversionChance);
                                    break;
                                case IRole.Healer:
                                    ConvertToCult(target, voteCult, Settings.HealerConversionChance);
                                    break;
                                case IRole.Pacifist:
                                    ConvertToCult(target, voteCult, Settings.PacifistConversionChance);
                                    break;
                                case IRole.WiseElder:
                                    ConvertToCult(target, voteCult, Settings.WiseElderConversionChance);
                                    break;
                                case IRole.Firefighter:
                                    ConvertToCult(target, voteCult, Settings.FirefighterConversionChance);
                                    break;
                                case IRole.Ghost:
                                    ConvertToCult(target, voteCult, Settings.GhostConversionChance);
                                    break;
                                default:
                                    ConvertToCult(target, voteCult);
                                    break;
                            }
                        }
                        else
                        {
                            foreach (var c in voteCult)
                                Send(GetLocaleString("CultTargetDead", target.GetName()), c.Id);
                        }
                    }
                }
            }

            //reset sheriffed
            var alivePlayers = Players.Where(x => x.IsSheriffed);
            foreach (var aliveOne in alivePlayers)
                aliveOne.IsSheriffed = false;

#endregion

#region Harlot Night

            //let the harlot know
            var harlots = Players.Where(x => x.PlayerRole == IRole.Harlot && !x.IsDead && !x.Frozen && !HowledTonight);
            if (harlots.Any())
            {
                foreach (var harlot in harlots)
                {
                    var target = Players.FirstOrDefault(x => x.Id == harlot.Choice);
                    if (target != null)
                    {

                        DBAction(harlot, target, "Fuck");
                        if (harlot.PlayersVisited.Contains(target.TeleUser.Id))
                            harlot.HasRepeatedVisit = true;
                        else
                            harlot.PointsToAdd += 0.5;

                        harlot.PlayersVisited.Add(target.TeleUser.Id);
                    }
                    else
                    {
                        //stayed home D:
                        harlot.HasStayedHome = true;
                    }
                    if (!harlot.IsDead)
                    {
                        if (target != null)
                        {
                            target.BeingVisitedSameNightCount++;
                            switch (target.PlayerRole)
                            {
                                case IRole.Wolf:
                                case IRole.AlphaWolf:
                                case IRole.WolfCub:
                                case IRole.Lycan:
                                case IRole.HungryWolf:
                                case IRole.RabidWolf:
                                case IRole.SnowWolf:
                                case IRole.Snooper:
                                case IRole.SpeedWolf:
                                case IRole.HowlingWolf:
                                    harlot.IsDead = true;
                                    harlot.TimeDied = DateTime.Now;
                                    harlot.DiedLastNight = true;
                                    harlot.DayOfDeath = GameDay;
                                    harlot.DiedByVisitingKiller = true;
                                    harlot.KilledByRole = IRole.Wolf;
                                    DBKill(target, harlot, KillMthd.VisitWolf);
                                    Send(GetLocaleString("HarlotFuckWolf", target.GetName()), harlot.Id);
                                    break;
                                case IRole.SerialKiller:
                                    harlot.IsDead = true;
                                    harlot.TimeDied = DateTime.Now;
                                    harlot.DiedLastNight = true;
                                    harlot.DayOfDeath = GameDay;
                                    harlot.DiedByVisitingKiller = true;
                                    harlot.KilledByRole = IRole.SerialKiller;
                                    DBKill(target, harlot, KillMthd.VisitKiller);
                                    Send(GetLocaleString("HarlotFuckKiller", target.GetName()), harlot.Id);
                                    break;
                                default:
                                    if (target.DiedLastNight && (WolfRoles.Contains(target.KilledByRole) || target.KilledByRole == IRole.SerialKiller) && !target.DiedByVisitingKiller)
                                    {
                                        harlot.IsDead = true;
                                        harlot.TimeDied = DateTime.Now;
                                        harlot.DiedLastNight = true;
                                        harlot.DayOfDeath = GameDay;
                                        harlot.DiedByVisitingVictim = true;
                                        harlot.KilledByRole = target.KilledByRole;
                                        harlot.RoleModel = target.Id; //store who they visited
                                        DBKill(target, harlot, KillMthd.VisitVictim);
                                    }
                                    else
                                    {
                                        Send(
                                            (target.PlayerRole == IRole.Cultist && Program.R.Next(100) < Settings.HarlotDiscoverCultChance) ?
                                                GetLocaleString("HarlotDiscoverCult", target.GetName()) :
                                                GetLocaleString("HarlotVisitNonWolf", target.GetName()),
                                            harlot.Id);
                                        if (!target.IsDead)
                                            Send(GetLocaleString("HarlotVisitYou"), target.Id);
                                        if (target.PlayerRole == IRole.Cultist)
                                            harlot.PointsToAdd += 1;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

#endregion

#region Oracle / Lookout

            //let the seer know
            /*var seers = Players.Where(x => x.PlayerRole == IRole.Seer && !x.IsDead);
            if (seers.Any())
            {
                foreach (var seer in seers)
                {
                    
                }
            }
            var sorcerers = Players.Where(x => x.PlayerRole == IRole.Sorcerer && !x.IsDead);
            if (sorcerers.Any())
            {
                foreach (var sorcerer in sorcerers)
                {
                    var target = Players.FirstOrDefault(x => x.Id == sorcerer.Choice);
                    
                }

            }
            var fools = Players.Where(x => x.PlayerRole == IRole.Fool && !x.IsDead);
            if (fools.Any())
            {
                foreach (var fool in fools)
                {
                    var target = Players.FirstOrDefault(x => x.Id == fool.Choice);
                    
                }
            }*/

            var oracles = Players.Where(x => x.PlayerRole == IRole.Oracle && !x.IsDead && !x.Frozen && !HowledTonight);
            if (oracles.Any())
            {
                foreach (var negSeer in oracles)
                {
                    var target = Players.FirstOrDefault(x => x.Id == negSeer.Choice);
                    if (target != null)
                    {
                        var possibleRoles = Players.Where(x => !x.IsDead && x.Id != negSeer.Id && x.PlayerRole != target.PlayerRole).Select(x => x.PlayerRole).ToList();
                        possibleRoles.Shuffle();
                        possibleRoles.Shuffle();
                        if (possibleRoles.Any())
                        {
                            Send(GetLocaleString("NegSeerSees", target.GetName(), GetRoleDescription(possibleRoles[0])), negSeer.Id);
                        }
                    }
                }
            }

            var lookouts = Players.Where(x => x.PlayerRole == IRole.Lookout && !x.IsDead && !x.Frozen && !HowledTonight);
            if (lookouts.Any())
            {
                foreach (var lookout in lookouts)
                {
                    var target = Players.FirstOrDefault(x => x.Id == lookout.Choice);
                    if (target != null)
                    {
                        var actors = Players.Where(x => x.Choice == target.Id && !x.Frozen && x.Id != lookout.Id);
                        List<IPlayer> visions = new List<IPlayer>();
                        foreach (var action in actors)
                        {
                            switch (action.PlayerRole)
                            {
                                case IRole.CultistHunter:
                                case IRole.Guard:
                                case IRole.GuardianAngel:
                                case IRole.Harlot:
                                case IRole.Herbalist:
                                case IRole.Lookout:
                                case IRole.Police:
                                case IRole.Pyro:
                                case IRole.SerialKiller:
                                case IRole.Sheriff:
                                case IRole.SnowWolf:
                                case IRole.Snooper:
                                case IRole.Firefighter:
                                    visions.Add(action);
                                    break;
                                default:
                                    break;
                            }
                        }
                        var curingHerbalists = Players.Where(x => !x.Frozen && x.Choice == 1 && x.LastPoisoned == target.Id);
                        if (curingHerbalists.Any())
                            foreach (var cu in curingHerbalists)
                                visions.Add(cu);
                        if (voteWolves.Any())
                        {
                            var randomWolf = voteWolves.FirstOrDefault();

                            if (target.Id == WolfTarget1 || target.Id == WolfTarget2)
                                visions.Add(randomWolf);
                        }

                        if (voteCult.Any())
                        {
                            var newbie = voteCult.OrderByDescending(x => x.DayCult).First();

                            if (target.Id == CultTarget)
                                visions.Add(newbie);
                        }

                        //now send visions
                        var msg = "";
                        if (visions.Any())
                            msg = GetLocaleString("LookoutVisits", target.GetName(), visions.Select(x => x.GetName()).Aggregate((current, next) => current + ", " + next));
                        else
                            msg = GetLocaleString("LookoutNoVisits", target.GetName());

                        Send(msg, lookout.Id);
                    }
                }
            }


            #endregion

            #region GA / Guards / Firefighter Night

            if (guards.Any())
            {
                foreach (var guard in guards)
                {
                    var save = Players.FirstOrDefault(x => x.Id == guard.Choice);
                    if (save != null)
                    {
                        save.BeingVisitedSameNightCount++;
                        if (save.PlayerRole == IRole.Ninja && save.HasUsedAbility)
                        {
                            Send(GetLocaleString("NinjaHidden"), save.Id);
                        }

                        if (save.GuardedLastNight)
                        {
                            if (guard.DiedByVisitingVictim)
                                Send(GetLocaleString("GuardHero", save.GetName()), guard.Id);
                            else
                                Send(GetLocaleString("GuardSaved", save.GetName()), guard.Id);
                        }
                        else if (!save.DiedLastNight)
                            Send(GetLocaleString("GuardNoAttack", save.GetName()), guard.Id);
                        else
                            Send(GetLocaleString("GuardsFail", save.GetName()), guard.Id);
                    }
                }
            }

            foreach (var guarded in Players.Where(x => x.GuardedLastNight))
                guarded.GuardedLastNight = false;

            if (fighters.Any())
            {
                foreach (var fighter in fighters)
                {
                    var save = Players.FirstOrDefault(x => x.Id == fighter.Choice);
                    if (save != null)
                    {
                        save.BeingVisitedSameNightCount++;
                        if (save.PlayerRole == IRole.Ninja && save.HasUsedAbility)
                        {
                            Send(GetLocaleString("NinjaHidden"), save.Id);
                        }

                        if (!save.FightedLastNight)
                        {
                            if (fighter.Id == save.Id)
                                Send(GetLocaleString("FighterFailSelf"), fighter.Id);
                            else
                                Send(GetLocaleString("FighterFail", save.GetName()), fighter.Id);
                            save.SaveWasted = true;
                        }
                        else
                        {
                            if (fighter.Id == save.Id)
                                Send(GetLocaleString("FighterSurvived"), fighter.Id);
                            else
                                Send(GetLocaleString("GuardSaved", save.GetName()), fighter.Id);
                        }
                    }
                }
            }

            foreach (var fighted in Players.Where(x => x.FightedLastNight))
                fighted.FightedLastNight = false;

            if (gas.Any())
            {
                foreach(var ga in gas)
                {
                    var save = Players.FirstOrDefault(x => x.Id == ga.Choice);
                    if (save != null)
                    {
                        //if (save != null)
                        //    DBAction(ga, save, "Guard");
                        save.BeingVisitedSameNightCount++;
                        if (save.PlayerRole == IRole.Ninja && save.HasUsedAbility)
                        {
                            Send(GetLocaleString("NinjaHidden"), save.Id);
                        }
                        if (save.WasSavedLastNight)
                        {
                            Send(GetLocaleString("GuardSaved", save.GetName()), ga.Id);
                        }
                        else if (save.DiedLastNight)
                        {
                            Send(GetLocaleString("GuardEmptyHouse", save.GetName()), ga.Id);
                        }

                        //check for save's role, even if they weren't attacked!
                        switch (save.PlayerRole)
                        {
                            case IRole.AlphaWolf:
                            case IRole.WolfCub:
                            case IRole.Wolf:
                            case IRole.Lycan:
                            case IRole.HungryWolf:
                            case IRole.RabidWolf:
                            case IRole.SnowWolf:
                            case IRole.Snooper:
                            case IRole.SpeedWolf:
                            case IRole.HowlingWolf:
                                if (Program.R.Next(100) > 50)
                                {
                                    ga.IsDead = true;
                                    ga.PointsToAdd -= 2;
                                    ga.TimeDied = DateTime.Now;
                                    ga.DiedLastNight = true;
                                    ga.DayOfDeath = GameDay;
                                    ga.DiedByVisitingKiller = true;
                                    ga.KilledByRole = IRole.Wolf;
                                    DBKill(save, ga, KillMthd.GuardWolf);
                                    Send(GetLocaleString("GuardWolf"), ga.Id);
                                }
                                else if (!save.WasSavedLastNight && !save.DiedLastNight)
                                //only send if GA survived and wolf wasn't attacked
                                {
                                    Send(GetLocaleString("GuardNoAttack", save.GetName()), ga.Id);
                                    ga.GAGuardWolfCount++;
                                }
                                break;
                            case IRole.SerialKiller:
                                ga.IsDead = true;
                                ga.TimeDied = DateTime.Now;
                                ga.DiedLastNight = true;
                                ga.DayOfDeath = GameDay;
                                ga.DiedByVisitingKiller = true;
                                ga.KilledByRole = IRole.SerialKiller;
                                DBKill(save, ga, KillMthd.GuardKiller);
                                Send(GetLocaleString("GuardKiller"), ga.Id);
                                break;
                            default:
                                if (!save.WasSavedLastNight && !save.DiedLastNight) //only send if save wasn't attacked
                                    Send(GetLocaleString("GuardNoAttack", save.GetName()), ga.Id);
                                break;
                        }

                        save.WasSavedLastNight = false;
                    }
                }
            }

            #endregion

            #region Roles return home / poisoned die...
            //sleepwalker goes back home
            if (sleepwalkers.Any())
            {
                foreach (var sleepwalker in sleepwalkers)
                {
                    sleepwalker.HasStayedHome = true;
                }
            }

            //kill poisoned players
            var poisonplayers = Players.Where(x => !x.IsDead && x.PoisonedBy != 0);
            if (poisonplayers.Any())
            {
                foreach (var poisonedguy in poisonplayers)
                {
                    if (poisonedguy.WasPoisoned)
                    {
                        if (poisonedguy.PlayerRole == IRole.WolfCub)
                            WolfCubKilled = true;
                        if (poisonedguy.PlayerRole == IRole.RabidWolf)
                            RabidKilled = true;
                        if (poisonedguy.PlayerRole == IRole.Baker)
                        {
                            _bakerDied = true;
                        }
                        poisonedguy.WasPoisoned = false;
                        var poisoner = Players.FirstOrDefault(x => x.Id == poisonedguy.PoisonedBy);
                        poisoner.PoisonTarget = 0;
                        if (BadRoles.Contains(poisonedguy.PlayerRole))
                            poisoner.PointsToAdd += 2;
                        poisonedguy.DiedLastNight = true;
                        poisonedguy.IsDead = true;
                        poisonedguy.TimeDied = DateTime.Now;
                        poisonedguy.DayOfDeath = GameDay;
                        poisonedguy.KilledByRole = IRole.Herbalist;
                        DBKill(poisoner, poisonedguy, KillMthd.Poisoned);
                    }
                    else
                    {
                        poisonedguy.WasPoisoned = true;
                    }
                }
            }

            #endregion

            WolfTarget1 = 0;
            WolfTarget2 = 0;
            CultTarget = 0;
            HowledTonight = false;

            CheckRoleChanges();

#region Night Death Notifications to Group


            //var secret = !DbGroup.HasFlag(GroupConfig.ShowRolesDeath);
            if (Players.Any(x => x.DiedLastNight))
            {
                foreach (var p in Players.Where(x => x.DiedLastNight))
                {
                    var msg = "";
                    /*if (secret)
                    {
                        SendWithQueue(GetLocaleString("GenericDeathNoReveal", p.GetName()));
                    }
                    else
                    {*/
                        //Killed by wolf
                        if (p.KilledByRole == IRole.Wolf && !p.DiedByVisitingKiller && !p.DiedByVisitingVictim)
                        {
                            switch (p.PlayerRole)
                            {
                                case IRole.ApprenticeSeer:
                                case IRole.Detective:
                                case IRole.Drunk:
                                case IRole.Fool:
                                case IRole.Gunner:
                                case IRole.Harlot:
                                case IRole.Mason:
                                case IRole.Seer:
                                case IRole.Sorcerer:
                                case IRole.WildChild:
                                case IRole.Baker:
                                    if (p.Devoured)
                                    {
                                        msg = GetLocaleString("GenericDeathNoReveal", p.GetName());
                                    }
                                    else
                                    {
                                        msg = GetLocaleString(p.PlayerRole.ToString() + "Eaten", p.GetName());
                                    }
                                    break;
                                case IRole.GuardianAngel:
                                    if (p.Devoured)
                                    {
                                        msg = GetLocaleString("GenericDeathNoReveal", p.GetName());
                                    }
                                    else
                                    {
                                        msg = GetLocaleString("GuardianEaten", p.GetName());
                                    }
                                    break;
                                case IRole.Imposter:
                                    if (p.Devoured)
                                    {
                                        msg = GetLocaleString("GenericDeathNoReveal", p.GetName());
                                    }
                                    else
                                    {
                                        var rm = Players.FirstOrDefault(x => x.Id == p.RoleModel);
                                        switch (rm.PlayerRole)
                                        {
                                            case IRole.ApprenticeSeer:
                                            case IRole.Detective:
                                            case IRole.Drunk:
                                            case IRole.Fool:
                                            case IRole.Gunner:
                                            case IRole.Harlot:
                                            case IRole.Mason:
                                            case IRole.Seer:
                                            case IRole.Sorcerer:
                                            case IRole.WildChild:
                                            case IRole.Baker:
                                                msg = GetLocaleString(rm.PlayerRole.ToString() + "Eaten", p.GetName());
                                                break;
                                            case IRole.GuardianAngel:
                                                msg = GetLocaleString("GuardianEaten", p.GetName());
                                                break;
                                            default:
                                                msg = GetLocaleString("DefaultEaten", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetRoleDescription(rm.PlayerRole)}");
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    if (p.Devoured)
                                    {
                                        msg = GetLocaleString("GenericDeathNoReveal", p.GetName());
                                    }
                                    else
                                    {
                                        msg = GetLocaleString("DefaultEaten", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetRoleDescription(p.PlayerRole)}");
                                    }
                                    break;
                            }
                        }
                        //killed by SK
                        else if (p.KilledByRole == IRole.SerialKiller && !p.DiedByVisitingKiller && !p.DiedByVisitingVictim)
                        {
                            switch (p.PlayerRole)
                            {
                                case IRole.Blacksmith:
                                case IRole.Cultist:
                                case IRole.Cupid:
                                case IRole.Drunk:
                                case IRole.GuardianAngel:
                                case IRole.Gunner:
                                case IRole.Mayor:
                                case IRole.Prince:
                                case IRole.Seer:
                                    msg = GetLocaleString(p.PlayerRole.ToString() + "Killed", p.GetName());
                                    break;

                                case IRole.Hunter:
                                    msg = null;
                                    SendWithQueue(GetLocaleString("DefaultKilled", p.GetName(),
                                        $"{p.GetName()} {GetLocaleString("Was")} {GetDescription(p)}"));
                                    HunterFinalShot(p, KillMthd.SerialKilled);
                                    break;
                                case IRole.Imposter:
                                    var rm = Players.FirstOrDefault(x => x.Id == p.RoleModel);
                                    switch (rm.PlayerRole)
                                    {
                                        case IRole.Blacksmith:
                                        case IRole.Cultist:
                                        case IRole.Cupid:
                                        case IRole.Drunk:
                                        case IRole.GuardianAngel:
                                        case IRole.Gunner:
                                        case IRole.Mayor:
                                        case IRole.Prince:
                                        case IRole.Seer:
                                            msg = GetLocaleString(rm.PlayerRole.ToString() + "Killed", p.GetName());
                                            break;
                                        default:
                                            msg = GetLocaleString("DefaultKilled", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetRoleDescription(rm.PlayerRole)}");
                                            break;
                                    }
                                    break;
                                case IRole.Ninja:
                                    if (p.HasUsedAbility)
                                    {
                                        msg = GetLocaleString(p.PlayerRole.ToString() + "Killed", p.GetName());
                                    }
                                    else
                                    {
                                        msg = GetLocaleString("DefaultKilled", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetDescription(p)}");
                                    }
                                    break;
                                default:
                                    msg = GetLocaleString("DefaultKilled", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetDescription(p)}");
                                    break;
                            }
                        }
                        // killed by pyro
                        else if (p.KilledByRole == IRole.Pyro && !p.DiedByVisitingKiller && !p.DiedByVisitingVictim)
                        {
                            if (p.PlayerRole == IRole.Imposter)
                            {
                                var rm = Players.FirstOrDefault(x => x.Id == p.RoleModel);
                                msg = GetLocaleString("DefaultIgnited", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetRoleDescription(rm.PlayerRole)}");
                            }
                            else
                            {
                                msg = GetLocaleString("DefaultIgnited", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetDescription(p)}");
                            }
                        }
                        // killed by herbalist
                        else if (p.KilledByRole == IRole.Herbalist && !p.DiedByVisitingKiller && !p.DiedByVisitingVictim)
                        {
                            if (p.PlayerRole == IRole.Imposter)
                            {
                                var rm = Players.FirstOrDefault(x => x.Id == p.RoleModel);
                                msg = GetLocaleString("DefaultPoisoned", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetRoleDescription(rm.PlayerRole)}");
                            }
                            else
                            {
                                msg = GetLocaleString("DefaultPoisoned", p.GetName(), $"{p.GetName()} {GetLocaleString("Was")} {GetDescription(p)}");
                            }
                        }
                        //died by visiting
                        else
                        {
                            switch (p.PlayerRole)
                            {
                                case IRole.WolfCub:
                                case IRole.AlphaWolf:
                                case IRole.Lycan:
                                case IRole.Wolf:
                                case IRole.RabidWolf:
                                case IRole.HungryWolf: //sk and hunter can kill
                                case IRole.SpeedWolf:
                                case IRole.HowlingWolf:
                                    if (p.PlayerRole == IRole.WolfCub)
                                        WolfCubKilled = true;
                                    if (p.PlayerRole == IRole.RabidWolf)
                                        RabidKilled = true;
                                    if (p.KilledByRole == IRole.SerialKiller)
                                        msg = GetLocaleString("SerialKillerKilledWolf", p.GetName());
                                    else //died from hunter
                                        msg = GetLocaleString(voteWolvesCount > 1 ? "HunterShotWolfMulti" : "HunterShotWolf", p.GetName()) + " " + GetLocaleString("PlayerRoleWas", p.GetName(), GetDescription(p));
                                    break;
                                case IRole.CultistHunter: //killed by sk
                                    msg = GetLocaleString("SerialKillerKilledCH", p.GetName());
                                    break;
                                case IRole.Sheriff: // killed by sk
                                    msg = GetLocaleString("SerialKillerKilledSheriff", p.GetName());
                                    break;
                                case IRole.Cultist:
                                    switch (p.KilledByRole)
                                    {
                                        case IRole.CultistHunter:
                                            msg = GetLocaleString("HunterKilledCultist", p.GetName());
                                            break;
                                        case IRole.Hunter:
                                            msg = GetLocaleString("HunterKilledVisiter", p.GetName(), $"{GetDescription(p)} {GetLocaleString("IsDead")}");
                                            break;
                                        case IRole.Wolf:
                                            msg = GetLocaleString("CultConvertWolfPublic", p.GetName());
                                            break;
                                        case IRole.SerialKiller:
                                            msg = GetLocaleString("CultConvertKillerPublic", p.GetName());
                                            break;
                                    }
                                    break;
                                case IRole.Guard:
                                    msg = GetLocaleString("GuardSacrificed", p.GetName());
                                    break;
                                case IRole.Ninja:
                                    msg = GetLocaleString("NinjaBackstabbed", p.GetName());
                                    break;
                                case IRole.Harlot:
                                    switch (p.KilledByRole)
                                    {
                                        case IRole.Wolf:
                                            if (p.DiedByVisitingKiller)
                                                msg = GetLocaleString("HarlotFuckedWolfPublic", p.GetName());
                                            else if (p.DiedByVisitingVictim)
                                                msg = GetLocaleString("HarlotFuckedVictimPublic", p.GetName(), Players.FirstOrDefault(x => x.Id == p.RoleModel).GetName());
                                            break;
                                        case IRole.SerialKiller:
                                            if (p.DiedByVisitingKiller)
                                                msg = GetLocaleString("HarlotFuckKillerPublic", p.GetName());
                                            else if (p.DiedByVisitingVictim)
                                                msg = GetLocaleString("HarlotFuckedKilledPublic", p.GetName(), Players.FirstOrDefault(x => x.Id == p.RoleModel).GetName());
                                            break;
                                    }
                                    break;
                                case IRole.GuardianAngel:
                                    if (p.KilledByRole == IRole.Wolf)
                                        msg = GetLocaleString("GAGuardedWolf", p.GetName());
                                    else if (p.KilledByRole == IRole.SerialKiller)
                                        msg = GetLocaleString("GAGuardedKiller", p.GetName());
                                    break;
                                case IRole.SnowWolf:
                                    if (p.KilledByRole == IRole.SerialKiller)
                                        msg = GetLocaleString("SnowFrozeKiller", p.GetName());
                                    else // died from hunter
                                        msg = GetLocaleString("SnowFrozeHunter", p.GetName());
                                    break;
                                case IRole.Pyro:
                                    if (p.KilledByRole == IRole.Hunter)
                                        msg = GetLocaleString("PyroVisitHunter", p.GetName());
                                    break;
                        }
                        }
                    //}
                    if (!String.IsNullOrEmpty(msg))
                        SendWithQueue(msg);
                    if (p.InLove)
                        KillLover(p);
                }

                var bloodyVictims = Players.Where(x => x.TimeDied > nightStart && x.IsDead);

                if (bloodyVictims.Count() >= 4)
                    foreach (var p in bloodyVictims)
                        AddAchievement(p, Achievements.BloodyNight);
            }
            else
            {
                if (IsRunning)
                    SendWithQueue(GetLocaleString("NoAttack"));
            }

            if (Players.Any(x => x.RevivedLastNight))
            {
                foreach (var p in Players.Where(x => x.RevivedLastNight))
                {
                    SendWithQueue(GetLocaleString("PlayerRevived", p.GetName()));
                }
            }

            #endregion

            if (CheckForGameEnd()) return;

            // to avoid deadtalk... still WIP
            /*if (Players.Any(x => x.DiedLastNight))
            {
                foreach (var p in Players.Where(x => x.DiedLastNight))
                {
                    Program.Bot.RestrictChatMemberAsync(ChatId, p.Id, default(DateTime), false, false, false, false);
                }
            }*/

                    //reset everything
                    foreach (var p in Players)
            {
                //p.DiedLastNight = false;
                p.RevivedLastNight = false;
                p.Choice = 0;
                p.Votes = 0;
                if (p.BeingVisitedSameNightCount >= 3)
                {
                    p.BusyNight = true;
                }
                if (p.Bitten & !p.IsDead)
                {
                    Send(GetLocaleString("PlayerBitten"), p.Id);
                }
                p.BeingVisitedSameNightCount = 0;
            }

        }

        private bool CheckForGameEnd(bool checkbitten = false)
        {
            if (Players == null)
                return true;
            if (!IsRunning) return true;
            var alivePlayers = Players.Where(x => !x.IsDead);

            //first of all, check for traitor!
            if (alivePlayers.All(x => !WolfRoles.Contains(x.PlayerRole)))
            {
                var snowwolf = alivePlayers.FirstOrDefault(x => SupportWolves.Contains(x.PlayerRole));
                if (snowwolf != null)
                {
                    if (!checkbitten || alivePlayers.All(x => !x.Bitten)) //snowwolf should not turn wolf if bitten is about to turn
                    {
                        //snowwolf becomes normal wolf!
                        snowwolf.PlayerRole = IRole.Wolf;
                        snowwolf.ChangedRolesCount++;
                        Send(GetLocaleString("SnowWolfTurnWolf"), snowwolf.Id);
                    }
                    else return false; //bitten is turning wolf! game doesn't end
                }
                else
                {
                    var traitor = alivePlayers.FirstOrDefault(x => x.PlayerRole == IRole.Traitor);
                    if (traitor != null)
                    {
                        if (!checkbitten || alivePlayers.All(x => !x.Bitten)) //traitor should not turn wolf if bitten is about to turn
                        {
                            //traitor turns wolf!
                            traitor.PlayerRole = IRole.Wolf;
                            traitor.Team = ITeam.Wolf;
                            traitor.HasDayAction = false;
                            traitor.HasNightAction = true;
                            traitor.ChangedRolesCount++;
                            Send(GetLocaleString("TraitorTurnWolf"), traitor.Id);
                        }
                        else return false; //bitten is turning wolf! game doesn't end
                    }
                }
            }

            switch (alivePlayers?.Count())
            {
                case 0:
                    return DoGameEnd(ITeam.NoOne);
                case 1:
                    var p = alivePlayers.FirstOrDefault();
                    if (p.PlayerRole == IRole.Tanner || p.PlayerRole == IRole.Sorcerer || p.PlayerRole == IRole.Survivor || p.PlayerRole == IRole.Imposter)
                        return DoGameEnd(ITeam.NoOne);
                    else
                        return DoGameEnd(p.Team);
                case 2:
                    //check for lovers
                    if (alivePlayers.All(x => x.InLove))
                        return DoGameEnd(ITeam.Lovers);
                    //check for Tanner + Sorcerer
                    if (alivePlayers.Any(x => x.PlayerRole == IRole.Sorcerer))
                    {
                        var other = alivePlayers.FirstOrDefault(x => x.PlayerRole != IRole.Sorcerer);
                        if (other != null && other.PlayerRole == IRole.Tanner)
                        {
                            return DoGameEnd(ITeam.NoOne);
                        }
                    }
                    //check for Hunter + SK / Wolf
                    if (alivePlayers.Any(x => x.PlayerRole == IRole.Hunter))
                    {
                        var other = alivePlayers.FirstOrDefault(x => x.PlayerRole != IRole.Hunter);
                        if (other == null)
                            return DoGameEnd(ITeam.Village);
                        if (other.PlayerRole == IRole.SerialKiller)
                            return DoGameEnd(ITeam.SKHunter);
                        if (WolfRoles.Contains(other.PlayerRole) || SupportWolves.Contains(other.PlayerRole))
                        {
                            var hunter = alivePlayers.First(x => x.PlayerRole == IRole.Hunter);
                            if (Program.R.Next(100) < Settings.HunterKillWolfChanceBase)
                            {
                                SendWithQueue(GetLocaleString("HunterKillsWolfEnd", hunter.GetName(), other.GetName()));
                                hunter.PointsToAdd += 3;
                                other.IsDead = true;
                                other.DayOfDeath = GameDay;
                                DBKill(hunter, other, KillMthd.HunterShot);
                                return DoGameEnd(ITeam.Village);
                            }
                            else
                            {
                                SendWithQueue(GetLocaleString("WolfKillsHunterEnd", hunter.GetName(), other.GetName()));
                                hunter.IsDead = true;
                                hunter.DayOfDeath = GameDay;
                                DBKill(other, hunter, KillMthd.Eat);
                                return DoGameEnd(ITeam.Wolf);
                            }
                        }
                    }
                    //check for SK
                    if (alivePlayers.Any(x => x.PlayerRole == IRole.SerialKiller))
                        return DoGameEnd(ITeam.SerialKiller);
                    //check for pyro
                    if (alivePlayers.Any(x => x.PlayerRole == IRole.Pyro))
                        return DoGameEnd(ITeam.Pyro);
                    //check for cult
                    if (alivePlayers.Any(x => x.PlayerRole == IRole.Cultist))
                    {
                        var other = alivePlayers.FirstOrDefault(x => x.PlayerRole != IRole.Cultist);
                        if (other == null) //two cults
                            return DoGameEnd(ITeam.Cult);
                        switch (other.PlayerRole)
                        {
                            case IRole.Wolf:
                            case IRole.WolfCub:
                            case IRole.AlphaWolf:
                            case IRole.Lycan:
                            case IRole.HungryWolf:
                            case IRole.RabidWolf:
                            case IRole.SnowWolf:
                            case IRole.Snooper:
                            case IRole.SpeedWolf:
                            case IRole.HowlingWolf:
                                return DoGameEnd(ITeam.Wolf);
                            case IRole.CultistHunter:
                                var cultist = alivePlayers.FirstOrDefault(x => x.PlayerRole == IRole.Cultist);
                                SendWithQueue(GetLocaleString("CHKillsCultistEnd", cultist.GetName(), other.GetName()));
                                DBKill(other, cultist, KillMthd.Hunt);
                                return DoGameEnd(ITeam.Village);
                            default:
                                //autoconvert the other
                                if (other.PlayerRole != IRole.Doppelgänger && other.PlayerRole != IRole.Survivor)
                                {
                                    other.PlayerRole = IRole.Cultist;
                                    other.Team = ITeam.Cult;
                                }
                                return DoGameEnd(ITeam.Cult);
                        }
                    }
                    break;
                default:
                    break;
            }

            var sk = alivePlayers.FirstOrDefault(x => x.PlayerRole == IRole.SerialKiller);
            if (alivePlayers.Any(x => x.Team == ITeam.SerialKiller)) //there is still SK alive, do nothing (surely more than two players)
            {
                if (alivePlayers.Any(x => x.PlayerRole != IRole.Survivor && x != sk))
                    return false;
                else
                    return DoGameEnd(ITeam.SerialKiller);
            }

            var pyro = alivePlayers.FirstOrDefault(x => x.PlayerRole == IRole.Pyro);
            if (alivePlayers.Any(x => x.Team == ITeam.Pyro)) //there is still pyro alive, do nothing (surely more than two players)
            {
                if (alivePlayers.Any(x => x.PlayerRole != IRole.Survivor && x != pyro))
                    return false;
                else
                    return DoGameEnd(ITeam.Pyro);
            }

            //is everyone left a cultist?
            if (alivePlayers.All(x => x.PlayerRole == IRole.Cultist || x.PlayerRole == IRole.Survivor))
                return DoGameEnd(ITeam.Cult);

            //is everyone left team wolf?
            if (alivePlayers.All(x => x.Team == ITeam.Wolf || x.PlayerRole == IRole.Survivor))
                return DoGameEnd(ITeam.Wolf);

            //do the wolves outnumber the others?
            if (alivePlayers.Count(x => WolfRoles.Contains(x.PlayerRole) || SupportWolves.Contains(x.PlayerRole)) >= alivePlayers.Count(x => !WolfRoles.Contains(x.PlayerRole) && !SupportWolves.Contains(x.PlayerRole)))
            {
                var wolves = alivePlayers.Where(x => WolfRoles.Contains(x.PlayerRole) || SupportWolves.Contains(x.PlayerRole));
                var others = alivePlayers.Where(x => !WolfRoles.Contains(x.PlayerRole) && !SupportWolves.Contains(x.PlayerRole));
                if (alivePlayers.Any(x => x.PlayerRole == IRole.Gunner && x.Bullet > 0))
                {
                    // gunner makes the difference only if wolves are exactly as many as the others, or two wolves are in love and the gunner can kill two of them at once
                    var gunnermakesthedifference = (wolves.Count() == others.Count()) || (wolves.Count() == others.Count() + 1 && wolves.Count(x => x.InLove) == 2);
                    if (gunnermakesthedifference)
                    {
                        // do nothing, gunner can still make VGs win
                        foreach (var p in alivePlayers.Where(x => x.Team == ITeam.Village))
                            AddAchievement(p, Achievements.GunnerSaves);
                    }
                    return false;
                }
                if (alivePlayers.Any(x => x.PlayerRole == IRole.Healer && !x.HasUsedAbility))
                    return false;
                if (alivePlayers.Count(x => x.PlayerRole == IRole.Hunter) == alivePlayers.Count(x => WolfRoles.Contains(x.PlayerRole)))
                    return false;
                if (others.Count() > 1 && wolves.Count() == others.Count() && alivePlayers.Any(x => x.PlayerRole == IRole.Mayor))
                    return false;
                return DoGameEnd(ITeam.Wolf);
            }

            if (alivePlayers.All(x => !WolfRoles.Contains(x.PlayerRole) && !SupportWolves.Contains(x.PlayerRole) && x.PlayerRole != IRole.Cultist && x.PlayerRole != IRole.SerialKiller)) //checks for cult and SK are actually useless...
                //no wolf, no cult, no SK... VG wins!
                if (!checkbitten || alivePlayers.All(x => !x.Bitten)) //unless bitten is about to turn into a wolf
                    return DoGameEnd(ITeam.Village);


            return false;
        }

        private bool DoGameEnd(ITeam team)
        {
            using (var db = new WWContext())
            {
                //Log.WriteLine($"Doing game end.  IsRunning: {IsRunning}");
                if (!IsRunning) return true;
                IsRunning = false;
                CheckLongHaul();
                var msg = "";

                var game = db.Games.FirstOrDefault(x => x.Id == GameId) ?? new Database.Game();
                game.TimeEnded = DateTime.Now;

                if (team == ITeam.Lovers)
                {
                    var lovers = Players.Where(x => x.InLove);
                    var forbidden = lovers.Any(x => WolfRoles.Contains(x.PlayerRole)) && lovers.Any(x => x.PlayerRole == IRole.Villager);
                    foreach (var w in lovers)
                    {
                        if (forbidden)
                            AddAchievement(w, Achievements.ForbiddenLove);
                        w.Won = true;
                        if (gameMode != 4)
                        {
                            var p = GetDBGamePlayer(w, db);
                            p.Won = true;
                        }
                    }
                }
                else
                {
                    foreach (var w in Players.Where(x => x.Team == team))
                    {
                        //for sk, only let the one that is alive win
                        if (team == ITeam.SerialKiller && w.IsDead)
                            continue;

                        //the winning tanner is the only one with DiedLastNight == true
                        if (team == ITeam.Tanner && !w.DiedLastNight)
                            continue;
                        
                        w.Won = true;
                        if (gameMode != 4)
                        {
                            var p = GetDBGamePlayer(w, db);
                            p.Won = true;
                        }
                        if (w.InLove)
                        {
                            //find lover
                            var lover = Players.FirstOrDefault(x => x.Id == w.LoverId);
                            if (lover != null)
                            {
                                lover.Won = true;
                                if (gameMode != 4)
                                {
                                    GetDBGamePlayer(lover, db).Won = true;
                                }
                            }
                        }
                    }
                }
                foreach (var surv in Players.Where(x => x.PlayerRole == IRole.Survivor))
                {
                    if (!surv.IsDead)
                    {
                        surv.Won = true;
                        if (gameMode != 4)
                            {
                                var p = GetDBGamePlayer(surv, db);
                                p.Won = true;
                            }
                        if (surv.InLove)
                        {
                            //find lover
                            var lover = Players.FirstOrDefault(x => x.Id == surv.LoverId);
                            if (lover != null)
                            {
                                lover.Won = true;
                                if (gameMode != 4)
                                {
                                    GetDBGamePlayer(lover, db).Won = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        surv.Won = false;
                    }
                }
                switch (team)
                {
                    case ITeam.NoOne:
                        var alives = Players.Where(x => !x.IsDead);
                        var deathmessage = "";
                        switch (alives.Count())
                        {
                            case 2: // Tanner and sorcerer, let first sorcerer, then tanner die.
                                if (alives.Any(x => x.PlayerRole == IRole.Tanner) && alives.First(x => x.PlayerRole != IRole.Tanner).PlayerRole == IRole.Sorcerer)
                                {
                                    var sorc = alives.FirstOrDefault(x => x.PlayerRole == IRole.Sorcerer);
                                    var tann = alives.FirstOrDefault(x => x.PlayerRole == IRole.Tanner);
                                    
                                    if (sorc != null && tann != null)
                                    {                                        
                                        DBKill(tann, tann, KillMthd.Suicide);
                                        tann.IsDead = true;
                                        tann.TimeDied = DateTime.Now;
                                        
                                        deathmessage = GetLocaleString("SorcererEnd", sorc.GetName()) + Environment.NewLine;
                                        deathmessage += Environment.NewLine + GetLocaleString("TannerEnd", tann.GetName());
                                    }
                                }
                                break;
                            
                            case 1: // Tanner or sorcerer
                                var lastone = alives.FirstOrDefault();
                                if (lastone != null)
                                {
                                    if (lastone.PlayerRole == IRole.Tanner)
                                    {
                                        DBKill(lastone, lastone, KillMthd.Suicide);
                                        lastone.IsDead = true;
                                        lastone.TimeDied = DateTime.Now;
                                        
                                        deathmessage = GetLocaleString("TannerEnd", lastone.GetName());
                                    }
                                    else if (lastone.PlayerRole == IRole.Sorcerer)
                                    {                                       
                                        deathmessage = GetLocaleString("SorcererEnd", lastone.GetName());
                                    }
                                }
                                break;
                                
                            default:
                                break;
                        }
                        
                        if (!string.IsNullOrEmpty(deathmessage)) SendWithQueue(deathmessage);
                        
                        msg += GetLocaleString("NoWinner");
                        game.Winner = "NoOne";
                        SendWithQueue(msg, GetRandomImage(NoWinner));
                        break;

                    case ITeam.Wolf:
                        if (Players.Count(x => !x.IsDead && (WolfRoles.Contains(x.PlayerRole)) || SupportWolves.Contains(x.PlayerRole)) > 1)
                        {
                            msg += GetLocaleString("WolvesWin");
                            game.Winner = "Wolves";
                            SendWithQueue(msg, GetRandomImage(WolvesWin));
                        }
                        else
                        {
                            msg += GetLocaleString("WolfWins");
                            game.Winner = "Wolf";
                            SendWithQueue(msg, GetRandomImage(WolfWin));
                        }
                        break;
                    case ITeam.Tanner:
                        msg += GetLocaleString("TannerWins");
                        game.Winner = "Tanner";
                        SendWithQueue(msg, GetRandomImage(TannerWin));
                        break;
                    case ITeam.Cult:
                        msg += GetLocaleString("CultWins");
                        game.Winner = "Cult";
                        SendWithQueue(msg, GetRandomImage(CultWins)); //, GetRandomImage(Program.VillagersWin));
                        break;
                    case ITeam.Pyro:
                        msg += GetLocaleString("PyroWins");
                        game.Winner = "Pyro";
                        SendWithQueue(msg, GetRandomImage(PyroWins));
                    break;
                    case ITeam.SerialKiller:
                        if (Players.Count(x => !x.IsDead) > 1)
                        {
                            var alive = Players.Where(x => !x.IsDead);
                            var sk = alive.FirstOrDefault(x => x.PlayerRole == IRole.SerialKiller);
                            var otherPerson = alive.FirstOrDefault(x => x != sk);
                            if (otherPerson.PlayerRole != IRole.Survivor)
                            {
                                otherPerson.Won = false;
                                if (gameMode != 4)
                                {
                                    var p = GetDBGamePlayer(otherPerson, db);
                                    p.Won = false;
                                }
                                SendWithQueue(GetLocaleString("SerialKillerWinsOverpower", sk.GetName(), otherPerson.GetName()));
                                DBKill(sk, otherPerson, KillMthd.SerialKilled);
                                otherPerson.IsDead = true;
                                otherPerson.TimeDied = DateTime.Now;
                            }
                        }
                        msg += GetLocaleString("SerialKillerWins");
                        game.Winner = "SerialKiller";
                        SendWithQueue(msg, GetRandomImage(SerialKillerWins));
                        break;
                    case ITeam.Lovers:
                        msg += GetLocaleString("LoversWin");
                        game.Winner = "Lovers";
                        SendWithQueue(msg, GetRandomImage(LoversWin));
                        break;
                    case ITeam.SKHunter:
                        var skhunter = Players.Where(x => !x.IsDead);
                        var hunter = skhunter.FirstOrDefault(x => x.PlayerRole != IRole.SerialKiller);
                        var skh = skhunter.FirstOrDefault(x => x.PlayerRole == IRole.SerialKiller);
                        msg += GetLocaleString("NoWinner");
                        game.Winner = "NoOne";
                        AddAchievement(skh, Achievements.DoubleKill);
                        AddAchievement(hunter, Achievements.DoubleKill);
                        DBKill(skh, hunter, KillMthd.SerialKilled);
                        DBKill(hunter, skh, KillMthd.HunterShot);
                        if (skh != null)
                        {
                            skh.IsDead = true;
                            skh.TimeDied = DateTime.Now;
                            if (hunter != null)
                            {
                                hunter.IsDead = true;
                                hunter.TimeDied = DateTime.Now;
                                SendWithQueue(GetLocaleString("SKHunterEnd", skh.GetName(), hunter.GetName()));
                            }
                        }
                        SendWithQueue(msg, GetRandomImage(NoWinner));
                        break;
                    default: //village
                        msg += GetLocaleString("VillageWins");
                        game.Winner = "Village";
                        SendWithQueue(msg, GetRandomImage(VillagersWin));
                        //var aux = Program.Bot.SendDocumentAsync(ChatId, new FileToSend(GetRandomImage(VillagersWin)), msg).Result.MessageId;
                        break;
                }

                if (Inverted)
                {
                    foreach (var w in Players)
                    {
                        bool won;
                        won = !w.Won;
                        if (w.DiedAFK)
                            won = false;
                        w.Won = won;
                        if (gameMode != 4)
                        {
                            var p = GetDBGamePlayer(w, db);
                            p.Won = won;
                        }
                    }
                }

                if (gameMode == 5 && !SomeoneSmited) // if it's ranked and no one was smited/fled, give points
                {
                    foreach (var pl in Players.Where(x => !BadRoles.Contains(x.PlayerRole)))
                    {
                        VillageScore = VillageScore + pl.Score;
                    }
                    foreach (var wolf in Players.Where(x => BadRoles.Contains(x.PlayerRole)))
                    {
                        WolfScore = WolfScore + wolf.Score;
                    }
                    VillageScore = VillageScore / Math.Max(1, Players.Count(x => !BadRoles.Contains(x.PlayerRole)));
                    WolfScore = WolfScore / Math.Max(1, Players.Count(x => BadRoles.Contains(x.PlayerRole)));
                    foreach (var p in Players)
                    {
                        var dbp = db.Players.FirstOrDefault(x => x.TelegramId == p.Id);
                        p.Score = dbp.Score;
                        p.PointsToAdd = Math.Truncate(p.PointsToAdd);
                        p.PointsToAdd = p.PointsToAdd + p.PlayerRole.GetRoleScore(p.Won, startingwolves, totalplayers);
                        if (!p.Won && p.IsDead)
                        {
                            if (p.PointsToAdd < 0 && p.DayOfDeath > 0)
                            {
                                p.PointsToAdd = Math.Min(-1, p.PointsToAdd + (GameDay / p.DayOfDeath + 1));
                            }
                        }
                        if (totalplayers > 19 && p.NonVote < 2) // survival bonus for big games
                        {
                            if (p.IsDead)
                                p.PointsToAdd += p.DayOfDeath / 2;
                            else
                                p.PointsToAdd += GameDay / 2;
                        }
                        if (p.PlayerRole == IRole.Villager && !p.IsDead)
                            p.PointsToAdd += 1;
                        /*double teamFactor = 1;
                        if (WolfScore != 0 && VillageScore != 0)
                        {
                            if (!BadRoles.Contains(p.PlayerRole))
                            {
                                teamFactor = VillageScore / WolfScore;
                            }
                            else
                            {
                                teamFactor = WolfScore / VillageScore;
                            }
                            if (p.PointsToAdd > 0)
                            {
                                teamFactor = 1 / teamFactor;
                            }
                        }
                        p.PointsToAdd = p.PointsToAdd * teamFactor;*/
                        if (p.Score > 2000)
                        {
                            if (p.PointsToAdd > 0)
                                p.PointsToAdd = p.PointsToAdd * 0.2;
                            else
                                p.PointsToAdd -= 5;

                        }
                        else if (p.Score > 1800)
                        {
                            if (p.PointsToAdd > 0)
                                p.PointsToAdd = p.PointsToAdd * 0.4;
                            else
                                p.PointsToAdd -= 4;
                        }
                        else if (p.Score > 1600)
                        {
                            if (p.PointsToAdd > 0)
                                p.PointsToAdd = p.PointsToAdd * 0.5;
                            else
                                p.PointsToAdd -= 3;
                        }
                        else if (p.Score > 1400)
                        {
                            if (p.PointsToAdd > 0)
                                p.PointsToAdd = p.PointsToAdd * 0.6;
                            else
                                p.PointsToAdd -= 2;
                        }
                        else if (p.Score > 1200)
                        {
                            if (p.PointsToAdd > 0)
                                p.PointsToAdd = p.PointsToAdd * 0.8;
                            else
                                p.PointsToAdd -= 1;
                        }
                        else if (p.Score < 900)
                        {
                            if (p.PointsToAdd > 0)
                                p.PointsToAdd = p.PointsToAdd * 1.5;
                        }
                        p.PointsToAdd = Math.Truncate(p.PointsToAdd);

                        if (Inverted)
                            p.PointsToAdd = 0;

                        p.Score = p.Score + (int)p.PointsToAdd;
                        dbp.Score = p.Score;
                    }
                }
                db.SaveChanges();
                var signosuma = "+";
                switch (DbGroup.ShowRolesEnd)
                {
                    case "None":
                        msg = $"{GetLocaleString("PlayersAlive")}: {Players.Count(x => !x.IsDead)} / {Players.Count}\n" + Players.OrderBy(x => x.TimeDied).Aggregate(msg, (current, p) => current + $"\n{p.GetName()}");
                        break;
                    case "All":
                        if (gameMode == 5)
                        {
                            msg = $"{GetLocaleString("PlayersAlive")}: {Players.Count(x => !x.IsDead)} / {Players.Count}\n" + Players.OrderBy(x => x.TimeDied).Aggregate("", (current, p) => current + ($"{p.GetRankName(gameMode)}: {(p.IsDead ? (p.Fled ? GetLocaleString("RanAway") : GetLocaleString("Dead")) : GetLocaleString("Alive")) + " - " + GetEndDescription(p) + (p.InLove ? "❤️" : "")} {(p.Won ? GetLocaleString("Won") : GetLocaleString("Lost"))} {(p.PointsToAdd < 0 ? p.PointsToAdd.ToString().ToBold() : (signosuma.ToBold() + p.PointsToAdd.ToString().ToBold()))} {"(" + p.Score.ToString().ToBold() + ")"}\n"));
                        }
                        else
                        {
                            msg = $"{GetLocaleString("PlayersAlive")}: {Players.Count(x => !x.IsDead)} / {Players.Count}\n" + Players.OrderBy(x => x.TimeDied).Aggregate("", (current, p) => current + ($"{p.GetRankName(gameMode)}: {(p.IsDead ? (p.Fled ? GetLocaleString("RanAway") : GetLocaleString("Dead")) : GetLocaleString("Alive")) + " - " + GetEndDescription(p) + (p.InLove ? "❤️" : "")} {(p.Won ? GetLocaleString("Won") : GetLocaleString("Lost"))}\n"));
                        }
                        break;
                    default:
                        msg = GetLocaleString("RemainingPlayersEnd") + Environment.NewLine;
                        msg = Players.Where(x => !x.IsDead).OrderBy(x => x.Team).Aggregate(msg, (current, p) => current + $"\n{p.GetName()}: {GetEndDescription(p)} {GetLocaleString(p.Team + "TeamEnd")} {(p.InLove ? "❤️" : "")} {GetLocaleString(p.Won ? "Won" : "Lost")}");
                        break;
                }
                if (game.TimeStarted.HasValue)
                {
                    _timePlayed = game.TimeEnded.Value - game.TimeStarted.Value;
                    msg += "\n" + GetLocaleString("EndTime", _timePlayed.Value.ToString(@"hh\:mm\:ss"));
                }
                SendWithQueue(msg);
                //Program.Bot.SendTextMessage(ChatId, "[Enjoy playing? Support the developers and get some swag!](https://teespring.com/stores/werewolf-for-telegram)", parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                UpdateAchievements();
                UpdateGroupRanking();
                //Program.Analytics.TrackAsync("gameend", new { winner = team.ToString(), groupid = ChatId, mode = Chaos ? "Chaos" : "Normal", size = Players.Count() }, "0");
                //if (ChatId == -1001094614730)
                //{
                //    foreach (var p in Players.Where(x => x.IsDead))
                //    {
                //        Program.Bot.RestrictChatMemberAsync(-1001094614730, p.Id, default(DateTime), true, true, true, true);
                //    }
                //}

                Thread.Sleep(10000);
                Program.RemoveGame(this);
                return true;
            }
        }


#endregion

#region Send Menus

        private void SendLynchMenu()
        {
            if (Players == null)
                return;
            NoOneCastLynch = true;
            foreach (var player in Players.Where(x => (!x.IsDead && !x.RanAway) || x.PlayerRole == IRole.Ghost).OrderBy(x => x.Name))
            {
                player.CurrentQuestion = null;
                player.Choice = 0;
                var choices = Players.Where(x => !x.IsDead && x.Id != player.Id && !x.RanAway).Select(x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId.ToString()}|{x.Id}") }).ToList();
                //choices.Add(new [] { new InlineKeyboardCallbackButton(GetLocaleString("Skip"), $"vote|{Program.ClientId.ToString()}|skip") });
                SendMenu(choices, player, GetLocaleString("AskLynch"), QuestionType.Lynch);
                Thread.Sleep(100);
            }
        }

        private void SendMenu(List<InlineKeyboardCallbackButton[]> choices, IPlayer to, string text, QuestionType qtype)
        {
            choices = choices.ToList();
            var skip = choices.FirstOrDefault(x => x[0].Text == GetLocaleString("Skip"));

            if (skip != null)
            {
                var index = choices.IndexOf(skip);
                skip = choices[index];
                choices.Remove(skip);
                choices.Add(skip);
            }
            var menu = new InlineKeyboardMarkup(choices.ToArray());
            try
            {
                var msgId = 0;

                try
                {
                    var result = Program.Send(text, to.Id, false, menu).Result;
                    msgId = result.MessageId;
                }
                catch (AggregateException ex)
                {
                    var e = ex.InnerExceptions.First();
                    Console.WriteLine($"Error getting menu send result: {e.Message} {to.TeleUser.Username} {to.Name}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error getting menu send result: {e.Message} {to.TeleUser.Username} {to.Name}");
                }
                to.CurrentQuestion = (new QuestionAsked
                {
                    QType = qtype,
                    ValidAnswers = choices.Select(x => x[0].Text).ToArray(),
                    MessageId = msgId
                });
            }
            catch (AggregateException ex)
            {
                var e = ex.InnerExceptions.First();
                Send(e.Message, to.Id);
                Console.WriteLine($"Error sending menu: {e.Message}");
            }
            catch (Exception e)
            {
                Send(e.Message, to.Id);
                Console.WriteLine($"Error sending menu: {e.Message}");
            }
        }

        private void SendDayActions()
        {
            if (Players == null) return;
            foreach (var p in Players)
            {
                p.CurrentQuestion = null;
                p.Choice = 0;
            }
            Thread.Sleep(1000); //sleep to let any clear keyboard messages go through....
            //really, the only day action is the detective, if there is one...
            var detectives = Players.Where(x => x != null && x.PlayerRole == IRole.Detective & !x.IsDead);
            if (detectives.Any())
            {
                foreach (var detective in detectives)
                {
                    detective.Choice = 0;
                    var options = Players.Where(x => !x.IsDead && x.Id != detective.Id).ToList();
                    if (options.Any())
                    {
                        var choices = options.Select(x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId}|{x.Id}") }).ToList();
                        choices.Add(new[] { new InlineKeyboardCallbackButton(GetLocaleString("Skip"), $"vote|{Program.ClientId}|-1") });
                        SendMenu(choices, detective, GetLocaleString("AskDetect"), QuestionType.Detect);
                    }
                }
            }

            var arson = Players.FirstOrDefault(x => x.PlayerRole == IRole.Pyro & !x.IsDead);
            var alivePlayers = Players.Where(x => !x.IsDead);
            if (arson != null && alivePlayers.All(x => !WolfRoles.Contains(x.PlayerRole) && x.PlayerRole != IRole.SerialKiller && (x.PlayerRole != IRole.Pyro || x.Id == arson.Id)))
            {
                arson.Choice = 0;
                var options = Players.Where(x => !x.IsDead && x.Id != arson.Id).ToList();
                if (options.Any())
                {
                    Send(GetLocaleString("ExtraDouse"), arson.Id);
                    var choices = options.Select(x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId}|{x.Id}") }).ToList();
                    choices.Add(new[] { new InlineKeyboardCallbackButton(GetLocaleString("Skip"), $"vote|{Program.ClientId}|-1") });
                    SendMenu(choices, arson, GetLocaleString("AskDouse"), QuestionType.Douse);
                }
            }

            var mayor = Players.FirstOrDefault(x => x.PlayerRole == IRole.Mayor & !x.IsDead);
            if (mayor != null && GameDay == 1)
            {
                var choices = new[]
                {
                    new[]
                    {
                        new InlineKeyboardCallbackButton(GetLocaleString("Reveal"), $"vote|{Program.ClientId}|reveal")
                    }
                }.ToList();
                SendMenu(choices, mayor, GetLocaleString("AskMayor"), QuestionType.Mayor);
            }

            var sandmans = Players.Where(x => x.PlayerRole == IRole.Sandman & !x.IsDead & !x.HasUsedAbility);
            if (sandmans.Any())
            {
                foreach (var sandman in sandmans)
                {
                    var choices = new[]
{
                    new[]
                    {
                        new InlineKeyboardCallbackButton(GetLocaleString("Yes"), $"vote|{Program.ClientId}|yes"), new InlineKeyboardCallbackButton(GetLocaleString("No"), $"vote|{Program.ClientId}|no")
                    }
                }.ToList();
                    SendMenu(choices, sandman, GetLocaleString("AskSandman"), QuestionType.Sandman);
                }
            }

            var blacksmiths = Players.Where(x => x.PlayerRole == IRole.Blacksmith & !x.IsDead & !x.HasUsedAbility);
            if (blacksmiths.Any())
            {
                foreach(var blacksmith in blacksmiths)
                {
                    var choices = new[]
                    {
                    new[]
                    {
                        new InlineKeyboardCallbackButton(GetLocaleString("Yes"), $"vote|{Program.ClientId}|yes"), new InlineKeyboardCallbackButton(GetLocaleString("No"), $"vote|{Program.ClientId}|no")
                    }
                }.ToList();
                    SendMenu(choices, blacksmith, GetLocaleString("SpreadDust"), QuestionType.SpreadSilver);
                }
            }

            var pacifists = Players.Where(x => x.PlayerRole == IRole.Pacifist & !x.IsDead);
            if (pacifists.Any() && GameDay == 1)
            {
                foreach(var pacifist in pacifists)
                {
                    var choices = new[]
{
                    new[]
                    {
                        new InlineKeyboardCallbackButton(GetLocaleString("Peace"), $"vote|{Program.ClientId}|peace")
                    }
                }.ToList();
                    SendMenu(choices, pacifist, GetLocaleString("AskPacifist"), QuestionType.Pacifist);
                }
            }

            var speedWolves = Players.FirstOrDefault(x => x.PlayerRole == IRole.SpeedWolf & !x.IsDead);
            if (speedWolves != null && GameDay == 1)
            {
                var choices = new[]
                {
                    new[]
                    {
                        new InlineKeyboardCallbackButton(GetLocaleString("RunAway"), $"vote|{Program.ClientId}|runaway")
                    }
                }.ToList();
                SendMenu(choices, speedWolves, GetLocaleString("AskRunaway"), QuestionType.SpeedWolf);
            }

            var gunners = Players.Where(x => x.PlayerRole == IRole.Gunner & !x.IsDead);

            if (gunners.Any())
            {
                foreach (var gunner in gunners)
                {
                    gunner.Choice = 0;
                    if (gunner.Bullet > 0)
                    {
                        var options = Players.Where(x => !x.IsDead && x.Id != gunner.Id).ToList();
                        if (options.Any())
                        {
                            var choices = options.Select(x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId}|{x.Id}") }).ToList();
                            choices.Add(new[] { new InlineKeyboardCallbackButton(GetLocaleString("Skip"), $"vote|{Program.ClientId}|-1") });
                            SendMenu(choices, gunner, GetLocaleString("AskShoot", gunner.Bullet), QuestionType.Shoot);
                        }
                    }
                }
            }
        }

        private void SendNightActions()
        {
            if (Players == null) return;
            Thread.Sleep(1000); //sleep to let any clear keyboard messages go through....
            foreach (var player in Players.Where(x => !x.IsDead))
            {
                player.CurrentQuestion = null;
                player.Choice = 0;
                string msg = "";
                var targetBase = Players.Where(x => !x.IsDead && x.Id != player.Id).ToList();

                List<IPlayer> targets = new List<IPlayer>();

                QuestionType qtype = QuestionType.Lynch;
                if (!AllWolves.Contains(player.PlayerRole) && !Roles120.Contains(player.PlayerRole) && HowledTonight) // don't send action to non-wolves if howled.
                    continue;
                switch (player.PlayerRole)
                {
                    case IRole.SerialKiller:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskKill");
                        qtype = QuestionType.SerialKill;
                        break;
                    case IRole.Pyro:
                        targets = targetBase;
                        msg = GetLocaleString("AskDouse");
                        qtype = QuestionType.Douse;
                        break;
                    case IRole.Harlot:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskVisit");
                        qtype = QuestionType.Visit;
                        break;
                    case IRole.Fool:
                    case IRole.Seer:
                    case IRole.Sorcerer:
                        if (!player.Frozen)
                        {
                            targets = targetBase.ToList();
                            msg = GetLocaleString("AskSee");
                            qtype = QuestionType.See;
                        }
                        break;
                    case IRole.Oracle:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskSee");
                        qtype = QuestionType.See;
                        break;
                    case IRole.Police:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskPolice");
                        qtype = QuestionType.PoliceSee;
                        break;
                    case IRole.Sheriff:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskSheriff");
                        qtype = QuestionType.SheriffSee;
                        break;
                    case IRole.Lookout:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskLook");
                        qtype = QuestionType.Look;
                        break;
                    case IRole.Guard:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskProtect");
                        qtype = QuestionType.Protect;
                        break;
                    case IRole.GuardianAngel:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskGuard");
                        qtype = QuestionType.Guard;
                        break;
                    case IRole.Firefighter:
                        targets = Players.Where(x => !x.IsDead && !x.SaveWasted).ToList();
                        msg = GetLocaleString("AskSave");
                        qtype = QuestionType.Save;
                        break;
                    case IRole.Ninja:
                        if (player.HasUsedAbility)
                        {
                            player.HasUsedAbility = false;
                        }
                        else
                        {
                            msg = GetLocaleString("AskHide");
                            qtype = QuestionType.Hide;
                        }
                        break;
                    case IRole.Herbalist:
                        if (player.PoisonTarget == 0)
                        {
                            targets = targetBase.ToList();
                            msg = GetLocaleString("AskPoison");
                            qtype = QuestionType.Poison;
                        }
                        else
                        {
                            var poisoned = Players.FirstOrDefault(x => x.Id == player.PoisonTarget && !x.IsDead);
                            if (poisoned != null)
                            {
                                msg = GetLocaleString("AskCure", poisoned.GetName());
                                qtype = QuestionType.Cure;
                            }
                            else
                            {
                                Send(GetLocaleString("PoisonedAlreadyDead"), player.Id);
                                player.Choice = -1;
                            }
                        }
                        break;
                    case IRole.Wolf:
                    case IRole.AlphaWolf:
                    case IRole.WolfCub:
                    case IRole.Lycan:
                    case IRole.HungryWolf:
                    case IRole.RabidWolf:
                    case IRole.SpeedWolf:
                    case IRole.HowlingWolf:
                        if (!_silverSpread)
                        {
                            targets = targetBase.Where(x => !WolfRoles.Contains(x.PlayerRole) && !SupportWolves.Contains(x.PlayerRole)).ToList();
                            msg = GetLocaleString("AskEat");
                            var others = targetBase.GetPlayersForRoles(WolfRoles, exceptPlayer: player).Where(x => !x.Drunk).ToList();
                            if (others.Any())
                            {
                                var andStr = $" {GetLocaleString("And").Trim()} ";
                                msg += GetLocaleString("DiscussWith", others.Select(x => x.GetName()).Aggregate((current, a) => current + andStr + a));
                            }
                            qtype = QuestionType.Kill;
                        }
                        else
                            msg = null;
                        break;
                    case IRole.Cultist:
                        //if (GameDay % 2 == 1)
                        {
                            targets = targetBase.Where(x => x.PlayerRole != IRole.Cultist).ToList();
                            msg = GetLocaleString("AskConvert");
                            var otherCults = targetBase.Where(x => x.PlayerRole == IRole.Cultist).ToList();
                            if (otherCults.Any())
                            {
                                var andStr = GetLocaleString("And");
                                msg += GetLocaleString("DiscussWith", otherCults.Select(x => x.GetName()).Aggregate((current, a) => current + andStr + a));
                            }
                            qtype = QuestionType.Convert;
                        }
                        //else
                        //{
                        //    player.Choice = -1;
                        //}
                        break;
                    case IRole.CultistHunter:
                        targets = targetBase.ToList();
                        msg = GetLocaleString("AskHunt");
                        qtype = QuestionType.Hunt;
                        break;
                    case IRole.WildChild:
                        if (GameDay == 1)
                        {
                            targets = targetBase.ToList();
                            msg = GetLocaleString("AskRoleModel");
                            qtype = QuestionType.RoleModel;
                        }
                        else player.Choice = -1;
                        break;
                    case IRole.Imposter:
                        if (GameDay == 1)
                        {
                            targets = targetBase.ToList();
                            msg = GetLocaleString("AskImposter");
                            qtype = QuestionType.RoleModel;
                        }
                        else player.Choice = -1;
                        break;
                    case IRole.Doppelgänger:
                        if (GameDay == 1)
                        {
                            targets = targetBase.ToList();
                            msg = GetLocaleString("AskDoppelganger");
                            qtype = QuestionType.RoleModel;
                        }
                        else player.Choice = -1;
                        break;
                    case IRole.Cupid:
                        //this is a bit more difficult....
                        if (GameDay == 1)
                        {
                            targets = Players.Where(x => !x.IsDead).ToList();
                            msg = GetLocaleString("AskCupid1");
                            qtype = QuestionType.Lover1;
                        }
                        else player.Choice = -1;
                        break;
                    case IRole.Healer:
                        if (GameDay != 1 && !player.HasUsedAbility)
                        {
                            targets = Players.Where(x => x.IsDead && x.Team == ITeam.Village).ToList();
                            msg = GetLocaleString("AskRevive");
                            qtype = QuestionType.Revive;
                        }
                        else player.Choice = -1;
                        break;
                    case IRole.SnowWolf:
                        if (!_silverSpread)
                        {
                            targets = targetBase.Where(x => !WolfRoles.Contains(x.PlayerRole) && !x.Frozen && !SupportWolves.Contains(x.PlayerRole)).ToList(); // don't freeze one player twice in a row
                            msg = GetLocaleString("AskFreeze");
                            qtype = QuestionType.Freeze;
                        }
                        break;
                    case IRole.Snooper:
                        if (!_silverSpread)
                        {
                            targets = targetBase.Where(x => !WolfRoles.Contains(x.PlayerRole) && !SupportWolves.Contains(x.PlayerRole)).ToList();
                            msg = GetLocaleString("AskSnoop");
                            qtype = QuestionType.Snoop;
                        }
                        break;
                    default:
                        continue;
                }
                var buttons = targets.Select(x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId}|{x.Id}") }).ToList();
                if (player.PlayerRole == IRole.Pyro && Players.Any(x => x.IsDoused && !x.IsDead))
                    buttons.Add(new[] { new InlineKeyboardCallbackButton(GetLocaleString("Ignite"), $"vote|{Program.ClientId}|{player.Id}") });
                if (player.PlayerRole != IRole.WildChild && player.PlayerRole != IRole.Cupid && player.PlayerRole != IRole.Doppelgänger && player.PlayerRole != IRole.Imposter)
                    buttons.Add(new[] { new InlineKeyboardCallbackButton(GetLocaleString("Skip"), $"vote|{Program.ClientId}|-1") });

                // for herbalist when deciding to save or not or ninja
                if (qtype == QuestionType.Cure || qtype == QuestionType.Hide)
                {
                    buttons = new[]
                    {
                    new[]
                    {
                        new InlineKeyboardCallbackButton(GetLocaleString("Yes"), $"vote|{Program.ClientId}|yes"), new InlineKeyboardCallbackButton(GetLocaleString("No"), $"vote|{Program.ClientId}|no")
                    }
                }.ToList();
                }

                foreach (var wolf in Players.Where(x => WolfRoles.Contains(x.PlayerRole) || SupportWolves.Contains(x.PlayerRole)))
                {
                    if (wolf.Drunk || _silverSpread)
                        wolf.NoEat = true;
                }

                if (!player.Drunk && !String.IsNullOrWhiteSpace(msg))
                {
                    SendMenu(buttons, player, msg, qtype);
                    Thread.Sleep(100);
                }
                else
                {
                    player.Choice = -1;
                }
            } // alive players foreach
            _silverSpread = false;

            foreach (var p in Players)
            {
                //reset drunk and frozen status
                p.Drunk = false;
                p.Frozen = false;
                p.SaveWasted = false;
            }
        }

#endregion

#region Helpers
        public void CleanupButtons()
        {
            foreach (var id in _joinButtons)
            {
                Program.Bot.DeleteMessageAsync(ChatId, id);
                Thread.Sleep(500);
            }
        }
        public void FleePlayer(int banid)
        {
            if (IsInitializing)
            {
                SendWithQueue("Cannot flee while game is initializing.  Try again once game is done starting.");
                return;
            }
            var p = Players?.FirstOrDefault(x => x.Id == banid);
            if (p != null)
            {
                if (p.IsDead)
                {
                    return;
                }
                SendWithQueue(GetLocaleString("Flee", p.GetName()));

                if (IsRunning)
                {
                    //kill the player
                    p.IsDead = true;
                    p.TimeDied = DateTime.Now;
                    p.DayOfDeath = GameDay;
                    p.Fled = true;
                    SomeoneSmited = true;

                    SendWithQueue(GetLocaleString("PlayerRoleWas", p.GetName(), GetRoleDescription(p.PlayerRole)));

                    /*if (DbGroup.HasFlag(GroupConfig.ShowRolesDeath))
                        SendWithQueue(GetLocaleString("PlayerRoleWas", p.GetName(), GetDescription(p.PlayerRole)));*/
                    CheckRoleChanges();

                    //add the 'kill'
                    DBKill(p, p, KillMthd.Flee);
                    CheckForGameEnd();
                }
                else if (IsJoining)
                {
                    _playerListChanged = true;
                    Players.Remove(p);
                    //SendWithQueue(GetLocaleString("CountPlayersRemain", Players.Count.ToBold()));
                }
            }
        }

        public void Kill()
        {
            //forces game to exit
            try
            {
                if (IsJoining) //try to remove the joining button...
                    Program.Bot.EditMessageCaptionAsync(ChatId, _joinMsgId, "", null);
            }
            catch
            {
                //ignored
            }
            Send("Game removed");
            Program.RemoveGame(this);
        }

        private string GetRandomImage(List<string> input)
        {
            return input[Program.R.Next(0, input.Count)];
        }

        public void SkipVote()
        {
            foreach (var p in Players.Where(x => x.Choice == 0))
            {
                p.Choice = -1;
                p.NonVote = 0;
            }
        }

        public void HunterFinalShot(IPlayer hunter, KillMthd method)
        {
            CheckRoleChanges();

            //send a menu to the hunter, asking who he wants to kill as he is hung....
            var hunterChoices = new List<InlineKeyboardCallbackButton[]>();
            hunterChoices.AddRange(Players.Where(x => !x.IsDead).Select(x => new[] { new InlineKeyboardCallbackButton(x.Name, $"vote|{Program.ClientId}|{x.Id}") }));
            hunterChoices.Add(new[] { new InlineKeyboardCallbackButton(GetLocaleString("Skip"), $"vote|{Program.ClientId}|-1") });

            //raise hunter from dead long enough to shoot
            hunter.IsDead = false;

            SendMenu(hunterChoices, hunter, GetLocaleString(method == KillMthd.Lynch ? "HunterLynchedChoice" : "HunterShotChoice"), QuestionType.HunterKill);

            hunter.Choice = 0;
            //hunter gets 30 seconds to choose
            for (int i = 0; i < 30; i++)
            {
                if (hunter.Choice != 0)
                {
                    i = 30;
                }
                Thread.Sleep(1000);
            }
            hunter.IsDead = true;
            hunter.DayOfDeath = GameDay;
            if (hunter.Choice == 0)
            {
                SendWithQueue(GetLocaleString(method == KillMthd.Lynch ? "HunterNoChoiceLynched" : "HunterNoChoiceShot", hunter.GetName()));
            }
            else
            {
                if (hunter.Choice == -1)
                {
                    SendWithQueue(GetLocaleString(method == KillMthd.Lynch ? "HunterSkipChoiceLynched" : "HunterSkipChoiceShot", hunter.GetName()));
                }
                else
                {
                    //someone has been killed.....
                    var killed = Players.FirstOrDefault(x => x.Id == hunter.Choice);
                    if (killed != null)
                    {
                        SendWithQueue(GetLocaleString(method == KillMthd.Lynch ? "HunterKilledFinalLynched" : "HunterKilledFinalShot", hunter.GetName(), killed.GetName(), /*!DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ? "" :*/ $"{killed.GetName()} {GetLocaleString("Was")} {GetDescription(killed)}"));
                        if (killed.PlayerRole == IRole.WiseElder)
                        {
                            SendWithQueue(GetLocaleString("HunterKilledWiseElder", hunter.GetName(), killed.GetName()));
                            hunter.PlayerRole = IRole.Villager;
                            hunter.ChangedRolesCount++;
                        }
                        killed.IsDead = true;
                        killed.DayOfDeath = GameDay;
                        if (killed.PlayerRole == IRole.WolfCub)
                            WolfCubKilled = true;
                        if (killed.PlayerRole == IRole.RabidWolf)
                            RabidKilled = true;
                        if (killed.PlayerRole == IRole.Baker)
                            _bakerDied = true;
                        killed.TimeDied = DateTime.Now;
                        if (killed.PlayerRole == IRole.Wolf || killed.PlayerRole == IRole.AlphaWolf || killed.PlayerRole == IRole.WolfCub || killed.PlayerRole == IRole.Lycan || killed.PlayerRole == IRole.HungryWolf || killed.PlayerRole == IRole.Pyro || killed.PlayerRole == IRole.SerialKiller || killed.PlayerRole == IRole.SnowWolf || killed.PlayerRole == IRole.Snooper || killed.PlayerRole == IRole.SpeedWolf || killed.PlayerRole == IRole.RabidWolf || killed.PlayerRole == IRole.HowlingWolf)
                            AddAchievement(hunter, Achievements.HeyManNiceShot);
                        if (BadRoles.Contains(killed.PlayerRole))
                        {
                            hunter.PointsToAdd += 7;
                        }

                        DBKill(hunter, killed, KillMthd.HunterShot);
                        if (killed.InLove)
                            KillLover(killed);

                        CheckRoleChanges();
                        if (killed.PlayerRole == IRole.Hunter)
                            HunterFinalShot(killed, KillMthd.HunterShot);
                    }
                }
            }
        }

        public void Dispose()
        {
            Players?.Clear();
            Players = null;
            MessageQueueing = false;
        }

        public enum GameTime
        {
            Day,
            Lynch,
            Night
        }

        private int ChooseRandomPlayerId(IPlayer exclude, bool all = true)
        {
            try
            {
                var possible = exclude != null ? Players.Where(x => x.Id != exclude.Id).ToList() : Players.ToList();
                if (!all)
                    possible = possible.Where(x => !x.IsDead).ToList();
                possible.Shuffle();
                possible.Shuffle();
                return possible[0].Id;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
                Send("Unable to choose random player\n" + Program.Version.FileVersion + $"\nGroup: {ChatId} ({ChatGroup})\nLanguage: {DbGroup?.Language ?? "null"}\n{Program.ClientId}\n{ex.Message}\n{ex.StackTrace}", Program.ErrorGroup);
                return -1;
            }
        }

        //private int ChooseRandomPlayerId(IEnumerable<IPlayer> exclude)
        //{
        //    try
        //    {
        //        var possible = Players.Where(x => exclude.All(y => y?.TeleUser.Id != x?.TeleUser.Id)).ToList();
        //        possible.Shuffle();
        //        possible.Shuffle();
        //        return possible[0].Id;
        //    }
        //    catch
        //    {
        //        return -1;
        //    }
        //}

        internal static void ReplyToCallback(CallbackQuery query, string text = null, bool edit = true, bool showAlert = false, InlineKeyboardMarkup replyMarkup = null)
        {
            //first answer the callback
            Program.Bot.AnswerCallbackQueryAsync(query.Id, edit ? null : text, showAlert);
            //edit the original message
            if (edit)
                Edit(query, text, replyMarkup);
        }

        internal static Task<Telegram.Bot.Types.Message> Edit(CallbackQuery query, string text, InlineKeyboardMarkup replyMarkup = null)
        {
            return Edit(query.Message.Chat.Id, query.Message.MessageId, text, replyMarkup);
        }

        internal static Task<Telegram.Bot.Types.Message> Edit(long id, int msgId, string text, InlineKeyboardMarkup replyMarkup = null)
        {
            Program.MessagesSent++;
            return Program.Bot.EditMessageTextAsync(id, msgId, text, replyMarkup: replyMarkup);
        }

        internal void LogException(AggregateException ae)
        {
            foreach (var e in ae.InnerExceptions)
                LogAllExceptions(e);
        }

        internal void LogAllExceptions(Exception e)
        {
            do
            {
                LogException(e);
                e = e.InnerException;
            } while (e != null);
            return;
        }

        internal void LogException(Exception e)
        {
            Send(Program.Version.FileVersion + $"\nGroup: {ChatId} ({ChatGroup})\nLanguage: {DbGroup?.Language ?? "null"}\n{Program.ClientId}\n{e.Message}\n{e.StackTrace}", Program.ErrorGroup);
        }

#endregion

#region Database Helpers

        // ReSharper disable UnusedParameter.Local
        private void DBAction(IPlayer initator, IPlayer receiver, string action)
        {
            //return; //dropping actions.  We never use them, they just take up a massive amount of space in the database
            //using (var db = new WWContext())
            //{
            //    try
            //    {
            //        var initid = initator.DBPlayerId;
            //        if (initid == 0)
            //        {
            //            initid = GetDBPlayer(initator, db).Id;
            //        }
            //        var recid = receiver.DBPlayerId;
            //        if (recid == 0)
            //            recid = GetDBPlayer(receiver, db).Id;
            //        if (DBGameId == 0)
            //        {
            //            DBGameId = db.Games.FirstOrDefault(x => x.Id == GameId)?.Id ?? 0;
            //        }
            //        var a = new Action
            //        {
            //            ActionTaken = action,
            //            GameId = DBGameId,
            //            InitiatorId = initid,
            //            ReceiverId = recid,
            //            TimeStamp = DateTime.Now,
            //            Day = GameDay
            //        };
            //        db.Actions.Add(a);

            //        db.SaveChanges();
            //    }
            //    catch (Exception)
            //    {
            //        //Log.WriteLine(e.Message + "\n" + e.StackTrace, LogLevel.Error, fileName: "error.log");
            //    }
            //}
        }

        private void DBKill(IPlayer killer, IPlayer victim, KillMthd method)
        {
            _playerListChanged = true;
            using (var db = new WWContext())
            {
                try
                {
                    var killerid = killer.DBPlayerId;
                    if (killerid == 0)
                    {
                        var dbpKiller = GetDBPlayer(killer, db);
                        killerid = dbpKiller.Id;
                    }
                    Player dbpVictim = null;
                    var victimid = victim.DBPlayerId;
                    if (victimid == 0)
                    {
                        dbpVictim = GetDBPlayer(victim, db);
                        victimid = dbpVictim.Id;
                    }
                    if (DBGameId == 0)
                    {
                        DBGameId = db.Games.FirstOrDefault(x => x.Id == GameId)?.Id ?? 0;
                    }

                    if (gameMode != 4)
                    {
                        var dbgp = dbpVictim == null ? GetDBGamePlayer(victim, db) : GetDBGamePlayer(dbpVictim);
                        dbgp.Survived = false;
                        db.SaveChanges();
                    }
                    var gk = new GameKill
                    {
                        Day = GameDay,
                        GameId = DBGameId,
                        KillMethodId = (int)method,
                        KillerId = killerid,
                        TimeStamp = DateTime.Now,
                        VictimId = victimid
                    };
                    db.GameKills.Add(gk);

                    db.SaveChanges();
                }
                catch (Exception)
                {
                    //Send(e.Message + "\n" + e.InnerException);
                    //Log.WriteLine(e.Message + "\n" + e.StackTrace, LogLevel.Error, fileName: "error.log");
                }

                try
                {
                    //if (ChatId == -1001094614730) //vets group
                    //{
                    //    //let's try this out, shall we?
                    //    var status = Program.Bot.GetChatMemberAsync(-1001094614730, victim.Id).Result;
                    //    if (status.Status != ChatMemberStatus.Administrator && status.Status != ChatMemberStatus.Creator)
                    //    {
                    //        Program.Bot.RestrictChatMemberAsync(-1001094614730, victim.Id, DateTime.Now.AddHours(1), false, false, false, false);
                    //    }
                    //}
                }
                catch
                {

                }
            }

            if (victim.LoverId == killer.Id && Time == GameTime.Night && method != KillMthd.LoverDied)
            {
                if (GameDay == 1) //killed lover on first night
                    AddAchievement(killer, Achievements.OhShi);
                else if (WolfRoles.Contains(killer.PlayerRole)) //wolf pack killed lover, not on first night
                    AddAchievement(killer, Achievements.ShouldveMentioned);
            }

        }

        private void KillLover(IPlayer victim)
        {
            var p = Players.FirstOrDefault(x => x.Id == victim.LoverId && !x.IsDead);
            if (p != null)
            {
                SendWithQueue(GetLocaleString("LoverDied", victim.GetName(), p.GetName(), /*!DbGroup.HasFlag(GroupConfig.ShowRolesDeath) ? "" :*/ $"{p.GetName()} {GetLocaleString("Was")} {GetDescription(p)}"));
                DBKill(victim, p, KillMthd.LoverDied);
                p.IsDead = true;
                p.DayOfDeath = GameDay;
                if (p.PlayerRole == IRole.WolfCub)
                    WolfCubKilled = true;
                if (p.PlayerRole == IRole.RabidWolf)
                    RabidKilled = true;
                if (p.PlayerRole == IRole.Baker)
                    _bakerDied = true;
                p.TimeDied = DateTime.Now;
            }
            CheckRoleChanges();
            if (p?.PlayerRole == IRole.Hunter)
            {
                HunterFinalShot(p, KillMthd.LoverDied);
            }
        }

        private bool _longHaulReached;

        private void CheckLongHaul()
        {
            if (Players == null) return;
            if ((DateTime.Now - _timeStarted).Hours >= 1 & !_longHaulReached)
            {
                foreach (var p in Players.Where(x => !x.IsDead && !x.Fled))
                {
                    AddAchievement(p, Achievements.LongHaul);
                }
                _longHaulReached = true;
            }
        }

        public int DBGameId { get; set; }

        private void DBKill(IEnumerable<IPlayer> killers, IPlayer victim, KillMthd method)
        {
            foreach (var killer in killers)
                DBKill(killer, victim, method);
        }

        private Player GetDBPlayer(IPlayer player, WWContext db)
        {
            if (player.DBPlayerId == 0)
            {
                var p = db.Players.FirstOrDefault(x => x.TelegramId == player.Id);
                player.DBPlayerId = p?.Id ?? 0;
                return p;
            }
            try
            {
                return db.Players.Find(player.DBPlayerId);
            }
            catch
            {
                return null;
            }
        }

        private GamePlayer GetDBGamePlayer(Player player)
        {
            return player?.GamePlayers.FirstOrDefault(x => x.GameId == GameId);
        }

        private GamePlayer GetDBGamePlayer(IPlayer player, WWContext db)
        {
            if (player.DBGamePlayerId == 0)
            {
                var p = GetDBGamePlayer(GetDBPlayer(player, db));
                player.DBGamePlayerId = p?.Id ?? 0;
                return p;
            }

            try
            {
                return db.GamePlayers.Find(player.DBGamePlayerId);
            }
            catch
            {
                return null;
            }
        }

        private void UpdateAchievements()
        {
            if (Players == null) return;
            if (gameMode == 3 || gameMode == 4) return;
            using (var db = new WWContext())
            {
                //check for convention
                var convention = Players.Count(x => x.PlayerRole == IRole.Cultist & !x.IsDead) >= 10;
                foreach (var player in Players.Where(x => !x.Fled)) //flee / afk? no achievements for you.
                {
                    var p = GetDBPlayer(player, db);


                    if (p != null)
                    {
                        // saving new achvs
                        BitArray newAch = new BitArray(200);

                        // existing achvs
                        BitArray ach = new BitArray(200);
                        if (p.Achievements != null)
                        {
                            ach = new BitArray(p.Achievements);
                        }

                        //calculate achievements
                        //automatically get welcome to hell
                        if (!ach.HasFlag(Achievements.WelcomeToHell))
                            newAch.Set(Achievements.WelcomeToHell);
                        if (!ach.HasFlag(Achievements.WelcomeToAsylum) && Chaos)
                            newAch.Set(Achievements.WelcomeToAsylum);
                        if (!ach.HasFlag(Achievements.AlzheimerPatient) && Language.Contains("Amnesia"))
                            newAch.Set(Achievements.AlzheimerPatient);
                        //if (!ach2.HasFlag(AchievementsReworked.OHAIDER) && Players.Any(x => x.TeleUser.Id == Program.Para))
                        //    newAch2.Set(AchievementsReworked.OHAIDER);
                        if (!ach.HasFlag(Achievements.SpyVsSpy) & !DbGroup.HasFlag(GroupConfig.ShowRolesDeath))
                            newAch.Set(Achievements.SpyVsSpy);
                        if (!ach.HasFlag(Achievements.NoIdeaWhat) & !DbGroup.HasFlag(GroupConfig.ShowRolesDeath) && Language.Contains("Amnesia"))
                            newAch.Set(Achievements.NoIdeaWhat);
                        if (!ach.HasFlag(Achievements.Enochlophobia) && Players.Count == 50)
                            newAch.Set(Achievements.Enochlophobia);
                        if (!ach.HasFlag(Achievements.Introvert) && Players.Count == 4)
                            newAch.Set(Achievements.Introvert);
                        if (!ach.HasFlag(Achievements.Naughty) && Language.Contains("NSFW"))
                            newAch.Set(Achievements.Naughty);
                        if (!ach.HasFlag(Achievements.Dedicated) && p.GamePlayers.Count >= 100)
                            newAch.Set(Achievements.Dedicated);
                        if (!ach.HasFlag(Achievements.Obsessed) && p.GamePlayers.Count >= 1000)
                            newAch.Set(Achievements.Obsessed);
                        if (!ach.HasFlag(Achievements.Veteran) && p.GamePlayers.Count >= 500)
                            newAch.Set(Achievements.Veteran);
                        if (!ach.HasFlag(Achievements.Masochist) && player.Won && player.PlayerRole == IRole.Tanner)
                            newAch.Set(Achievements.Masochist);
                        if (!ach.HasFlag(Achievements.Wobble) && !player.IsDead && player.PlayerRole == IRole.Drunk && Players.Count >= 10)
                            newAch.Set(Achievements.Wobble);
                        if (!ach.HasFlag(Achievements.Survivalist) && p.GamePlayers.Count(x => x.Survived) >= 100)
                            newAch.Set(Achievements.Survivalist);
                        if (!ach.HasFlag(Achievements.MasonBrother) && player.PlayerRole == IRole.Mason && Players.Count(x => x.PlayerRole == IRole.Mason & !x.IsDead) >= 2)
                            newAch.Set(Achievements.MasonBrother);
                        if (!ach.HasFlag(Achievements.ChangingSides) && player.OriginalRole != player.PlayerRole && player.Won)
                            newAch.Set(Achievements.ChangingSides);
                        if (!ach.HasFlag(Achievements.LoneWolf) && Players.Count >= 10 && WolfRoles.Contains(player.PlayerRole) && Players.GetPlayersForRoles(WolfRoles, false).Count() == 1 && Players.GetPlayersForRoles(SupportWolves, false).Count() == 0 && player.Won)
                            newAch.Set(Achievements.LoneWolf);
                        if (!ach.HasFlag(Achievements.Inconspicuous) && !player.HasBeenVoted & !player.IsDead)
                            newAch.Set(Achievements.Inconspicuous);
                        if (!ach.HasFlag(Achievements.Promiscuous) && !player.HasStayedHome & !player.HasRepeatedVisit && player.PlayersVisited.Count >= 5)
                            newAch.Set(Achievements.Promiscuous);
                        if (!ach.HasFlag(Achievements.DoubleShifter) && player.ChangedRolesCount >= 2)
                            newAch.Set(Achievements.DoubleShifter);
                        if (!ach.HasFlag(Achievements.BrokenClock) && player.FoolCorrectSeeCount >= 2)
                            newAch.Set(Achievements.BrokenClock);
                        if (!ach.HasFlag(Achievements.SmartGunner) && player.PlayerRole == IRole.Gunner & !player.BulletHitVillager && player.Bullet == 0)
                            newAch.Set(Achievements.SmartGunner);
                        if (!ach.HasFlag(Achievements.CultCon) && player.PlayerRole == IRole.Cultist && convention)
                            newAch.Set(Achievements.CultCon);
                        if (!ach.HasFlag(Achievements.SerialSamaritan) && player.PlayerRole == IRole.SerialKiller && player.SerialKilledWolvesCount >= 3)
                            newAch.Set(Achievements.SerialSamaritan);
                        if (!ach.HasFlag(Achievements.CultistTracker) && player.PlayerRole == IRole.CultistHunter && player.CHHuntedCultCount >= 3)
                            newAch.Set(Achievements.CultistTracker);
                        if (!ach.HasFlag(Achievements.ImNotDrunk) && player.PlayerRole == IRole.ClumsyGuy && player.ClumsyCorrectLynchCount >= 3)
                            newAch.Set(Achievements.ImNotDrunk);
                        if (!ach.HasFlag(Achievements.WuffieCult) && player.PlayerRole == IRole.AlphaWolf && player.AlphaConvertCount >= 3)
                            newAch.Set(Achievements.WuffieCult);
                        if (!ach.HasFlag(Achievements.DidYouGuardYourself) && player.PlayerRole == IRole.GuardianAngel && player.GAGuardWolfCount >= 3)
                            newAch.Set(Achievements.DidYouGuardYourself);
                        if (!ach.HasFlag(Achievements.ThreeLittleWolves) && player.PlayerRole == IRole.Sorcerer && Players.GetPlayersForRoles(WolfRoles, true).Count() >= 3)
                            newAch.Set(Achievements.ThreeLittleWolves);
                        if (!ach.HasFlag(Achievements.President) && player.PlayerRole == IRole.Mayor && player.MayorLynchAfterRevealCount >= 3)
                            newAch.Set(Achievements.President);
                        if (!ach.HasFlag(Achievements.ItWasABusyNight) && player.BusyNight)
                            newAch.Set(Achievements.ItWasABusyNight);
                        if (!ach.HasFlag(Achievements.TannerWolf) && player.LynchedWolf && player.Won)
                            newAch.Set(Achievements.TannerWolf);
                        //now save
                        p.Achievements = ach.Or(newAch).ToByteArray();
                        var newPoints = 0;
                        for (int i = 0; i < newAch.Length; i++)
                        {
                            if (newAch[i])
                                newPoints += 5;
                        }
                        p.Score += newPoints;
                        db.SaveChanges();

                        //notify
                        var newFlags = newAch.GetUniqueFlags().ToList();
                        if (newAch.Cast<bool>().All(x => x == false)) continue;
                        var msg2 = "New Unlocks!".ToBold() + Environment.NewLine;
                        msg2 = newFlags.Aggregate(msg2, (current, a) => current + $"{a.GetName().ToBold()}\n{a.GetDescription()}\n\n");
                        Send(msg2, p.TelegramId);
                        Send(GetLocaleString("PointsObtained", newPoints, p.Score).ToBold(), p.TelegramId);
                    }
                }
            }
        }

        private void UpdateGroupRanking()
        {
            using (var db = new WWContext())
            {
                var refreshdate = db.RefreshDate.FirstOrDefault().Date;
                if (DateTime.Now.Date - refreshdate >= TimeSpan.FromDays(1))
                {
                    refreshdate = DateTime.Now.Date;
                    db.RefreshDate.FirstOrDefault().Date = refreshdate;
                    db.SaveChanges();
                }

                var grpranking = db.GroupRanking.FirstOrDefault(x => x.GroupId == DbGroup.Id && x.Language == Locale.Language);
                if (grpranking == null)
                {
                    grpranking = new GroupRanking { GroupId = DbGroup.Id, Language = Locale.Language, LastRefresh = refreshdate };
                    db.GroupRanking.Add(grpranking);
                    db.SaveChanges();
                }

                if (grpranking.LastRefresh < refreshdate && grpranking.GamesPlayed != 0) //games played should always be != 0, but you never know..
                {
                    var daysspan = (refreshdate - grpranking.LastRefresh).Days; //well really this should be 1
                    daysspan = daysspan == 0 ? 1 : daysspan;
                    var avgplayerspergame = ((decimal)grpranking.PlayersCount) / grpranking.GamesPlayed; //this is between 0 and 35
                    var playerfactor = -((decimal)0.05) * (avgplayerspergame * avgplayerspergame) + (decimal)2.5 * avgplayerspergame - (decimal)11.25; //quadratic function, max at 25 (equals 20), zero at 5.
                    var avgminutesperday = grpranking.MinutesPlayed / daysspan; //average minutes played per day
                    var timefactor = avgplayerspergame * (decimal)1.6 * avgminutesperday / 1440; //(avg minutes per day played by the avg player) / (15 h in minutes). 15h is approximately the time played per day by the most active groups.
                    var malus = (playerfactor - timefactor) * (playerfactor - timefactor) / 5; //give some malus if they played for little time with lots of people or vice versa. 
                    grpranking.Ranking = Math.Round(playerfactor + timefactor - malus, 10);
                    grpranking.PlayersCount = 0;
                    grpranking.MinutesPlayed = 0;
                    grpranking.GamesPlayed = 0;
                    grpranking.LastRefresh = refreshdate;
                    db.SaveChanges();
                }

                if (_timePlayed.HasValue)
                {
                    grpranking.GamesPlayed++;
                    grpranking.PlayersCount += Players.Count();
                    grpranking.MinutesPlayed += Math.Round((decimal)_timePlayed.Value.TotalMinutes, 10);
                    db.SaveChanges();
                }

                db.SaveChanges();
            }
        }

        private void AddAchievement(IPlayer player, Achievements a)
        {
            if (gameMode == 3 || gameMode == 4) return;
            using (var db = new WWContext())
            {
                var p = GetDBPlayer(player, db);
                if (p != null)
                {
                    var ach = new BitArray(200);
                    if (p.Achievements != null)
                        ach = new BitArray(p.Achievements);
                    if (ach.HasFlag(a)) return; //no point making another db call if they already have it
                    ach = ach.Set(a);
                    p.Achievements = ach.ToByteArray();
                    player.Score = p.Score;
                    player.Score += 5;
                    p.Score = player.Score;
                    db.SaveChanges();

                    Send($"Achievement Unlocked!\n{a.GetName().ToBold()}\n{a.GetDescription()}", player.Id);
                    Send(GetLocaleString("PointsObtained", 5, player.Score).ToBold(), player.Id);
                }
            }
        }

        #endregion
    }
}
