using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


// class for handling tooltips when hovering over items, when revealing all interactables, and to handle the description box and text at the top of the screen
//Make sure the Tooltip object and all its children are on the "Tooltips" layer, and that the tooltip camera only renders the tooltip layer.
//All other cameras should exclude the tooltips layer.
public class Tooltip : MonoBehaviour
{
    //private GameController gc;

    //the single tooltip when hovering over interactables and items
    public Transform tooltipHolder;
    public TextMeshPro tooltipText;    
    private RectTransform tooltipRect;
    private bool floatingTooltipActive;
    private const float tooltipFontSize = 30f;

    //multiple hints when showing all interactable objects
    private List<Transform> hintTooltips = new List<Transform>();

    //the description box and text
    public RectTransform descriptionHolder;
    public TextMeshPro descriptionText;
    public bool descriptionActive;
    private int descriptionActivatedFrame;
    private const float descriptionFontSize = 50f; //probably not necessary as the TMP component uses "auto size".
    public GameObject clickToCont;

    public void Initialise(GameController _gc)
    {
        //gc = _gc;

        //Debug.Log("Initialising Tooltip");
        tooltipRect = tooltipText.GetComponent<RectTransform>();
        tooltipText.text = "";
        tooltipText.fontSize = tooltipFontSize;

        tooltipText.gameObject.SetActive(false);
        floatingTooltipActive = false;

        descriptionText.fontSize = descriptionFontSize;

        descriptionHolder.GetComponent<SpriteRenderer>().enabled = false;
        descriptionText.gameObject.SetActive(false);
        descriptionHolder.gameObject.SetActive(false);
        descriptionActive = false;
    }

    public void ActivateTooltip(string txt)
    {
        tooltipText.gameObject.SetActive(true);
        tooltipText.text = txt;
        floatingTooltipActive = true;
    }

    public void DeactivateTooltip()
    {
        tooltipText.text = "";
        tooltipText.gameObject.SetActive(false);
        floatingTooltipActive = false;
    }

    /// <summary>
    /// activates multiple tooltips to show all interactables in the location
    /// Receives an array of all interactables from the Location, but only shows text for active interactables that have a tooltipText defined
    /// </summary>
    public void ActivateHints(BoltInteractable[] interactables)
    {
        //if the hints already exist, destroy them and return 
        //results in a second middle mouse button click or space bar removing the hints
        if (hintTooltips.Count > 0)
        {
            //Debug.Log("deactivating hints");
            DeactivateHints();
            return;
        }
        //Debug.Log("activating hints");
        foreach (BoltInteractable currentInteractable in interactables)
        {
            //if it's currently active in the scene, and if the tooltip text isn't blank
            if (currentInteractable.isActive && !(currentInteractable.tooltipText == "" || currentInteractable.tooltipText == " " || currentInteractable.tooltipText == null))
            {
                //instantiate new tooltip holders and add to list
                GameObject newHintHolder = Instantiate(tooltipHolder.gameObject, currentInteractable.transform.localPosition, Quaternion.identity, transform);
                newHintHolder.name = currentInteractable.tooltipText + " hint";
                hintTooltips.Add(newHintHolder.GetComponent<Transform>());
                hintTooltips[hintTooltips.Count - 1].localPosition = currentInteractable.transform.localPosition;
                hintTooltips[hintTooltips.Count - 1].GetChild(0).gameObject.SetActive(true);
                hintTooltips[hintTooltips.Count - 1].GetChild(0).GetComponent<TextMeshPro>().text = currentInteractable.tooltipText;
                //adjust xpivot tooltip so that the text for interactables at far left or far right of screen are shifted across
                float xPivot = Mathf.Pow(2f * hintTooltips[hintTooltips.Count - 1].localPosition.x / Screen.width, 3) / 2f + 0.5f;
                //ypivot is normally central unless near the top of the screen
                float yPivot = 0.5f;
                if ((hintTooltips[hintTooltips.Count - 1].localPosition.y + Screen.height / 2f) > 680f)
                {
                    yPivot = 1f;
                }
                hintTooltips[hintTooltips.Count - 1].GetChild(0).GetComponent<RectTransform>().pivot = new Vector2(xPivot, yPivot);
            }
        }
    }

    public void DeactivateHints()
    {
        //destroy tooltips in list and clear list
        foreach(Transform tooltip in hintTooltips)
        {
            Destroy(tooltip.gameObject);            
        }
        hintTooltips.Clear();
    }

    //activate description text
    public void ActivateDescription(string txt)
    {
        //do nothing if no string or empty string
        if (txt == "" || txt == " " || txt == null)
        { return; }

        //reset frame count to avoid description being shown and removed in same frame
        descriptionActivatedFrame = Time.frameCount;
        //deactivate any existing text first
        if (descriptionActive)
        {
            DeactivateDescription();
        }
        //Debug.Log("activating description");
        descriptionHolder.gameObject.SetActive(true);
        descriptionText.gameObject.SetActive(true);
        descriptionText.text = txt;

        descriptionHolder.GetComponent<SpriteRenderer>().enabled = true;

        descriptionActive = true;        
    }

    //deactivate description text
    public void DeactivateDescription()
    {
        //do nothing if no description or if it was only activated this frame
        if (descriptionActivatedFrame == Time.frameCount || descriptionActive==false)
        { return; }

        //Debug.Log("deactivating description");
        descriptionText.text = "";
        descriptionText.gameObject.SetActive(false);
        descriptionHolder.GetComponent<SpriteRenderer>().enabled = false;
        descriptionHolder.gameObject.SetActive(false);
        descriptionActive = false;
    }

    private void LateUpdate()
    {
        if (floatingTooltipActive)
        {
            Vector3 mousePos = Input.mousePosition;            
            tooltipHolder.localPosition = new Vector3(mousePos.x - Screen.width / 2f, mousePos.y - Screen.height / 2f, -1f);
            //adjust xpivot so that tooltips at far left or far right of screen are shifted across
            float xPivot = Mathf.Pow((2f * mousePos.x / Screen.width) - 1f, 3) / 2f + 0.5f;
            //ypivot is normally 0 unless near the top of the screen
            float yPivot = 0f;
            if (mousePos.y > 680f)
            {
                yPivot = 3.5f;
            }
            tooltipRect.pivot = new Vector2(xPivot, yPivot);
        }
        
    }

}
