using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererStrokeConsumer : BaseStrokeConsumer
{
    private float WidthMultiplier = .1f;
    private LineRenderer _lr;
    
    protected override void ProcessCurrentPath()
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        _lr.material.color = currentColor;
        _lr.positionCount = currentPath.Count;
        var curve = new AnimationCurve();
        for (var i = 0; i < currentPath.Count; i++)
        {
            var pt = currentPath[i];
            _lr.SetPosition(i, new Vector3(pt[0], pt[1], pt[2]));
            curve.AddKey(i / (float)currentPath.Count, pt[6] * WidthMultiplier);
        }
        _lr.widthCurve = curve;

        Vector3[] positions = currentPath.Select(x => new Vector3(x[0], x[1], x[2])).ToArray();
        _lr.SetPositions(positions);
    }
    
}
