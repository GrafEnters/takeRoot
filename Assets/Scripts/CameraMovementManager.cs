using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovementManager : MonoBehaviour {
    [SerializeField]
    private float _cameraSpeed = 10f;

    [SerializeField]
    private float _edgeMargin = 20f;

    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private Rect _cameraBounds;

    private Vector2 _startPosition;
    private Vector2 _currentPosition;
    private bool _isDragging;
    private Camera _main;

    [Serializable]
    public enum MouseButton {
        RightButton,
        MiddleButton
    }

    [SerializeField]
    private MouseButton _mouseButtonToUse = MouseButton.RightButton;

    void Start() {
        _main = Camera.main!;  // Cache Camera.main once in Start
    }

    void Update() {
        TryDragCameraWithMouse();
        if (!_isDragging) {
            TryMoveCameraNearScreenEdge();
        }
        ClampCameraPosition();
    }

    private void TryMoveCameraNearScreenEdge() {
        Vector3 cameraMovement = Vector3.zero;
        Vector3 mousePosition = Input.mousePosition;

        if (mousePosition.x <= _edgeMargin) {
            cameraMovement.x = -1;
        } else if (mousePosition.x >= Screen.width - _edgeMargin) {
            cameraMovement.x = 1;
        }

        if (mousePosition.y <= _edgeMargin) {
            cameraMovement.y = -1;
        } else if (mousePosition.y >= Screen.height - _edgeMargin) {
            cameraMovement.y = 1;
        }

        if (cameraMovement != Vector3.zero) {
            _cameraTransform.position += cameraMovement * _cameraSpeed * Time.deltaTime;
        }
    }

    private void TryDragCameraWithMouse() {
        bool isPressed = _mouseButtonToUse == MouseButton.RightButton 
                         ? Mouse.current.rightButton.isPressed 
                         : Mouse.current.middleButton.isPressed;

        if (isPressed) {
            if (!_isDragging) {
                _startPosition = Mouse.current.position.ReadValue();
                _isDragging = true;
            }

            _currentPosition = Mouse.current.position.ReadValue();
            DragCamera();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame || Mouse.current.middleButton.wasReleasedThisFrame) {
            _isDragging = false;
        }
    }

    private void DragCamera() {
        Vector2 delta = _currentPosition - _startPosition;
        Vector3 movement = new Vector3(delta.x, delta.y, 0);
        _cameraTransform.position -= movement * Time.deltaTime;

        _startPosition = _currentPosition;
    }

    private void ClampCameraPosition() {
        float cameraWidth = _main.orthographicSize * 2 * _main.aspect;
        float cameraHeight = _main.orthographicSize * 2;

        float clampedX = Mathf.Clamp(_cameraTransform.position.x, _cameraBounds.xMin + cameraWidth / 2, _cameraBounds.xMax - cameraWidth / 2);
        float clampedY = Mathf.Clamp(_cameraTransform.position.y, _cameraBounds.yMin + cameraHeight / 2, _cameraBounds.yMax - cameraHeight / 2);
        _cameraTransform.position = new Vector3(clampedX, clampedY, _cameraTransform.position.z);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3(_cameraBounds.center.x, _cameraBounds.center.y, 0),
            new Vector3(_cameraBounds.width, _cameraBounds.height, 0));
    }
}
