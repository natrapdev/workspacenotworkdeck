using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using MyFirst3DGame.Items;

namespace MyFirst3DGame.scenes.characters.states;

public partial class HumanoidResource : Node
{
    [Export] public bool GodMode { get; set; } = false;
    [Export] public float BodyMass { get; set; } = 68f; // kg
    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float StaminaGain { get; set; } = 3f;
    [Export] public float FatigueGain { get; set; } = .1f;
    [Export] public int InventorySpace { get; set; } = 3;

    public CharacterBody3D Character { get; set; }
    public HumanoidModel Humanoid { get; set; }
    public LookAtModifier3D HeadLookAtModifier { get; set; }
    public BoneAttachment3D HeadBoneAttachment { get; set; }

    private Skeleton3D _characterSkeleton;
    private int _characterSkeletonHeadIndex;
    private readonly List<string> _statuses = [];
    private float _totalBloodVolume;
    private float _heartRate; // beats per minute
    private float _strokeVolume;
    private float _cardiacOutput;
    private float _currentStamina = 1f;
    private float _currentFatigue = 0;

    // These weights will determine the best nearby interactable item that will be selected.
    private const float DistanceWeight = 1f;
    private const float AngleWeight = 0.01f;

    // How body mass should be distributed across the body
    private readonly Dictionary<string, float> _bodyPartMassCoefficients = new()
    {
        {"head", 0.0826f},
        {"thorax", 0.2010f},
        {"abdomen", 0.1310f},
        {"pelvis", 0.1370f},
        {"upper arm", 0.0325f}, // We usually have two of these so multiply by 2 for total
        {"lower arm", 0.0187f - 0.0006f}, // All body parts add up to 1.0006 so this is to make it consitent
        {"hand", 0.0065f},
        {"thigh", 0.1050f},
        {"shin", 0.0475f},
        {"foot", 0.0143f}
    };

    public Transform3D HeadBoneGlobalTransform
    {
        get
        {
            Transform3D relativeTransform = _characterSkeleton.GetBoneGlobalPose(_characterSkeletonHeadIndex);
            Transform3D globalTransform = _characterSkeleton.GlobalTransform * relativeTransform;

            return globalTransform;
        }
    }

    public float TotalBloodVolume { get { return BodyMass * 75f; } } // blood in mL/kg
    public float CurrentStamina { get { return _currentStamina / MaxStamina; } }

    private Node _worldItemsContainer;
    private Dictionary<string, float> _bodyPartBloodVolume;

    public InteractableItem ItemFocus { get; set; }

    private float _lastStamina;

    public Node3D CameraPivot;

    public override void _Ready()
    {
        _worldItemsContainer = FindWorldItemContainer();
        _bodyPartBloodVolume = _bodyPartMassCoefficients;
        _currentStamina = MaxStamina;

        foreach (string bodyPart in _bodyPartMassCoefficients.Keys)
        {
            _bodyPartBloodVolume[bodyPart] = CalculateBodyPartBloodVolume(bodyPart);
        }

        _lastStamina = _currentStamina;

        Humanoid = GetParent() as HumanoidModel;

        _characterSkeleton = Humanoid.Skeleton;
        HeadLookAtModifier = _characterSkeleton.GetNode<LookAtModifier3D>("HeadLookAt");
        HeadBoneAttachment = _characterSkeleton.GetNode<BoneAttachment3D>("HeadBoneAttachment");
        _characterSkeletonHeadIndex = _characterSkeleton.FindBone("spine.006");

        CameraPivot = Humanoid.GetParent().GetNode<Node3D>("CameraPivot");
    }

    public void Update(float delta)
    {
        if (_currentStamina != _lastStamina)
        {
            GD.Print($"Stamina: {_currentStamina}");
        }

        UpdateStamina(StaminaGain * delta);

        List<Node3D> nearbyPickableItems = SearchForNearbyWorldItems();

        UpdateNearbyItems(
            FindBestPickableItem(nearbyPickableItems)
        );


        _lastStamina = _currentStamina;
    }

