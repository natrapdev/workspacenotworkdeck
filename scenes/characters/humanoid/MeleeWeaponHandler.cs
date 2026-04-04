using Godot;
using MyFirst3DGame.Items;
using MyFirst3DGame.scenes.characters.states;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class MeleeWeaponHandler : Node3D
{
    private HumanoidCombat _combat;

    private PhysicsDirectSpaceState3D _physicsSpace;

    private Weapon _currentWeapon;
    private Marker3D _bladeStart;
    private Marker3D _bladeEnd;
    private float _bladeWidth;
    private float _bladeLength;

    private bool _hasHitThisFrame;
    private readonly HashSet<CollisionObject3D> _hitObjectsThisFrame = new();

    // Reusable ray buffer
    private readonly List<PhysicsRayQueryParameters3D> _raycastPool = new();

    private float _lastAnimationPosition = -1f;

    private const string _ThrustAnimKeyword = "Thrust";
    private const string _SlashAnimKeyword = "Slash";

    public override void _Ready()
    {
        _physicsSpace = GetWorld3D().DirectSpaceState;

    }

    private void InitializeRaycastPool()
    {
        for (int i = 0; i < _currentWeapon.RaycastAmount; i++)
        {
            _raycastPool.Add(new PhysicsRayQueryParameters3D());
        }
    }

    private void RefreshWeaponBounds()
    {
        if (_combat.Humanoid.CurrentWeapon is null) return;

        _bladeStart = _combat.Humanoid.CurrentWeapon.BladeStartMarker;
        _bladeEnd = _combat.Humanoid.CurrentWeapon.BladeEndMarker;
        _bladeWidth = _combat.Humanoid.CurrentWeapon.BladeWidth;
        _bladeLength = _bladeStart.Position.DistanceTo(_bladeEnd.Position);
    }


}
