using System;
using System.IO;
using System.Security.Cryptography;

namespace dupeFinder{
    public class ByteTool {

        public static string GetHundredByteMd5Hash(string path)
        {
            string returnMd5Hash;
            byte[] firstHundredBytes = GetHundredByteArray(path, 0);

            using(MD5 crypt = MD5.Create())
            {
                byte[] byteArrayHash = crypt.ComputeHash(firstHundredBytes);
                returnMd5Hash = BitConverter.ToString(byteArrayHash).Replace("-", "").ToLower().ToLowerInvariant();
            }

            return returnMd5Hash;
        }

        public static bool CompareByteArray(string path_A, string path_B)
        {
            FileInfo fileInfo_A = new FileInfo(path_A);
            FileInfo fileInfo_B = new FileInfo(path_B);

            if(fileInfo_A.Length == fileInfo_B.Length)
            {
                using(FileStream fileStream_A = new FileStream(path_A, FileMode.Open))
                using(FileStream fileStream_B = new FileStream(path_B, FileMode.Open))
                {
                    int byteCounter = 0;
                    while(byteCounter < fileInfo_A.Length && fileStream_A.ReadByte() == fileStream_B.ReadByte())
                    {
                        byteCounter++;
                    }
                    if(byteCounter == fileInfo_A.Length) return true;
                }
            }

            return false;
        }


        private static byte[] GetHundredByteArray(string path, int offset)
        {
            byte[] returnArray = new byte[1024];
            using(FileStream fs = new FileStream(path,FileMode.Open))
            {
                int bytesRead = fs.Read(returnArray, offset, 1024);
            }

            return returnArray;
        }
    }
}