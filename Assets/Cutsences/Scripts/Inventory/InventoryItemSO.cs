using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Inventory Item")]
public class InventoryItemSO : ScriptableObject
{
    public string itemDisplayName;
    public string itemDescription;
    //public bool countable;
    public Sprite itemSprite;

    //private Inventory inventory; //tracks the inventory the items gets added to
    private InventoryBox inventoryBox; //tracks the inventoryBox the items gets contained in    
    
    //updates item when it's picked up to track which inventoryBox it's contained in
    public void UpdateAfterPickup(InventoryBox _inventoryBox)
    {        
        inventoryBox = _inventoryBox;
        //inventory = inventoryBox.GetInventory();
    }

    //removes itself from the inventory
    public void RemoveFromInventory()
    {
        if(inventoryBox!=null)
        {
            inventoryBox.GetInventory().RemoveFromInventory(inventoryBox);
        }
        else
        {
            Debug.LogError("item is not in an inventoryBox");
        }
    }
}
