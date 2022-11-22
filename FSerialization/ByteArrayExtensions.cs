using System;
using System.Reflection;
using System.Collections.Generic;

using static FSerialization.TypeWrangler;

namespace FSerialization {
    public static class ByteArrayExtensions {
        public static T Get<T>(this byte[] me, ref int index) {
            T result = default;

            if (typeof(T).IsArray) { // if array deserializing
                int len = me.Get<int>(ref index);
                object array = Activator.CreateInstance(typeof(T), new object[] { len }); // create array
                for (int i = 0; i < len; i++) { // read values into array
                    MethodInfo reflectionGetT
                        = typeof(ByteArrayExtensions).GetMethod("Get").MakeGenericMethod(array.GetType().GetElementType());
                    object[] arguments = { me, index };
                    ((Array)array).SetValue(reflectionGetT.Invoke(null, arguments), i);
                    // retrieve ref
                    index = (int)arguments[1];
                }

                result = (T)array;
            }
            else { // otherwise, just read the value
                switch (StandardNameOf<T>()) {
                    case BYTE: {
                            result = (T)(object)me[index];
                            index++;
                            break;
                        }
                    case BOOL: {
                            result = (T)(object)(me[index] == 1);
                            index++;
                            break;
                        }
                    case CHAR: {
                            result = (T)(object)(char)me[index];
                            index++;
                            break;
                        }

                    case INT: {
                            result = (T)(object)BitConverter.ToInt32(me, index);
                            index += 4;
                            break;
                        }
                    case UINT: {
                            result = (T)(object)BitConverter.ToUInt32(me, index);
                            index += 4;
                            break;
                        }
                    case LONG: {
                            result = (T)(object)BitConverter.ToInt64(me, index);
                            index += 8;
                            break;
                        }
                    case FLOAT: {
                            result = (T)(object)BitConverter.ToSingle(me, index);
                            index += 4;
                            break;
                        }

                    case STRING: {
                            result = (T)(object)new string(me.Get<char[]>(ref index));
                            break;
                        }
                }
            }

            return result;
        }

        public static void AddBytes(this List<byte> me, params byte[] bytes) {
            me.AddRange(bytes);
        }
        public static void Append<T>(this List<byte> me, T item) {
            object o = item; // for cleaner casting
            if (typeof(T).IsArray) {
                Array array = (Array)o;
                me.Append(array.Length);

                for (int i = 0; i < array.Length; i++) {
                    //me.Append(array.GetValue(i));
                    me.AppendByStringTypeName(array.GetValue(i), typeof(T).Name.TrimEnd("[]".ToCharArray()).ToLower());
                }
            }
            else {
                switch (StandardNameOf<T>()) {
                    case BYTE: {
                            me.AddBytes((byte)o);
                            break;
                        }
                    case BOOL: {
                            me.AddBytes((bool)o ? (byte)1 : (byte)0);
                            break;
                        }
                    case CHAR: {
                            me.AddBytes(/*me[writeIndex] = */(byte)(char)o);
                            break;
                        }

                    case INT: {
                            me.AddBytes(BitConverter.GetBytes((int)o));
                            break;
                        }
                    case UINT: {
                            me.AddBytes(BitConverter.GetBytes((uint)o));
                            break;
                        }
                    case LONG: {
                            me.AddBytes(BitConverter.GetBytes((long)o));
                            break;
                        }

                    case FLOAT: {
                            me.AddBytes(BitConverter.GetBytes((float)o));
                            break;
                        }

                    case STRING: {
                            me.Append(((string)o).ToCharArray());
                            break;
                        }
                }
            }
        }
        private static void AppendByStringTypeName(this List<byte> me, object item, string standardName) {
            object o = item; // for cleaner casting
            switch (standardName) {
                case BYTE: {
                        me.Append<byte>((byte)o);
                        break;
                    }
                case CHAR: {
                        me.Append<char>((char)o);
                        break;
                    }
                case INT: {
                        me.Append<int>((int)o);
                        break;
                    }
            }
        }

        public static byte[] DoubleSize(this byte[] me) {
            Array.Resize(ref me, me.Length * 2);
            return me;
        }
    }
}
