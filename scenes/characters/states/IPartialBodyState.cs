using Godot;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.states;

public interface IPartialBodyState
{
    [Export] LegState LegBehaviour { get; set; }
    
    void LegsTrackLookDirection(InputPackage input, float delta) => LegBehaviour.CurrentState.TrackLookDirection(input, delta);
    void LegsUpdate(InputPackage input, float delta) => LegBehaviour.Update(input, delta);
}