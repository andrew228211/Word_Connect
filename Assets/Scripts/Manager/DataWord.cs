using System;
using System.Collections.Generic;
[Serializable]
public class DataWord
{
        public int x;
        public int y;
        public string direction; //V: Doc, H la ngang
        public string word;
}

    [Serializable]
    public class WordWrapper
    {
        public int levelId;
        public int width;
        public int height;
        public string letters; // circle letters
        public DataWord[] words;
        public List<string> extra;

        public WordWrapper()
        {
            
        }
        public WordWrapper(int levelId, int width, int height, string letters, DataWord[] word, List<string> extra = null)
        {
            this.levelId = levelId;
            this.width = width;
            this.height = height;
            this.letters = letters;
            this.words = word;
            this.extra = extra ?? new List<string>();
        }
    }    
