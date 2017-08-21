using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;

public struct TreeNode
{
    public int idx;
    public List<int> childList;
}

public class DataTreeEditWindows : EditorWindow
{
    private string outputPath = Application.dataPath + "/DataTree/";
    
    private string mName;

    private GUIStyle graphBackgroundGUIStyle;

    private Material mGridMaterial;

    private Material mLineMaterial;

    private float mGraphZoom = 1f;

    private Rect mGraphRect;

    private Vector2 mPos = new Vector2(75, 875);

    private Vector2 mGraphScrollSize = new Vector2(20000f, 20000f);

    private Vector2 mGraphScrollPosition = new Vector2(-1f, -1f);

    private Vector2 mCurrentMousePosition = Vector2.zero;

    private Vector2 mGraphOffset = Vector2.one;

    private Vector2 m_dataScrollPosition;

    private bool m_init = false;

    private Dictionary<int, bool> m_conDic = new Dictionary<int, bool>();

    private Dictionary<int, List<int>> m_nodeDic = new Dictionary<int, List<int>>();

    private List<Queue<TreeNode>> m_treeList = new List<Queue<TreeNode>>();
    
    private List<int> m_conList = new List<int>();

    private int m_selectKey = -1;

    private int m_idx = -1;

    private List<NodeData> m_nodeList = new List<NodeData>();

    #region tank

    private List<ConfigData> m_configDataList;

    private ConfigNodeData m_configNodeData;

    #endregion


