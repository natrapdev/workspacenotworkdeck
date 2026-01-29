using Godot;
using MyFirst3DGame.Items;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Animator : Node
{
	public StateModel StateModel { get; private set; }
	public AnimationTree AnimationTree { get; private set; }
	[Export] public string BodyStateMachinePath { get; set; } = "parameters/BodyStateMachine/playback";
	[Export] public string DefaultLocomotionPath { get; set; } = "parameters/DefaultLocomotion/blend_position";
	[Export] public string OneHandedLocomotionPath { get; set; } = "parameters/OneHandedLocomotion/blend_position";
	[Export] public string LocomotionBlendPath { get; set; } = "parameters/LocomotionBlend2/blend_amount";
	[Export] public string LegsBodyBlendPath { get; set; } = "parameters/LegsBodyBlend2/blend_amount";

	public Humanoid Humanoid { get; set; }

	public Node3D CharacterModel;
	public CharacterBody3D Character;

	private AnimationNodeBlend2 _locomotionBlend;
	private AnimationNodeBlend2 _legsBodyBlend;
	public string CurrentAnimation { get; set; }
	string locomotionPath;
	private bool _isPlayingAnimation = false;

	public override void _Ready()
	{
		Humanoid = GetParent<Humanoid>();
		StateModel = Humanoid.GetNode<StateModel>("StateModel");
		AnimationTree = Humanoid.AnimationTree;
		CharacterModel = Humanoid.CharacterModel;
		Character = Humanoid.Character;
		locomotionPath = DefaultLocomotionPath;
	}

	public override void _Process(double delta)
	{
		string currentState = StateModel.CurrentStateName;
		
		if (String.IsNullOrWhiteSpace(CurrentAnimation) && Humanoid.CanMove)
		{
			Vector2 inputDirection = Input.GetVector("move_right", "move_left", "move_back", "move_forward");
			AnimationTree.Set(locomotionPath, inputDirection);
		}
	}

	public async void PlayAnimation(string anim, bool canMove)
	{

	}

	public async void PlayUnsheatheAnimation(Weapon weapon)
	{
		var playback = (AnimationNodeStateMachinePlayback)AnimationTree.Get(BodyStateMachinePath);
		var root = (AnimationNodeBlendTree)AnimationTree.TreeRoot;
		var legsBodyBlend = root.GetNode("LegsBodyBlend2");
		string anim = weapon.WeaponSlot == 1 ? "unsheathe1" : "unsheathe2";

		if (anim.Equals("unsheathe1"))
		{
			Humanoid.CanMove = false;
			legsBodyBlend.FilterEnabled = false;
			AnimationTree.Set(LegsBodyBlendPath, 0);
			string animation = weapon.Moves[anim];
			playback.Travel(animation);

			await ToSignal(GetTree().CreateTimer(0.66f), SceneTreeTimer.SignalName.Timeout);

			weapon.ParentWeaponInventory.EquipWeapon(1);

			await ToSignal(playback, AnimationNodeStateMachinePlayback.SignalName.StateFinished);

			// legsBodyBlend.FilterEnabled = true;
			AnimationTree.Set(LegsBodyBlendPath, 1);
			Humanoid.CanMove = true;

			ChangeMainLocomotionPath(weapon.WeaponType);
		}
	}

	public void ChangeMainLocomotionPath(string mode)
	{
		if (String.IsNullOrWhiteSpace(mode))
		{
			AnimationTree.Set(LocomotionBlendPath, 0);
			locomotionPath = DefaultLocomotionPath;
		}
		else if (mode.Equals("one_handed"))
		{
			AnimationTree.Set(LocomotionBlendPath, 1f);
			locomotionPath = OneHandedLocomotionPath;
		}
	}

	private static string GetAnimationFromWeapon(Weapon weapon, string move)
	{
		return weapon.Moves[move];
	}

}
