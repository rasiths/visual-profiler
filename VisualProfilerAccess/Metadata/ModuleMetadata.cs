﻿using System.Diagnostics.Contracts;
using System.IO;
using Microsoft.Cci;

namespace VisualProfilerAccess.Metadata
{
    public class ModuleMetadata : MetadataBase<ModuleMetadata>
    {
        public ModuleMetadata(Stream byteStream, MetadataCache<AssemblyMetadata> assemblyCache ) : base(byteStream)
        {
            FilePath = byteStream.DeserializeString();
            AssemblyId = byteStream.DeserializeUint32();
            Assembly = assemblyCache[AssemblyId];
        }

        public string FilePath { get; private set; }
        public uint AssemblyId { get; private set; }

        public AssemblyMetadata Assembly { get; set; }
        
        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.ModuleMedatada; }
        }
        
    }
}