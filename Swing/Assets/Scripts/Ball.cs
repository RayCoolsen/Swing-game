using UnityEngine;
using TMPro;

public class Ball : MonoBehaviour
{
    private int type;
    private int weight;

    [SerializeField] private int standardFontSize = 50;
    [SerializeField] private int smallFontSize = 44;

    private int startSortingLayer;
    SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Sprite[] ballsprites;
    [SerializeField] private TextMeshProUGUI textDisplay;

    [SerializeField] private AnimationCurve expandCurve;

    [SerializeField] private float collapseTime = 0.5f;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        startSortingLayer = m_SpriteRenderer.sortingOrder;
    }

    public void Initialize(int maxtype, int maxweight)
    {
        maxtype = Mathf.Min(maxtype, ballsprites.Length);
        type = Random.Range(0, maxtype);
        weight = Random.Range(1, maxweight + 1);
        m_SpriteRenderer.sprite = ballsprites[type];
        ChangeWeightText();
    }

    public void InitializeScale()
    {
        type = -1;
        weight = 0;
        //m_SpriteRenderer.enabled = false;
        textDisplay.enabled = false;
    }

    public int GetWeight()
    {
        return weight;
    }

    public void SetWeight(int changedweight)
    {
        weight = changedweight;
        ChangeWeightText();
    }

    public int GetBallType()
    {
        return type;
    }

    public void Pop()
    {
        //Play pop Animation/Shader???
        LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutQuint).setOnComplete(Kill);
    }

    public void Collapse(Vector3 target)
    {
        textDisplay.canvas.sortingOrder -= 3;
        m_SpriteRenderer.sortingOrder -= 3;
        LeanTween.move(gameObject, target, collapseTime).setEase(LeanTweenType.easeInExpo).setOnComplete(Kill);
    }

    public void Expanding()
    {
        m_SpriteRenderer.sortingOrder += 1;
        LeanTween.scale(gameObject, Vector3.one * 2, collapseTime + 0.2f).setEase(expandCurve).setOnComplete(NormalizeScale);
    }

    private void NormalizeScale()
    {
        gameObject.transform.localScale = Vector3.one;
        m_SpriteRenderer.sortingOrder = startSortingLayer;
    }

    private void ChangeWeightText()
    {
        textDisplay.text = weight.ToString();

        if (weight >= 100)
        {
            textDisplay.fontSize = smallFontSize;
        }
        else
        {
            textDisplay.fontSize = standardFontSize;
        }
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}
