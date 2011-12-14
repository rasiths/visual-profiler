using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public abstract class MetadataBase
    {
        public uint Id { get; set; }
        public uint MdToken { get; set; }

        public abstract MetadataTypes MetadataType { get; }
    }

    public abstract class MetadataBase<TMetadata> : MetadataBase where TMetadata : MetadataBase<TMetadata>
    {
        protected MetadataBase(Stream byteStream)
        {
            Contract.Requires(byteStream != null);
            Id = byteStream.DeserializeUint32();
            MdToken = byteStream.DeserializeUint32();
        }
    }
}