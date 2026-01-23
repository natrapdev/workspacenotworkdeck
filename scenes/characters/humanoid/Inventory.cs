using Godot;
using MyFirst3DGame.Items;
using System;
using System.Collections.Generic;

namespace MyFirst3DGame.scenes.characters.states;

public partial class Inventory : Node
{
    [Export] public int InventorySpace { get; set; } = 3;
    public Node3D[] InventoryContent { get; set; }

    public override void _Ready()
    {
        InventoryContent = new Node3D[InventorySpace];
    }

    public Node3D GetInventoryItem(int index)
    {
        return InventoryContent[index];
    }

    public void AddItemToInventory(Node3D item)
    {
        for (int i = 0; i < InventoryContent.Length; i++)
        {
            if (InventoryContent[i] is null)
            {
                AddItemToInventory(item, i);
                return;
            }
        }
    }

    public void AddItemToInventory(Node3D item, int index)
    {
        if (InventoryContent[index] is null)
        {
            InventoryContent[index] = item;

            if (item is PickableItem)
            {
                PickableItem pickedItem = item as PickableItem;
                pickedItem.IsPickedUp = true;
            }
        }
        else
        {
            GD.PushWarning($"Item \"{item?.Name}\" can't take inventory space at index {index}; it is already occupied by \"{InventoryContent[index]?.Name}\"");
        }
    }

    public void RemoveItemFromInventory(int index)
    {
        if (InventoryContent[index] is not null)
        {
            InventoryContent[index] = null;
        }
    }

    public void ClearInventory()
    {
        for (int i = 0; i < InventoryContent.Length; i++)
        {
            this.InventoryContent[i] = null;
        }
    }
}