    public void InitData(string name)
    {
        this.mName = name;
        this.m_init = false;
        this.m_selectKey = -1;
        this.m_idx = -1;
        this.m_conDic.Clear();
        this.m_conList.Clear();
        this.m_nodeList.Clear();
        DataTreeEditCtr.ClearAllData();

        if (this.graphBackgroundGUIStyle == null)
        {
            this.InitGraphBackgroundGUIStyle();
        }

        if (this.mGridMaterial == null)
        {
            this.mGridMaterial = new Material(Shader.Find("Hidden/Behavior Designer/Grid"));
            this.mGridMaterial.hideFlags = HideFlags.HideAndDontSave;
            this.mGridMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        if (this.mLineMaterial == null)
        {
            this.mLineMaterial = new Material(Shader.Find("DataTreeEditor/Line"));
            this.mLineMaterial.hideFlags = HideFlags.HideAndDontSave;
            this.mLineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        if (name.Equals("Config") || name.Equals("ConfigTree"))
        {
            if (this.m_configDataList == null)
            {
                this.m_configDataList = new List<ConfigData>();
            }
            else
            {
                this.m_configDataList.Clear();
            }
            DataTreeEditTool.LoadCsv_List<ConfigData>("Config", this.m_configDataList);

            this.m_init = true;

            this.m_conDic.Add(-1, true);
            for (int i = 0; i < this.m_configDataList.Count; ++i)
            {
                if (!this.m_conDic.ContainsKey(this.m_configDataList[i].GetconfigDataCondition()))
                {
                    this.m_conDic.Add(this.m_configDataList[i].GetconfigDataCondition(), false);
                }
            }

            this.m_conList = new List<int>(this.m_conDic.Keys);

            if (name.Equals("ConfigTree"))
            {
                this.LoadFileData();
            }
        }
    }

    void OnGUI()
    {
        if (this.m_init)
        {
            this.mCurrentMousePosition = Event.current.mousePosition;

            this.SetupSizes();
            this.Draw();
            this.HandleEvents();
        }
    }

    private void Clear()
    {

    }

    void OnDestroy()
    {
        
    }

    private void InitGraphBackgroundGUIStyle()
    {
        Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
        if (EditorGUIUtility.isProSkin)
        {
            texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
        }
        else
        {
            texture2D.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
        }
        texture2D.hideFlags = HideFlags.HideAndDontSave;
        texture2D.Apply();
        this.graphBackgroundGUIStyle = new GUIStyle(GUI.skin.box);
        this.graphBackgroundGUIStyle.normal.background = texture2D;
        this.graphBackgroundGUIStyle.active.background = texture2D;
        this.graphBackgroundGUIStyle.hover.background = texture2D;
        this.graphBackgroundGUIStyle.focused.background = texture2D;
        this.graphBackgroundGUIStyle.normal.textColor = Color.white;
        this.graphBackgroundGUIStyle.active.textColor = Color.white;
        this.graphBackgroundGUIStyle.hover.textColor = Color.white;
        this.graphBackgroundGUIStyle.focused.textColor = Color.white;
    }

    private void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Height(1000), GUILayout.Width(300));

        if (GUILayout.Button("清空编辑树数据"))
        {
            this.ClearEditTreeData();
        }

        if (GUILayout.Button("导出编辑树数据"))
        {
            this.ExportToFile();
        }

        GUILayout.Label("条件筛选:");
        for (int i = 0; i < this.m_conList.Count; ++i)
        {
            if (this.m_selectKey == this.m_conList[i])
            {
                this.m_conDic[m_conList[i]] = true;
            }
            else
            {
                this.m_conDic[m_conList[i]] = false;
            }
            this.m_conDic[this.m_conList[i]] = GUILayout.Toggle(this.m_conDic[this.m_conList[i]], this.m_conList[i].ToString());
            if (this.m_conDic[m_conList[i]])
            {
                this.m_selectKey = this.m_conList[i];
            }
        }

        GUILayout.Label("可选择节点数据:");
        this.m_dataScrollPosition = GUILayout.BeginScrollView(this.m_dataScrollPosition, GUILayout.Height(720), GUILayout.Width(300));
        for (int i = 0; i < this.m_configDataList.Count; ++i)
        {
            if (!this.CheckNodeUsed(this.m_configDataList[i].GetconfigDataID()) && (this.m_selectKey == -1 || this.m_configDataList[i].GetconfigDataCondition() == this.m_selectKey))
            {
                if (GUILayout.Button(this.m_configDataList[i].GetconfigDataName()))
                {
                    this.m_idx = i;
                }
            }
        }
        GUILayout.EndScrollView();

        GUI.DrawTexture(new Rect(0, 850, 290, 150), Resources.Load("EditorRes/backgroud") as Texture);
        
        GUILayout.EndVertical();
        
        this.DrawGraphArea();
        
        GUILayout.EndHorizontal();

        if (this.m_idx != -1)
        {
            if (this.m_configNodeData == null)
            {
                this.m_configNodeData = new ConfigNodeData(this.m_configDataList[m_idx], this.mPos, true);
            }
            else
            {
                this.m_configNodeData.configData = this.m_configDataList[m_idx];
            }
            this.m_configNodeData.Show(this.m_configNodeData.Pos);
        }

        
    }

    private void SetupSizes()
    {
        this.mGraphRect = new Rect(300f, 0f, (float)(Screen.width - 300 - 15), (float)(Screen.height - 36 - 21));
        if (this.mGraphScrollPosition == new Vector2(-1f, -1f))
        {
            this.mGraphScrollPosition = (this.mGraphScrollSize - new Vector2(this.mGraphRect.width, this.mGraphRect.height)) / 2f - 2f * new Vector2(15f, 15f);
        }
    }
    
    private void DrawGraphArea()
    {
        Vector2 v = Vector2.zero;
        if (Event.current.type != EventType.scrollWheel)
        {
            Vector2 vector = GUI.BeginScrollView(new Rect(this.mGraphRect.x, this.mGraphRect.y, this.mGraphRect.width + 15f, this.mGraphRect.height + 15f), this.mGraphScrollPosition, new Rect(0f, 0f, this.mGraphScrollSize.x, this.mGraphScrollSize.y), true, true);
            if (vector != this.mGraphScrollPosition && Event.current.type != EventType.DragUpdated && Event.current.type != EventType.Ignore)
            {
                v = (this.mGraphScrollPosition - vector) / this.mGraphZoom;
                this.mGraphOffset -= (vector - this.mGraphScrollPosition) / this.mGraphZoom;
                this.mGraphScrollPosition = vector;
            }
            GUI.EndScrollView();
        }
        GUI.Box(this.mGraphRect, string.Empty, graphBackgroundGUIStyle);
        this.DrawGrid();

        DataTreeEditTool.Begin(this.mGraphRect, this.mGraphZoom);
        this.DrawConnectionLines();

        for (int i = 0; i < this.m_nodeList.Count; ++i)
        {
            this.m_nodeList[i].Show(v, true);
        }
        DataTreeEditTool.End();
        
    }

