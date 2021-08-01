using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween : MonoBehaviour
{
    [Header ("Throw Tweening")]
    [SerializeField] private AnimationCurve positionThrowCurve;
    [SerializeField] private AnimationCurve scaleThrowCurveY;
    [SerializeField] private AnimationCurve scaleThrowCurveX;
    [SerializeField] private float tweenThrowSpeed = 1;

    [Header("Collapse Tweening")]
    [SerializeField] private float collapseTime = 0.5f;
    [SerializeField] private LeanTweenType collapseEaseType = LeanTweenType.linear;

    [Header("Player Tweening")]
    [SerializeField] private Vector3 playerSizeTween = Vector3.one * 0.5f;
    [SerializeField] private float playertweentime = 0.25f;
    [SerializeField] private AnimationCurve scalePlayerSizeCurve;


    ScaleGrid scaleGrid;

    private void Awake()
    {
        scaleGrid = GetComponent<ScaleGrid>();
    }

    public void PlayerSizeTween(GameObject player)
    {
        player.transform.localScale = Vector3.one;
        LeanTween.scale(player, playerSizeTween, playertweentime).setEase(scaleThrowCurveY);
    }

    // Start is called before the first frame update
    public void ThrowTween(GameObject ball, Vector3 tweenposition)
    {
        float tweenduration = (tweenposition - ball.transform.position).magnitude * tweenThrowSpeed;
        LeanTween.scaleY(ball, 2f, tweenduration).setEase(scaleThrowCurveY);
        LeanTween.scaleX(ball, 0f, tweenduration).setEase(scaleThrowCurveX);
        LeanTween.move(ball, tweenposition, tweenduration).setEase(positionThrowCurve).setOnComplete(FinishThrowTween);
    }

    private void FinishThrowTween()
    {
        scaleGrid.SetTweeningState(false);
        Debug.Log("Done Tweening!!!");
    }

    public void CollapseTween(int x, GameObject[] collapsSlot)
    {

        for (int y = 0; y < collapsSlot.Length; y++)
        {
            if (collapsSlot[y] != null)
                LeanTween.move(collapsSlot[y], new Vector3(x, y, 0), collapseTime).setEase(collapseEaseType);
        }
    }

    public float GetCollapseTime()
    {
        return collapseTime + .1f;
    }

    // Update is called once per frame
    public void YeetBallTween()
    {
        
    }
}
