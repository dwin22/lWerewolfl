# Werewolf for Telegram

This is a fork of the Werewolf Telegram bot. It adds new gamemodes, roles and functionality to the original one. All credits go to the original bot, so check them out too. If you want to donate, please donate to them. Without their work, this version of the bot wouldn't have been possible.

NOTE: Most new strings are only in Spanish. But you can translate them to your language easily. You can copy the new strings from the Spanish file in the language folder, paste them to the language you want to use and edit them. If you want me to add them to this github repo, just send me the language file in telegram: t.me/TheEdenProject

New gamemodes:
- Superchaos: almost every role can appear multiple times. If you are looking for the craziest games, this is your gamemode.
- Startclumsy: troll gamemode where normal villagers (and some other low weight roles) are replaced by clumsies.

New roles:
- Pyro: new killer role. Wins alone, and quietly douses other players to burn them all at once by visiting themself. Unlike the serial killer, a dead pyro can still win if another pyro wins the game, since their dream of burning the whole village is still completed.
- Baker: village role that needs to hide. If it dies, no matter how, the villagers won't be able to lynch for one day. This role adds a unique concept of a villager role that needs to hide from killers (like seer) but doesn't have any use or benefit to the village (like clumsy).
- Healer: village role than can revive a dead player once in the whole game. It can only revive village team roles, to avoid trolls.
- Police: village role than can appear multiple times (like villager or mason) and can check one player each night, to know whether they are suspicious or not. To compensate for the fact that there can be multiple polices, they all have a 20% chance to get the wrong answer each night. Cooperation between polices is key, but they don't know each other like masons do, so fake police claims are to be expected.
- Atheist: village role that can't be converted to the cult. Can only appear in 11+ player games, or in chaos games.
- Sheriff: village role that can visit one player each night to know whether they leave their house or not. It will also protect them from cult conversion. They can still be converted by cult, though, even when visiting.
- Imposter: wolf ally role (like Sorcerer) that will choose a player to imitate for the whole game. Unlike the doppelganger, it won't be able to do the actions of the copied role, but it will show up as that role to the village when dead, or to the seer. It will also know the copied role as soon as they choose their role-model, and it doesn't need them to die.
- Hungry Wolf: a wolf role that has a chance to hide the role of their victim to the rest of the village.
- Rabid Wolf: a wolf role that can only appear with Alpha Wolf. While the Alpha Wolf is alive, it will keep searching for a new Rabid Wolf every time it dies (100% chance to infect until the pack has a new Rabid Wolf or the Alpha Wolf dies). This means that the village should try to avoid lynching a Rabid Wolf before lynching the Alpha.
- Survivor: this role is the anti-tanner. It wins as long as it survives, no matter which team is the winner. It can even win with the serial killer, giving it a deserved buff.

New features:
- Game start time won't start decreasing until there are enough players for a game.
- Games can have a minimum of 4 players now!
- Games can have a maximum of 50 players in them now.
- Cult (and cultist hunter) can now appear in chaos without the 11 player requirement, to make it truly chaos.
- You can see the original role of each player at the end (and only at the end) of the game, next to their new role (in case it changed).

Balance changes:
- Sorcerer can see exact roles. No more being useless because you found Seer but can't find GA. And no more being useless because you saw Lycan as villager. Sorcerer is now a fun role.
- Seer can see the role as soon as they click the button. This is made to prevent those cases where seer would be revived by healer and would die the next night being completely useless.
- Sorcerer and Fool will also see the role immediately, for consistency.
- Role weights of most roles were changed, to make games more balanced. Examples of big changes are Tanner (from players/2 to 3 - no more big games with lots of villagers and almost no evils) or Traitor (from 0 to -4).
- Some wolf-team roles, or roles that can turn into werewolf, can't appear in small games anymore, to avoid unfair games.
- Cursed, Harlot and Baker are disabled in 4 player games, to avoid games that end the first night.
- There can be a maximum of 1 wolf for each 4 players in a game (used to be 1 for each 5 players before).
- Some games won't end when wolves outnumber villagers, if certain village roles that can win the game are alive (For example, Mayor+GA or Healer).

