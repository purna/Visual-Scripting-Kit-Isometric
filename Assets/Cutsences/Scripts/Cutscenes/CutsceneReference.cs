using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

//References and cutscene. Add it as a component to a child object of the CutsceneController
//The name of the object must be the name of the Unity scene that contains the cutscene
[RequireComponent(typeof(ScriptMachine))]
public class CutsceneReference : MonoBehaviour
{
    [HideInInspector]
    public string cutsceneName; //name the gameobject with the correct cutscene scene name
    private GameController controller;

    private void Start()
    {                
        cutsceneName = gameObject.name;

    }

    /// <summary>
    /// Triggers the loadCutscene event in "CutsceneReferenceStartAndEndFlow" and "CutsceneReferenceSimplePlayer". Normally called by a InteractableGoToCutscene flow macro
    /// </summary>
    public void CutsceneLoadFlowTrigger()
    {
        CustomEvent.Trigger(gameObject, "loadCutscene");        
    }

    /// <summary>
    /// Triggers the endCutscene event in "CutsceneReferenceStartAndEndFlow" and "CutsceneReferenceSimplePlayer". 
    /// Normally called by the CutsceneController once the cutscene has finished 
    /// </summary>
    public void CutsceneEndFlowTrigger()
    {
        CustomEvent.Trigger(gameObject, "endCutscene");
    }    

   

    /// <summary>
    /// Triggers the cutsceneLoaded event in "CutsceneReferenceStartAndEndFlow" and "CutsceneReferenceSimplePlayer". 
    /// Normally called once the CutsceneController has finished loading the cutscene
    /// </summary>
    public void CutsceneLoadedFlowTrigger()
    {
        CustomEvent.Trigger(gameObject, "cutsceneLoaded");
    }

  
    
}

    
