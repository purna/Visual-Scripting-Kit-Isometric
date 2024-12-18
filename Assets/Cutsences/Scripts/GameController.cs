using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

//single game controller instance for managing other game features
//include the pre-prepared scene "0 GameController" to include this and other important game level objects
//This scene must be scene 0 in the unity build settings
public class GameController : MonoBehaviour
{    
    private Camera gcCamera; //starting camera used for game menu and loading bar
    [HideInInspector]
    public Chapter currentChapter; //reference to the chapter
    
    private IEnumerator loadingCoroutine;
        
    public CursorController cursors; //the different game cursors


    private void Start()
    {
        //initialise important objects 
        gcCamera = gameObject.GetComponent<Camera>();
        gcCamera.depth = 10; //make sure gcCamera is at the front
        gcCamera.enabled = true;
        cursors.Initialise();
    }

}
