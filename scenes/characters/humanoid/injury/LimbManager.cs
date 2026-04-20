using Godot;
using MyFirst3DGame.scenes.characters.humanoid.injury.data;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.humanoid.injury;

/// <summary>
/// Manages limb state and handles dismemberment.
/// Creates physics objects for detached limbs.
/// </summary>
public partial class LimbManager : Node
{
    private List<LimbData> _limbData;
    private Dictionary<string, int> _limbNameToIndex;
    private List<DetachedLimbState> _detachedLimbs;
    
    private HumanoidSkeleton _skeleton;
    private Skeleton3D _skeleton3D;
    private Node3D _limbContainer;
    
    private bool _isInitialized = false;
    
    /// <summary>
    /// Event fired when a limb is dismembered.
    /// </summary>
    public event Action<DismembermentResult> OnLimbDismembered;
    
    /// <summary>
    /// Event fired when a limb's attachment state changes.
    /// </summary>
    public event Action<string, bool> OnLimbAttachmentChanged; // (limbName, isAttached)
    
    /// <summary>
    /// Gets the number of limbs.
    /// </summary>
    public int LimbCount => _limbData?.Count ?? 0;
    
    /// <summary>
    /// Gets the number of detached limbs.
    /// </summary>
    public int DetachedLimbCount => _detachedLimbs?.Count ?? 0;
    
    /// <summary>
    /// Initializes the limb manager.
    /// </summary>
    public void Initialize(List<DamageModule> damageModules, HumanoidSkeleton skeleton)
    {
        _skeleton = skeleton;
        _skeleton3D = skeleton;
        _limbData = new List<LimbData>();
        _limbNameToIndex = new Dictionary<string, int>();
        _detachedLimbs = new List<DetachedLimbState>();
        
        // Create limb container for detached limbs
        _limbContainer = new Node3D
        {
            Name = "DetachedLimbs"
        };
        GetTree().CurrentScene.AddChild(_limbContainer);
        
        // Initialize limb data from damage modules
        for (int i = 0; i < damageModules.Count; i++)
        {
            DamageModule module = damageModules[i];
            string limbName = TranslateName(module.Name);
            
            // Get bone index from skeleton
            int boneIndex = FindBoneIndex(limbName);
            
            // Get visual node
            Node3D visualNode = GetVisualNode(module.Parent);
            
            LimbData limb = new LimbData(
                name: limbName,
                isAttached: true,
                thickness: module.Thickness,
                boneIndex: boneIndex,
                visualNode: visualNode
            );
            
            _limbData.Add(limb);
            _limbNameToIndex[limbName] = i;
        }
        
        _isInitialized = true;
    }
    
    public override void _Process(double delta)
    {
        if (!_isInitialized) return;
        
        // Update detached limbs
        UpdateDetachedLimbs((float)delta);
        
        // Cleanup expired detached limbs
        CleanupDetachedLimbs();
    }
    
    /// <summary>
    /// Gets the thickness of a limb.
    /// </summary>
    public float GetLimbThickness(int limbIndex)
    {
        if (limbIndex < 0 || limbIndex >= _limbData.Count) return 0.5f;
        return _limbData[limbIndex].Thickness;
    }
    
    /// <summary>
    /// Gets the thickness of a limb by name.
    /// </summary>
    public float GetLimbThickness(string limbName)
    {
        int index = GetLimbIndex(limbName);
        return GetLimbThickness(index);
    }
    
    /// <summary>
    /// Gets the attachment state of a limb.
    /// </summary>
    public bool IsLimbAttached(string limbName)
    {
        int index = GetLimbIndex(limbName);
        if (index < 0) return false;
        return _limbData[index].IsAttached;
    }
    
    /// <summary>
    /// Gets the limb data for a specific limb.
    /// </summary>
    public LimbData GetLimbData(string limbName)
    {
        int index = GetLimbIndex(limbName);
        if (index < 0) return new LimbData();
        return _limbData[index];
    }
    
    /// <summary>
    /// Gets the limb index by name.
    /// </summary>
    public int GetLimbIndex(string limbName)
    {
        return _limbNameToIndex.GetValueOrDefault(limbName, -1);
    }
    
