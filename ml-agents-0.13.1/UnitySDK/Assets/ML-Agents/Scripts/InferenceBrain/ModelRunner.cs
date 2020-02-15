using System.Collections.Generic;
using Barracuda;
using UnityEngine.Profiling;

namespace MLAgents.InferenceBrain
{
    public class ModelRunner
    {
        List<Agent> m_Agents = new List<Agent>();
        ITensorAllocator m_TensorAllocator;
        TensorGenerator m_TensorGenerator;
        TensorApplier m_TensorApplier;

        NNModel m_Model;
        InferenceDevice m_InferenceDevice;
        IWorker m_Engine;
        bool m_Verbose = false;
        string[] m_OutputNames;
        IReadOnlyList<TensorProxy> m_InferenceInputs;
        IReadOnlyList<TensorProxy> m_InferenceOutputs;
        Dictionary<int, List<float>> m_Memories = new Dictionary<int, List<float>>();

        bool m_VisualObservationsInitialized;

        /// <summary>
        /// Initializes the Brain with the Model that it will use when selecting actions for
        /// the agents
        /// </summary>
        /// <param name="model"> The Barracuda model to load </param>
        /// <param name="brainParameters"> The parameters of the Brain used to generate the
        /// placeholder tensors </param>
        /// <param name="inferenceDevice"> Inference execution device. CPU is the fastest
        /// option for most of ML Agents models. </param>
        /// <param name="seed"> The seed that will be used to initialize the RandomNormal
        /// and Multinomial objects used when running inference.</param>
        /// <exception cref="UnityAgentsException">Throws an error when the model is null
        /// </exception>
        public ModelRunner(
            NNModel model,
            BrainParameters brainParameters,
            InferenceDevice inferenceDevice = InferenceDevice.CPU,
            int seed = 0)
        {
            Model barracudaModel;
            this.m_Model = model;
            this.m_InferenceDevice = inferenceDevice;
            this.m_TensorAllocator = new TensorCachingAllocator();
            if (model != null)
            {
#if BARRACUDA_VERBOSE
                m_Verbose = true;
#endif

                D.logEnabled = this.m_Verbose;

                barracudaModel = ModelLoader.Load(model.Value);
                var executionDevice = inferenceDevice == InferenceDevice.GPU
                    ? BarracudaWorkerFactory.Type.ComputePrecompiled
                    : BarracudaWorkerFactory.Type.CSharp;
                this.m_Engine = BarracudaWorkerFactory.CreateWorker(executionDevice, barracudaModel, this.m_Verbose);
            }
            else
            {
                barracudaModel = null;
                this.m_Engine = null;
            }

            this.m_InferenceInputs = BarracudaModelParamLoader.GetInputTensors(barracudaModel);
            this.m_OutputNames = BarracudaModelParamLoader.GetOutputNames(barracudaModel);
            this.m_TensorGenerator = new TensorGenerator(
                seed, this.m_TensorAllocator, this.m_Memories, barracudaModel);
            this.m_TensorApplier = new TensorApplier(
                brainParameters, seed, this.m_TensorAllocator, this.m_Memories, barracudaModel);
        }

        static Dictionary<string, Tensor> PrepareBarracudaInputs(IEnumerable<TensorProxy> infInputs)
        {
            var inputs = new Dictionary<string, Tensor>();
            foreach (var inp in infInputs)
            {
                inputs[inp.name] = inp.data;
            }

            return inputs;
        }

        public void Dispose()
        {
            if (this.m_Engine != null)
                this.m_Engine.Dispose();
            this.m_TensorAllocator?.Reset(false);
        }

        List<TensorProxy> FetchBarracudaOutputs(string[] names)
        {
            var outputs = new List<TensorProxy>();
            foreach (var n in names)
            {
                var output = this.m_Engine.Peek(n);
                outputs.Add(TensorUtils.TensorProxyFromBarracuda(output, n));
            }

            return outputs;
        }

        public void PutObservations(Agent agent)
        {
            this.m_Agents.Add(agent);
        }
        public void DecideBatch()
        {
            var currentBatchSize = this.m_Agents.Count;
            if (currentBatchSize == 0)
            {
                return;
            }

            if (!this.m_VisualObservationsInitialized)
            {
                // Just grab the first agent in the collection (any will suffice, really).
                // We check for an empty Collection above, so this will always return successfully.
                var firstAgent = this.m_Agents[0];
                this.m_TensorGenerator.InitializeObservations(firstAgent, this.m_TensorAllocator);
                this.m_VisualObservationsInitialized = true;
            }

            Profiler.BeginSample("LearningBrain.DecideAction");

            Profiler.BeginSample($"MLAgents.{this.m_Model.name}.GenerateTensors");
            // Prepare the input tensors to be feed into the engine
            this.m_TensorGenerator.GenerateTensors(this.m_InferenceInputs, currentBatchSize, this.m_Agents);
            Profiler.EndSample();

            Profiler.BeginSample($"MLAgents.{this.m_Model.name}.PrepareBarracudaInputs");
            var inputs = PrepareBarracudaInputs(this.m_InferenceInputs);
            Profiler.EndSample();

            // Execute the Model
            Profiler.BeginSample($"MLAgents.{this.m_Model.name}.ExecuteGraph");
            this.m_Engine.Execute(inputs);
            Profiler.EndSample();

            Profiler.BeginSample($"MLAgents.{this.m_Model.name}.FetchBarracudaOutputs");
            this.m_InferenceOutputs = this.FetchBarracudaOutputs(this.m_OutputNames);
            Profiler.EndSample();

            Profiler.BeginSample($"MLAgents.{this.m_Model.name}.ApplyTensors");
            // Update the outputs
            this.m_TensorApplier.ApplyTensors(this.m_InferenceOutputs, this.m_Agents);
            Profiler.EndSample();

            Profiler.EndSample();

            this.m_Agents.Clear();
        }

        public bool HasModel(NNModel other, InferenceDevice otherInferenceDevice)
        {
            return this.m_Model == other && this.m_InferenceDevice == otherInferenceDevice;
        }
    }
}
