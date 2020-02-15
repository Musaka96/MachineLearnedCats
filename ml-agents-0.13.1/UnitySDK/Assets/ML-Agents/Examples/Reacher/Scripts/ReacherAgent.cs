using UnityEngine;
using MLAgents;

public class ReacherAgent : Agent
{
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject hand;
    public GameObject goal;
    ReacherAcademy m_MyAcademy;
    float m_GoalDegree;
    Rigidbody m_RbA;
    Rigidbody m_RbB;
    // speed of the goal zone around the arm (in radians)
    float m_GoalSpeed;
    // radius of the goal zone
    float m_GoalSize;
    // Magnitude of sinusoidal (cosine) deviation of the goal along the vertical dimension
    float m_Deviation;
    // Frequency of the cosine deviation of the goal along the vertical dimension
    float m_DeviationFreq;

    /// <summary>
    /// Collect the rigidbodies of the reacher in order to resue them for
    /// observations and actions.
    /// </summary>
    public override void InitializeAgent()
    {
        this.m_RbA = this.pendulumA.GetComponent<Rigidbody>();
        this.m_RbB = this.pendulumB.GetComponent<Rigidbody>();
        this.m_MyAcademy = GameObject.Find("Academy").GetComponent<ReacherAcademy>();

        this.SetResetParameters();
    }

    /// <summary>
    /// We collect the normalized rotations, angularal velocities, and velocities of both
    /// limbs of the reacher as well as the relative position of the target and hand.
    /// </summary>
    public override void CollectObservations()
    {
        this.AddVectorObs(this.pendulumA.transform.localPosition);
        this.AddVectorObs(this.pendulumA.transform.rotation);
        this.AddVectorObs(this.m_RbA.angularVelocity);
        this.AddVectorObs(this.m_RbA.velocity);

        this.AddVectorObs(this.pendulumB.transform.localPosition);
        this.AddVectorObs(this.pendulumB.transform.rotation);
        this.AddVectorObs(this.m_RbB.angularVelocity);
        this.AddVectorObs(this.m_RbB.velocity);

        this.AddVectorObs(this.goal.transform.localPosition);
        this.AddVectorObs(this.hand.transform.localPosition);

        this.AddVectorObs(this.m_GoalSpeed);
    }

    /// <summary>
    /// The agent's four actions correspond to torques on each of the two joints.
    /// </summary>
    public override void AgentAction(float[] vectorAction)
    {
        this.m_GoalDegree += this.m_GoalSpeed;
        this.UpdateGoalPosition();

        var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * 150f;
        var torqueZ = Mathf.Clamp(vectorAction[1], -1f, 1f) * 150f;
        this.m_RbA.AddTorque(new Vector3(torqueX, 0f, torqueZ));

        torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * 150f;
        torqueZ = Mathf.Clamp(vectorAction[3], -1f, 1f) * 150f;
        this.m_RbB.AddTorque(new Vector3(torqueX, 0f, torqueZ));
    }

    /// <summary>
    /// Used to move the position of the target goal around the agent.
    /// </summary>
    void UpdateGoalPosition()
    {
        var radians = this.m_GoalDegree * Mathf.PI / 180f;
        var goalX = 8f * Mathf.Cos(radians);
        var goalY = 8f * Mathf.Sin(radians);
        var goalZ = this.m_Deviation * Mathf.Cos(this.m_DeviationFreq * radians);
        this.goal.transform.position = new Vector3(goalY, goalZ, goalX) + this.transform.position;
    }

    /// <summary>
    /// Resets the position and velocity of the agent and the goal.
    /// </summary>
    public override void AgentReset()
    {
        this.pendulumA.transform.position = new Vector3(0f, -4f, 0f) + this.transform.position;
        this.pendulumA.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        this.m_RbA.velocity = Vector3.zero;
        this.m_RbA.angularVelocity = Vector3.zero;

        this.pendulumB.transform.position = new Vector3(0f, -10f, 0f) + this.transform.position;
        this.pendulumB.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        this.m_RbB.velocity = Vector3.zero;
        this.m_RbB.angularVelocity = Vector3.zero;

        this.m_GoalDegree = Random.Range(0, 360);
        this.UpdateGoalPosition();

        this.SetResetParameters();


        this.goal.transform.localScale = new Vector3(this.m_GoalSize, this.m_GoalSize, this.m_GoalSize);
    }

    public void SetResetParameters()
    {
        var fp = this.m_MyAcademy.FloatProperties;
        this.m_GoalSize = fp.GetPropertyWithDefault("goal_size", 5);
        this.m_GoalSpeed = Random.Range(-1f, 1f) * fp.GetPropertyWithDefault("goal_speed", 1);
        this.m_Deviation = fp.GetPropertyWithDefault("deviation", 0);
        this.m_DeviationFreq = fp.GetPropertyWithDefault("deviation_freq", 0);
    }
}
