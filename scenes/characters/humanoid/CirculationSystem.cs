using Godot;

namespace MyFirst3DGame.scenes.characters.humanoid;

public partial class CirculationSystem : Node
{
    [Export] public DamageModel Model { get; set; }
    [Export] public HumanoidLimbs LimbsParent { get; set; }
    public const float BLOOD_PER_BODY_MASS_KILOGRAM = 75f;
    public float TotalBloodVolume { get { return Model.BodyMass * BLOOD_PER_BODY_MASS_KILOGRAM; } }
    public float CurrentBloodVolume { get; set; }

    public void Update(float delta)
    {
        LoseBloodIfPossible(delta);
    }

    public void LoseBloodIfPossible(float delta)
    {
        foreach (Limb limb in LimbsParent.Limbs.Values)
        {
            LoseBloodInLimb(limb, limb.CurrentBleedRate * delta);

            // Neighbouring limbs will lose blood as well
            foreach (Limb neighbourLimb in limb.Neighbours)
            {
                LoseBloodInLimb(neighbourLimb, limb.CurrentBleedRate / 2 * delta);
            }
        }
    }

    public void LoseBloodInLimb(Limb limb, float bloodLost)
    {
        limb.CurrentBloodVolume -= bloodLost;
        CurrentBloodVolume -= bloodLost;
    }
}