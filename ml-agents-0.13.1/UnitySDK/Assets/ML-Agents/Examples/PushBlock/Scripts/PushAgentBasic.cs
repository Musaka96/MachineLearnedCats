//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using MLAgents;

public class PushAgentBasic : Agent
{
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    public GameObject area;

    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;

    PushBlockAcademy m_Academy;

    /// <summary>
    /// The goal to push the block to.
    /// </summary>
    public GameObject goal;

    /// <summary>
    /// The block to be pushed to the goal.
    /// </summary>
    public GameObject block;

    /// <summary>
    /// Detects when the block touches the goal.
    /// </summary>
    [HideInInspector]
    public GoalDetect goalDetect;

    public bool useVectorObs;

    Rigidbody m_BlockRb;  //cached on initialization
    Rigidbody m_AgentRb;  //cached on initialization
    Material m_GroundMaterial; //cached on Awake()

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;

    void Awake()
    {
        this.m_Academy = FindObjectOfType<PushBlockAcademy>(); //cache the academy
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.goalDetect = this.block.GetComponent<GoalDetect>();
        this.goalDetect.agent = this;

        // Cache the agent rigidbody
        this.m_AgentRb = this.GetComponent<Rigidbody>();
        // Cache the block rigidbody
        this.m_BlockRb = this.block.GetComponent<Rigidbody>();
        // Get the ground's bounds
        this.areaBounds = this.ground.GetComponent<Collider>().bounds;
        // Get the ground renderer so we can change the material when a goal is scored
        this.m_GroundRenderer = this.ground.GetComponent<Renderer>();
        // Starting material
        this.m_GroundMaterial = this.m_GroundRenderer.material;

        this.SetResetParameters();
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-this.areaBounds.extents.x * this.m_Academy.spawnAreaMarginMultiplier,
                this.areaBounds.extents.x * this.m_Academy.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-this.areaBounds.extents.z * this.m_Academy.spawnAreaMarginMultiplier,
                this.areaBounds.extents.z * this.m_Academy.spawnAreaMarginMultiplier);
            randomSpawnPos = this.ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.01f, 2.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void ScoredAGoal()
    {
        // We use a reward of 5.
        this.AddReward(5f);

        // By marking an agent as done AgentReset() will be called automatically.
        this.Done();

        // Swap ground material for a bit to indicate we scored.
        this.StartCoroutine(this.GoalScoredSwapGroundMaterial(this.m_Academy.goalScoredMaterial, 0.5f));
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        this.m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        this.m_GroundRenderer.material = this.m_GroundMaterial;
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);

        // Goalies and Strikers have slightly different action spaces.
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
            case 5:
                dirToGo = this.transform.right * -0.75f;
                break;
            case 6:
                dirToGo = this.transform.right * 0.75f;
                break;
        }
        this.transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        this.m_AgentRb.AddForce(dirToGo * this.m_Academy.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void AgentAction(float[] vectorAction)
    {
        // Move the agent using the action.
        this.MoveAgent(vectorAction);

        // Penalty given each step to encourage agent to finish task quickly.
        this.AddReward(-1f / this.agentParameters.maxStep);
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

    /// <summary>
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBlock()
    {
        // Get a random position for the block.
        this.block.transform.position = this.GetRandomSpawnPos();

        // Reset block velocity back to zero.
        this.m_BlockRb.velocity = Vector3.zero;

        // Reset block angularVelocity back to zero.
        this.m_BlockRb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    public override void AgentReset()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        this.area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        this.ResetBlock();
        this.transform.position = this.GetRandomSpawnPos();
        this.m_AgentRb.velocity = Vector3.zero;
        this.m_AgentRb.angularVelocity = Vector3.zero;

        this.SetResetParameters();
    }

    public void SetGroundMaterialFriction()
    {
        var resetParams = this.m_Academy.FloatProperties;

        var groundCollider = this.ground.GetComponent<Collider>();

        groundCollider.material.dynamicFriction = resetParams.GetPropertyWithDefault("dynamic_friction", 0);
        groundCollider.material.staticFriction = resetParams.GetPropertyWithDefault("static_friction", 0);
    }

    public void SetBlockProperties()
    {
        var resetParams = this.m_Academy.FloatProperties;

        var scale = resetParams.GetPropertyWithDefault("block_scale", 2);
        //Set the scale of the block
        this.m_BlockRb.transform.localScale = new Vector3(scale, 0.75f, scale);

        // Set the drag of the block
        this.m_BlockRb.drag = resetParams.GetPropertyWithDefault("block_drag", 0.5f);
    }

    public void SetResetParameters()
    {
        this.SetGroundMaterialFriction();
        this.SetBlockProperties();
    }
}
