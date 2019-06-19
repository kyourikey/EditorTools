using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class EditorWebCamera : EditorWindow
{
    private static EditorWebCamera _window;
    private static WebCamDevice[] _webCamDevices;
    private static WebCamTexture _webCamTexture;
    
    private static float _maxWaitTime = 3f;
    private static float _waitStartTime = 0f;

    [MenuItem("Tools/EditorWebCamera", false)]
    static void Open()
    {
        _window = GetWindow<EditorWebCamera>();

        _webCamDevices = WebCamTexture.devices;

        if (_webCamDevices.Length <= 0)
        {
            return;
        }

        if (_webCamTexture == null)
        {
            _webCamTexture = new WebCamTexture(_webCamDevices[0].name);
        }

        _webCamTexture.Play();

        WaitWebCamFrame();
    }

    void OnDestroy()
    {
        _webCamTexture.Stop();

        _webCamTexture = null;
    }

    void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        if (_webCamDevices.Length <= 0)
        {
            return;
        }

        var height = _webCamTexture.height;
        var width = _webCamTexture.width;
        while (true)
        {
            if (height <= Screen.height &&
                width <= Screen.width)
            {
                break;
            }

            height /= 2;
            width /= 2;
        }
        var rect = new Rect(new Vector2(0, 100), new Vector2(width, height));
        EditorGUI.DrawPreviewTexture(rect, _webCamTexture);
        
        foreach (var device in _webCamDevices)
        {
            if (GUILayout.Button(device.name))
            {
                _webCamTexture.Stop();

                _webCamTexture = new WebCamTexture(device.name);

                _webCamTexture.Play();

                WaitWebCamFrame();
            }
        }
    }

    // Editor拡張だとStartCoroutineが使えないので
    private static void WaitWebCamFrame()
    {
        var t = WaitWebCamFrameCoroutine();
        while (t.MoveNext())
        {
            //Debug.Log("Current: " + t.Current);
        }
    }

    private static IEnumerator WaitWebCamFrameCoroutine()
    {
        _waitStartTime = Time.realtimeSinceStartup;
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (_webCamTexture.isPlaying)
            {
                if (_webCamTexture.didUpdateThisFrame)
                {
                    break;
                }
            }

            if (_waitStartTime + _maxWaitTime <= Time.realtimeSinceStartup)
            {
                _webCamTexture.Stop();
                break;
            }
        }
    }
}
