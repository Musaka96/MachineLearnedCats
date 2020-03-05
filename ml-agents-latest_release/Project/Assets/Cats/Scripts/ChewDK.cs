namespace Chew
{
    using UnityEngine;

    public static class ChewDK
    {
        public static void MoveTowards(
            Vector3 targetPos, Rigidbody rb, float targetVel, float maxVel) {
            var moveToPos = targetPos - rb.worldCenterOfMass;
            var velocityTarget = Time.fixedDeltaTime * targetVel * moveToPos;
            if (float.IsNaN(velocityTarget.x) == false) {
                rb.velocity = Vector3.MoveTowards(
                    rb.velocity, velocityTarget, maxVel);
            }
        }
    }
}