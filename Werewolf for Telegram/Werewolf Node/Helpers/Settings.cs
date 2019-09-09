using System.Collections.Generic;

namespace Werewolf_Node
{
    static class Settings
    {
#if DEBUG
        public static string ServerIP = "127.0.0.1";
#else
        public static string ServerIP = "127.0.0.1";

#endif
#if DEBUG
        public static int Port = 9049;
#elif RELEASE
        public static int Port = 9050;
#elif RELEASE2
        public static int Port = 9051;
#elif BETA
        public static int Port = 9052;
#endif


#if RELEASE2
        internal static List<string> VillagerDieImages = new List<string> { "BQADAwAD2QEAAnQXsQeU2FMN-2D3GgI", "BQADAwADggADdBexB1_X0udQaRs7Ag", "BQADBAADWAMAAt4cZAcXTtE-UCQXxAI" }; //1
        internal static List<string> WolfWin = new List<string> { "BQADAwADgQADdBexB5kx2CsSNDp2Ag", "BQADAwADgAADdBexBx7XahnW5XBsAg" };
        internal static List<string> WolvesWin = new List<string> { "BQADBAADlwMAAtgaZAeaog1gGP_fkwI", "BQADBAADcAMAAn8ZZAfjUfLaMGoxzgI" };
        internal static List<string> VillagersWin = new List<string> { "BQADAwADgwADdBexB90OD5PHXLAuAg" };
        internal static List<string> NoWinner = new List<string> { "BQADBAAD8QgAAqIeZAeLBjBE4l0LSAI", "BQADBAADuAMAAlUXZAfHXDmd504z5AI" };
        internal static List<string> StartGame = new List<string> { "BQADAwADhAADdBexB7b36d3MSPzDAg", "BQADBAADwg0AAu0XZAcwCVhaZgAB_CsC" };
        internal static List<string> StartChaosGame = new List<string> { "BQADAwAD1wEAAnQXsQeswdJwV9BIyQI", "BQADAwAD2AEAAnQXsQeGw_-A8E7DLwI" }; //2
        internal static List<string> TannerWin = new List<string> { "BQADBAADQwgAAuQaZAcjXuF_tkE3JwI", "BQADBAAD_gMAAtgaZAf2YeVX6mXnUQI" };
        internal static List<string> CultWins = new List<string> { "BQADBAADWAMAAosYZAcuRvZYBpQmXwI", "BQADBAADHwsAAgUYZAcUTEIahD8XSQI" };
        internal static List<string> SerialKillerWins = new List<string> { "BQADBAADdQMAAsEcZAd7skaRqoWKzQI", "BQADBAADmgMAArgcZAdPyqayfRT6bQI", "BQADBAADOAQAAqUXZAeeuV5vjRd6QAI", "BQADBAADKwMAAsQZZAfwd2_EAeeOTgI" };
        internal static List<string> LoversWin = new List<string> { "BQADBAAD8hUAAhYYZAeHmbRRzioXXQI", "BQADBAADYAMAAkMdZAfR4qo8c95FGgI" };
#elif RELEASE
        internal static List<string> VillagerDieImages = new List<string> { "BQADAwADggADdBexB2aZTAMpXRGUAg", "BQADBAADKgMAAoMbZAfmSqOE1YY-9wI", "BQADBAADWAMAAt4cZAe6rGbV3KvLggI" };
        internal static List<string> WolfWin = new List<string> { "BQADAwADgAADdBexB015e-EU6O9CAg", "BQADAwADgQADdBexB9ksBD5NOWQvAg" };
        internal static List<string> WolvesWin = new List<string> { "BQADBAADlwMAAtgaZAfCK5MVLj27CwI", "BQADBAADcAMAAn8ZZAe0Xjey6zCmQwI" };
        internal static List<string> VillagersWin = new List<string> { "BQADAwADgwADdBexB2K0cDWari8QAg" };
        internal static List<string> NoWinner = new List<string> { "BQADBAAD8QgAAqIeZAeufUVx9eT3SgI", "BQADBAADuAMAAlUXZAfNGmq-KVN_0AI" };
        internal static List<string> StartGame = new List<string> { "BQADAwADhAADdBexB3lX1nJOQOdEAg", "BQADBAADwg0AAu0XZAcUNBLuEjnBhgI" };
        internal static List<string> StartChaosGame = new List<string> { "BQADBAAD7wYAAgcYZAei_MiVQcRUIAI", "BQADBAAD_wcAAiUYZAeV5N-hkeMW0QI" };
        internal static List<string> TannerWin = new List<string> { "BQADBAADQwgAAuQaZAcWZKq6Zm4NJAI", "BQADBAAD_gMAAtgaZAeJOVus4yf3RAI" };
        internal static List<string> CultWins = new List<string> { "BQADBAADWAMAAosYZAf5H-sYZ53nMwI", "BQADBAADHwsAAgUYZAeDkYYi5N3cIgI" };
        internal static List<string> SerialKillerWins = new List<string> { "BQADBAADKwMAAsQZZAf4t254zcOVdgI", "BQADBAADOAQAAqUXZAdnNEO6TaxtnQI", "BQADBAADdQMAAsEcZAfkYMOxn9xzBAI", "BQADBAADmgMAArgcZAfW46sJoTg9VQI" };
        internal static List<string> LoversWin = new List<string> { "BQADBAAD8hUAAhYYZAeSI-kDTlm6QAI", "BQADBAADYAMAAkMdZAesBzPWN8zN3QI" };
#else
        public static List<string> VillagerDieImages = new List<string> { "CgADBAADKgMAAoMbZAcDMzzvBLtkDgI", "CgADBAADWAMAAt4cZAca-4d_WgIvAwI", "CgADAwADggADdBexB2dfz7NFUvhUAg" };
        public static List<string> WolfWin = new List<string> { "CgADAwADgAADdBexB3RS8Ngm9Qk-Ag", "CgADAwADgQADdBexB4nNsYbJdwQcAg" };
        public static List<string> WolvesWin = new List<string> { "CgADBAADlwMAAtgaZAcGis70fHk5_wI", "CgADBAADcAMAAn8ZZAe4INcCBiINwQI" };
        public static List<string> VillagersWin = new List<string> { "CgADAwADgwADdBexBzd2zefLWqdCAg" };
        public static List<string> NoWinner = new List<string> { "CgADBAADuAMAAlUXZAcr9cAAAfH9XIEC", "CgADBAAD8QgAAqIeZAfXa3s4HDi0VAI" };
        public static List<string> StartGame = new List<string> { "CgADBAADwg0AAu0XZAeJYaNtQl8otgI", "CgADAwADhAADdBexBzhVGklBymDqAg" };
        public static List<string> StartChaosGame = new List<string> { "CgADBAAD7wYAAgcYZAfZ1dhbwjxX5QI", "CgADBAAD_wcAAiUYZAfPZzx5cHb5RwI" };
        public static List<string> TannerWin = new List<string> { "CgADBAAD_gMAAtgaZAczTl8FGKfMbQI", "CgADBAADQwgAAuQaZAcQOHeUv7YwVgI" };
        public static List<string> CultWins = new List<string> { "CgADBAADWAMAAosYZAe-7g6vJNEKnwI", "CgADBAADHwsAAgUYZAf97T2hRLDVegI" };
        public static List<string> SerialKillerWins = new List<string> { "CgADBAADOAQAAqUXZAe78wwTRk8L4wI", "CgADBAADdQMAAsEcZAdHFMBLTBk-7gI", "CgADBAADmgMAArgcZAfvhgTxjBBWjAI", "CgADBAADKwMAAsQZZAfLl-elnd1eZwI" };
        public static List<string> LoversWin = new List<string> { "CgADBAADYAMAAkMdZAfJH7O_SbVC_gI", "CgADBAAD8hUAAhYYZAd3a0TjAwaT9AI" };
#endif