    public void PayCosts(State state)
    {
        UpdateStamina(-state.StaminaCost);
        UpdateFatigue(-state.FatigueCost);
    }
    public void PayCosts(State state, float delta)
    {
        UpdateStamina(-state.StaminaCost * delta);
        UpdateFatigue(-state.FatigueCost * delta);
    }

    public void UpdateNearbyItems(PickableItem newPickableItemFocus)
    {
        // PickableItem old;
        ItemFocus?.ToggleTooltip(false);
        // old = ItemFocus;
        ItemFocus = newPickableItemFocus;
        ItemFocus?.ToggleTooltip(true);

        // if (old is not null && old.IsPickedUp && old != ItemFocus)
        // {
        //     old.QueueFree();
        // }
    }

    private Node FindWorldItemContainer()
    {
        Node mainSceneNode = null;
        Node itemContainer;

        foreach (Node node in GetTree().Root.GetChildren())
        {
            if (String.Equals(node.Name, "main", StringComparison.OrdinalIgnoreCase))
            {
                mainSceneNode = node;
                break;
            }
        }

        if (mainSceneNode == null)
        {
            GD.PrintErr("Could not find a main scene.");
        }

        itemContainer = mainSceneNode.GetNodeOrNull<Node>("ItemCollection");

        if (itemContainer == null)
        {
            GD.PushWarning("Could not find node \"ItemCollection\".");

            foreach (var child in mainSceneNode.GetChildren())
            {
                if (child.IsInGroup("pickable_items"))
                {
                    GD.PushWarning($"World items are currently being stored in node \"{child.Name}\". Consider renaming this node to \"ItemCollection\".");
                    itemContainer = child;
                }
            }
        }

        return itemContainer;
    }

    private List<Node3D> SearchForNearbyWorldItems()
    {
        List<Node3D> pickableItems = [];

        foreach (Node3D item in _worldItemsContainer.GetChildren().Cast<Node3D>())
        {
            if (item is not PickableItem pickableItem)
            {
                continue;
            }
            else if (pickableItem.CanInteract(Humanoid.Character, CameraPivot))
            {
                pickableItems.Add(item);
            }
        }

        return pickableItems;
    }

    private PickableItem FindBestPickableItem(List<Node3D> items)
    {
        PickableItem selectedPickableItem = null;
        float bestScore = float.PositiveInfinity;

        foreach (Node3D item in items)
        {
            PickableItem pickableItem = item as PickableItem;
            float angle = pickableItem.LookDifference(Humanoid.Character, CameraPivot);
            float distance = pickableItem.DistanceBetween(Humanoid.Character);

            float score = angle;

            if (score < bestScore)
            {
                selectedPickableItem = pickableItem;
                bestScore = score;
            }
        }

        return selectedPickableItem;
    }

    public Transform3D GetBoneGlobalTransform(int boneIndex)
    {
        Transform3D relativeTransform = _characterSkeleton.GetBoneGlobalPose(boneIndex);
        Transform3D globalTransform = _characterSkeleton.GlobalTransform * relativeTransform;

        return globalTransform;
    }

    public Transform3D GetHeadBoneGlobalPose() => _characterSkeleton.GetBoneGlobalPose(_characterSkeletonHeadIndex);
    public bool HasEnoughStamina(State state) => state.StaminaCost <= _currentStamina && _currentStamina > 0;
    public void UpdateStamina(float changeValue) => Mathf.Clamp(_currentStamina += changeValue, -MaxStamina, MaxStamina);
    public void UpdateFatigue(float changeValue) => _currentFatigue += changeValue;
    public float CalculateBodyPartBloodVolume(string bodyPart) => TotalBloodVolume * _bodyPartMassCoefficients[bodyPart];
    public float CalculateBodyPartMass(string bodyPart) => BodyMass * _bodyPartMassCoefficients[bodyPart];
    public float BloodVolumeInBodyPart(string bodyPart) => _bodyPartBloodVolume[bodyPart];
}