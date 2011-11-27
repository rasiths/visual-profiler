using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public class AssemblyMetadata : MetadataBase<AssemblyMetadata>
    {
        public string Name { get; set; }
        public bool IsProfilingEnabled { get; set; }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.AssemblyMetadata; }
        }

        protected override void Deserialize(Stream byteStream)
        {
            Name = byteStream.DeserializeString();
            IsProfilingEnabled = byteStream.DeserializeBool();
        }
    }
}