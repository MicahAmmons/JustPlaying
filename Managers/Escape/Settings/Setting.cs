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

    public int? CurrentValue { get; set; }
    public int? DefaultValue { get; set; }
    public int? MaxValue { get; set; }
    public int? MinValue { get; set; }
    [JsonIgnore] public Rectangle RenderRect; // Used only for drawing
}

public enum SettingType
{
    Numerical,
    Toggle,
    Keybind
}
