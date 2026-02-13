using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;
public interface IChildState
{
    State BaseState { get; set; }
}
