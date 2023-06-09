using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text writeText;
    public void SetTrack(int track)
    {
        Setting.track = track;
        if(track == 1)
        {
            Setting.maxTime = 60;
        }
        else if(track == 2)
        {
            Setting.maxTime = 120;
        }
        else if (track == 3)
        {
            Setting.maxTime = 150;
        }
        else if(track == 4)
        {
            Setting.maxTime = 150;
        }
    }

    public void ChangeSensorsCount()
    {
        Setting.sensorCount = int.Parse(dropdown.options[dropdown.value].text);
    }


    public void StartSimulation()
    {
        SceneManager.LoadScene("Track" + Setting.track);
    }    

    public void ChangePopulationSize()
    {
        Setting.populationSize = ((int)slider.value);
        writeText.text = ((int)slider.value).ToString();
    }

    public void ChangeMutationProbability()
    {
        float value = Mathf.Round(slider.value * 100f) / 100f;
        Setting.mutationProbability = value;
        writeText.text = value.ToString();
    }

    public void ChangeSensorsLength()
    {
        float value = Mathf.Round(slider.value * 100f) / 100f;
        Setting.sensorLength = value;
        writeText.text = value.ToString();
    }

    public void ChangeHiddenLayerSize()
    {
        Setting.hiddenLayerSize = int.Parse(dropdown.options[dropdown.value].text);
    }

    public void ChangeHiddenLayerCount()
    {
        Setting.hiddenLayersCount = int.Parse(dropdown.options[dropdown.value].text);
    }

    public void ChangeParentsCount()
    {
        Setting.parentsCount = int.Parse(dropdown.options[dropdown.value].text);
    }
}
