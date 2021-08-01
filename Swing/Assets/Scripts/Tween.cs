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

    [Header("Yeet Tweening")]
    [SerializeField] private float tweenUpYeetSpeed = 1f;
    [SerializeField] private AnimationCurve positionUpYeetCurve;
    [SerializeField] private AnimationCurve scaleUpYeetCurveY;
    [SerializeField] private AnimationCurve scaleUpYeetCurveX;

    [SerializeField] private Transform leftOut;
    [SerializeField] private Transform rightOut;
    [SerializeField] private AnimationCurve scaleSideYeetCurveY;
    [SerializeField] private AnimationCurve scaleSideYeetCurveX;


    [Header("Player Tweening")]
    [SerializeField] private Vector3 playerSizeTween = Vector3.one * 0.5f;
    [SerializeField] private float playertweentime = 0.2f;
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
        LeanTween.move(ball, tweenposition, tweenduration).setEase(positionThrowCurve).setOnComplete(FinishBallTween);
    }

    private void FinishBallTween()
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
    public void YeetBallTween(GameObject ball, Vector3 tweenposition, int fullrounds, int direction)
    {
        //Starting with yeeting up
        float tweenDurationUp = (tweenposition.y - ball.transform.position.y) * tweenUpYeetSpeed;
        Vector3 tweenUpPosition = new Vector3(ball.transform.position.x, tweenposition.y, 0);

        LeanTween.scaleY(ball, 2f, tweenDurationUp).setEase(scaleUpYeetCurveY);
        LeanTween.scaleX(ball, 0.5f, tweenDurationUp).setEase(scaleUpYeetCurveX);
        LeanTween.move(ball, tweenUpPosition, tweenDurationUp).setEase(positionUpYeetCurve).setOnComplete(() => YeetSidewaysTween(ball, tweenposition, fullrounds, direction));
        //.setOnComplete(FinishBallTween);
    }

    private void YeetSidewaysTween(GameObject ball, Vector3 tweenposition, int fullrounds, int direction)
    {
        Transform wayout;
        float yeetSideDuration;

        if (direction < 0)
        {
            wayout = leftOut;
        }
        else
        {
            wayout = rightOut;
        }

        Debug.Log("fullrounds: " + fullrounds);

        if (fullrounds > 0)
        {
            yeetSideDuration = Mathf.Abs((wayout.transform.position.x - ball.transform.position.x) * tweenUpYeetSpeed);
            LeanTween.scaleY(ball, 0.5f, yeetSideDuration).setEase(scaleSideYeetCurveY);
            LeanTween.scaleX(ball, 2f, yeetSideDuration).setEase(scaleSideYeetCurveX);
            LeanTween.move(ball, wayout, yeetSideDuration).setEase(LeanTweenType.linear).setOnComplete(() => YeetSidewaysTween(ball, tweenposition, fullrounds, direction));
        }

        Debug.Log("It works ???" + tweenposition);
    }
}
