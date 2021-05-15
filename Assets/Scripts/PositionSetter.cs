using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionSetter : MonoBehaviour
{
    [SerializeField] Transform target = null;
    [SerializeField] Slider slider = null;
    void Start()
    {
        slider.value = target.position.y;
    }

    public void SetPos(float y) {
        target.position = Vector3.up * y;
    }
}
