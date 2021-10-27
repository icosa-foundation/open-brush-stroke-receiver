// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;


/// Class for responding to Http Requests. request handlers can be added for specfic paths.
public class HttpServer : MonoBehaviour
{
    public int HttpPort = 40075;
    
    private HttpListener _httpListener;
    private Dictionary<string, Action<HttpListenerContext>> _httpRequestHandlers =
        new Dictionary<string, Action<HttpListenerContext>>();

    void Awake()
    {
        try
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(String.Format("http://+:{0}/", HttpPort));
            _httpListener.Start();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                while (_httpListener != null && _httpListener.IsListening)
                {
                    HttpListenerContext ctx;
                    try
                    {
                        ctx = _httpListener.GetContext();
                    }
                    catch (HttpListenerException)
                    {
                        // Irritatingly HttpListener will try to complete contexts when you call Close or
                        // Abort, even though it has already disposed the context.
                        break;
                    }
                    try
                    {
                        if (!ctx.Request.IsLocal)
                        {
                            // Return 403: Forbidden if the originator was non-local.
                            ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        }
                        else
                        {
                            var handlerKey = _httpRequestHandlers.Keys.FirstOrDefault(
                                x => ctx.Request.Url.LocalPath.StartsWith(x));
                            if (handlerKey == null)
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            }
                            else
                            {
                                try
                                {
                                    _httpRequestHandlers[handlerKey](ctx);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogException(ex);
                                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                    var bytes = Encoding.UTF8.GetBytes(ex.Message);
                                    ctx.Response.ContentLength64 = bytes.Length;
                                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                                }
                            }
                        }
                    }
                    finally
                    {
                        ctx.Response.OutputStream.Close();
                    }
                }
            });
        }
        catch (System.Net.Sockets.SocketException e)
        {
            Debug.LogFormat("HttpListener failed to start\n{0}", e);
            _httpListener = null;
        }
    }

    private void OnDestroy()
    {
        if (_httpListener != null)
        {
            _httpListener.Abort();
            _httpListener = null;
        }
    }

    /// Adds a handler to the Http server that responds to a given path.
    /// Path should include / at the start - e.g. /load  /files  /pages  etc
    /// The action takes a listener context and should make all appropriate adjustments to the
    /// response. The response does not need to be closed.
    public void AddHttpHandler(string path, Action<HttpListenerContext> handler)
    {
        _httpRequestHandlers.Add(path, handler);
    }

    /// Adds a handler to the Http server that responds to a given path.
    /// Path should include / at the start - e.g. /load  /files  /pages  etc
    /// The function takes a request and should return its response as an html string. 
    /// The response does not need to be closed.
    public void AddHttpHandler(string path, Func<HttpListenerRequest, string> handler)
    {
        var wrapper = new Action<HttpListenerContext>((context) =>
        {
            string response = handler(context.Request);
            if (response == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentType = "text/html";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        });
        _httpRequestHandlers.Add(path, wrapper);
    }

    // Removes a path from the Http server.
    public void RemoveHttpHandler(string path)
    {
        _httpRequestHandlers.Remove(path);
    }
}
