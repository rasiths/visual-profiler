using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.Metadata
{
    class MetadataDeserializer
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
            while (byteStream.Position < byteStream.Length)
            {
                var metadataType = DeserializationUtils.DeserializeMetadataType(byteStream);
                MetadataBase result = null;
                switch (metadataType)
                {
                    case MetadataTypes.AssemblyMetadata:
                        result = AssemblyMetadata.DeserializeMetadataAndCacheIt(byteStream);
                        break;
                    case MetadataTypes.ModuleMedatada:
                        result = ModuleMetadata.DeserializeMetadataAndCacheIt(byteStream);
                        break;
                    case MetadataTypes.ClassMedatada:
                        break;
                    case MetadataTypes.MethodMedatada:
                        break;
                    case MetadataTypes.ProfilingData:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Contract.Assume(metadataType == result.MetadataType);
            }
        }
    }
}
