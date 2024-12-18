using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


//Interactables are anything a player can click on or interact with
//Options include static objects to look at, objects to use, objects to pick up, doorways to pass through to navigate to a different location, etc
[ExecuteAlways] //execute always makes sure to add some of the key components if you create a new interactable from scratch. Alternatively, you can use the interactable prefab as a template
[RequireComponent(typeof(ScriptMachine), typeof(Rigidbody2D))] //every interactable requires a bolt flowmachine and a rigidbody2D
public class BoltInteractable : MonoBehaviour
{
    //key settings for the interactable are initialised in flowmachine macro "Interactble"
    [HideInInspector]
    public string tooltipText = ""; //tooltip text when hovering over interactable
    [HideInInspector]
    public string lookText = ""; //displayed text when looking at interactable    
    [HideInInspector]
    public CursorController.CursorName cursor = CursorController.CursorName.look; //what cursor to use when hovering over the interactable

    //references to other important components
    [HideInInspector]
    public Inventory inventory; 
    [HideInInspector]
    public Chapter chapter; 
    [HideInInspector]
    public Tooltip tooltip; 
    [HideInInspector]
    public CursorController cursors;
    
    [HideInInspector]
    public bool isInitialised = false;
    [HideInInspector]
    public bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.IsPlaying(gameObject))
        {
            //only do this in the unity editor
            return;
        }
        else
        {            
            //make sure the rigidbody2D is set up correctly. This is necessary because the collider object is placed on a child of the interactable for greater control over size, shape, and orientation
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }            
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = true;
        }
    }

    /// <summary>
    /// method for initialising the interactable and triggering the bolt flowmachine
    /// </summary>    
    public void InitialiseInteractable(Chapter _chapter)
    {
        chapter = _chapter;        
        tooltip = chapter.tooltip; 
        cursors = chapter.gc.cursors;
        inventory = chapter.inventory;


        //make sure to move inteactable between background and camera or it might be invisible or impossible to click on
        if (transform.localPosition.z >= 0f || transform.localPosition.z <= -10f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -1f);
        }
        //check whethere there's a rigidbody2d attached to this object (required for mouseover etc events to work when collider on a child)
        //if not, add one
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        //provide rigidbody2d with correct settings
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;

        //make sure objects aren't behind camera
        if (transform.localPosition.z < -10)
        {            
            transform.localPosition.Set(transform.localPosition.x, transform.localPosition.y, -9f);
        }
        isInitialised = true;

        //trigger the bolt flowmachine interactable to complete the initialisation.
        //all the settings for the interactable are placed in the flowmachine
        CustomEvent.Trigger(gameObject, "isInitialised");
    }
    
    /// <summary>
    /// activate or deactive the interactable. Currently redundant as it only calls the private "SetVisible" method, but leaving it like this in case more complex behaviours are required in future
    /// </summary>
    /// <param name="active"></param>
    public void ActivateInteractable(bool active)
    {
        SetVisible(active);
    }

    /// <summary>
    /// Make the interactable visible/invisible
    /// </summary>
    /// <param name="visible"></param>
    private void SetVisible(bool visible)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(visible);
        }
        isActive = visible;
    }

    /// <summary>
    /// checks for clicks while the mouse is over the interactable's collider and responds depending on the context
    /// </summary>
    private void OnMouseOver()
    {
        //don't do anything if interactable isn't initialised
        if (!isInitialised)
        { return; }
        //only if the gamecontroller is receiving clicks
        if (chapter.receivingInputs)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Pressed primary button.");
                //cancel if object is in process of being dropped
                if (chapter.inventory.carriedItem.IsDropping())
                { return; }

                LeftAction();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                //Debug.Log("Pressed secondary button.");
                //cancel if object is in process of being dropped
                if (chapter.inventory.carriedItem.IsDropping())
                { return; }

                RightAction();
            }
        }
    }

    /// <summary>
    /// Checks for the mouse entering the interactable's collider
    /// </summary>
    private void OnMouseEnter()
    {
        if (!isInitialised)
        { return; }
        //Debug.Log(gameObject.name);
        MouseEnterAction();        
    }

    /// <summary>
    /// Checks for the mouse leaving the interactable's collider
    /// </summary>
    private void OnMouseExit()
    {
        if (!isInitialised) { return; }
        //Debug.Log(gameObject.name);
        MouseExitAction();
    }

    /// <summary>
    /// Updates the cursors when the mouse enters the interactable's collider
    /// </summary>
    private void MouseEnterAction()
    {
        //Debug.Log(gameObject.name);
        tooltip.ActivateTooltip(tooltipText);
        if (chapter.inventory.carriedItem.IsActive())
        {
            chapter.inventory.carriedItem.UpdateInventoryTooltip(tooltipText); //if it's an interactable, update the tooltip above the inventory
        }
        else
        {
            cursors.ChangeCursor(cursor);
        }
    }

    /// <summary>
    /// Updates the cursors when the mouse leaves the interactable's collider
    /// </summary>
    private void MouseExitAction()
    {
        //Debug.Log(gameObject.name);
        tooltip.DeactivateTooltip();

        if (chapter.inventory.carriedItem.IsActive())
        {
            chapter.inventory.carriedItem.UpdateInventoryTooltip();
        }
        else
        {            
            cursors.ChangeCursor(CursorController.CursorName.main);
        }        
    }    

    /// <summary>
    ///  method called when the right mouse button is clicked over the interactable. 
    /// </summary>
    private void RightAction()
    {
        //if an item is being carried, the right mouse drops it, so need to make sure the correct cursor is displayed
        if (chapter.inventory.carriedItem.IsActive())
        {
            //Debug.Log(lookText + " | right click (carrying item)");
            MouseExitAction();
            MouseEnterAction();
        }
        //otherwise, just display the lookText for the interactable
        else
        {
            //Debug.Log(lookText + " | right click");
            tooltip.ActivateDescription(lookText);
        }
    }

    /// <summary>
    /// method called when the left mouse button is clicked over the interactable
    /// triggers different events on the bolt flowmachine depending on whether an item is being carried or not
    /// </summary>
    public void LeftAction()
    {
        if (chapter.inventory.carriedItem.IsActive())
        {            
            CustomEvent.Trigger(gameObject, "LeftActionCarrying", inventory.carriedItem.GetCarriedItem());
        }
        else
        {
            CustomEvent.Trigger(gameObject, "LeftAction");
        }
    }    

    /// <summary>
    /// when the interactable's location is entered or left, trigger the relevant events on the bolt flowmachine
    /// may be used to activate or decative repeating sounds or animations
    /// </summary>
    /// <param name="entered"></param>
    public void LocationEntered(bool entered)
    {
        if (entered)
        {
            CustomEvent.Trigger(gameObject, "LocationEntered", isActive);
        }
        else
        {
            CustomEvent.Trigger(gameObject, "LocationLeft", isActive);
        }
    }
}