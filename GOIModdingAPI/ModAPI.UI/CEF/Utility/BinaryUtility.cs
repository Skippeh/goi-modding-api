using System;

namespace ModAPI.UI.CEF.Utility
{
    internal static class BinaryUtility
    {
        public enum ValueType
        {
            ByteArray,
            Date,
            UInt32
        }
        
        public static byte[] Pack(DateTime dateTime)
        {
            var ticks = (ulong) dateTime.Ticks;
            var result = new byte[9];
            result[0] = (byte) ValueType.Date;
            result[1] = (byte) (ticks >> 56);
            result[2] = (byte) (ticks >> 48 & 0xff);
            result[3] = (byte) (ticks >> 40 & 0xff);
            result[4] = (byte) (ticks >> 32 & 0xff);
            result[5] = (byte) (ticks >> 24 & 0xff);
            result[6] = (byte) (ticks >> 16 & 0xff);
            result[7] = (byte) (ticks >> 8 & 0xff);
            result[8] = (byte) (ticks & 0xff);
            
            return result;
        }

        public static byte[] Pack(uint value)
        {
            var result = new byte[5];
            result[0] = (byte) ValueType.UInt32;
            result[1] = (byte) (value >> 24);
            result[2] = (byte) (value >> 16 & 0xff);
            result[3] = (byte) (value >> 8 & 0xff);
            result[4] = (byte) (value & 0xff);

            return result;
        }

        public static byte[] Pack(byte[] bytes)
        {
            var result = new byte[bytes.Length + 1];
            result[0] = (byte) ValueType.ByteArray;

            for (int i = 0; i < bytes.Length; ++i)
            {
                result[i + 1] = bytes[i];
            }

            return result;
        }

        public static object Unpack(byte[] bytes, out ValueType valueType)
        {
            switch ((ValueType)bytes[0])
            {
                case ValueType.ByteArray:
                {
                    byte[] result = new byte[bytes.Length - 1];
                    
                    for (int i = 0; i < result.Length; ++i)
                    {
                        result[i] = bytes[i + 1];
                    }

                    valueType = ValueType.ByteArray;
                    return result;
                }
                case ValueType.UInt32:
                {
                    uint result = (uint) bytes[1] << 24;
                    result |= (uint) bytes[2] << 16;
                    result |= (uint) bytes[3] << 8;
                    result |= bytes[4];

                    valueType = ValueType.UInt32;
                    return result;
                }
                case ValueType.Date:
                {
                    ulong ticks = (ulong) bytes[1] << 56;
                    ticks |= (ulong) bytes[2] << 48;
                    ticks |= (ulong) bytes[3] << 40;
                    ticks |= (ulong) bytes[4] << 32;
                    ticks |= (ulong) bytes[5] << 24;
                    ticks |= (ulong) bytes[6] << 16;
                    ticks |= (ulong) bytes[7] << 8;
                    ticks |= bytes[8];

                    valueType = ValueType.Date;
                    DateTime result = new DateTime((long) ticks);
                    return result;
                }
            }

            throw new NotImplementedException();
        }
    }
}