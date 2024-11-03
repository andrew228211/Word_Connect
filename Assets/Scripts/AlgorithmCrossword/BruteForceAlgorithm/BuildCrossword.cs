using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Brute_Force_Algorithm
{
    public class BuildCrossword
    {
        private const int GRID_ROWS = 50;
        private const int GRID_COLS = 50;
        private Dictionary<char, List<(int row, int col)>> char_index;
        private List<WordElement> bad_words;
        private CrosswordCell[,] grid;
        private List<WordElement> word_elements;
        private List<string> words_in;
        private List<string> clues_in;

        public BuildCrossword(List<string> words_in, List<string> clues_in)
        {
            this.words_in = words_in;
            this.clues_in = clues_in;
            char_index = new Dictionary<char, List<(int row, int col)>>();
            bad_words = new List<WordElement>();
            grid = new CrosswordCell[GRID_ROWS, GRID_COLS];
            word_elements = new List<WordElement>();

            if (words_in.Count < 2) throw new Exception("A crossword must have at least 2 words");
            if (words_in.Count != clues_in.Count) throw new Exception("The number of words must equal the number of clues");

            for (int i = 0; i < words_in.Count; i++)
            {
                word_elements.Add(new WordElement(words_in[i], i));
            }

            word_elements.Sort((a, b) => b.word.Length - a.word.Length);
        }
        public List<WordElement> GetBadWords()
        {
            return bad_words;
        }

        private Direction RandomDirection()
        {
            return new Random().Next(2) == 0 ? Direction.Horizontal : Direction.Vertical;
        }
        private void Clear()
        {
            for (int r = 0; r < GRID_ROWS; r++)
            {
                for (int c = 0; c < GRID_COLS; c++)
                {
                    grid[r, c] = null;
                }
            }
            char_index = new Dictionary<char, List<(int row, int col)>>();
        }

        private int CanPlaceCharAt(char val, int row, int col)
        {
            if (grid[row, col] == null) return 0;
            if (grid[row, col].c == val) return 1;

            return -1;
        }

        
        /// <summary>
        /// Có thể đặt từ trên hàng và cột theo hướng
        /// </summary>
        /// <param name="word">từ</param>
        /// <param name="row">hàng</param>
        /// <param name="col">cột</param>
        /// <param name="direction">hướng</param>
        public int CanPlaceWordAt(string word, int row, int col, Direction direction)
        {
            if (row < 0 || row >= grid.GetLength(0) || col < 0 || col >= grid.GetLength(1)) return -1;
            if (direction == Direction.Horizontal)
            {
                if (col + word.Length > grid.GetLength(0)) return -1; //ngoài bảng
                if (col - 1 >= 0 && grid[row, col - 1] != null) return -1; //không thể đặt theo hướng bên trái
                if (col + word.Length < GRID_COLS && grid[row, col + word.Length] != null) return -1; //không thể dặt từ theo hướng bên phải

                //Kiểm tra hàng trên chắc chắn không có từ khác
                //Chạy cùng lúc nếu ổn sẽ có từ 
                //Kí tự bên dưới giao với từ hiện tại
                for (int r = row - 1, c = col, i = 0; r >= 0 && c < col + word.Length; c++, i++)
                {
                    bool is_empty = grid[r, c] == null;
                    bool is_intersection = grid[row, c] != null && grid[row, c].c == word[i];
                    bool can_place_here = is_empty || is_intersection;
                    if (!can_place_here) return -1;
                }
                //Tương tự tìm kiếm ở hàng bên dưới từ
                for (int r = row + 1, c = col, i = 0; r < grid.GetLength(0) && c < col + word.Length; c++, i++)
                {
                    bool is_empty = grid[r, c] == null;
                    bool is_intersection = grid[row, c] != null && grid[row, c].c == word[i];
                    bool can_place_here = is_empty || is_intersection;
                    if (!can_place_here) return -1;
                }
                //kiểm tra đảm bảo không chồng chéo kí tự
                int cntIntersections = 0;
                for (int c = col, i = 0; c < col + word.Length; c++, i++)
                {
                    var result = CanPlaceCharAt(word[i], row, c);
                    if (result == -1) return -1;
                    else cntIntersections += result;
                }
                return cntIntersections;
            }
            else
            {
                if (row + word.Length > GRID_ROWS) return -1;
                //Không thể có từ hướng trên
                if (row - 1 >= 0 && grid[row - 1, col] != null) return -1;
                //Không có từ hướng dưới
                if (row + word.Length < GRID_ROWS && grid[row + word.Length, col] != null) return -1;

                //Kiểm tra cột bên trái để chắc chắn rằng không có từ khác
                for (int c = col - 1, r = row, i = 0; c >= 0 && r < row + word.Length; r++, i++)
                {
                    bool is_empty = grid[r, c] == null;
                    bool is_intersection = grid[r, col] != null && grid[r, col].c == word[i];
                    bool can_place_here = is_empty || is_intersection;
                    if (!can_place_here) return -1;
                }
                //Tương tự nhưng cột bên phải
                for (int c = col + 1, r = row, i = 0; r < row + word.Length && c < GRID_COLS; r++, i++)
                {
                    bool is_empty = grid[r, c] == null;
                    bool is_intersection = grid[r, col] != null && grid[r, col].c == word[i];
                    bool can_place_here = is_empty || is_intersection;
                    if (!can_place_here) return -1;
                }

                int cntIntersections = 0;
                for (int r = row, i = 0; r < row + word.Length; r++, i++)
                {
                    var result = CanPlaceCharAt(word[i], r, col);
                    if (result == -1) return -1;
                    else cntIntersections += result;
                }
                return cntIntersections;
            }
        }

        /// <summary>
        /// Tìm vị trí cho từ
        /// </summary>
        private Position FindPositionForWord(string word)
        {
            List<Position> bests = new List<Position>();
            //Kiểm tra char_index của mỗi chữ cái để xem có thể đặt theo hướng không
            for (int i = 0; i < word.Length; i++)
            {
                var possible_locations_on_grid = char_index[word[i]];
                if (possible_locations_on_grid == null) continue;
                foreach (var point in possible_locations_on_grid)
                {
                    var r = point.row;
                    var c = point.col;
                    //c-i,r-i là offset cho các kí tự
                    int intersections_across = CanPlaceWordAt(word, r, c - i, Direction.Horizontal);
                    int intersections_down = CanPlaceWordAt(word, r - i, c, Direction.Vertical);

                    if (intersections_across != -1)
                    {
                        bests.Add(new Position(intersections_across, r, c - i, Direction.Horizontal));
                    }
                    if (intersections_down != -1)
                    {
                        bests.Add(new Position(intersections_down, r - i, c, Direction.Vertical));
                    }
                }
            }
            if (bests.Count == 0) return null;
            Random random = new Random();
            Position bestPos = bests[random.Next(bests.Count)];
            return bestPos;
        }

        /// <summary>
        /// Thêm kí tự vào Gird nếu ô đó trống
        /// </summary>
        /// <param name="word">từ chỉ định</param>
        /// <param name="index_of_word_in_input_list">vị trí của từ trong danh sách đầu vào</param>
        /// <param name="index_of_char">vị trí của kí tự trong word</param>
        /// <param name="r">hàng</param>
        /// <param name="c">cột</param>
        /// <param name="direction">hướng</param>
        private void AddCellToGrid(string word, int index_of_word_in_input_list, int index_of_char, int r, int c, Direction direction)
        {
            var char_val = word[index_of_char];
            if (grid[r, c] == null)
            {
                grid[r, c] = new CrosswordCell(char_val);

                if (!char_index.ContainsKey(char_val)) char_index[char_val] = new List<(int row, int col)>();

                char_index[char_val].Add((r, c));
            }
            grid[r, c].wordParent = word;
            var is_start_of_word = index_of_char == 0;
            if (direction == Direction.Horizontal)
            {
                grid[r, c].Across = new CrosswordNode(is_start_of_word, index_of_word_in_input_list);
            }
            else if (direction == Direction.Vertical)
            {
                grid[r, c].Down = new CrosswordNode(is_start_of_word, index_of_word_in_input_list);
            }
        }

        /// <summary>
        /// Đặt từ ở hàng và cột theo chỉ định,  kí tự tiếp theo phải hoặc dưới tuỳ theo hướng
        /// </summary>
        /// <param name="word">từ</param>
        /// <param name="index_of_word_in_input_list">vị trí của từ trong danh sách đầu vào</param>
        /// <param name="row">hàng</param>
        /// <param name="col">cột</param>
        /// <param name="direction">hướng</param>
        private void PlaceWordAt(string word, int index_of_word_in_input_list, int row, int col, Direction direction)
        {
            if (direction == Direction.Horizontal)
            {
                for (int c = col, i = 0; c < col + word.Length; c++, i++)
                {
                    AddCellToGrid(word, index_of_word_in_input_list, i, row, c, direction);
                }
            }
            else if (direction == Direction.Vertical)
            {
                for (int r = row, i = 0; r < row + word.Length; r++, i++)
                {
                    AddCellToGrid(word, index_of_word_in_input_list, i, r, col, direction);
                }
            }
        }

        /// <summary>
        /// Di chuyển gird tới gird nhỏ nhất sẽ phù hợp với nó
        /// </summary>
        private CrosswordCell[,] MinimizeGrid()
        {
            int r_min = GRID_ROWS - 1, r_max = 0, c_min = GRID_COLS - 1, c_max = 0;
            for (int r = 0; r < GRID_ROWS; r++)
            {
                for (int c = 0; c < GRID_COLS; c++)
                {
                    if (grid[r, c] != null)
                    {
                        if (r < r_min) r_min = r;
                        if (r > r_max) r_max = r;
                        if (c < c_min) c_min = c;
                        if (c > c_max) c_max = c;
                    }
                }
            }

            var rows = r_max - r_min + 1;
            var cols = c_max - c_min + 1;
            var new_grid = new CrosswordCell[rows, cols];

            for (int r = r_min, r2 = 0; r2 < rows; r++, r2++)
            {
                for (int c = c_min, c2 = 0; c2 < cols; c++, c2++)
                {
                    new_grid[r2, c2] = grid[r, c];
                }
            }

            return new_grid;
        }

        public CrosswordCell[,] GetGrid(int max_tries)
        {
            bool word_has_been_added_to_grid = false;
            List<List<WordElement>> groups = new List<List<WordElement>> { word_elements.Skip(1).ToList() };
            for (int tries = 0; tries < max_tries; tries++)
            {
                Clear();
                Direction start_dir = RandomDirection();
                var r = GRID_ROWS / 2;
                var c = GRID_COLS / 2;
                var word_element = word_elements[0];
                if (start_dir == Direction.Horizontal)
                {
                    c -= word_element.word.Length / 2;
                }
                else
                {
                    r -= word_element.word.Length / 2;
                }
                if (CanPlaceWordAt(word_element.word, r, c, start_dir) != -1)
                {
                    PlaceWordAt(word_element.word, word_element.index, r, c, start_dir);
                }
                else
                {
                    bad_words = new List<WordElement> { word_element };
                    return null;
                }
                groups = new List<List<WordElement>> { word_elements.Skip(1).ToList() };
                for (int g = 0; g < groups.Count; g++)
                {
                    word_has_been_added_to_grid = false;
                    for (int i = 0; i < groups[g].Count; i++)
                    {
                        var word_element_g = groups[g][i];
                        var best_position = FindPositionForWord(word_element_g.word);
                        if (best_position == null)
                        {
                            if (groups.Count - 1 == g) groups.Add(new List<WordElement>());
                            groups[g + 1].Add(word_element_g);
                        }
                        else
                        {
                            int r_pos = best_position.row;
                            int c_pos = best_position.column;
                            Direction dir = best_position.direction;
                            PlaceWordAt(word_element_g.word, word_element_g.index, r_pos, c_pos, dir);
                            word_has_been_added_to_grid = true;
                        }
                    }
                    if (!word_has_been_added_to_grid) break;
                }
                if (word_has_been_added_to_grid) return MinimizeGrid();
            }
            bad_words = groups[groups.Count - 1];
            return null;
        }

        public CrosswordCell[,] GetSquareGrid(int max_tries)
        {
            CrosswordCell[,] best_grid = null;
            double best_ratio = 0;
            for (int i = 0; i < max_tries; i++)
            {
                var a_grid = GetGrid(1);
                if (a_grid == null) continue;
                double ratio = Math.Min(a_grid.GetLength(0), a_grid.GetLength(1)) * 1.0 / Math.Max(a_grid.GetLength(0), a_grid.GetLength(1));
                if (ratio > best_ratio)
                {
                    best_grid = a_grid;
                    best_ratio = ratio;
                }
                if (best_grid.GetLength(0) >= 15 || best_grid.GetLength(1) >= 15)
                {
                    i--;
                    continue;
                } 
                if (best_ratio == 1) break;
            }
            return best_grid;
        }
    }
}