        public static int
#if DEBUG
            MinPlayers = 4,

#else
            MinPlayers = 4,
#endif
            MaxPlayers = 50,
            TimeDay = 30,
            TimeNight = 90,
            TimeLynch = 90,

            PlayerCountSeerCursed = 6,
            PlayerCountHarlot = 7,
            PlayerCountBeholderChance = 8,
            PlayerCountSecondWolf = 9,
            PlayerCountGunner = 9,
            PlayerCountTraitor = 10,
            PlayerCountGuardianAngel = 11,
            PlayerCountDetective = 12,
            PlayerCountApprenticeSeer = 13,
            PlayerCountCultist = 15,
            PlayerCountThirdWolf = 16,
            PlayerCountWildChild = 17,
            PlayerCountFoolChance = 18,
            PlayerCountMasons = 21,
            PlayerCountDoppelGanger = 22,
            PlayerCountCupid = 23,
            PlayerCountHunter = 24,
            PlayerCountSerialKiller = 25,
            PlayerCountSecondCultist = 26,
            MaxGames = 80,
            TannerChance = 40,
            FoolChance = 20,
            BeholderChance = 50,
            SeerConversionChance = 40,
            GuardianAngelConversionChance = 60,
            DetectiveConversionChance = 70,
            CursedConversionChance = 60,
            HarlotConversionChance = 70,
            HarlotDiscoverCultChance = 50,
            ChanceDetectiveCaught = 40,
            HunterConversionChance = 50,
            HunterKillCultChance = 50,
            HunterKillWolfChanceBase = 30,
            SerialKillerConversionChance = 20,
            AlphaWolfConversionChance = 20,
            SorcererConversionChance = 40,
            BlacksmithConversionChance = 75,
            OracleConversionChance = 50,
            SandmanConversionChance = 60,
            SheriffConversionChance = 60,
            HealerConversionChance = 40,
            PacifistConversionChance = 80,
            WiseElderConversionChance = 30,
            FirefighterConversionChance = 60,
            GhostConversionChance = 40,

            GameJoinTime = 1800,
            MaxJoinTime = 300;



#if DEBUG
        //public static long MainChatId = -134703013;
        public static long MainChatId = -1001049529775; //Beta group
#else
        public static long MainChatId = -1001030085238;
#endif
        public static long VeteranChatId = -1001094614730;
        public static string VeteranChatLink = "werewolfvets";

        public static bool RandomLynch = false;
    }
}
