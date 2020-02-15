using System.IO.Abstractions;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MLAgents
{
    /// <summary>
    /// Demonstration Recorder Component.
    /// </summary>
    [RequireComponent(typeof(Agent))]
    public class DemonstrationRecorder : MonoBehaviour
    {
        public bool record;
        public string demonstrationName;
        Agent m_RecordingAgent;
        string m_FilePath;
        DemonstrationStore m_DemoStore;
        public const int MaxNameLength = 16;

        void Start()
        {
            if (Application.isEditor && this.record)
            {
                this.InitializeDemoStore();
            }
        }

        void Update()
        {
            if (Application.isEditor && this.record && this.m_DemoStore == null)
            {
                this.InitializeDemoStore();
            }
        }

        /// <summary>
        /// Creates demonstration store for use in recording.
        /// </summary>
        public void InitializeDemoStore(IFileSystem fileSystem = null)
        {
            this.m_RecordingAgent = this.GetComponent<Agent>();
            this.m_DemoStore = new DemonstrationStore(fileSystem);
            var behaviorParams = this.GetComponent<BehaviorParameters>();
            this.demonstrationName = SanitizeName(this.demonstrationName, MaxNameLength);
            this.m_DemoStore.Initialize(
                this.demonstrationName,
                behaviorParams.brainParameters,
                behaviorParams.behaviorName);
            Monitor.Log("Recording Demonstration of Agent: ", this.m_RecordingAgent.name);
        }

        /// <summary>
        /// Removes all characters except alphanumerics from demonstration name.
        /// Shorten name if it is longer than the maxNameLength.
        /// </summary>
        public static string SanitizeName(string demoName, int maxNameLength)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -]");
            demoName = rgx.Replace(demoName, "");
            // If the string is too long, it will overflow the metadata.
            if (demoName.Length > maxNameLength)
            {
                demoName = demoName.Substring(0, maxNameLength);
            }
            return demoName;
        }

        /// <summary>
        /// Forwards AgentInfo to Demonstration Store.
        /// </summary>
        public void WriteExperience(AgentInfo info)
        {
            this.m_DemoStore.Record(info);
        }

        public void Close()
        {
            if (this.m_DemoStore != null)
            {
                this.m_DemoStore.Close();
                this.m_DemoStore = null;
            }
        }

        /// <summary>
        /// Closes Demonstration store.
        /// </summary>
        void OnApplicationQuit()
        {
            if (Application.isEditor && this.record)
            {
                this.Close();
            }
        }
    }
}
