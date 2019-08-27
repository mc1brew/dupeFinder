using System.Collections.Generic;
using System;
using System.Text;

namespace dupeFinder {
    public class FileDictionary
    {
        private Dictionary<string, List<string>> dictionary;

        public FileDictionary()
        {
            dictionary = new Dictionary<string, List<string>>();
        }

        public int Count {
            get {
                return dictionary.Count;
            }
        }

        public void Add(string hash, string path)
        {
            if(dictionary.ContainsKey(hash))
            {
                dictionary[hash].Add(path);
                Console.WriteLine("Duplicate file found.");
                var outputText = string.Join("], [", dictionary[hash]);
                Console.WriteLine($"[{hash}], [{outputText}]");
            }
            else{
                dictionary.Add(hash,new List<string>() {path});
            }
        }

        public string[] ToStringArray()
        { 
            List<string> stringList = new List<string>();

            foreach(var item in dictionary)
            {
                if(item.Value.Count>1)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append($"{item.Key}");
                    foreach(var path in item.Value)
                    {
                        sb.Append($",{path}");
                    }

                    stringList.Add(sb.ToString());
                }
            }

            return stringList.ToArray();
        }

    }
}