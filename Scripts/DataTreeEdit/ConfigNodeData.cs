using UnityEngine;

public class ConfigNodeData : NodeData
{
    private ConfigData m_configData = null;

    private GUIStyle m_labelstyle = null;

    public ConfigData configData
    {
        set {
            this.m_configData = value;
        }

        get
        {
            return this.m_configData;
        }
    }

    public ConfigNodeData(ConfigData data, Vector2 pos, bool preview = false)
    {
        this.m_configData = data;
        this.Pos = pos;
        this.m_isPreview = preview;
        this.Init();
    }

    public override void Show(Vector2 vector, bool flag = false)
    {
        base.Show(vector, flag);

        if (this.m_configData != null)
        {
            m_labelstyle.normal.textColor = Color.black;
            if (this.IsClick && !this.IsPreview)
            {
                GUI.DrawTexture(new Rect(this.Pos + new Vector2(-3, 0), this.m_size + new Vector2(6, -5)), this.m_selectImage);
            }
            GUI.DrawTexture(new Rect(this.Pos, this.m_size), this.image);
            GUI.Label(new Rect(new Vector2(this.Pos.x + 10, this.Pos.y + this.m_size.y / 2 - 15), new Vector2(this.m_size.x - 20, 30)), this.m_configData.GetconfigDataName(), m_labelstyle);
        }
        else
        {
            m_labelstyle.normal.textColor = Color.red;
            GUI.Label(new Rect(new Vector2(this.Pos.x + 10, this.Pos.y + this.m_size.y / 2 - 15), new Vector2(this.m_size.x - 20, 30)), "配置数据为空", m_labelstyle);
        }
    }
    
    protected override void Init()
    {
        if (this.m_configData != null)
        {
            this.m_idx = this.m_configData.GetconfigDataID();
        }

        if (this.image == null)
        {
            this.image = Resources.Load("EditorRes/box") as Texture;
        }

        if (this.m_selectImage == null)
        {
            this.m_selectImage = Resources.Load("EditorRes/select") as Texture;
        }

        if (m_labelstyle == null)
        {
            m_labelstyle = new GUIStyle();
            m_labelstyle.alignment = TextAnchor.MiddleCenter;
            m_labelstyle.fontSize = 12;
        }

        base.Init();
    }
}