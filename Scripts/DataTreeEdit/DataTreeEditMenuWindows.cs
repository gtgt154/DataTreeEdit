using UnityEngine;
using UnityEditor;
using System.IO;

enum DataTreeEditWindowType
{
    Create,
    Load,
}

public class DataTreeEditMenuWindows : EditorWindow
{
    private static DataTreeEditWindowType type;

    [MenuItem("数据树编辑/新建数据树")]
    public static void CreateEditDataTree()
    {
        type = DataTreeEditWindowType.Create;

        DataTreeEditMenuWindows windows = EditorWindow.GetWindow<DataTreeEditMenuWindows>();

        windows.autoRepaintOnSceneChange = true;
        windows.titleContent = new GUIContent("新建数据树");
        windows.minSize = new Vector2(300, 300);

        windows.Show();
    }

    [MenuItem("数据树编辑/读取数据树")]
    public static void LoadEditDataTree()
    {
        type = DataTreeEditWindowType.Load;

        DataTreeEditMenuWindows windows = EditorWindow.GetWindow<DataTreeEditMenuWindows>();

        windows.autoRepaintOnSceneChange = true;
        windows.titleContent = new GUIContent("读取数据树");
        windows.minSize = new Vector2(300, 300);

        windows.Show();
    }

    private TextAsset data;

    void OnGUI()
    {
        if (type == DataTreeEditWindowType.Create)
        {
            OnCreateGUI();
        }
        else if (type == DataTreeEditWindowType.Load)
        {
            OnLoadGUI();
        }
    }

    private void OnCreateGUI()
    {
        DataTreeEditTool.GUILabelType();

        GUILayout.Label("数据集选择");

        DataTreeEditTool.GUILabelType(TextAnchor.UpperLeft);

        data = DataTreeEditTool.GUIobject_CaneditArea("加载数据集文件", data, true, LoadData);

        GUILayout.Space(2);
    }

    private void OnLoadGUI()
    {
        DataTreeEditTool.GUILabelType();

        GUILayout.Label("数据树选择");

        DataTreeEditTool.GUILabelType(TextAnchor.UpperLeft);

        data = DataTreeEditTool.GUIobject_CaneditArea("加载数据树文件", data, true, LoadData);

        GUILayout.Space(2);
    }

    void OnDestroy()
    {
        
    }

    private void LoadData()
    {
        if (Application.isPlaying)
        {
            UtilityCanOrCantWindows.CreateWindows("Error", "正在运行请停止后重试", null, null);
            return;
        }

        if (data != null)
        {
            string path = AssetDatabase.GetAssetPath(data);
            if (type == DataTreeEditWindowType.Create && !Path.GetExtension(path).Equals(".csv"))
            {
                EditorUtility.DisplayDialog("提示", "需要读取的数据集错误！", "确定");
                return;
            }

            if (type == DataTreeEditWindowType.Load && !data.name.Contains("Tree"))
            {
                EditorUtility.DisplayDialog("提示", "需要读取的数据树错误！", "确定");
                return;
            }

            DataTreeEditWindows windows = EditorWindow.GetWindow<DataTreeEditWindows>();

            windows.autoRepaintOnSceneChange = true;
            windows.titleContent = new GUIContent("编辑数据树");
            windows.minSize = new Vector2(1600, 1000);
            windows.InitData(data.name);

            windows.Show();

            this.Close();
        }
    }
}
