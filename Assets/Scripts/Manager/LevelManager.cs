using Brute_Force_Algorithm;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LevelManager : MonoBehaviour
{
    private string _fileName = "Level/words_2";
    private HashSet<string> _setLetter; //Lưu trữ kí tự in hoa 
    #region sinh Crossword

    private float _lenRow; //Tính theo pixel
    private float _lenCol;
    [SerializeField] private RectTransform _girdParent; // đây là parent của các ô nằm trong nó

    private Dictionary<string, List<Cell>> _dicWordCells;
    public int GetSumLetter()
    {
        return _setLetter.Count;
    }
    public Dictionary<string,List<Cell>> GetDicWordCells()
    {
        return _dicWordCells;
    }
    public void InitLevel()
    {
        _setLetter = new HashSet<string>();
        InitGrid();
        ArrageLettersInCircle();
    }
    private void InitGrid()
    {
        //Lấy dữ liệu từ file và xử lí nó dùng thuật toán brutforce
        string[] txt = FileHandle.LoadText(_fileName).Split("\r\n");
       // string[] arr = { "clan", "nova", "oval", "volcano", "vocal", "cool", "coal", "van", "loan", "cola", "can", "con" };
        List<string> words = new List<string>();
        _dicWordCells = new Dictionary<string, List<Cell>>();
        foreach (string s in txt)
        {
            string tmp=s.ToUpper();
            words.Add(tmp);
            _dicWordCells.Add(tmp, new List<Cell>());
        }
        BuildCrossword cw = new BuildCrossword(words, words);
        int tries = 10;
        CrosswordCell[,] _crosswordCells = cw.GetSquareGrid(tries);
        //Cho vào các ô trên UI
        AddToCells(_crosswordCells);
    }
    private void AddToCells(CrosswordCell[,] _crosswordCells)
    {
        if (_crosswordCells == null) return;
        _lenRow = _girdParent.rect.height; 
        _lenCol= _girdParent.rect.width; 
        // khởi tạo kích thước mỗi ô
        float lenCellx = _lenRow / (_crosswordCells.GetLength(0)+1);
        float lenCelly=_lenCol/(_crosswordCells.GetLength(1)+1);
        float minLenCell= Math.Min(lenCelly, lenCellx);

        float offsetX = - (_lenRow/2 - minLenCell);
        float offsetY =  _lenCol/2 - minLenCell;
        Debug.Log(offsetX + " " + offsetY); 
        string s = "";
        // Đặt kí tự vào trong ô
        for (int i = 0; i < _crosswordCells.GetLength(0); i++)
        {
            for (int j = 0; j < _crosswordCells.GetLength(1); j++)
            {
                if (_crosswordCells[i, j] == null)
                {
                    s += '-';
                    continue;
                }
                //Tạo ô 
                Cell cell = ObjectPool.instance.GetObjet("cell").GetComponent<Cell>();
                cell.SetCell(_crosswordCells[i, j].c, i * minLenCell + offsetX,offsetY - j * minLenCell, minLenCell);
                s += _crosswordCells[i, j].c;
                _setLetter.Add(_crosswordCells[i,j].c.ToString().ToUpper());
                //Thêm ô vào dictionary 
                // Kiểm tra ngang
                string misWord = "";
                for (int k = j - 1; k >= 0; k--)  //Lùi về sau để kiểm tra từ còn thiếu
                {
                    if (_crosswordCells[i, k] == null) { break; }
                    misWord = _crosswordCells[i, k].c + misWord;
                }
                for (int k = j; k < _crosswordCells.GetLength(1); k++)
                {
                    if (_crosswordCells[i, k] == null) { break; }
                    misWord += _crosswordCells[i, k].c;
                }
                if (misWord.Length > 1)
                {
                    Debug.Log($"{_crosswordCells[i, j].wordParent} + {_crosswordCells[i, j].c} + mis {misWord}");
                    _dicWordCells[misWord].Add(cell);
                }

                //Kiểm tra dọc
                misWord = "";
                for (int k = i - 1; k >= 0; k--)  //Lùi về sau để kiểm tra từ còn thiếu
                {
                    if (_crosswordCells[k, j] == null) { break; }
                    misWord = _crosswordCells[k, j].c + misWord;
                }
                for (int k = i; k < _crosswordCells.GetLength(0); k++)
                {
                    if (_crosswordCells[k, j] == null) { break; }
                    misWord += _crosswordCells[k, j].c;
                }
                if (misWord.Length > 1)
                {
                    Debug.Log($"{_crosswordCells[i, j].wordParent} + {_crosswordCells[i, j].c} + mis {misWord}");

                    _dicWordCells[misWord].Add(cell);
                }
            }
            s += '\n';
        }
        Debug.Log(s);
        //TestDic();
    }

    //Test thử in xem có dictionary không
    private void TestDic()
    {
        string tmp = "";
        foreach (var item in _dicWordCells)
        {
            string s = item.Key + " ki tu la: ";
            tmp = item.Key;
            foreach (var cell in item.Value)
            {
                s += cell.GetText() + " ";
            }
            Debug.Log(s);
        }
        foreach(var cell in _dicWordCells[tmp])
        {
            cell.OnLetter();
        }
    }
    #endregion

    #region Sắp xếp kí tự quanh hình tròn
    private float _radius = 1.3f;
    private void ArrageLettersInCircle()
    {
        float angleStep = Mathf.PI * 2f / _setLetter.Count;
        int cnt=0;
        foreach (string s in _setLetter) { 
            float angle = cnt * angleStep ;
            float x = Mathf.Cos(angle) * _radius;
            float y = Mathf.Sin(angle) * _radius;

            LetterText letter = ObjectPool.instance.GetObjet("letter").GetComponent<LetterText>();
            letter.SetTransform(y, x, s);
            cnt++;
        }
    }
    #endregion
}
