using System;
using System.Linq;
using System.Runtime.InteropServices;
using ModAPI.UI.CEF.Utility;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF.Extensions
{
    internal static class CefValueExtensions
    {
        #region Cef > V8
        
        public static CefV8Value ToV8Value(this CefValue value)
        {
            if (value == null || value.GetValueType() == CefValueType.Null)
                return CefV8Value.CreateNull();

            switch (value.GetValueType())
            {
                case CefValueType.Bool:
                    return CefV8Value.CreateBool(value.GetBool());
                case CefValueType.Double:
                    return CefV8Value.CreateDouble(value.GetDouble());
                case CefValueType.Int:
                    return CefV8Value.CreateInt(value.GetInt());
                case CefValueType.String:
                    return CefV8Value.CreateString(value.GetString());
                case CefValueType.Dictionary:
                {
                    var cefDict = value.GetDictionary();
                    var result = CefV8Value.CreateObject();

                    foreach (string key in cefDict.GetKeys())
                    {
                        result.SetValue(key, cefDict.GetValue(key).ToV8Value());
                    }

                    return result;
                }
                case CefValueType.List:
                {
                    var cefList = value.GetList();
                    var result = CefV8Value.CreateArray(cefList.Count);

                    for (int i = 0; i < cefList.Count; ++i)
                    {
                        result.SetValue(i, ToV8Value(cefList.GetValue(i)));
                    }

                    return result;
                }
                case CefValueType.Binary:
                {
                    var bytes = value.GetBinary().ToArray();
                    object binaryValue = BinaryUtility.Unpack(bytes, out var valueType);

                    switch (valueType)
                    {
                        case BinaryUtility.ValueType.ByteArray:
                        {
                            var container = new ArrayBufferContainer((byte[]) binaryValue);
                            var result = CefV8Value.CreateArrayBuffer(container.Data, (ulong) (bytes.Length - 1), container);
                            return result;
                        }
                        case BinaryUtility.ValueType.UInt32:
                        {
                            var result = CefV8Value.CreateUInt((uint) binaryValue);
                            return result;
                        }
                        case BinaryUtility.ValueType.Date:
                        {
                            var result = CefV8Value.CreateDate((DateTime) binaryValue);
                            return result;
                        }
                    }

                    throw new ArgumentException($"Unknown binary data. Header byte = {bytes[0]}");
                }
            }

            throw new ArgumentException($"Could not convert CefValue to CefV8Value, unknown type: {value.GetValueType()}");
        }
        
        #endregion
        
        #region V8 > Cef

        public static CefValue ToCefValue(this CefV8Value value)
        {
            if (!value.IsValid)
                throw new ArgumentException("Value is not valid.");
            
            if (value.IsFunction)
                throw new ArgumentException("Can't convert V8 functions to CefValue.");

            if (value.IsArrayBuffer)
                throw new ArgumentException("Can't convert V8 ArrayBuffer to CefValue.");

            CefValue result = CefValue.Create();

            if (value.IsBool)
            {
                result.SetBool(value.GetBoolValue());
                return result;
            }

            if (value.IsString)
            {
                result.SetString(value.GetStringValue());
                return result;
            }

            if (value.IsDouble)
            {
                result.SetDouble(value.GetDoubleValue());
                return result;
            }

            if (value.IsInt)
            {
                result.SetInt(value.GetIntValue());
                return result;
            }

            if (value.IsNull || value.IsUndefined)
            {
                result.SetNull();
                return result;
            }

            if (value.IsUInt)
            {
                result.SetBinary(CefBinaryValue.Create(BinaryUtility.Pack(value.GetUIntValue())));
                return result;
            }

            if (value.IsDate)
            {
                var binaryValue = CefBinaryValue.Create(BinaryUtility.Pack(value.GetDateValue()));
                result.SetBinary(binaryValue);
                return result;
            }

            if (value.IsObject)
            {
                var dictionary = CefDictionaryValue.Create();

                foreach (string key in value.GetKeys())
                {
                    CefValue cefValue = value.GetValue(key).ToCefValue();
                    dictionary.SetValue(key, cefValue);
                }

                result.SetDictionary(dictionary);
                return result;
            }

            if (value.IsArray)
            {
                var list = CefListValue.Create();

                for (int i = 0; i < value.GetArrayLength(); ++i)
                {
                    list.SetValue(i, value.GetValue(i).ToCefValue());
                }
                
                result.SetList(list);
            }

            // Known unhandled types:
            // - Binary (can't get bytes from none native array buffers)

            throw new ArgumentException($"Could not convert CefV8Value to CefValue, unknown value type: {value}");
        }
        
        #endregion

        private class ArrayBufferContainer : CefV8ArrayBufferReleaseCallback
        {
            public readonly IntPtr Data;
            private GCHandle handle;
            
            public ArrayBufferContainer(byte[] bytes)
            {
                handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                Data = handle.AddrOfPinnedObject();
            }
            
            protected override void ReleaseBuffer(IntPtr buffer)
            {
                handle.Free();
            }
        }
    }
}