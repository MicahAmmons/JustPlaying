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
        IconTextureString = $"MonsterIcons/{saveData.Name}Icon";
     //   CalculateLevel();
      //  CalculateStats();
       // CheckMilestones();
    }

    //public void CalculateLevel()
    //{
    //    float xp = _saveData.XP;
    //    float xpToLevel = _progressionData.XPForLevel1;
    //    int level = 0;
    //    while (xp > xpToLevel)
    //    {
    //        xp -= xpToLevel;
    //        xpToLevel *= _progressionData.XPMultiplier;
    //        level++;
    //    }
    //    Level = level;
    //    CurrentXP = (int)xp;
    //    XPNeededForNextLevel = (int)xpToLevel;
    //}
    //public void CalculateStats()
    //{
    //    MaxHealth = _saveData.AbilityPoints["Health"] * _progressionData.StatGainPerPoint["Health"] + _progressionData.BaseStats.Health;
    //   // Attack = _saveData.AbilityPoints["Attack"] * _progressionData.StatGainPerPoint["Attack"] + _progressionData.BaseStats.Attack;
    //    Defense = _saveData.AbilityPoints["Defense"] * _progressionData.StatGainPerPoint["Defense"] + _progressionData.BaseStats.Defense;
    //    SummonCost = 3;
    //}
}
