using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using MyFirst3DGame.Items;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Resources : Node
{
    [Export] public bool GodMode { get; set; } = false;
    [Export] public float BodyMass { get; set; } = 68f; // kg
    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float StaminaGain { get; set; } = 3f;
    [Export] public float FatigueGain { get; set; } = .1f;
    [Export] public int InventorySpace { get; set; } = 3;

    public CharacterBody3D character;
    public Node3D characterModel;

    private readonly List<string> _statuses = [];
    private float _totalBloodVolume;
    private float _heartRate; // beats per minute
    private float _strokeVolume;
    private float _cardiacOutput;
    private float _currentStamina = 1f;
    private float _currentFatigue = 0;

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

    private Node _worldItemsContainer;
    private Dictionary<string, float> _bodyPartBloodVolume;
    private string[] _inventory;
    private readonly List<Node3D> _nearbyPickableItems = [];
    public PickableItem pickableItemFocus;

    private float _lastStamina;

    public override void _Ready()
    {
        _worldItemsContainer = FindWorldItemContainer();
        _inventory = new string[InventorySpace];
        _bodyPartBloodVolume = _bodyPartMassCoefficients;
        _currentStamina = MaxStamina;

        foreach (string bodyPart in _bodyPartMassCoefficients.Keys)
        {
            _bodyPartBloodVolume[bodyPart] = CalculateBodyPartBloodVolume(bodyPart);
        }

        _lastStamina = _currentStamina;
    }

    public void Update()
    {
        if (_currentStamina != _lastStamina)
        {
            GD.Print($"Stamina: {_currentStamina}");
        }

        ChangeStamina(StaminaGain);

        List<Node3D> nearbyPickableItems = SearchForNearbyWorldItems();

        if (nearbyPickableItems != null || nearbyPickableItems.Count > 0)
        {
            UpdateNearbyItems(FindBestPickableItem(nearbyPickableItems));
        }

        _lastStamina = _currentStamina;
    }

    public void UpdateNearbyItems(PickableItem newPickableItemFocus)
    {
        if (pickableItemFocus == null && newPickableItemFocus != null)
        {
            pickableItemFocus = newPickableItemFocus;
            pickableItemFocus.TogglePickUpTooltip(true);
        }
        else if (pickableItemFocus != null && newPickableItemFocus == null)
        {
            pickableItemFocus.TogglePickUpTooltip(false);
        }
        else if (newPickableItemFocus != pickableItemFocus)
        {
            pickableItemFocus.TogglePickUpTooltip(false);
            pickableItemFocus = newPickableItemFocus;
            pickableItemFocus.TogglePickUpTooltip(true);
        }
    }

    private Node FindWorldItemContainer()
    {
        Node mainSceneNode = null;
        Node itemContainer;

        foreach (Node node in GetTree().Root.GetChildren())
        {
            if (node.Name.Equals("Main"))
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
            if (item is not PickableItem)
            {
                continue;
            }

            PickableItem pickableItem = item as PickableItem;

            if (pickableItem.CanBePickedUp(characterModel))
            {
                pickableItems.Add(item);
            }
        }

        return pickableItems;
    }

    private PickableItem FindBestPickableItem(List<Node3D> items)
    {
        PickableItem selectedPickableItem = null;
        float closestAngleDifference = 180;

        foreach (Node3D item in items)
        {
            PickableItem pickableItem = item as PickableItem;
            float angleDifference = pickableItem.AngleDifference2D(character);

            if (angleDifference < closestAngleDifference)
            {
                selectedPickableItem = pickableItem;
                closestAngleDifference = angleDifference;
            }
        }

        return selectedPickableItem;
    }

    public bool HasEnoughStamina(CharacterState state)
    {
        return state.staminaCost <= _currentStamina && _currentStamina > 0;
    }

    public void ChangeStamina(float changeValue)
    {
        _currentFatigue = Mathf.Clamp(_currentFatigue + changeValue, -MaxStamina, MaxStamina);
    }
    public void ChangeFatigue(float changeValue)
    {
        _currentFatigue += changeValue;
    }
    public float CalculateTotalBloodVolume()
    {
        return BodyMass * 75f; // blood in mL/kg
    }

    public float CalculateBodyPartBloodVolume(string bodyPart)
    {
        return CalculateTotalBloodVolume() * _bodyPartMassCoefficients[bodyPart];
    }

    public float CalculateBodyPartMass(string bodyPart)
    {
        return BodyMass * _bodyPartMassCoefficients[bodyPart];
    }

    public float BloodVolumeInBodyPart(string bodyPart)
    {
        return _bodyPartBloodVolume[bodyPart];
    }

    public float CurrentStamina()
    {
        return this._lastStamina / MaxStamina;
    }
}