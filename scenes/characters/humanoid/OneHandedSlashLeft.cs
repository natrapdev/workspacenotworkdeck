using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public partial class OneHandedSlashLeft : State, IPartialBodyState
{
    [Export] public LegState LegBehaviour { get; set; }
    protected override void OnUpdate(InputPackage input, float delta)
    {
        
    }
}
