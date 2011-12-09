using NUnit.Framework;
using VisualProfilerAccess.Metadata;

namespace VisualProfilerAccessTests.MetadataTests
{
    [TestFixture]
    public class MethodMetadataTest
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            AssemblyMetadata.Cache.Clear();
            ModuleMetadata.Cache.Clear();
            ClassMetadata.Cache.Clear();
            ModuleMetadata.Cache.Clear();
            _assemblyMetadata = AssemblyMetadata.DeserializeMetadata(_assemblyBytes.ConvertToMemoryStream());
            _moduleMetadata = ModuleMetadata.DeserializeMetadata(_moduleBytes.ConvertToMemoryStream());
            _classMetadata = ClassMetadata.DeserializeMetadata(_classBytes.ConvertToMemoryStream());

            _methodMetadata1 = MethodMetadata.DeserializeMetadata(_method1Bytes.ConvertToMemoryStream(), false);
            _methodMetadata2 = MethodMetadata.DeserializeMetadata(_method2Bytes.ConvertToMemoryStream(), false);
            _methodMetadata3 = MethodMetadata.DeserializeMetadata(_method3Bytes.ConvertToMemoryStream(), false);
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
                                                   0x9C, 0x2E, 0x21, 0x00, 0x01, 0x00, 0x00, 0x06,
                                                   0x80, 0x00, 0x00, 0x00, 0x44, 0x00, 0x3a, 0x00, 0x5c, 0x00, 0x48,
                                                   0x00, 0x6f, 0x00, 0x6e, 0x00, 0x7a, 0x00, 0x69, 0x00, 0x6b, 0x00,
                                                   0x5c, 0x00, 0x44, 0x00, 0x65, 0x00, 0x73, 0x00, 0x6b, 0x00, 0x74,
                                                   0x00, 0x6f, 0x00, 0x70, 0x00, 0x5c, 0x00, 0x4d, 0x00, 0x61, 0x00,
                                                   0x6e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x6c, 0x00, 0x62, 0x00, 0x72,
                                                   0x00, 0x6f, 0x00, 0x74, 0x00, 0x5c, 0x00, 0x4d, 0x00, 0x61, 0x00,
                                                   0x6e, 0x00, 0x64, 0x00, 0x65, 0x00, 0x6c, 0x00, 0x62, 0x00, 0x72,
                                                   0x00, 0x6f, 0x00, 0x74, 0x00, 0x5c, 0x00, 0x62, 0x00, 0x69, 0x00,
                                                   0x6e, 0x00, 0x5c, 0x00, 0x44, 0x00, 0x65, 0x00, 0x62, 0x00, 0x75,
                                                   0x00, 0x67, 0x00, 0x5c, 0x00, 0x4d, 0x00, 0x61, 0x00, 0x6e, 0x00,
                                                   0x64, 0x00, 0x65, 0x00, 0x6c, 0x00, 0x62, 0x00, 0x72, 0x00, 0x6f,
                                                   0x00, 0x74, 0x00, 0x2e, 0x00, 0x65, 0x00, 0x78, 0x00, 0x65, 0x00,
                                                   0x00, 0x3A, 0x37,0x00
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

        private readonly byte[] _method1Bytes = {
                                                    0x34, 0x34, 0x21, 0x00, 0x01, 0x00, 0x00, 0x06, 0x08, 0x00, 0x00,
                                                    0x00, 0x4D,
                                                    0x00, 0x61, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x01, 0x00, 0x00, 0x00,
                                                    0x08, 0x00,
                                                    0x00, 0x00, 0x61, 0x00, 0x72, 0x00, 0x67, 0x00, 0x73, 0x00, 0x6C,
                                                    0x34, 0x21,
                                                    0x00
                                                };

        private readonly byte[] _method2Bytes = {
                                                    0x4C, 0x34, 0x21, 0x00, 0x03, 0x00, 0x00, 0x06,
                                                    0x16, 0x00, 0x00, 0x00, 0x4F, 0x00, 0x74, 0x00, 0x68, 0x00, 0x65,
                                                    0x00, 0x72,
                                                    0x00, 0x4D, 0x00, 0x65, 0x00, 0x74, 0x00, 0x68, 0x00, 0x6F, 0x00,
                                                    0x64, 0x00,
                                                    0x00, 0x00, 0x00, 0x00, 0x6C, 0x34, 0x21, 0x00
                                                };

