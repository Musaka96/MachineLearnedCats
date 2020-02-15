using System.Collections;
using UnityEngine;
using MLAgents;

public class HallwayAgent : Agent
{
    public GameObject ground;
    public GameObject area;
    public GameObject symbolOGoal;
    public GameObject symbolXGoal;
    public GameObject symbolO;
    public GameObject symbolX;
    public bool useVectorObs;
    Rigidbody m_AgentRb;
    Material m_GroundMaterial;
    Renderer m_GroundRenderer;
    HallwayAcademy m_Academy;
    int m_Selection;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.m_Academy = FindObjectOfType<HallwayAcademy>();
        this.m_AgentRb = this.GetComponent<Rigidbody>();
        this.m_GroundRenderer = this.ground.GetComponent<Renderer>();
        this.m_GroundMaterial = this.m_GroundRenderer.material;
    }

    public override void CollectObservations()
    {
        if (this.useVectorObs)
        {
            this.AddVectorObs(this.GetStepCount() / (float)this.agentParameters.maxStep);
        }
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        this.m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        this.m_GroundRenderer.material = this.m_GroundMaterial;
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);
        switch (action)
        {
            case 1:
                dirToGo = this.transform.forward * 1f;
                break;
            case 2:
                dirToGo = this.transform.forward * -1f;
                break;
            case 3:
                rotateDir = this.transform.up * 1f;
                break;
            case 4:
                rotateDir = this.transform.up * -1f;
                break;
        }
        this.transform.Rotate(rotateDir, Time.deltaTime * 150f);
        this.m_AgentRb.AddForce(dirToGo * this.m_Academy.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void AgentAction(float[] vectorAction)
    {
        this.AddReward(-1f / this.agentParameters.maxStep);
        this.MoveAgent(vectorAction);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("symbol_O_Goal") || col.gameObject.CompareTag("symbol_X_Goal"))
        {
            if ((this.m_Selection == 0 && col.gameObject.CompareTag("symbol_O_Goal")) ||
                (this.m_Selection == 1 && col.gameObject.CompareTag("symbol_X_Goal")))
            {
                this.SetReward(1f);
                this.StartCoroutine(this.GoalScoredSwapGroundMaterial(this.m_Academy.goalScoredMaterial, 0.5f));
            }
            else
            {
                this.SetReward(-0.1f);
                this.StartCoroutine(this.GoalScoredSwapGroundMaterial(this.m_Academy.failMaterial, 0.5f));
            }
            this.Done();
        }
    }

    public override float[] Heuristic()
    {
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { 3 };
        }
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { 1 };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { 4 };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new float[] { 2 };
        }
        return new float[] { 0 };
    }

    public override void AgentReset()
    {
        var agentOffset = -15f;
        var blockOffset = 0f;
        this.m_Selection = Random.Range(0, 2);
        if (this.m_Selection == 0)
        {
            this.symbolO.transform.position =
                new Vector3(0f + Random.Range(-3f, 3f), 2f, blockOffset + Random.Range(-5f, 5f))
                + this.ground.transform.position;
            this.symbolX.transform.position =
                new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                + this.ground.transform.position;
        }
        else
        {
            this.symbolO.transform.position =
                new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                + this.ground.transform.position;
            this.symbolX.transform.position =
                new Vector3(0f, 2f, blockOffset + Random.Range(-5f, 5f))
                + this.ground.transform.position;
        }

        this.transform.position = new Vector3(0f + Random.Range(-3f, 3f),
            1f, agentOffset + Random.Range(-5f, 5f))
            + this.ground.transform.position;
        this.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        this.m_AgentRb.velocity *= 0f;

        var goalPos = Random.Range(0, 2);
        if (goalPos == 0)
        {
            this.symbolOGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + this.area.transform.position;
            this.symbolXGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + this.area.transform.position;
        }
        else
        {
            this.symbolXGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + this.area.transform.position;
            this.symbolOGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + this.area.transform.position;
        }
    }
}
