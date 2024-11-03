using System.Collections;
using System.Collections.Generic;
using System;
namespace GeneticAlgorithm
{
    public class GeneticCrossWord
    {
        private const int PENALTY = 10;
        private int GRID_SIZE = 20;
        private char[][] grid;
        private Random random = new Random();
        public List<CrossWord> words = new List<CrossWord>();
        private int currentFitness;
        /// <summary>
        /// Khởi tạo có tham số gồm danh sách từ và độ dài các từ
        /// </summary>
        /// <param name="words">Danh sách các từ</param>
        /// <param name="maxSize">Độ dài của từ lớn nhất</param>
        public  GeneticCrossWord(List<string> words, int maxSize)
        {
            this.words = new List<CrossWord>();
            GRID_SIZE = maxSize;
            grid = new char[GRID_SIZE][];
            //Dat vi tri moi tu random
            foreach (string word in words)
            {
                int row, col;
                Direction direction = random.Next(0, 2) == 0 ? Direction.Horizontal : Direction.Vertical;
                if (direction == Direction.Horizontal)
                {
                    col = random.Next(GRID_SIZE - word.Length + 1);
                    row = random.Next(GRID_SIZE);
                }
                else
                {
                    row = random.Next(GRID_SIZE - word.Length + 1);
                    col = random.Next(GRID_SIZE);
                }
                this.words.Add(new CrossWord(word, row, col, direction));
            }
            currentFitness = -1;
        }
        public int GetCurrentFitness()
        {
            if (this.currentFitness < 0)
            {
                this.currentFitness = this.CalculateFitness();
            }

            return this.currentFitness;
        }
        private void ResetGrid()
        {
            for (int i = 0; i < GRID_SIZE; i++)
            {
                grid[i]=new char[GRID_SIZE];
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    grid[i][j] = '-';
                }
            }
        }

        private bool CharInBounds(int row, int col)
        {
            return row >= 0 && row < GRID_SIZE && col >= 0 && col < GRID_SIZE;
        }
        #region Tính toán mức độ phù hợp của crossword hiện tại Calculate Fitness

        /// <summary>
        /// Kiểm tra sự trùng lặp
        /// </summary>
        /// <returns>Trả về giá trị penaly cho các từ trùng lặp</returns>
        private int OverlapCheck()
        {
            int penalty = 0;
            ResetGrid();
            foreach (CrossWord word in this.words)
            {
                char[] arrayChar = word._word.ToCharArray();
                for (int i = 0; i < arrayChar.Length; i++)
                {
                    int row = word._row + (word._direction == Direction.Horizontal ? 0 : i);
                    int col = word._col + (word._direction == Direction.Vertical ? 0 : i);
                    if (CharInBounds(row, col))
                    {
                        if (grid[row][col] != '-' && grid[row][col] != arrayChar[i])
                        {
                            penalty += PENALTY;
                        }
                        grid[row][col] = arrayChar[i];
                    }
                }
            }
            return penalty;
        }

