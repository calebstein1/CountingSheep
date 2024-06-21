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
    private static ModData _saveData = new();
    
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
        if (Game1.IsMultiplayer)
        {
            return true;
        }
        if (Game1.timeOfDay >= 2000)
        {
            Game1.activeClickableMenu = new NumberSelectionMenu(
                message: "When would you like to wake up?",
                behaviorOnSelection: (value, price, who) =>
                {
                    _saveData.AlarmClock = value;
                    Game1.exitActiveMenu();
                },
                minValue: 5,
                maxValue: 10,
                defaultNumber: Math.Max(5, GetNaturalAwakeTime() / 100)
            );

            // TODO: figure out how to block return until dialog is dismissed
            return true;
        }
        Game1.activeClickableMenu = new DialogueBox("It's too early to go to bed!");
        return false;
    }

    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(ModManifest.UniqueID);

        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), "startSleep"),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(DoSleepPrefix))
        );
        
        helper.Events.GameLoop.DayEnding += (sender, e) =>
        {
            if (Game1.IsMultiplayer) return;
            _saveData.LastBedtime = Game1.timeOfDay;
            helper.Data.WriteSaveData("CountingSheep", _saveData);
        };

        helper.Events.GameLoop.DayStarted += (sender, e) =>
        {
            if (Game1.IsMultiplayer) return;
            _saveData = helper.Data.ReadSaveData<ModData>("CountingSheep") ?? _saveData;
            var hoursSlept = CalculateTimeSlept(_saveData.LastBedtime, _saveData.AlarmClock);
            Game1.timeOfDay = Math.Max(500, _saveData.AlarmClock);
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