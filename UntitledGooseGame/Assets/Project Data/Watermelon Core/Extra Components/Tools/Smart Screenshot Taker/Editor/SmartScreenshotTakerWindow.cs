using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.IO;
using System.Linq;
using UnityEngine.Rendering.Universal;
using Watermelon;
using System.Text;

public class EditorSmartScreenshotTakerWindow : WatermelonWindow
{
    private const string PREFS_SELECTED_RATIOS = "screenshotRatios";
    private const string PREFS_TRANSPARENT = "screenshotTransparent";
    private const string PREFS_SCALE_SIZE = "screenshotSize";

    private readonly int[] SCALE_SIZES = new int[] { 1, 2, 4, 8 };
    private const string BASE_FOLDER_NAME = "Screenshots/";
    
    private static bool[] activeRatios;

    private static bool isInitialized;

    private static GameViewSizeGroupType gameViewSizeGroupType;

    private static object gameViewSizesInstance;
    private static MethodInfo getGroup;

    private bool useTransparent;

    private string[] sizeNames;
    private int selectedSize = 0;

    private IEnumerator screenShotCoroutine;

    private GUIContent folderButtonGUIContent;
    
    [MenuItem("Window/Smart Screenshot Taker/Screnshot Taker Window")]
    public static EditorSmartScreenshotTakerWindow ShowWindow()
    {
        EditorSmartScreenshotTakerWindow window = GetWindow<EditorSmartScreenshotTakerWindow>(false);
        window.titleContent = new GUIContent("Smart Screenshot Taker");
        
        return window;
    }

