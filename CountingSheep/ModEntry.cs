using StardewModdingAPI;
using StardewValley;

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
    
    public override void Entry(IModHelper helper)
    {
        var saveData = new ModData();
        
        helper.Events.GameLoop.DayEnding += (sender, e) =>
        {
            saveData.LastBedtime = Game1.timeOfDay;
            saveData.AlarmClock = GetNaturalAwakeTime();
            helper.Data.WriteSaveData("CountingSheep", saveData);
        };

        helper.Events.GameLoop.DayStarted += (sender, e) =>
        {
            saveData = helper.Data.ReadSaveData<ModData>("CountingSheep") ?? saveData;
            var hoursSlept = CalculateTimeSlept(saveData.LastBedtime, saveData.AlarmClock);
            Game1.timeOfDay = Math.Max(600, saveData.AlarmClock);
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