    private void DrawGrid()
    {
        this.mGridMaterial.SetPass((!EditorGUIUtility.isProSkin) ? 1 : 0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        this.DrawGridLines(10f * this.mGraphZoom, new Vector2(this.mGraphOffset.x % 10f * this.mGraphZoom, this.mGraphOffset.y % 10f * this.mGraphZoom));
        GL.End();
        GL.PopMatrix();
        this.mGridMaterial.SetPass((!EditorGUIUtility.isProSkin) ? 3 : 2);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        this.DrawGridLines(50f * this.mGraphZoom, new Vector2(this.mGraphOffset.x % 50f * this.mGraphZoom, this.mGraphOffset.y % 50f * this.mGraphZoom));
        GL.End();
        GL.PopMatrix();
    }

    private void DrawGridLines(float gridSize, Vector2 offset)
    {
        float num = this.mGraphRect.x + offset.x;
        if (offset.x < 0f)
        {
            num += gridSize;
        }
        for (float num2 = num; num2 < this.mGraphRect.x + this.mGraphRect.width; num2 += gridSize)
        {
            this.DrawLine(new Vector2(num2, this.mGraphRect.y), new Vector2(num2, this.mGraphRect.y + this.mGraphRect.height));
        }
        float num3 = this.mGraphRect.y + offset.y;
        if (offset.y < 0f)
        {
            num3 += gridSize;
        }
        for (float num4 = num3; num4 < this.mGraphRect.y + this.mGraphRect.height; num4 += gridSize)
        {
            this.DrawLine(new Vector2(this.mGraphRect.x, num4), new Vector2(this.mGraphRect.x + this.mGraphRect.width, num4));
        }
    }

    private void DrawLine(Vector2 p1, Vector2 p2)
    {
        GL.Vertex(p1);
        GL.Vertex(p2);
    }

    private void DrawNodeLine(Vector2 p1, Vector2 p2)
    {
        Vector3 up = p1;
        Vector3 down = p2;
        if (p2.y < p1.y)
        {
            up = p2;
            down = p1;
        }
        float dy = down.y - up.y;

        GL.Vertex(up);
        GL.Vertex(new Vector3(up.x, up.y + dy / 2, 0));
        GL.Vertex(new Vector3(up.x, up.y + dy / 2, 0));
        GL.Vertex(new Vector3(down.x, down.y - dy / 2, 0));
        GL.Vertex(new Vector3(down.x, down.y - dy / 2, 0));
        GL.Vertex(down);
    }

    private void HandleEvents()
    {
        if (Event.current.type == EventType.ScrollWheel)
        {
            if (this.MouseZoom())
            {
                Event.current.Use();
            }
        }
        else if (Event.current.type == EventType.MouseDrag)
        {
            Event.current.Use();
        }
        else if (Event.current.type == EventType.MouseUp)
        {
            this.DragLogic();
            Event.current.Use();
        }
        else if (Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Delete)
            {
                this.DeleteNodes();
                Event.current.Use();
            }
        }
    }

    private bool MouseZoom()
    {
        Vector2 vector;
        if (!this.GetMousePositionInGraph(out vector))
        {
            return false;
        }
        float num = -Event.current.delta.y / 150f;
        this.mGraphZoom += num;
        this.mGraphZoom = Mathf.Clamp(this.mGraphZoom, 0.4f, 1f);
        Vector2 vector2;
        this.GetMousePositionInGraph(out vector2);
        this.mGraphOffset += vector2 - vector;
        this.mGraphScrollPosition += vector2 - vector;
        DataTreeEditCtr.AdjustNodeList(this.m_nodeList, (vector2- vector), this.mGraphZoom);
        return true;
    }
    
