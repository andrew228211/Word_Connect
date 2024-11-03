using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Brute_Force_Algorithm
{
    public class CrosswordNode 
    {
        public bool isStartOfWord { get; set; }
        public int index { get; set; }

        public CrosswordNode(bool isStartOfWord, int index)
        {
            this.isStartOfWord = isStartOfWord;
            this.index = index;
        }
    }
}