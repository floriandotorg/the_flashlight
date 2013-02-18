using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace utility
{
    static class IsolatedStorage
    {
        static private void DoListFiles(this IsolatedStorageFile storage, string path, int layer)
        {
            foreach (var folder in storage.GetDirectoryNames(Path.Combine(path, "*.*")))
            {
                System.Diagnostics.Debug.WriteLine(new String('-', layer) + folder);
                storage.DoListFiles(Path.Combine(path, folder), layer + 1);
            }

            foreach (var file in storage.GetFileNames(Path.Combine(path, "*.*")))
            {
                System.Diagnostics.Debug.WriteLine(new String('-', layer) + file);
            }
        }

        static public void ListFiles(this IsolatedStorageFile storage, string path = "")
        {
            System.Diagnostics.Debug.WriteLine("Files in IsolatedStorage:");
            storage.DoListFiles(path, 0);
        }

        static public void DeletePath(this IsolatedStorageFile storage, string path)
        {
            // Iterate through the subfolders and check for further subfolders            
            foreach (var folder in storage.GetDirectoryNames(Path.Combine(path, "*.*")))
            {
                storage.DeletePath(Path.Combine(path, folder));
            }

            // Delete all files at the root level in that folder.
            foreach (var file in storage.GetFileNames(Path.Combine(path, "*.*")))
            {
                string f = Path.Combine(path, file);
                storage.DeleteFile(f);
            }

            if (path != "")
            {
                // Finally delete the path
                storage.DeleteDirectory(path);
            }
        }
    }
}
