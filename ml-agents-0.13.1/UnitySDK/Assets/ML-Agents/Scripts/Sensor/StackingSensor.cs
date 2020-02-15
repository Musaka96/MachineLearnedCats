namespace MLAgents.Sensor
{
    /// <summary>
    /// Sensor that wraps around another Sensor to provide temporal stacking.
    /// Conceptually, consecutive observations are stored left-to-right, which is how they're output
    /// For example, 4 stacked sets of observations would be output like
    ///   |  t = now - 3  |  t = now -3  |  t = now - 2  |  t = now  |
    /// Internally, a circular buffer of arrays is used. The m_CurrentIndex represents the most recent observation.
    /// </summary>
    public class StackingSensor : ISensor
    {
        /// <summary>
        /// The wrapped sensor.
        /// </summary>
        ISensor m_WrappedSensor;

        /// <summary>
        /// Number of stacks to save
        /// </summary>
        int m_NumStackedObservations;
        int m_UnstackedObservationSize;

        string m_Name;
        int[] m_Shape;

        /// <summary>
        /// Buffer of previous observations
        /// </summary>
        float[][] m_StackedObservations;

        int m_CurrentIndex;
        WriteAdapter m_LocalAdapter = new WriteAdapter();

        /// <summary>
        ///
        /// </summary>
        /// <param name="wrapped">The wrapped sensor</param>
        /// <param name="numStackedObservations">Number of stacked observations to keep</param>
        public StackingSensor(ISensor wrapped, int numStackedObservations)
        {
            // TODO ensure numStackedObservations > 1
            this.m_WrappedSensor = wrapped;
            this.m_NumStackedObservations = numStackedObservations;

            this.m_Name = $"StackingSensor_size{numStackedObservations}_{wrapped.GetName()}";

            var shape = wrapped.GetFloatObservationShape();
            this.m_Shape = new int[shape.Length];

            this.m_UnstackedObservationSize = wrapped.ObservationSize();
            for (int d = 0; d < shape.Length; d++)
            {
                this.m_Shape[d] = shape[d];
            }

            // TODO support arbitrary stacking dimension
            this.m_Shape[0] *= numStackedObservations;
            this.m_StackedObservations = new float[numStackedObservations][];
            for (var i = 0; i < numStackedObservations; i++)
            {
                this.m_StackedObservations[i] = new float[this.m_UnstackedObservationSize];
            }
        }

        public int Write(WriteAdapter adapter)
        {
            // First, call the wrapped sensor's write method. Make sure to use our own adapater, not the passed one.
            this.m_LocalAdapter.SetTarget(this.m_StackedObservations[this.m_CurrentIndex], 0);
            this.m_WrappedSensor.Write(this.m_LocalAdapter);

            // Now write the saved observations (oldest first)
            var numWritten = 0;
            for (var i = 0; i < this.m_NumStackedObservations; i++)
            {
                var obsIndex = (this.m_CurrentIndex + 1 + i) % this.m_NumStackedObservations;
                adapter.AddRange(this.m_StackedObservations[obsIndex], numWritten);
                numWritten += this.m_UnstackedObservationSize;
            }

            return numWritten;
        }

        /// <summary>
        /// Updates the index of the "current" buffer.
        /// </summary>
        public void Update()
        {
            this.m_WrappedSensor.Update();
            this.m_CurrentIndex = (this.m_CurrentIndex + 1) % this.m_NumStackedObservations;
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

        // TODO support stacked compressed observations (byte stream)

    }
}
