using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public class ClassMetadata : MetadataBase<ClassMetadata>
    {
        public string Name { get; set; }
        public bool IsGeneric { get; set; }
        public ModuleMetadata Module { get; set; }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.ClassMedatada; }
        }

        protected override void Deserialize(Stream byteStream)
        {
            Contract.Ensures(Module != null);

            Name = byteStream.DeserializeString();
            IsGeneric = byteStream.DeserializeBool();
            uint moduleId = byteStream.DeserializeUint32();
            Module = ModuleMetadata.Cache[moduleId];
        }

        public override string ToString()
        {
            string className = Name + (IsGeneric ? "<>" : string.Empty);
            return className;
        }
    }
}