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
    
    public override void Entry(IModHelper helper)
    {
        var saveData = new ModData();
        
        helper.Events.GameLoop.DayEnding += (sender, e) =>
        {
            saveData.LastBedtime = Game1.timeOfDay;
            helper.Data.WriteSaveData("CountingSheep", saveData);
        };

        helper.Events.GameLoop.DayStarted += (sender, e) =>
        {
            saveData = helper.Data.ReadSaveData<ModData>("CountingSheep") ?? saveData;
            var hoursSlept = CalculateTimeSlept(saveData.LastBedtime, saveData.AlarmClock);
            Monitor.Log($"You slept for {hoursSlept} hours", LogLevel.Debug);
            Game1.timeOfDay = saveData.AlarmClock;
            switch (hoursSlept)
            {
                case < 6:
                    // TODO: lower stamina and sluggish
                    break;
                case < 8:
                    // TODO: lower stamina, not sluggish
                    break;
            }
        };
    }
}