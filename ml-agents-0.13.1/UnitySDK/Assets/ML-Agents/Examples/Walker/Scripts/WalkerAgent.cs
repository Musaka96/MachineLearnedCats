using UnityEngine;
using MLAgents;

public class WalkerAgent : Agent
{
    [Header("Specific to Walker")]
    [Header("Target To Walk Towards")]
    [Space(10)]
    public Transform target;

    Vector3 m_DirToTarget;
    public Transform hips;
    public Transform chest;
    public Transform spine;
    public Transform head;
    public Transform thighL;
    public Transform shinL;
    public Transform footL;
    public Transform thighR;
    public Transform shinR;
    public Transform footR;
    public Transform armL;
    public Transform forearmL;
    public Transform handL;
    public Transform armR;
    public Transform forearmR;
    public Transform handR;
    JointDriveController m_JdController;
    bool m_IsNewDecisionStep;
    int m_CurrentDecisionStep;

    Rigidbody m_HipsRb;
    Rigidbody m_ChestRb;
    Rigidbody m_SpineRb;

    IFloatProperties m_ResetParams;

    public override void InitializeAgent()
    {
        this.m_JdController = this.GetComponent<JointDriveController>();
        this.m_JdController.SetupBodyPart(this.hips);
        this.m_JdController.SetupBodyPart(this.chest);
        this.m_JdController.SetupBodyPart(this.spine);
        this.m_JdController.SetupBodyPart(this.head);
        this.m_JdController.SetupBodyPart(this.thighL);
        this.m_JdController.SetupBodyPart(this.shinL);
        this.m_JdController.SetupBodyPart(this.footL);
        this.m_JdController.SetupBodyPart(this.thighR);
        this.m_JdController.SetupBodyPart(this.shinR);
        this.m_JdController.SetupBodyPart(this.footR);
        this.m_JdController.SetupBodyPart(this.armL);
        this.m_JdController.SetupBodyPart(this.forearmL);
        this.m_JdController.SetupBodyPart(this.handL);
        this.m_JdController.SetupBodyPart(this.armR);
        this.m_JdController.SetupBodyPart(this.forearmR);
        this.m_JdController.SetupBodyPart(this.handR);

        this.m_HipsRb = this.hips.GetComponent<Rigidbody>();
        this.m_ChestRb = this.chest.GetComponent<Rigidbody>();
        this.m_SpineRb = this.spine.GetComponent<Rigidbody>();

        var academy = FindObjectOfType<WalkerAcademy>();
        this.m_ResetParams = academy.FloatProperties;

        this.SetResetParameters();
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp)
    {
        var rb = bp.rb;
        this.AddVectorObs(bp.groundContact.touchingGround ? 1 : 0); // Is this bp touching the ground
        this.AddVectorObs(rb.velocity);
        this.AddVectorObs(rb.angularVelocity);
        var localPosRelToHips = this.hips.InverseTransformPoint(rb.position);
        this.AddVectorObs(localPosRelToHips);

        if (bp.rb.transform != this.hips && bp.rb.transform != this.handL && bp.rb.transform != this.handR &&
            bp.rb.transform != this.footL && bp.rb.transform != this.footR && bp.rb.transform != this.head)
        {
            this.AddVectorObs(bp.currentXNormalizedRot);
            this.AddVectorObs(bp.currentYNormalizedRot);
            this.AddVectorObs(bp.currentZNormalizedRot);
            this.AddVectorObs(bp.currentStrength / this.m_JdController.maxJointForceLimit);
        }
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations()
    {
        this.m_JdController.GetCurrentJointForces();

        this.AddVectorObs(this.m_DirToTarget.normalized);
        this.AddVectorObs(this.m_JdController.bodyPartsDict[this.hips].rb.position);
        this.AddVectorObs(this.hips.forward);
        this.AddVectorObs(this.hips.up);

        foreach (var bodyPart in this.m_JdController.bodyPartsDict.Values)
        {
            this.CollectObservationBodyPart(bodyPart);
        }
    }

    public override void AgentAction(float[] vectorAction)
    {
        this.m_DirToTarget = this.target.position - this.m_JdController.bodyPartsDict[this.hips].rb.position;

        // Apply action to all relevant body parts.
        if (this.m_IsNewDecisionStep)
        {
            var bpDict = this.m_JdController.bodyPartsDict;
            var i = -1;

            bpDict[this.chest].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            bpDict[this.spine].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);

            bpDict[this.thighL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.thighR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.shinL].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[this.shinR].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[this.footR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
            bpDict[this.footL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);


            bpDict[this.armL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.armR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
            bpDict[this.forearmL].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[this.forearmR].SetJointTargetRotation(vectorAction[++i], 0, 0);
            bpDict[this.head].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);

            //update joint strength settings
            bpDict[this.chest].SetJointStrength(vectorAction[++i]);
            bpDict[this.spine].SetJointStrength(vectorAction[++i]);
            bpDict[this.head].SetJointStrength(vectorAction[++i]);
            bpDict[this.thighL].SetJointStrength(vectorAction[++i]);
            bpDict[this.shinL].SetJointStrength(vectorAction[++i]);
            bpDict[this.footL].SetJointStrength(vectorAction[++i]);
            bpDict[this.thighR].SetJointStrength(vectorAction[++i]);
            bpDict[this.shinR].SetJointStrength(vectorAction[++i]);
            bpDict[this.footR].SetJointStrength(vectorAction[++i]);
            bpDict[this.armL].SetJointStrength(vectorAction[++i]);
            bpDict[this.forearmL].SetJointStrength(vectorAction[++i]);
            bpDict[this.armR].SetJointStrength(vectorAction[++i]);
            bpDict[this.forearmR].SetJointStrength(vectorAction[++i]);
        }

        this.IncrementDecisionTimer();

        // Set reward for this step according to mixture of the following elements.
        // a. Velocity alignment with goal direction.
        // b. Rotation alignment with goal direction.
        // c. Encourage head height.
        // d. Discourage head movement.
        this.AddReward(
            +0.03f * Vector3.Dot(this.m_DirToTarget.normalized, this.m_JdController.bodyPartsDict[this.hips].rb.velocity)
            + 0.01f * Vector3.Dot(this.m_DirToTarget.normalized, this.hips.forward)
            + 0.02f * (this.head.position.y - this.hips.position.y)
            - 0.01f * Vector3.Distance(this.m_JdController.bodyPartsDict[this.head].rb.velocity,
                this.m_JdController.bodyPartsDict[this.hips].rb.velocity)
        );
    }

    /// <summary>
    /// Only change the joint settings based on decision frequency.
    /// </summary>
    public void IncrementDecisionTimer()
    {
        if (this.m_CurrentDecisionStep == this.agentParameters.numberOfActionsBetweenDecisions ||
            this.agentParameters.numberOfActionsBetweenDecisions == 1)
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
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void AgentReset()
    {
        if (this.m_DirToTarget != Vector3.zero)
        {
            this.transform.rotation = Quaternion.LookRotation(this.m_DirToTarget);
        }

        foreach (var bodyPart in this.m_JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        this.m_IsNewDecisionStep = true;
        this.m_CurrentDecisionStep = 1;
        this.SetResetParameters();
    }

    public void SetTorsoMass()
    {
        this.m_ChestRb.mass = this.m_ResetParams.GetPropertyWithDefault("chest_mass", 8);
        this.m_SpineRb.mass = this.m_ResetParams.GetPropertyWithDefault("spine_mass", 10);
        this.m_HipsRb.mass = this.m_ResetParams.GetPropertyWithDefault("hip_mass", 15);
    }

    public void SetResetParameters()
    {
        this.SetTorsoMass();
    }
}
