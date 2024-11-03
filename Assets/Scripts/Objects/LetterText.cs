using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterText : MonoBehaviour
{
    public TextMeshProUGUI txtLetter;
    public SpriteRenderer sprCell;
    [SerializeField]private Transform tfrm;
    public void SetTransform(float x,float y,string letter)
    {
        tfrm.localPosition = new Vector3(x,y,0);
        txtLetter.text=letter;
    }

}