    /// <summary>
    /// Dismembers a limb.
    /// Creates a physics body for the detached limb and removes it from the skeleton.
    /// </summary>
    public DismembermentResult DismemberLimb(DismembermentRequest request)
    {
        if (!_isInitialized)
        {
            return new DismembermentResult(false, request.LimbName, null, Vector3.Zero);
        }
        
        int limbIndex = GetLimbIndex(request.LimbName);
        if (limbIndex < 0)
        {
            return new DismembermentResult(false, request.LimbName, null, Vector3.Zero);
        }
        
        LimbData limb = _limbData[limbIndex];
        
        if (!limb.IsAttached)
        {
            return new DismembermentResult(false, request.LimbName, null, Vector3.Zero);
        }
        
        // Check if limb can be dismembered
        if (!CanDismemberLimb(request.LimbName))
        {
            return new DismembermentResult(false, request.LimbName, null, Vector3.Zero);
        }
        
        // Create detached limb physics body
        RigidBody3D detachedBody = CreateDetachedLimbBody(limb, request);
        
        if (detachedBody == null)
        {
            return new DismembermentResult(false, request.LimbName, null, Vector3.Zero);
        }
        
        // Hide/remove limb from skeleton
        HideLimbFromSkeleton(limb);
        
        // Update limb state
        limb.IsAttached = false;
        limb.PhysicsBody = detachedBody;
        limb.DetachTime = (float)Time.GetUnixTimeFromSystem();
        _limbData[limbIndex] = limb;
        
        // Add to detached limbs list for cleanup
        DismembermentConfig config = DismembermentConfigs.GetConfigForLimb(request.LimbName);
        DetachedLimbState detachedState = new DetachedLimbState(
            request.LimbName,
            detachedBody,
            config.DetachedLimbMass * 10f // Longer lifetime for larger limbs
        );
        _detachedLimbs.Add(detachedState);
        
        // Fire events
        OnLimbDismembered?.Invoke(new DismembermentResult(
            true,
            request.LimbName,
            detachedBody,
            request.DetachPoint
        ));
        
        OnLimbAttachmentChanged?.Invoke(request.LimbName, false);
        
        return new DismembermentResult(
            true,
            request.LimbName,
            detachedBody,
            request.DetachPoint
        );
    }
    
    /// <summary>
    /// Checks if a limb can be dismembered.
    /// </summary>
    public bool CanDismemberLimb(string limbName)
    {
        int index = GetLimbIndex(limbName);
        if (index < 0) return false;
        
        LimbData limb = _limbData[index];
        
        // Cannot dismember thorax (it's the core)
        if (limb.Name.Equals("thorax", StringComparison.OrdinalIgnoreCase)) return false;
        
        // Cannot dismember already detached limbs
        if (!limb.IsAttached) return false;
        
        return true;
    }
    
    /// <summary>
    /// Creates a physics body for a detached limb.
    /// </summary>
    private RigidBody3D CreateDetachedLimbBody(LimbData limb, DismembermentRequest request)
    {
        if (limb.VisualNode == null) return null;
        
        DismembermentConfig config = DismembermentConfigs.GetConfigForLimb(limb.Name);

        // Create rigid body
        RigidBody3D rigidBody = new RigidBody3D
        {
            Name = $"Detached_{limb.Name}",
            Mass = config.DetachedLimbMass,
            // Set initial position and rotation
            GlobalPosition = request.DetachPoint,
            GlobalRotation = limb.VisualNode.GlobalRotation
        };

        // Create collision shape
        if (limb.VisualNode.GetChild(0) is Node3D childNode && childNode.GetChild(0) is CollisionShape3D collisionShape)
        {
            CollisionShape3D newCollisionShape = new CollisionShape3D
            {
                Shape = collisionShape.Shape
            };
            rigidBody.AddChild(newCollisionShape);
        }
        
        // Create visual mesh
        if (limb.VisualNode.GetChild(0) is Node3D childNode3d)
        {
            foreach (Node child in childNode3d.GetChildren())
            {
                if (child is MeshInstance3D meshInstance)
                {
                    MeshInstance3D newMesh = new()
                    {
                        Mesh = meshInstance.Mesh,
                        MaterialOverride = meshInstance.MaterialOverride
                    };
                    rigidBody.AddChild(newMesh);
                }
            }
        }
        
        // Apply detach force and torque
        rigidBody.ApplyCentralImpulse(request.DetachForce * config.DetachForceMultiplier);
        rigidBody.ApplyTorqueImpulse(request.DetachTorque * config.DetachTorqueMultiplier);
        
        // Add to scene
        _limbContainer.AddChild(rigidBody);
        
        return rigidBody;
    }
    
