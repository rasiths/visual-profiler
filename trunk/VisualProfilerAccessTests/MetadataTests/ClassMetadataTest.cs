﻿using NUnit.Framework;
using VisualProfilerAccess.Metadata;

namespace VisualProfilerAccessTests.MetadataTests
{
    [TestFixture]
    public class ClassMetadataTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            AssemblyMetadata.Cache.Clear();
            ModuleMetadata.Cache.Clear();
            ClassMetadata.Cache.Clear();
            _assemblyMetadata = AssemblyMetadata.DeserializeMetadata(_assemblyBytes.ConvertToMemoryStream());
            _moduleMetadata = ModuleMetadata.DeserializeMetadata(_moduleBytes.ConvertToMemoryStream());
            _classMetadata = ClassMetadata.DeserializeMetadata(_classBytes.ConvertToMemoryStream());
        }

        #endregion

        private readonly byte[] _assemblyBytes = {
                                                     0x00, 0x3A, 0x37, 0x00, 0x01, 0x00, 0x00, 0x20, 0x18, 0x00, 0x00,
                                                     0x00, 0x54, 0x00,
                                                     0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x41, 0x00, 0x73, 0x00, 0x73,
                                                     0x00, 0x65, 0x00,
                                                     0x6D, 0x00, 0x62, 0x00, 0x6C, 0x00, 0x79, 0x00, 0x01
                                                 };

        private readonly byte[] _moduleBytes = {
                                                   0x9C, 0x2E, 0x21, 0x00, 0x01, 0x00, 0x00, 0x06, 0x00, 0x3A, 0x37,
                                                   0x00
                                               };

        private readonly byte[] _classBytes = {
                                                  0x6C, 0x34, 0x21, 0x00, 0x02, 0x00, 0x00, 0x02, 0x2E, 0x00, 0x00, 0x00
                                                  , 0x54, 0x00,
                                                  0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x4E, 0x00, 0x61, 0x00, 0x6D, 0x00
                                                  , 0x65, 0x00,
                                                  0x73, 0x00, 0x70, 0x00, 0x61, 0x00, 0x63, 0x00, 0x65, 0x00, 0x2E, 0x00
                                                  , 0x54, 0x00,
                                                  0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x43, 0x00, 0x6C, 0x00, 0x61, 0x00
                                                  , 0x73, 0x00,
                                                  0x73, 0x00, 0x00, 0x9C, 0x2E, 0x21, 0x00
                                              };

        private AssemblyMetadata _assemblyMetadata;
        private ModuleMetadata _moduleMetadata;
        private ClassMetadata _classMetadata;

        private const uint ExpectedId = 0x0021346c;
        private const uint ExpectedMdToken = 0x02000002;
        private const string ExpectedName = "TestNamespace.TestClass";

        [Test]
        public void IdTest()
        {
            Assert.AreEqual(ExpectedId, _classMetadata.Id, "Class id does not match.");
        }

        [Test]
        public void IsGenericTest()
        {
            Assert.IsFalse(_classMetadata.IsGeneric);
        }

        [Test]
        public void MdTokenTest()
        {
            Assert.AreEqual(ExpectedMdToken, _classMetadata.MdToken, "Class MdToken does not match.");
        }

        [Test]
        public void MetadataTypeTest()
        {
            Assert.AreEqual(MetadataTypes.ClassMedatada, _classMetadata.MetadataType);
        }

        [Test]
        public void NameTest()
        {
            Assert.AreEqual(ExpectedName, _classMetadata.Name);
        }

        [Test]
        public void ParentIdTest()
        {
            Assert.IsTrue(ReferenceEquals(_moduleMetadata, _classMetadata.Module),
                          "Class' parent module does not match.");
        }

        [Test]
        public void StaticDeserializeAndCachingTest()
        {
            ClassMetadata.DeserializeMetadata(_classBytes.ConvertToMemoryStream());
            ClassMetadata classMetadata = ClassMetadata.Cache[ExpectedId];
            Assert.IsNotNull(classMetadata, "Data was not inserted into the cache.");
        }
    }
}