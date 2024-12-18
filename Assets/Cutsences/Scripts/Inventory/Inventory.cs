using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//the inventory class controls the inventory and any items picked up or carried by the player
//WARNING: Current implementation of inventory will just overflow if you are carrying too many items.    
//Make sure the inventory object and all its children are on the 2inventory" layer, and that the inventory camera only renders the inventory layer
//All other cameras in the project should exclude the inventory layer
//The inventory is contained within its own scene so that multiple chapters will be able to have the same inventory when this is feature implemented
//but also to allow a game to have multiple inventories. e.g. switching between different characters or a separate inventory for each chapter
public class Inventory : MonoBehaviour
{        
    [HideInInspector]
    public Chapter chapter;
    public CursorController cursors;
    public Camera inventoryCamera; //the inventory has its own camera that is enabled/disabled to turn the inventory on/off
    private bool isInitialised = false;

    //1280x720 values - pivots all in centre    
    public const float backgroundDefaultYPos = -315f; //used by CarriedItem.cs
    private const float boxSize = 80f; //size of an inventoryBox
    private const float boxSpacing = 3f; //space between boxes in the inventory
    private const float firstBoxPos = -595f; //location of the first inventory box    

    public Transform background; //the background image
    public TextMeshPro inventoryTooltip; //the tooltip just above the inventory panel
    
    public InventoryBox blankBox; //a prefab to create new inventory items
    [HideInInspector]
    public List<InventoryBox> regularItems = new List<InventoryBox>(); //list of items in the inventory

    public CarriedItem carriedItem; //the item currently being carried

    //various animnations 
    private IEnumerator pickupAnimation;
    private IEnumerator shuffleEnumerator;
    private IEnumerator shuffleAnimation;
    
    private bool isOpen; //is the inventory open?    

    /// <summary>
    /// method for initialising the inventory, but remains closed (inventory camera not enabled) until requested
    /// </summary>
    /// <param name="_chapter"></param>
    public void Initialise(Chapter _chapter)
    {
        Debug.Log("initialising inventory");
        Activate(true);        
        
        chapter = _chapter;
        cursors = chapter.gc.cursors;
        inventoryCamera = GetComponentInChildren<Camera>();
        OpenInventory(false);

        UpdateTooltip("");
        carriedItem.Initialise(this);        

        isInitialised = true;
    }

    /// <summary>
    /// Activates the inventory by enabling all the child objects
    /// </summary>
    /// <param name="on"></param>
    public void Activate(bool on)
    {
        foreach (Transform child in transform)
        {
            //Debug.Log(child.name);
            child.gameObject.SetActive(on);
        }        
    }

    /// <summary>
    /// opens the inventory by enabling the inventory camera, or vice versa
    /// </summary>
    /// <param name="open"></param>
    public void OpenInventory(bool open)
    {
        inventoryCamera.enabled = open;
        isOpen = open;
    }

    /// <summary>
    /// changes the cursor if the mouse enters the inventory's collider
    /// </summary>
    public virtual void OnMouseEnter()
    {
        if (!isInitialised)
        { return; }        
        cursors.ChangeCursor(CursorController.CursorName.main);
    }

    /// <summary>
    /// changes the cursor if the mouse leaves the inventory's collider
    /// </summary>
    private void OnMouseExit()
    {
        if (!isInitialised)
        { return; }        
        cursors.ChangeCursor(CursorController.CursorName.main);
    }

    /// <summary>
    /// method for updaing the inventory tooltip text
    /// </summary>
    /// <param name="txt"></param>
    public void UpdateTooltip(string txt)
    {
        inventoryTooltip.text = txt;
    }

    /// <summary>
    /// adds an inventoryitem to the inventory
    /// </summary>
    /// <param name="item">the InventoryItemSO object to add</param>
    public void AddToInventory(InventoryItemSO item)
    {
        //stop responding to clicks during animation
        chapter.ReceiveInputs(false);

        //get starting position of item for animation
        Vector3 startpos = new Vector3(Input.mousePosition.x - Screen.width / 2f, Input.mousePosition.y - Screen.height / 2f - backgroundDefaultYPos, -1f);

        //Instantiate and name a blank prefab for the inventory item 
        GameObject newItem = Instantiate(blankBox.gameObject,background);
        newItem.name = item.itemDisplayName;
        //get the new blank InventoryBox from the prefab and initialise it
        InventoryBox newItemBox = newItem.GetComponent<InventoryBox>();        
        newItemBox.Initialise(this, item);
        //add the new item to the list
        regularItems.Add(newItemBox);

        //update the inventoryitem to track where it's being held
        item.UpdateAfterPickup(newItemBox); 

        //where is the item going to end up in the inventory?
        Vector3 endpos = GetItemPosition(regularItems.Count - 1);

        //run the animation to fly the object into the inventory
        pickupAnimation = AnimateAddToInventory(regularItems.Count - 1, startpos, endpos);
        StartCoroutine(pickupAnimation);
    }

    /// <summary>
    /// method for working out the co-ordinates of where an item should go in the inventory
    /// </summary>    
    public Vector3 GetItemPosition(int itemNum)
    {
        Vector3 pos = new Vector3(firstBoxPos + (boxSize + boxSpacing) * (itemNum), 0f, -1f);        
        return pos;
    }
    
    IEnumerator AnimateAddToInventory(int itemNum, Vector3 startpos, Vector3 endpos)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 1.5f;
            regularItems[itemNum].transform.localPosition = Vector3.Lerp(startpos, endpos, Mathf.Sin(t * Mathf.PI / 2f));
            yield return null;
        }

        regularItems[itemNum].transform.localPosition = endpos;
        regularItems[itemNum].isActive = true;
        //Debug.Log("itemNum: " + itemNum.ToString() + " at pos: " + endpos.ToString());

        //start responding to clicks
        chapter.ReceiveInputs(true);
    }
    
    /// <summary>
    /// does the inventory contain the given item?
    /// </summary>    
    public bool InventoryContainsItem(InventoryItemSO item)
    {
        foreach (InventoryBox box in regularItems)
        {
            if(box.item==item)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// removes an item from the inventory    
    /// </summary>    
    public void RemoveFromInventory(InventoryBox item)
    {
        chapter.ReceiveInputs(false);
        carriedItem.Deactivate();
        regularItems.Remove(item);
        Destroy(item.gameObject);

        //shuffle items along
        shuffleEnumerator = ShuffleInventory();
        StartCoroutine(shuffleEnumerator);
        
    }

    /// <summary>
    /// nice animation for shuffling items in the inventory along if one is removed
    /// </summary>
    /// <returns></returns>
    IEnumerator ShuffleInventory()
    {
        for(int i=0; i<regularItems.Count; i++)
        {
            if (regularItems[i].transform.localPosition != GetItemPosition(i)) 
            {
                shuffleAnimation = AnimateShuffleInventory(i, regularItems[i].transform.localPosition, GetItemPosition(i));
                StartCoroutine(shuffleAnimation);
            }
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        //start responding to clicks
        chapter.ReceiveInputs(true);
    }

    IEnumerator AnimateShuffleInventory(int itemNum, Vector3 startpos, Vector3 endpos)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            regularItems[itemNum].transform.localPosition = Vector3.Lerp(startpos, endpos, Mathf.Sin(t * Mathf.PI / 2f));
            yield return null;
        }

        regularItems[itemNum].transform.localPosition = endpos;        
        
    }
}