    /// <summary>
    /// Hides a limb from the skeleton.
    /// </summary>
    private void HideLimbFromSkeleton(LimbData limb)
    {
        if (limb.VisualNode == null) return;
        
        // Hide the visual node
        limb.VisualNode.Visible = false;
        
        // If there's a collision shape, disable it
        if (limb.VisualNode.GetChild(0) is Node3D childNode)
        {
            foreach (Node child in childNode.GetChildren())
            {
                if (child is CollisionShape3D collisionShape)
                {
                    collisionShape.Disabled = true;
                }
            }
        }
        
        // Update skeleton bone if applicable
        if (_skeleton3D != null && limb.BoneIndex >= 0)
        {
            // Scale bone to zero to hide it
            // _skeleton3D.SetBoneScale(limb.BoneIndex, Vector3.Zero);
            _skeleton3D.SetBoneEnabled(limb.BoneIndex, false);
        }
    }
    
    /// <summary>
    /// Updates detached limbs.
    /// </summary>
    private void UpdateDetachedLimbs(float delta)
    {
        for (int i = 0; i < _detachedLimbs.Count; i++)
        {
            DetachedLimbState state = _detachedLimbs[i];
            
            if (state.PhysicsBody != null)
            {
                // Apply gravity if needed
                // Update any other detached limb effects
            }
        }
    }
    
    /// <summary>
    /// Cleans up expired detached limbs.
    /// </summary>
    private void CleanupDetachedLimbs()
    {
        for (int i = _detachedLimbs.Count - 1; i >= 0; i--)
        {
            DetachedLimbState state = _detachedLimbs[i];
            
            if (state.ShouldRemove())
            {
                if (state.PhysicsBody != null && IsInstanceValid(state.PhysicsBody))
                {
                    state.PhysicsBody.QueueFree();
                }
                
                _detachedLimbs.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Finds the bone index for a limb.
    /// </summary>
    private int FindBoneIndex(string limbName)
    {
        if (_skeleton3D == null) return -1;
        
        // Try exact match first
        int boneIndex = _skeleton3D.FindBone(limbName);
        if (boneIndex >= 0) return boneIndex;
        
        // Try with common variations
        string[] variations = [
            limbName,
            limbName.Replace(" ", "."),
            limbName.Replace(" ", "_"),
            $"mixamorig:{limbName}",
            $"mixamorig:{limbName.Replace(" ", ".")}"
        ];
        
        foreach (string variation in variations)
        {
            boneIndex = _skeleton3D.FindBone(variation);
            if (boneIndex >= 0) return boneIndex;
        }
        
        return -1;
    }
    
    /// <summary>
    /// Gets the visual node for a limb.
    /// </summary>
    private Node3D GetVisualNode(Node3D parent)
    {
        if (parent == null) return null;
        
        // Find the first MeshInstance3D or CollisionShape3D in the hierarchy
        foreach (Node child in parent.GetChildren())
        {
            if (child is Node3D child3D)
            {
                if (child is MeshInstance3D || child is CollisionShape3D)
                {
                    return child3D;
                }
                
                Node3D result = GetVisualNode(child3D);
                if (result != null) return result;
            }
        }
        
        return parent;
    }
    
    /// <summary>
    /// Translates a node name to a limb name.
    /// </summary>
    private string TranslateName(string nodeName)
    {
        string name = nodeName;
        
        // Remove left/right prefixes
        string toRemove = "Left";
        int index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            name = name.Remove(index, toRemove.Length);
        }
        
        toRemove = "Right";
        index = name.IndexOf(toRemove, StringComparison.OrdinalIgnoreCase);
        if (index != -1)
        {
            name = name.Remove(index, toRemove.Length);
        }
        
        // Convert PascalCase to lowercase with spaces
        return System.Text.RegularExpressions.Regex.Replace(name, "(?<!^)(?=[A-Z])", " ").ToLower().Trim();
    }
}
