using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModAPI.UI.CEF.Extensions;
using ModAPI.UI.CEF.Utility;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF.Conversion
{
    internal static class CefConvert
    {
        private class TypeCache
        {
            public readonly Dictionary<string, PropertyInfo> Properties = new Dictionary<string, PropertyInfo>();
        }

        private static readonly Dictionary<Type, TypeCache> TypeCaches = new Dictionary<Type, TypeCache>();
        
        public static CefValue ConvertObject(object obj)
        {
            CefValue value = CefValue.Create();

            if (obj == null)
            {
                value.SetNull();
                return value;
            }

            if (obj is int intValue)
            {
                value.SetInt(intValue);
                return value;
            }

            if (obj is short shortValue)
            {
                value.SetInt(shortValue);
                return value;
            }

            if (obj is ushort ushortValue)
            {
                value.SetInt(ushortValue);
                return value;
            }

            if (obj is byte byteValue)
            {
                value.SetInt(byteValue);
                return value;
            }

            if (obj is string stringValue)
            {
                value.SetString(stringValue);
                return value;
            }

            if (obj is long || obj is float || obj is double)
            {
                value.SetDouble((double) obj);
                return value;
            }

            if (obj is uint uintValue)
            {
                var binaryValue = CefBinaryValue.Create(BinaryUtility.Pack(uintValue));
                value.SetBinary(binaryValue);
                return value;
            }

            if (obj is DateTime dateTimeValue)
            {
                var binaryValue = CefBinaryValue.Create(BinaryUtility.Pack(dateTimeValue));
                value.SetBinary(binaryValue);
                return value;
            }

            if (obj is byte[] byteArrayValue)
            {
                var binaryValue = CefBinaryValue.Create(BinaryUtility.Pack(byteArrayValue));
                value.SetBinary(binaryValue);
                return value;
            }
            
            var type = obj.GetType();

            if (type.IsEnum)
            {
                value.SetInt((int) obj);
                return value;
            }

            CefDictionaryValue dictionaryValue = CefDictionaryValue.Create();

            foreach (PropertyInfo propertyInfo in GetProperties(type))
            {
                dictionaryValue.SetValue(propertyInfo.Name.ToCamelCase(), ConvertObject(propertyInfo.GetValue(obj, null)));
            }

            value.SetDictionary(dictionaryValue);
            return value;
        }
        
        public static object ConvertValue(CefValue value, Type targetType)
        {
            if (targetType.IsAbstract)
                throw new ArgumentException("Can not convert to an abstract type.");
            
            var valueType = value.GetValueType();
            
            switch (valueType)
            {
                case CefValueType.Null:
                    return targetType.IsClass ? null : Activator.CreateInstance(targetType);
                case CefValueType.Bool:
                    return TryChangeType(value, value.GetBool(), targetType);
                case CefValueType.Double:
                    return TryChangeType(value, value.GetDouble(), targetType);
                case CefValueType.Int:
                    return TryChangeType(value, value.GetInt(), targetType);
                case CefValueType.String:
                    return TryChangeType(value, value.GetString(), targetType);
                case CefValueType.Binary:
                {
                    var objValue = BinaryUtility.Unpack(value.GetBinary().ToArray(), out var objValueType);

                    switch (objValueType)
                    {
                        case BinaryUtility.ValueType.Date:
                            return TryChangeType(value, objValueType, targetType);
                        case BinaryUtility.ValueType.UInt32:
                            return TryChangeType(value, objValue, targetType);
                        case BinaryUtility.ValueType.ByteArray:
                            return TryChangeType(value, (byte[]) objValue, targetType);
                        default: throw new NotImplementedException();
                    }
                }
                case CefValueType.List:
                {
                    if (targetType.IsArray)
                        throw new ArgumentException("Non-byte arrays are not supported. Use List<T> instead.");

                    if (targetType.GetGenericTypeDefinition() != typeof(List<>))
                        throw new ArgumentException("CefValue type is List but the native type is not List<T>. Only List<T> is supported for non byte collections.");
                    
                    var cefList = value.GetList();
                    var listType = typeof(List<>);
                    var elementType = targetType.GetElementType();
                    var objListType = listType.MakeGenericType(elementType);
                    var result = (IList) Activator.CreateInstance(objListType);

                    for (int i = 0; i < cefList.Count; ++i)
                    {
                        var managedValue = ConvertValue(cefList.GetValue(i), elementType);
                        result.Add(managedValue);
                    }

                    return result;
                }
                case CefValueType.Dictionary:
                {
                    var cefDict = value.GetDictionary();
                    object result;

                    try
                    {
                        result = Activator.CreateInstance(targetType, nonPublic: true);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Could not convert CefValue (type={value.GetValueType()}) to {targetType.Name}.", ex);
                    }

                    foreach (string key in cefDict.GetKeys())
                    {
                        PropertyInfo propertyInfo = GetProperty(targetType, key);

                        if (propertyInfo == null || !propertyInfo.CanWrite)
                            continue;

                        var managedValue = ConvertValue(cefDict.GetValue(key), propertyInfo.PropertyType);
                        propertyInfo.SetValue(result, managedValue, null);
                    }
                    
                    return result;
                }
            }
            
            throw new NotImplementedException();
        }

        private static object TryChangeType(CefValue cefValue, object value, Type targetType)
        {
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Could not convert CefValue (type={cefValue.GetValueType()}) to {targetType.Name}: {ex.Message}", ex);
            }
        }

        public static T ConvertValue<T>(CefValue value) => (T) ConvertValue(value, typeof(T));

        private static PropertyInfo GetProperty(Type targetType, string propertyName)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            
            if (!TypeCaches.TryGetValue(targetType, out var typeCache))
            {
                typeCache = new TypeCache();
                TypeCaches[targetType] = typeCache;
            }

            if (!typeCache.Properties.TryGetValue(propertyName, out var propertyInfo))
            {
                propertyInfo = targetType.GetProperty(propertyName, GetPropertyBindingFlags());

                if (propertyInfo == null)
                    return null;

                typeCache.Properties[propertyName] = propertyInfo;
            }

            return propertyInfo;
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            if (!TypeCaches.TryGetValue(targetType, out var typeCache))
            {
                typeCache = new TypeCache();
                TypeCaches[targetType] = typeCache;
            }

            foreach (var propertyInfo in targetType.GetProperties(GetPropertyBindingFlags()))
            {
                typeCache.Properties[propertyInfo.Name] = propertyInfo;
                yield return propertyInfo;
            }
        }

        private static BindingFlags GetPropertyBindingFlags() => BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
    }
}