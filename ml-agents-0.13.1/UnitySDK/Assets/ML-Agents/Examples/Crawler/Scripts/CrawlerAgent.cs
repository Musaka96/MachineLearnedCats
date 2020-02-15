using UnityEngine;
using MLAgents;

[RequireComponent(typeof(JointDriveController))] // Required to set joint forces
public class CrawlerAgent : Agent
{
    [Header("Target To Walk Towards")][Space(10)]
    public Transform target;

    public Transform ground;
    public bool detectTargets;
    public bool targetIsStatic;
    public bool respawnTargetWhenTouched;
    public float targetSpawnRadius;

    [Header("Body Parts")][Space(10)] public Transform body;
    public Transform leg0Upper;
    public Transform leg0Lower;
    public Transform leg1Upper;
    public Transform leg1Lower;
    public Transform leg2Upper;
    public Transform leg2Lower;
    public Transform leg3Upper;
    public Transform leg3Lower;

    [Header("Joint Settings")][Space(10)] JointDriveController m_JdController;
    Vector3 m_DirToTarget;
    float m_MovingTowardsDot;
    float m_FacingDot;

    [Header("Reward Functions To Use")][Space(10)]
    public bool rewardMovingTowardsTarget; // Agent should move towards target

    public bool rewardFacingTarget; // Agent should face the target
    public bool rewardUseTimePenalty; // Hurry up

    [Header("Foot Grounded Visualization")][Space(10)]
    public bool useFootGroundedVisualization;

    public MeshRenderer foot0;
    public MeshRenderer foot1;
    public MeshRenderer foot2;
    public MeshRenderer foot3;
    public Material groundedMaterial;
    public Material unGroundedMaterial;
    bool m_IsNewDecisionStep;
    int m_CurrentDecisionStep;

    Quaternion m_LookRotation;
    Matrix4x4 m_TargetDirMatrix;

    public override void InitializeAgent()
    {
        this.m_JdController = this.GetComponent<JointDriveController>();
        this.m_CurrentDecisionStep = 1;
        this.m_DirToTarget = this.target.position - this.body.position;


        //Setup each body part
        this.m_JdController.SetupBodyPart(this.body);
        this.m_JdController.SetupBodyPart(this.leg0Upper);
        this.m_JdController.SetupBodyPart(this.leg0Lower);
        this.m_JdController.SetupBodyPart(this.leg1Upper);
        this.m_JdController.SetupBodyPart(this.leg1Lower);
        this.m_JdController.SetupBodyPart(this.leg2Upper);
        this.m_JdController.SetupBodyPart(this.leg2Lower);
        this.m_JdController.SetupBodyPart(this.leg3Upper);
        this.m_JdController.SetupBodyPart(this.leg3Lower);
    }

    /// <summary>
    /// We only need to change the joint settings based on decision freq.
    /// </summary>
    public void IncrementDecisionTimer()
    {
        if (this.m_CurrentDecisionStep == this.agentParameters.numberOfActionsBetweenDecisions
            || this.agentParameters.numberOfActionsBetweenDecisions == 1)
        {
            this.m_CurrentDecisionStep = 1;
            this.m_IsNewDecisionStep = true;
        }
        else
        {
            this.m_CurrentDecisionStep++;
            this.m_IsNewDecisionStep = false;
        }
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp)
    {
        var rb = bp.rb;
        this.AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Whether the bp touching the ground

        var velocityRelativeToLookRotationToTarget = this.m_TargetDirMatrix.inverse.MultiplyVector(rb.velocity);
        this.AddVectorObs(velocityRelativeToLookRotationToTarget);

        var angularVelocityRelativeToLookRotationToTarget = this.m_TargetDirMatrix.inverse.MultiplyVector(rb.angularVelocity);
        this.AddVectorObs(angularVelocityRelativeToLookRotationToTarget);

        if (bp.rb.transform != this.body)
        {
            var localPosRelToBody = this.body.InverseTransformPoint(rb.position);
            this.AddVectorObs(localPosRelToBody);
            this.AddVectorObs(bp.currentXNormalizedRot); // Current x rot
            this.AddVectorObs(bp.currentYNormalizedRot); // Current y rot
            this.AddVectorObs(bp.currentZNormalizedRot); // Current z rot
            this.AddVectorObs(bp.currentStrength / this.m_JdController.maxJointForceLimit);
        }
    }

    public override void CollectObservations()
    {
        this.m_JdController.GetCurrentJointForces();

        // Update pos to target
        this.m_DirToTarget = this.target.position - this.body.position;
        this.m_LookRotation = Quaternion.LookRotation(this.m_DirToTarget);
        this.m_TargetDirMatrix = Matrix4x4.TRS(Vector3.zero, this.m_LookRotation, Vector3.one);

        RaycastHit hit;
        if (Physics.Raycast(this.body.position, Vector3.down, out hit, 10.0f))
        {
            this.AddVectorObs(hit.distance);
        }
        else
            this.AddVectorObs(10.0f);

        // Forward & up to help with orientation
        var bodyForwardRelativeToLookRotationToTarget = this.m_TargetDirMatrix.inverse.MultiplyVector(this.body.forward);
        this.AddVectorObs(bodyForwardRelativeToLookRotationToTarget);

        var bodyUpRelativeToLookRotationToTarget = this.m_TargetDirMatrix.inverse.MultiplyVector(this.body.up);
        this.AddVectorObs(bodyUpRelativeToLookRotationToTarget);

        foreach (var bodyPart in this.m_JdController.bodyPartsDict.Values)
        {
            this.CollectObservationBodyPart(bodyPart);
        }
    }

