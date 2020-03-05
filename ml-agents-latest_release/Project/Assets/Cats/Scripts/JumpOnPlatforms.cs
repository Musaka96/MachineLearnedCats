using MLAgents;
using System.Collections.Generic;
using UnityEngine;


// Notes : 
//    1. The higer he goes the better the reward
//    2. Make a maximun amount of blocks for tracking so it doesn't
//       matter


public class JumpOnPlatforms : Agent
{
    [SerializeField]
    List<GameObject> obstacles;
    Rigidbody agentRigibody;

    [SerializeField]
    float jumpAngle = -45f;
    [SerializeField]
    float jumpForce = 300f;

    [SerializeField]
    float distancePenaltyDivider = 10f;

    [SerializeField]
    float distancePenaltyOffset = 2f;

    [SerializeField]
    GameObject goal;

    [SerializeField]
    bool grounded;

    [SerializeField]
    float totalPenalty = 0f;

    [SerializeField]
    float doneReward = 10f;

    [SerializeField]
    float fallDamage = -10f;

    [SerializeField]
    int maxJumps = 10;
    int currentJumps;

    float previousDistance;


    private void Awake() {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#else
     Debug.logger.logEnabled = false;
#endif
        this.agentRigibody = this.GetComponent<Rigidbody>();
    }

    public override void AgentAction(float[] vectorAction) {
        if (this.grounded) {

            for (var i = 0; i < vectorAction.Length; i++) {
                vectorAction[i] = Mathf.Clamp(vectorAction[i], -1f, 1f);
            }

            var x = vectorAction[0];
            var y = this.ScaleAction(vectorAction[1], 0, 1);

            var z = vectorAction[2];
            this.agentRigibody.AddForce(new Vector3(x, y + 1, z) * this.jumpForce);
        }
    }

    public override void CollectObservations() {
        this.AddVectorObs(this.transform.localPosition);

        this.AddVectorObs(this.goal.transform.localPosition);
    }

    public override void AgentReset() {
        this.transform.localPosition = new Vector3(Random.value * 5,
                                      0f,
                                      Random.value * 10);

        this.goal.transform.localPosition = new Vector3(Random.value * 5,
                                      3.8f,
                                      Random.value * 10);

        this.agentRigibody.velocity = Vector3.zero;
        this.agentRigibody.rotation = Quaternion.identity;
        this.agentRigibody.angularVelocity = Vector3.zero;

        this.currentJumps = this.maxJumps;

        this.previousDistance = 0;
    }

    public override void InitializeAgent() {
        base.InitializeAgent();
        this.currentJumps = this.maxJumps;
        this.previousDistance = 0;
    }

    private void FixedUpdate() {
        if (this.transform.position.y < -1.5f) {
            this.SetReward(this.fallDamage);
            this.Done();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            this.AgentReset();
        }
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.CompareTag("Ground")) {

            this.currentJumps--;
            Debug.Log(this.currentJumps);
            if (this.currentJumps == 0) {
                this.SetReward(this.fallDamage);
                this.Done();
            }


            float distance = Vector3.Distance(this.goal.transform.localPosition, this.transform.localPosition);
            float distanceTraversed = Mathf.Abs(distance - this.previousDistance);
            //Debug.Log("TRAVERSED: " + distanceTraversed);
            if (this.previousDistance != 0) {
                float triesPenalty = 0.01f / this.currentJumps;
                this.AddReward(-triesPenalty);
                Debug.Log("triesPne: " + (-triesPenalty));
            }
            this.previousDistance = distanceTraversed;


            var distancePenalty = 
                (-Vector3.Distance(this.goal.transform.localPosition, this.transform.localPosition)
                + this.distancePenaltyOffset) / this.distancePenaltyDivider;
            this.AddReward(distancePenalty);
            //Debug.Log("DistancePenalty: " + distancePenalty);

            this.grounded = true;
            this.RequestDecision();
        }

        if (collider.gameObject == this.goal) {
            //Debug.Log("Done");
            this.SetReward(this.doneReward);
            this.Done();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Ground")) {
            this.grounded = false;
        }
    }
}