    [MenuItem("Window/Smart Screenshot Taker/Take Screenshot _F12")]
    public static void TakeScreenshotShortcut()
    {
        EditorSmartScreenshotTakerWindow window = ShowWindow();

        if (Application.isPlaying)
        {
            if (window != null)
            {
                window.TakeScreenshot();
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Initialize();

        sizeNames = SCALE_SIZES.Select(x => x.ToString()).ToArray();

        LoadPrefs();
    }


    private void OnDisable()
    {
        SavePrefs();
    }

    private void OnDestroy()
    {
        SavePrefs();
    }

    public void TakeScreenshot()
    {
        if (screenShotCoroutine == null)
        {
            if (isInitialized && activeRatios.Where(x => x).Count() > 0)
            {
                EditorApplication.update += EditorUpdate;

                screenShotCoroutine = TakeScreenshotEnumerator();
            }
        }
        else
        {
            Debug.LogWarning("Screenshot Taker is already in work. Please wait.");
        }
    }

    private void EditorUpdate()
    {
        if(screenShotCoroutine != null)
        {
            if(!screenShotCoroutine.MoveNext())
            {
                EditorApplication.update -= EditorUpdate;

                screenShotCoroutine = null;
            }
        }
    }
        
    private static void Initialize()
    {
        Type sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        Type singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        PropertyInfo instanceProp = singleType.GetProperty("instance");
        getGroup = sizesType.GetMethod("GetGroup");
        gameViewSizesInstance = instanceProp.GetValue(null, null);

        gameViewSizeGroupType = GetCurrentGroupType();

        activeRatios = new bool[AspectRatios.aspectRatios.Length];
        for (int i = 0; i < AspectRatios.aspectRatios.Length; i++)
        {
            if (!SizeExists(gameViewSizeGroupType, AspectRatios.aspectRatios[i].xAspect, AspectRatios.aspectRatios[i].yAspect))
            {
                AddCustomSize(gameViewSizeGroupType, AspectRatios.aspectRatios[i].xAspect, AspectRatios.aspectRatios[i].yAspect, AspectRatios.aspectRatios[i].name);
            }
        }

        isInitialized = true;
    }

    protected override void Styles()
    {
        folderButtonGUIContent = new GUIContent(string.Empty, EditorStylesExtended.GetTexture("icon_folder", EditorStylesExtended.IconColor));
    }

    private void OnGUI()
    {
        if (!isInitialized)
            Initialize();

        InitStyles();

        EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

        if(EditorGUILayoutCustom.HeaderButton("SCREENSHOT TAKER", folderButtonGUIContent, EditorStylesExtended.button_01, GUILayout.Width(24), GUILayout.Height(24)))
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", Application.dataPath.Replace("/Assets", "/Screenshots").Replace("/", @"\"));
#else
            Debug.LogWarning("The button won't work on the current operating system!");
#endif
        }

        EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);
        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < activeRatios.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent(AspectRatios.aspectRatios[i].name));
            activeRatios[i] = EditorGUILayout.Toggle(activeRatios[i], GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                string folderPath = Application.dataPath.Replace("Assets", "") + BASE_FOLDER_NAME + AspectRatios.aspectRatios[i].name + "/";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        if (EditorGUI.EndChangeCheck())
        {
            SavePrefs();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

        EditorGUILayoutCustom.Header("SETTINGS");

        EditorGUI.BeginChangeCheck();
        useTransparent = EditorGUILayout.Toggle("Transparent: ", useTransparent);
        selectedSize = EditorGUILayout.Popup("Extra size: ", selectedSize, sizeNames);
        if(EditorGUI.EndChangeCheck())
        {
            SavePrefs();
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("Capture (F12)", EditorStylesExtended.button_01, GUILayout.Height(36)))
        {
            TakeScreenshot();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
    }

    private IEnumerator TakeScreenshotEnumerator()
    {
        Camera mainCamera = Camera.main;

        List<Camera> cameras = new List<Camera>();
        cameras.Add(mainCamera);

        UniversalAdditionalCameraData cameraData = mainCamera.GetUniversalAdditionalCameraData();

        if(cameraData.cameraStack.Count > 0)
            cameras.AddRange(cameraData.cameraStack);

        float timeScale = Time.timeScale;
        Time.timeScale = 0;

        Assembly assembly = Assembly.GetAssembly(typeof(ScreenshotResizeAttribute));
        IEnumerable<MethodInfo> methods = assembly.GetTypes().SelectMany(m => m.GetMethods().Where(i => i.GetCustomAttributes(typeof(ScreenshotResizeAttribute), false).Count() > 0));
        Dictionary<MethodInfo, UnityEngine.Object[]> cachableObjects = new Dictionary<MethodInfo, UnityEngine.Object[]>();

        EditorWindow window = FullscreenManger.OpenFullscreen(EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")));

        yield return null;

        foreach (var method in methods)
        {
            UnityEngine.Object[] objects = FindObjectsOfType(method.ReflectedType);
            if (objects != null)
            {
                cachableObjects.Add(method, objects);
            }
        }

        for (int i = 0; i < AspectRatios.aspectRatios.Length; i++)
        {
            if (activeRatios[i])
            {
                string path = BASE_FOLDER_NAME + AspectRatios.aspectRatios[i].name + "/Sccreenshot_" + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString();

                int sizeIndex = FindSize(gameViewSizeGroupType, AspectRatios.aspectRatios[i].xAspect, AspectRatios.aspectRatios[i].yAspect);
                if (sizeIndex != -1)
                    SetSize(sizeIndex);

                yield return null;
                yield return null;

                window.Repaint();

                foreach (var cachableObject in cachableObjects)
                {
                    for (int c = 0; c < cachableObject.Value.Length; c++)
                    {
                        cachableObject.Key.Invoke(cachableObject.Value[c], null);
                    }
                }

                yield return null;

                int width = AspectRatios.aspectRatios[i].xAspect * SCALE_SIZES[selectedSize];
                int height = AspectRatios.aspectRatios[i].yAspect * SCALE_SIZES[selectedSize];

                Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
                for(int x = 0; x < width; x++)
                {
                    for(int y = 0; y < height; y++)
                    {
                        result.SetPixel(x, y, Color.clear);
                    }
                }
                result.Apply();

                yield return new WaitForEndOfFrame();

                foreach (Camera camera in cameras)
                {
                    if(!useTransparent && camera == mainCamera)
                    {
                        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8, RenderTextureMemoryless.None);

                        RenderTexture.active = renderTexture;

                        camera.targetTexture = renderTexture;
                        camera.Render();

                        result.ReadPixels(new Rect(0.0f, 0.0f, width, height), 0, 0, true);
                        result.Apply();

                        camera.targetTexture = null;

                        RenderTexture.active = null;
                        RenderTexture.ReleaseTemporary(renderTexture);

                        continue;
                    }

                    if (camera.enabled)
                    {
                        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8, RenderTextureMemoryless.None);

                        Texture2D tex_white = new Texture2D(width, height, TextureFormat.ARGB32, false);
                        Texture2D tex_black = new Texture2D(width, height, TextureFormat.ARGB32, false);
                        Texture2D tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);

                        Rect grab_area = new Rect(0, 0, width, height);

                        RenderTexture.active = renderTexture;

                        var cameraAdditionalData = camera.GetUniversalAdditionalCameraData();
                        CameraRenderType defaultRenderType = cameraAdditionalData.renderType;
                        Color defaultRenderColor = camera.backgroundColor;

                        cameraAdditionalData.renderType = CameraRenderType.Base;
                        camera.targetTexture = renderTexture;
                        camera.clearFlags = CameraClearFlags.SolidColor;

                        camera.backgroundColor = Color.black;
                        camera.Render();
                        tex_black.ReadPixels(grab_area, 0, 0);
                        tex_black.Apply();

                        camera.backgroundColor = Color.white;
                        camera.Render();
                        tex_white.ReadPixels(grab_area, 0, 0);
                        tex_white.Apply();

                        // Create Alpha from the difference between black and white camera renders
                        for (int y = 0; y < tex_transparent.height; ++y)
                        {
                            for (int x = 0; x < tex_transparent.width; ++x)
                            {
                                float alpha = tex_white.GetPixel(x, y).r - tex_black.GetPixel(x, y).r;
                                alpha = 1.0f - alpha;
                                Color color;
                                if (alpha == 0)
                                {
                                    color = Color.clear;
                                }
                                else
                                {
                                    color = tex_black.GetPixel(x, y) / alpha;
                                }
                                color.a = alpha;
                                tex_transparent.SetPixel(x, y, color);

                                Color parentColor = result.GetPixel(x, y);
                                Color blendColor;
                                blendColor.a = 1 - (1 - color.a) * (1 - parentColor.a);
                                if (blendColor.a < 1.0e-6) continue; // Fully transparent -- R,G,B not important
                                blendColor.r = color.r * color.a / blendColor.a + parentColor.r * parentColor.a * (1 - color.a) / blendColor.a;
                                blendColor.g = color.g * color.a / blendColor.a + parentColor.g * parentColor.a * (1 - color.a) / blendColor.a;
                                blendColor.b = color.b * color.a / blendColor.a + parentColor.b * parentColor.a * (1 - color.a) / blendColor.a;

                                result.SetPixel(x, y, blendColor);
                            }
                        }

                        // Reset default values
                        cameraAdditionalData.renderType = defaultRenderType;
                        camera.backgroundColor = defaultRenderColor;

                        camera.targetTexture = null;

                        RenderTexture.active = null;
                        RenderTexture.ReleaseTemporary(renderTexture);

                        Texture2D.Destroy(tex_black);
                        Texture2D.Destroy(tex_white);
                        Texture2D.Destroy(tex_transparent);
                    }
                }

                result.Apply();
                byte[] resultBytes = result.EncodeToPNG();

                File.WriteAllBytes(path + ".png", resultBytes);

                Texture2D.Destroy(result);

                yield return null;
            }
        }

        FullscreenManger.CloseWindow();

        Time.timeScale = timeScale;
    }

