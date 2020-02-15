using UnityEngine;
using MLAgents;

public class BasicAgent : Agent
{
    [Header("Specific to Basic")]
    BasicAcademy m_Academy;
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    int m_Position;
    int m_SmallGoalPosition;
    int m_LargeGoalPosition;
    public GameObject largeGoal;
    public GameObject smallGoal;
    int m_MinPosition;
    int m_MaxPosition;

    public override void InitializeAgent()
    {
        this.m_Academy = FindObjectOfType(typeof(BasicAcademy)) as BasicAcademy;
    }

    public override void CollectObservations()
    {
        this.AddVectorObs(this.m_Position, 20);
    }

    public override void AgentAction(float[] vectorAction)
    {
        var movement = (int)vectorAction[0];

        var direction = 0;

        switch (movement)
        {
            case 1:
                direction = -1;
                break;
            case 2:
                direction = 1;
                break;
        }

        this.m_Position += direction;
        if (this.m_Position < this.m_MinPosition) { this.m_Position = this.m_MinPosition; }
        if (this.m_Position > this.m_MaxPosition) { this.m_Position = this.m_MaxPosition; }

        this.gameObject.transform.position = new Vector3(this.m_Position - 10f, 0f, 0f);

        this.AddReward(-0.01f);

        if (this.m_Position == this.m_SmallGoalPosition)
        {
            this.Done();
            this.AddReward(0.1f);
        }

        if (this.m_Position == this.m_LargeGoalPosition)
        {
            this.Done();
            this.AddReward(1f);
        }
    }

    public override void AgentReset()
    {
        this.m_Position = 10;
        this.m_MinPosition = 0;
        this.m_MaxPosition = 20;
        this.m_SmallGoalPosition = 7;
        this.m_LargeGoalPosition = 17;
        this.smallGoal.transform.position = new Vector3(this.m_SmallGoalPosition - 10f, 0f, 0f);
        this.largeGoal.transform.position = new Vector3(this.m_LargeGoalPosition - 10f, 0f, 0f);
    }

    public override float[] Heuristic()
    {
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { 2 };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { 1 };
        }
        return new float[] { 0 };
    }

    public override void AgentOnDone()
    {
    }

    public void FixedUpdate()
    {
        this.WaitTimeInference();
    }

    void WaitTimeInference()
    {
        if (!this.m_Academy.IsCommunicatorOn)
        {
            this.RequestDecision();
        }
        else
        {
            if (this.m_TimeSinceDecision >= this.timeBetweenDecisionsAtInference)
            {
                this.m_TimeSinceDecision = 0f;
                this.RequestDecision();
            }
            else
            {
                this.m_TimeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }
}
