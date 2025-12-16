using Godot;
using System;
using System.Linq;
using System.Security.Cryptography;

public partial class CombatController : Node3D
{
    [Signal] public delegate void HitEventHandler(double damage);
    [Export] public Node3D CharacterModel { get; set; }
    [Export] public Node3D ViewportArms { get; set; }
    [Export] public string UpperBodyStatePlaybackPath { get; set; }
    [Export] public string UpperBodyBlendPlaybackPath { get; set; }
    [Export] public Node3D HeldWeapon { get; set; }
    [Export] public AnimationTree animationTree { get; set; }
    [Export] public int MiddleSwingAttackType { get; set; }
    [Export] public int UpperSwingAttackType { get; set; }
    [Export] public int ThrustAttackType { get; set; }
    [Export] public int WeaponRaycastAmount { get; set; } = 7;

    public Skeleton3D Skeleton;
    public Node3D RightHandContainer;
    public Node3D HipContainer;
    public Node3D ItemContainer;

    public Skeleton3D ViewportSkeleton;
    public Node3D ViewportRightHandContainer;

    private bool holdingWeapon = false;
    private bool usingTwoHands;

    private bool executeAttack = false;
    private float lastClickTime = 0f;
    private int clickCount = 0;
    private const float MAX_CLICK_INTERVAL = 0.275f; // max gap between cliks in milliseconds

    // -= PLACEHOLDER =-
    // TODO: Add weapon config to specify if style is OneHanded or TwoHanded
    public string weaponStyleName = "OneHanded";
    private string armBlendPlaybackPath = "parameters/Blend2/blend_amount";
    private string armStatePlaybackPath = "parameters/ArmStateMachine/playback";
    private string armTimeScalePath = "parameters/ArmsTimeScale/scale";
    public AnimationTree ArmAnimationTree;

    private PhysicsDirectSpaceState3D _spaceState;

