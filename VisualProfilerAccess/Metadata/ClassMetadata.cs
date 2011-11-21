using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.Metadata
{
    class ClassMetadata : MetadataBase<ClassMetadata>
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

            Name = DeserializationUtils.DeserializeString(byteStream);
            IsGeneric = DeserializationUtils.DeserializeBool(byteStream);
            uint moduleId = DeserializationUtils.DeserializeUint32(byteStream);
            Module = ModuleMetadata.Cache[moduleId];

        }
    }
}
