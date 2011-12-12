using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualProfilerAccess.Metadata;
using VisualProfilerAccess.SourceLocation;

namespace VisualProfilerAccessTests.SourceLocationTests
{
    class SrcLocatorMockupFkt : ISourceLocatorFactory
    {
        public ISourceLocator GetSourceLocator(string modulePath)
        {
            SrcLocatorMockup srcLocatorMockup = new SrcLocatorMockup(modulePath);
            return srcLocatorMockup;
        }

        public ISourceLocator GetSourceLocator(MethodMetadata methodMd)
        {
            return GetSourceLocator(methodMd.Class.Module.FilePath);
        }
    }

    class SrcLocatorMockup : ISourceLocator
    {
        private readonly string _modulePath;

        public SrcLocatorMockup(string modulePath)
        {
            _modulePath = modulePath;
        }

        public void Dispose()
        {
        }

        public IEnumerable<IMethodLine> GetMethodLines(uint methodMdToken)
        {
            MethodLineMockup[] methodLineMockups = new []{ new MethodLineMockup(),new MethodLineMockup(),new MethodLineMockup()};
            return methodLineMockups;
        }

        public string GetSourceFilePath(uint methodMdToken)
        {
            return _modulePath + ".cs";
        }
    }

    class MethodLineMockup : IMethodLine
    {
        public int StartLine
        {
            get { return 111; }
        }

        public int StartColumn
        {
            get { return 9; }
        }

        public int EndColumn
        {
            get { return 23; }
        }

        public int StartIndex
        {
            get { return 1125; }
        }

        public int EndIndex
        {
            get { return 1222; }
        }

        public int EndLine
        {
            get { return 111; }
        }
    }
}
