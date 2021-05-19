using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography;

namespace DupeFinder{
    public class DupeFinder{
        public FileDictionary FindMatches(List<string> searchPaths, List<string> filters)
        {
            FileDictionary fileDictionary = new FileDictionary();
            List<string> filesStringList = new List<string>();

            //Get all files to be processed.
            foreach(var directory in searchPaths){
                filesStringList.AddRange(GetFilteredFiles(directory, filters));
            }

            //Perform preliminary file scan on the first kilobyte.
            fileDictionary = PreliminaryFileScan(filesStringList);

            //Process records that represent potential matches
            //TODO: Revisit this, why are you using async?
            fileDictionary = Task.Run(() => ScanAllFiles(fileDictionary)).Result;

            return fileDictionary;
        }

        ///Get the paths of the files to be scanned based on the path and designated filters.
        ///
        /// path: directory to look for files.
        /// filters: file types to look for.
        private static List<string> GetFilteredFiles(string path, List<string> filters)
        {
            List<string> filesStringList = new List<string>();
            //TODO: Make this list an external config file rather than hard coded.
            List<string> excludeDirectories = new List<string>()
            {
                "\\.tmp.drivedownload\\"
                , "\\.git\\"
                , "\\bin\\"
                , "\\obj\\"
                , "\\lib\\"
                , "\\src\\"
                , "\\*.7z.tmp"
            };

            foreach(string filter in filters)
            {
                filesStringList.AddRange(Directory.GetFiles(path, filter, SearchOption.AllDirectories));
                
                foreach(string excludedDirectory in excludeDirectories)
                {
                   int removedCount = filesStringList.RemoveAll(x => x.Contains(excludedDirectory));
                   if(filesStringList.Count == 0) break;
                }
            }

            return filesStringList;
        }
    
        public FileDictionary PreliminaryFileScan(List<string> filePaths)
        {
            //Get contents of directory;
            FileDictionary returnFileDictionary = new FileDictionary();
            OnFileScanBegin(true, filePaths.Count);

            foreach(var filePath in filePaths)
            {
                var firstKilobyteHash = ByteTool.GetKilobyteMd5Hash(filePath);
                
                try {
                    bool fileHasMatch = returnFileDictionary.Add(firstKilobyteHash, filePath);
                    OnFileProcessed(fileHasMatch);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            return returnFileDictionary;
        }
        
        /// Process all preliminary scans and do a full byte comparison to see if they are actual matches.
        /// 
        /// fileDictionary: A list of all possible matches.
        public async Task<FileDictionary> ScanAllFiles(FileDictionary fileDictionary)
        {
            var filesForFullScan = fileDictionary.Where(x => x.Value.Count > 1);
            FileDictionary returnFileDictionary = new FileDictionary();
            OnFileScanBegin(false, filesForFullScan.Count());

            foreach(var fileDictionaryItem in filesForFullScan)
            {
                string[] filePathArray = fileDictionaryItem.Value.ToArray();

                //Scrolling through the array of potential file matches to determine if there is a full match.
                //referencePathPosition is the the reference file for the comparison.
                //comparePathPosition is the file being compared.
                for(int referencePathPosition = 0; referencePathPosition < filePathArray.Length; referencePathPosition++)
                {
                    for(int comparePathPosition = referencePathPosition+1; comparePathPosition < filePathArray.Length; comparePathPosition++)
                    {
                        //If the files are a byte match then we get the full hash for the dictionary for accurate tracking.
                        //This is going to be a problem for really big files (over a couple of gigs).
                        //I'll have to figure that out later.
                        bool fileMatch = await ByteTool.CompareByteArray(filePathArray[referencePathPosition], filePathArray[comparePathPosition]);
                        if(fileMatch)
                        {
                            try {
                                returnFileDictionary.Add(fileDictionaryItem.Key, filePathArray[referencePathPosition]);
                                //This statement is an if because it's possible the file might have been added once during the comparions.
                                //Right now the comparison looks like this:
                                //| F1 | F2 | F3 | F4 |
                                // If F1 matches F2 and F3 on the first sweep then on the second sweep F2 will match F3.
                                //This means that it would appear to be another match, but really we already knew this.
                                //There is probably a more convenient way of dropping those out.  Probably I can just pop it out of the array.
                                bool fileHasMatch = returnFileDictionary.Add(fileDictionaryItem.Key, filePathArray[comparePathPosition]);
                                OnFileProcessed(fileHasMatch);
                            }
                            catch(Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }

            return returnFileDictionary;
        }

        private void OnFileProcessed(bool fileHasMatch)
        {
            EventHandler handler = FileProcessed;
            if(handler != null)
            {
                handler(this, new EventArgs());
            }
            if(fileHasMatch) OnDuplicateFound();
            else OnDuplicateNotFound();
        }
        private void OnDuplicateFound()
        {
            EventHandler handler = DuplicateFound;
            if(handler != null)
            {
                handler(this, new EventArgs());
            }
        }
        private void OnDuplicateNotFound()
        {
            EventHandler handler = DuplicateNotFound;
            if(handler != null)
            {
                handler(this, new EventArgs());
            }
        }
        private void OnFileScanBegin(bool IsPreliminaryScan, int NumberOfFilesToBeScanned)
        {
            OnScanBeginEventArgs eventArgs = new OnScanBeginEventArgs();
            eventArgs.FilesToBeScanned = NumberOfFilesToBeScanned;
            eventArgs.PreliminaryScan = IsPreliminaryScan;
            EventHandler<OnScanBeginEventArgs> handler = ScanBegins;
            if(handler != null)
            {
                handler(this, eventArgs);
            }
        }
        public event EventHandler DuplicateFound;
        public event EventHandler DuplicateNotFound;
        public event EventHandler FileProcessed;
        public event EventHandler<OnScanBeginEventArgs> ScanBegins;
    }

    public class OnScanBeginEventArgs : EventArgs
    {
        public bool PreliminaryScan {get;set;}
        public int FilesToBeScanned {get;set;}
    }
}