    private bool GetMousePositionInGraph(out Vector2 mousePosition)
    {
        mousePosition = this.mCurrentMousePosition;
        if (!this.mGraphRect.Contains(mousePosition))
        {
            return false;
        }
        mousePosition -= new Vector2(this.mGraphRect.xMin, this.mGraphRect.yMin);
        mousePosition /= this.mGraphZoom;
        return true;
    }

    private void DragLogic()
    {
        if (this.m_configNodeData != null && this.m_configNodeData.IsDrag)
        {
            this.m_configNodeData.IsDrag = false;

            if (this.m_configNodeData.IsPreview)
            {
                this.m_configNodeData.Pos = this.mPos;
            }
            else
            {
                ConfigNodeData data = new ConfigNodeData(this.m_configNodeData.configData, this.m_configNodeData.Pos);
                this.m_nodeList.Add(data);
                this.m_idx = -1;
                this.m_configNodeData = null;
            }
        }
        else
        {
            for (int i = 0; i < this.m_nodeList.Count; ++i)
            {
                if (this.m_nodeList[i].IsDrag)
                {
                    this.m_nodeList[i].IsDrag = false;
                    break;
                }
            }
        }
    }

    private void DeleteNodes()
    {
        for (int i = this.m_nodeList.Count - 1; i >= 0 ; --i)
        {
            if (this.m_nodeList[i].IsClick)
            {
                DataTreeEditCtr.RemoveLineByNode(this.m_nodeList[i].Idx);
                this.m_nodeList.Remove(this.m_nodeList[i]);
            }
        }
        
    }

    private bool CheckNodeUsed(int idx)
    {
        for (int i = 0; i < this.m_nodeList.Count; ++i)
        {
            if (this.m_nodeList[i].Idx == idx)
            {
                return true;
            }
        }
        return false;
    }

    private void DrawConnectionLines()
    {
        if (DataTreeEditCtr.m_nodeLineList.Count > 0)
        {
            this.mLineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            for (int i = 0; i < DataTreeEditCtr.m_nodeLineList.Count; ++i)
            {
                int idx_1 = DataTreeEditCtr.m_nodeLineList[i].childNode.idx;
                int idx_2 = DataTreeEditCtr.m_nodeLineList[i].parentNode.idx;
                this.DrawNodeLine(this.GetNodeDataPositionByIdx(idx_1), this.GetNodeDataPositionByIdx(idx_2));
            }
            GL.End();
            GL.PopMatrix();
        }
        
    }
    
    private Vector2 GetNodeDataPositionByIdx(int idx)
    {
        Vector2 pos = Vector2.one;
        for (int i = 0; i < this.m_nodeList.Count; ++i)
        {
            if (this.m_nodeList[i].Idx == idx)
            {
                pos.x = this.m_nodeList[i].Pos.x + 150/ 2;
                pos.y = this.m_nodeList[i].Pos.y + 75 / 2;
                break;
            }
        }
        return pos;
    }

    private void ClearEditTreeData()
    {
        this.m_idx = -1;
        this.m_configNodeData = null;

        for (int i = this.m_nodeList.Count - 1; i >= 0; --i)
        {
            DataTreeEditCtr.RemoveLineByNode(this.m_nodeList[i].Idx);
            this.m_nodeList.Remove(this.m_nodeList[i]);
        }
        DataTreeEditCtr.ClearAllData();
    }

