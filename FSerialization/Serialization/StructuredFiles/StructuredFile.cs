using ProjectAndromeda.Serialization;
using System;
using System.Reflection;

using static ProjectAndromeda.Serialization.TypeWrangler;

namespace ProjectAndromeda.Serialization.StructuredFiles;

#pragma warning disable IDE1006 // Naming Styles
[AttributeUsage(AttributeTargets.Property)]
public class save : Attribute { }
#pragma warning restore IDE1006 // Naming Styles
public abstract class StructuredFile {
    protected File file;

    protected int currentLocation;

    protected void BeginWrite(int startingLocation) {
        currentLocation = startingLocation;
    }
    protected void Write(byte[] data) {
        file.ExpandToRequestedLocation = true;
        file.WriteBytes(currentLocation, data);
        currentLocation += data.Length;
    }

    protected void BeginRead(int startingLocation) {
        currentLocation = startingLocation;
    }
    protected T Read<T>(byte[] data) {
        return (T)Read(typeof(T), data);
    }
    protected object Read(Type type, byte[] data) {
        object result = Deserializer.Deserialize(type, data, currentLocation);
        currentLocation += Size(type, result);
        return result;
    }

    public virtual bool Save() { return false; }
    public virtual StructuredFile Load() { return null; }

    protected StructuredFile(FileLocation location) {
        file = new(location, 0);
    }
}

public abstract class StructuredFile<T>
    : StructuredFile
    where T : StructuredFile {

    public new virtual T Load() { return default; }

    public virtual void SaveAllProperties() {
        PropertyInfo[] properties = typeof(T).GetProperties();

        BeginWrite(0);
        foreach (PropertyInfo property in properties) {
            if (!Attribute.IsDefined(property, typeof(save))) {
                continue;
            }

            Write(Serializer.Serialize(property.PropertyType, property.GetValue(this)));
        }

        FileManager.Write(file, true);
    }
    public virtual T LoadAllProperties() {
        FileManager.Read(file.Location, out file);
        T result = (T)Activator.CreateInstance(typeof(T), file.Location);
        ((StructuredFile<T>)(object)result).file.Data = file.Data;
        PropertyInfo[] properties = typeof(T).GetProperties();

        BeginRead(0);
        foreach (PropertyInfo property in properties) {
            if (!Attribute.IsDefined(property, typeof(save))) {
                continue;
            }

            property.SetValue(result, Read(property.PropertyType, file.Data));
        }

        return result;
    }

    protected StructuredFile(FileLocation location) : base(location) { }
}
