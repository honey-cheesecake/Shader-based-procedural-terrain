using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour {
    static Material mat = null;
    static bool enableWater = true;
    [SerializeField] MeshRenderer meshRenderer = null;
    [SerializeField] GameObject water = null;
    [SerializeField] Toggle waterToggleUI = null;

    void Start() {
        if (mat != null) meshRenderer.material = mat;
        water.SetActive(enableWater);
        waterToggleUI.isOn = enableWater;
    }
    public void SetMat(Material newMat) {
        mat = newMat;
    }
    public void SetWaterEnabled(bool b) {
        enableWater = b;
    }
    public void ReloadScene() {
        SceneManager.LoadScene(0);
    }
}
