using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace GeneticAlgorithm
{
    public class CrossWord
    {
        public string _word;
        public int _row;
        public int _col;
        public Direction _direction;

        public CrossWord(string word, int row, int col, Direction direction)
        {
            _word = word;
            _row = row;
            _col = col;
            _direction = direction;
        }
    }
}

