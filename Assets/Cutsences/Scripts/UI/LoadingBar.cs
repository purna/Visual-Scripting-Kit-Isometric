using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public RectTransform fill;
    public RectTransform loadingBar;
    
    public void UpdateProgress(float progress)
    {
        Vector2 barSize = loadingBar.sizeDelta;
        Vector2 fillSize = new Vector2(progress * barSize.x, 0f);
        //Debug.Log(progress.ToString() + " " + barSize.x.ToString() + " " + (progress * barSize.x).ToString());
        fill.sizeDelta = fillSize;
    }

}
