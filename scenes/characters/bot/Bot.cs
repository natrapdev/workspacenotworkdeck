using Godot;
using System;
using MyFirst3DGame.scenes.characters.humanoid;
using MyFirst3DGame.scenes.characters.states;
using System.Threading.Tasks;

namespace MyFirst3DGame.scenes.characters.bot;

public partial class Bot : CharacterBody3D
{
	[Export] public Player Player { get; set; }
	[Export] public CharacterAppearance CharacterModel { get; set; }
	[Export] public HumanoidModel Humanoid { get; set; }
	[Export] public InputGatherer InputSource { get; set; }
	[Export] public int AppearanceSet { get; set; } = 1; // wok alert
	private Skeleton3D _skeleton;
	private HumanoidStates _characterStateModel;
	public BoneAttachment3D HeadBoneAttachment;

	[Export] public float AttackSensitivityMultiplier { get; set; } = 1f;

	public override void _Ready()
	{
		CharacterModel.AcceptModel(Humanoid);
		_characterStateModel = Humanoid.StateContainer;
		_skeleton = Humanoid.Skeleton;
		HeadBoneAttachment = _skeleton.GetNode<BoneAttachment3D>("HeadBoneAttachment");

		var bot = GetNodeOrNull("../Human2") as Bot;
		if (bot == null) return;
		var mode = bot.InputSource as HumanAi;
		mode.CurrentTask = "chase";
	}

	public override void _Process(double delta)
	{
		InputPackage input = InputSource.GatherInput();
		Humanoid.Update(input, (float)delta);
		MoveAndSlide();
	}
}
