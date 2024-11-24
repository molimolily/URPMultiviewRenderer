using UnityEngine;
using Unity.Mathematics;
using System.Runtime.InteropServices;

namespace MVR
{
    /// <summary>
    /// ���_���Ƃ̃f�[�^
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PerViewData
    {
        public Matrix4x4 viewMatrix;
        public Matrix4x4 projectionMatrix;

        public static int Size => Marshal.SizeOf(typeof(PerViewData));
    }
}