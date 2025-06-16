using Microsoft.Xna.Framework.Graphics;
using PlayingAround.Data.SaveData;
using PlayingAround.Entities.Summons;
using PlayingAround.Managers.Assets;

public class SummonedMonster
{
    public string Name => _saveData.Name;
    public string IconTextureString { get; private set; }
    public int MaxHealth { get; private set; }
    public int Defense { get; private set; }
    public int Level { get; private set; }
    public int CurrentXP {get ; private set; }
    //public int TotalXP => _saveData.XP;
    public int XPNeededForNextLevel { get; private set; }
    public bool IsReadyToLevelUp => CurrentXP >= XPNeededForNextLevel;
    public float XPProgressPercent => (float)CurrentXP / XPNeededForNextLevel;
    public int NumberOfKills { get; private set; }
    public int SummonCost {  get; private set; }


    // Internals
    private SummonsSaveData _saveData;
    private SummonProgressionData _progressionData;

    public SummonedMonster(SummonsSaveData saveData, SummonProgressionData progressionData)
    {
        _saveData = saveData;
        _progressionData = progressionData;
        IconTextureString = $"{saveData.Name}Icon";
        NumberOfKills = saveData.NumberOfKills;
    }
}
