using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;
public partial class Humanoid : Node
{
    [Export] public CharacterBody3D Character { get; set; }
    [Export] public Node3D CharacterModel { get; set; }

    private Resource _humanoidResources;
    
}
