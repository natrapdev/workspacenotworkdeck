using Godot;
using MyFirst3DGame.scenes.characters.states;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class DamageHandler : Node
{
    private HumanoidModel _humanoid;
    private DamageModel _damageModel;

    public override void _Ready()
    {
        _damageModel = GetParent<DamageModel>();
        _humanoid = _damageModel.Humanoid;
    }

    public readonly List<InjurySeverity> Severities =
    [
        new("negligible", 1),
        new("minimal", 0.8f),
        new("moderate", 0.6f, 1.1f),
        new("serious", 0.4f, 1.3f),
        new("critical", 0.2f, 1.5f)
    ];

    public (bool dead, string reason) DieIfMatchesDeathCondition()
    {
        return (false, "Not dead");
    }

    public string GetSeverityName(float remainingBloodRatio)
    {
        int index = GetSeverityIndex(remainingBloodRatio);
        return Severities[index].Name;
    }

    public float GetSeverityMultiplier(float remainingBloodRatio)
    {
        int index = GetSeverityIndex(remainingBloodRatio);
        return Severities[index].Multiplier;
    }
    
    private int GetSeverityIndex(float bloodRatio)
    {
        return Severities.FindLastIndex(x => x.Treshold >= bloodRatio);
    }
    
}
