using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Security.Cryptography;

namespace fileHash
{
    class Program
    {
        static void Main(string[] args)
        {            
            //Check to ensure arguements are good.
            if(args[0] == null)
            {
                Console.WriteLine("Variable: 'args[0]' is null");
                return;
            }

            if(args[1] == null)
            {
                Console.WriteLine("Variable: 'args[1]' is null");
                return;
            }

            if(!Directory.Exists(args[0]))
            {
                Console.WriteLine($"Path does not exist: {args[0].ToString()}");
                return;
            }

            if(!Directory.Exists(args[1]))
            {
                Console.WriteLine($"Path does not exist: {args[1].ToString()}");
                return;
            }

            string filters = string.Empty;
            if(args.Length == 3) filters = args[2];

            FileDictionary fileDictionary = new FileDictionary();
            List<string> directories = new List<string>();

            string outputPath = "C:\\Users\\kwright\\src\\fileHash\\output\\output.csv";

            directories.Add(args[0].ToString());
            directories.Add(args[1].ToString());

            foreach(var directory in directories){
                fileDictionary = ScanAllFiles(fileDictionary, directory, filters);
            }

            File.AppendAllLines(outputPath, fileDictionary.ToStringArray());

            //Check for subdirectories


            //Hash File
            //Store In Redis
            //Compare Hashes
        }

        public static bool CompareByteArrays(byte[] array1, byte[] array2){
           
            bool bEqual = false;
            if (array1 == null || array2 == null) return bEqual;

            if (array1.Length == array2.Length)
            {
                int i=0;
                while ((i < array1.Length) && (array1[i] == array2[i]))
                {
                    i += 1;
                }
                if (i == array1.Length) 
                {
                    bEqual = true;
                }
            }

            return bEqual;
        }

        public static FileDictionary ScanAllFiles(FileDictionary fileDictionary, string path, string filters)
        {
            //Get contents of directory;
            var files = GetFilteredFiles(path, filters);

            foreach(var file in files)
            {
                var filePath = file;
                var byteArrayOfFile = File.ReadAllBytes(filePath);

                using (MD5 crypt = MD5.Create())
                {
                    var fileHash = crypt.ComputeHash(byteArrayOfFile);
                    var fileHashString = BitConverter.ToString(fileHash).Replace("-", "").ToLower().ToLowerInvariant();

                    try {
                        fileDictionary.Add(fileHashString, filePath);
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            return fileDictionary;
        }

        public static string[] GetFilteredFiles(string path, string filters)
        {
            string[] filtersArray = filters.Split(',');
            List<string> filesArray = new List<string>();

            foreach(string filter in filtersArray)
            {
                filesArray.AddRange(Directory.GetFiles(path, filter, SearchOption.AllDirectories));
            }

            return filesArray.ToArray();
        }
    }
}
