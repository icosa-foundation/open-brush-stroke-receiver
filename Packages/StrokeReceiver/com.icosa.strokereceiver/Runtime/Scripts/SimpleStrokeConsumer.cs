using System;
using System.Collections;
using UnityEngine;

public class SimpleStrokeConsumer : BaseStrokeConsumer
{
    protected override void ProcessCurrentPath()
    {
        Debug.Log($"Brush: {currentBrush} Color: {currentColor} Path: {currentPath.Count} points");
    }
}
