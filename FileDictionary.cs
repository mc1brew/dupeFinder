using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

namespace dupeFinder {
    public class FileDictionary : System.Collections.Generic.Dictionary<string, List<string>>
    {
        public bool Add(string hash, string path)
        {
            bool returnValue = false;
            if(this.ContainsKey(hash))
            {
                if(!this[hash].Contains(path))
                {
                    this[hash].Add(path);
                    returnValue = true;
                }
            }
            else{
                this.Add(hash,new List<string>() {path});
            }

            return returnValue;
        }

        public string[] ToStringArray()
        { 
            List<string> stringList = new List<string>();

            foreach(var item in this)
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