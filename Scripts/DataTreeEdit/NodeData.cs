using UnityEngine;

public class NodeData : GUIContent
{
    protected Texture m_selectImage = null;

    protected int m_idx = -1;

    private Vector2 m_pos;

    private Vector2 m_posUp;

    private Vector2 m_posDown;

    protected Vector2 m_size = new Vector2(150, 100);

    protected Vector2 m_btnSize = new Vector2(70, 25);

    protected GUIStyle m_buttonDownStyle = null;

    protected bool m_isPreview;

    protected bool m_isDrag = false;

    protected bool m_isClick = false;

    public int Idx
    {
        get
        {
            return this.m_idx;
        }
    }

    public Vector2 Pos
    {
        set
        {
            this.m_pos = value;
        }
        get
        {
            return this.m_pos;
        }
    }

    public Vector2 Size
    {
        get
        {
            return this.m_size;
        }
    }

    public bool IsPreview
    {
        get
        {
            return this.m_isPreview;
        }
    }

    public bool IsDrag
    {
        set
        {
            this.m_isDrag = value;
        }

        get
        {
            return this.m_isDrag;
        }
    }

    public bool IsClick
    {
        set
        {
            this.m_isClick = value;
        }

        get
        {
            return this.m_isClick;
        }
    }

    protected NodeData()
    {

    }

    public virtual void Show(Vector2 vector, bool flag = false)
    {
        if (flag)
        {
            AdjustNode(vector, 1);

            if (GUI.Button(new Rect(this.m_posUp, this.m_btnSize), "", m_buttonDownStyle))
            {
                Node node = new Node();
                node.state = NodeState.In;
                node.idx = this.m_idx;
                DataTreeEditCtr.AddNode(node);
            }
            if (GUI.Button(new Rect(this.m_posDown, this.m_btnSize), "", m_buttonDownStyle))
            {
                Node node = new Node();
                node.state = NodeState.Out;
                node.idx = this.m_idx;
                DataTreeEditCtr.AddNode(node);
            }
        }
        this.HandleEvents();
    }
    
    protected virtual void Init()
    {
        if (m_buttonDownStyle == null)
        {
            m_buttonDownStyle = new GUIStyle();
            m_buttonDownStyle.normal.background = Resources.Load("EditorRes/down") as Texture as Texture2D;
        }

        this.m_posUp = new Vector2(this.m_pos.x + 40, this.m_pos.y - 18);
        this.m_posDown = new Vector2(this.m_pos.x + 40, this.m_pos.y + this.m_size.y - 12);
    }
    
    public void AdjustNode(Vector2 offset, float value)
    {
        this.m_pos.x += offset.x;
        this.m_pos.y += offset.y;
        this.m_size.x = 150 * value;
        this.m_size.y = 100 * value;

        this.m_posUp.x = this.m_pos.x + 40 * value;
        this.m_posUp.y = this.m_pos.y - 18 * value;
        this.m_btnSize.x = 70 * value;
        this.m_btnSize.y = 25 * value;

        this.m_posDown.x = this.m_pos.x + 40 * value;
        this.m_posDown.y = this.m_pos.y + this.m_size.y - 12 * value;
        this.m_btnSize.x = 70 * value;
        this.m_btnSize.y = 25 * value;
    }

    private void HandleEvents()
    {
        if (Event.current.type == EventType.MouseDrag)
        {
            if (Event.current.button == 0 && new Rect(this.m_pos, this.m_size).Contains(Event.current.mousePosition))
            {
                this.m_isDrag = true;
                this.m_pos.x += Event.current.delta.x;
                this.m_pos.y += Event.current.delta.y;
                this.m_posUp.x += Event.current.delta.x;
                this.m_posUp.y += Event.current.delta.y;
                this.m_posDown.x += Event.current.delta.x;
                this.m_posDown.y += Event.current.delta.y;

                //if (this.m_pos.x > 300 && this.m_pos.x < 1600 && this.m_pos.y > 10 && this.m_pos.y < 960)
                //{
                //    this.m_isPreview = false;
                //}
                //else
                //{
                //    this.m_isPreview = true;
                //}

                Event.current.Use();
            }
        }
        else if (Event.current.type == EventType.MouseUp)
        {
            if (Event.current.button == 0)
            {
                if (this.m_isDrag)
                {
                    if (this.m_isPreview && this.m_pos.x > 300 && this.m_pos.x < 1600 && this.m_pos.y > 10 && this.m_pos.y < 960)
                    {
                        this.m_isPreview = false;
                    }

                    this.m_isClick = true;
                }
                else if(new Rect(this.m_pos, this.m_size).Contains(Event.current.mousePosition))
                {
                    this.m_isClick = !this.m_isClick;
                }
            }
        }
    }
}