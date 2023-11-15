using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetRes : MonoBehaviour
{
    #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            private int[] widthRes = new[] { 640, 768, 896, 1024, 1152, 1280, 1408, 1536, 1664, 1792, 1920 };
            private int[] heightRes = new[] { 360, 432, 504, 576, 648, 720, 792, 864, 936, 1008, 1080 };
             
            // Start is called before the first frame update
            void Start()
            {
                Resolution[] resolutions = Screen.resolutions;
                
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;
    
                var maxWidth = 896;
                
                foreach (var res in widthRes)
                {
                    if (res <= screenWidth)
                        maxWidth = res;
                    else
                        break;
                }
    
                Debug.Log($"maxWidth: {maxWidth} screenWidth: {screenWidth}");
                var widthIndex = Array.IndexOf(widthRes, maxWidth);
    
                Screen.SetResolution(maxWidth, heightRes[widthIndex], FullScreenMode.MaximizedWindow);
            }
    #endif
}
