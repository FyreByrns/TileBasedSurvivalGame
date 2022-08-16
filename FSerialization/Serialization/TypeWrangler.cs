using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ProjectAndromeda.Serialization;

public static partial class TypeWrangler {
    #region constant standard type names 

    public const string BOOL = "boolean";
    public const string BYTE = "byte";
    public const string SHORT = "short";
    public const string INT = "int32";
    public const string UINT = "uint32";
    public const string LONG = "int64";
    public const string ULONG = "ulong64";
    public const string FLOAT = "single";
    public const string DOUBLE = "double";
    public const string STRING = "string";
    public const string CHAR = "char";

    #endregion

    private class UnmanagedChecker<T> where T : unmanaged { }
    public static bool IsUnmanaged<T>() {
        return IsUnmanaged(typeof(T));
    }
    public static bool IsUnmanaged(Type type) {
        try {
            // if the type is not unmanaged, MakeGenericType will throw
            //   due to type restrictions on UnmanagedChecker
            typeof(UnmanagedChecker<>).MakeGenericType(type);
            return true;
        }
        catch {
            return false;
        }
    }

    public static string StandardNameOf<T>() {
        return StandardNameOf(typeof(T));
    }
    public static string StandardNameOf(Type type) {
        return type.Name.ToLower();
    }

    public static T As<T>(object obj) {
        try {
            return (T)obj;
        }
        catch {
            Logger.Log($"error casting to {StandardNameOf<T>()}");
        }
        return default(T);
    }

    private static MethodInfo MSizeOf { get; }
        = typeof(Marshal).GetMethod("SizeOf", Type.EmptyTypes);
    public static int Size<T>()
        where T : unmanaged {
        return Size<T>(default(T));
    }
    public static int Size<T>(T obj) {
        return Size(typeof(T), obj);
    }
    public static int Size(Type type, object obj) {
        if (type.IsArray) {
            Array array = (Array)obj;
            int runningTotal = 0;
            for (int i = 0; i < array.Length; i++) {
                runningTotal += Size(type.GetElementType(), array.GetValue(i));
            }
            return 4 + runningTotal;
        }
        else if (IsDictionary(type)) {
            Type[] kv = type.GetGenericArguments();
            IDictionary dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(kv));
            int runningTotal = 0;
            foreach (object key in dict.Keys) {
                runningTotal += Size(kv[0], key);
                runningTotal += Size(kv[1], dict[key]);
            }

            return 4 + runningTotal;
        }
        else {
            if (IsUnmanaged(type)) {
                return (int)MSizeOf.MakeGenericMethod(type).Invoke(null, null);
            }

            switch (StandardNameOf(type)) {
                case STRING: return As<string>(obj).Length + 4;

                default: return 0;
            }
        }
    }

    public static bool IsDictionary<T>() {
        return IsDictionary(typeof(T));
    }
    public static bool IsDictionary(Type type) {
        return type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}