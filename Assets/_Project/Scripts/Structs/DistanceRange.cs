using System;
using UnityEngine;

[Serializable]
public struct DistanceRange {
    
    public float min;
    public float max;
    
    public DistanceRange(float min, float max) {
        this.min = min;
        this.max = max;
    }

    public bool Contains(float val) => val >= min && val <= max; 

    public bool IsTooClose(float val) => val < min;
    public bool IsTooFar(float val) => val > max;
    public float Clamp(float val) => Mathf.Clamp(val, min, max);

    public float Midpoint => (min+max) * 0.5f;


}


