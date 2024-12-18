using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls an item picked up from the inventory to be used on an interactable
/// Currently no support for using inventory items with each other
/// </summary>
public class CarriedItem : MonoBehaviour
{
    private Inventory inventory;
    public SpriteRenderer boundary; //a background for the icon. Currently not used
    public SpriteRenderer icon;
    private bool isActive; //is the carried item active and visible?
    private bool isDropping = false; //is the dropping animation playing?
    private InventoryBox inventoryBox; //the inventoryBox for the item that is being carried    
    public string itemName;
    private Vector3 localPos;    
    private IEnumerator droppingAnimation;

    private Color onCol = new Color(0.1f, 0.5f, 0.1f, 0.75f);
    private Color defaultCol = new Color(0.1f, 0.1f, 0.1f, 0.5f);

    public void Initialise(Inventory _inventory)
    {        
        inventory = _inventory;        
        //deactivate object initially
        boundary.enabled = false;
        icon.enabled = false;
        isActive = false;        
    }

    public void Activate(InventoryBox _inventoryBox)//, Sprite _icon, string _itemName, Vector3 _localPos)
    {
        inventoryBox = _inventoryBox;        
        icon.sprite = inventoryBox.icon.sprite;     
        itemName = inventoryBox.item.itemDisplayName;
        localPos = inventoryBox.transform.localPosition;

        icon.enabled = true;
        //boundary.enabled = true;
        isActive = true;
        UpdateInventoryTooltip();        
    }

    //deactivate once dropped or when used
    public void Deactivate()
    {
        boundary.enabled = false;        
        icon.enabled = false;        
        isActive = false;
        inventoryBox.isActive = true; //reactivate the source inventorybox in the inventory
        inventory.cursors.ChangeToCarryCursor(false);
        inventory.UpdateTooltip("");
    }    

    //highlight the boundary of the item if it's over an interactable or other inventory item
    //currently not used as the boundary is permanently inactive
    public void HighlightItem(bool on)
    {
        if (on)
        {
            //boundary.enabled = true;
            boundary.color = onCol;
        }
        else
        {
            //boundary.enabled = false;
            boundary.color = defaultCol;
        }
    }

    //left click uses the item or drops it if not over anything
    //this returns the inventoryitem so the calling class (e.g. interact class if clicked on a scene object or inventorybox class if clicked on another inventory item) can react
    //note that the inventoryBox is not nulled when an item is dropped, so this could send outdated info if called when inactive
    //so must first check the carrieditem is active and not being dropped
    public InventoryItemSO GetCarriedItem()
    {
        if (IsCarried())
        {
            return inventoryBox.item;
        }
        else
        {
            return null;
        }
    }

    public InventoryBox GetInventoryBox()
    {
        if (IsCarried())
        {
            return inventoryBox;
        }
        else
        {
            return null;
        }
    }

    //right click puts the item back into the inventory
    //or the interact class drops the item after trying and failing to use it
    public void DropItem()
    {
        //stop responding to clicks
        inventory.chapter.ReceiveInputs(false);

        inventory.UpdateTooltip("");

        isDropping = true;
        droppingAnimation = DropAnimation();
        StartCoroutine(droppingAnimation);
    }
   

    IEnumerator DropAnimation()
    {
        Vector3 startpos = transform.localPosition;
        Vector3 endpos = localPos;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f; //half a second animination
            transform.localPosition = Vector3.Lerp(startpos, endpos, Mathf.Sin(t * Mathf.PI / 2f));
            yield return null;
        }

        Deactivate();
        isDropping = false;
        //Debug.Log("itemName: " + itemName.ToString() + " dropped");

        //start responding to clicks
        inventory.chapter.ReceiveInputs(true);
    }

    public void UpdateInventoryTooltip(string t = "...")
    {
        inventory.UpdateTooltip("Use " + itemName + " with " + t);
        //HighlightItem(t != "..."); //highlight the boundary if it's over something usable
    }

    public bool IsCarried()
    {
        return (isActive && !isDropping);
    }

    public bool IsActive()
    {
        return isActive;
    }

    public bool IsDropping()
    {
        return isDropping;
    }

    private void LateUpdate()
    {
        if (isActive && !isDropping)
        {
            Vector3 mousePos = Input.mousePosition;            
            Vector3 offset = new Vector3(40f, -40f, transform.localPosition.z);

            //work out the new local position for the carried item graphic, with small offsets to have it below and to the right of the mouse cursor
            //the background of the inventory is moved down 315 units (y=-315), so offset that in the y direction as well
            transform.localPosition = new Vector3(mousePos.x - Screen.width / 2f + offset.x, mousePos.y - Screen.height / 2f + offset.y + 315f, offset.z);
            if (Input.GetMouseButtonDown(0))//left click uses item, but Interact and InventoryBox classes handle that - this class does nothing on left click
            {                
            }
            if (Input.GetMouseButtonDown(1))//right click to drop
            {
                DropItem();
            }
        }
    }
}
