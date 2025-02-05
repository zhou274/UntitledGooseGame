using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.IMGUI.Controls;

public class FileTreeWindow : EditorWindow
{
    private static FileTreeWindow window;
    private static FileTreeView treeView;
    private static Action<bool, string[]> callback;
    private Vector2 scrollVector;
    private Rect treeRect;

    public static void OpenWindow(string relativeFolder, string title, List<string> enabledPaths, Action<bool,string[]> onCompleteCallback)
    {
        CloseWindow();
        callback = onCompleteCallback;
        BuildTree(relativeFolder, enabledPaths);
        window = (FileTreeWindow)EditorWindow.GetWindow(typeof(FileTreeWindow));
        window.titleContent = new GUIContent(title);
        window.Show();
    }

    private static void BuildTree(string relativeFolder, List<string> enabledPaths)
    {
        //Get all assets and sort them
        List<string> assetPaths = new List<string>();
        assetPaths.AddRange(AssetDatabase.GetAllAssetPaths());
        assetPaths.Sort();

        for (int i = assetPaths.Count - 1; i >= 0; i--)
        {
            if (!assetPaths[i].Contains(relativeFolder))
            {
                assetPaths.RemoveAt(i);
            }
        }

        treeView = new FileTreeView(new TreeViewState(), assetPaths, enabledPaths);
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        scrollVector = EditorGUILayout.BeginScrollView(scrollVector);
        treeRect = EditorGUILayout.BeginVertical(GUI.skin.box,GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
        treeView.OnGUI(treeRect);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Cancel"))
        {
            CloseWindow();
            callback?.Invoke(false, null);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Apply"))
        {
            CloseWindow();
            callback?.Invoke(true, treeView.GetResults()); ;
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private static void CloseWindow()
    {
        if (window != null)
        {
            window.Close();
        }
    }

}
