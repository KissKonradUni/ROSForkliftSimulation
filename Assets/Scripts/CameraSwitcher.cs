using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera mainCamera;
    public CinemachineFreeLook freeCamera;

    private bool _isFreeCamera = false;
    
    // Update is called once per frame
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.C)) return;
        
        _isFreeCamera = !_isFreeCamera;
        
        freeCamera.Priority = _isFreeCamera ? 10 : 0;
        mainCamera.Priority = _isFreeCamera ? 0 : 10;

        Cursor.lockState = _isFreeCamera ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = _isFreeCamera;
    }
}
