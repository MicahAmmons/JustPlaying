using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Settings
{
    public List<Setting> AllSettings { get; set; } = new();
}

public class Setting
{
    public string Name { get; set; }

    public SettingType Type { get; set; }

    public int CurrentValue { get; set; } = -1;
    public int DefaultValue { get; set; } = -1;
    public int MaxValue { get; set; } = 2;
    public int MinValue { get; set; } = -2;
    [JsonIgnore] public Rectangle RenderRect; 
    [JsonIgnore] public Rectangle UpArrowRect;
    [JsonIgnore] public Rectangle DownArrowRect;
}

public enum SettingType
{
    Numerical,
    Toggle,
    Keybind
}
