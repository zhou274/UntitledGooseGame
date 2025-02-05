using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System;
using UnityEditor;

public class FileTreeView : TreeView
{
    private const float TOGGLE_WIDTH = 18;
    private List<string> assetPaths;
    private List<string> enabledPaths;
    private TreeViewItem[] treeViewItemsArray;
    private bool[] treeItemEnableValueArray;
    private TreeViewItem rootTreeItem;
    private int childIndex;
    private Rect tempToggleRect;

    public FileTreeView(TreeViewState state, List<string> assetPaths, List<string> enabledPaths) : base(state)
    {
        this.assetPaths = assetPaths;
        this.enabledPaths = enabledPaths;
        extraSpaceBeforeIconAndLabel = TOGGLE_WIDTH;
        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        rootTreeItem = new TreeViewItem(-1, -1);
        treeViewItemsArray = new TreeViewItem[assetPaths.Count];
        treeItemEnableValueArray = new bool[assetPaths.Count];

        for (int i = 0; i < assetPaths.Count; i++)
        {
            treeViewItemsArray[i] = new TreeViewItem(i,0, GetFileName(assetPaths[i]));
            treeItemEnableValueArray[i] = enabledPaths.Contains(assetPaths[i]);
        }

        childIndex = 0;
        SetParent(-1);
        SetupDepthsFromParentsAndChildren(rootTreeItem);

        //enable child paths
        for (int i = 0; i < treeViewItemsArray.Length; i++)
        {
            if (treeItemEnableValueArray[i])
            {
                SetBoolValue(treeViewItemsArray[i], true);
            }
        }

        return rootTreeItem;
    }

    private void SetParent(int currentParent)
    {
        if(childIndex >= assetPaths.Count)
        {
            return;
        }

        if (currentParent == -1) // handle root
        {
            rootTreeItem.AddChild(treeViewItemsArray[childIndex]);

            if (AssetDatabase.IsValidFolder(assetPaths[childIndex]))
            {
                childIndex++;
                SetParent(childIndex - 1);
                SetParent(currentParent);
            }
            else
            {
                childIndex++;
                SetParent(currentParent);
            }
        }
        else if (assetPaths[childIndex].Contains(assetPaths[currentParent])) // handle rest of elements
        {
            treeViewItemsArray[currentParent].AddChild(treeViewItemsArray[childIndex]);

            if (AssetDatabase.IsValidFolder(assetPaths[childIndex]))
            {
                childIndex++;
                SetParent(childIndex - 1);
                SetParent(currentParent);
            }
            else
            {
                childIndex++;
                SetParent(currentParent);
            }
        }
    }

    private string GetFileName(string assetPath)
    {
        return assetPath.Substring(assetPath.LastIndexOf('/') + 1);
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        tempToggleRect = args.rowRect;
        tempToggleRect.x += GetContentIndent(args.item);
        
        tempToggleRect.width = TOGGLE_WIDTH;
        if (tempToggleRect.xMax < args.rowRect.xMax)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.Toggle(tempToggleRect, treeItemEnableValueArray[args.item.id]);

            if (EditorGUI.EndChangeCheck())
            {
                SetBoolValue(args.item, !treeItemEnableValueArray[args.item.id]);
            }
        }

        base.RowGUI(args);
    }

    private void SetBoolValue(TreeViewItem item, bool value)
    {
        treeItemEnableValueArray[item.id] = value;

        if (item.hasChildren)
        {
            foreach (TreeViewItem child in item.children)
            {
                SetBoolValue(child, value);
            }
        }
    }

    protected override void SingleClickedItem(int id)
    {
        base.SingleClickedItem(id);
        SetBoolValue(treeViewItemsArray[id], !treeItemEnableValueArray[id]);
    }

    protected override void DoubleClickedItem(int id)
    {
        SetExpanded(id, !GetExpanded().Contains(id));
        SetBoolValue(treeViewItemsArray[id], !treeItemEnableValueArray[id]);
    }

    public string[] GetResults()
    {
        List<string> results = new List<string>();

        for (int i = 0; i < assetPaths.Count; i++)
        {
            if (treeItemEnableValueArray[i])
            {
                results.Add(assetPaths[i]);
            }
        }

        return results.ToArray();
    }
}
