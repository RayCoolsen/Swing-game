using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScaleGrid : MonoBehaviour
{
    // only even numbers for the width!
    private static int width = 8;
    private static int height = 9;
    private static int supheight = 2;
    [SerializeField] private GameObject ballprefab;
    [SerializeField] private GameObject scaleDisplayprefab;
    [SerializeField] private GameObject player;
    private SpriteRenderer p_SpriteRenderer;
    private GameObject playerball;
    private int playerpos = 0;
    GameObject[,] scaleGrid = new GameObject[width, height];
    GameObject[,] supplyGrid = new GameObject[width, supheight];
    GameObject[] scaleDisplay = new GameObject[width];
    TextMeshProUGUI[] scaleDisplayText = new TextMeshProUGUI[width];
    private int[] slotweight = new int[width];
    [SerializeField] private int[] slotheight = new int[width];

    [SerializeField] private int startmaxweight = 5;
    [SerializeField] private int startmaxtype = 3;


    private Tween tween;
    private bool tweening = false;

    enum ScalePosition // Which side is down
    {
        Left,
        Balance,
        Right
    }

    ScalePosition[] scalePositions = new ScalePosition[width/2];


    private void Awake()
    {
        p_SpriteRenderer = player.GetComponent<SpriteRenderer>();
        tween = GetComponent<Tween>();
    }

    private void Start()
    {
        player.transform.position = new Vector3(0, height -0.5f, 0);

        for (int i = 0; i < scalePositions.Length; i++)
        {
            scalePositions[i] = ScalePosition.Balance;
        }

        for (int x = 0; x < width; x++)
        {
            // Instantiate the Scale Balls
            var newscaleball = Instantiate(ballprefab, new Vector3(x, 0, 0), Quaternion.identity);
            newscaleball.GetComponent<Ball>().InitializeScale();
            scaleGrid[x, 0] = newscaleball;
            slotheight[x] = 1;

            // Instantiate the Scale Display
            var newscale = Instantiate(scaleDisplayprefab, new Vector3(x, -1, 0), Quaternion.identity);
            scaleDisplay[x] = newscale;
            scaleDisplayText[x] = newscale.GetComponentInChildren<TextMeshProUGUI>();
            slotweight[x] = 0;

            // Instantiate the Supply Balls
            for (int y = 0; y < supheight; y++)
            {
                var newsupball = Instantiate(ballprefab, new Vector3(x, y + height + 1, 0), Quaternion.identity);
                newsupball.GetComponent<Ball>().Initialize(startmaxtype, startmaxweight);
                supplyGrid[x, y] = newsupball;
            }
        }
    }

    private void Update()
    {
        //+++ WeightScaleTipWarning
        if (Input.GetKeyDown(KeyCode.RightArrow) && playerpos < width-1)
        {
            tween.PlayerSizeTween(player);
            player.transform.position += Vector3.right;
            playerpos++;
            if(playerball != null)
                playerball.transform.position = player.transform.position;
            HeightWarning(playerpos);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && playerpos > 0)
        {
            tween.PlayerSizeTween(player);
            player.transform.position += Vector3.left;
            playerpos--;
            if (playerball != null)
                playerball.transform.position = player.transform.position;
            HeightWarning(playerpos);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Throwing Ball
            if(playerball!=null)
                StartCoroutine(ThrowingBall(playerpos, playerball));

            //Loading New Ball
            playerball = supplyGrid[playerpos, 0];
            playerball.transform.position = player.transform.position;

            // New Ball Supply
            SupplyReload(playerpos);
            ScaleHeight(playerpos);
            HeightWarning(playerpos);
        }

    }

    IEnumerator ThrowingBall(int slot, GameObject ball)
    {
        int y = ScaleHeight(slot);
        if (y < height)
        {
            scaleGrid[slot, y] = ball;

            //scaleGrid[slot, y].transform.position = new Vector3(slot, y, 0);
            tweening = true;
            tween.ThrowTween(scaleGrid[slot, y], new Vector3(slot, y, 0));
            SlotWeight(slot);

            yield return new WaitWhile(() => tweening);

            Debug.Log("StartCalculating");

            while (CheckMatches())
            {
                yield return new WaitForSeconds(0.5f); // Give Balls time to pop ???
                CollapseScaleGrid();
                yield return new WaitForSeconds(tween.GetCollapseTime()); // Waiting for the Collapse to finish
            }

            //BallCompression


            for (int x = 0; x < width; x++)
            {
                ScaleHeight(x);
                SlotWeight(x);
            }

            TiltScales();

            //Test
            for (int x = 0; x < width; x++)
            {
                ScaleHeight(x);
                SlotWeight(x);

                if (slotheight[x] >= height)
                {
                    Debug.Log("GameOver!");
                }
            }


            #region Plan???
            //Loop1
            // Loop2
            //  Check Matches - Exit2
            //   Flood balls
            //  Drop Balls
            // Compressing the balls
            // Slot Weight for all effected slots?
            // Tilt scale
            //Exit1?
            // Calculate impact (??? What does that mean?)
            //CheckGameOver
            //
            // Bälle hinterherwerfen ???
            #endregion
        
        }
        else
        {
            Debug.Log("GameOver!");
            Destroy(playerball);
        }
        yield return null;

    }

    private int ScaleHeight(int slot)
    {
        int currentheight = height;
        for (int y = 0; y < height; y++)
        {
            if(scaleGrid[slot, y] == null)
            {
                currentheight = y;
                break;
            }
        }
        slotheight[slot] = currentheight;
        return currentheight;
    }

    private void SlotWeight(int slot)
    {
        int weight = 0;
        for (int y = 0; y < height; y++)
        {
            if (scaleGrid[slot, y] == null)
                break;
            weight += scaleGrid[slot, y].GetComponent<Ball>().GetWeight();
        }
        slotweight[slot] = weight;
        scaleDisplayText[slot].text = weight.ToString();
    }
    //////////////////////////////
    private bool CheckMatches() // int startx = 0, int starty = 0, int endx = width, int endy= height  (Optimization?) // endx = min(endx,width) 
    {
        bool match = false;
        List<(int, int)> allPoppedBalls = new List<(int, int)>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CheckMatchPos(x, y))
                {
                    match = true;
                    Debug.Log("Found Match: " + x + " " + y);
                    List<(int, int)> newPoppedBalls = FloodFill(x, y);
                    foreach (var item in newPoppedBalls)
                    {
                        UniqueAdd(allPoppedBalls, item);
                    }
                }
            }
        }

        foreach (var pball in allPoppedBalls)
        {
            int px = pball.Item1;
            int py = pball.Item2;
            GameObject popedball = scaleGrid[px, py];
            scaleGrid[px, py] = null;
            popedball.GetComponent<Ball>().Pop();
        }

        return match;
    }

    private bool CheckMatchPos(int x, int y)
    {
        if (scaleGrid[x, y] == null)
            return false;

        int type = scaleGrid[x, y].GetComponent<Ball>().GetBallType();

        if (type == -1)
            return false;

        int type2;
        int type3;
        if(x + 2 < width && scaleGrid[x + 1, y] != null && scaleGrid[x + 2, y] != null)
        {
            type2 = scaleGrid[x + 1, y].GetComponent<Ball>().GetBallType();
            type3 = scaleGrid[x + 2, y].GetComponent<Ball>().GetBallType();
            if(type == type2 && type == type3)
                return true;
        }
        return false;
    }

    private List<(int, int)> FloodFill(int x, int y) // One colour + joker + other colour ???
    {
        int i = 0;
        int poptyp = scaleGrid[x, y].GetComponent<Ball>().GetBallType();
        List<(int, int)> poppedBalls = new List<(int, int)>();
        poppedBalls.Add((x,y));

        while (i<poppedBalls.Count)
        {
            (int, int) currentBall = poppedBalls[i];
            ExpandToNeighbors(currentBall);
            i++;
        }

        return poppedBalls;

        void ExpandToNeighbors((int, int) curBall)
        {
            int type2;
            int cx = curBall.Item1;
            int cy = curBall.Item2;
            if (cx + 1 < width && scaleGrid[cx + 1, cy] != null)
            {
                type2 = scaleGrid[cx + 1, cy].GetComponent<Ball>().GetBallType();
                if (poptyp == type2)
                    UniqueAdd(poppedBalls, (cx + 1, cy)); ;
            }
            if (cx - 1 >= 0 && scaleGrid[cx - 1, cy] != null)
            {
                type2 = scaleGrid[cx - 1, cy].GetComponent<Ball>().GetBallType();
                if (poptyp == type2)
                    UniqueAdd(poppedBalls, (cx - 1, cy)); ;
            }
            if (cy + 1 < height && scaleGrid[cx, cy + 1] != null)
            {
                type2 = scaleGrid[cx, cy + 1].GetComponent<Ball>().GetBallType();
                if (poptyp == type2)
                    UniqueAdd(poppedBalls, (cx, cy + 1)); ;
            }
            if (cy - 1 >= 0 && scaleGrid[cx, cy - 1] != null)
            {
                type2 = scaleGrid[cx, cy - 1].GetComponent<Ball>().GetBallType();
                if (poptyp == type2)
                    UniqueAdd(poppedBalls, (cx, cy - 1)); ;
            }
        }
    }

    private void UniqueAdd(List<(int, int)> list, (int, int) xy)
    {
        if (!list.Contains(xy))
        {
            list.Add(xy);
        }
    }

    private void CollapseScaleGrid()
    {
        
        for (int x = 0; x < width; x++)
        {
            CollapseScaleGridSlot(x);
        }
    }

    private void CollapseScaleGridSlot(int x)
    {

        GameObject[] temp_array = new GameObject[height];
        int i = 0;
        for (int y = 0; y < height; y++)
        {
            temp_array[y] = null;
            if (scaleGrid[x, y] != null)
            {
                temp_array[i] = scaleGrid[x, y];
                i++;
            }
        }
        tween.CollapseTween(x, temp_array);
        for (int y = 0; y < height; y++)
        {
            scaleGrid[x, y] = temp_array[y];
            //if (scaleGrid[x, y] != null)
            //    scaleGrid[x, y].transform.position = new Vector3(x, y, 0);
        }
    }

    private bool TiltScales()
    {
        bool tilt = false;

        for (int scale = 0; scale < scalePositions.Length; scale++)
        {
            ScalePosition newScalePos = TiltCheck(scale);
            if(scalePositions[scale] != newScalePos)
            {
                tilt = true;
                (bool, int, int)  newyeet = Tilt(scale, scalePositions[scale], newScalePos);
                scalePositions[scale] = newScalePos;
                if (newyeet.Item1)
                    Yeet(newyeet.Item2, newyeet.Item3);
            }
        }

        return tilt;

        (bool, int, int) Tilt(int scale, ScalePosition oldScalePos, ScalePosition newScalePos)
        {
            bool yeeting = (newScalePos != ScalePosition.Balance);
            int yeetslot = 0;
            int links = 2 * scale;
            int rechts = 2 * scale + 1;
            int change = newScalePos - oldScalePos;
            int weightdifference = slotweight[rechts] - slotweight[links];

            Debug.Log("Tilt old: " + oldScalePos + " new: " + newScalePos);
            Debug.Log("ScalePosition change: " + change);
            Debug.Log("weightdifference: " + weightdifference);

            if (change <= -1 && scaleGrid[links, 0] != null && scaleGrid[links, 0].GetComponent<Ball>().GetBallType() == -1)
            {
                Debug.Log("in if-1");
                InsertBallFromBelow(scaleGrid[links, 0], rechts);
                scaleGrid[links, 0] = null;
                if(change == -2 && scaleGrid[links, 1] != null && scaleGrid[links, 1].GetComponent<Ball>().GetBallType() == -1)
                {
                    Debug.Log("in if-2");
                    InsertBallFromBelow(scaleGrid[links, 1], rechts);
                    scaleGrid[links, 1] = null;
                }
                CollapseScaleGridSlot(links);
                
                if(yeeting)
                    yeetslot = rechts;
            }

            if (change >= 1 && scaleGrid[rechts, 0] != null && scaleGrid[rechts, 0].GetComponent<Ball>().GetBallType() == -1)
            {
                Debug.Log("in if1");
                InsertBallFromBelow(scaleGrid[rechts, 0], links);
                scaleGrid[rechts, 0] = null;
                if (change == 2 && scaleGrid[rechts, 1] != null && scaleGrid[rechts, 1].GetComponent<Ball>().GetBallType() == -1)
                {
                    Debug.Log("in if2");
                    InsertBallFromBelow(scaleGrid[rechts, 1], links);
                    scaleGrid[rechts, 1] = null;
                }
                CollapseScaleGridSlot(rechts);

                if (yeeting)
                    yeetslot = links;
            }

            return (yeeting, yeetslot, weightdifference);

        }

        void Yeet(int slot, int distance)
        {
            int curheight = ScaleHeight(slot);
            Debug.Log("in yeet");

            if (curheight - 1 >= 0 && scaleGrid[slot, curheight - 1] != null && scaleGrid[slot, curheight - 1].GetComponent<Ball>().GetBallType() != -1)
            {
                GameObject yeetball = scaleGrid[slot, curheight - 1];
                scaleGrid[slot, curheight - 1] = null;

                (int, int) YeetResult = YeetImpact(slot, distance);

                Debug.Log("Yeet!!!");

                //ThrowingBall(YeetResult.Item1, yeetball);
                StartCoroutine(ThrowingBall(YeetResult.Item1, yeetball));
                /////////////////////////////////////////////
            }
        }
    }

    private ScalePosition TiltCheck(int i)
    {
        ScalePosition newScalePos;
        if (slotweight[2 * i] > slotweight[2 * i + 1])
        {
            newScalePos = ScalePosition.Left;
        }
        else if (slotweight[2 * i] < slotweight[2 * i + 1])
        {
            newScalePos = ScalePosition.Right;
        }
        else
        {
            newScalePos = ScalePosition.Balance;
        }
        return newScalePos;
    }

    private (int, int)  YeetImpact (int startingslot, int distance)
    {
        int endslot = (startingslot + distance) % width;
        int fullrounds = (startingslot + distance) / width;

        if (endslot < 0)
            endslot += width;

        Debug.Log("startingslot: " + startingslot + ", distance: " + distance);
        Debug.Log("endslot: " + endslot + ", fullrounds: " + fullrounds);

        return (endslot, fullrounds);
    }

    private void InsertBallFromBelow(GameObject ball, int slot)
    {
        //GameOver?
        if (slotheight[slot] == height - 1)
            Debug.Log("GameOver?");

        for (int y = height - 1; y > 0; y--)
        {
            scaleGrid[slot, y] = scaleGrid[slot, y-1];
            if(scaleGrid[slot, y] != null)
                scaleGrid[slot, y].transform.position = new Vector3(slot, y, 0);
        }
        scaleGrid[slot, 0] = ball;
        scaleGrid[slot, 0].transform.position = new Vector3(slot, 0, 0);
    }

    private void SupplyReload(int slot)
    {
        for (int y = 0; y < supheight-1; y++)
        {
            supplyGrid[slot, y] = supplyGrid[slot, y + 1];
            supplyGrid[slot, y].transform.position = new Vector3(slot, y + height + 1, 0);
        }
        var newball = Instantiate(ballprefab, new Vector3(slot, supheight + height, 0), Quaternion.identity);
        newball.GetComponent<Ball>().Initialize(startmaxtype, startmaxweight);
        supplyGrid[slot, supheight - 1] = newball;
    }

    private void HeightWarning(int pos)
    {
        if (slotheight[pos] == height -1)
        {
            p_SpriteRenderer.color = Color.red;
        }else if (slotheight[pos] == height - 2 )
        {
            p_SpriteRenderer.color = Color.yellow;
        }else
        {
            p_SpriteRenderer.color = Color.white;
        }
    }

    public void SetTweeningState(bool state)
    {
        tweening = state;
    }
}
