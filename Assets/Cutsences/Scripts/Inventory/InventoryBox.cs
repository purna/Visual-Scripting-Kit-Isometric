using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class for each box in the inventory that references an InventoryItemSO
//enables clicking on and interacting with items in the inventory
public class InventoryBox : MonoBehaviour
{    
    public InventoryItemSO item;    
    public SpriteRenderer boundary;
    public SpriteRenderer icon;
    public bool isActive = false;    
    private Inventory inventory;

    private string itemDescription;
    private string itemName;

    //private Color offCol = new Color(0.5f, 0.1f, 0.1f);
    private Color onCol = new Color(0.1f, 0.5f, 0.1f);
    private Color defaultCol = new Color(0.1f, 0.1f, 0.1f);

    public void Initialise(Inventory _inventory, InventoryItemSO _item)
    {        
        inventory = _inventory;        
        boundary.color = defaultCol;
        isActive = false;
        item = _item;
        icon.sprite = _item.itemSprite;
        itemDescription = _item.itemDescription;
        itemName = _item.itemDisplayName;
    }

    private void OnMouseOver()
    {
        //don't do anything if interactable isn't initialised
        if (!isActive)
        { return; }
        //only if the gamecontroller is receiving clicks
        if (inventory.chapter.receivingInputs)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Pressed primary button.");
                //only do default left action if not holding an inventory item
                if (inventory.carriedItem.IsActive())
                {
                    //cancel if object is in process of being dropped
                    if (inventory.carriedItem.IsDropping())
                    { return; }
                    LeftActionCarrying();
                }
                else
                {
                    LeftAction();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                //Debug.Log("Pressed secondary button.");
                //only do default rightAction if not holding an inventory item
                if (inventory.carriedItem.IsActive())
                {
                    //cancel if object is in process of being dropped
                    if (inventory.carriedItem.IsDropping())
                    { return; }
                    RightActionCarrying();
                }
                else
                {
                    RightAction();
                }
            }
        }
    }

    private void OnMouseEnter()
    {
        if (!isActive)
        { return; }

        if (inventory.carriedItem.IsActive())
        {
            MouseEnterActionCarrying();
        }
        else
        {
            MouseEnterAction();
        }

    }

    private void OnMouseExit()
    {
        if (!isActive)
        { return; }
        if (inventory.carriedItem.IsActive())
        {
            MouseExitActionCarrying();
        }
        else
        {
            MouseExitAction();
        }        
    }

    private void MouseEnterAction()
    {
        //option to change to/from a different cursor when hovering over items in the inventory. Have decided against for now
        //inventory.cursors.ChangeCursor(CursorController.CursorName.pickup);
        inventory.UpdateTooltip(itemName);
        boundary.color = onCol;
    }

    private void MouseEnterActionCarrying()
    {
        inventory.carriedItem.UpdateInventoryTooltip(itemName);
        boundary.color = onCol;        
    }

    private void MouseExitAction()
    {
        //option to change to/from a different cursor when hovering over items in the inventory. Have decided against for now
        //inventory.cursors.ChangeCursor(CursorController.CursorName.main);
        inventory.UpdateTooltip("");
        boundary.color = defaultCol;
    }

    private void MouseExitActionCarrying()
    {
        inventory.carriedItem.UpdateInventoryTooltip();
        boundary.color = defaultCol;        
    }

    //right click looks at an item
    public void RightAction()
    {
        inventory.UpdateTooltip(itemDescription);
    }

    //right click on an item when carrying another item. Item is dropped under control of CarriedItem script, so run routines to exit then reenter the item
    public void RightActionCarrying()
    {      
        MouseExitActionCarrying();
        MouseEnterAction();
    }

    //left click on an item either picks it up or uses a control item
    public void LeftAction()
    {

            PickedItem();

    }

    //left click on an item while carrying another might them together in a future version
    public void LeftActionCarrying()
    {

    }

    //pick an item from the inventory to use
    private void PickedItem()
    {
        isActive = false; //deactivate this item so can't use it with itself
        boundary.color = defaultCol; //turn off highlight        
        inventory.cursors.ChangeToCarryCursor(true); //change to the small pointer
        inventory.carriedItem.Activate(this); //activate the carried item gameobject
    }

    //returns the inventory that the Inventory box has been added to
    public Inventory GetInventory()
    {
        return inventory;
    }

    public void ChangeIcon(Sprite newSprite)
    {
        icon.sprite = newSprite;
    }

    public void UpdateDescriptionText(string text)
    {
        itemDescription = text;
    }

    public void UpdateNameText(string text)
    {
        itemName = text;
    }

}
