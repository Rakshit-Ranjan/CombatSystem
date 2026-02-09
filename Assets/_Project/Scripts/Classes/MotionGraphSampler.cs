using UnityEngine;
using System;

public class MotionGraphSampler {   

    public MotionGraph motionGraph;
    public float previousTime;

    public void Begin(MotionGraph motionGraph) {
        this.motionGraph = motionGraph;
        previousTime = 0f;
    }

    public (Vector3 localDelta, float deltaYaw) Sample(float normalizedTime) {
        if (motionGraph == null) {
            return (Vector3.zero, 0f);
        }

        normalizedTime = Mathf.Clamp01(normalizedTime);

        float forwardPrev = motionGraph.forward.Evaluate(previousTime);
        float rightPrev = motionGraph.right.Evaluate(previousTime);
        float upPrev = motionGraph.up.Evaluate(previousTime);
        float yawPrev = motionGraph.yaw.Evaluate(previousTime);

        float forwardCurr = motionGraph.forward.Evaluate(normalizedTime);
        float rightCurr = motionGraph.right.Evaluate(normalizedTime);
        float upCurr = motionGraph.up.Evaluate(normalizedTime);
        float yawCurr = motionGraph.yaw.Evaluate(normalizedTime);

        Vector3 localDelta = new Vector3(
            rightCurr - rightPrev,
            upCurr - upPrev,
            forwardCurr - forwardPrev
        ) * motionGraph.distanceMultiplier;
        float deltaYaw = yawCurr - yawPrev;
        previousTime = normalizedTime;
        return (localDelta, deltaYaw);
    }

    public void Reset() {
        motionGraph = null;
        previousTime = 0f;
    }


}