using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ProjectAndromeda.Serialization;

public static partial class TypeWrangler {
    public static class Serializer {
        public static Dictionary<string, Func<object, byte[]>> Serializers = new() {
            {
                BYTE,
                (o) => new[] { (byte)o }
            },
            {
                INT,
                (o) => BitConverter.GetBytes((int)o)
            },
            {
                STRING,
                (o) => {
                    string s = (string)o;
                    byte[] result = new byte[s.Length + 4];

                    // serialize length
                    byte[] len = Serialize(s.Length);
                    for (int i = 0; i < len.Length; i++) {
                        result[i] = len[i];
                    }

                    // serialize chars
                    for (int i = 0; i < s.Length; i++) {
                        result[i + 4] = (byte)s[i];
                    }
                    return result;
                }
            },
        };

        public static bool SerializerExistsFor(Type type) {
            return Serializers.ContainsKey(StandardNameOf(type));
        }

        public static byte[] Serialize<T>(T obj) {
            return Serialize(typeof(T), obj);
        }
        public static byte[] Serialize(Type type, object obj) {
            if (type.IsArray) {
                Array a = (Array)obj;
                List<byte> result = new();

                foreach (byte b in BitConverter.GetBytes(a.Length)) {
                    result.Add(b);
                }

                for (int i = 0; i < a.Length; i++) {
                    object o = a.GetValue(i);
                    foreach (byte b in Serialize(type.GetElementType(), o)) {
                        result.Add(b);
                    }
                }

                return result.ToArray();
            }
            else if (IsDictionary(type)) {
                Type[] kv = type.GetGenericArguments();
                IDictionary d = (IDictionary)obj;

                List<byte> result = new();

                foreach (byte b in BitConverter.GetBytes(d.Count)) {
                    result.Add(b);
                }

                foreach (object key in d.Keys) {
                    foreach (byte b in Serialize(kv[0], key)) {
                        result.Add(b);
                    }
                    foreach (byte b in Serialize(kv[1], d[key])) {
                        result.Add(b);
                    }
                }

                return result.ToArray();
            }
            else {
                if (SerializerExistsFor(type)) {
                    return Serializers[StandardNameOf(type)](obj);
                }
            }

            return null;
        }
    }

    public static class Deserializer {
        public static Dictionary<string, Func<byte[], int, object>> Deserializers = new() {
            {
                BYTE,
                (b, s) => {
                    return b[s];
                }
            },
            {
                INT,
                (b, s) => {
                    return BitConverter.ToInt32(b, s);
                }
            },
            {
                STRING,
                (b, s) => {
                    int len = Deserialize<int>(b, s);
                    char[] chars = new char[len];

                    for (int i = 0; i < len; i++) {
                        chars[i] = (char)b[s + i + 4];
                    }

                    return new string(chars);
                }
            }
        };

        public static bool DeserializerExistsFor(Type type) {
            return Deserializers.ContainsKey(StandardNameOf(type));
        }

        public static T Deserialize<T>(byte[] data, int start) {
            return (T)Deserialize(typeof(T), data, start);
        }
        public static object Deserialize(Type type, byte[] data, int start) {
            if (type.IsArray) {
                int len = Deserialize<int>(data, start);
                Array array = (Array)Activator.CreateInstance(type, new object[] { len });
                int location = 0;

                for (int i = 0; i < len; i++) {
                    object element = Deserialize(type.GetElementType(), data, start + location + 4);
                    location += Size(type.GetElementType(), element);
                    array.SetValue(element, i);
                }

                return array;
            }
            else if (IsDictionary(type)) {
                Type[] kv = type.GetGenericArguments();
                IDictionary d = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(kv));
                int len = Deserialize<int>(data, start);
                int location = 0;

                for (int i = 0; i < len; i++) {
                    object key = Deserialize(kv[0], data, start + location + 4);
                    location += Size(kv[0], key);
                    object value = Deserialize(kv[1], data, start + location + 4);
                    location += Size(kv[1], value);

                    d[key] = value;
                }

                return d;
            }
            else {
                if (DeserializerExistsFor(type)) {
                    try {
                        return Deserializers[StandardNameOf(type)](data, start);
                    }
                    catch (ArgumentOutOfRangeException) {
                        Logger.Log("not enough data to deserialize");
                    }
                }
            }

            return null;
        }
    }
}