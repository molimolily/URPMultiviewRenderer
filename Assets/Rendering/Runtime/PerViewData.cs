using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace MVR
{
    /// <summary>
    /// 視点ごとのデータ
    /// </summary>
    public struct PerViewData
    {
        public float4x4 viewMatrix;
        public float4x4 projectionMatrix;

        public static int Size => Marshal.SizeOf(typeof(PerViewData));
    }
}