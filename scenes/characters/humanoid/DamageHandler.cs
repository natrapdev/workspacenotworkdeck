using Godot;
using MyFirst3DGame.scenes.characters.states;
using System;

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

    
}
