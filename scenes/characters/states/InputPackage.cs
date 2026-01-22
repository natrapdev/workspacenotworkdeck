using System;
using System.Collections.Generic;
using Godot;

namespace MyFirst3DGame.scenes.characters.states;
public class InputPackage
{
    public List<string> actions = [];
    public Vector2 direction;
    public Node3D interactedItem;
}