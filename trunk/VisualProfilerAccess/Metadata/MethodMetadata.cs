using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.Metadata
{
    class MethodMetadata : MetadataBase<MethodMetadata>
    {
        public string Name { get; set; }
        public string[] Parameters { get; set; }
        public ClassMetadata Class { get; set; }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.MethodMedatada; }
        }

        protected override void Deserialize(Stream byteStream)
        {
            Contract.Ensures(Class != null);

            Name = DeserializationUtils.DeserializeString(byteStream);

            uint paramCount = DeserializationUtils.DeserializeUint32(byteStream);
            Parameters = new string[paramCount];
            for (int i = 0; i < paramCount; i++)
            {
                string param = DeserializationUtils.DeserializeString(byteStream);
                Parameters[i] = param;
            }

            uint classId = DeserializationUtils.DeserializeUint32(byteStream);
            Class = ClassMetadata.Cache[classId];
        }
    }
}
