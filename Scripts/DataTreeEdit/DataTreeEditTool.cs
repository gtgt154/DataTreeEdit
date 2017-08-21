using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class DataTreeEditTool
{
    private static Dictionary<int, List<string>> dic = new Dictionary<int, List<string>>();

    private static string curpath;

    private static Matrix4x4 prevGuiMatrix;

    private static Rect groupRect = default(Rect);

    public static void LoadCsv_List<T>(string path, List<T> list)
    {
        try
        {
            if (list != null)
                list.Clear();

            path = string.Format("{0}/CSV/{1}.csv", Application.dataPath, path);
            curpath = path;
            CreateCsvClass<T>(File.ReadAllText(curpath), list);
        }
        catch (Exception ex)
        {
            Debug.LogError(curpath + " 发生异常:" + ex);
        }
    }

    private static void CreateCsvClass<T>(string Content, List<T> instances)
    {
        dic.Clear();
        fgCSVReader.LoadFromString(Content, new fgCSVReader.ReadLineDelegate(ReadLineTest));

        Type type = typeof(T);
        List<FieldInfo> fields = new List<FieldInfo>();

        List<int> keys = new List<int>(dic.Keys);
        for (int i = 0; i < keys.Count; ++i)
        {
            if (i == 0)
            {
                List<string> values = dic[keys[i]];
                for (int j = 0; j < values.Count; ++j)
                {
                    fields.Add(type.GetField(values[j].Trim(), BindingFlags.Instance | BindingFlags.NonPublic));
                }
            }
            else if (i == 1)
            {
                //List<string> values = this.dic[keys[i]];
                //for (int j = 0; j < values.Count; ++j)
                //{
                //    if (fields[j].FieldType.ToString().ToLower() != values[j].ToLower())
                //    {
                //        LogMgr.LogError("类型不匹配 " + fields[j].FieldType + " in csv " + values[j]);
                //    }
                //}
            }
            else
            {
                object ins = Activator.CreateInstance(type, true);
                instances.Add((T)ins);

                List<string> values = dic[keys[i]];
                for (int j = 0; j < values.Count; ++j)
                {
                    if (string.IsNullOrEmpty(values[j]))
                        continue;

                    FieldInfo field = fields[j];
                    if (field == null)
                    {
                        continue;
                    }

                    if (field.FieldType == typeof(Int32))
                    {
                        int target;
                        if (int.TryParse(values[j], out target))
                        {
                            fields[j].SetValue(ins, target);
                        }
                        else
                        {
                            Debug.LogErrorFormat("int转换失败 :{0} 对于 {1}", values[j], type);
                        }

                    }
                    else if (field.FieldType == typeof(String))
                    {
                        fields[j].SetValue(ins, values[j]);
                    }
                    else if (field.FieldType == typeof(Vector3))
                    {
                        string content = values[j];
                        string[] contentarray = content.Split(',');
                        if (contentarray.Length == 3)
                        {
                            fields[j].SetValue(ins, new Vector3(float.Parse(contentarray[0]), float.Parse(contentarray[1]), float.Parse(contentarray[2])));
                        }
                        else
                        {
                            Debug.LogErrorFormat("内容异常 {0}", Content);
                        }
                    }
                    else if (field.FieldType == typeof(Quaternion))
                    {
                        string content = values[j];
                        string[] contentarray = content.Split(',');
                        if (contentarray.Length == 3)
                        {
                            fields[j].SetValue(ins, Quaternion.Euler(float.Parse(contentarray[0]), float.Parse(contentarray[1]), float.Parse(contentarray[2])));
                        }
                        else
                            Debug.LogErrorFormat("内容异常 {0}", Content);
                    }
                    else
                    {
                        Debug.LogErrorFormat("不支持的类型 :{0} csv中类型为 :{1} 对于 {2}", field.FieldType, values[j], type);
                    }

                }
            }
        }
    }

    private static void ReadLineTest(int line_index, List<string> line)
    {
        Debug.LogFormat("\n==> Line {0}, {1} column(s)", line_index, line.Count);
        for (int i = 0; i < line.Count; i++)
        {
            Debug.LogFormat("Cell {0}: *{1}*", i, line[i]);
        }

        dic[line_index] = new List<string>(line);
    }

    public static GUIStyle GUILabelType(TextAnchor anchor = TextAnchor.UpperCenter)
    {
        GUIStyle labelstyle = GUI.skin.GetStyle("Label");
        labelstyle.alignment = anchor;
        return labelstyle;
    }

    public static T GUIobject_CaneditArea<T>(string content,T data, bool surtecancel = true, Action enableFunc = null) where T:UnityEngine.Object
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(content);
        data = (T)EditorGUILayout.ObjectField(data, typeof(T),false);

        if (GUILayout.Button("确定"))
        {
            if (enableFunc != null)
                enableFunc();
        }

        GUILayout.EndHorizontal();
        return data;
    }

    public static string GUILabel_CaneditArea(string content,string defualttext,bool surtecancel= true,Action enableFunc =null)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(content);
        string str= GUILayout.TextField(defualttext);

        if (GUILayout.Button("确定"))
        {
            if (enableFunc != null)
                enableFunc();
        }

        GUILayout.EndHorizontal();
        return str;
    }

    public static void CreateSplit()
    {
        GUILayout.Label("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    }

    public static Vector2 TopLeft(Rect rect)
    {
        return new Vector2(rect.xMin, rect.yMin);
    }

    public static Rect ScaleSizeBy(Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    public static Rect Begin(Rect screenCoordsArea, float zoomScale)
    {
        GUI.EndGroup();
        Rect rect = ScaleSizeBy(screenCoordsArea, 1f / zoomScale, TopLeft(screenCoordsArea));
        rect.y += 21f;
        GUI.BeginGroup(rect);
        prevGuiMatrix = GUI.matrix;
        Matrix4x4 lhs = Matrix4x4.TRS(TopLeft(rect), Quaternion.identity, Vector3.one);
        Vector3 one = Vector3.one;
        one.y = zoomScale;
        one.x = zoomScale;
        Matrix4x4 rhs = Matrix4x4.Scale(one);
        GUI.matrix = lhs * rhs * lhs.inverse * GUI.matrix;
        return rect;
    }

    public static void End()
    {
        GUI.matrix = prevGuiMatrix;
        GUI.EndGroup();
        groupRect.y = 21f;
        groupRect.width = (float)Screen.width;
        groupRect.height = (float)Screen.height;
        GUI.BeginGroup(groupRect);
    }
}