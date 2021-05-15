using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateMaterial : MonoBehaviour {
    public enum PropertyType {Float, VectorX, VectorY, ColorR, ColorG, ColorB, ColorA};

    [SerializeField] MeshRenderer rend = null;
    [SerializeField] Slider slider = null;
    [SerializeField] PropertyType propertyType = 0;
    [SerializeField] string property = "";

    Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mat = rend.material;
        slider.value = GetProperty();
    }

    public void SetProperty(float value) {
        mat = rend.material;
        switch (propertyType) {
            case PropertyType.Float:
                mat.SetFloat(property, value);
                break;
            case PropertyType.VectorX:
                Vector4 originalVec = mat.GetVector(property);
                originalVec.x = value;
                mat.SetVector(property, originalVec);
                break;
            case PropertyType.VectorY:
                originalVec = mat.GetVector(property);
                originalVec.y = value;
                mat.SetVector(property, originalVec);
                break;
            case PropertyType.ColorR:
                Color col = mat.GetColor(property);
                col.r = value;
                mat.SetVector(property, col);
                break;
            case PropertyType.ColorG:
                col = mat.GetColor(property);
                col.g = value;
                mat.SetVector(property, col);
                break;
            case PropertyType.ColorB:
                col = mat.GetColor(property);
                col.b = value;
                mat.SetVector(property, col);
                break;
            default:
            case PropertyType.ColorA:
                col = mat.GetColor(property);
                col.a = value;
                mat.SetVector(property, col);
                break;
        }
    }
    private float GetProperty() {
        mat = rend.material;
        switch (propertyType) {
            case PropertyType.Float:
                return mat.GetFloat(property);
            case PropertyType.VectorX:
                return mat.GetVector(property).x;
            case PropertyType.VectorY:
                return mat.GetVector(property).y;
            case PropertyType.ColorR:
                return mat.GetColor(property).r;
            case PropertyType.ColorG:
                return mat.GetColor(property).g;
            case PropertyType.ColorB:
                return mat.GetColor(property).b;
            default:
            case PropertyType.ColorA:
                return mat.GetColor(property).a;
        }
    }
}
