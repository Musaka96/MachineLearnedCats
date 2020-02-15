using System.IO;
using System.IO.Abstractions;
using Google.Protobuf;
using UnityEngine;

namespace MLAgents
{
    /// <summary>
    /// Responsible for writing demonstration data to file.
    /// </summary>
    public class DemonstrationStore
    {
        public const int MetaDataBytes = 32; // Number of bytes allocated to metadata in demo file.
        readonly IFileSystem m_FileSystem;
        const string k_DemoDirectory = "Assets/Demonstrations/";
        const string k_ExtensionType = ".demo";

        string m_FilePath;
        DemonstrationMetaData m_MetaData;
        Stream m_Writer;
        float m_CumulativeReward;

        public DemonstrationStore(IFileSystem fileSystem)
        {
            if (fileSystem != null)
            {
                this.m_FileSystem = fileSystem;
            }
            else
            {
                this.m_FileSystem = new FileSystem();
            }
        }

        /// <summary>
        /// Initializes the Demonstration Store, and writes initial data.
        /// </summary>
        public void Initialize(
            string demonstrationName, BrainParameters brainParameters, string brainName)
        {
            this.CreateDirectory();
            this.CreateDemonstrationFile(demonstrationName);
            this.WriteBrainParameters(brainName, brainParameters);
        }

        /// <summary>
        /// Checks for the existence of the Demonstrations directory
        /// and creates it if it does not exist.
        /// </summary>
        void CreateDirectory()
        {
            if (!this.m_FileSystem.Directory.Exists(k_DemoDirectory))
            {
                this.m_FileSystem.Directory.CreateDirectory(k_DemoDirectory);
            }
        }

        /// <summary>
        /// Creates demonstration file.
        /// </summary>
        void CreateDemonstrationFile(string demonstrationName)
        {
            // Creates demonstration file.
            var literalName = demonstrationName;
            this.m_FilePath = k_DemoDirectory + literalName + k_ExtensionType;
            var uniqueNameCounter = 0;
            while (this.m_FileSystem.File.Exists(this.m_FilePath))
            {
                literalName = demonstrationName + "_" + uniqueNameCounter;
                this.m_FilePath = k_DemoDirectory + literalName + k_ExtensionType;
                uniqueNameCounter++;
            }

            this.m_Writer = this.m_FileSystem.File.Create(this.m_FilePath);
            this.m_MetaData = new DemonstrationMetaData { demonstrationName = demonstrationName };
            var metaProto = this.m_MetaData.ToProto();
            metaProto.WriteDelimitedTo(this.m_Writer);
        }

        /// <summary>
        /// Writes brain parameters to file.
        /// </summary>
        void WriteBrainParameters(string brainName, BrainParameters brainParameters)
        {
            // Writes BrainParameters to file.
            this.m_Writer.Seek(MetaDataBytes + 1, 0);
            var brainProto = brainParameters.ToProto(brainName, false);
            brainProto.WriteDelimitedTo(this.m_Writer);
        }

        /// <summary>
        /// Write AgentInfo experience to file.
        /// </summary>
        public void Record(AgentInfo info)
        {
            // Increment meta-data counters.
            this.m_MetaData.numberExperiences++;
            this.m_CumulativeReward += info.reward;
            if (info.done)
            {
                this.EndEpisode();
            }

            // Write AgentInfo to file.
            var agentProto = info.ToInfoActionPairProto();
            agentProto.WriteDelimitedTo(this.m_Writer);
        }

        /// <summary>
        /// Performs all clean-up necessary
        /// </summary>
        public void Close()
        {
            this.EndEpisode();
            this.m_MetaData.meanReward = this.m_CumulativeReward / this.m_MetaData.numberEpisodes;
            this.WriteMetadata();
            this.m_Writer.Close();
        }

        /// <summary>
        /// Performs necessary episode-completion steps.
        /// </summary>
        void EndEpisode()
        {
            this.m_MetaData.numberEpisodes += 1;
        }

        /// <summary>
        /// Writes meta-data.
        /// </summary>
        void WriteMetadata()
        {
            var metaProto = this.m_MetaData.ToProto();
            var metaProtoBytes = metaProto.ToByteArray();
            this.m_Writer.Write(metaProtoBytes, 0, metaProtoBytes.Length);
            this.m_Writer.Seek(0, 0);
            metaProto.WriteDelimitedTo(this.m_Writer);
        }
    }
}
