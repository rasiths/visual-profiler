using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VisualProfilerAccess.Metadata;

namespace VisualProfilerAccessTests.MetadataTests
{
    [TestFixture]
    public class ModuleMetadataTest
    {
        private byte[] _assemblyBytes = {
                                      0x00, 0x3A, 0x37, 0x00, 0x01, 0x00, 0x00, 0x20, 0x18, 0x00, 0x00, 0x00, 0x54, 0x00,
                                      0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x41, 0x00, 0x73, 0x00, 0x73, 0x00, 0x65, 0x00,
                                      0x6D, 0x00, 0x62, 0x00, 0x6C, 0x00, 0x79, 0x00, 0x01
                                  };

        private byte[] _moduleBytes = {
                                         0x9C, 0x2E, 0x21, 0x00, 0x01, 0x00, 0x00, 0x06, 0x00, 0x3A, 0x37, 0x00
                                     };

        private AssemblyMetadata _assemblyMetadata;
        private ModuleMetadata _moduleMetadata;

        private const uint ExpectedId = 0x00212e9c;
        private const uint ExpectedMdToken = 0x06000001;

        [SetUp]
        public void SetUp()
        {
            AssemblyMetadata.Cache.Clear();
            ModuleMetadata.Cache.Clear();
            _assemblyMetadata = AssemblyMetadata.DeserializeMetadata(_assemblyBytes.ConvertToMemoryStream());
            _moduleMetadata = ModuleMetadata.DeserializeMetadata(_moduleBytes.ConvertToMemoryStream(), false);
        }

        [Test]
        public void MetadataTypeTest()
        {
            
            Assert.AreEqual(MetadataTypes.ModuleMedatada, _moduleMetadata.MetadataType);
        }


        [Test]
        public void StaticDeserializeAndCachingTest()
        {
            ModuleMetadata.DeserializeMetadata(_moduleBytes.ConvertToMemoryStream());
            ModuleMetadata moduleMetadata = ModuleMetadata.Cache[ExpectedId];
            Assert.IsNotNull(moduleMetadata, "Data was not inserted into the cache.");
        }

        [Test]
        public void IdTest()
        {
            Assert.AreEqual(ExpectedId, _moduleMetadata.Id, "Module id does not match.");
        }

        [Test]
        public void ParentIdTest()
        {
            Assert.IsTrue(object.ReferenceEquals(_assemblyMetadata, _moduleMetadata.Assembly), "Module's parent assembly does not match.");
        }

        [Test]
        public void MdTokenTest()
        {
            Assert.AreEqual(ExpectedMdToken, _moduleMetadata.MdToken, "Module MdToken does not match.");
        }


    }
}
