using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


//A chapter is a self contained section of the game containing multiple locations, each location having interactable objects
//all locations must be children of the chapter, and all interactables must be children of their location
public class Chapter : MonoBehaviour
{
    [HideInInspector]
    public GameController gc; //refernce to the gamecontroller
    [HideInInspector]
    public GameController cutscenes; //references to any cutscenes contained in separate unity scenes
    [HideInInspector]
    public Tooltip tooltip; //refernce to the tooltip
    public string inventorySceneName; //a chapter refers to a separate inventory scene for managing picked up items    
    [HideInInspector]
    public Inventory inventory;
    private Location[] locationArray; //array of all locations
    private Location startLocation; //a single location where the chapter starts
    private Location currentLocation; //the currently active location
    private RectTransform uiFaderPanel; //the fade in/out overlay
    
    //coroutines for loading chapter assets
    private IEnumerator initLocationsCoroutine;
    private IEnumerator initInventoryCoroutine;
    
    [HideInInspector]
    public bool isInitialised = false; //is the chapter initialised
    [HideInInspector]
    public bool receivingInputs = false; //is the chapter responding to mouse clicks? Used to prevent interaction during animations

    public CutsceneReference startingCutscene; //an optional cutscene to open the chapter

    private void Start()
    {
        //if there's no gamecontroller, load the GC scene. Used to simplify testing so you can just run the game with the chapter scene open
        if (FindObjectOfType<GameController>() == null)
        {
            SceneManager.LoadScene(0, LoadSceneMode.Additive);
        }
         
    }

    /// <summary>
    /// method for initialising the chapter, including the inventory and the chapter's locations
    /// </summary>
    /// <param name="_gc">reference to the gamecontroller</param>
    public void InitialiseChapter(GameController _gc)
    {
        //stop player interacting until setup complete
        receivingInputs = false;

        //Debug.Log("Initialising Chapter");
        gc = _gc;
        //tooltip = gc.tooltip;
        //uiFaderPanel = gc.fadeToColour;

    
        initInventoryCoroutine = InitialiseInventoryCoroutine();
        StartCoroutine(initInventoryCoroutine);
    }

    /// <summary>
    /// Asynchronously loads and initialises the inventory scene identified by inventorySceneName
    /// </summary>    
    IEnumerator InitialiseInventoryCoroutine()
    {
        //if the inventory scene isn't already loaded, load it
        if (!SceneManager.GetSceneByName(inventorySceneName).isLoaded)
        {
            Debug.Log("loading inventory scene");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(inventorySceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        //find the first Inventory object in the new scene, select it as the chapter's inventory and initialise it
        GameObject[] goList = SceneManager.GetSceneByName(inventorySceneName).GetRootGameObjects();
        foreach (GameObject go in goList)
        {
            Debug.Log("finding inventory object");
            if ((inventory = go.GetComponent<Inventory>()) != null)
            {                
                break;
            }
        }
        inventory.Initialise(this);
        InitialiseLocations();
    }

    /// <summary>
    /// starts the InititialiseLocationsCoroutine coroutine once the inventory is initialised
    /// </summary>
    private void InitialiseLocations()
    {
        initLocationsCoroutine = InititialiseLocationsCoroutine();
        StartCoroutine(initLocationsCoroutine);
    }

    /// <summary>
    /// Initialise the locations in a coroutine to be able to update the progress bar as each location gets initialised
    /// </summary>
    /// <returns></returns>
    IEnumerator InititialiseLocationsCoroutine()
    {
        //find all locations beneath the chapter, including inactive ones
        //"include inactive = true" is used because it may be easier to deactivate locations when not edititing them
        //If a location is not intended to be included in the chapter, it must be moved so it is no longer a child of the chapter object
        locationArray = GetComponentsInChildren<Location>(true);
        startLocation = null;
        for (int i = 0; i < locationArray.Length; i++)            
        {
            Location location = locationArray[i];
            //activate any inactive locations
            if (!location.isActiveAndEnabled)
            {
                //Debug.Log("activating " + location.name);
                location.gameObject.SetActive(true);
            }
            //Initialise all locations and identify the starting location (InitialiseLocation returns true if it is the starting location)
            if (location.InitialiseLocation(this))
            {
                //check that there's only one starting location
                if (startLocation != null)
                {
                    Debug.LogError("Error - more than one starting location: " + startLocation.name + " and " + location.name);
                }
                else
                {
                    startLocation = location;
                }
            }            

            //gc.UpdateLoadingPanel((float)(i + 1f) / locationArray.Length);
            yield return new WaitForSeconds(0.1f); //can change this to "yield return null;" for near instantaneous loading
        }

        //if no startLocation assigned, make the first found location the startLocation
        if (startLocation == null)
        {
            Debug.LogError("Error - no starting location. Setting it to " + locationArray[0].name);
            startLocation = locationArray[0];
        }

        isInitialised = true;
        ChapterInitComplete();
    } 

    /// <summary>
    /// start the game or check for a starting cutscene once chapter initialisation completed
    /// </summary>
    public void ChapterInitComplete()
    {
        //Debug.Log("ChapterInitComplete: " + isInitialised.ToString());
        currentLocation = startLocation; 
        //tell the GC to close the menu and loading bar
        //gc.ChapterInitComplete();

        //play the startingCutscene if there is one
        if (cutscenes != null && startingCutscene != null)
        {
            startingCutscene.CutsceneLoadFlowTrigger();
        }
        else //if there's no startingCutscene, just start the game
        {
            ReceiveInputs(true);
            ActivateCurrentLocation(true);
        }
    }

    /// <summary>
    /// returns the private field current location
    /// </summary>
    public Location GetCurrentLocation()
    {
        return currentLocation;
    }

    /// <summary>
    /// update the current location, usually when navigating using interactables in each location
    /// </summary>
    /// <param name="newLoc"></param>
    public void UpdateCurrentLocation(Location newLoc)
    {        
        currentLocation = newLoc;        
    }

    /// <summary>
    /// Activate/deactivate the current location and the inventory
    /// </summary>    
    public void ActivateCurrentLocation(bool active)
    {
        currentLocation.ActivateLocation(active);
        inventory.OpenInventory(active);
    }

    /// <summary>
    /// Show some text in the description box at the top of the screen. 
    /// </summary>
    /// <param name="txt"></param>
    public void ShowDescription(string txt)
    {
        //gc.tooltip.ActivateDescription(txt);
    }    

    /// <summary>
    /// activate/deactivate the fader overlay
    /// </summary>    
    public void ActivateUIFader(bool active)
    {                                
        uiFaderPanel.gameObject.SetActive(active);
        //if the fader is being turned off, then start receiving inputs, and vice versa    
        ReceiveInputs(!active);
    }

    /// <summary>
    /// Select the colour of the overlay
    /// </summary>    
    public void SetUIFaderColour(Color color)
    {
        uiFaderPanel.GetComponent<Image>().color = color;
    }

    /// <summary>
    /// update the alpha value of the overlay to fade it in or out
    /// </summary>    
    public void SetUIFaderAlpha(float alpha)
    {
        Color color = uiFaderPanel.GetComponent<Image>().color;
        color.a = alpha;
        uiFaderPanel.GetComponent<Image>().color = color;
    }

    /// <summary>
    /// update whether the chapter is receiving and responding to clicks or not.
    /// Turned off during animations or when moving to cutscenes, for example
    /// </summary>    
    public void ReceiveInputs(bool receive)
    {
        //Debug.Log("receiving inputs: " + receive.ToString());
        receivingInputs = receive;
    }

}
