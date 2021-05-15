using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateMaterialColor : MonoBehaviour
{
    [SerializeField] MeshRenderer rend = null;
    [SerializeField] FlexibleColorPicker picker = null;
    [SerializeField] string property = "";

    Material mat;

    void Start() {
        mat = rend.material;
        picker.color = mat.GetColor(property);
    }

    public void UpdateMat() {
        mat = rend.material;
        mat.SetColor(property, picker.color);
    }
}
