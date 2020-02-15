using UnityEngine;
using MLAgents;

public class BouncerAgent : Agent
{
    [Header("Bouncer Specific")]
    public GameObject target;
    public GameObject bodyObject;
    Rigidbody m_Rb;
    Vector3 m_LookDir;
    public float strength = 10f;
    float m_JumpCooldown;
    int m_NumberJumps = 20;
    int m_JumpLeft = 20;

    IFloatProperties m_ResetParams;

    public override void InitializeAgent()
    {
        this.m_Rb = this.gameObject.GetComponent<Rigidbody>();
        this.m_LookDir = Vector3.zero;

        var academy = FindObjectOfType<Academy>();
        this.m_ResetParams = academy.FloatProperties;

        this.SetResetParameters();
    }

    public override void CollectObservations()
    {
        this.AddVectorObs(this.gameObject.transform.localPosition);
        this.AddVectorObs(this.target.transform.localPosition);
    }

    public override void AgentAction(float[] vectorAction)
    {
        for (var i = 0; i < vectorAction.Length; i++)
        {
            vectorAction[i] = Mathf.Clamp(vectorAction[i], -1f, 1f);
        }
        var x = vectorAction[0];
        var y = this.ScaleAction(vectorAction[1], 0, 1);
        var z = vectorAction[2];
        this.m_Rb.AddForce(new Vector3(x, y + 1, z) * this.strength);

        this.AddReward(-0.05f * (
            vectorAction[0] * vectorAction[0] +
            vectorAction[1] * vectorAction[1] +
            vectorAction[2] * vectorAction[2]) / 3f);

        this.m_LookDir = new Vector3(x, y, z);
    }

    public override void AgentReset()
    {
        this.gameObject.transform.localPosition = new Vector3(
            (1 - 2 * Random.value) * 5, 2, (1 - 2 * Random.value) * 5);
        this.m_Rb.velocity = default(Vector3);
        var environment = this.gameObject.transform.parent.gameObject;
        var targets =
            environment.GetComponentsInChildren<BouncerTarget>();
        foreach (var t in targets)
        {
            t.Respawn();
        }
        this.m_JumpLeft = this.m_NumberJumps;

        this.SetResetParameters();
    }

    public override void AgentOnDone()
    {
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(this.transform.position, new Vector3(0f, -1f, 0f), 0.51f) && this.m_JumpCooldown <= 0f)
        {
            this.RequestDecision();
            this.m_JumpLeft -= 1;
            this.m_JumpCooldown = 0.1f;
            this.m_Rb.velocity = default(Vector3);
        }

        this.m_JumpCooldown -= Time.fixedDeltaTime;

        if (this.gameObject.transform.position.y < -1)
        {
            this.AddReward(-1);
            this.Done();
            return;
        }

        if (this.gameObject.transform.localPosition.x < -19 || this.gameObject.transform.localPosition.x > 19
            || this.gameObject.transform.localPosition.z < -19 || this.gameObject.transform.localPosition.z > 19)
        {
            this.AddReward(-1);
            this.Done();
            return;
        }
        if (this.m_JumpLeft == 0)
        {
            this.Done();
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[3];

        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
        action[2] = Input.GetAxis("Vertical");
        return action;
    }

    void Update()
    {
        if (this.m_LookDir.magnitude > float.Epsilon)
        {
            this.bodyObject.transform.rotation = Quaternion.Lerp(this.bodyObject.transform.rotation,
                Quaternion.LookRotation(this.m_LookDir),
                Time.deltaTime * 10f);
        }
    }

    public void SetTargetScale()
    {
        var targetScale = this.m_ResetParams.GetPropertyWithDefault("target_scale", 1.0f);
        this.target.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
    }

    public void SetResetParameters()
    {
        this.SetTargetScale();
    }
}
