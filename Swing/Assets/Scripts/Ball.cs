using UnityEngine;
using TMPro;

public class Ball : MonoBehaviour
{
    private int type;
    private int weight;

    SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Sprite[] ballsprites;
    [SerializeField] private TextMeshProUGUI textDisplay;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int maxtype, int maxweight)
    {
        type = Random.Range(0, maxtype+1);
        weight = Random.Range(1, maxweight + 1);
        m_SpriteRenderer.sprite = ballsprites[type];
        textDisplay.text = weight.ToString();
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

    public int GetBallType()
    {
        return type;
    }

    public void Pop()
    {
        //Play pop Animation/Shader???
        Destroy(gameObject);
    }
}
