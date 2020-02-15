using System.Collections.Generic;
using UnityEngine;

namespace MLAgents.Sensor
{
    public class VectorSensor : ISensor
    {
        // TODO use float[] instead
        // TOOD allow setting float[]
        List<float> m_Observations;
        int[] m_Shape;
        string m_Name;

        public VectorSensor(int observationSize, string name = null)
        {
            if (name == null)
            {
                name = $"VectorSensor_size{observationSize}";
            }

            this.m_Observations = new List<float>(observationSize);
            this.m_Name = name;
            this.m_Shape = new[] { observationSize };
        }

        public int Write(WriteAdapter adapter)
        {
            var expectedObservations = this.m_Shape[0];
            if (this.m_Observations.Count > expectedObservations)
            {
                // Too many observations, truncate
                Debug.LogWarningFormat(
                    "More observations ({0}) made than vector observation size ({1}). The observations will be truncated.",
                    this.m_Observations.Count, expectedObservations
                );
                this.m_Observations.RemoveRange(expectedObservations, this.m_Observations.Count - expectedObservations);
            }
            else if (this.m_Observations.Count < expectedObservations)
            {
                // Not enough observations; pad with zeros.
                Debug.LogWarningFormat(
                    "Fewer observations ({0}) made than vector observation size ({1}). The observations will be padded.",
                    this.m_Observations.Count, expectedObservations
                );
                for (int i = this.m_Observations.Count; i < expectedObservations; i++)
                {
                    this.m_Observations.Add(0);
                }
            }
            adapter.AddRange(this.m_Observations);
            return expectedObservations;
        }

        public void Update()
        {
            this.Clear();
        }

        public int[] GetFloatObservationShape()
        {
            return this.m_Shape;
        }

        public string GetName()
        {
            return this.m_Name;
        }

        public virtual byte[] GetCompressedObservation()
        {
            return null;
        }

        public virtual SensorCompressionType GetCompressionType()
        {
            return SensorCompressionType.None;
        }

        void Clear()
        {
            this.m_Observations.Clear();
        }

        void AddFloatObs(float obs)
        {
            this.m_Observations.Add(obs);
        }

        // Compatibility methods with Agent observation. These should be removed eventually.

        /// <summary>
        /// Adds a float observation to the vector observations of the agent.
        /// </summary>
        /// <param name="observation">Observation.</param>
        public void AddObservation(float observation)
        {
            this.AddFloatObs(observation);
        }

        /// <summary>
        /// Adds an integer observation to the vector observations of the agent.
        /// </summary>
        /// <param name="observation">Observation.</param>
        public void AddObservation(int observation)
        {
            this.AddFloatObs(observation);
        }

        /// <summary>
        /// Adds an Vector3 observation to the vector observations of the agent.
        /// </summary>
        /// <param name="observation">Observation.</param>
        public void AddObservation(Vector3 observation)
        {
            this.AddFloatObs(observation.x);
            this.AddFloatObs(observation.y);
            this.AddFloatObs(observation.z);
        }

        /// <summary>
        /// Adds an Vector2 observation to the vector observations of the agent.
        /// </summary>
        /// <param name="observation">Observation.</param>
        public void AddObservation(Vector2 observation)
        {
            this.AddFloatObs(observation.x);
            this.AddFloatObs(observation.y);
        }

        /// <summary>
        /// Adds a collection of float observations to the vector observations of the agent.
        /// </summary>
        /// <param name="observation">Observation.</param>
        public void AddObservation(IEnumerable<float> observation)
        {
            foreach (var f in observation)
            {
                this.AddFloatObs(f);
            }
        }

        /// <summary>
        /// Adds a quaternion observation to the vector observations of the agent.
        /// </summary>
        /// <param name="observation">Observation.</param>
        public void AddObservation(Quaternion observation)
        {
            this.AddFloatObs(observation.x);
            this.AddFloatObs(observation.y);
            this.AddFloatObs(observation.z);
            this.AddFloatObs(observation.w);
        }

        /// <summary>
        /// Adds a boolean observation to the vector observation of the agent.
        /// </summary>
        /// <param name="observation"></param>
        public void AddObservation(bool observation)
        {
            this.AddFloatObs(observation ? 1f : 0f);
        }


        public void AddOneHotObservation(int observation, int range)
        {
            for (var i = 0; i < range; i++)
            {
                this.AddFloatObs(i == observation ? 1.0f : 0.0f);
            }
        }
    }
}