    #region Prefs
    private void LoadPrefs()
    {
        useTransparent = EditorPrefs.GetBool(PREFS_TRANSPARENT, false);
        selectedSize = EditorPrefs.GetInt(PREFS_SCALE_SIZE, 0);

        string aspectRatiosString = EditorPrefs.GetString(PREFS_SELECTED_RATIOS, string.Empty);
        if (!string.IsNullOrEmpty(aspectRatiosString))
        {
            string[] ratios = aspectRatiosString.Split(';');
            for (int i = 0; i < ratios.Length; i++)
            {
                int selectedIndex = -1;

                if (int.TryParse(ratios[i], out selectedIndex))
                {
                    if (activeRatios.IsInRange(selectedIndex))
                    {
                        activeRatios[selectedIndex] = true;
                    }
                }
            }
        }
    }

    private void SavePrefs()
    {
        StringBuilder ratiosSaveStringBuilder = new StringBuilder();
        for (int i = 0; i < activeRatios.Length; i++)
        {
            if (activeRatios[i])
            {
                ratiosSaveStringBuilder.Append(i);
                ratiosSaveStringBuilder.Append(';');
            }
        }

        EditorPrefs.SetString(PREFS_SELECTED_RATIOS, ratiosSaveStringBuilder.ToString());
        EditorPrefs.SetBool(PREFS_TRANSPARENT, useTransparent);
        EditorPrefs.SetInt(PREFS_SCALE_SIZE, selectedSize);
    }
    #endregion

    #region Unity
    private static GameViewSizeGroupType GetCurrentGroupType()
    {
        var getCurrentGroupTypeProp = gameViewSizesInstance.GetType().GetProperty("currentGroupType");
        return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(gameViewSizesInstance, null);
    }

    private static void SetSize(int index)
    {
        Type gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        PropertyInfo selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var gvWnd = EditorWindow.GetWindow(gvWndType);
        selectedSizeIndexProp.SetValue(gvWnd, index, null);
    }

    private static void AddCustomSize(GameViewSizeGroupType sizeGroupType, int width, int height, string text)
    {
        System.Object group = GetGroup(sizeGroupType);
        MethodInfo addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
        Type gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");

        //ConstructorInfo ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
        ConstructorInfo ctor = gvsType.GetConstructors()[0];

        System.Object newSize = ctor.Invoke(new object[] { 1, width, height, text });
        addCustomSize.Invoke(group, new object[] { newSize });
    }

    private static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height)
    {
        return FindSize(sizeGroupType, width, height) != -1;
    }

    private static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height)
    {
        var group = GetGroup(sizeGroupType);
        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
        var getGameViewSize = groupType.GetMethod("GetGameViewSize");
        var gvsType = getGameViewSize.ReturnType;
        var widthProp = gvsType.GetProperty("width");
        var heightProp = gvsType.GetProperty("height");
        var indexValue = new object[1];
        for (int i = 0; i < sizesCount; i++)
        {
            indexValue[0] = i;
            var size = getGameViewSize.Invoke(group, indexValue);
            int sizeWidth = (int)widthProp.GetValue(size, null);
            int sizeHeight = (int)heightProp.GetValue(size, null);
            if (sizeWidth == width && sizeHeight == height)
                return i;
        }

        return -1;
    }

    private static object GetGroup(GameViewSizeGroupType type)
    {
        return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
    }
    #endregion

    [InitializeOnLoadMethod]
    public static void InitializeScreenshotTaker()
    {
        Initialize();

        string fullPath = Application.dataPath.Replace("Assets", "") + BASE_FOLDER_NAME;
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);
    }
}
