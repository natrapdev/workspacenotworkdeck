using Godot;
using System;
using System.Dynamic;

public partial class ViewportCameraController : Camera3D
{
    [Export] Camera3D MainCamera { get; set; }

    public override void _Process(double delta)
    {
        GlobalTransform = MainCamera.GlobalTransform;
    }
}