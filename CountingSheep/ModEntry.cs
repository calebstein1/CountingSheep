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
            helper.Data.WriteSaveData("LastBedtime", saveData);
        };

        helper.Events.GameLoop.DayStarted += (sender, e) =>
        {
            saveData = helper.Data.ReadSaveData<ModData>("LastBedtime") ?? new ModData();
            Monitor.Log($"YOu slept for {CalculateTimeSlept(saveData.LastBedtime, 600)} hours", LogLevel.Debug);
        };
    }
}