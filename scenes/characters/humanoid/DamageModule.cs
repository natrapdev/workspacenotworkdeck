using Godot;

namespace MyFirst3DGame.scenes.characters.humanoid;

public struct DamageModule(Node3D parent, string name, string material, float thickness, float bloodVolume, float currentBleedRate, float maxBleedRate, float volume)
{
    public Node3D Parent { get; set; } = parent;
    public string Name { get; set; } = name;
    public string Material { get; set; } = material;
    public float Thickness { get; set; } = thickness;
    public float BloodVolume { get; set; } = bloodVolume;
    public float MaxBloodVolume { get; set; } = bloodVolume;
    public float BleedRate { get; set; } = currentBleedRate;
    public float MaxBleedRate { get; set; } = maxBleedRate;
    public float Volume { get; set; } = volume;
}