using Godot;

using System;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Humanoid : Node
{
    [Export] public CharacterBody3D Character { get; set; }
    [Export] public Node3D CharacterModel { get; set; }
    
    [Export] public Marker3D HeadLookAtTarget { get; set; }

    private StateModel _stateModelNode;
    private Resources _characterResource;
    public Inventory Inventory;

    public override void _Ready()
    {
        _stateModelNode = GetNode<StateModel>("StateModel");
        _characterResource = GetNode<Resources>("Resource");
        Inventory = GetNode<Inventory>("Inventory");
    }

    public void MoveHeadLookAtTarget(Vector3 position)
    {
        HeadLookAtTarget.GlobalPosition = Character.Transform.Origin + position;
    }


}
