using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

public class UtilityCanOrCantWindows : EditorWindow
{
    private List<object> ShowList;

    private Dictionary<string, object> CachedInstanceList;

    private Action<List<object>> sure;

    private Action<List<object>> cancel;

    private Action onguifunc;

    private string content;

    public static UtilityCanOrCantWindows CreateWindows(string title,string content, Action<List<object>> SureAction, Action<List<object>> CancelAction, params object[] objarray)
    {
        UtilityCanOrCantWindows windows = EditorWindow.GetWindow<UtilityCanOrCantWindows>();
        windows.ShowList = new List<object>(objarray);
        windows.sure = SureAction;
        windows.cancel = CancelAction;
        windows.titleContent = new GUIContent(title);
        windows.content = content;
        windows.ShowUtility();

        return windows;
    }

    public static UtilityCanOrCantWindows CreateGUIWindows(string title, string content, Action GUIfUNC)
    {
        UtilityCanOrCantWindows windows = EditorWindow.GetWindow<UtilityCanOrCantWindows>();
        windows.titleContent = new GUIContent(title);
        windows.content = content;
        windows.onguifunc = GUIfUNC;
        windows.ShowUtility();

        return windows;
    }

    void OnGUI()
    {
        GUILayout.Label(content);

        if (ShowList != null)
        {
            if (CachedInstanceList == null)
                CachedInstanceList = new Dictionary<string, object>();

            for (int i = 0; i < ShowList.Count; ++i)
            {
                object o = ShowList[i];
                if (o != null)
                {
                    if (o is Enum)
                    {

                    }
                    else if (o is String)
                    {

                    }
                    else if (o is UnityEngine.Object)
                    {

                    }
                    else if (o is Type)
                    {
                        Type type = o as Type;
                        string key = type.Name + "_" + i.ToString();
                        if (type == typeof(string))
                        {
                            string value = "";

                            if (this.CachedInstanceList.ContainsKey(key))
                            {
                                value = this.CachedInstanceList[key] as string;
                            }

                            value = GUILayout.TextField(value);
                            this.CachedInstanceList[key] = value;
                        }
                        else if (type == typeof(Enum) || type.IsSubclassOf(typeof(Enum)))
                        {
                            System.Enum value = Activator.CreateInstance(type) as Enum;

                            if (this.CachedInstanceList.ContainsKey(key))
                            {
                                value = this.CachedInstanceList[key] as Enum;
                            }

                            value = EditorGUILayout.EnumPopup(value);
                            this.CachedInstanceList[key] = value;
                        }
                        else if (type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object)))
                        {
                            UnityEngine.Object value = ScriptableObject.CreateInstance(type) as UnityEngine.Object;

                            if (this.CachedInstanceList.ContainsKey(key))
                            {
                                value = this.CachedInstanceList[key] as UnityEngine.Object;
                            }

                            value = EditorGUILayout.ObjectField(value, type, true);
                            this.CachedInstanceList[key] = value;
                        }
                        else if (type == typeof(float) || type == typeof(int))
                        {
                            float floatvalue =0f;
                            if (this.CachedInstanceList.ContainsKey(key))
                            {
                                floatvalue = (float)this.CachedInstanceList[key] ;
                            }

                            floatvalue = EditorGUILayout.FloatField(floatvalue);
                            this.CachedInstanceList[key] = floatvalue;
                        }
                    }
                }
            }

            if (sure != null)
            {
                if (GUILayout.Button("确定"))
                {
                    List<object> list = new List<object>(CachedInstanceList.Values);
                    sure(list);
                    GetWindow<UtilityCanOrCantWindows>().Close();
                }
            }

            if (cancel != null)
            {
                if (GUILayout.Button("取消"))
                {
                    List<object> list = new List<object>(CachedInstanceList.Values);
                    cancel(list);
                    GetWindow<UtilityCanOrCantWindows>().Close();
                }
            }

            if (sure == null && cancel == null)
            {
                if (GUILayout.Button("确定"))
                {
                    GetWindow<UtilityCanOrCantWindows>().Close();
                }
            }
        }

        if (onguifunc != null)
            onguifunc();
    }
}