        /// <summary>
        /// Kiểm tra kết nối các từ.
        /// Sử dụng DFS để xác định số lượng thành phần riêng biệt
        /// </summary>
        /// <returns>Trả về giá trị penalty cho các phần riêng biệt cua crossword</returns>
        private int ConnectivityCheck()
        {
            bool[][] visited = new bool[GRID_SIZE][];
            for(int i=0;i<GRID_SIZE; i++)
            {
                visited[i] = new bool[GRID_SIZE];
            }
            int connectedCompoents = 0;
            for (int i = 0; i < GRID_SIZE; i++)
            {
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    if (!visited[i][j] && grid[i][j] != '-')
                    {
                        Dfs(i, j, visited);
                        connectedCompoents++;
                    }
                }
            }
            return connectedCompoents > 1 ? connectedCompoents * PENALTY : 0;
        }
        private void Dfs(int row, int col, bool[][] visited)
        {
            if (!CharInBounds(row, col) || visited[row][col] || grid[row][col] == '-')
            {
                return;
            }
            visited[row][col] = true;
            Dfs(row - 1, col, visited);
            Dfs(row + 1, col, visited);
            Dfs(row, col - 1, visited);
            Dfs(row, col + 1, visited);
        }

        /// <summary>
        /// Kiểm tra sự tồn tại của crossword. Đi tới các từ lân cận và kiểm tra
        /// </summary>
        /// <param name="currentWord">Có 1 neighbour</param>
        /// <param name="isFirstChar">Cờ vị trí từ tìm kiếm</param>
        /// <returns>Trả về true nếu crossword tồn tại, false ngược lại</returns>
        private bool IsCrossingWordAbsent(CrossWord currentWord, bool isFirstChar)
        {
            int row;
            int col;
            // neu chu hien tai nam ngang can co chu nam doc
            if (currentWord._direction == Direction.Horizontal)
            {
                row = currentWord._row - 1;
                if (isFirstChar)
                {
                    col = currentWord._col;
                }
                else
                {
                    col = currentWord._col + currentWord._word.Length - 1;
                }

                while (CharInBounds(row, col) && grid[row][col] != '-')
                {
                    CrossWord crossingWord = GetWordByCoordinates(row, col, Direction.Vertical);

                    if (crossingWord != null && (crossingWord._col + crossingWord._word.Length - 1) >= currentWord._row)
                    {
                        return false;
                    }

                    row -= 1;
                }

            }
            else
            { // if the current word is vertical, the horizontal crossing word is needed
                if (isFirstChar)
                {
                    row = currentWord._row;
                }
                else
                {
                    row = currentWord._col + currentWord._word.Length - 1;
                }

                col = currentWord._col - 1;
                while (CharInBounds(row, col) && grid[row][col] != '-')
                {
                    CrossWord crossingWord = GetWordByCoordinates(row, col, Direction.Horizontal);

                    if (crossingWord != null && (crossingWord._col + crossingWord._word.Length - 1) >= currentWord._col)
                    {
                        return false;
                    }

                    col -= 1;
                }

            }

            return true;
        }

        /// <summary>
        /// Tìm kiếm từ theo tham số đã cho
        /// </summary>
        /// <param name="row">hàng</param>
        /// <param name="col">cột</param>
        /// <param name="dir">hướng của từ</param>
        /// <returns>Từ tương ứng với tham số va null nếu không tìm thấy</returns>
        private CrossWord GetWordByCoordinates(int row, int col, Direction dir)
        {
            foreach (CrossWord word in words)
            {
                if (word._row == row && word._col == col && word._direction == dir)
                {
                    return word;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Kiểm tra các từ lân cận
        /// </summary>
        /// <returns>return gia tri penalty co cac tu lan can khong hop le</returns>
        private int NeighbouringWordsCheck()
        {
            int penalty = 0;

            foreach (CrossWord word in words)
            {
                char[] arrayChar = word._word.ToCharArray();

                // Đếm các từ liền kề theo mọi hướng
                int adjCharCounterSideUp = 0;
                int adjCharCounterSideDown = 0;
                int adjCharCounterSideLeft = 0;
                int adjCharCounterSideRight = 0;

                for (int charIdx = 0; charIdx < arrayChar.Length; charIdx++)
                {
                    int row;
                    int col;
                    if (word._direction == Direction.Horizontal)
                    {
                        row = word._row;
                        col = word._col + charIdx;

                        if (CharInBounds(row, col))
                        {
                            if (charIdx == 0)
                            {
                                if (CharInBounds(row, col - 1) && grid[row][col - 1] != '-')
                                {
                                    penalty += PENALTY;
                                }
                                if (CharInBounds(row - 1, col) && grid[row - 1][col] != '-')
                                {
                                    if (IsCrossingWordAbsent(word, true))
                                    {
                                        penalty += PENALTY;
                                    }
                                }
                            }
                            else if (charIdx == arrayChar.Length - 1)
                            {
                                if (CharInBounds(row, col + 1) && grid[row][col + 1] != '-')
                                {
                                    penalty += PENALTY;
                                }
                                if (CharInBounds(row - 1, col) && grid[row - 1][col] != '-')
                                {
                                    if (IsCrossingWordAbsent(word, false))
                                    {
                                        penalty += PENALTY;
                                    }
                                }
                            }
                            if (CharInBounds(row - 1, col) && grid[row - 1][col] != '-')
                            {
                                adjCharCounterSideUp++;
                                if (adjCharCounterSideUp > 1)
                                {
                                    penalty += PENALTY;
                                }
                            }
                            else if (CharInBounds(row - 1, col) && grid[row - 1][col] == '-')
                            {
                                adjCharCounterSideUp = 0;
                            }
                            if (CharInBounds(row + 1, col) && grid[row + 1][col] != '-')
                            {
                                adjCharCounterSideDown++; 
                                if (adjCharCounterSideDown > 1)
                                {
                                    penalty += PENALTY; 
                                }
                            }
                            else if (CharInBounds(row + 1, col) && grid[row + 1][col] == '-')
                            {
                                adjCharCounterSideDown = 0; 
                            }
                        }
                    }
                    else
                    { 
                        row = word._row + charIdx;
                        col = word._col; 

                        if (CharInBounds(row, col))
                        {
                            if (charIdx == 0)
                            {
                                if (CharInBounds(row - 1, col) && grid[row - 1][col] != '-')
                                {
                                    penalty += PENALTY; 
                                }
                                if (CharInBounds(row, col - 1) && grid[row][col - 1] != '-')
                                {
                                    if (IsCrossingWordAbsent(word, true))
                                    {
                                        penalty += PENALTY; 
                                    }
                                }
                            }
                            else if (charIdx == arrayChar.Length - 1)
                            {
                                if (CharInBounds(row + 1, col) && grid[row + 1][col] != '-')
                                {
                                    penalty += PENALTY; 
                                }
                                if (CharInBounds(row, col - 1) && grid[row][col - 1] != '-')
                                {
                                    if (IsCrossingWordAbsent(word, false))
                                    {
                                        penalty += PENALTY;
                                    }
                                }
                            }
                            if (CharInBounds(row, col - 1) && grid[row][col - 1] != '-')
                            {
                                adjCharCounterSideLeft++; 
                                if (adjCharCounterSideLeft > 1)
                                {
                                    penalty += PENALTY;
                                }
                            }
                            else if (CharInBounds(row, col - 1) && grid[row][col - 1] == '-')
                            {
                                adjCharCounterSideLeft = 0; 
                            }
                            if (CharInBounds(row, col + 1) && grid[row][col + 1] != '-')
                            {
                                adjCharCounterSideRight++; 
                                if (adjCharCounterSideRight > 1)
                                {
                                    penalty += PENALTY; 
                                }
                            }
                            else if (CharInBounds(row, col + 1) && grid[row][col + 1] == '-')
                            {
                                adjCharCounterSideRight = 0; 
                            }
                        }
                    }
                }
            }

            return penalty;
        }
        private int CalculateFitness()
        {
            int fitness = 0;
            fitness += OverlapCheck();
            fitness += ConnectivityCheck();
            fitness += NeighbouringWordsCheck();
            return fitness;
        }
        #endregion

        #region Trao đổi chéo Crossover cho 2 parent
        public GeneticCrossWord CrossOver(GeneticCrossWord partner) {
            GeneticCrossWord child = new GeneticCrossWord(new List<string>(),GRID_SIZE);
            for(int i = 0; i < words.Count; i++)
            {
                CrossWord parent1 = words[i];
                CrossWord parent2 = partner.words[i];
                //Random chọn parent cho gen hiện tại
                bool wordParent=random.Next(0,2)==0? true : false;
                int row=wordParent ? parent1._row : parent2._row;
                int col=wordParent ? parent1._col : parent2._col;
                Direction direction = wordParent ? parent1._direction : parent2._direction;
                child.words.Add(new CrossWord(parent1._word,row, col, direction));
            }
            return child;
        }
        #endregion

        #region Đột biến
        /// <summary>
        /// Đột biến Gen
        /// </summary>
        public void Mutate()
        {
            //Random gen word
            int wordIndex=random.Next(words.Count);
            CrossWord word = words[wordIndex];
            //Random thay đổi vị trí từ hoặc hướng
            word._direction=random.Next(0,2)==0?Direction.Horizontal:Direction.Vertical;
            if (word._direction == Direction.Horizontal)
            {
                word._col = random.Next(GRID_SIZE - word._word.Length + 1);
                word._row=random.Next(GRID_SIZE);
            }
            else
            {
                word._row = random.Next(GRID_SIZE - word._word.Length + 1);
                word._col = random.Next(GRID_SIZE);
            }
            //Cập nhật lại fitness để tính toán lại
            this.currentFitness = -1;
        }
        #endregion

        #region Copy Paste
        /// <summary>
        /// Sao chép một số nhiễm sắc thể mà thấy phù hợp ở thế hệ tiếp theo.
        /// </summary>
        /// <returns>Trả về bản sao của crossword hiện tại</returns>
        public  GeneticCrossWord Copy()
        {
             GeneticCrossWord copy = new  GeneticCrossWord(new List<string>(),GRID_SIZE);
            foreach(CrossWord word in words)
            {
                copy.words.Add(new CrossWord(word._word,word._row,word._col,word._direction));
            }
            return copy;
        }
        #endregion

        #region Trả về Gird chứa các từ CrossWord
        public char[][] GetGridForCrossWord()
        {
            char[][] res = new char[GRID_SIZE][];
            for (int i = 0; i < GRID_SIZE; i++)
            {
                res[i] = new char[GRID_SIZE];
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    res[i][j] = '-';
                }
            }
            foreach (CrossWord word in words)
            {
                char[] arrayChar = word._word.ToCharArray();
                for (int i = 0; i < arrayChar.Length; i++)
                {
                    int row = word._row + (word._direction == Direction.Horizontal ? 0 : i);
                    int col = word._col + (word._direction == Direction.Vertical ? 0 : i);

                    if (row >= 0 && row < GRID_SIZE && col >= 0 && col < GRID_SIZE)
                    {
                        res[row][col] = arrayChar[i];
                    }
                    else
                    {
                        Console.WriteLine("Warning: Word '" + word._word + "' is out of bounds in the crossword grid.");
                    }
                }
            }
            return res;
        }
        #endregion
    }
}
