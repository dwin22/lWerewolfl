using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Database;
using Werewolf_Node.Models;

namespace Werewolf_Node.Helpers
{
    public static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string ToBold(this object str)
        {
            if (str == null)
                return null;
            return $"<b>{str.ToString().FormatHTML()}</b>";
        }

        public static string ToItalic(this object str)
        {
            if (str == null)
                return null;
            return $"<i>{str.ToString().FormatHTML()}</i>";
        }

        public static string FormatHTML(this string str)
        {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        public static string GetName(this IPlayer player, bool menu = false, bool dead = false)
        {
            var name = player.Name;

            var end = name.Substring(name.Length - Math.Min(name.Length, 5));
            name = name.Substring(0, Math.Max(name.Length - 5, 0));
            end = end.Replace("🥇", "").Replace("🥈", "").Replace("🥉", "").Replace("💎", "").Replace("🏆", "").Replace("🔅", "").Replace("🔆", "");

            name += end;

            if (menu)
                return name;
            //if (!String.IsNullOrEmpty(player.TeleUser.Username))
            if (!dead)
                return $"<a href=\"tg://user?id={player.TeleUser.Id}\">{name.FormatHTML()}</a>";

            return name.ToBold();
        }

        public static string GetRankName(this IPlayer player, int gm = 0)
        {
            var name = player.Name;

            var end = name.Substring(0, Math.Min(name.Length, 5));
            if (name.Length > 5)
                name = name.Substring(5);
            else
                name = "";
            end = end.Replace("🥇", "").Replace("🥈", "").Replace("🥉", "").Replace("💎", "").Replace("🏆", "").Replace("🔅", "").Replace("🔆", "");

            if (player.ShowRank && gm == 5)
            {
                if (player.Score >= 2200)
                    end = "🏆 " + end;
                else if (player.Score >= 2000)
                    end = "💎 " + end;
                else if (player.Score >= 1800)
                    end = "🥇 " + end;
                else if (player.Score >= 1600)
                    end = "🥈 " + end;
                else if (player.Score >= 1400)
                    end = "🥉 " + end;
                else if (player.Score >= 1200)
                    end = "🔆 " + end;
                else if (player.Score > 1000)
                    end = "🔅 " + end;
            }

            name = end + name;

            return $"<a href=\"tg://user?id={player.TeleUser.Id}\">{name.FormatHTML()}</a>";
        }

        public static IEnumerable<IPlayer> GetLivingPlayers(this IEnumerable<IPlayer> players)
        {
            return players?.Where(x => !x.IsDead);
        }

        public static IEnumerable<IPlayer> GetPlayersForTeam(this IEnumerable<IPlayer> players, ITeam team, bool aliveOnly = true, IPlayer exceptPlayer = null)
        {
            return players?.Where(x => x.Team == team && (!aliveOnly || !x.IsDead) && x.Id != exceptPlayer?.Id);
        }

        public static IPlayer GetPlayerForRole(this IEnumerable<IPlayer> players, IRole role, bool aliveOnly = true, IPlayer exceptPlayer = null)
        {
            return players?.FirstOrDefault(x => x.PlayerRole == role && (!aliveOnly || !x.IsDead) && x.Id != exceptPlayer?.Id);
        }

        public static IEnumerable<IPlayer> GetPlayersForRoles(this IEnumerable<IPlayer> players, IRole[] roles,
            bool aliveOnly = true, IPlayer exceptPlayer = null)
        {
            return players?.Where(x => roles.Contains(x.PlayerRole) && (!aliveOnly || !x.IsDead) && x.Id != exceptPlayer?.Id);
        }


        public static int GetStrength(this IRole role, List<IRole> allRoles)
        {
            IRole[] WolfRoles = { IRole.Wolf, IRole.WolfCub, IRole.AlphaWolf, IRole.Lycan, IRole.HungryWolf, IRole.RabidWolf, IRole.SnowWolf, IRole.Snooper, IRole.SpeedWolf, IRole.HowlingWolf };
            IRole[] nonConvertibleRoles = { IRole.GuardianAngel, IRole.Cursed, IRole.Harlot, IRole.Wolf, IRole.AlphaWolf, IRole.WolfCub, IRole.SerialKiller,
                IRole.HungryWolf, IRole.Pyro, IRole.Sheriff, IRole.Lycan, IRole.Atheist, IRole.RabidWolf, IRole.SnowWolf, IRole.Sleepwalker, IRole.Snooper, IRole.SpeedWolf, IRole.HowlingWolf };
            switch (role)
            {
                case IRole.Villager:
                    return 1;
                case IRole.Drunk:
                    return 3;
                case IRole.Harlot:
                    return 6 - (!allRoles.Any(x => WolfRoles.Contains(x) || x == IRole.SerialKiller || x == IRole.Cultist) ? 3 : 0); // not that good against pyro
                case IRole.Seer:
                    return 7;
                case IRole.Traitor:
                    return -5;
                case IRole.GuardianAngel:
                    return 7;
                case IRole.Detective:
                    return 6;
                case IRole.Wolf:
                    return 10;
                case IRole.Cursed:
                    return 2 - (allRoles.Any(x => WolfRoles.Contains(x) || x == IRole.SnowWolf) ? 5 : 0);
                case IRole.Gunner:
                    return 6;
                case IRole.Tanner:
                    return 5;
                case IRole.Fool:
                    return 3;
                case IRole.WildChild:
                    return -3;
                case IRole.Beholder:
                    return 2 + (allRoles.Any(x => x == IRole.Seer) ? 2 : 0); //only good if seer is present!
                case IRole.ApprenticeSeer:
                    return 6;
                case IRole.Cultist:
                    return 7 - Math.Min(allRoles.Count(x => nonConvertibleRoles.Contains(x)), 5);
                case IRole.CultistHunter:
                    return allRoles.Count(x => x == IRole.Cultist) == 0 ? 2 : 5;
                case IRole.Mason:
                    return allRoles.Count(x => x == IRole.Mason) <= 1 ? 2 : allRoles.Count(x => x == IRole.Mason) + 2; //strength in numbers
                case IRole.Doppelgänger:
                    return -2;
                case IRole.Cupid:
                    return -2;
                case IRole.Hunter:
                    return 5;
                case IRole.SerialKiller:
                    return 12 - ((allRoles.Any(x => x == IRole.Cultist || x == IRole.Pyro || WolfRoles.Contains(x)) && allRoles.Count() > 7) ? 4 : 0);
                case IRole.Sorcerer:
                    return 6;
                case IRole.AlphaWolf:
                    return 14;
                case IRole.WolfCub:
                    return new[] { IRole.AlphaWolf, IRole.Wolf, IRole.Cursed, IRole.Lycan, IRole.WildChild, IRole.HungryWolf, IRole.Traitor, IRole.SnowWolf, IRole.Snooper }
                        .Any(x => allRoles.Contains(x)) ? 12 : 10;
                case IRole.Blacksmith:
                    return 5 - (!allRoles.Any(x => WolfRoles.Contains(x) || x == IRole.SnowWolf) ? 1 : 0); // only good vs ww
                case IRole.ClumsyGuy:
                    return -1;
                case IRole.Mayor:
                    return 4;
                case IRole.Prince:
                    return 3;
                case IRole.WolfMan:
                    return 1;
                case IRole.Pacifist:
                    return 4;
                case IRole.WiseElder:
                    return 5 - (!allRoles.Any(x => WolfRoles.Contains(x) || x == IRole.SnowWolf || x == IRole.Cultist) ? 3 : 0); // only good vs ww and cult
                case IRole.Oracle:
                    return 5;
                case IRole.Sandman:
                    return 6;
                case IRole.Lycan:
                    return 10 + (allRoles.Any(x => x == IRole.Seer) ? 2 : 0); //only good if seer is present!
                case IRole.Thief:
                    return 4;
                case IRole.Survivor:
                    return 1;
                case IRole.Atheist:
                    return 3;
                case IRole.Pyro:
                    return 12 - ((allRoles.Any(x => x == IRole.SerialKiller || x == IRole.Pyro || WolfRoles.Contains(x))  && allRoles.Count() > 7) ? 4 : 0);
                case IRole.HungryWolf:
                    return 12;
                case IRole.Sheriff:
                    return 7;
                case IRole.Police:
                    return 3;
                case IRole.Imposter:
                    return 5;
                case IRole.Baker:
                    return -3;
                case IRole.Healer:
                    return 7;
                case IRole.RabidWolf:
                    return 16;
                case IRole.Sleepwalker:
                    return 2;
                case IRole.Herbalist:
                    return 9;
                case IRole.SnowWolf:
                    return 12;
                case IRole.Snooper:
                    return 12 + allRoles.Count(x => x == IRole.Herbalist || x == IRole.Drunk || x == IRole.Baker);
                case IRole.Ninja:
                    return 5 - (!allRoles.Any(x => WolfRoles.Contains(x) || x == IRole.Cultist || x == IRole.SerialKiller) ? 3 : 0); // not that good against pyro
                case IRole.SpeedWolf:
                    return 14;
                case IRole.Lookout:
                    return 5;
                case IRole.Guard:
                    return 3;
                case IRole.HowlingWolf:
                    return 12;
                case IRole.Firefighter:
                    return 6;
                case IRole.Ghost:
                    return 5;
                case IRole.Miner:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }

        }

        public static double GetRoleScore(this IRole role, bool won, int initialww, int playercount)
        {
            switch (role)
            {
                case IRole.SerialKiller:
                case IRole.Pyro:
                    return won ? (playercount + 3) : -2;
                case IRole.Wolf:
                case IRole.AlphaWolf:
                case IRole.WolfCub:
                case IRole.Lycan:
                case IRole.HungryWolf:
                case IRole.RabidWolf:
                case IRole.Sorcerer:
                case IRole.Imposter:
                case IRole.Snooper:
                case IRole.SnowWolf:
                case IRole.SpeedWolf:
                case IRole.HowlingWolf:
                    return won ? ((playercount / Math.Max(1, initialww)) + 2) : -4;
                case IRole.Tanner:
                    return won ? 16 : -3;
                case IRole.CultistHunter:
                    return won ? 9 : -5;
                case IRole.Survivor:
                    return won ? 6 : -2;
                case IRole.Cultist:
                    return won ? 5 : -4;
                case IRole.Doppelgänger:
                    return won ? 5 : -2;
                default:
                    return won ? 8 : -4;
            }
        }

        public static string GetEmoji(this IRole role)
        {
            switch (role)
            {
                case IRole.Villager:
                    return "👱";
                case IRole.Drunk:
                    return "🍻";
                case IRole.Harlot:
                    return "💋";
                case IRole.Seer:
                    return "👳";
                case IRole.Traitor:
                    return "🖕";
                case IRole.GuardianAngel:
                    return "👼";
                case IRole.Detective:
                    return "🕵️";
                case IRole.Wolf:
                    return "🐺";
                case IRole.Cursed:
                    return "😾";
                case IRole.Gunner:
                    return "🔫";
                case IRole.Tanner:
                    return "👺";
                case IRole.Fool:
                    return "🃏";
                case IRole.WildChild:
                    return "👶";
                case IRole.Beholder:
                    return "👁";
                case IRole.ApprenticeSeer:
                    return "🙇";
                case IRole.Cultist:
                    return "👤";
                case IRole.CultistHunter:
                    return "💂";
                case IRole.Mason:
                    return "👷";
                case IRole.Doppelgänger:
                    return "🎭";
                case IRole.Cupid:
                    return "🏹";
                case IRole.Hunter:
                    return "🎯";
                case IRole.SerialKiller:
                    return "🔪";
                case IRole.Sorcerer:
                    return "🔮";
                case IRole.AlphaWolf:
                    return "⚡️";
                case IRole.WolfCub:
                    return "🐶";
                case IRole.Blacksmith:
                    return "⚒";
                case IRole.ClumsyGuy:
                    return "🤕";
                case IRole.Mayor:
                    return "🎖";
                case IRole.Prince:
                    return "👑";
                case IRole.WolfMan:
                    return "👱🌚";
                case IRole.Lycan:
                    return "🐺🌝";
                case IRole.Sandman:
                    return "💤";
                case IRole.Oracle:
                    return "🌀";
                case IRole.WiseElder:
                    return "📚";
                case IRole.Pacifist:
                    return "☮️";
                case IRole.Thief:
                    return "😈";
                case IRole.Pyro:
                    return "🔥";
                case IRole.HungryWolf:
                    return "🐺🍽";
                case IRole.Atheist:
                    return "👦";
                case IRole.Survivor:
                    return "⛺️";
                case IRole.Sheriff:
                    return "🤠";
                case IRole.Police:
                    return "👮";
                case IRole.Imposter:
                    return "❌";
                case IRole.Baker:
                    return "🍞";
                case IRole.Healer:
                    return "🌟";
                case IRole.RabidWolf:
                    return "🐺🤢";
                case IRole.Sleepwalker:
                    return "😴";
                case IRole.Herbalist:
                    return "🍃";
                case IRole.SnowWolf:
                    return "🐺❄️";
                case IRole.Ninja:
                    return "💨";
                case IRole.Snooper:
                    return "🐾";
                case IRole.SpeedWolf:
                    return "🐺💨";
                case IRole.Lookout:
                    return "🔭";
                case IRole.Guard:
                    return "🛡";
                case IRole.HowlingWolf:
                    return "🐺🌕";
                case IRole.Firefighter:
                    return "👨‍🚒";
                case IRole.Ghost:
                    return "👻";
                case IRole.Miner:
                    return "⛏";
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }
    }
}
