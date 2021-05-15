using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsUI = null;
    [SerializeField] private float fpsRefreshPeriod = 0.5f;
    private int frameCount = 0;
    private float nextFPSUpdateTime = 0;

    // Update is called once per frame
    void Update()
    {
        ++frameCount;
        if (nextFPSUpdateTime <= Time.unscaledTime) {
            fpsUI.text = $"FPS: {(int)(frameCount / fpsRefreshPeriod)}";
            frameCount = 0;
            nextFPSUpdateTime = Time.unscaledTime + fpsRefreshPeriod;
        }
    }
}
