using System.Collections.Generic;

namespace VisualProfilerAccess.Metadata
{
    public class MetadataCache<TMetadata> where TMetadata : MetadataBase<TMetadata>
    {
        private Dictionary<uint, TMetadata> _cache = new Dictionary<uint, TMetadata>();

        public Dictionary<uint, TMetadata> Cache
        {
            get { return _cache; }
            set { _cache = value; }
        }

        public TMetadata this[uint metadataId]
        {
            get
            {
                return _cache[metadataId];
            }
            set
            {
                _cache[metadataId] = value;
            }
        }

        public void Add(TMetadata metadata)
        {
            this[metadata.Id] = metadata;
        }
    }
}