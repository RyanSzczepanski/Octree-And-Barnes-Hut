using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRenderer
{
    public static void DrawCube(Vector3 center, float radius, Color color, float time)
    {
        for (int i = 0; i < 8; i++)
        {
            //Face1
            Debug.DrawLine(center + OctreeData.GetOffsetVector(0) * radius, center + OctreeData.GetOffsetVector(1) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(0) * radius, center + OctreeData.GetOffsetVector(2) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(3) * radius, center + OctreeData.GetOffsetVector(1) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(3) * radius, center + OctreeData.GetOffsetVector(2) * radius, color, time);
            //Face2
            Debug.DrawLine(center + OctreeData.GetOffsetVector(4) * radius, center + OctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(4) * radius, center + OctreeData.GetOffsetVector(6) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(7) * radius, center + OctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(7) * radius, center + OctreeData.GetOffsetVector(6) * radius, color, time);
            //Connecting Arms
            Debug.DrawLine(center + OctreeData.GetOffsetVector(0) * radius, center + OctreeData.GetOffsetVector(4) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(1) * radius, center + OctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(2) * radius, center + OctreeData.GetOffsetVector(6) * radius, color, time);
            Debug.DrawLine(center + OctreeData.GetOffsetVector(3) * radius, center + OctreeData.GetOffsetVector(7) * radius, color, time);
        }
    }
}
