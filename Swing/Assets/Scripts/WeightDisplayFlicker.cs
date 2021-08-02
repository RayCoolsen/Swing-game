using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeightDisplayFlicker : MonoBehaviour
{
    TextMeshProUGUI mytext;
    Color mycolor;
    Color alphacolor;
    [SerializeField] private AnimationCurve weightWarningCurve;
    [SerializeField] private float weightwarningtime = 0.5f;


    private void Start()
    {
        mytext = GetComponentInChildren<TextMeshProUGUI>();
        mycolor = mytext.color;
        Color alphacolor = new Color(mycolor.r, mycolor.g, mycolor.b, 0);
    }


    public void TweenWeightWarning()
    {
        LeanTween.value(gameObject, updateAlphaValueCallback, mycolor, alphacolor, weightwarningtime).setEase(weightWarningCurve).setOnComplete(TweenWeightWarning);
    }

    private void updateAlphaValueCallback(Color val)
    {
        mytext.color = val;
    }


    public void StopWeightWarning()
    {
        LeanTween.cancel(gameObject);
        mytext.color = mycolor;
    }
}
