using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

public static class FullscreenManger
{
    private static EditorWindow openedWindow;

    private readonly static Dictionary<string, float> specialHeight = new Dictionary<string, float>()
    {
        { "Game", 18 }
    };
    
    [MenuItem("Window/Switch Fullscreen _F10")]
    public static void SwitchFullscreen()
    {
        if(openedWindow != null)
        {
            openedWindow.Close();

            openedWindow = null;
        }
        else
        {
            if(EditorWindow.focusedWindow != null)
            {
                OpenFullscreen(EditorWindow.focusedWindow);
            }
        }
    }
    
    public static EditorWindow OpenFullscreen(EditorWindow window)
    {
        if (openedWindow != null)
        {
            openedWindow.Close();

            openedWindow = null;
        }

        if (window != null)
        {
            if (window.titleContent.text == "Game")
            {
                Type gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
                PropertyInfo selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                EditorWindow gameViewWindow = EditorWindow.GetWindow(gvWndType);
                
                int selectedSizeIndex = (int)selectedSizeIndexProp.GetValue(gameViewWindow, null);
                
                openedWindow = (EditorWindow)EditorWindow.CreateInstance(gvWndType);
                openedWindow.ShowAuxWindow();

                selectedSizeIndexProp.SetValue(openedWindow, selectedSizeIndex, null);
            }
            else
            {
                openedWindow = (EditorWindow)EditorWindow.CreateInstance(EditorWindow.focusedWindow.GetType());
                openedWindow.ShowAuxWindow();
            }

            float overridedTabHeight = 0;
            if (specialHeight.ContainsKey(openedWindow.titleContent.text))
                overridedTabHeight = specialHeight[openedWindow.titleContent.text];

            Rect newPos = new Rect(0, 0 - overridedTabHeight, Screen.currentResolution.width, Screen.currentResolution.height + overridedTabHeight);

            openedWindow.position = newPos;
            openedWindow.minSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height + overridedTabHeight);
            openedWindow.maxSize = openedWindow.minSize;
            openedWindow.position = newPos;

            openedWindow.Focus();

            return openedWindow;
        }

        return null;
    }

    public static void CloseWindow()
    {
        if (openedWindow != null)
        {
            openedWindow.Close();

            openedWindow = null;
        }
    }
}