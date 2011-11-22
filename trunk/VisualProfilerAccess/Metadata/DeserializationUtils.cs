using System;
using System.IO;
using System.Text;

namespace VisualProfilerAccess.Metadata
{
    public class DeserializationUtils
    {
        public static uint DeserializeUint32(Stream byteStream)
        {
            byte[] buffer = new byte[sizeof(uint)];
            byteStream.Read(buffer, 0, buffer.Length);
            uint uInt32 = BitConverter.ToUInt32(buffer, 0);
            return uInt32;
        }

        public static string DeserializeString(Stream byteStream)
        {
            uint stringLength = DeserializeUint32(byteStream);
            byte[] buffer = new byte[stringLength];
            byteStream.Read(buffer, 0, buffer.Length);
            string s = Encoding.Unicode.GetString(buffer);
            return s;
        }

        public static bool DeserializeBool(Stream byteStream)
        {
            byte[] buffer = new byte[sizeof(bool)];
            byteStream.Read(buffer, 0, buffer.Length);
            bool b = BitConverter.ToBoolean(buffer, 0);
            return b;
        }

        public static MetadataTypes DeserializeMetadataType(Stream stream)
        {
            uint uint32 = DeserializeUint32(stream);
            MetadataTypes messageTypes = (MetadataTypes) uint32;
            return messageTypes;
        }
    }
}