using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RaidDropdown : MonoBehaviour
{
    [Header("References")]
    public Button toggleButton;
    public RectTransform panel;       // El contenedor con Image + Layout
    public RectTransform content;     // Donde instanciar los items (puede ser el mismo panel)
    public GameObject itemPrefab;     // Botón/TMP desactivado usado como plantilla
    public Canvas rootCanvas;

    [Header("Behaviour")]
    public float yOffset = 6f;
    public Vector2 minSize = new Vector2(200f, 0f);
    public Vector2 maxSize = new Vector2(340f, 360f); // límite de altura

    public bool closeOnOutsideClick = true;
    public bool closeOnEscape = true;

    private bool _open;
    private Camera _uiCamera;
    private RectTransform _canvasRect;

    void Awake()
    {
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        _uiCamera = rootCanvas != null ? rootCanvas.worldCamera : null;
        _canvasRect = rootCanvas != null ? rootCanvas.transform as RectTransform : null;

        if (panel != null) panel.gameObject.SetActive(false);
        if (toggleButton != null) toggleButton.onClick.AddListener(Toggle);
    }

    void OnDestroy()
    {
        if (toggleButton != null) toggleButton.onClick.RemoveListener(Toggle);
    }

    void Update()
    {
        if (!_open) return;

        if (closeOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (closeOnOutsideClick && Input.GetMouseButtonDown(0))
        {
            Vector2 mouse = Input.mousePosition;
            bool overPanel   = panel != null && RectTransformUtility.RectangleContainsScreenPoint(panel, mouse, _uiCamera);
            bool overButton  = toggleButton != null && RectTransformUtility.RectangleContainsScreenPoint(toggleButton.transform as RectTransform, mouse, _uiCamera);
            if (!overPanel && !overButton)
            {
                Close();
                return;
            }
        }
    }

    public void Toggle()
    {
        if (_open) Close();
        else Open();
    }

    public void Open()
    {
        if (panel == null || _canvasRect == null) return;

        panel.gameObject.SetActive(true);
        _open = true;

        // Asegurar un ancho mínimo igual al botón
        float targetWidth = minSize.x;
        var btnRT = toggleButton != null ? toggleButton.transform as RectTransform : null;
        if (btnRT != null) targetWidth = Mathf.Max(targetWidth, btnRT.rect.width);

        // Forzar layout para tener tamaño real antes de clamping
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        var panelLE = panel.GetComponent<LayoutElement>();
        if (panelLE != null)
        {
            panelLE.minWidth = targetWidth;
            panelLE.preferredWidth = Mathf.Max(panelLE.preferredWidth, targetWidth);
        }

        // Reposicionar bajo el botón
        PositionPanelUnderButton();

        // Limitar altura
        ClampToCanvasAndLimitHeight();
    }

    public void Close()
    {
        if (panel == null) return;
        panel.gameObject.SetActive(false);
        _open = false;
    }

    private void PositionPanelUnderButton()
    {
        if (toggleButton == null || panel == null || _canvasRect == null) return;

        var btnRT = toggleButton.transform as RectTransform;
        Vector3[] corners = new Vector3[4];
        btnRT.GetWorldCorners(corners);
        // corners: 0=BL, 1=TL, 2=TR, 3=BR
        Vector3 bottomCenterWorld = (corners[0] + corners[3]) * 0.5f;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect,
            RectTransformUtility.WorldToScreenPoint(_uiCamera, bottomCenterWorld),
            _uiCamera,
            out localPoint);

        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = localPoint + new Vector2(0f, -yOffset);
    }

    private void ClampToCanvasAndLimitHeight()
    {
        // Asegurar que no se salga de la pantalla y limitar altura
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        var canvasRect = _canvasRect.rect;
        var size = panel.rect.size;

        // Limitar altura
        float clampedHeight = Mathf.Min(size.y, maxSize.y);
        float width = Mathf.Clamp(size.x, minSize.x, maxSize.x);

        // Aplicar tamaño con LayoutElement si está
        var le = panel.GetComponent<LayoutElement>();
        if (le != null)
        {
            le.preferredHeight = clampedHeight;
            le.preferredWidth = width;
        }
        else
        {
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clampedHeight);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        // Recalcular tras cambio de tamaño
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        size = panel.rect.size;

        // Clamping posición dentro del canvas
        Vector2 pos = panel.anchoredPosition;
        float halfW = size.x * 0.5f;
        float topY = pos.y; // pivot y=1, anclado al centro del canvas
        float bottomY = pos.y - size.y;

        float canvasHalfW = canvasRect.width * 0.5f;
        float canvasHalfH = canvasRect.height * 0.5f;

        // Clamp X
        pos.x = Mathf.Clamp(pos.x, -canvasHalfW + halfW + 8f, canvasHalfW - halfW - 8f);
        // Clamp Y (que no se vaya por abajo)
        float minY = -canvasHalfH + 8f;
        if (bottomY < minY)
        {
            float delta = minY - bottomY;
            pos.y += delta;
        }

        panel.anchoredPosition = pos;
    }

    // API para poblar
    public void ClearItems()
    {
        if (content == null) return;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var ch = content.GetChild(i);
            if (ch.gameObject == itemPrefab) continue; // por si el prefab vive dentro
            Destroy(ch.gameObject);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void AddItem(string text, UnityAction onClick)
    {
        if (itemPrefab == null || content == null) return;

        GameObject go = Instantiate(itemPrefab, content);
        go.SetActive(true);
        var item = go.GetComponent<RaidDropdownItem>();
        if (item == null) item = go.AddComponent<RaidDropdownItem>();
        item.Init(text, () =>
        {
            onClick?.Invoke();
            Close(); // cerrar al seleccionar
        });
    }
}