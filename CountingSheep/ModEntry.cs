using StardewModdingAPI;
using StardewValley;

namespace CountingSheep;

public sealed class ModData
{
    public int LastBedtime { get; set; } = 2200;
}

internal sealed class ModEntry : Mod
{
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
            saveData = helper.Data.ReadSaveData<ModData>("LastBedtime") ?? new ModData { LastBedtime = 2200 };
        };
    }
}