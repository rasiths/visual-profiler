using System.Collections.Generic;
using VisualProfilerAccess.Metadata;

namespace VisualProfilerAccess.SourceLocation
{
    public class SourceLocatorFactory : ISourceLocatorFactory
    {
        public static readonly Dictionary<string, SourceLocator> Cache = new Dictionary<string, SourceLocator>();

        public ISourceLocator GetSourceLocator(string modulePath)
        {
            SourceLocator sourceLocator ;
            bool found = Cache.TryGetValue(modulePath, out sourceLocator);
            if(!found)
            {
                Cache[modulePath] = sourceLocator = new SourceLocator(modulePath);
            }

            return sourceLocator;
        }

        public ISourceLocator GetSourceLocator(MethodMetadata methodMd)
        {
            var sourceLocator = GetSourceLocator(methodMd.Class.Module.FilePath);
            return sourceLocator;
        }
    }
}