using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedSlashRight : State, IChildState, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }
    [Export] public State BaseState { get; set; }


    protected override void OnUpdate(InputPackage input, float delta)
    {
        
    }
}
