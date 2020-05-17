using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    public class VectorHelper
    {
        public static Vector3 ToVector(float[] floats)
        {
            if (floats.Length == 3)
            {
                return new Vector3(floats[0], floats[1], floats[2]);
            }
            else if (floats.Length == 2)
            {
                return new Vector3(floats[0], floats[1], 0.0f);
            }

            return Vector3.zero;
        }
    }
}