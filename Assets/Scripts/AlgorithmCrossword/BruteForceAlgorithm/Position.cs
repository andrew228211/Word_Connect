using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Brute_Force_Algorithm
{
    public class Position 
    {
        public int intersections;
        public int row;
        public int column;
        public Direction direction;

        public Position(int intersections, int row, int column, Direction direction)
        {
            this.intersections = intersections;
            this.row = row;
            this.column = column;
            this.direction = direction;
        }
    }
}