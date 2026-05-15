using Godot;
using System;

namespace MyFirst3DGame.scenes.characters.states;

public partial class IkController : Node
{
    [Export] public HumanoidModel Humanoid { get; set; }

    public override void _Ready()
    {
        Skeleton3D skeleton = Humanoid.Skeleton;
        var spineIk = skeleton.GetNode("SpineCCDIK3D") as Ccdik3D;
        var cameraPivot = GetNode("../CameraPivot") as Node3D;

        Marker3D spineIkTarget = new()
        {
            Name = "SpineTarget"
        };

        cameraPivot?.AddChild(spineIkTarget);
        spineIkTarget.Position = new Vector3(0, 1, 0);

        spineIk?.SetTargetNode(0, spineIk.GetPathTo(spineIkTarget));
    }
}
