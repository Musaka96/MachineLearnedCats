using UnityEngine;
using MLAgents;

public class FoodCollectorAgent : Agent
{
    FoodCollectorAcademy m_MyAcademy;
    public GameObject area;
    FoodCollectorArea m_MyArea;
    bool m_Frozen;
    bool m_Poisoned;
    bool m_Satiated;
    bool m_Shoot;
    float m_FrozenTime;
    float m_EffectTime;
    Rigidbody m_AgentRb;
    float m_LaserLength;
    // Speed of agent rotation.
    public float turnSpeed = 300;

    // Speed of agent movement.
    public float moveSpeed = 2;
    public Material normalMaterial;
    public Material badMaterial;
    public Material goodMaterial;
    public Material frozenMaterial;
    public GameObject myLaser;
    public bool contribute;
    public bool useVectorObs;


    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.m_AgentRb = this.GetComponent<Rigidbody>();
        Monitor.verticalOffset = 1f;
        this.m_MyArea = this.area.GetComponent<FoodCollectorArea>();
        this.m_MyAcademy = FindObjectOfType<FoodCollectorAcademy>();

        this.SetResetParameters();
    }

    public override void CollectObservations()
    {
        if (this.useVectorObs)
        {
            var localVelocity = this.transform.InverseTransformDirection(this.m_AgentRb.velocity);
            this.AddVectorObs(localVelocity.x);
            this.AddVectorObs(localVelocity.z);
            this.AddVectorObs(System.Convert.ToInt32(this.m_Frozen));
            this.AddVectorObs(System.Convert.ToInt32(this.m_Shoot));
        }
    }

    public Color32 ToColor(int hexVal)
    {
        var r = (byte)((hexVal >> 16) & 0xFF);
        var g = (byte)((hexVal >> 8) & 0xFF);
        var b = (byte)(hexVal & 0xFF);
        return new Color32(r, g, b, 255);
    }

    public void MoveAgent(float[] act)
    {
        this.m_Shoot = false;

        if (Time.time > this.m_FrozenTime + 4f && this.m_Frozen)
        {
            this.Unfreeze();
        }
        if (Time.time > this.m_EffectTime + 0.5f)
        {
            if (this.m_Poisoned)
            {
                this.Unpoison();
            }
            if (this.m_Satiated)
            {
                this.Unsatiate();
            }
        }

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        if (!this.m_Frozen)
        {
            var shootCommand = false;
            var forwardAxis = (int)act[0];
            var rightAxis = (int)act[1];
            var rotateAxis = (int)act[2];
            var shootAxis = (int)act[3];

            switch (forwardAxis)
            {
                case 1:
                    dirToGo = this.transform.forward;
                    break;
                case 2:
                    dirToGo = -this.transform.forward;
                    break;
            }

            switch (rightAxis)
            {
                case 1:
                    dirToGo = this.transform.right;
                    break;
                case 2:
                    dirToGo = -this.transform.right;
                    break;
            }

            switch (rotateAxis)
            {
                case 1:
                    rotateDir = -this.transform.up;
                    break;
                case 2:
                    rotateDir = this.transform.up;
                    break;
            }
            switch (shootAxis)
            {
                case 1:
                    shootCommand = true;
                    break;
            }
            if (shootCommand)
            {
                this.m_Shoot = true;
                dirToGo *= 0.5f;
                this.m_AgentRb.velocity *= 0.75f;
            }
            this.m_AgentRb.AddForce(dirToGo * this.moveSpeed, ForceMode.VelocityChange);
            this.transform.Rotate(rotateDir, Time.fixedDeltaTime * this.turnSpeed);
        }

        if (this.m_AgentRb.velocity.sqrMagnitude > 25f) // slow it down
        {
            this.m_AgentRb.velocity *= 0.95f;
        }

        if (this.m_Shoot)
        {
            var myTransform = this.transform;
            this.myLaser.transform.localScale = new Vector3(1f, 1f, this.m_LaserLength);
            var rayDir = 25.0f * myTransform.forward;
            Debug.DrawRay(myTransform.position, rayDir, Color.red, 0f, true);
            RaycastHit hit;
            if (Physics.SphereCast(this.transform.position, 2f, rayDir, out hit, 25f))
            {
                if (hit.collider.gameObject.CompareTag("agent"))
                {
                    hit.collider.gameObject.GetComponent<FoodCollectorAgent>().Freeze();
                }
            }
        }
        else
        {
            this.myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    void Freeze()
    {
        this.gameObject.tag = "frozenAgent";
        this.m_Frozen = true;
        this.m_FrozenTime = Time.time;
        this.gameObject.GetComponentInChildren<Renderer>().material = this.frozenMaterial;
    }

    void Unfreeze()
    {
        this.m_Frozen = false;
        this.gameObject.tag = "agent";
        this.gameObject.GetComponentInChildren<Renderer>().material = this.normalMaterial;
    }

    void Poison()
    {
        this.m_Poisoned = true;
        this.m_EffectTime = Time.time;
        this.gameObject.GetComponentInChildren<Renderer>().material = this.badMaterial;
    }

    void Unpoison()
    {
        this.m_Poisoned = false;
        this.gameObject.GetComponentInChildren<Renderer>().material = this.normalMaterial;
    }

    void Satiate()
    {
        this.m_Satiated = true;
        this.m_EffectTime = Time.time;
        this.gameObject.GetComponentInChildren<Renderer>().material = this.goodMaterial;
    }

    void Unsatiate()
    {
        this.m_Satiated = false;
        this.gameObject.GetComponentInChildren<Renderer>().material = this.normalMaterial;
    }

    public override void AgentAction(float[] vectorAction)
    {
        this.MoveAgent(vectorAction);
    }

    public override float[] Heuristic()
    {
        var action = new float[4];
        if (Input.GetKey(KeyCode.D))
        {
            action[2] = 2f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            action[2] = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            action[0] = 2f;
        }
        action[3] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
        return action;
    }

    public override void AgentReset()
    {
        this.Unfreeze();
        this.Unpoison();
        this.Unsatiate();
        this.m_Shoot = false;
        this.m_AgentRb.velocity = Vector3.zero;
        this.myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
        this.transform.position = new Vector3(Random.Range(-this.m_MyArea.range, this.m_MyArea.range),
            2f, Random.Range(-this.m_MyArea.range, this.m_MyArea.range))
            + this.area.transform.position;
        this.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

        this.SetResetParameters();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("food"))
        {
            this.Satiate();
            collision.gameObject.GetComponent<FoodLogic>().OnEaten();
            this.AddReward(1f);
            if (this.contribute)
            {
                this.m_MyAcademy.totalScore += 1;
            }
        }
        if (collision.gameObject.CompareTag("badFood"))
        {
            this.Poison();
            collision.gameObject.GetComponent<FoodLogic>().OnEaten();

            this.AddReward(-1f);
            if (this.contribute)
            {
                this.m_MyAcademy.totalScore -= 1;
            }
        }
    }

    public override void AgentOnDone()
    {
    }

    public void SetLaserLengths()
    {
        this.m_LaserLength = this.m_MyAcademy.FloatProperties.GetPropertyWithDefault("laser_length", 1.0f);
    }

    public void SetAgentScale()
    {
        float agentScale = this.m_MyAcademy.FloatProperties.GetPropertyWithDefault("agent_scale", 1.0f);
        this.gameObject.transform.localScale = new Vector3(agentScale, agentScale, agentScale);
    }

    public void SetResetParameters()
    {
        this.SetLaserLengths();
        this.SetAgentScale();
    }
}
