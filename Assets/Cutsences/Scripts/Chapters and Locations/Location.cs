using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//every Location is a single screen in the game with its own camera
//It must be a child of a Chapter object
//The children of the Location are interactables objects
public class Location : MonoBehaviour
{    
    private Chapter chapter; //reference to the chapter
    //refernces to the parts of the crossfader object for fading from one location to another
    private RectTransform crossfader; 
    private RawImage crossfadeImg;
    private CanvasGroup crossfadeCanvas;    
    private IEnumerator fadeCoroutine;
    
    private IEnumerator shakeCoroutine;//a classic shake animation for the location

    [HideInInspector]
    public bool fading = false; //is the location fading in/out
    [HideInInspector]
    public Camera cam; //the location's camera
    [HideInInspector]
    public bool isInitialised; //is the location initialised?
    
    public bool startLocation; //is the location the Chapter's starting location - there must be only one startLocation in the chapter

    private void Start()
    {        
        isInitialised = false;
        cam = GetComponentInChildren<Camera>();
        //force camera to be 10 units back so that it can see all interactables
        cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, cam.transform.localPosition.y, -10f);
    }

    /// <summary>
    /// method for initialising the location, including the location's interactables
    /// </summary>    
    /// <returns>returns true if it is the startingLocation</returns>
    public bool InitialiseLocation(Chapter _chapter)
    {
        //Debug.Log("Initialising Location " + gameObject.name); 
        chapter = _chapter;
        
        
        //activate and initialise all interactables by searching for BoltInteractable components even if they're inactive
        //the initialisation method then deactivates any interactables that aren't intended to be visible at the start of the game
        foreach (BoltInteractable boltInteractable in GetComponentsInChildren<BoltInteractable>(true))
        {
            if (!boltInteractable.isActiveAndEnabled)
            {
                //Debug.Log("activating " + interactable.name);
                boltInteractable.gameObject.SetActive(true);
            }
            boltInteractable.InitialiseInteractable(chapter);            
        }

        ActivateLocation(false);

        isInitialised = true;
        return startLocation;
    }
    
    /// <summary>
    /// method to jump to this location with a cross fade
    /// </summary>
    /// <param name="sourceLocation">the source location that is currently active</param>
    /// <param name="fadeTime">how long should the crossfade take. 1 second is good. <=0 gives a snap rather than a fade</param>
    public void JumpTo(Location sourceLocation, float fadeTime)
    {
        //stop responding to clicks
        chapter.ReceiveInputs(false);
        //tell the chapter object that this is the new "current location"
        chapter.UpdateCurrentLocation(this);

        //if fadeTime<=0, then snap to new destination, otherwise fade
        if (fadeTime <= 0)
        {
            //enable camera on this location (the destination)
            ActivateLocation(true);
            //swap the camera depths
            sourceLocation.cam.depth = 0;
            cam.depth = 1;
            //disable source camera
            sourceLocation.ActivateLocation(false);
            //start responding to clicks
            chapter.ReceiveInputs(true);
        }
        else
        {
            fadeCoroutine = CrossFade(sourceLocation, fadeTime);
            StartCoroutine(fadeCoroutine);
        }

    }

    /// <summary>
    /// method to jump FROM this location to a given destination location, with crossfade
    /// </summary>
    /// <param name="destinationLocation">the destination to jump to</param>
    /// <param name="fadeTime">how long should the crossfade take. 1 second is good. <=0 gives a snap rather than a fade</param>
    public void JumpToDestination(Location destinationLocation, float fadeTime)
    {        
        destinationLocation.JumpTo(this, fadeTime);
    }

    /// <summary>
    /// IEnumerator for animating the crossfade
    /// </summary>        
    private IEnumerator CrossFade(Location sourceLocation, float fadeTime)
    {
        //Debug.Log("CrossFade Coroutine");
        crossfader.gameObject.SetActive(true);

        ActivateLocation(true);
        Camera camSrc = sourceLocation.cam;
        Camera camDest = cam;
        camSrc.depth = 1;
        camDest.depth = 0;

        RenderTexture renderTex = new RenderTexture(Screen.width, Screen.height, 24);
        camSrc.targetTexture = renderTex;
        renderTex.Create();
        crossfadeImg.texture = renderTex;

        camSrc.depth = 0;
        camDest.depth = 1;

        //check for errors in fadeTime, though this 
        if (fadeTime <= 0) //this coroutine shouldn't even be called with fT<=0, but just in case...
        {
            fadeTime = 1;
        }
        if (fadeTime > 10) //check and correct stupidly long fadeTimes
        {
            Debug.Log("Did you really mean a fadeTime of " + fadeTime.ToString() + " seconds? Shortened to 10 seconds");
            fadeTime = 10;
        }

        float t = 1;
        while (t > 0)//fade out tex.
        {
            t -= Time.deltaTime / fadeTime;
            if (t < 0)
            {
                t = 0;
            }
            crossfadeCanvas.alpha = t;
            yield return null;
        }

        camSrc.targetTexture = null;
        renderTex.Release();

        sourceLocation.ActivateLocation(false);
        crossfader.gameObject.SetActive(false);

        //start responding to clicks
        chapter.ReceiveInputs(true);
    }

    /// <summary>
    /// activate or deactivate this location. 
    /// Calls an event on each interactable to tell it that the parent location is active/inactive
    /// May be useful if, for example, interactables are animated or produce sounds that you only wanting playing while the location is active
    /// </summary>
    public void ActivateLocation(bool active)    
    {        
        EnableCamera(active);
        foreach (BoltInteractable boltInteractable in GetComponentsInChildren<BoltInteractable>(true))
        {
            boltInteractable.LocationEntered(active);
        }
    }        

    /// <summary>
    /// activates/deactivates the location's camera to make it visible
    /// Should only be one location camera active at a time except during crossfades
    /// </summary>
    /// <param name="enable"></param>
    private void EnableCamera(bool enable)
    {
        cam.enabled = enable;
    }

    //check for clicks during the update phase
    //The check at the location level checks for adding or removing tooltips
    //Each interactable checks for clicks on itself separately
    private void Update()
    {
        if (isInitialised)
        {
            if (chapter.GetCurrentLocation() == this)
            {
                //if clicked middle mouse button, activate hints (or deactivate if already active)
                //and deactivate any description text
                if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.Space))
                {
                    chapter.tooltip.ActivateHints(GetComponentsInChildren<BoltInteractable>());
                    chapter.tooltip.DeactivateDescription();
                }
                //if clicked other mouse buttons, deactivate hints and description text
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {                    
                    chapter.tooltip.DeactivateHints();
                    chapter.tooltip.DeactivateDescription();
                }
            }
        }
    }
   
    /// <summary>
    /// method for changing the background image of the location
    /// </summary>
    /// <param name="newBackground"></param>
    public void ChangeBackground(Sprite newBackground)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = newBackground;
    }

    /// <summary>
    /// method for shaking the location camera - A CASSIC! ;)
    /// </summary>
    /// <param name="vertical">vertical shake amount: 10 to 20 is good</param>
    /// <param name="horizontal">horizontal shake amount: 10 to 20 is good</param>
    /// <param name="duration">How long the shake should last - don't overdo it.</param>
    public void ShakeLocation(int vertical, int horizontal, float duration)
    {
        shakeCoroutine = ShakeLocationAnimation(vertical, horizontal, duration);
        StartCoroutine(shakeCoroutine);
    }

    /// <summary>
    /// Ienumerator for animatin the location shake
    /// </summary>    
    private IEnumerator ShakeLocationAnimation(int vertical, int horizontal, float duration)
    {        
        
        Vector3 startPos = cam.transform.localPosition;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            
            cam.transform.localPosition = startPos + new Vector3(Random.Range(-horizontal, horizontal), Random.Range(-vertical, vertical), 0f);
            yield return null;
        }
        cam.transform.localPosition = startPos;
        
    }

}
