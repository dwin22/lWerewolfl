using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public enum Achievements
    {
        [Display(Name = "None"), Description("You haven't played a game yet!")]
        None = 0,
        [Display(Name = "¡Bienvenido al infierno!"), Description("Juega una partida")]
        WelcomeToHell = 1,
        [Display(Name = "¡Bienvenido al manicomio!"), Description("Juega una partida caótica")]
        WelcomeToAsylum = 2,
        [Display(Name = "Paciente de Alzheimer"), Description("Juega una partida amnesia")]
        AlzheimerPatient = 3,
        [Display(Name = "O HAI DER!"), Description("Play a game with Para's secret account (not @para949)")]
        OHAIDER = 4,
        [Display(Name = "Espía vs espía"), Description("Juega una partida en modo secreto (sin revelar roles)")]
        SpyVsSpy = 5,
        [Display(Name = "Explorador"), Description("Juega al menos 2 partidas en 10 grupos distintos")]
        Explorer = 6,
        [Display(Name = "Linguista"), Description("Juega al menos 2 partidas con 10 idiomas distintos")]
        Linguist = 7,
        [Display(Name = "No tengo ni idea de qué hago"), Description("Juega una partida en amnesia secreta")]
        NoIdeaWhat = 8,
        [Display(Name = "Enoclofobia"), Description("Juega una partida de 50 jugadores")]
        Enochlophobia = 9,
        [Display(Name = "Introvertido"), Description("Juega una partida de 4 jugadores")]
        Introvert = 10,
        [Display(Name = "Travieso"), Description("Juega una partida usando un idioma NSFW")]
        Naughty = 11,
        [Display(Name = "Dedicado"), Description("Juega 100 partidas")]
        Dedicated = 12,
        [Display(Name = "Obsesionado"), Description("Juega 1000 partidas")]
        Obsessed = 13,
        [Display(Name = "Here's Johnny!"), Description("Mata a 50 personas como asesina en serie (en total)")]
        HereJohnny = 14,
        [Display(Name = "¡Yo te cubro!"), Description("Salva a 50 personas como ángel de la guarda (en total)")]
        GotYourBack = 15,
        [Display(Name = "Masoquista"), Description("Gana una partida como el veterano")]
        Masochist = 16,
        [Display(Name = "Hip hip"), Description("Sobrevive una partida de 10 o más jugadores como el borracho")]
        Wobble = 17,
        [Display(Name = "Discreto"), Description("En una partida de 20 o más jugadores, logra sobrevivir sin ser votado para linchar ni una sola vez")]
        Inconspicuous = 18,
        [Display(Name = "Superviviente"), Description("Sobrevive 100 partidas")]
        Survivalist = 19,
        [Display(Name = "Oveja Negra"), Description("Sé linchado el primero 3 partidas seguidas")]
        BlackSheep = 20,
        [Display(Name = "Promiscuo"), Description("Como la ramera, sobrevive las 5 primeras noches sin quedarte en casa ni visitar personas repetidas")]
        Promiscuous = 21,
        [Display(Name = "Hermanos Masones"), Description("Sé uno de los dos (o más) masones supervivientes al final de la partida")]
        MasonBrother = 22,
        [Display(Name = "Double Shifter"), Description("Cambia de roles dos veces en una partida (culteo no cuenta)")]
        DoubleShifter = 23,
        [Display(Name = "Hey Man, Nice Shot"), Description("Como cazador, usa tu disparo final para matar a un lobo, asesina o pirómano")]
        HeyManNiceShot = 24,
        [Display(Name = "Y por eso no debes quedarte en casa"), Description("Como lobo o cultista, mata o convierte a la ramera que se quedó en casa")]
        DontStayHome = 25,
        [Display(Name = "Visión doble"), Description("Sé una de las dos videntes vivas al mismo tiempo")]
        DoubleVision = 26,
        [Display(Name = "Muerte doble"), Description("Forma parte del final de asesina y cazador")]
        DoubleKill = 27,
        [Display(Name = "Debí saberlo..."), Description("Como vidente, percibe a la observadora")]
        ShouldHaveKnown = 28,
        [Display(Name = "Percibo desconfianza"), Description("Como vidente, sé linchada el primer día")]
        LackOfTrust = 29,
        [Display(Name = "Domingo sangriento"), Description("Sé una de las 4 víctimas que murieron en una misma noche")]
        BloodyNight = 30,
        [Display(Name = "¡Cambiar de bando funciona!"), Description("Cambia de rol en la partida y gana")]
        ChangingSides = 31,
        [Display(Name = "Amor Prohibido"), Description("Gana como pareja de lobo + simple aldeano")]
        ForbiddenLove = 32,
        [Display(Name = "Desarrollador"), Description("Colabora en el desarrollo del bot en github")]
        Developer = 33,
        [Display(Name = "Impaciente"), Description("Sé el primero en linchar 5 veces en una partida")]
        FirstStone = 34,
        [Display(Name = "Buen Justi"), Description("Como justiciero, mata a dos malos con tus disparos")]
        SmartGunner = 35,
        [Display(Name = "Astuto"), Description("Como detective, logra ver a 4 malos seguidos distintos")]
        Streetwise = 36,
        [Display(Name = "Speed Dating"), Description("Sé seleccionado como amante por el bot (el cupido se quedó sin tiempo)")]
        OnlineDating = 37,
        [Display(Name = "Reloj Averiado"), Description("Como necio, logra percibir bien a dos personas en una partida")]
        BrokenClock = 38,
        [Display(Name = "¡Tan cerca!"), Description("Como veterano, sé una de las personas más linchadas en un empate")]
        SoClose = 39,
        [Display(Name = "Convención Cultista"), Description("Sé uno de 10 cultistas supervivientes al final de la partida")]
        CultCon = 40,
        [Display(Name = "Amor Propio"), Description("Como Cupido, elígete a ti mismo como uno de los amantes")]
        SelfLoving = 41,
        [Display(Name = "Debiste decir algo"), Description("Como lobo, tu manada mata a tu amante (no cuenta la primera noche)")]
        ShouldveMentioned = 42,
        [Display(Name = "Tanner Overkill"), Description("Como veterano, haz que todos los vivos te linchen")]
        TannerOverkill = 43,
        [Display(Name = "Serial Samaritan"), Description("Como asesina en serie, mata 3 lobos en una partida")]
        SerialSamaritan = 44,
        [Display(Name = "Sacrificio"), Description("Sé el cultista encargado de convertir a la cazadora de cultistas")]
        CultFodder = 45,
        [Display(Name = "Lobo Solitario"), Description("En una partida de 10 o más jugadores, sé el único lobo y gana")]
        LoneWolf = 46,
        [Display(Name = "Cazadores en Manada"), Description("Sé uno de los 7 lobos vivos")]
        PackHunter = 47,
        [Display(Name = "Saved by the Bull(et)"), Description("Sé parte de la aldea en una partida que no acaba gracias a que al justiciero le quedan balas")]
        GunnerSaves = 48,
        [Display(Name = "Leal"), Description("Sobrevive 1 hora en una partida")]
        LongHaul = 49,
        [Display(Name = "OH SHI-"), Description("Mata a tu enamorado la primera noche")]
        OhShi = 50,
        [Display(Name = "Veterano"), Description("Juega 500 partidas")]
        Veteran = 51,
        [Display(Name = "¡Brujas a la hoguera!"), Description("Como lobo, mata a la hechicera")]
        NoSorcery = 52,
        [Display(Name = "Rastreador de Cultistas"), Description("Como cazadora de cultistas, caza a 3 cultistas en una misma partida")]
        CultistTracker = 53,
        [Display(Name = "Torpe líder"), Description("Como torpe, acierta 3 linchamientos en una partida")]
        ImNotDrunk = 54,
        [Display(Name = "Alfeador"), Description("Como lobo alfa, logra convertir con éxito a 3 jugadores en lobos (en la misma partida)")]
        WuffieCult = 55,
        [Display(Name = "¿Te protegiste a ti mismo?"), Description("Como ángel de la guarda, sobrevive 3 veces en una partida tras intentar proteger a un lobo")]
        DidYouGuardYourself = 56,
        [Display(Name = "República"), Description("Como princesa, sé linchada por segunda vez")]
        SpoiledRichBrat = 57,
        [Display(Name = "Los Tres Lobitos"), Description("Como hechicera, acaba la partida habiendo 3 o más lobos vivos")]
        ThreeLittleWolves = 58,
        [Display(Name = "Presidente"), Description("Como alcalde, ten 3 linchamientos exitosos tras revelarte")]
        President = 59,
        [Display(Name = "¡He ayudado! :D"), Description("Como lobezno, haz que la manada coma doble tras tu muerte")]
        IHelped = 60,
        [Display(Name = "¡Menuda Noche!"), Description("Sé visitado por 3 o más roles distintos en una misma noche")]
        ItWasABusyNight = 61,
        [Display(Name = "Vetelobo"), Description("Como lobo, gana la partida justo tras ser linchado")]
        TannerWolf = 62,
    } // MAX VALUE: 9223372036854775807
      //            

    public static partial class Extensions
    {
        public static string GetDescription(this Achievements value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
        public static string GetName(this Achievements value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
        }

        public static byte[] ToByteArray(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }


        public static IEnumerable<Achievements> GetUniqueFlags(this BitArray flags)
        {
            for (var i = 0; i < flags.Length; i++)
            {
                if (flags.Get(i))
                {
                    yield return (Achievements)i;
                }
            }
        }

        public static bool HasFlag(this BitArray array, Achievements achv)
        {
            return array[(int)achv];
        }

        public static BitArray Set(this BitArray array, Achievements achv)
        {
            array[(int)achv] = true;
            return array;
        }

        public static BitArray Unset(this BitArray array, Achievements achv)
        {
            array[(int)achv] = false;
            return array;
        }
    }
}