    private void ExportToFile()
    {
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string path = outputPath + this.mName + "Tree.txt";
        if (this.mName.Contains("Tree"))
        {
            path = outputPath + this.mName + ".txt";
        }
        using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write))
        {
            stream.SetLength(0);

            byte[] bys = System.Text.Encoding.UTF8.GetBytes(DataTreeEditCtr.GenTreeInfos());
            stream.Write(bys, 0, bys.Length);
            stream.Close();

            EditorUtility.DisplayDialog("提示", "数据导出完毕！", "确定");
        }
    }

    private void LoadFileData()
    {
        try
        {
            this.m_treeList.Clear();
            this.m_nodeDic.Clear();
            string filePath = outputPath + this.mName + ".txt";
            if (File.Exists(Path.GetFullPath(filePath)))
            {
                List<string> list = new List<string>(File.ReadAllLines(filePath));
                if (list.Count > 0)
                {
                    string[] roots = list[0].Split(',');
                    
                    for (int i = 1; i < list.Count; ++i)
                    {
                        string[] parent = list[i].Split(':');
                        string[] child = parent[1].Split(',');
                        int key = int.Parse(parent[0]);
                        if (!this.m_nodeDic.ContainsKey(key))
                        {
                            List<int> childList = new List<int>();
                            for (int j = 0; j < child.Length; ++j)
                            {
                                childList.Add(int.Parse(child[j]));
                            }
                            this.m_nodeDic.Add(key, childList);
                        }
                        else
                        {
                            Debug.Log("---------------- 重复的key : " + key);
                        }   
                    }

                    for (int j = 0; j < roots.Length; ++j)
                    {
                        TreeNode treeNode = new TreeNode();
                        treeNode.idx = int.Parse(roots[j]);
                        treeNode.childList = this.m_nodeDic[int.Parse(roots[j])];
                        Queue<TreeNode> queue = new Queue<TreeNode>();
                        queue.Enqueue(treeNode);
                        this.m_treeList.Add(queue);
                    }

                    if (this.mName.Equals("ConfigTree"))
                    {
                        this.GenTankTree();   
                    }
                }
                
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void GenTankTree()
    {
        for (int i = 0; i < this.m_treeList.Count; ++i)
        {
            Queue<TreeNode> queue = this.m_treeList[i];
            int level = 0;
            while (queue.Count > 0)
            {
                List<TreeNode> nodeList = new List<TreeNode>();
                while (queue.Count > 0)
                {
                    TreeNode node = queue.Dequeue();
                    bool flag = true;
                    for (int j = 0; j < nodeList.Count; ++j)
                    {
                        if (nodeList[j].idx == node.idx)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        nodeList.Add(node);
                    }
                }
                for (int j = 0; j < nodeList.Count; ++j)
                {
                    TreeNode treeNode = nodeList[j];

                    if (!this.CheckNodeUsed(treeNode.idx))
                    {
                        ConfigNodeData dataNode = new ConfigNodeData(this.GetConfigDataByIdx(treeNode.idx), new Vector2(500 + (j - (nodeList.Count - 1) / 2.0f) * 200, 300 + level * 200));
                        this.m_nodeList.Add(dataNode);
                    }
                    
                    for (int k = 0; k < treeNode.childList.Count; ++k)
                    {
                        TreeNode tNode = new TreeNode();
                        tNode.idx = treeNode.childList[k];

                        if (this.m_nodeDic.ContainsKey(treeNode.childList[k]))
                        {
                            tNode.childList = this.m_nodeDic[treeNode.childList[k]];
                        }
                        else
                        {
                            tNode.childList = new List<int>();
                        }

                        queue.Enqueue(tNode);

                        Node inNode = new Node();
                        inNode.state = NodeState.In;
                        inNode.idx = treeNode.childList[k];

                        Node outNode = new Node();
                        outNode.state = NodeState.Out;
                        outNode.idx = treeNode.idx;

                        DataTreeEditCtr.AddNode(inNode);
                        DataTreeEditCtr.AddNode(outNode);
                    }
                }

                ++level;
            }
        }
    }

    private ConfigData GetConfigDataByIdx(int idx)
    {
        ConfigData data = null;
        for (int i = 0; i < this.m_configDataList.Count; ++i)
        {
            if (idx == this.m_configDataList[i].GetconfigDataID())
            {
                data = this.m_configDataList[i];
                break;
            }
        }
        return data;
    }
}
