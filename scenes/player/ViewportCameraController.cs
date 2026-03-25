namespace Viewport;

using Godot;

public partial class ViewportCameraController : Camera3D
{
    [Export] ViewportModel Viewport { get; set; }

    public Camera3D MainCamera;

    public override void _Process(double delta)
    {
        GlobalTransform = MainCamera.GlobalTransform;
    }
}