using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDisplayerSelfSetter : MonoBehaviour
{
    private void Start()
    {
        TextDisplayer td = GetComponent<TextDisplayer>();
        td.SetText(td.textMesh.text, true, true);
    }
}
