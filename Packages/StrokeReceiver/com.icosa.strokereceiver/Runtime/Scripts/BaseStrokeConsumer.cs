using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseStrokeConsumer : MonoBehaviour
{
    [NonSerialized] public Color currentColor;
    [NonSerialized] public string currentBrush;
    [NonSerialized] public List<List<float>> currentPath;
    
    private Queue _strokeQueue;
    private bool pathReady = false;

    protected virtual void Update()
    {
        string command = GetNextCommand();
        if (command != null)
        {
            DecodeCommand(command);
            if (pathReady) ProcessCurrentPath();
            pathReady = false;
        }
    }

    protected virtual string GetNextCommand()
    {
        if (_strokeQueue == null) _strokeQueue = gameObject.GetComponent<StrokeReceiver>().StrokeQueue;
        try
        {
            return (string)_strokeQueue.Dequeue();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    protected virtual void DecodeCommand(string command)
    {
        var parts = command.Split(new []{'='}, 2).ToList();
        if (parts[0]=="brush.type")
        {
            // brush.type=2241cd32-8ba2-48a5-9ee7-2caef7e9ed62
            currentBrush = parts[1];
        }
        else if (parts[0]=="color.set.rgb")
        {
            // color.set.rgb=0.2,0.2,0.5
            string[] rgb = parts[1].Split(',');
            currentColor = new Color(
                float.Parse(rgb[0]),
                float.Parse(rgb[1]),
                float.Parse(rgb[2])
            );
        }
        else if (parts[0]=="draw.stroke")
        {
            // each point is 7 floats: position xyz, euler rotation xyz, pressure
            // draw.stroke=[-1,12,9,0,180,0,1],[-1,13,9,0,180,0,1]...
            JArray strokeData = JArray.Parse($"[{parts[1]}]");
            currentPath = strokeData.Select(
                pt=>pt.ToList().Select(
                    val=>float.Parse(val.ToString())
                ).ToList()
            ).ToList();
            pathReady = true;
        }
    }

    protected abstract void ProcessCurrentPath();

}
