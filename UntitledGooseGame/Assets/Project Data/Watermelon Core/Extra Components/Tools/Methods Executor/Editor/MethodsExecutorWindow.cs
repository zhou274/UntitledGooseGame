using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon
{
    public class MethodsExecutorWindow : EditorWindow
    {
        private BindingFlags flags = BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static MonoBehaviour selectedScript;
        private static MethodInfo[] methods;
        private MethodInfo[] resultMethods;

        private static FieldInfo[] fields;
        private FieldInfo[] resultFields;
        private FieldInfo selectedField;
        private object selectedFieldValue;
        private Type selectedFieldType;

        private MethodInfo selectedMethod;
        private ParameterInfo[] selectedMethodParams;

        private string selectedMethodParamsLine;

        private object[] selectedMethodParamsValues;

        private Vector2 scrollPosition;
        private Vector2 selectedMethodScrollPosition;

        private string searchText = "";

        private readonly string[] tabs = new string[] { "Methods", "Variables" };
        private int selectedTab = 0;

        [MenuItem("CONTEXT/Object/Methods Executor", priority = 20)]
        private static void Executor(MenuCommand command)
        {
            Init((MonoBehaviour)command.context);
        }

        [MenuItem("CONTEXT/Object/Methods Executor", validate = true)]
        private static bool ExecutorValidate(MenuCommand command)
        {
            if (command == null || command.context == null)
                return false;

            return command.context.GetType().IsSubclassOf(typeof(MonoBehaviour));
        }

        public static void Init(MonoBehaviour script)
        {
            MethodsExecutorWindow w = EditorWindow.GetWindow<MethodsExecutorWindow>();
            w.titleContent = new GUIContent("Methods Executor");
            w.minSize = new Vector2(280, 330);
            w.SelectScript(script);
        }

        private void OnEnable()
        {
            selectedMethod = null;
            selectedField = null;
            scrollPosition = Vector2.zero;
            selectedMethodScrollPosition = Vector2.zero;
        }

        private void SelectScript(MonoBehaviour script)
        {
            if (script != null)
            {
                Type scriptType = script.GetType();
                methods = scriptType.GetMethods(flags);
                fields = scriptType.GetFields(flags);
            }

            selectedScript = script;
            scrollPosition = Vector2.zero;
            selectedMethodScrollPosition = Vector2.zero;
            selectedMethod = null;
            selectedField = null;
        }

        void OnGUI()
        {
            EditorGUIUtility.fieldWidth = 10;
            EditorGUIUtility.labelWidth = 80;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.BeginChangeCheck();
            selectedScript = (MonoBehaviour)EditorGUILayout.ObjectField("Script: ", selectedScript, typeof(MonoBehaviour), true);
            if (EditorGUI.EndChangeCheck())
            {
                SelectScript(selectedScript);
            }

            if (selectedScript != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Height(200));

                GUILayout.BeginHorizontal();
                searchText = GUILayout.TextField(searchText, GUI.skin.FindStyle("ToolbarSeachTextField"));
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    // Remove focus if cleared
                    searchText = "";
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(2);

                if (string.IsNullOrEmpty(searchText))
                {
                    resultMethods = methods;
                    resultFields = fields;
                }
                else
                {
                    resultMethods = Array.FindAll(methods, x => x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                    resultFields = Array.FindAll(fields, x => x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                EditorGUI.BeginChangeCheck();
                selectedTab = GUILayout.Toolbar(selectedTab, tabs);
                if (EditorGUI.EndChangeCheck())
                {
                    scrollPosition = Vector2.zero;
                    selectedMethod = null;
                    selectedField = null;
                }

                if (selectedTab == 0) // Methods tab
                {
                    for (int i = 0; i < resultMethods.Length; i++)
                    {
                        MethodInfo methodInfo = resultMethods[i];

                        if (GUILayout.Button(methodInfo.Name, EditorStyles.miniButton))
                        {
                            selectedMethod = methodInfo;

                            selectedMethodParams = methodInfo.GetParameters();
                            selectedMethodParamsValues = new object[selectedMethodParams.Length];

                            string paramsLine = "";

                            for (int p = 0; p < selectedMethodParams.Length; p++)
                            {
                                if (p != 0)
                                    paramsLine += " ";

                                paramsLine += selectedMethodParams[p].ParameterType + ";";
                            }

                            selectedMethodParamsLine = paramsLine;
                        }
                    }
                }
                else // Variables tab
                {
                    for (int i = 0; i < resultFields.Length; i++)
                    {
                        FieldInfo fieldInfo = resultFields[i];

                        if (GUILayout.Button(fieldInfo.Name, EditorStyles.miniButton))
                        {
                            object variable = fieldInfo.GetValue(selectedScript);

                            if (variable != null)
                            {
                                selectedFieldType = variable.GetType();
                                if (!selectedFieldType.IsArray && !selectedFieldType.IsGenericType)
                                {
                                    selectedField = fieldInfo;
                                    selectedFieldValue = variable;
                                }
                            }
                            else
                            {
                                selectedFieldType = fieldInfo.DeclaringType;
                                if (selectedFieldType.IsSubclassOf(typeof(Object)))
                                {
                                    selectedFieldValue = new Object();
                                }
                                else if (selectedFieldType.IsSerializable && !selectedFieldType.IsArray && !selectedFieldType.IsGenericType && selectedFieldType != typeof(object))
                                {
                                    selectedFieldValue = Activator.CreateInstance(selectedMethodParams[i].ParameterType);
                                }

                                if (selectedFieldValue != null)
                                {
                                    selectedField = fieldInfo;
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();

            if (selectedScript != null)
            {
                if (selectedTab == 0) // Methods tab
                {
                    if (selectedMethod != null)
                    {
                        bool hasError = false;

                        Rect messageRect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(Screen.height - 255));

                        selectedMethodScrollPosition = EditorGUILayout.BeginScrollView(selectedMethodScrollPosition, false, false, GUILayout.Height(Screen.height - 260));

                        string methodTitle = selectedMethod.Name + (string.IsNullOrEmpty(selectedMethodParamsLine) ? "" : " (" + selectedMethodParamsLine + ")");
                        EditorGUILayout.LabelField(new GUIContent(methodTitle, methodTitle), EditorStyles.boldLabel);

                        for (int i = 0; i < selectedMethodParams.Length; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(selectedMethodParams[i].Name) + ": ");

                            if (selectedMethodParams[i].ParameterType == typeof(string))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = "";

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType == typeof(bool))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = false;

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType == typeof(int))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = 0;

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType == typeof(float))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = 0.0f;

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType == typeof(Type))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = typeof(Object);

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType == typeof(Vector2))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = Vector2.zero;

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType == typeof(Vector3))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = Vector3.zero;

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType.IsEnum)
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = Enum.ToObject(selectedMethodParams[i].ParameterType, 0);

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType.IsSubclassOf(typeof(Object)))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = new Object();

                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                            }
                            else if (selectedMethodParams[i].ParameterType.IsSerializable && !selectedMethodParams[i].ParameterType.IsArray && !selectedMethodParams[i].ParameterType.IsGenericType && selectedMethodParams[i].ParameterType != typeof(object))
                            {
                                if (selectedMethodParamsValues[i] == null)
                                    selectedMethodParamsValues[i] = Activator.CreateInstance(selectedMethodParams[i].ParameterType);

                                EditorGUILayout.BeginVertical(GUI.skin.box);
                                selectedMethodParamsValues[i] = EditorGUILayoutCustom.UniversalField(selectedMethodParamsValues[i], selectedMethodParams[i].ParameterType);
                                EditorGUILayout.EndVertical();
                            }
                            else
                            {
                                hasError = true;
                            }

                            EditorGUILayout.EndHorizontal();
                        }

                        GUILayout.FlexibleSpace();

                        GUI.enabled = !hasError;
                        if (GUILayout.Button("Invoke"))
                        {
                            Undo.RecordObject(selectedScript, selectedScript.name);

                            object result = selectedMethod.Invoke(selectedScript, selectedMethodParamsValues);

                            if (result != null)
                            {
                                try
                                {
                                    Debug.Log(result.ToString());
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning(e);
                                }
                            }

                            EditorUtility.SetDirty(selectedScript);
                        }
                        GUI.enabled = true;

                        EditorGUILayout.EndScrollView();

                        if (hasError)
                        {
                            GUI.Box(messageRect, "");
                            EditorGUI.HelpBox(messageRect, "Unknown params type!", MessageType.Error);
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    if (selectedField != null)
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(Screen.height - 255));

                        GUI.enabled = !(!selectedField.IsInitOnly && selectedField.IsLiteral);

                        string methodTitle = selectedField.Name + " (" + selectedFieldType + ")";
                        EditorGUILayout.LabelField(new GUIContent(methodTitle, methodTitle), EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(selectedField.Name) + ": ");
                        selectedFieldValue = EditorGUILayoutCustom.UniversalField(selectedFieldValue, selectedFieldType);

                        EditorGUILayout.EndHorizontal();

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Save"))
                        {
                            Undo.RecordObject(selectedScript, selectedScript.name);

                            selectedField.SetValue(selectedScript, selectedFieldValue);

                            EditorUtility.SetDirty(selectedScript);
                        }
                        GUI.enabled = true;

                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }
    }
}