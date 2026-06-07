using Godot;
using System;
using System.Linq;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class CirculationSystem : Node
{
    [Export] public DamageModel Model { get; set; }
    
    private const float BloodPerBodyMassKilogram = 75f;
    public float TotalBloodVolume { get { return Model.BodyMass * BloodPerBodyMassKilogram; } }
    public float CurrentBloodVolume { get; set; }

    public void Update(float delta)
    {
        LoseBloodIfPossible(delta);
    }

    private void LoseBloodIfPossible(float delta)
    {
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
        CurrentBloodVolume -= Mathf.Clamp(CurrentBloodVolume - bloodLost, 0, TotalBloodVolume);
        
        var label = limb.GetNodeOrNull<Label3D>("Label");
    
        if (label is not null)
        {
            label.Text = $"{limb.RemainingBloodRatio * 100:F2}%";
        }
    }
}