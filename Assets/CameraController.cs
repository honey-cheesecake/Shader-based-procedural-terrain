using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {
    [SerializeField] float sens = -5;
    [SerializeField] float damp = 0.3f;
    [SerializeField] float startFOV = 40;
    [SerializeField] float maxFOV = 55;
    [SerializeField] float minFOV = 15;

    [SerializeField] CinemachineFreeLook cinemachine = null;
    [SerializeField] Camera mainCamera = null;
    [SerializeField] Camera childCamera = null;

    float targetFOV = 0;
    float currFOV = 0;
    bool cinEnabled = false;
    // Start is called before the first frame update
    void Start() {
        cinemachine.enabled = false;
        cinemachine.m_CommonLens = true;
        mainCamera.fieldOfView = currFOV;
        cinemachine.m_Lens.FieldOfView = startFOV;
        targetFOV = startFOV;
    }

    // Update is called once per frame
    void Update() {
        // only get input if mouse is not on top of UI
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
                cinemachine.enabled = true;
                cinEnabled = true;
            }
            if (Input.GetMouseButtonUp(0)) {
                Cursor.lockState = CursorLockMode.None;
                cinemachine.enabled = false;
                cinEnabled = false;
            }
            targetFOV += Input.mouseScrollDelta.y * sens;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }
        currFOV = Mathf.Lerp(currFOV, targetFOV, damp * Time.deltaTime);

        if (cinEnabled) {
            cinemachine.m_Lens.FieldOfView = currFOV;
        } else {
            mainCamera.fieldOfView = currFOV;
        }
        childCamera.fieldOfView = currFOV;
    }
}
