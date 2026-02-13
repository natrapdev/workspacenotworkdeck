using Godot;
using MyFirst3DGame.Items;

using System;
using System.Dynamic;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Humanoid : Node
{
    // [Export] public CharacterBody3D Character { get; private set; }
    // [Export] public Node3D CharacterModel { get; private set; }
    // [Export] public Marker3D HeadLookAtTarget { get; private set; }
    // [Export] public AnimationTree AnimationTree { get; private set; }

    // public HumanoidResource CharacterResource { get; private set; }
    // public Inventory Inventory { get; private set; }
    // public WeaponInventory WeaponInventory { get; set; }
    // public Skeleton3D Skeleton { get; private set; }
    // public StateModel StateModel { get; private set; }
    // public HumanoidCombat Combat { get; private set; }
    // public bool CanMove { get; set; } = true;

    // public BoneAttachment3D HeadBoneAttachment { get; private set; }

    // public Weapon ActiveWeapon { get; set; }

    // private const float _BodyRotationSpeed = 30f;

    // public override void _Ready()
    // {
    //     StateModel = GetNode<StateModel>("StateModel");
    //     CharacterResource = GetNode<HumanoidResource>("Resource");
    //     Inventory = GetNode<Inventory>("Inventory");
    //     WeaponInventory = Inventory.GetNode<WeaponInventory>("WeaponInventory");
    //     Skeleton = CharacterModel.GetNode<Skeleton3D>("rig/Skeleton3D");
    //     Combat = GetNode<HumanoidCombat>("Combat");
    //     HeadBoneAttachment = CharacterResource.HeadBoneAttachment;
    // }

    // public void MoveHeadLookAtTarget(Vector3 position)
    // {
    //     HeadLookAtTarget.GlobalPosition = position;
    // }

    // public void _Update(InputPackage input, float delta)
    // {
    //     Vector3 direction = CharacterResource.HeadBoneGlobalTransform.Basis * new Vector3(-input.direction.X,0,-input.direction.Y).Normalized();
    //     Vector3 characterForward = CharacterModel.Basis.Z;
    //     float angle = characterForward.SignedAngleTo(direction, Vector3.Up);
    //     CharacterModel.RotateY(Mathf.Clamp(angle, -_BodyRotationSpeed * delta, _BodyRotationSpeed * delta));
    // }

    // public void Update(InputPackage input, float delta)
    // {
    //     var currentState = StateModel.States[StateModel.CurrentStateName];

    //     input = Combat.Contextualize(input);
    //     var relevance = currentState.CheckRelevance(input);


    //     if (!relevance.Equals("OK"))
    //     {
    //         SwitchTo(relevance);
    //     }

    //     currentState.Update(input, delta);
    // }

    // public void ProcessInputVector(InputPackage input, float delta)
    // {
    //     // Vector3 direction = CharacterResource.GetHeadBoneGlobalTransform().Basis * new Vector3(-input.direction.X,0,-input.direction.Y).Normalized();
    //     // Vector3 characterForward = CharacterModel.Basis.Z;
    //     // float angle = characterForward.SignedAngleTo(direction, Vector3.Up);
    //     // CharacterModel.RotateY(Mathf.Clamp(angle, -_BodyRotationSpeed * delta, _BodyRotationSpeed * delta));

    //     Vector3 characterRotation = CharacterModel.GlobalRotation;
	// 	float targetAngle = HeadBoneAttachment.GlobalRotation.Y;
	// 	float currentAngle = characterRotation.Y;
	// 	float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, _BodyRotationSpeed * delta);

    //     CharacterModel.GlobalRotation = new Vector3(characterRotation.X, newAngle, characterRotation.Z);
    // }

    // public void SwitchTo(String state)
    // {
    //     var currentState = StateModel.States[StateModel.CurrentStateName];
    //     currentState.OnExitState();
    //     currentState = StateModel.States[state];
    //     currentState.OnEnterState();
    // }
}
