using Godot;

using System;
using System.Dynamic;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Humanoid : Node
{
    [Export] public CharacterBody3D Character { get; private set; }
    [Export] public Node3D CharacterModel { get; private set; }
    [Export] public Marker3D HeadLookAtTarget { get; private set; }
    [Export] public AnimationTree AnimationTree { get; private set; }

    public Resources CharacterResource { get; private set; }
    public Inventory Inventory { get; private set; }
    public WeaponInventory WeaponInventory { get; set; }
    public Skeleton3D Skeleton { get; private set; }
    public StateModel StateModel { get; private set; }
    public bool CanMove { get; set; } = true;


    public override void _Ready()
    {
        StateModel = GetNode<StateModel>("StateModel");
        CharacterResource = GetNode<Resources>("Resource");
        Inventory = GetNode<Inventory>("Inventory");
        WeaponInventory = Inventory.GetNode<WeaponInventory>("WeaponInventory");
        Skeleton = CharacterModel.GetNode<Skeleton3D>("rig/Skeleton3D");
    }

    public void MoveHeadLookAtTarget(Vector3 position)
    {
        HeadLookAtTarget.GlobalPosition = position;
    }
}
