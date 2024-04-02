using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WI
{
    public class MaxRangeLimitter
    {
        public Collider maxRange;

        public void SetRange(Collider maxrRangeCollider)
        {
            maxRange = maxrRangeCollider;
        }

        public bool MoveRangeLimit(Vector3 pos)
        {
            if (maxRange == null)
                return false;

            if (maxRange.bounds.Contains(pos))
            {
                return true;
            }
            return false;
        }
    }
}
