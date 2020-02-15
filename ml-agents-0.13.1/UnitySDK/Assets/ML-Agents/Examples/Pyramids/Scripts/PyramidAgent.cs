using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using MLAgents;

public class PyramidAgent : Agent
{
    public GameObject area;
    PyramidArea m_MyArea;
    Rigidbody m_AgentRb;
    RayPerception m_RayPer;
    PyramidSwitch m_SwitchLogic;
    public GameObject areaSwitch;
    public bool useVectorObs;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.m_AgentRb = this.GetComponent<Rigidbody>();
        this.m_MyArea = this.area.GetComponent<PyramidArea>();
        this.m_RayPer = this.GetComponent<RayPerception>();
        this.m_SwitchLogic = this.areaSwitch.GetComponent<PyramidSwitch>();
    }

    public override void CollectObservations()
    {
        if (this.useVectorObs)
        {
            const float rayDistance = 35f;
            float[] rayAngles = { 20f, 90f, 160f, 45f, 135f, 70f, 110f };
            float[] rayAngles1 = { 25f, 95f, 165f, 50f, 140f, 75f, 115f };
            float[] rayAngles2 = { 15f, 85f, 155f, 40f, 130f, 65f, 105f };

            string[] detectableObjects = { "block", "wall", "goal", "switchOff", "switchOn", "stone" };
            this.AddVectorObs(this.m_RayPer.Perceive(rayDistance, rayAngles, detectableObjects));
            this.AddVectorObs(this.m_RayPer.Perceive(rayDistance, rayAngles1, detectableObjects, 0f, 5f));
            this.AddVectorObs(this.m_RayPer.Perceive(rayDistance, rayAngles2, detectableObjects, 0f, 10f));
            this.AddVectorObs(this.m_SwitchLogic.GetState());
            this.AddVectorObs(this.transform.InverseTransformDirection(this.m_AgentRb.velocity));
        }
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
        this.transform.Rotate(rotateDir, Time.deltaTime * 200f);
        this.m_AgentRb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
    }

    public override void AgentAction(float[] vectorAction)
    {
        this.AddReward(-1f / this.agentParameters.maxStep);
        this.MoveAgent(vectorAction);
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
        var enumerable = Enumerable.Range(0, 9).OrderBy(x => Guid.NewGuid()).Take(9);
        var items = enumerable.ToArray();

        this.m_MyArea.CleanPyramidArea();

        this.m_AgentRb.velocity = Vector3.zero;
        this.m_MyArea.PlaceObject(this.gameObject, items[0]);
        this.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

        this.m_SwitchLogic.ResetSwitch(items[1], items[2]);
        this.m_MyArea.CreateStonePyramid(1, items[3]);
        this.m_MyArea.CreateStonePyramid(1, items[4]);
        this.m_MyArea.CreateStonePyramid(1, items[5]);
        this.m_MyArea.CreateStonePyramid(1, items[6]);
        this.m_MyArea.CreateStonePyramid(1, items[7]);
        this.m_MyArea.CreateStonePyramid(1, items[8]);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("goal"))
        {
            this.SetReward(2f);
            this.Done();
        }
    }

    public override void AgentOnDone()
    {
    }
}
