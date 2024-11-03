using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brute_Force_Algorithm
{ 
    public class WordElement 
    {
        public string word { get; set; }
        public int index { get; set; }

        public WordElement(string word, int index)
        {
            this.word = word;
            this.index = index;
        }

    }
}
