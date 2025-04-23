using Microsoft.Xna.Framework;
using System;
using PlayingAround.Utils;

using System.Text.Json.Serialization;
using System.Collections.Generic;
using PlayingAround.Managers.CombatMan.CombatAttacks;

namespace PlayingAround.Entities.Monster.PlayMonsters
{
    public class PlayMonsterData
    {
        [JsonPropertyName("movementSpeed")] public float MovementSpeed { get; set; }
        [JsonPropertyName("speed")] public int Speed { get; set; }
        [JsonPropertyName("health")] public float Health {  get; set; }

        [JsonPropertyName("turnBehavior")] public string TurnBehavior { get; set; }

        [JsonIgnore]public Rectangle PacingBoundaryRect => PacingBoundary?.ToRectangle() ?? new Rectangle();
        [JsonPropertyName("pacingBoundary")] public MonsterRectangle PacingBoundary { get; set; }

        [JsonPropertyName("movementPattern")] public string MovementPattern { get; set; }
        [JsonPropertyName("iconPath")] public string IconPath { get; set; }
        [JsonPropertyName("difficulty")] public float Difficulty { get; set; }
        [JsonPropertyName("pauseDuration")] public float PauseDuration { get; set; }
        [JsonPropertyName("attacks")] public List<SingleAttack> Attacks { get; set; }
        [JsonPropertyName("immunities")] public List<string> Immunities { get; set; }
        [JsonPropertyName("resistances")] public List<string> Resistances {  get; set; }
        [JsonPropertyName("vulnerabilities")] public List<string> Vulnerabilities { get; set; }
        [JsonPropertyName("attackPower")] public int AttackPower { get; set; }
        [JsonPropertyName("chooseAttackBehavior")] public string ChooseAttackBehavior { get; set; }
    }
    public class MonsterRectangle
    {
        [JsonPropertyName("x")] public int X { get; set; }
        [JsonPropertyName("y")] public int Y { get; set; }
        [JsonPropertyName("width")] public int Width { get; set; }
        [JsonPropertyName("height")] public int Height { get; set; }

        // Helper to convert to a real Rectangle
        public Rectangle ToRectangle() => new Rectangle(X, Y, Width, Height);
    }

}
