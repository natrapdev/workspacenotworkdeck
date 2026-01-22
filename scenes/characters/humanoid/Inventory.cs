using Godot;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.humanoid;
public partial class Inventory : Node
{
    [Export] public int InventorySpace { get; set; } = 3;

    private string[] _inventoryContent;

    public override void _Ready()
    {
        _inventoryContent = new string[InventorySpace];
    }

    public string[] GetInventory()
    {
        return _inventoryContent;
    }

    public void SetInventory(string[] newInventory)
    {
        this._inventoryContent = newInventory;
    }

    public void AddItemToInventory(string item)
    {
        for (int i = 0; i < _inventoryContent.Length; i++)
        {
            if (String.IsNullOrEmpty(_inventoryContent[i]))
            {
                _inventoryContent[i] = item;
                break;
            }
        }
    }

    public void AddItemToInventory(string item, int index)
    {
        if (String.IsNullOrEmpty(_inventoryContent[index]))
        {
            _inventoryContent[index] = item;
        }
        else
        {
            GD.PushWarning($"Item \"{item}\" can't take inventory space at index {index}; it is already occupied by \"{_inventoryContent[index]}\"");
        }
    }

    public void ClearInventory()
    {
        for (int i = 0; i < _inventoryContent.Length; i++)
        {
            this._inventoryContent[i] = "";
        }
    }
}
