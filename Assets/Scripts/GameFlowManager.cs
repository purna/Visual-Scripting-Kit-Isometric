using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


    // The Root component for the game.
    // It sets the game state and broadcasts events to notify the different systems of a game state change.

    public class GameFlowManager : MonoBehaviour
    {
        public static EventManager eventManager;

        [Header("Win")]
        [SerializeField, Tooltip("The name of the scene you want to load when the game is won.")]
        string m_WinScene = "Menu Win";
        [SerializeField, Tooltip("The delay in seconds between the game is won and the win scene is loaded.")]
        float m_WinSceneDelay = 5.0f;

        [Header("Lose")]
        [SerializeField, Tooltip("The name of the scene you want to load when the game is lost.")]
        string m_LoseScene = "Menu Lose";
        [SerializeField, Tooltip("The delay in seconds between the game is lost and the lose scene is loaded.")]
        float m_LoseSceneDelay = 3.0f;

        [SerializeField, HideInInspector, Tooltip("The delay in seconds until we activate the controller look inputs.")]
        float m_StartGameLockedControllerTimer = 0.3f;

        public static string PreviousScene { get; private set; }

        public bool GameIsEnding { get; private set; }

        float m_GameOverSceneTime;
        string m_GameOverSceneToLoad;

        //CinemachineFreeLook m_FreeLookCamera;

        string m_ControllerAxisXName;
        string m_ControllerAxisYName;

        void Awake()
        {
            
        }

        void Start()
        {
        // Subscribe to the GameOverEvent
        EventManager.AddListener<GameOverEvent>(OnGameOver);
        }


    void Update()
        {
            if (GameIsEnding)
            {
                if (Time.time >= m_GameOverSceneTime)
                {

                    EventManager.SetPreviousScene(SceneManager.GetActiveScene().name);

                    SceneManager.LoadScene(m_GameOverSceneToLoad);
                }
            }
        }

        void OnGameOver(GameOverEvent evt)
        {
            if (!GameIsEnding)
            {
                GameIsEnding = true;

                // Remember the scene to load and handle the camera accordingly.
                if (evt.Win)
                {
                    m_GameOverSceneToLoad = m_WinScene;
                    m_GameOverSceneTime = Time.time + m_WinSceneDelay;
                }
                else
                {
                    m_GameOverSceneToLoad = m_LoseScene;
                    m_GameOverSceneTime = Time.time + m_LoseSceneDelay;

                    
                }
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<GameOverEvent>(OnGameOver);
        }

       
    }

