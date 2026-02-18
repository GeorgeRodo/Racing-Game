using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    [Header("Graphics Settings")]
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;
    
    void Start()
    {
        LoadSettings();
    }
    
    // AUDIO 
    public void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
    
    public void OnMusicVolumeChanged(float value)
    {

        PlayerPrefs.SetFloat("MusicVolume", value);
    }
    
    public void OnSFXVolumeChanged(float value)
    {

        PlayerPrefs.SetFloat("SFXVolume", value);
    }
    
    // GRAPHICS
    public void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    public void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
    }
    
    // LOAD/SAVE
    void LoadSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVol;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol;
        
        AudioListener.volume = masterVol;
        
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        int quality = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        
        if (fullscreenToggle != null) fullscreenToggle.isOn = fullscreen;
        if (qualityDropdown != null) qualityDropdown.value = quality;
        
        Screen.fullScreen = fullscreen;
        QualitySettings.SetQualityLevel(quality);
    }
}