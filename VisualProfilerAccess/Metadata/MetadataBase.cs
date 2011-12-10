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
        protected abstract void Deserialize(Stream byteStream);
        protected virtual void Initialize() { }
    }

    public abstract class MetadataBase<TMetadata> : MetadataBase where TMetadata : MetadataBase<TMetadata>, new()
    {
        private static Dictionary<uint, TMetadata> _cache = new Dictionary<uint, TMetadata>();

        public static Dictionary<uint, TMetadata> Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public static TMetadata DeserializeAndCacheMetadata(Stream byteStream, bool addToCache = true)
        {
            Contract.Requires(byteStream != null);
            var metadata = new TMetadata();

            metadata.Id = byteStream.DeserializeUint32();
            metadata.MdToken = byteStream.DeserializeUint32();
            metadata.Deserialize(byteStream);
            metadata.Initialize();

            if (addToCache)
            {
                Cache[metadata.Id] = metadata;
            }

            return metadata;
        }
    }
}