using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    public InventoryItemSO item; //the inventoryitem scriptable object representing the pickup    
    public bool deactivateAfterPickup = true; //deactivate game object once picked up?    
}
