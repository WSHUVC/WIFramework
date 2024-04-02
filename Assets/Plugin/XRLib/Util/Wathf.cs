
using UnityEditor;
using UnityEngine;

namespace WI
{
    public static class Wathf
    {
        /// <summary>
        /// 1.618 : 1 = x : y
        /// ex) standard = 3
        /// result[0] = f(x==standard)
        /// result[1] = f(y==standard)
        /// </summary>
        /// <param name="standard"></param>
        /// <param name="result"></param>
        public static void GoldenRatio(float standard, out float[] result)
        {
            result = new float[2];
            result[0] = standard * 1.618f;
            result[1] = standard * 0.618f;
        }
    }
}