        private readonly byte[] _method3Bytes = {
                                                    0x58, 0x34, 0x21, 0x00, 0x04, 0x00, 0x00, 0x06, 0x32, 0x00, 0x00,
                                                    0x00, 0x54,
                                                    0x00, 0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x4D, 0x00, 0x65, 0x00,
                                                    0x73, 0x00,
                                                    0x73, 0x00, 0x61, 0x00, 0x67, 0x00, 0x65, 0x00, 0x57, 0x00, 0x69,
                                                    0x00, 0x74,
                                                    0x00, 0x68, 0x00, 0x32, 0x00, 0x41, 0x00, 0x72, 0x00, 0x67, 0x00,
                                                    0x75, 0x00,
                                                    0x6D, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x74, 0x00, 0x73, 0x00, 0x02,
                                                    0x00, 0x00,
                                                    0x00, 0x1A, 0x00, 0x00, 0x00, 0x74, 0x00, 0x65, 0x00, 0x73, 0x00,
                                                    0x74, 0x00,
                                                    0x41, 0x00, 0x72, 0x00, 0x67, 0x00, 0x75, 0x00, 0x6D, 0x00, 0x65,
                                                    0x00, 0x6E,
                                                    0x00, 0x74, 0x00, 0x41, 0x00, 0x1A, 0x00, 0x00, 0x00, 0x74, 0x00,
                                                    0x65, 0x00,
                                                    0x73, 0x00, 0x74, 0x00, 0x41, 0x00, 0x72, 0x00, 0x67, 0x00, 0x75,
                                                    0x00, 0x6D,
                                                    0x00, 0x65, 0x00, 0x6E, 0x00, 0x74, 0x00, 0x42, 0x00, 0x6C, 0x34,
                                                    0x21, 0x00
                                                };

        private AssemblyMetadata _assemblyMetadata;
        private ModuleMetadata _moduleMetadata;
        private ClassMetadata _classMetadata;
        private MethodMetadata _methodMetadata1;
        private MethodMetadata _methodMetadata2;
        private MethodMetadata _methodMetadata3;
        private const uint ExpectedId1 = 0x00213434;
        private const uint ExpectedId2 = 0x0021344c;
        private const uint ExpectedId3 = 0x00213458;
        private const uint ExpectedMdToken1 = 0x06000001;
        private const uint ExpectedMdToken2 = 0x06000003;
        private const uint ExpectedMdToken3 = 0x06000004;
        private const string ExpectedName1 = "Main";
        private const string ExpectedName2 = "OtherMethod";
        private const string ExpectedName3 = "TestMessageWith2Arguments";

        [Test]
        public void IdTest()
        {
            Assert.AreEqual(ExpectedId1, _methodMetadata1.Id, "Method1 id does not match.");
            Assert.AreEqual(ExpectedId2, _methodMetadata2.Id, "Method2 id does not match.");
            Assert.AreEqual(ExpectedId3, _methodMetadata3.Id, "Method3 id does not match.");
        }

        [Test]
        public void MdTokenTest()
        {
            Assert.AreEqual(ExpectedMdToken1, _methodMetadata1.MdToken, "Method1 MdToken does not match.");
            Assert.AreEqual(ExpectedMdToken2, _methodMetadata2.MdToken, "Method2 MdToken does not match.");
            Assert.AreEqual(ExpectedMdToken3, _methodMetadata3.MdToken, "Method3 MdToken does not match.");
        }

        [Test]
        public void MetadataTypeTest()
        {
            Assert.AreEqual(MetadataTypes.MethodMedatada, _methodMetadata1.MetadataType);
            Assert.AreEqual(MetadataTypes.MethodMedatada, _methodMetadata2.MetadataType);
            Assert.AreEqual(MetadataTypes.MethodMedatada, _methodMetadata3.MetadataType);
        }

        [Test]
        public void NameTest()
        {
            Assert.AreEqual(ExpectedName1, _methodMetadata1.Name);
            Assert.AreEqual(ExpectedName2, _methodMetadata2.Name);
            Assert.AreEqual(ExpectedName3, _methodMetadata3.Name);
        }

        [Test]
        public void ParemetersTest()
        {
            Assert.AreEqual(1, _methodMetadata1.Parameters.Length);
            Assert.AreEqual("args", _methodMetadata1.Parameters[0]);

            Assert.AreEqual(0, _methodMetadata2.Parameters.Length);

            Assert.AreEqual(2, _methodMetadata3.Parameters.Length);
            Assert.AreEqual("testArgumentA", _methodMetadata3.Parameters[0]);
            Assert.AreEqual("testArgumentB", _methodMetadata3.Parameters[1]);
        }

        [Test]
        public void ParentIdTest()
        {
            Assert.IsTrue(ReferenceEquals(_classMetadata, _methodMetadata1.Class),
                          "Method1's parent module does not match.");
            Assert.IsTrue(ReferenceEquals(_classMetadata, _methodMetadata2.Class),
                          "Method2's parent module does not match.");
            Assert.IsTrue(ReferenceEquals(_classMetadata, _methodMetadata3.Class),
                          "Method3's parent module does not match.");
        }

        [Test]
        public void StaticDeserializeAndCachingTest()
        {
            MethodMetadata.DeserializeMetadata(_method1Bytes.ConvertToMemoryStream());
            MethodMetadata.DeserializeMetadata(_method2Bytes.ConvertToMemoryStream());
            MethodMetadata.DeserializeMetadata(_method3Bytes.ConvertToMemoryStream());
            MethodMetadata methodMetadata1 = MethodMetadata.Cache[ExpectedId1];
            MethodMetadata methodMetadata2 = MethodMetadata.Cache[ExpectedId2];
            MethodMetadata methodMetadata3 = MethodMetadata.Cache[ExpectedId3];
            Assert.IsNotNull(methodMetadata1, "Data was not inserted into the cache.");
            Assert.IsNotNull(methodMetadata2, "Data was not inserted into the cache.");
            Assert.IsNotNull(methodMetadata3, "Data was not inserted into the cache.");
        }
    }
}