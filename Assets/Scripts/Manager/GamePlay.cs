using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamePlay : Singleton<GamePlay>
{
    [SerializeField] private LineRenderer _lineRenderer;
    private bool isDragging;
    private Vector3 endPoint;
    [SerializeField] private LayerMask _layer;

    [SerializeField]private List<Transform> points = new List<Transform>(); //Các điểm kí tự chạm phải
    private int sumPos = 0;

    [SerializeField] private LevelManager _levelManager;
    private string _word;

    [SerializeField] private GameObject _objShowWord;
    [SerializeField] private TextMeshProUGUI _txtShowWord;
    private bool _timeShowWord;
    private Dictionary<string, List<Cell>> _dicWordCells;
    private void Start()
    {
        // SetUpLine(tfrms);
        _levelManager.InitLevel();
        sumPos = _levelManager.GetSumLetter();
        _dicWordCells=_levelManager.GetDicWordCells();
    }
    private void Update()
    {
        OnTouch();
    }
    public void SetSumPos(int maxSize)
    {
        sumPos = maxSize;
    }
    private void OnTouch()
    {
        if (sumPos <= 0) { return; }
        if (_timeShowWord) return;
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, _layer);
            if (hit.collider != null)
            {
                isDragging = true;

                points.Clear();
                points.Add(hit.collider.transform);
                _lineRenderer.positionCount = 1;
                _lineRenderer.SetPosition(0, hit.collider.transform.position);
                _word = hit.collider.gameObject.GetComponent<LetterText>().txtLetter.text.ToString();
                _objShowWord.SetActive(true);
                _objShowWord.transform.localScale = Vector3.one;
                _txtShowWord.text = _word;
            }
        }
        if (isDragging && points.Count<sumPos)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, _layer);
            if (hit.collider != null)
            {
                Transform hitPoint = hit.collider.transform;
                // Kiểm tra xem vị trí vừa đi qua có trùng với vị trí đã lưu ở gần cuối cùng hay không, nếu trùng thì xoá nó đi
                if (points.Count >= 2)
                {
                    if (points[points.Count - 2] == hitPoint)
                    {
                        _lineRenderer.positionCount -= 2;
                        points.RemoveAt(points.Count - 1);
                        _word.Substring(_word.Length - 1);
                        _txtShowWord.text = _word;
                    }
                }
                if (points.Contains(hitPoint))
                {
                    return;
                }
                if (points.Count == 0 || points[points.Count - 1] != hitPoint)
                {
                    points.Add(hitPoint);
                    _lineRenderer.positionCount = points.Count;
                   _lineRenderer.SetPosition(points.Count - 1, hitPoint.position);
                    _word += hit.collider.gameObject.GetComponent<LetterText>().txtLetter.text.ToString();
                    _txtShowWord.text = _word;
                }

            }
            else
            {
                // Nếu không va chạm, cập nhật vị trí điểm cuối cùng của line
                _lineRenderer.positionCount = points.Count + 1;
                _lineRenderer.SetPosition(points.Count, mousePos);
            }
        }
       

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            _lineRenderer.positionCount = 0;
            points.Clear();
            _timeShowWord = true;
            _objShowWord.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.Flash).OnComplete(() =>
            {
                _timeShowWord = false;
                _objShowWord.SetActive(false);
            });
            Debug.Log(_word);
            if (_dicWordCells.ContainsKey(_word))
            {
                foreach(var cell in _dicWordCells[_word])
                {
                    cell.OnLetter();
                }
            }
        }
    }
    public void SetUpLine(Transform[] points)
    {
        _lineRenderer.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++) {
            _lineRenderer.SetPosition(i, points[i].position);   
        }
    }
}
