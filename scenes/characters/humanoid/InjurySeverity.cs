namespace MyFirst3DGame.scenes.characters.humanoid;

public readonly struct InjurySeverity(string name, float threshold, float multiplier = 1f)
{
    public string Name { get; } = name;
    public float Treshold { get; } = threshold;
    public float Multiplier { get; } = multiplier;
}