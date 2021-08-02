using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeightDisplayFlicker : MonoBehaviour
{
    TextMeshProUGUI mytext;
    Color mycolor;
    Color alphacolor;
    bool weightwarning = false;
    [SerializeField] private AnimationCurve weightWarningCurve;
    [SerializeField] private float weightwarningtime = 0.5f;


    void Start()
    {
        mytext = GetComponentInChildren<TextMeshProUGUI>();
        mycolor = mytext.color;
        Color alphacolor = new Color(mycolor.r, mycolor.g, mycolor.b, 0);
    }


    public void TweenWeightWarning()
    {
        weightwarning = true;
        TweenWeightWarningProcess();
    }

    private void TweenWeightWarningProcess()
    {
        if (weightwarning)
        {
            LeanTween.value(gameObject, updateAlphaValueCallback, mycolor, alphacolor, weightwarningtime).setEase(weightWarningCurve).setOnComplete(TweenWeightWarningProcess);
        }
        else
        {
            mytext.color = mycolor;
        }
    }

    private void updateAlphaValueCallback(Color val)
    {
        mytext.color = val;
    }


    public void StopWeightWarning()
    {
        weightwarning = false;
    }
}
