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
    float heightPenalty = 40f;

    [SerializeField]
    GameObject goal;

    bool grounded;

    [SerializeField]
    float totalPenalty = 0f;

    [SerializeField]
    float doneReward = 10f;

    [SerializeField]
    float fallDamage = -10f;

    private void Awake() {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
     Debug.logger.logEnabled = false;
#endif
        this.agentRigibody = this.GetComponent<Rigidbody>();
    }

    public override void AgentAction(float[] vectorAction) {
        if (this.grounded) {
            //Debug.Log("Jump: " + new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]));
            this.agentRigibody.AddForce(
                new Vector3(vectorAction[0], vectorAction[1], vectorAction[2])
                * this.jumpForce, ForceMode.Acceleration);
        }
    }

    public override float[] Heuristic() {
        return base.Heuristic();
    }

    public override void CollectObservations() {
        this.AddVectorObs(this.transform.localPosition);

        Bounds goalBounds = this.goal.GetComponent<MeshRenderer>().bounds;

        this.AddVectorObs(Vector3.Distance(goalBounds.center, this.transform.position));
        //this.AddVectorObs(goalBounds.size);

        //foreach (var obstacle in this.obstacles) {
        //    Bounds targetBounds = obstacle.GetComponent<MeshRenderer>().bounds;

        //    this.AddVectorObs(targetBounds.center - this.transform.position);
        //    this.AddVectorObs(targetBounds.size);
        //}

        this.AddVectorObs(this.agentRigibody.velocity);
        this.AddVectorObs(this.grounded);
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
    }

    public override void InitializeAgent() {
        base.InitializeAgent();
    }

    private void FixedUpdate() {
        //var distancePenalty = (Vector3.Distance(this.goal.transform.position, this.transform.position) + this.distancePenaltyOffset);
        var distancePenalty = (-Vector3.Distance(this.goal.transform.position, this.transform.position) + this.distancePenaltyOffset) / this.distancePenaltyDivider;
        this.SetReward(distancePenalty);
        Debug.Log("DistancePenalty: " + distancePenalty);

        var heightPenalty = this.transform.position.y / 30f;
        var test = Mathf.Abs(this.goal.transform.position.y - this.transform.position.y);
        //Debug.Log(test);
        var heightReward = Mathf.Clamp(1f / test, -1, 1) / 50f;
        //this.SetReward(heightPenalty);
        this.SetReward(heightReward);
        Debug.Log("height reward: " + heightReward);

        this.totalPenalty += this.GetReward();
        //Debug.Log(this.totalPenalty);

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
            Debug.Log("Grounded");
            this.grounded = true;
        }

        if (collider.gameObject == this.goal) {
            Debug.Log("Done");
            this.SetReward(this.doneReward);
            this.Done();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Ground")) {
            Debug.Log("Un-Grounded");
            this.grounded = false;
        }
    }
}