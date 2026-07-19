using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    public Slider sliderVolumen;

    void Start()
    {
        float volumenGuardado = PlayerPrefs.GetFloat("Volumen", 1f);

        if (sliderVolumen != null)
        {
            sliderVolumen.value = volumenGuardado;
        }

        AudioListener.volume = volumenGuardado;
    }

    public void CambiarVolumen()
    {
        if (sliderVolumen == null)
            return;

        float volumen = sliderVolumen.value;
        AudioListener.volume = volumen;
        PlayerPrefs.SetFloat("Volumen", volumen);
        PlayerPrefs.Save();
    }
}
