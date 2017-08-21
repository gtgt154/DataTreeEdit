using UnityEngine;
using System.Text;
using System.Collections.Generic;

public enum NodeState
{
    In,
    Out,
}

public struct Node
{
    public NodeState state;

    public int idx;
}

public struct NodeLine
{
    public Node childNode;
    public Node parentNode;
}

public class DataTreeEditCtr
{
    private static List<Node> m_nodeList = new List<Node>();

    public static List<NodeLine> m_nodeLineList = new List<NodeLine>();

    public static void AddNode(Node node)
    {
        m_nodeList.Add(node);
        if (m_nodeList.Count > 1)
        {
            if (m_nodeList[1].state == m_nodeList[0].state)
            {
                m_nodeList.Remove(m_nodeList[0]);
            }
            else
            {
                AddNodeLine();
            }
        }
    }

    public static void AdjustNodeList(List<NodeData> nodeList, Vector2 offset, float value)
    {
        for (int i = 0; i < nodeList.Count; ++i)
        {
            nodeList[i].AdjustNode(offset, value);
        }
    }

    public static void RemoveLineByNode(int idx)
    {
        for (int i = m_nodeList.Count - 1; i >= 0; --i)
        {
            if (m_nodeList[i].idx == idx)
            {
                m_nodeList.Remove(m_nodeList[i]);
            }
        }

        for (int i = m_nodeLineList.Count - 1; i >= 0; --i)
        {
            if (m_nodeLineList[i].childNode.idx == idx || m_nodeLineList[i].parentNode.idx == idx)
            {
                m_nodeLineList.Remove(m_nodeLineList[i]);
            }
        }
    }

    public static string GenTreeInfos()
    {
        List<int> parentList = new List<int>();
        List<int> childList = new List<int>();
        Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
        for (int i = 0; i < m_nodeLineList.Count; ++i)
        {
            if (!dic.ContainsKey(m_nodeLineList[i].parentNode.idx))
            {
                List<int> list = new List<int>();
                list.Add(m_nodeLineList[i].childNode.idx);
                dic.Add(m_nodeLineList[i].parentNode.idx, list);
            }
            else
            {
                bool flag = true;
                List<int> list = dic[m_nodeLineList[i].parentNode.idx];
                for (int j = 0; j < list.Count; ++j)
                {
                    if (list[j] == m_nodeLineList[i].childNode.idx)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    list.Add(m_nodeLineList[i].childNode.idx);
                }
            }

            parentList.Add(m_nodeLineList[i].parentNode.idx);
            childList.Add(m_nodeLineList[i].childNode.idx);
        }

        List<int> rootList = new List<int>();
        for (int i = 0; i < parentList.Count; ++i)
        {
            bool flag = true;
            for (int j = 0; j < childList.Count; ++j)
            {
                if (parentList[i] == childList[j])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                for (int k = 0; k < rootList.Count; ++k)
                {
                    if (parentList[i] == rootList[k])
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    rootList.Add(parentList[i]);
                }
            }
        }

        StringBuilder sb = new StringBuilder(2048);
        for (int i = 0; i < rootList.Count; ++i)
        {
            if (i < rootList.Count - 1)
            {
                sb.Append(rootList[i] + ",");
            }
            else
            {
                sb.Append(rootList[i] + "\r\n");
            }
        }

        List<int> keyList = new List<int>(dic.Keys);
        for (int i = 0; i < keyList.Count; ++i)
        {
            sb.Append(keyList[i] + ":");
            for (int j = 0; j < dic[keyList[i]].Count; ++j)
            {
                if (j < dic[keyList[i]].Count - 1)
                {
                    sb.Append(dic[keyList[i]][j] + ",");
                }
                else
                {
                    sb.Append(dic[keyList[i]][j] + "\r\n");
                }
            }
        }
        return sb.ToString();
    }

    public static void ClearAllData()
    {
        m_nodeList.Clear();
        m_nodeLineList.Clear();
    }

    private static void AddNodeLine()
    {
        if (m_nodeList.Count == 2)
        {
            NodeLine line = new NodeLine();
            if (m_nodeList[0].state == NodeState.In)
            {
                line.childNode = m_nodeList[0];
                line.parentNode = m_nodeList[1];
            }
            else
            {
                line.childNode = m_nodeList[1];
                line.parentNode = m_nodeList[0];
            }
            m_nodeLineList.Add(line);
            m_nodeList.Clear();
        }
    }
}