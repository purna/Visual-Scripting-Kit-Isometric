using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Conversation")]
public class CutsceneConversationSO : ScriptableObject
{
    public List<string> charactersList = new List<string>(); //strings formatted for how you want the name to be displayed by TMPro
    public List<DialogueItem> dialogueList = new List<DialogueItem>();    

    [System.Serializable]
    public class DialogueItem
    {
        public int charactersListIndex = -1;
        [TextArea(2,3)]
        public string dialogue; //string formatted for how you want the text to be displayed by TMPro
    }
}
