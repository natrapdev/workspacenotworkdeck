using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public interface ILegState
{
    HumanoidModel Humanoid { get; set; }
    HumanoidStates StateContainer { get; set; }
    HumanoidLegStates Parent { get; set; }
    State CurrentState { get; set; }

    void Update(InputPackage input, float delta);
    void UpdateLegsState(InputPackage input, float delta);
    void ChangeState(State state);
}
