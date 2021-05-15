using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchValues : MonoBehaviour {
    Material sourceMat = null;
    [SerializeField] MeshRenderer source = null;
    // [SerializeField] bool rendererIsSource = false;
    [SerializeField] MeshRenderer dest = null;
    [SerializeField] bool constantlyUpdate = true;


    private void Start() {
        //if (rendererIsSource) 
        sourceMat = source.material;
    }

    void Update() {
        if (!constantlyUpdate) return;
        Match();
    }

    private void Match() {
        dest.material.SetVector("_Offset", sourceMat.GetVector("_Offset"));

        dest.material.SetFloat("Base_Height", sourceMat.GetFloat("Base_Height"));
        dest.material.SetFloat("Base_Scale", sourceMat.GetFloat("Base_Scale"));
        dest.material.SetFloat("Base_Pow", sourceMat.GetFloat("Base_Pow"));

        dest.material.SetFloat("Mountain_Height", sourceMat.GetFloat("Mountain_Height"));
        dest.material.SetFloat("Mountain_Scale", sourceMat.GetFloat("Mountain_Scale"));
        dest.material.SetFloat("Mountain_Pow", sourceMat.GetFloat("Mountain_Pow"));

        dest.material.SetFloat("River_Height", sourceMat.GetFloat("River_Height"));
        dest.material.SetFloat("River_Scale", sourceMat.GetFloat("River_Scale"));
        dest.material.SetFloat("River_Pow", sourceMat.GetFloat("River_Pow"));
    }
}
