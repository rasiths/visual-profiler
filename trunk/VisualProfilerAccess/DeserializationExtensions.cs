using System;
using System.IO;
using System.Text;
using VisualProfilerAccess.Metadata;

namespace VisualProfilerAccess
{
    public static class DeserializationExtensions
    {
        public static uint DeserializeUint32(this Stream byteStream)
        {
            byte[] buffer = new byte[sizeof(uint)];
            byteStream.Read(buffer, 0, buffer.Length);
            uint uInt32 = BitConverter.ToUInt32(buffer, 0);
            return uInt32;
        }

        public static string DeserializeString(this Stream byteStream)
        {
            uint stringLength = byteStream.DeserializeUint32();
            byte[] buffer = new byte[stringLength];
            byteStream.Read(buffer, 0, buffer.Length);
            string s = Encoding.Unicode.GetString(buffer);
            return s;
        }

        public static bool DeserializeBool(this Stream byteStream)
        {
            byte[] buffer = new byte[sizeof(bool)];
            byteStream.Read(buffer, 0, buffer.Length);
            bool b = BitConverter.ToBoolean(buffer, 0);
            return b;
        }

        public static MetadataTypes DeserializeMetadataType(this Stream stream)
        {
            uint uint32 = stream.DeserializeUint32();
            MetadataTypes messageTypes = (MetadataTypes) uint32;
            return messageTypes;
        }

        public static UInt64 DeserializeUInt64(this Stream byteStream)
        {
            byte[] buffer = new byte[sizeof(UInt64)];
            byteStream.Read(buffer, 0, buffer.Length);
            ulong uInt64 = BitConverter.ToUInt64(buffer, 0);
            return uInt64;
        }

        public static Actions DeserializeActions(this Stream byteStream)
        {
            byte[] buffer = new byte[sizeof(UInt32)];
            byteStream.Read(buffer, 0, buffer.Length);

            Actions action = (Actions)BitConverter.ToUInt32(buffer, 0);
            return action;
        }
    }
}