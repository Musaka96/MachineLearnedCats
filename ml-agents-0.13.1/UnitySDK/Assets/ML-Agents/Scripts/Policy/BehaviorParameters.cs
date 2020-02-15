using Barracuda;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLAgents
{

    /// <summary>
    /// The Factory to generate policies.
    /// </summary>
    public class BehaviorParameters : MonoBehaviour
    {

        [Serializable]
        private enum BehaviorType
        {
            Default,
            HeuristicOnly,
            InferenceOnly
        }

        [HideInInspector]
        [SerializeField]
        BrainParameters m_BrainParameters = new BrainParameters();
        [HideInInspector]
        [SerializeField]
        NNModel m_Model;
        [HideInInspector]
        [SerializeField]
        InferenceDevice m_InferenceDevice;
        [HideInInspector]
        [SerializeField]
        BehaviorType m_BehaviorType;
        [HideInInspector]
        [SerializeField]
        string m_BehaviorName = "My Behavior";
        [HideInInspector] [SerializeField]
        int m_TeamID = 0;
        [HideInInspector]
        [SerializeField]
        [Tooltip("Use all Sensor components attached to child GameObjects of this Agent.")]
        bool m_useChildSensors = true;

        public BrainParameters brainParameters
        {
            get { return this.m_BrainParameters; }
        }

        public bool useChildSensors
        {
            get { return this.m_useChildSensors; }
        }

        public string behaviorName
        {
            
            get { return this.m_BehaviorName + "?team=" + this.m_TeamID;} 

        }

        public IPolicy GeneratePolicy(Func<float[]> heuristic)
        {
            switch (this.m_BehaviorType)
            {
                case BehaviorType.HeuristicOnly:
                    return new HeuristicPolicy(heuristic);
                case BehaviorType.InferenceOnly:
                    return new BarracudaPolicy(this.m_BrainParameters, this.m_Model, this.m_InferenceDevice);
                case BehaviorType.Default:
                    if (FindObjectOfType<Academy>().IsCommunicatorOn)
                    {
                        return new RemotePolicy(this.m_BrainParameters, this.behaviorName);
                    }
                    if (this.m_Model != null)
                    {
                        return new BarracudaPolicy(this.m_BrainParameters, this.m_Model, this.m_InferenceDevice);
                    }
                    else
                    {
                        return new HeuristicPolicy(heuristic);
                    }
                default:
                    return new HeuristicPolicy(heuristic);
            }
        }

        public void GiveModel(
            string behaviorName,
            NNModel model,
            InferenceDevice inferenceDevice = InferenceDevice.CPU)
        {
            this.m_Model = model;
            this.m_InferenceDevice = inferenceDevice;
            this.m_BehaviorName = behaviorName;
        }
    }
}
