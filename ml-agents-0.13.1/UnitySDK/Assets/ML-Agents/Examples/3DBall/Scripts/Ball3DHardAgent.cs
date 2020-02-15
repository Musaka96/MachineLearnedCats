using UnityEngine;
using MLAgents;

public class Ball3DHardAgent : Agent
{
    [Header("Specific to Ball3DHard")]
    public GameObject ball;
    Rigidbody m_BallRb;
    IFloatProperties m_ResetParams;

    public override void InitializeAgent()
    {
        this.m_BallRb = this.ball.GetComponent<Rigidbody>();
        var academy = FindObjectOfType<Academy>();
        this.m_ResetParams = academy.FloatProperties;
        this.SetResetParameters();
    }

    public override void CollectObservations()
    {
        this.AddVectorObs(this.gameObject.transform.rotation.z);
        this.AddVectorObs(this.gameObject.transform.rotation.x);
        this.AddVectorObs((this.ball.transform.position - this.gameObject.transform.position));
    }

    public override void AgentAction(float[] vectorAction)
    {
        var actionZ = 2f * Mathf.Clamp(vectorAction[0], -1f, 1f);
        var actionX = 2f * Mathf.Clamp(vectorAction[1], -1f, 1f);

        if ((this.gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
            (this.gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
        {
            this.gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        }

        if ((this.gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
            (this.gameObject.transform.rotation.x > -0.25f && actionX < 0f))
        {
            this.gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
        }
        if ((this.ball.transform.position.y - this.gameObject.transform.position.y) < -2f ||
            Mathf.Abs(this.ball.transform.position.x - this.gameObject.transform.position.x) > 3f ||
            Mathf.Abs(this.ball.transform.position.z - this.gameObject.transform.position.z) > 3f)
        {
            this.Done();
            this.SetReward(-1f);
        }
        else
        {
            this.SetReward(0.1f);
        }
    }

    public override void AgentReset()
    {
        this.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        this.gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        this.gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        this.m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        this.ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
            + this.gameObject.transform.position;
    }

    public void SetBall()
    {
        //Set the attributes of the ball by fetching the information from the academy
        this.m_BallRb.mass = this.m_ResetParams.GetPropertyWithDefault("mass", 1.0f);
        var scale = this.m_ResetParams.GetPropertyWithDefault("scale", 1.0f);
        this.ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        this.SetBall();
    }
}
