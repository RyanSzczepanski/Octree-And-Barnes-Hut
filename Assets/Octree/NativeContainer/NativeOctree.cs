using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

struct OctNode
{
	public SpacialOctreeData spacialOctreeData;
	public NodeChildren children;
	public bool isLeaf;
}

public unsafe partial struct NativeOctree<T> : IDisposable where T : unmanaged
{
	#if ENABLE_UNITY_COLLECTIONS_CHECKS
	AtomicSafetyHandle safetyHandle;
	[NativeSetClassTypeToNullOnSchedule]
	DisposeSentinel disposeSentinel;
	#endif

	[NativeDisableUnsafePtrRestriction]
	UnsafeList<T>* elements;

	[NativeDisableUnsafePtrRestriction]
	UnsafeList<OctNode>* nodes;

	int elementsCount;

	double3 bounds;

	public NativeOctree(double3 bounds, Allocator allocator = Allocator.Temp)
    : this()
		{
			this.bounds = bounds;
			elementsCount = 0;
		}


	public void Dispose()
    {
		UnsafeList<T>.Destroy(elements);
		elements = null;
		UnsafeList<OctNode>.Destroy(nodes);
		nodes = null;

		#if ENABLE_UNITY_COLLECTIONS_CHECKS
		DisposeSentinel.Dispose(ref safetyHandle, ref disposeSentinel);
		#endif
    }
}

