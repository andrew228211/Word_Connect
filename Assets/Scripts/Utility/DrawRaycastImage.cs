//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UI;

//[RequireComponent(typeof(RectTransform))]
//public class DrawRaycastImage : MonoBehaviour
//{
//    private static Vector2 kShadowOffset = new Vector2(1, -1);
//    private static Color kShadowColor = new Color(0, 0, 0, 0.5f);
//    private const float kDottedLineSize = 5f;
//    void OnDrawGizmos()
//    {
//        Image image = GetComponent<Image>();
//        Vector4 raycastPadding = new Vector4();
//        raycastPadding = image.raycastPadding;
//        RectTransform gui = GetComponent<RectTransform>();
//        Rect rectInOwnSpace = gui.rect;
//        // Rect rectInUserSpace = rectInOwnSpace;
//        Rect rectInParentSpace = rectInOwnSpace;
//        Transform ownSpace = gui.transform;
//        // Transform userSpace = ownSpace;
//        Transform parentSpace = ownSpace;
//        RectTransform guiParent = null;
//        if (ownSpace.parent != null)
//        {
//            parentSpace = ownSpace.parent;
//            rectInParentSpace.x += ownSpace.localPosition.x;
//            rectInParentSpace.y += ownSpace.localPosition.y;

//            guiParent = parentSpace.GetComponent<RectTransform>();
//        }
//        Rect paddingRect = new Rect(rectInParentSpace);
//        paddingRect.xMin += raycastPadding.x;
//        paddingRect.xMax -= raycastPadding.z;
//        paddingRect.yMin += raycastPadding.y;
//        paddingRect.yMax -= raycastPadding.w;
//        Handles.color = Color.green;
//        DrawRect(paddingRect, parentSpace, true);
//    }
//    void DrawRect(Rect rect, Transform space, bool dotted)
//    {
//        Vector3 p0 = space.TransformPoint(new Vector2(rect.x, rect.y));
//        Vector3 p1 = space.TransformPoint(new Vector2(rect.x, rect.yMax));
//        Vector3 p2 = space.TransformPoint(new Vector2(rect.xMax, rect.yMax));
//        Vector3 p3 = space.TransformPoint(new Vector2(rect.xMax, rect.y));
//        if (!dotted)
//        {
//            Handles.DrawLine(p0, p1);
//            Handles.DrawLine(p1, p2);
//            Handles.DrawLine(p2, p3);
//            Handles.DrawLine(p3, p0);
//        }
//        else
//        {
//            DrawDottedLineWithShadow(kShadowColor, kShadowOffset, p0, p1, kDottedLineSize);
//            DrawDottedLineWithShadow(kShadowColor, kShadowOffset, p1, p2, kDottedLineSize);
//            DrawDottedLineWithShadow(kShadowColor, kShadowOffset, p2, p3, kDottedLineSize);
//            DrawDottedLineWithShadow(kShadowColor, kShadowOffset, p3, p0, kDottedLineSize);
//        }
//    }
//    public static void DrawDottedLineWithShadow(Color shadowColor, Vector2 screenOffset, Vector3 p1, Vector3 p2, float screenSpaceSize)
//    {
//        Camera cam = Camera.current;
//        if (!cam || Event.current.type != EventType.Repaint)
//            return;

//        Color oldColor = Handles.color;

//        // shadow
//        shadowColor.a = shadowColor.a * oldColor.a;
//        Handles.color = shadowColor;
//        Handles.DrawDottedLine(
//            cam.ScreenToWorldPoint(cam.WorldToScreenPoint(p1) + (Vector3)screenOffset),
//            cam.ScreenToWorldPoint(cam.WorldToScreenPoint(p2) + (Vector3)screenOffset), screenSpaceSize);

//        // line itself
//        Handles.color = oldColor;
//        Handles.DrawDottedLine(p1, p2, screenSpaceSize);
//    }
//}
