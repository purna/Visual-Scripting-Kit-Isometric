
using UnityEngine;
using UnityEngine.SceneManagement;

    public class LoadSceneButton : MonoBehaviour
    {
        public static GameFlowManager gameFlowManager;
        
        public string sceneName = "";

        public void LoadScene()
        {
            SceneManager.LoadScene(sceneName);
        }



        public void LoadPreviousScene()
        {
            string previousScene = EventManager.GetPreviousScene();
            if (!string.IsNullOrEmpty(previousScene))
            {
                SceneManager.LoadScene(previousScene);
            }
            else
            {
                LoadScene();
            }
        }
}
