using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

//Plays a cutscene. 
//IMPORTANT: Add it as a component on a ROOT gameobject in the unity scene containing the cutscne, preferably the first root object
//Even better, have the CutscenePlayer as the only root object (except for the Bolt "Scene Variables" object) and put all other scene objects as chilcren of the player.
//The cutscene player should also probably implement a bolt state machine. The CutsceneConversation bolt macros are written based on this assumption
public class CutscenePlayer : MonoBehaviour
{
    //private CutsceneController controller;    

    private GameController controller;
    public string cutsceneName;        

    public void StartCutscene(GameController _controller, string _cutsceneName)
    {
        controller = _controller;
        cutsceneName = _cutsceneName;        
        //CustomEvent.Trigger(gameObject, "playCutscene");
    }

    /// <summary>
    /// called this method (eg from a bolt statemachine) when the cutscene ends to go back to the game
    /// </summary>
    public void EndCutscene()
    {        
        //controller.EndCutscene(cutsceneName);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }    

}
