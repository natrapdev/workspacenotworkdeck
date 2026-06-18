using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Dead : State
{
    public override State ChangeState(InputPackage input) => this;

    protected override void OnEnter()
    {
        
        Ragdoll();
    }

    private void Ragdoll()
    {
        Skeleton.Ragdoll();
    }
}