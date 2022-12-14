using System;
using System.Reflection;
using System.Collections.Generic;

using static ProjectAndromeda.Serialization.TypeWrangler;
using ProjectAndromeda;

namespace ProjectAndromeda.Serialization {
    static class ByteArrayExtensions {
        // store read index for each byte array
        static Dictionary<int, int> perArrayIndices = new Dictionary<int, int>();

        public static T Get<T>(this byte[] me) {
            int arrayID = me.GetHashCode();
            if (!perArrayIndices.ContainsKey(arrayID)) {
                perArrayIndices[arrayID] = 0;
            }

            int readIndex = perArrayIndices[arrayID];
            Logger.Log($"{typeof(T).Name} at {readIndex} in {arrayID}:", ConsoleColor.DarkYellow);
            T result = default;

            if (typeof(T).IsArray) { // if array deserializing
                int len = me.Get<int>();
                object array = Activator.CreateInstance(typeof(T), new object[] { len }); // create array
                for (int i = 0; i < len; i++) { // read values into array
                    MethodInfo info = typeof(ByteArrayExtensions).GetMethod("Get").MakeGenericMethod(array.GetType().GetElementType());
                    ((Array)array).SetValue(info.Invoke(null, new object[] { me }), i);
                }

                result = (T)array;
            }
            else { // otherwise, just read the value
                switch (StandardNameOf<T>()) {
                    case BYTE: {
                            result = (T)(object)me[readIndex];
                            readIndex++;
                            break;
                        }
                    case BOOL: {
                            result = (T)(object)(me[readIndex] == 1);
                            readIndex++;
                            break;
                        }
                    case CHAR: {
                            result = (T)(object)(char)me[readIndex];
                            readIndex++;
                            break;
                        }

                    case INT: {
                            result = (T)(object)BitConverter.ToInt32(me, readIndex);
                            readIndex += 4;
                            break;
                        }
                    case UINT: {
                            result = (T)(object)BitConverter.ToUInt32(me, readIndex);
                            readIndex += 4;
                            break;
                        }
                    case LONG: {
                            result = (T)(object)BitConverter.ToInt64(me, readIndex);
                            readIndex += 8;
                            break;
                        }
                    case FLOAT: {
                            result = (T)(object)BitConverter.ToSingle(me, readIndex);
                            readIndex += 4;
                            break;
                        }

                    case STRING: {
                            result = (T)(object)new string(me.Get<char[]>());
                            break;
                        }
                }
            }
            perArrayIndices[arrayID] = readIndex;

            Logger.Log($"\t{result}", ConsoleColor.DarkYellow);
            return result;
        }

        public static void AddBytes(this List<byte> me, params byte[] bytes) {
            me.AddRange(bytes);
        }
        public static void Append<T>(this List<byte> me, T item) {
            int arrayID = me.GetHashCode();
            if (!perArrayIndices.ContainsKey(arrayID)) {
                perArrayIndices[arrayID] = 0;
            }
            int writeIndex = perArrayIndices[arrayID];

            object o = item; // for cleaner casting
            if (typeof(T).IsArray) {
                Array array = (Array)o;
                me.Append(array.Length);

                for (int i = 0; i < array.Length; i++) {
                    me.Append(array.GetValue(i));
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
                            me.AddBytes(me[writeIndex] = (byte)(char)o);
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

            perArrayIndices[arrayID] = writeIndex;
        }

        public static byte[] DoubleSize(this byte[] me) {
            Array.Resize(ref me, me.Length * 2);
            return me;
        }

        public static void Done(this byte[] me) {
            int arrayID = me.GetHashCode();
            if (perArrayIndices.ContainsKey(arrayID)) {
                perArrayIndices.Remove(arrayID);
            }
        }
    }
}
