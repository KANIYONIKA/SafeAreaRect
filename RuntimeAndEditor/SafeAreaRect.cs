using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class SafeAreaRect : MonoBehaviour
{
    [System.Flags]
    public enum Edge
    {
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
    }

    [SerializeField]
    private Edge controlEdges = (Edge)~0;

    public Edge ControlEdges => controlEdges;

    private Rect lastSafeArea;
    private Vector2Int lastResolution;
    private Edge lastControlEdges;
#if UNITY_EDITOR
  private DrivenRectTransformTracker drivenRectTransformTracker = new DrivenRectTransformTracker();
#endif

    private void Update()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply(force: true);
    }

#if UNITY_EDITOR
  private void OnDisable()
  {
    drivenRectTransformTracker.Clear();
  }
#endif

    public void Apply(bool force = false)
    {
        var rectTransform = (RectTransform)transform;
        var safeArea = Screen.safeArea;
        var resolution = new Vector2Int(Screen.width, Screen.height);
        if (resolution.x == 0 || resolution.y == 0)
        {
            return;
        }
        if (!force)
        {
            if (rectTransform.anchorMax == Vector2.zero)
            {
                // Do apply.
                // ※Undoすると0になるので再適用させる
            }
            else if (lastSafeArea == safeArea && lastResolution == resolution && lastControlEdges == controlEdges)
            {
                return;
            }
        }
        this.lastSafeArea = safeArea;
        this.lastResolution = resolution;
        this.lastControlEdges = controlEdges;

#if UNITY_EDITOR
    drivenRectTransformTracker.Clear();
    drivenRectTransformTracker.Add(
        this,
        rectTransform,
        DrivenTransformProperties.AnchoredPosition
        | DrivenTransformProperties.SizeDelta
        | DrivenTransformProperties.AnchorMin
        | DrivenTransformProperties.AnchorMax
    );
#endif

        var normalizedMin = new Vector2(safeArea.xMin / resolution.x, safeArea.yMin / resolution.y);
        var normalizedMax = new Vector2(safeArea.xMax / resolution.x, safeArea.yMax / resolution.y);
        if ((controlEdges & Edge.Left) == 0)
        {
            normalizedMin.x = 0;
        }
        if ((controlEdges & Edge.Right) == 0)
        {
            normalizedMax.x = 1;
        }
        if ((controlEdges & Edge.Top) == 0)
        {
            normalizedMax.y = 1;
        }
        if ((controlEdges & Edge.Bottom) == 0)
        {
            normalizedMin.y = 0;
        }

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchorMin = normalizedMin;
        rectTransform.anchorMax = normalizedMax;
    }
}