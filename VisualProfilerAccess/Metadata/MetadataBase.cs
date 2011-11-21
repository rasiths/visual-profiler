using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.Metadata
{
    internal abstract class MetadataBase
    {
        public abstract MetadataTypes MetadataType { get; }
        protected abstract void Deserialize(Stream byteStream);
    }

    internal abstract class MetadataBase<TMetadata> : MetadataBase where TMetadata : MetadataBase<TMetadata>, new()
    {
        private static Dictionary<uint, TMetadata> _cache = new Dictionary<uint, TMetadata>();
      
        public uint Id { get; set; }
        public uint MdToken { get; set; }

        public static Dictionary<uint, TMetadata> Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public static TMetadata DeserializeMetadataAndCacheIt(Stream byteStream)
        {
            Contract.Requires(byteStream != null);
            TMetadata metadata = new TMetadata();

            metadata.Id = DeserializationUtils.DeserializeUint32(byteStream);
            metadata.MdToken = DeserializationUtils.DeserializeUint32(byteStream);
            metadata.Deserialize(byteStream);

            Cache[metadata.Id] = metadata;

            return metadata;
        }
    }
}

