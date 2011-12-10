using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public abstract class MetadataBase
    {
        public uint Id { get; set; }
        public uint MdToken { get; set; }

        public abstract MetadataTypes MetadataType { get; }
        public abstract void AddToStaticCache();
        
    }

    public abstract class MetadataBase<TMetadata> : MetadataBase where TMetadata : MetadataBase<TMetadata>
    {
        private static Dictionary<uint, TMetadata> _cache = new Dictionary<uint, TMetadata>();

        public static Dictionary<uint, TMetadata> Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public override void AddToStaticCache()
        {
            Cache[Id] = (TMetadata) this;
        }

        protected MetadataBase(Stream byteStream)
        {
            Contract.Requires(byteStream != null);
            Id = byteStream.DeserializeUint32();
            MdToken = byteStream.DeserializeUint32();
        }
    }
}