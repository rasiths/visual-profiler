using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public class ModuleMetadata : MetadataBase<ModuleMetadata>
    {
        public AssemblyMetadata Assembly { get; set; }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.ModuleMedatada; }
        }

        protected override void Deserialize(Stream byteStream)
        {
            Contract.Ensures(Assembly != null);
            uint assemblyId = byteStream.DeserializeUint32();
            Assembly = AssemblyMetadata.Cache[assemblyId];
        }
    }
}