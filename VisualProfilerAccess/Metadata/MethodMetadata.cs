using System.Diagnostics.Contracts;
using System.IO;

namespace VisualProfilerAccess.Metadata
{
    public class MethodMetadata : MetadataBase<MethodMetadata>
    {
        public MethodMetadata(Stream byteStream) : base(byteStream)
        {
            Name = byteStream.DeserializeString();

            uint paramCount = byteStream.DeserializeUint32();
            Parameters = new string[paramCount];
            for (int i = 0; i < paramCount; i++)
            {
                string param = byteStream.DeserializeString();
                Parameters[i] = param;
            }

            ClassId = byteStream.DeserializeUint32();
   
        }

        public string Name { get; private set; }
        public string[] Parameters { get; private set; }
        public uint ClassId { get; private set; }

        public ClassMetadata Class
        {
            get { return ClassMetadata.Cache[ClassId]; }
        }

        public override MetadataTypes MetadataType
        {
            get { return MetadataTypes.MethodMedatada; }
        }
        
        public override string ToString()
        {
            string parameterString = string.Empty;
            foreach (string parameter in Parameters)
            {
                parameterString += parameter + ", ";
            }
            parameterString = parameterString.TrimEnd(", ".ToCharArray());

            string str = string.Format("[{0}]{1}.{2}({3}) - {4}", Class.Module.Assembly.Name, Class, Name, parameterString, Class.Module.FilePath);
            return str;
        }
    }
}