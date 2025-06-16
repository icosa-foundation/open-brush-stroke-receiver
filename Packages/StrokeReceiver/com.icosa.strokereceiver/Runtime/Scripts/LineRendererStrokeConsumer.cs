using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererStrokeConsumer : BaseStrokeConsumer
{
    private readonly float GlobalWidthMultiplier = .1f;
    private LineRenderer _lr;
    
    protected override void ProcessCurrentPath()
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();

        _lr.material.color = currentColor;
        _lr.positionCount = currentPath.Count;
        _lr.widthMultiplier = currentBrushSize;
        _lr.numCornerVertices = 3;
        _lr.numCapVertices = 3;
        var curve = new AnimationCurve();
        for (var i = 0; i < currentPath.Count; i++)
        {
            var pt = currentPath[i];
            var pos = new Vector3(pt[0], pt[1], pt[2]);
            var orientation = Quaternion.Euler(pt[3], pt[4], pt[5]); // Not used in LineRenderer
            _lr.SetPosition(i, pos);
            float pressure = pt[6];
            float sizeAndPressure = pressure * GlobalWidthMultiplier;
            float time = i / (float)currentPath.Count;
            curve.AddKey(time, sizeAndPressure);
        }
        _lr.widthCurve = curve;

        Vector3[] positions = currentPath.Select(x => new Vector3(x[0], x[1], x[2])).ToArray();
        _lr.SetPositions(positions);
    }
    
}
