using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public static class MetadataDeserializer
    {
        public static void DeserializeAllMetadataAndCacheIt(Stream byteStream)
        {
            long initialStreamPostion = byteStream.Position;
            uint metadataByteCount = byteStream.DeserializeUint32();
            long metadataLastBytePosition = metadataByteCount + sizeof (uint) + initialStreamPostion;
            while (byteStream.Position < metadataLastBytePosition)
            {
                MetadataTypes metadataType = byteStream.DeserializeMetadataType();
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