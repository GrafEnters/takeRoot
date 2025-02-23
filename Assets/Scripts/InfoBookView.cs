using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoBookView : MonoBehaviour {
    public static bool IsAutoOpenInfoPanel;

    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TextMeshProUGUI _nameText;

    [SerializeField]
    private ResourceGridView _resourceGridView;

    [SerializeField]
    private EquipmentGridUiView _equipmentGridView;

    [field: SerializeField]
    public Toggle _infoToggle;

    public void Init(InfoBookData data) {
        _icon.sprite = data.Icon;
        _nameText.text = data.Name;

        _resourceGridView.FillGrid(data.Resources);
        _equipmentGridView.gameObject.SetActive(false);
    }

    public void Init(SettlerData settlerData) {
        _icon.sprite = settlerData.InfoBookIcon;
        _nameText.text = settlerData.Name;

        _equipmentGridView.gameObject.SetActive(true);
        _equipmentGridView.Set(settlerData);
    }

    public void SetToggle(bool isOn) {
        _infoToggle.isOn = isOn;
    }

    public bool GetToggle() {
        return _infoToggle.isOn;
    }
}

public class InfoBookData {
    public Sprite Icon;
    public string Name;

    public List<ResourceData> Resources;
}