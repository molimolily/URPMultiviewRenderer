using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace MVR
{
    /// <summary>
    /// ���_���Ƃ̃f�[�^
    /// </summary>
    public struct PerViewData
    {
        public float4x4 viewMatrix;
        public float4x4 projectionMatrix;

        public static int Size => Marshal.SizeOf(typeof(PerViewData));
    }
}