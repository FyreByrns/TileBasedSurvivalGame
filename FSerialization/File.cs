using System;

namespace FSerialization {
    public class File {
        public FileLocation Location { get; set; }
        public bool ExpandToRequestedLocation { get; set; }
            = false;
        public byte[] Data;

        bool ExpandTo(int length) {
            if (length >= Data.Length) {
                if (ExpandToRequestedLocation) {
                    Array.Resize(ref Data, length);
                    return true;
                }

                return false;
            }

            return false;
        }

        public bool WriteByte(int location, byte b) {
            if (!ExpandTo(location)) {
                return false;
            }

            Data[location] = b;
            return true;
        }
        public bool WriteBytes(int location, params byte[] bytes) {
            if (!ExpandTo(location + bytes.Length)) {
                return false;
            }

            for (int i = 0; i < bytes.Length; i++) {
                Data[i + location] = bytes[i];
            }
            return true;
        }

        public bool WriteInt(int location, int value) {
            if (!ExpandTo(location + 4)) {
                return false;
            }

            byte[] intBytes = BitConverter.GetBytes(value);
            for (int i = 0; i < intBytes.Length; i++) {
                Data[i + location] = intBytes[i];
            }
            return true;
        }

        public bool WriteString(int location, string str) {
            if (!ExpandTo(location + str.Length + 4)) {
                return false;
            }

            if (!WriteInt(location, str.Length)) {
                return false;
            }

            for (int i = 0; i < str.Length; i++) {
                if (!WriteByte(location + 4 + i, (byte)str[i])) {
                    return false;
                }
            }

            return true;
        }

        public File() {
            Data = new byte[0];
            ExpandToRequestedLocation = true;
        }
        public File(FileLocation location, int startingSize) {
            Location = location;
            Data = new byte[startingSize];
        }
        public File(FileLocation location, byte[] data) {
            Location = location;
            Data = data;
        }
    }
}