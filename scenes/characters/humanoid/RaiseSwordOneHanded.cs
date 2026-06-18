using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class RaiseSwordOneHanded : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }

    public override State ChangeState(InputPackage input)
    {
        return !Character.IsOnFloor() ? Parent.GetStateByName("airborne") : base.ChangeState(input);
    }

    protected override void OnUpdate(InputPackage input, float delta)
    {
        
    }

    protected override void OnEnter()
    {
        Humanoid.Animator.SetBodySpeedScale(1f);
        // NextState??=Parent.GetStateByName("slash1_one_handed");
        if (!FollowUpStates.Contains(Parent.GetStateByName("slash1_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("slash1_one_handed"));
        }
        if (!FollowUpStates.Contains(Parent.GetStateByName("slash3_one_handed")))
        {
            FollowUpStates.Add(Parent.GetStateByName("slash3_one_handed"));
        }
    }
}
