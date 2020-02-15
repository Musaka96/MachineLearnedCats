using UnityEngine;
using System;

namespace MLAgents
{

    /// <summary>
    /// The Heuristic Policy uses a hards coded Heuristic method
    /// to take decisions each time the RequestDecision method is
    /// called.
    /// </summary>
    public class HeuristicPolicy : IPolicy
    {
        Func<float[]> m_Heuristic;
        Agent m_Agent;

        /// <inheritdoc />
        public HeuristicPolicy(Func<float[]> heuristic)
        {
            this.m_Heuristic = heuristic;
        }

        /// <inheritdoc />
        public void RequestDecision(Agent agent)
        {
            this.m_Agent = agent;
        }

        /// <inheritdoc />
        public void DecideAction()
        {
            if (this.m_Agent != null)
            {
                this.m_Agent.UpdateVectorAction(this.m_Heuristic.Invoke());
            }
        }

        public void Dispose()
        {

        }
    }
}
