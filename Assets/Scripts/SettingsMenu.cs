using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Text masterVolumeValueText;

    void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = AudioSettingsRuntime.GetMasterVolume();
        }

        UpdateValueText();
    }

    void OnEnable()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
    }

    void OnDisable()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }
    }

    public void Toggle()
    {
        if (panel == null)
        {
            return;
        }

        bool next = !panel.activeSelf;
        panel.SetActive(next);
        if (next && masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioSettingsRuntime.GetMasterVolume();
            UpdateValueText();
        }
    }

    public void Open()
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(true);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioSettingsRuntime.GetMasterVolume();
        }

        UpdateValueText();
    }

    public void Close()
    {
        if (panel == null)
        {
            return;
        }

        panel.SetActive(false);
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioSettingsRuntime.SetMasterVolume(value);
        UpdateValueText();
    }

    void UpdateValueText()
    {
        if (masterVolumeValueText == null)
        {
            return;
        }

        int percent = Mathf.RoundToInt(AudioSettingsRuntime.GetMasterVolume() * 100f);
        masterVolumeValueText.text = percent.ToString() + "%";
    }
}

