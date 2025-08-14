public class HighlightStyleData
{
    public string color { get; set; } = "White";
    public float fade { get; set; } = 1f; // 0..1
}

public class TileCellHighlightData
{
    public HighlightStyleData invalidTarget { get; set; }
    public HighlightStyleData validTarget { get; set; }
    public HighlightStyleData walkable { get; set; }
    public HighlightStyleData playerStartable { get; set; }
    public HighlightStyleData monsterStartable { get; set; }
}
