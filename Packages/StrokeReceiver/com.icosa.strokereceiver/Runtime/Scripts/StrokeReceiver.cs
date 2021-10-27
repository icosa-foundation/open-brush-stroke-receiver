using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class StrokeReceiver : MonoBehaviour
{
    public string ReceiverDomain = "localhost";
    public string ReceiverBasePath = "/receive";
    public string OpenBrushBaseUrl = "http://localhost:40074/api/v1";
    public string OpenBrushRegisterCommand = "listenfor.strokes";
    private HttpServer _httpServer;
    private readonly Queue _strokeQueue = Queue.Synchronized(new Queue());

    public Queue StrokeQueue => _strokeQueue;

    void Start()
    {
        _httpServer = GetComponent<HttpServer>();
        _httpServer.AddHttpHandler(ReceiverBasePath, HttpStrokeCallback);
        RegisterWithLocalOpenBrush();
    }

    private void RegisterWithLocalOpenBrush()
    {
        string url = $"{OpenBrushBaseUrl}?{OpenBrushRegisterCommand}=http://{ReceiverDomain}:{_httpServer.HttpPort}{ReceiverBasePath}";
        StartCoroutine(GetRequest(url));
    }
    
    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.responseCode != 200)
            {
                // Retry
                StartCoroutine(GetRequest(url));
            }
        }
    }
    
    string HttpStrokeCallback(HttpListenerRequest request)
    {
        var urlPath = request.Url.LocalPath;
        var query = Uri.UnescapeDataString(request.Url.Query);
        if (urlPath == ReceiverBasePath && query.Length > 1)
        {
            var payload = query.Substring(1);
            StrokeQueue.Enqueue(payload);
        }
        return "OK";
    }


}
