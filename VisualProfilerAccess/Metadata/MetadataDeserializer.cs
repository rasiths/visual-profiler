using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.Metadata
{
    public class MetadataDeserializer
    {
        private enum OutboundMessageTypes
        {
            //Frontend -> Backend
            SendMetadata = 1,
            SendProfilingData = 2,
            Terminate = 3,
        }

        public static void DeserializeAllMetadataAndCacheIt(Stream byteStream)
        {
            uint metadataByteCount = DeserializationUtils.DeserializeUint32(byteStream);
            uint metadataLastBytePosition = metadataByteCount + sizeof(uint);
            while (byteStream.Position < metadataLastBytePosition)
            {
                var metadataType = DeserializationUtils.DeserializeMetadataType(byteStream);
                MetadataBase result = null;
                switch (metadataType)
                {
                    case MetadataTypes.AssemblyMetadata:
                        result = AssemblyMetadata.DeserializeMetadata(byteStream);
                        break;
                    case MetadataTypes.ModuleMedatada:
                        result = ModuleMetadata.DeserializeMetadata(byteStream);
                        break;
                    case MetadataTypes.ClassMedatada:
                        result = ClassMetadata.DeserializeMetadata(byteStream);
                        break;
                    case MetadataTypes.MethodMedatada:
                        result = MethodMetadata.DeserializeMetadata(byteStream);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Contract.Assume(result != null);
                Contract.Assume(metadataType == result.MetadataType);
            }
        }
    }
}
