using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/UICurvyText", 15)]
[RequireComponent(typeof(Text))]
public class UICurvyText : BaseMeshEffect
{
    [SerializeField]
    private float m_Radius = 100f;

    public float Radius
    {
        get { return m_Radius; }
        set
        {
            if (m_Radius == value) return;
            m_Radius = value;
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }
    }
    
    private Text _textComponent;
    public Text TextComponent
    {
        get
        {
            if (_textComponent == null)
            {
                _textComponent = GetComponent<Text>();
            }
            return _textComponent;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }
#endif

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }
        
        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        ModifyVertices(vertexList);

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }

    private void ModifyVertices(List<UIVertex> verts)
    {
        if (verts.Count == 0 || TextComponent == null)
            return;

        var rectTransform = this.transform as RectTransform;
        if (rectTransform == null) return;
        
        if (m_Radius == 0 || Mathf.Abs(m_Radius) < rectTransform.rect.width / 2f)
        {
            return;
        }
        
        var rect = rectTransform.rect;
        float halfWidth = rect.width / 2f;
        
        float angleRadAtEdge = Mathf.Asin(halfWidth / Mathf.Abs(m_Radius));
        float totalArcAngle = 2f * angleRadAtEdge;
        
        float circleCenterY = -Mathf.Sign(m_Radius) * Mathf.Sqrt(m_Radius * m_Radius - halfWidth * halfWidth);
        Vector3 circleCenter = new Vector3(rect.center.x, circleCenterY);

        for (int i = 0; i < verts.Count; i += 6)
        {
            var topLeft = verts[i + 0];
            var topRight = verts[i + 1];
            var bottomRight = verts[i + 2];
            var bottomLeft = verts[i + 4];

            var blPos = bottomLeft.position;
            var brPos = bottomRight.position;
            float charHeight = (topLeft.position - blPos).magnitude;
            float charWidth = (brPos - blPos).magnitude;

            float normX_left = (blPos.x - rect.xMin) / rect.width;
            float normX_right = (brPos.x - rect.xMin) / rect.width;

            float angleRad_left = (normX_left - 0.5f) * totalArcAngle;
            float angleRad_right = (normX_right - 0.5f) * totalArcAngle;

            Vector3 arcPos_left = new Vector3(m_Radius * Mathf.Sin(angleRad_left), m_Radius * Mathf.Cos(angleRad_left), 0) + circleCenter;
            Vector3 arcPos_right = new Vector3(m_Radius * Mathf.Sin(angleRad_right), m_Radius * Mathf.Cos(angleRad_right), 0) + circleCenter;
            
            Vector3 direction = (arcPos_right - arcPos_left).normalized;
            Vector3 normal = (arcPos_left - circleCenter).normalized;

            Vector3 final_bl_pos = arcPos_left + normal * blPos.y;
            
            Vector3 final_br_pos = final_bl_pos + direction * charWidth;
            Vector3 final_tl_pos = final_bl_pos + normal * charHeight;
            Vector3 final_tr_pos = final_br_pos + normal * charHeight;

            bottomLeft.position = final_bl_pos;
            bottomRight.position = final_br_pos;
            topLeft.position = final_tl_pos;
            topRight.position = final_tr_pos;

            verts[i + 0] = topLeft;
            verts[i + 1] = topRight;
            verts[i + 2] = bottomRight;
            verts[i + 3] = bottomRight;
            verts[i + 4] = bottomLeft;
            verts[i + 5] = topLeft;
        }
    }
}
