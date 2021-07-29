using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ExploringDirectories
{

    public static class FileSystemUtility
    {
        public static string GetDosAttributes(this System.IO.FileSystemInfo file)
        {
            string outputAttributes = string.Empty;

            if (((File.GetAttributes(file.FullName) & FileAttributes.ReadOnly)
                == FileAttributes.ReadOnly))
                outputAttributes += "r";
            else
                outputAttributes += "-";

            if (((File.GetAttributes(file.FullName) & FileAttributes.Hidden)
                == FileAttributes.Hidden))
                outputAttributes += "h";
            else
                outputAttributes += "-";

            if (((File.GetAttributes(file.FullName) & FileAttributes.ReadOnly)
                == FileAttributes.ReadOnly))
                outputAttributes += "a";
            else
                outputAttributes += "-";

            if (((File.GetAttributes(file.FullName) & FileAttributes.System)
                == FileAttributes.System))
                outputAttributes += "s";
            else
                outputAttributes += "-";

            return outputAttributes;
        }

        public static string FindFirstCreationDate(this System.IO.FileSystemInfo directory)
        {
            DateTime firstDate = DateTime.Now;
            firstDate = FindFirstDate(directory.FullName, firstDate);

            if (firstDate < DateTime.Now)
                return $"The oldest file is here since: {firstDate}";
            else
                return $"There are no files here!";
        }

        private static DateTime FindFirstDate(string path, DateTime firstDate)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(path);
            FileInfo[] files = currentDirectory.GetFiles();
            DirectoryInfo[] directories = currentDirectory.GetDirectories();

            foreach (FileInfo file in files)
            {
                if (file.CreationTime < firstDate)
                    firstDate = file.CreationTime;
            }

            foreach (DirectoryInfo dir in directories)
            {
                FindFirstDate(dir.FullName, firstDate);
            }

            return firstDate;
        }
    }

    public class FileSystemComparator : IComparer<string>
    {
        public int Compare(string s1, string s2)
        {
            if (s1.Length < s2.Length)
                return -1;
            if (s1.Length > s2.Length)
                return 1;

            return s1.CompareTo(s2);
        }
    }

    public class Program
    {
        static void PrintDirectory(string path, int depth)
        {
            if (!Directory.Exists(path))
                return;

            string leftMargin = string.Empty;
            for (int i = 0; i < depth; i++)
            {
                leftMargin += "   ";
            }

            foreach (string f in Directory.GetFiles(path))
            {
                string outputInfo = leftMargin + Path.GetFileName(f);
                FileInfo file = new FileInfo(f);
                outputInfo += " " + file.Length + " bytes";
                outputInfo += " " + file.GetDosAttributes();
                Console.WriteLine(outputInfo);
            }

            foreach (string c in Directory.GetDirectories(path))
            {
                FileInfo catalog = new FileInfo(c);
                DirectoryInfo directory = new DirectoryInfo(c);

                string outputInfo = leftMargin + Path.GetFileName(c);
                int size = directory.GetDirectories().Length + directory.GetFiles().Length;
                outputInfo += " (" + size + ")";
                outputInfo += " " + catalog.GetDosAttributes();
                Console.WriteLine(outputInfo);

                PrintDirectory(c, depth + 1);
            }
        }

        static void CreateCollection(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);

            SortedDictionary<string, long> collection = 
                new SortedDictionary<string, long>(new FileSystemComparator());

            foreach (string f in Directory.GetFiles(path))
            {
                FileInfo file = new FileInfo(f);
                collection.Add(Path.GetFileName(f), file.Length);
            }

            foreach (string cat in Directory.GetDirectories(path))
            {
                DirectoryInfo directory = new DirectoryInfo(cat);
                long size = directory.GetDirectories().Length + directory.GetFiles().Length;
                collection.Add(Path.GetDirectoryName(cat), size);
            }

            try
            {
                using (FileStream fs = new FileStream("data.dat", FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, collection);
                }   
            } 
            catch(SerializationException e)
            {
                Console.WriteLine($"Failed to serialize: {e.Message}");
            }

            DeserializeCollection();
        }

        static void DeserializeCollection()
        {
            SortedDictionary<string, long> collection = null;

            try
            {
                using (FileStream fs = new FileStream("data.dat", FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    collection = (SortedDictionary<string, long>)formatter.Deserialize(fs);
                }
            }
            catch (SerializationException e)
            {
                Console.WriteLine($"Failed to deserialize: {e.Message}");
            }

            foreach (KeyValuePair<string,long> file in collection)
            {
                Console.WriteLine($"{file.Key} -> {file.Value}");
            }
        }


        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                string path = args[0];
                string outputInfo = Path.GetFileName(path);

                FileInfo catalog = new FileInfo(path);
                DirectoryInfo directory = new DirectoryInfo(path);

                int size = directory.GetDirectories().Length + directory.GetFiles().Length;
                outputInfo += " (" + size + ")";
                outputInfo += " " + catalog.GetDosAttributes();

                Console.WriteLine(outputInfo);

                PrintDirectory(path, 1);
            }

            Console.Read();
        }
    }
}
