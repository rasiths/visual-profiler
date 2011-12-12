using System;
using System.Diagnostics.Contracts;
using System.IO;
using VisualProfilerAccess.SourceLocation;

namespace VisualProfilerAccess.Metadata
{
    public static class MetadataDeserializer
    {
        private static ISourceLocatorFactory _sourceLocatorFactory = new SourceLocatorFactory();

        public static ISourceLocatorFactory SourceLocatorFactory
        {
            get { return _sourceLocatorFactory; }
            set { _sourceLocatorFactory = value; }
        }

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
                        AssemblyMetadata assemblyMetadata = new AssemblyMetadata(byteStream);
                        result = assemblyMetadata;
                        break;
                    case MetadataTypes.ModuleMedatada:
                        ModuleMetadata moduleMetadata = new ModuleMetadata(byteStream);
                        result = moduleMetadata;
                        break;
                    case MetadataTypes.ClassMedatada:
                        ClassMetadata classMetadata = new ClassMetadata(byteStream);
                        result = classMetadata;
                        break;
                    case MetadataTypes.MethodMedatada:
                        MethodMetadata methodMetadata = new MethodMetadata(byteStream);
                        methodMetadata.SourceLocatorFactory = _sourceLocatorFactory;
                        result = methodMetadata;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                result.AddToStaticCache();

                Contract.Assume(result != null);
                Contract.Assume(metadataType == result.MetadataType);
            }
        }
    }
}