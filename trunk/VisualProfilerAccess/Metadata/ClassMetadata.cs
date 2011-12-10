using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public class ClassMetadata : MetadataBase<ClassMetadata>
    {
        public ClassMetadata(Stream byteStream) : base(byteStream)
        {
            Name = byteStream.DeserializeString();
            IsGeneric = byteStream.DeserializeBool();
            ModuleId = byteStream.DeserializeUint32();
        }

        public string Name { get; private set; }
        public bool IsGeneric { get; private set; }
        public ModuleMetadata Module { get { return ModuleMetadata.Cache[ModuleId];}  }
        public uint ModuleId { get; private set; }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.ClassMedatada; }
        }

        public override string ToString()
        {
            string className = Name + (IsGeneric ? "<>" : string.Empty);
            return className;
        }
    }
}