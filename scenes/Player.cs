using Godot;
using System;
using System.Linq;
using MyFirst3DGame.scenes.characters.states;
public partial class Player : CharacterBody3D
{
	[Export] public Node3D CharacterModel { get; set; }
	[Export] public Node HumanoidNode { get; set; }
	[Export] public Node InputSource { get; set; }



	private Skeleton3D _skeleton;
	private CharacterState _characterStates;
	private StateModel _characterStateModel;

	public override void _Ready()
	{
		_characterStateModel = HumanoidNode.GetNode<StateModel>("StateModel");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		InputPackage input = ((InputGatherer)InputSource).GatherInput();
		_characterStateModel.Update(input, (float)delta);
		MoveAndSlide();
	}
}
