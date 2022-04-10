using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings
{
    private bool isFullScreen;
    private Resolution currentResolution;
    private GraphicsQuality graphicsQuality;

    [HideInInspector]
    public enum Resolution { RES1440P, RES1080P, RES720P };
    [HideInInspector]
    public enum GraphicsQuality { HIGH, MED, LOW };

    public GameSettings()
    {
        isFullScreen = false;
        currentResolution = Resolution.RES1080P;
        graphicsQuality = GraphicsQuality.HIGH;
    }//end of GameSettings

    public bool GetIsFullScreen() { return isFullScreen; }
    public void SetIsFullScreen(bool fullScreen) { isFullScreen = fullScreen; }

    public Resolution GetCurrentResolution() { return currentResolution; }
    public void SetCurrentResolution(Resolution res) { currentResolution = res; }

    public GraphicsQuality GetGraphicsQuality() { return graphicsQuality; }
    public void SetGraphicsQuality(GraphicsQuality quality) { graphicsQuality = quality; }
}
