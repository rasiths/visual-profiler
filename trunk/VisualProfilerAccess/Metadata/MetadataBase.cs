using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualProfilerAccess.Metadata
{
    public abstract class MetadataBase
    {
        public uint Id { get; set; }
        public uint MdToken { get; set; }

        public abstract MetadataTypes MetadataType { get; }
        protected abstract void Deserialize(Stream byteStream);
    }

    public abstract class MetadataBase<TMetadata> : MetadataBase where TMetadata : MetadataBase<TMetadata>, new()
    {
        private static Dictionary<uint, TMetadata> _cache = new Dictionary<uint, TMetadata>();
      
        public static Dictionary<uint, TMetadata> Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public static TMetadata DeserializeMetadata(Stream byteStream, bool addToCache = true)
        {
            Contract.Requires(byteStream != null);
            TMetadata metadata = new TMetadata();

            metadata.Id = DeserializationUtils.DeserializeUint32(byteStream);
            metadata.MdToken = DeserializationUtils.DeserializeUint32(byteStream);
            metadata.Deserialize(byteStream);

            if (addToCache)
            {
                Cache[metadata.Id] = metadata;
            }

            return metadata;
        }
    }
}

