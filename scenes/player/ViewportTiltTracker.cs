using Godot;
using System;
using Viewport;

namespace MyFirst3DGame.scenes.characters.states;

public partial class ViewportTiltTracker : Node3D
{
    [Export] public ViewportModel Viewport { get; set; }
    [Export] public Node3D TiltSubject { get; set; }
    [Export] float TiltSensitivityHorizontal { get; set; } = 8f;
    [Export] float TiltSensitivityVertical { get; set; } = 6f;
    [Export] float TiltSpeed { get; set; } = 5f;
    [Export] float MaxTiltAngleHorizontal { get; set; } = 40f;
    [Export] float MaxTiltAngleVertical { get; set; } = 45f;

    private bool _isTracking = false;

    private Vector3 _baseRotation = Vector3.Zero;
    private Vector3 _targetRotation = Vector3.Zero;
    private Vector3 _currentRotation = Vector3.Zero;
    private Vector2 _mouseMotionDelta = Vector2.Zero;
    private bool _tracking = false;

    public override void _Ready()
    {
        _baseRotation = TiltSubject.RotationDegrees;
        _currentRotation = _baseRotation;
        _targetRotation = _baseRotation;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motionEvent && _tracking)
        {
            _mouseMotionDelta = motionEvent.Relative;
        }
    }

    public void UpdateTilt(float delta, Vector2 attackDirection)
    {
        if (!_tracking)
        {
            SmoothReturnToBase(delta);
            return;
        }

        float targetTilt = GetTiltInDirection(attackDirection);

        _targetRotation = new Vector3(_baseRotation.X, _baseRotation.Y, targetTilt);

        SmoothApplyTilt(delta);
    }

    private float GetTiltInDirection(Vector2 attackDirection)
    {
        return attackDirection switch
        {
            Vector2 dir when dir == Viewport.AttackVectors["slash1"] => CalculateTilt(
                -_mouseMotionDelta.Y, -MaxTiltAngleHorizontal, MaxTiltAngleHorizontal, TiltSensitivityHorizontal
            ),
            Vector2 dir when dir == Viewport.AttackVectors["slash2"] => CalculateTilt(
                _mouseMotionDelta.Y, -MaxTiltAngleHorizontal, MaxTiltAngleHorizontal, TiltSensitivityHorizontal
            ),
            Vector2 dir when dir == Viewport.AttackVectors["slash3"] => CalculateTilt(
                -_mouseMotionDelta.X, -MaxTiltAngleVertical, MaxTiltAngleVertical, TiltSensitivityVertical
            ),
            _ => 0f
        };
    }

    private static float CalculateTilt(float mouseDelta, float minTilt, float maxTilt, float sensitivity)
    {
        float targetTilt = mouseDelta * sensitivity;
        targetTilt = Mathf.Clamp(targetTilt, minTilt, maxTilt);
        return targetTilt;
    }

    private void SmoothApplyTilt(float delta)
    {
        float lerpFactor = 1f - Mathf.Exp(-TiltSpeed * delta);
        _currentRotation = _currentRotation.Lerp(_targetRotation, lerpFactor);
        TiltSubject.RotationDegrees = _currentRotation;
    }

    public void SmoothReturnToBase(float delta)
    {
        if (_tracking) return;
        SmoothApplyTilt(delta * 5f);
    }

    public void StartTracking()
    {
        _tracking = true;
    }

    public void StopTracking()
    {
        _tracking = false;
        _targetRotation = _baseRotation;
        _mouseMotionDelta = Vector2.Zero;
    }
}