    /// <summary>
    /// Agent touched the target
    /// </summary>
    public void TouchedTarget()
    {
        this.AddReward(1f);
        if (this.respawnTargetWhenTouched)
        {
            this.GetRandomTargetPos();
        }
    }

    /// <summary>
    /// Moves target to a random position within specified radius.
    /// </summary>
    public void GetRandomTargetPos()
    {
        var newTargetPos = Random.insideUnitSphere * this.targetSpawnRadius;
        newTargetPos.y = 5;
        this.target.position = newTargetPos + this.ground.position;
    }

    public override void AgentAction(float[] vectorAction)
    {
        if (this.detectTargets)
        {
            foreach (var bodyPart in this.m_JdController.bodyPartsDict.Values)
            {
                if (bodyPart.targetContact && !this.IsDone() && bodyPart.targetContact.touchingTarget)
                {
                    this.TouchedTarget();
                }
            }
        }

        // If enabled the feet will light up green when the foot is grounded.
        // This is just a visualization and isn't necessary for function
        if (this.useFootGroundedVisualization)
        {
            this.foot0.material = this.m_JdController.bodyPartsDict[this.leg0Lower].groundContact.touchingGround
                ? this.groundedMaterial
                : this.unGroundedMaterial;
            this.foot1.material = this.m_JdController.bodyPartsDict[this.leg1Lower].groundContact.touchingGround
                ? this.groundedMaterial
                : this.unGroundedMaterial;
            this.foot2.material = this.m_JdController.bodyPartsDict[this.leg2Lower].groundContact.touchingGround
                ? this.groundedMaterial
                : this.unGroundedMaterial;
            this.foot3.material = this.m_JdController.bodyPartsDict[this.leg3Lower].groundContact.touchingGround
                ? this.groundedMaterial
                : this.unGroundedMaterial;
        }

        // Joint update logic only needs to happen when a new decision is made
        if (this.m_IsNewDecisionStep)
        {
            // The dictionary with all the body parts in it are in the jdController
            var bpDict = this.m_JdController.bodyPartsDict;

            var i = -1;
            // Pick a new target joint rotation
            bpDict[this.leg0Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.leg1Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.leg2Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.leg3Upper].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.leg0Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[this.leg1Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[this.leg2Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[this.leg3Lower].SetJointTargetRotation(vectorAction[++i], 0, 0);

            // Update joint strength
            bpDict[this.leg0Upper].SetJointStrength(vectorAction[++i]);
            bpDict[this.leg1Upper].SetJointStrength(vectorAction[++i]);
            bpDict[this.leg2Upper].SetJointStrength(vectorAction[++i]);
            bpDict[this.leg3Upper].SetJointStrength(vectorAction[++i]);
            bpDict[this.leg0Lower].SetJointStrength(vectorAction[++i]);
            bpDict[this.leg1Lower].SetJointStrength(vectorAction[++i]);
            bpDict[this.leg2Lower].SetJointStrength(vectorAction[++i]);
            bpDict[this.leg3Lower].SetJointStrength(vectorAction[++i]);
        }

        // Set reward for this step according to mixture of the following elements.
        if (this.rewardMovingTowardsTarget)
        {
            this.RewardFunctionMovingTowards();
        }

        if (this.rewardFacingTarget)
        {
            this.RewardFunctionFacingTarget();
        }

        if (this.rewardUseTimePenalty)
        {
            this.RewardFunctionTimePenalty();
        }

        this.IncrementDecisionTimer();
    }

    /// <summary>
    /// Reward moving towards target & Penalize moving away from target.
    /// </summary>
    void RewardFunctionMovingTowards()
    {
        this.m_MovingTowardsDot = Vector3.Dot(this.m_JdController.bodyPartsDict[this.body].rb.velocity, this.m_DirToTarget.normalized);
        this.AddReward(0.03f * this.m_MovingTowardsDot);
    }

    /// <summary>
    /// Reward facing target & Penalize facing away from target
    /// </summary>
    void RewardFunctionFacingTarget()
    {
        this.m_FacingDot = Vector3.Dot(this.m_DirToTarget.normalized, this.body.forward);
        this.AddReward(0.01f * this.m_FacingDot);
    }

    /// <summary>
    /// Existential penalty for time-contrained tasks.
    /// </summary>
    void RewardFunctionTimePenalty()
    {
        this.AddReward(-0.001f);
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void AgentReset()
    {
        if (this.m_DirToTarget != Vector3.zero)
        {
            this.transform.rotation = Quaternion.LookRotation(this.m_DirToTarget);
        }
        this.transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f));

        foreach (var bodyPart in this.m_JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }
        if (!this.targetIsStatic)
        {
            this.GetRandomTargetPos();
        }
        this.m_IsNewDecisionStep = true;
        this.m_CurrentDecisionStep = 1;
    }
}
