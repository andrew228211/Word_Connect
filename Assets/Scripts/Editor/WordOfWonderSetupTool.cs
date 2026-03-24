using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Brute_Force_Algorithm;
public class ToolLevelWordOfWonder : EditorWindow
    {
        public const string LEVEL_DATA_PATH = "Assets/14.TuanTran/51.WordOfWonder/LevelData/";
        public const int GRID_SIZE = 18; // max = 18x18

        private const int DEFAULT_MAX_TRIES = 250;
        private const string AI_KEY_PREF = "WOW_TOOL_AI_KEY";
        private const string AI_MODEL_PREF = "WOW_TOOL_AI_MODEL";
        private const string DEFAULT_AI_MODEL = "gpt-4.1-mini";

        private string _lettersInput = "CAT";
        private string _wordsInput = "CAT\nACT";
        private string _extraWordsInput = string.Empty;
        private string _aiApiKey = string.Empty;
        private string _aiModel = DEFAULT_AI_MODEL;
        private int _aiExtraWordLimit = 10;
        private bool _isGeneratingExtra;
        public int _levelId = 1;
        private int _maxTries = DEFAULT_MAX_TRIES;
        private Vector2 _scroll;

        private string _resultMessage = "Nhap du lieu roi bam Build Crossword.";
        private MessageType _resultType = MessageType.Info;

        private CrosswordCell[,] _generatedGrid;
        private readonly List<DataWord> _generatedDataWords = new List<DataWord>();
        private readonly List<string> _generatedExtraWords = new List<string>();
        private string _gridPreview = string.Empty;
        private string _dataWordJson = string.Empty;
      
        [MenuItem("Tools/WordOfWonder/Level Crossword Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<ToolLevelWordOfWonder>("Level Crossword Tool");
            window.minSize = new Vector2(520, 540);
        }

        private void OnEnable()
        {
            _aiApiKey = EditorPrefs.GetString(AI_KEY_PREF, string.Empty);
            _aiModel = EditorPrefs.GetString(AI_MODEL_PREF, DEFAULT_AI_MODEL);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Word Of Wonder - Crossword Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Nhap letters + words de build crossword. Ban co the nhap/AI-generate danh sach extra words, tool se validate va xuat vao JSON.", MessageType.None);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(6);
            DrawInputArea();

            EditorGUILayout.Space(8);
            DrawActionButtons();

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(_resultMessage, _resultType);

            if (_generatedGrid != null)
            {
                DrawOutputArea();
            }

            EditorGUILayout.EndScrollView();
            DrawnResultMessage();
        }

        private void DrawInputArea()
        {
            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            _levelId = EditorGUILayout.IntField("Level ID", Mathf.Max(1, _levelId));
            
            _lettersInput = EditorGUILayout.TextField("Letters", _lettersInput);
            
            _maxTries = EditorGUILayout.IntSlider("Max Tries", Mathf.Clamp(_maxTries, 1, 2000), 1, 2000);

            EditorGUILayout.LabelField("Words (1 dong 1 tu, co the dung dau ',' hoac ';')");
            _wordsInput = EditorGUILayout.TextArea(_wordsInput, GUILayout.MinHeight(120));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Extra (optional - 1 dong 1 tu)");
            _extraWordsInput = EditorGUILayout.TextArea(_extraWordsInput, GUILayout.MinHeight(80));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("AI Extra Generator", EditorStyles.boldLabel);
            string nextApiKey = EditorGUILayout.PasswordField("AI API Key", _aiApiKey);
            if (!string.Equals(nextApiKey, _aiApiKey, StringComparison.Ordinal))
            {
                _aiApiKey = nextApiKey;
                EditorPrefs.SetString(AI_KEY_PREF, _aiApiKey ?? string.Empty);
            }

            string nextModel = EditorGUILayout.TextField("AI Model", _aiModel);
            if (!string.Equals(nextModel, _aiModel, StringComparison.Ordinal))
            {
                _aiModel = nextModel;
                EditorPrefs.SetString(AI_MODEL_PREF, _aiModel ?? DEFAULT_AI_MODEL);
            }

            _aiExtraWordLimit = EditorGUILayout.IntSlider("AI Extra Limit", Mathf.Clamp(_aiExtraWordLimit, 1, 30), 1, 30);

            EditorGUI.BeginDisabledGroup(_isGeneratingExtra);
            if (GUILayout.Button(_isGeneratingExtra ? "Generating Extra..." : "Generate Extra By AI", GUILayout.Height(24)))
            {
                _ = GenerateExtraWordsByAiAsync();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build Crossword + DataWord", GUILayout.Height(30)))
            {
                BuildCrosswordAndMapData();
            }

            if (GUILayout.Button("Clear", GUILayout.Height(30)))
            {
                ClearOutput();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawOutputArea()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Grid Size: {_generatedGrid.GetLength(0)} x {_generatedGrid.GetLength(1)}");
            EditorGUILayout.LabelField($"DataWord Entries: {_generatedDataWords.Count}");
            EditorGUILayout.LabelField($"Extra Words: {_generatedExtraWords.Count}");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Grid Preview ('.' = empty)");
            EditorGUILayout.TextArea(_gridPreview, GUILayout.MinHeight(140));

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Copy DataWord JSON", GUILayout.Height(24)))
            {
                EditorGUIUtility.systemCopyBuffer = _dataWordJson;
                ShowTemporaryMessage("Da copy DataWord JSON vao clipboard.", MessageType.Info);
            }

            if (GUILayout.Button("Save DataWord JSON", GUILayout.Height(24)))
            {
                SaveDataWordJsonToAsset();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("DataWord JSON");
            EditorGUILayout.TextArea(_dataWordJson, GUILayout.MinHeight(180));

            EditorGUILayout.EndVertical();
        }
        private void DrawnResultMessage()
        {
            if (string.IsNullOrEmpty(_resultMessage))
            {
                return;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"{_generatedDataWords.Count} Results / {_generatedExtraWords.Count} Extra", EditorStyles.boldLabel);

        }
        private void BuildCrosswordAndMapData()
        {
            ClearOutput(keepMessage: true);

            if (!TryParseInput(out string letters, out List<string> words, out List<string> extraWords, out string error))
            {
                SetResult(error, MessageType.Error);
                return;
            }

            if (!ValidateWordCanBeBuiltFromLetters(letters, words, "Word", out string invalidWordError))
            {
                SetResult(invalidWordError, MessageType.Error);
                return;
            }

            if (!ValidateWordCanBeBuiltFromLetters(letters, extraWords, "Extra", out string invalidExtraError))
            {
                SetResult(invalidExtraError, MessageType.Error);
                return;
            }

            BuildCrossword crossword;
            CrosswordCell[,] grid;
            try
            {
                List<string> clues = words.Select(_ => string.Empty).ToList();
                crossword = new BuildCrossword(words, clues);
                grid = crossword.GetSquareGrid(_maxTries);
            }
            catch (Exception e)
            {
                SetResult($"BuildCrossword bi loi: {e.Message}", MessageType.Error);
                return;
            }

            if (grid == null)
            {
                List<string> badWords = crossword.GetBadWords().Select(w => w.word).ToList();
                string badWordText = badWords.Count > 0 ? string.Join(", ", badWords) : "khong xac dinh";
                SetResult($"Khong the tao crossword hop le voi input hien tai. Bad words: {badWordText}", MessageType.Error);
                return;
            }
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            if (rows > GRID_SIZE || cols > GRID_SIZE)
            {
                SetResult($"Crossword tao ra la {rows}x{cols}, vuot gioi han {GRID_SIZE}x{GRID_SIZE}.", MessageType.Error);
                return;
            }

            _generatedGrid = grid;
            _generatedDataWords.AddRange(ConvertGridToDataWords(_generatedGrid, words));
            _generatedExtraWords.AddRange(extraWords);
            _gridPreview = BuildGridPreview(_generatedGrid);

          
            WordWrapper wordWrapper = new WordWrapper(
                levelId: _levelId,
                width: cols,
                height: rows,
                letters: letters,
                word: _generatedDataWords.ToArray(),
                extra: _generatedExtraWords
            );
            _dataWordJson = JsonUtility.ToJson(wordWrapper, true);

            SetResult($"Hop le. Da tao crossword {rows}x{cols}, map du lieu ra {_generatedDataWords.Count} DataWord entries, extra {_generatedExtraWords.Count} words.", MessageType.Info);
        }

        private bool TryParseInput(out string letters, out List<string> words, out List<string> extraWords, out string error)
        {
            letters = SanitizeToken(_lettersInput);
            words = new List<string>();
            extraWords = new List<string>();
            error = null;

            if (string.IsNullOrWhiteSpace(letters))
            {
                error = "Letters dang rong.";
                return false;
            }

            if (!ContainsOnlyLetters(letters))
            {
                error = "Letters chi duoc phep chua ky tu chu cai.";
                return false;
            }

            if (!TryParseWordsFromInput(_wordsInput, "Word", out words, out error))
            {
                return false;
            }

            if (words.Count < 2)
            {
                error = "BuildCrossword can toi thieu 2 words.";
                return false;
            }

            if (!TryParseWordsFromInput(_extraWordsInput, "Extra", out extraWords, out error))
            {
                return false;
            }

            HashSet<string> existingWords = new HashSet<string>(words, StringComparer.Ordinal);
            extraWords = extraWords
                .Where(w => !existingWords.Contains(w))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return true;
        }

        private static bool TryParseWordsFromInput(string raw, string label, out List<string> words, out string error)
        {
            words = new List<string>();
            error = null;

            foreach (string token in ParseRawWordTokens(raw))
            {
                string cleaned = SanitizeToken(token);
                if (string.IsNullOrWhiteSpace(cleaned))
                {
                    continue;
                }

                if (!ContainsOnlyLetters(cleaned))
                {
                    error = $"{label} '{token}' chua ky tu khong hop le.";
                    return false;
                }

                words.Add(cleaned);
            }

            words = words.Distinct(StringComparer.Ordinal).ToList();
            return true;
        }

        private static IEnumerable<string> ParseRawWordTokens(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                yield break;
            }

            string normalizedRaw = raw.Replace(",", "\n").Replace(";", "\n");
            string[] tokens = normalizedRaw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                yield return token;
            }
        }

        private bool ValidateWordCanBeBuiltFromLetters(string letters, List<string> words, string label, out string error)
        {
            error = null;
            Dictionary<char, int> letterCounter = CountLetters(letters);

            foreach (string word in words)
            {
                if (CanWordBeBuiltFromLetterCounter(letterCounter, word))
                {
                    continue;
                }

                error = $"{label} '{word}' khong tao duoc tu letters '{letters}'.";
                return false;
            }

            return true;
        }

        private static bool CanWordBeBuiltFromLetterCounter(Dictionary<char, int> letterCounter, string word)
        {
            Dictionary<char, int> wordCounter = CountLetters(word);
            foreach (KeyValuePair<char, int> pair in wordCounter)
            {
                char c = pair.Key;
                int need = pair.Value;
                if (!letterCounter.TryGetValue(c, out int available) || need > available)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task GenerateExtraWordsByAiAsync()
        {
            if (_isGeneratingExtra)
            {
                return;
            }

            string letters = SanitizeToken(_lettersInput);
            if (string.IsNullOrWhiteSpace(letters))
            {
                SetResult("Letters dang rong, chua the goi AI.", MessageType.Warning);
                return;
            }

            if (!ContainsOnlyLetters(letters))
            {
                SetResult("Letters chi duoc phep chua ky tu chu cai.", MessageType.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_aiApiKey))
            {
                SetResult("AI API Key dang rong.", MessageType.Warning);
                return;
            }

            _isGeneratingExtra = true;
            SetResult("Dang goi AI de sinh extra words...", MessageType.Info);
            Repaint();

            try
            {
                List<string> aiWords = await WordOfWonderAiClient.GenerateExtraWordsAsync(
                    apiKey: _aiApiKey,
                    model: _aiModel,
                    letters: letters,
                    maxWords: _aiExtraWordLimit);

                HashSet<string> baseWords = new HashSet<string>(StringComparer.Ordinal);
                foreach (string token in ParseRawWordTokens(_wordsInput))
                {
                    string cleaned = SanitizeToken(token);
                    if (!string.IsNullOrWhiteSpace(cleaned) && ContainsOnlyLetters(cleaned))
                    {
                        baseWords.Add(cleaned);
                    }
                }

                Dictionary<char, int> letterCounter = CountLetters(letters);
                List<string> filteredWords = aiWords
                    .Select(SanitizeToken)
                    .Where(w => !string.IsNullOrWhiteSpace(w) && ContainsOnlyLetters(w))
                    .Where(w => CanWordBeBuiltFromLetterCounter(letterCounter, w))
                    .Where(w => !baseWords.Contains(w))
                    .Distinct(StringComparer.Ordinal)
                    .Take(_aiExtraWordLimit)
                    .ToList();

                _extraWordsInput = string.Join("\n", filteredWords);
                SetResult($"AI da sinh {filteredWords.Count} extra words hop le.", MessageType.Info);
            }
            catch (Exception e)
            {
                SetResult($"Goi AI bi loi: {e.Message}", MessageType.Error);
            }
            finally
            {
                _isGeneratingExtra = false;
                Repaint();
            }
        }

        private static string SanitizeToken(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(input.Length);
            foreach (char c in input.Trim())
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                sb.Append(char.ToUpperInvariant(c));
            }

            return sb.ToString();
        }

        private static bool ContainsOnlyLetters(string text)
        {
            return text.All(char.IsLetter);
        }

        private static Dictionary<char, int> CountLetters(string text)
        {
            Dictionary<char, int> counter = new Dictionary<char, int>();
            foreach (char c in text)
            {
                if (!counter.ContainsKey(c))
                {
                    counter[c] = 0;
                }

                counter[c]++;
            }

            return counter;
        }
        
        private static List<DataWord> ConvertGridToDataWords(CrosswordCell[,] grid, List<string> words)
        {
            List<DataWord> result = new List<DataWord>();
            int rows = grid.GetLength(0);
            HashSet<string> inputWords = words != null
                ? new HashSet<string>(words, StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> emitted = new HashSet<string>(StringComparer.Ordinal);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    CrosswordCell cell = grid[r, c];
                    if (cell == null)
                    {
                        continue;
                    }

                    if (HasHorizontalNeighbor(grid, r, c))
                    {
                        int startCol = FindHorizontalStartCol(grid, r, c);
                        string key = $"H:{r}:{startCol}";
                        if (emitted.Add(key))
                        {
                            string horizontalWord = ReadHorizontalWord(grid, r, startCol);
                            if (inputWords.Count == 0 || inputWords.Contains(horizontalWord))
                            {
                                result.Add(new DataWord
                                {
                                    x = startCol,
                                    y = r,
                                    direction = "H",
                                    word = horizontalWord
                                });
                            }
                        }
                    }

                    if (HasVerticalNeighbor(grid, r, c))
                    {
                        int startRow = FindVerticalStartRow(grid, r, c);
                        string key = $"V:{startRow}:{c}";
                        if (emitted.Add(key))
                        {
                            string verticalWord = ReadVerticalWord(grid, startRow, c);
                            if (inputWords.Count == 0 || inputWords.Contains(verticalWord))
                            {
                                result.Add(new DataWord
                                {
                                    x = c,
                                    y = startRow,
                                    direction = "V",
                                    word = verticalWord
                                });
                            }
                        }
                    }
                }
            } 
            result.Sort((a, b) =>
            {
                int cmpY = a.y.CompareTo(b.y);
                if (cmpY != 0) return cmpY;

                int cmpX = a.x.CompareTo(b.x);
                if (cmpX != 0) return cmpX;

                return string.CompareOrdinal(a.direction, b.direction);
            });
            return result;
        }

        private static bool HasHorizontalNeighbor(CrosswordCell[,] grid, int row, int col)
        {
            int cols = grid.GetLength(1);
            bool hasLeft = col > 0 && grid[row, col - 1] != null;
            bool hasRight = col < cols - 1 && grid[row, col + 1] != null;
            return hasLeft || hasRight;
        }

        private static bool HasVerticalNeighbor(CrosswordCell[,] grid, int row, int col)
        {
            int rows = grid.GetLength(0);
            bool hasUp = row > 0 && grid[row - 1, col] != null;
            bool hasDown = row < rows - 1 && grid[row + 1, col] != null;
            return hasUp || hasDown;
        }

        private static int FindHorizontalStartCol(CrosswordCell[,] grid, int row, int col)
        {
            int startCol = col;
            while (startCol > 0 && grid[row, startCol - 1] != null)
            {
                startCol--;
            }

            return startCol;
        }

        private static int FindVerticalStartRow(CrosswordCell[,] grid, int row, int col)
        {
            int startRow = row;
            while (startRow > 0 && grid[startRow - 1, col] != null)
            {
                startRow--;
            }

            return startRow;
        }

        private static string ReadHorizontalWord(CrosswordCell[,] grid, int row, int startCol)
        {
            int cols = grid.GetLength(1);
            StringBuilder sb = new StringBuilder();
            for (int c = startCol; c < cols && grid[row, c] != null; c++)
            {
                sb.Append(grid[row, c].c);
            }

            return sb.ToString();
        }

        private static string ReadVerticalWord(CrosswordCell[,] grid, int startRow, int col)
        {
            int rows = grid.GetLength(0);
            StringBuilder sb = new StringBuilder();
            for (int r = startRow; r < rows && grid[r, col] != null; r++)
            {
                sb.Append(grid[r, col].c);
            }

            return sb.ToString();
        }

        private static string BuildGridPreview(CrosswordCell[,] grid)
        {
            StringBuilder sb = new StringBuilder();
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    sb.Append(grid[r, c]?.c ?? '.');
                    if (c < cols - 1)
                    {
                        sb.Append(' ');
                    }
                }

                if (r < rows - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private void SaveDataWordJsonToAsset()
        {
            string defaultName = "Crossword_DataWord";
            string savePath = EditorUtility.SaveFilePanelInProject(
                "Save DataWord Json",
                defaultName,
                "json",
                "Chon noi luu file DataWord Json",
                LEVEL_DATA_PATH);

            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), savePath);
            absolutePath = Path.GetFullPath(absolutePath);
            File.WriteAllText(absolutePath, _dataWordJson, Encoding.UTF8);
            AssetDatabase.Refresh();

            SetResult($"Da luu DataWord JSON: {savePath}", MessageType.Info);
        }

        private void ClearOutput(bool keepMessage = false)
        {
            _generatedGrid = null;
            _generatedDataWords.Clear();
            _generatedExtraWords.Clear();
            _gridPreview = string.Empty;
            _dataWordJson = string.Empty;

            if (!keepMessage)
            {
                SetResult("Da xoa ket qua.", MessageType.Info);
            }
        }

        private void ShowTemporaryMessage(string message, MessageType type)
        {
            _resultMessage = message;
            _resultType = type;
            Repaint();
        }

        private void SetResult(string message, MessageType type)
        {
            _resultMessage = message;
            _resultType = type;
        }
    }
