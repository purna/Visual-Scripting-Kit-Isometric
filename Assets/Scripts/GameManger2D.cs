using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TarodevController
{
    public class GameManager2D : MonoBehaviour
    {
        public int coinsCounter = 0;

        [SerializeField] private ScriptableStats _stats;

        public GameObject playerGameObject;
        private PlayerController _player;
        public GameObject deathPlayerPrefab;
        public Text coinText;

        private Color originalColor; // To store the original color
        private float fadeDuration = 2f; // Time in seconds to fade to red
        private float fadeTimer = 0f; // Timer to track how long the fade has been happening

        void Start()
        {
             _player = GetComponent<PlayerController>();

        }

        void Update()
        {
            coinText.text = coinsCounter.ToString();



            if (_stats.deathState == true)
            {
                playerGameObject.SetActive(false);

                // Instantiate the deathPlayer prefab
                GameObject deathPlayer = (GameObject)Instantiate(deathPlayerPrefab, playerGameObject.transform.position, playerGameObject.transform.rotation);

                // Match the scale with the player object
                deathPlayer.transform.localScale = new Vector3(playerGameObject.transform.localScale.x, playerGameObject.transform.localScale.y, playerGameObject.transform.localScale.z);

               // Access the child SpriteRenderer in the deathPlayer (assuming the SpriteRenderer is a child of deathPlayer)
                SpriteRenderer deathPlayerSpriteRenderer = deathPlayer.GetComponent<SpriteRenderer>();

               // Check if the SpriteRenderer exists to avoid errors
                if (deathPlayerSpriteRenderer != null)
                {
                    // Store the original color to allow fading back to it later (if needed)
                    originalColor = deathPlayerSpriteRenderer.color;

                     // Gradually fade to red and make the alpha 0 (transparent) using Lerp
                    StartCoroutine(FadeToRedAndTransparent(deathPlayerSpriteRenderer));

                }
                else
                {
                    Debug.LogWarning("No SpriteRenderer found in the child objects of deathPlayer prefab!");
                }
                            _stats.deathState = false;

                // Reload the level after 3 seconds
                Invoke("ReloadLevel", 3);
            }

        }

        // Coroutine to handle the fading process (to red and transparent)
        private IEnumerator FadeToRedAndTransparent(SpriteRenderer spriteRenderer)
        {
            while (fadeTimer < fadeDuration)
            {
                // Increase the fadeTimer
                fadeTimer += Time.deltaTime;

                // Interpolate between the original color and red, and also fade alpha to 0
                float lerpAlpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration); // Fade alpha from 1 to 0

                // Set the color to red and alpha to lerpAlpha (which decreases to 0)
                spriteRenderer.color = Color.Lerp(originalColor, Color.red, fadeTimer / fadeDuration);
                Color newColor = spriteRenderer.color;
                newColor.a = lerpAlpha; // Set the alpha value to fade out
                spriteRenderer.color = newColor;

                // Wait for the next frame
                yield return null;
            }

            // Ensure the color is set to red with zero alpha (fully transparent) after the fade is complete
            spriteRenderer.color = new Color(1f, 0f, 0f, 0f); // Fully transparent red
        }

        private void ReloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex );
        }
    }
}
