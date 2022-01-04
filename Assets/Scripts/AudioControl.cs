using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioControl : MonoBehaviour
{
    public AudioMixer masterMixer;
    public Slider BGMSlider, EffectSlider;

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.HasKey("BGM"))
        {
            BGMSlider.value = PlayerPrefs.GetFloat("BGM");
            masterMixer.SetFloat("BGM", PlayerPrefs.GetFloat("BGM"));
        }
        else
        {
            BGMSlider.value = 0;
            masterMixer.SetFloat("BGM", 0);
        }

        if (PlayerPrefs.HasKey("Effect"))
        {
            EffectSlider.value = PlayerPrefs.GetFloat("Effect");
            masterMixer.SetFloat("Effect", PlayerPrefs.GetFloat("Effect"));
        }
        else
        {
            EffectSlider.value = 0;
            masterMixer.SetFloat("Effect", 0);
        }
    }

    /// <summary>
    /// b : true = BGM | false = Effect
    /// </summary>
    /// <param name="b"></param>
    public void AudioSlideControl(bool b)
    {
        float sound;
        string audioname;
        if (b)
        {
            sound = BGMSlider.value;
            audioname = "BGM";
        }
        else
        {
            sound = EffectSlider.value;
            audioname = "Effect";
        }

        if(sound == -40f)
        {
            masterMixer.SetFloat(audioname, -80);
            PlayerPrefs.SetFloat(audioname, -80);
        }
        else
        {
            masterMixer.SetFloat(audioname, sound);
            PlayerPrefs.SetFloat(audioname, sound);
        }
    }

    /// <summary>
    /// b : true = BGM | false = Effect
    /// </summary>
    /// <param name="b"></param>
    public void AudioBtnClick(bool b)
    {
        string audioname;
        if(b)
        {
            audioname = "BGM";
            if(BGMSlider.value != -40f)
            {
                masterMixer.SetFloat(audioname, -80);
                PlayerPrefs.SetFloat(audioname, -80);
                BGMSlider.value = -40f;
            }
            else
            {
                masterMixer.SetFloat(audioname, -20);
                PlayerPrefs.SetFloat(audioname, -20);
                BGMSlider.value = -20f;
            }
        }
        else
        {
            audioname = "Effect";
            if (EffectSlider.value != -40f)
            {
                masterMixer.SetFloat(audioname, -80);
                PlayerPrefs.SetFloat(audioname, -80);
                EffectSlider.value = -40f;
            }
            else
            {
                masterMixer.SetFloat(audioname, -20);
                PlayerPrefs.SetFloat(audioname, -20);
                EffectSlider.value = -20f;
            }
        }
    }
}
