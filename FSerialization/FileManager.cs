using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using SFile = System.IO.File;

namespace FSerialization {
    public static class FileManager {
        public static bool Write(File file, bool overwrite = true) {
            if (SFile.Exists(file.Location.FullPath)) {
                if (!overwrite) {
                    return false;
                }

                SFile.Delete(file.Location.FullPath);
            }

            Directory.CreateDirectory(file.Location.DirectoryPath);
            SFile.WriteAllBytes(file.Location.FullPath, file.Data);
            return true;
        }

        public static bool Read(FileLocation location, out File file, bool createIfNonexistant = false) {
            if (SFile.Exists(location.FullPath)) {
                byte[] data = SFile.ReadAllBytes(location.FullPath);

                file = new File(location, data);
                return true;
            }
            else if (createIfNonexistant) {
                SFile.Create(location.FullPath);
                return Read(location, out file);
            }

            file = null;
            return false;
        }
    }
}