using StardewModdingAPI;
using StardewValley;
using HarmonyLib;
using StardewValley.Menus;

namespace CountingSheep;

public sealed class ModData
{
    public int LastBedtime { get; set; } = 2200;
    public int AlarmClock { get; set; } = 600;
}

internal sealed class ModEntry : Mod
{
    private static int CalculateTimeSlept(int bedTime, int awakeTime)
    {
        var diff = 2400 - ((bedTime / 100) * 100);
        return awakeTime + diff;
    }

    private static int GetNaturalAwakeTime()
    {
        return Game1.timeOfDay - 1600;
    }

    private static bool DoSleepPrefix()
    {
        if (Game1.timeOfDay >= 2000) return true;
        
        if (Game1.IsMultiplayer)
        {
            return true;
        }
        Game1.activeClickableMenu = new DialogueBox("It's too early to go to bed!");
        return false;
    }
    
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(ModManifest.UniqueID);
        var saveData = new ModData();

        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), "startSleep"),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(DoSleepPrefix))
        );
        
        helper.Events.GameLoop.DayEnding += (sender, e) =>
        {
            saveData.LastBedtime = Game1.timeOfDay;
            saveData.AlarmClock = GetNaturalAwakeTime();
            helper.Data.WriteSaveData("CountingSheep", saveData);
            if (Game1.IsMultiplayer) return;
        };

        helper.Events.GameLoop.DayStarted += (sender, e) =>
        {
            saveData = helper.Data.ReadSaveData<ModData>("CountingSheep") ?? saveData;
            var hoursSlept = CalculateTimeSlept(saveData.LastBedtime, saveData.AlarmClock);
            Game1.timeOfDay = Math.Max(500, saveData.AlarmClock);
            if (Game1.IsMultiplayer) return;
            switch (hoursSlept)
            {
                case < 600:
                    Monitor.Log("You're really tired", LogLevel.Info);
                    Game1.player.stamina *= 0.5f;
                    break;
                case < 800:
                    Monitor.Log("You're tired", LogLevel.Info);
                    Game1.player.stamina *= 0.75f;
                    break;
            }
        };
    }
}