And lots of more new features coming soon! (I'm working on new achievements atm)

### Visual Studio Team Services Continuous Integration		
![build status](https://parabola949.visualstudio.com/_apis/public/build/definitions/c0505bb4-b972-452b-88be-acdc00501797/2/badge)

## Requirements
* .NET Framework 4.5.2
* SQL Server (I am using 2017)
* Any program to manage databases (I am using Microsoft SQL Server Management Studio)
* Windows Server

## Setup

To set up werewolf on a private server, follow these steps:

1. Go to [BotFather](https://telegram.me/BotFather) and create a new bot.  Answer all of the questions it asks, and you will receive an API Token.
   * On your server, open regedit, and go to `HKLM\SOFTWARE\`, create a new Key named `Werewolf` (HKLM - HKEY_LOCAL_MACHINE)
   * In the new key create a new string value named `ProductionAPI`.  
   * Paste your API token here.
2. Grab the Werewolf Database.sql file from this repository
   * Open the file in notepad, notepad++, whatever you use
   * Double check the path at the top of the file - update it if you are using a different SQL version
   * Run the sql script.  This will create the `werewolf` database and all the tables / views / stored procs to go with it
   * If you already have some admins (including yourself), add their TelegramID's to the `dbo.Admin` table
		* In order to obtain your ID, headover to your bot in telegram and /Start. After that, toss a random text to it. Enter this URL to your browser (https://api.telegram.org/botYOURTELEGRAMBOTAPIKEY/getUpdates)
3. Now it's time to compile the source code
   * On your server, open regedit
   * In the `Werewolf` key create a new string value named `BotConnectionString`.
   * Paste the Connection String here.
        * Connection String should be this (change the values) `metadata=res://*/WerewolfModel.csdl|res://*/WerewolfModel.ssdl|res://*/WerewolfModel.msl;provider=System.Data.SqlClient;provider connection string="data source=SERVERADDRESS;initial catalog=werewolf;user id=USERNAME;password=PASSWORD;MultipleActiveResultSets=True;App=EntityFramework"`
			* If you are using Windows Authentication for your MSSQL Server, do take note that the password property will NO Longer be required. You're required to replace it(both user id and password) with "Trusted_Connection=True;" instead.
      * .gitignore has marked this file, so it won't be committed. **However, when you create the setting, VS will copy it to the app.config - make sure to remove it if you plan on committing back to your fork**
   * In Visual Studio, open the solution.  Make sure you are set to `RELEASE` build.  You may want to go into `Werewolf_Control.Handlers.UpdateHandler.cs` and change `internal static int Para = 129046388;` to match your id.  Also, double check the settings.cs files in both Control and Node.
   * Build the solution
4. Server directories
   * Pick any directory for your root directory

   | Directory | Contents |
   |-----------|---------:|
   |`root\Instance Name\Control`|Control build|
   |`root\Instance Name\Node 1`|Node build|
   |`root\Instance Name\Node <#>`|Node updates can be added to a new Node folder.  Running `/replacenodes` in Telegram will tell the bot to automatically find the newest node (by build time) and run it|
   |`root\Instance Name\Logs`|Logging directory|
   |`root\Languages`|Language xml files - These files are shared by all instances of Werewolf|

   * Note - Once all nodes are running the newest version (Node 2 directory), the next time you update nodes, you can put the new files in Node 1 and `/replacenodes`.  Again, the bot will always take whichever node it finds that is the newest, as long as the directory has `Node` in the name.  **do not name any other directory in the root folder anything with `Node` in it**
5. Fire up the bot!




## GIF SUPPORT
In order to use GIFs with the bot, you will need to "teach" the bot the new GIF IDs.  From Telegram, run `/learngif`, the bot will respond with `GIF learning = true`.  Now send it a GIF, and the bot will reply with an ID.  Send the bot all the GIFs you need.  In the Node project, go to Helpers > Settings.cs and find the GIF lists.  You'll need to remove all of the existing IDs and put in the IDs you just got from the bot.

You can test these by running `/dumpgifs` (preferrably in Private Message!).  Make sure you check out DevCommands.cs, and look at the `DumpGifs()` method - most of it is commented out.  Uncomment what you need.
