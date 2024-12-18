using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    //to add new cursor types, add a new name to the CursorName enum, declare a new public Texture2D and initialise in unity editor
    //then add a line to the Initiliase() method to add it to the cursor dictionary.
    public enum CursorName
    {
        main, look, use, pickup, navigateUp, carryItem, inventory
    }    
    public Texture2D mainCursor;
    public Texture2D lookCursor;
    public Texture2D useCursor;
    public Texture2D pickupCursor;
    public Texture2D navigateCursorUp;
    public Texture2D carryItemCursor;
    public Texture2D inventoryCursor;
    private Dictionary<CursorName, Texture2D> cursors = new Dictionary<CursorName, Texture2D>();
    private Dictionary<CursorName, Vector2> cursorOffsets = new Dictionary<CursorName, Vector2>();
    private bool carryingItem = false;
    private CursorName hiddenCursorChange = CursorName.main; //keeps record of cursor changes that are hidden while carrying an item so it can switch to correct cursor on item drop

    public void Initialise()
    {        
        cursors.Add(CursorName.main, mainCursor);
        cursors.Add(CursorName.look, lookCursor);
        cursors.Add(CursorName.use, useCursor);
        cursors.Add(CursorName.pickup, pickupCursor);
        cursors.Add(CursorName.navigateUp, navigateCursorUp);
        cursors.Add(CursorName.carryItem, carryItemCursor);
        cursors.Add(CursorName.inventory, inventoryCursor);

        cursorOffsets.Add(CursorName.main, Vector2.zero);
        cursorOffsets.Add(CursorName.look, Vector2.zero);
        cursorOffsets.Add(CursorName.use, Vector2.zero);
        cursorOffsets.Add(CursorName.pickup, Vector2.zero);
        cursorOffsets.Add(CursorName.navigateUp, new Vector2(16f, 0f));
        cursorOffsets.Add(CursorName.carryItem, Vector2.zero);
        cursorOffsets.Add(CursorName.inventory, new Vector2(0f, 0f));

        Cursor.SetCursor(cursors[CursorName.main], cursorOffsets[CursorName.main], CursorMode.Auto);
    }

    public void ChangeToCarryCursor(bool activate)
    {
        carryingItem = activate;
        if (activate)
        {
            Cursor.SetCursor(cursors[CursorName.carryItem], cursorOffsets[CursorName.carryItem], CursorMode.Auto);
            hiddenCursorChange = CursorName.main;
        }
        else
        {
            Cursor.SetCursor(cursors[hiddenCursorChange], cursorOffsets[hiddenCursorChange], CursorMode.Auto);            
        }
    }

    public void ChangeCursor(CursorName c)
    {
        if (c == CursorName.carryItem)
        {
            Debug.Log("error, should call ChangeToCarryCursor directly");
            ChangeToCarryCursor(true);
            return;
        }

        if (!carryingItem) //only change cursor if not carrying an inventory item
        {            
            Cursor.SetCursor(cursors[c], cursorOffsets[c], CursorMode.Auto);
        }
        else //but do store the change in case item is dropped
        {
            hiddenCursorChange = c;
        }
    }
    
}
