using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    class ModuleMetadata : MetadataBase<ModuleMetadata>
    {
        public string FileName { get; set; }
        public AssemblyMetadata Assembly { get; set; }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.ModuleMedatada; }
        }

        protected override void Deserialize(Stream byteStream)
        {
           	Contract.Ensures(Assembly != null);

            FileName = DeserializationUtils.DeserializeString(byteStream);
            uint assemblyId = DeserializationUtils.DeserializeUint32(byteStream);
            Assembly = AssemblyMetadata.Cache[assemblyId];
        }
    }
}
