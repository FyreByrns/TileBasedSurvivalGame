using System;
using System.Collections.Generic;
namespace ProjectAndromeda.Serialization.StructuredFiles;

public class PaletteFile
: StructuredFile<PaletteFile> {
    byte[] rawColourData;
    [save] public byte[] RawColourData { get => rawColourData; set { rawColourData = value; } }

    public int Length => RawColourData.Length / 4;
    public RGBA[] Colours {
        get {
            RGBA[] colours = new RGBA[Length];

            for (int i = 0; i < Length; i++) {
                colours[i] = GetColour(i);
            }

            return colours;
        }
        set {
            rawColourData = new byte[value.Length * 4];

            foreach (RGBA colour in value) {
                AddColour(colour);
            }
        }
    }

    public RGBA GetColour(int index) {
        if (RawColourData.Length >= index * 4) {
            int i = index * 4;
            return new RGBA(
                    RawColourData[i],
                    RawColourData[i + 1],
                    RawColourData[i + 2],
                    RawColourData[i + 3]);
        }

        return default;
    }

    public void SetColour(int index, RGBA colour) {
        int i = index * 4;
        if (RawColourData.Length < i + 4) {
            Array.Resize(ref rawColourData, i + 4);
        }

        rawColourData[i] = colour.R;
        rawColourData[i + 1] = colour.G;
        rawColourData[i + 2] = colour.B;
        rawColourData[i + 3] = colour.A;
    }

    public void AddColour(RGBA colour) {
        SetColour((RawColourData.Length - 4) / 4 + 1, colour);
    }

    public PaletteFile(FileLocation location) : base(location) {
        RawColourData = Array.Empty<byte>();
    }
}

public class PaletteManifest
: StructuredFile<PaletteManifest> {
    [save] public Dictionary<string, int> NamesToIndices { get; set; }

    public RGBA GetColour(string name, PaletteFile source) {
        int i = NamesToIndices[name];
        if (i < source.Colours.Length) {
            return source.GetColour(i);
        }

        Logger.Log("palette is too small");
        return default;
    }

    public void AddColour(string name, int index, PaletteFile source) {
        NamesToIndices[name] = index;
    }

    public PaletteManifest(FileLocation location) : base(location) {
        NamesToIndices = new Dictionary<string, int>();
    }
}
