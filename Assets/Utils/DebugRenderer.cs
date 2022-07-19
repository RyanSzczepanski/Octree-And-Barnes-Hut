using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DebugRenderer
{
    public static void DrawCube(float3 center, float radius, Color color, float time)
    {
        for (int i = 0; i < 8; i++)
        {
            //Face1
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(0) * radius, center + SpacialOctreeData.GetOffsetVector(1) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(0) * radius, center + SpacialOctreeData.GetOffsetVector(2) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(3) * radius, center + SpacialOctreeData.GetOffsetVector(1) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(3) * radius, center + SpacialOctreeData.GetOffsetVector(2) * radius, color, time);
            //Face2
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(4) * radius, center + SpacialOctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(4) * radius, center + SpacialOctreeData.GetOffsetVector(6) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(7) * radius, center + SpacialOctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(7) * radius, center + SpacialOctreeData.GetOffsetVector(6) * radius, color, time);
            //Connecting Arms
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(0) * radius, center + SpacialOctreeData.GetOffsetVector(4) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(1) * radius, center + SpacialOctreeData.GetOffsetVector(5) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(2) * radius, center + SpacialOctreeData.GetOffsetVector(6) * radius, color, time);
            Debug.DrawLine(center + SpacialOctreeData.GetOffsetVector(3) * radius, center + SpacialOctreeData.GetOffsetVector(7) * radius, color, time);
        }
    }
}
