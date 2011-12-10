using System.Diagnostics.Contracts;
using System.IO;
using Microsoft.Cci;

namespace VisualProfilerAccess.Metadata
{
    public class ModuleMetadata : MetadataBase<ModuleMetadata>
    {
        public string FilePath { get; set; }
        public AssemblyMetadata Assembly { get; set; }
        public IModule Module { get; set; }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.ModuleMedatada; }
        }

        protected override void Deserialize(Stream byteStream)
        {
            Contract.Ensures(Assembly != null);
            FilePath = byteStream.DeserializeString();
            uint assemblyId = byteStream.DeserializeUint32();
            Assembly = AssemblyMetadata.Cache[assemblyId];
        }

        protected override void Initialize()
        {
            
        }
    }
}