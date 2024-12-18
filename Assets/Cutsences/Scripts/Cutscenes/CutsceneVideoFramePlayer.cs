using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneVideoFramePlayer : MonoBehaviour
{
    public List<Sprite> videoFrame = new List<Sprite>();
    private bool playVideo;
    private IEnumerator coroutine;
    private SpriteRenderer rend;

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        //rend.sprite = videoFrame[0];
    }

    public void StartVideo(float frameTime)
    {
        if (playVideo)
        {
            StopVideo();
        }
        playVideo = true;
        coroutine = VideoPlayer(frameTime);
        StartCoroutine(coroutine);
    }

    private IEnumerator VideoPlayer(float frameTime)
    {
        int frame = 0;
        while (playVideo)
        {
            rend.sprite = videoFrame[frame];
            frame++;
            if (frame >= videoFrame.Count)
            {
                frame = 0;
            }
            yield return new WaitForSeconds(frameTime);
        }
    }

    public void PauseVideo()
    {
        playVideo = false;
    }

    public void StopVideo()
    {
        StopCoroutine(coroutine);
        playVideo = false;
        rend.sprite = null;
    }
}
