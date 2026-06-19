using Godot;
using System;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class CirculationSystem : Node
{
    [ExportGroup("Connections")]
    [Export] public DamageModel Model { get; set; }
    [Export] public DamageHandler DamageHandler { get; set; }
    [ExportGroup("Values")]
    [Export] public float FatalBloodLevel { get; set; } = 0.5f;

    private const float BloodPerBodyMassKilogram = 75f;
    public float TotalBloodVolume { get { return Model.BodyMass * BloodPerBodyMassKilogram; } }
    public float CurrentBloodVolume { get; set; }
    public float RemainingBloodRatio { get { return CurrentBloodVolume / TotalBloodVolume; } }

    private float _bloodLossRate;

    public override void _Ready()
    {
        CurrentBloodVolume = TotalBloodVolume;
    } 
    
    public void Update(float delta)
    {
        LoseBloodIfPossible(delta);
        DieIfBloodTooLow();
    }

    private void LoseBloodIfPossible(float delta)
    {
        _bloodLossRate = 0;  
        foreach (Limb limb in Model.LimbCollection.Limbs.Values.Where(limb => limb.CurrentBloodVolume > 0))
        {
            LoseBloodInLimb(limb, limb.CurrentBleedRate * delta);
        
            // Neighbouring limbs will lose blood as well
            foreach (Limb neighbourLimb in limb.Neighbours)
            {
                LoseBloodInLimb(neighbourLimb, limb.CurrentBleedRate / 2 * delta);
            }
        }
    }

    private void LoseBloodInLimb(Limb limb, float bloodLost)
    {
        limb.CurrentBloodVolume = Mathf.Clamp(limb.CurrentBloodVolume - bloodLost, 0, limb.MaxBloodVolume);
        CurrentBloodVolume = Mathf.Clamp(CurrentBloodVolume - bloodLost, 0, TotalBloodVolume);
        
        _bloodLossRate += bloodLost;

        if (limb.CurrentBloodVolume <= 0)
        {
            var area3D = limb.GetNodeOrNull<Area3D>("Area3D");
            area3D?.QueueFree();
        }
        
        var label = limb.GetNodeOrNull<Label3D>("Label");
        if (label is not null) label.Text = $"{limb.RemainingBloodRatio * 100:F2}%";
    }

    private void CheckOverallWellness()
    {
        float remainingBlood = CurrentBloodVolume / TotalBloodVolume;
        string severity = DamageHandler.GetSeverityName(remainingBlood);
    }

    private void CheckLimbWellness(Limb limb)
    {
        float remainingBlood = limb.CurrentBloodVolume / limb.MaxBloodVolume;
        string severity = DamageHandler.GetSeverityName(remainingBlood);
    }

    private void DieIfBloodTooLow()
    {
        if (RemainingBloodRatio > FatalBloodLevel) return;
        HumanoidModel humanoid = Model.Humanoid;
        humanoid.SwitchTo("dead");
    }
}