    public override void _Ready()
    {
        var character = GetNode<Node3D>("../CharacterModel");
        Skeleton = character.GetNode<Skeleton3D>("Armature/Skeleton3D");
        RightHandContainer = Skeleton.GetNode<Node3D>("RightHandAttachment/RightHandContainer");
        HipContainer = Skeleton.GetNode<Node3D>("HipAttachment/HipContainer");

        ViewportSkeleton = ViewportArms.GetNode<Skeleton3D>("Armature/Skeleton3D");
        ViewportRightHandContainer = ViewportSkeleton.GetNode<Node3D>("RightHandAttachment/RightHandContainer");
        ItemContainer = ViewportArms.GetNode<Node3D>("ItemContainer");
        ArmAnimationTree = ViewportArms.GetNode<AnimationTree>("AnimationTree");

        HeldWeapon = ViewportArms.GetNode<Node3D>("ItemContainer/ArmingSword");

        Skeleton.GetNode<MeshInstance3D>("LeftUpperArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("RightUpperArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("LeftLowerArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("RightLowerArm").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("LeftHand").Visible = false;
        Skeleton.GetNode<MeshInstance3D>("RightHand").Visible = false;

        UnEquipWeapon();

        _spaceState = GetWorld3D().DirectSpaceState;


    }

    bool isAttacking = false;

    public override void _Process(double delta)
    {
        var playback = (AnimationNodeStateMachinePlayback)ArmAnimationTree.Get(armStatePlaybackPath);

        ItemContainer.GetNode<Area3D>("ArmingSword/WeaponArea").Monitoring = false;

        if (playback.GetCurrentNode().Equals("End") && !holdingWeapon)
        {
            UnEquipWeapon();
        }
        else
        {
            if (playback.IsPlaying() &&
            (playback.GetCurrentNode().Equals(weaponStyleName + "MiddleSwing")
            || playback.GetCurrentNode().Equals(weaponStyleName + "UpperSwing")
            || playback.GetCurrentNode().Equals(weaponStyleName + "Thrust")))
            {
                //GD.Print($"Anim length: {playback.GetCurrentLength()}");
                //GD.Print($"Play pos: {playback.GetCurrentPlayPosition()}");
                var camPivot = GetNode<Node3D>("../CameraPivot");
                CharacterModel.RotationDegrees = CharacterModel.RotationDegrees.Lerp(new Vector3(0, camPivot.RotationDegrees.Y, 0), 0.1f);

                if (playback.GetCurrentPlayPosition() < playback.GetCurrentLength() / 2.25
                && playback.GetCurrentPlayPosition() > playback.GetCurrentLength() * 0.2)
                {
                    ItemContainer.GetNode<Area3D>("ArmingSword/WeaponArea").Monitoring = true;
                    isAttacking = true;
                } else
                {
                    isAttacking = false;
                }
            }
        }


        if (!executeAttack)
        {
            lastClickTime += (float)delta;

            if ((lastClickTime >= MAX_CLICK_INTERVAL || clickCount >= 3) && holdingWeapon)
            {
                executeAttack = true;
                Attack(clickCount);
                clickCount = 0;
            }
        }

        if (Input.IsActionJustPressed("attack_main"))
        {
            clickCount++;
            lastClickTime = 0f;
            executeAttack = false;
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (isAttacking)
        {
            var weaponStart = HeldWeapon.GetNode<Marker3D>("BladeStart");
            var weaponEnd = HeldWeapon.GetNode<Marker3D>("BladeEnd");
            float increment = (weaponEnd.Position.Y - weaponStart.Position.Y) / WeaponRaycastAmount;
            Vector3 targetPosition = new(-0.5f, 0, 0);

            for (int i = 0; i < WeaponRaycastAmount; i++)
            {
                float posY = weaponStart.Position.Y + increment * i;

                var queryParams = PhysicsRayQueryParameters3D.Create(
                    weaponStart.Position + new Vector3(0, increment * i, 0), targetPosition
                );

                var result = _spaceState.IntersectRay(queryParams);

                if (result.Count > 0)
                {
                    Node3D collider = (Node3D)result["collider"];
                    GD.Print($"Ray {i} hit {collider.Name} at pos {result["position"]}");
                }

            }
        }
    }


    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("EquipWeapon"))
        {
            holdingWeapon = !holdingWeapon;

            var playback = (AnimationNodeStateMachinePlayback)ArmAnimationTree.Get(armStatePlaybackPath);

            if (holdingWeapon)
            {
                playback.Travel("Start");
                EquipWeapon();
            }
            else
            {
                playback.Travel("End");
            }
        }
    }


    public void Attack(int attackType)
    {
        var playback = (AnimationNodeStateMachinePlayback)ArmAnimationTree.Get(armStatePlaybackPath);

        if (attackType == MiddleSwingAttackType)
        {
            playback.Travel(weaponStyleName + "MiddleSwing");
        }
        else if (attackType == UpperSwingAttackType)
        {
            playback.Travel(weaponStyleName + "UpperSwing");
        }
        else if (attackType == ThrustAttackType)
        {
            playback.Travel(weaponStyleName + "Thrust");
        }


    }

    public void PickUpWeapon()
    {

    }

    public void EquipWeapon()
    {
        ItemContainer.GetNode<MeshInstance3D>("ArmingSword/armingSword").Visible = true;
        var parent = ItemContainer.GetParent();
        parent.RemoveChild(ItemContainer);
        ViewportRightHandContainer.AddChild(ItemContainer);
        ItemContainer.Position = Vector3.Zero;
        ItemContainer.RotationDegrees = Vector3.Zero;
    }

    public void UnEquipWeapon()
    {
        //var parent = ItemContainer.GetParent();
        //parent.RemoveChild(ItemContainer);
        ItemContainer.Position = Vector3.Zero;
        ItemContainer.RotationDegrees = Vector3.Zero;
        ItemContainer.GetNode<MeshInstance3D>("ArmingSword/armingSword").Visible = false;
        ItemContainer.GetNode<Area3D>("ArmingSword/WeaponArea").Monitoring = false;
    }
}
