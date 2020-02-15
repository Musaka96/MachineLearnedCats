using System;
using UnityEngine;
using System.Linq;
using MLAgents;
using UnityEngine.Serialization;

public class GridAgent : Agent
{
    Academy m_Academy;
    [FormerlySerializedAs("m_Area")]
    [Header("Specific to GridWorld")]
    public GridArea area;
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;

    [Tooltip("Because we want an observation right before making a decision, we can force " +
        "a camera to render before making a decision. Place the agentCam here if using " +
        "RenderTexture as observations.")]
    public Camera renderCamera;

    [Tooltip("Selecting will turn on action masking. Note that a model trained with action " +
        "masking turned on may not behave optimally when action masking is turned off.")]
    public bool maskActions = true;

    const int k_NoAction = 0;  // do nothing!
    const int k_Up = 1;
    const int k_Down = 2;
    const int k_Left = 3;
    const int k_Right = 4;

    public override void InitializeAgent()
    {
        this.m_Academy = FindObjectOfType<Academy>();
    }

    public override void CollectObservations()
    {
        // There are no numeric observations to collect as this environment uses visual
        // observations.

        // Mask the necessary actions if selected by the user.
        if (this.maskActions)
        {
            this.SetMask();
        }
    }

    /// <summary>
    /// Applies the mask for the agents action to disallow unnecessary actions.
    /// </summary>
    void SetMask()
    {
        // Prevents the agent from picking an action that would make it collide with a wall
        var positionX = (int)this.transform.position.x;
        var positionZ = (int)this.transform.position.z;
        var maxPosition = (int)this.m_Academy.FloatProperties.GetPropertyWithDefault("gridSize", 5f) - 1;

        if (positionX == 0)
        {
            this.SetActionMask(k_Left);
        }

        if (positionX == maxPosition)
        {
            this.SetActionMask(k_Right);
        }

        if (positionZ == 0)
        {
            this.SetActionMask(k_Down);
        }

        if (positionZ == maxPosition)
        {
            this.SetActionMask(k_Up);
        }
    }

    // to be implemented by the developer
    public override void AgentAction(float[] vectorAction)
    {
        this.AddReward(-0.01f);
        var action = Mathf.FloorToInt(vectorAction[0]);

        var targetPos = this.transform.position;
        switch (action)
        {
            case k_NoAction:
                // do nothing
                break;
            case k_Right:
                targetPos = this.transform.position + new Vector3(1f, 0, 0f);
                break;
            case k_Left:
                targetPos = this.transform.position + new Vector3(-1f, 0, 0f);
                break;
            case k_Up:
                targetPos = this.transform.position + new Vector3(0f, 0, 1f);
                break;
            case k_Down:
                targetPos = this.transform.position + new Vector3(0f, 0, -1f);
                break;
            default:
                throw new ArgumentException("Invalid action value");
        }

        var hit = Physics.OverlapBox(
            targetPos, new Vector3(0.3f, 0.3f, 0.3f));
        if (hit.Where(col => col.gameObject.CompareTag("wall")).ToArray().Length == 0)
        {
            this.transform.position = targetPos;

            if (hit.Where(col => col.gameObject.CompareTag("goal")).ToArray().Length == 1)
            {
                this.Done();
                this.SetReward(1f);
            }
            if (hit.Where(col => col.gameObject.CompareTag("pit")).ToArray().Length == 1)
            {
                this.Done();
                this.SetReward(-1f);
            }
        }
    }

    public override float[] Heuristic()
    {
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { k_Right };
        }
        if (Input.GetKey(KeyCode.W))
        {
            return new float[] { k_Up };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { k_Left };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new float[] { k_Down };
        }
        return new float[] { k_NoAction };
    }

    // to be implemented by the developer
    public override void AgentReset()
    {
        this.area.AreaReset();
    }

    public void FixedUpdate()
    {
        this.WaitTimeInference();
    }

    void WaitTimeInference()
    {
        if (this.renderCamera != null)
        {
            this.renderCamera.Render();
        }

        if (this.m_Academy.IsCommunicatorOn)
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
