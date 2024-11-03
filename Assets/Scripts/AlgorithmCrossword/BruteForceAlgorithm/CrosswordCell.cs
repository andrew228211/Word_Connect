using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brute_Force_Algorithm
{
    public class CrosswordCell 
    {
        public string wordParent {  get; set; }
        public char c { get; set; }
        public CrosswordNode Across { get; set; }
        public CrosswordNode Down { get; set; }

        public CrosswordCell(char letter)
        {
            c = letter;
            Across = null;
            Down = null;
        }
    }
}
