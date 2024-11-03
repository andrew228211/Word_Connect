using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _txtLetter;
    [SerializeField] private GameObject _objLetter;
    [SerializeField] private Image _img;
    [SerializeField] private Color _color;
    [SerializeField] private RectTransform _rectTransform;
    
    public void Reset()
    {
        _txtLetter.text = "";
        _img.color= Color.white;
    }
    public void SetCell(char _txtLetter, float x, float y, float width)
    {
        this._txtLetter.text= _txtLetter.ToString();
        _objLetter.SetActive(false);
        _rectTransform.anchoredPosition= new Vector2(x,y);
        _img.color = Color.white;
       _rectTransform.sizeDelta= new Vector2(width,width);
    }
    public void OnLetter()
    {
        _objLetter.SetActive(true);
        _img.color = _color;
    }
    public string GetText()
    {
        return _txtLetter.text;
    }
}
