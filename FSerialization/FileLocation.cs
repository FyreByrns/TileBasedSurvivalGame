using System.IO;

namespace FSerialization {
    public class FileLocation {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Location { get; set; }
        public string FullPath
            => $"{Directory.GetCurrentDirectory()}/{Location}/{Name}.{Extension}";
        public string DirectoryPath
            => $"{Directory.GetCurrentDirectory()}/{Location}";

        public FileLocation() { }
        public FileLocation(string location, string name, string extension) {
            Name = name;
            Extension = extension;
            Location = location;
        }
    }
}