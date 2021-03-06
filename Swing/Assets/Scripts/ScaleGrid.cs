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
    [SerializeField] private GameObject scalePrefab;
    [SerializeField] private GameObject scaleDisplayprefab;
    [SerializeField] private GameObject player;
    private SpriteRenderer p_SpriteRenderer;
    private GameObject playerball;
    private int playerpos = 0;
    GameObject[,] scaleGrid = new GameObject[width, height];
    GameObject[,] supplyGrid = new GameObject[width, supheight];
    GameObject[] scaleDisplay = new GameObject[width];
    TextMeshProUGUI[] scaleDisplayText = new TextMeshProUGUI[width];
    private float scaleWeightSmallDisplay = 35;
    private float scaleWeightBigDisplay = 50;
    private int[] slotweight = new int[width];
    [SerializeField] private int[] slotheight = new int[width];

    [SerializeField] private int maxweight = 5;
    [SerializeField] private int maxtype = 3;

    private int thrownballs = 0;
    private int level = 1;
    [SerializeField] private int weightlevelup = 50;
    [SerializeField] private int typelevelup = 100;
    [SerializeField] private TextMeshProUGUI leveldisplay;

    [SerializeField] private int collapseHeight = 5;


    private Tween tween;
    private bool tweening = false;

    private bool gameover = false;

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
            Instantiate(scalePrefab, new Vector3(2*i + 0.5f , -1, 0), Quaternion.identity);
        }

        for (int x = 0; x < width; x++)
        {
            // Instantiate the Scale Balls
            var newscaleball = Instantiate(ballprefab, new Vector3(x, 0, 0), Quaternion.identity);
            newscaleball.GetComponent<Ball>().InitializeScale();
            scaleGrid[x, 0] = newscaleball;
            slotheight[x] = 1;

            // Instantiate the Scale Display
            var newscale = Instantiate(scaleDisplayprefab, new Vector3(x, -1.75f, 0), Quaternion.identity);
            scaleDisplay[x] = newscale;
            scaleDisplayText[x] = newscale.GetComponentInChildren<TextMeshProUGUI>();
            slotweight[x] = 0;

            // Instantiate the Supply Balls
            for (int y = 0; y < supheight; y++)
            {
                var newsupball = Instantiate(ballprefab, new Vector3(x, y + height + 1, 0), Quaternion.identity);
                newsupball.GetComponent<Ball>().Initialize(maxtype, maxweight);
                supplyGrid[x, y] = newsupball;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ResetBalls();
        }

        if (gameover)
            return;

        //+++ WeightScaleTipWarning
        if (Input.GetKeyDown(KeyCode.RightArrow) && playerpos < width-1)
        {
            MovePlayer(playerpos + 1);
        }else if (Input.GetKeyDown(KeyCode.LeftArrow) && playerpos > 0)
        {
            MovePlayer(playerpos - 1);
        }else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ReleaseBall();
        }
    }

    public void MovePlayer(int slot)
    {
        if (playerpos == slot || gameover)
            return;

        playerpos = slot;
        tween.PlayerSizeTween(player);
        Vector3 newPlayerPos = new Vector3(slot, player.transform.position.y, 0);
        player.transform.position = newPlayerPos;
        if (playerball != null)
        {
            playerball.transform.position = newPlayerPos;
            TiltWarning(playerpos, playerball.GetComponent<Ball>().GetWeight());
        }
        HeightWarning(playerpos);
    }

    public void ReleaseBall()
    {
        if (gameover)
            return;


        //Throwing Ball
        if (playerball != null)
        {
            // Leveling Up the difficulty.
            thrownballs++;
            if (thrownballs % weightlevelup == 0)
                maxweight++;
            if (thrownballs % typelevelup == 0)
            {
                maxtype++;
                level++;
                leveldisplay.text = $"Level: {level}";
            }

            StartCoroutine(ThrowingBall(playerpos, playerball));
        }
            
        //Loading New Ball
        playerball = supplyGrid[playerpos, 0];
        playerball.transform.position = player.transform.position;
        
        // New Ball Supply
        SupplyReload(playerpos);
        ScaleHeight(playerpos);
        HeightWarning(playerpos);
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

            //Debug.Log("StartCalculating");

            while (CheckMatches())
            {
                yield return new WaitForSeconds(0.5f); // Give Balls time to pop ???
                CollapseScaleGrid();
                yield return new WaitForSeconds(tween.GetCollapseTime()); // Waiting for the Collapse to finish
            }



            for (int x = 0; x < width; x++)
            {
                ScaleHeight(x);
                SlotWeight(x);
            }


            #region TiltScales()
            for (int scale = 0; scale < scalePositions.Length; scale++)
            {
                ScalePosition newScalePos = TiltCheck(scale);
                if (scalePositions[scale] != newScalePos)
                {
                    (bool, int, int) newyeet = Tilt(scale, scalePositions[scale], newScalePos);
                    scalePositions[scale] = newScalePos;
                    if (newyeet.Item1)
                    {
                        GameObject yeetball = YeetBall(newyeet.Item2);
                        (int, int) YeetResult = YeetImpact(newyeet.Item2, newyeet.Item3);
                        //Debug.Log("Yeet!!!");

                        if (yeetball != null)
                        {
                            tweening = true;
                            // Tweening
                            tween.YeetBallTween(yeetball, new Vector3(YeetResult.Item1, height - 0.5f, 0), YeetResult.Item2, newyeet.Item3);
                            // wait
                            yield return new WaitWhile(() => tweening);

                            //ThrowingBall(YeetResult.Item1, yeetball);
                            StartCoroutine(ThrowingBall(YeetResult.Item1, yeetball));
                            yield break; // M?glicherweise sollten zuerst alle Yeetb?lle gefunden werden und sie dann alle gleichzeitig yeeten.
                        }
                    }

                }
            }
            #endregion


            //BallCompression
            TowerCollapseCheck();

            //CheckMatches() ???

            //Test
            for (int x = 0; x < width; x++)
            {
                ScaleHeight(x);
                SlotWeight(x);

                if (slotheight[x] >= height)
                {
                    GameOver();
                }
            }

            // Stopping the WeightWarning
            if (playerball != null)
                TiltWarning(playerpos, playerball.GetComponent<Ball>().GetWeight());

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
            // B?lle hinterherwerfen ???
            #endregion

        }
        else
        {
            Destroy(playerball);
            GameOver();
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
        if (weight > 99)
        {
            scaleDisplayText[slot].fontSize = scaleWeightSmallDisplay;
        }else
        {
            scaleDisplayText[slot].fontSize = scaleWeightBigDisplay;
        }
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
                    //Debug.Log("Found Match: " + x + " " + y);
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

    private ScalePosition TiltCheck(int i)
    {
        return WeightToScalePosition(slotweight[2 * i], slotweight[2 * i + 1]);
    }

    private ScalePosition WeightToScalePosition(int weightleft, int weightright)
    {
        ScalePosition newScalePos;

        if (weightleft > weightright)
        {
            newScalePos = ScalePosition.Left;
        }
        else if (weightleft < weightright)
        {
            newScalePos = ScalePosition.Right;
        }
        else
        {
            newScalePos = ScalePosition.Balance;
        }

        return newScalePos;
    }


    /// <summary>
    /// return (yeeting, yeetslot, weightdifference);
    /// </summary>
    /// 
    private (bool, int, int) Tilt(int scale, ScalePosition oldScalePos, ScalePosition newScalePos)
    {
        bool yeeting = (newScalePos != ScalePosition.Balance);
        int yeetslot = 0;
        int links = 2 * scale;
        int rechts = 2 * scale + 1;
        int change = newScalePos - oldScalePos;
        int weightdifference = slotweight[rechts] - slotweight[links];

        //Debug.Log("Tilt old: " + oldScalePos + " new: " + newScalePos);
        //Debug.Log("ScalePosition change: " + change);
        //Debug.Log("weightdifference: " + weightdifference);

        if (change <= -1 && scaleGrid[links, 0] != null && scaleGrid[links, 0].GetComponent<Ball>().GetBallType() == -1)
        {
            //Debug.Log("in if-1");
            InsertBallFromBelow(scaleGrid[links, 0], rechts);
            scaleGrid[links, 0] = null;
            if (change == -2 && scaleGrid[links, 1] != null && scaleGrid[links, 1].GetComponent<Ball>().GetBallType() == -1)
            {
                //Debug.Log("in if-2");
                InsertBallFromBelow(scaleGrid[links, 1], rechts);
                scaleGrid[links, 1] = null;
            }
            CollapseScaleGridSlot(links);

            if (yeeting)
                yeetslot = rechts;
        }

        if (change >= 1 && scaleGrid[rechts, 0] != null && scaleGrid[rechts, 0].GetComponent<Ball>().GetBallType() == -1)
        {
            //Debug.Log("in if1");
            InsertBallFromBelow(scaleGrid[rechts, 0], links);
            scaleGrid[rechts, 0] = null;
            if (change == 2 && scaleGrid[rechts, 1] != null && scaleGrid[rechts, 1].GetComponent<Ball>().GetBallType() == -1)
            {
                //Debug.Log("in if2");
                InsertBallFromBelow(scaleGrid[rechts, 1], links);
                scaleGrid[rechts, 1] = null;
            }
            CollapseScaleGridSlot(rechts);

            if (yeeting)
                yeetslot = links;
        }

        return (yeeting, yeetslot, weightdifference);

    }

    private GameObject YeetBall(int slot)
    {
        int curheight = ScaleHeight(slot);
        //Debug.Log("searching ball for yeeting");
        GameObject yeetball = null;

        if (curheight - 1 >= 0 && scaleGrid[slot, curheight - 1] != null && scaleGrid[slot, curheight - 1].GetComponent<Ball>().GetBallType() != -1)
        {
            yeetball = scaleGrid[slot, curheight - 1];
            scaleGrid[slot, curheight - 1] = null;
        }
        return yeetball;
    }

    /// <summary>
    /// return (endslot, fullrounds);
    /// </summary>
    private (int, int)  YeetImpact (int startingslot, int distance)
    {
        int endslot = (startingslot + distance) % width;
        int fullrounds = Mathf.Abs(Mathf.FloorToInt((startingslot + distance)*1f / width));

        

        if (endslot < 0)
            endslot += width;

        //Debug.Log("startingslot: " + startingslot + ", distance: " + distance);
        //Debug.Log("endslot: " + endslot + ", fullrounds: " + fullrounds);

        return (endslot, fullrounds);
    }

    private void InsertBallFromBelow(GameObject ball, int slot)
    {
        //GameOver?
        if (slotheight[slot] == height - 1)
            GameOver();

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
        newball.GetComponent<Ball>().Initialize(maxtype, maxweight);
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

    private void TiltWarning(int slot, int addWeight)
    {
        ScalePosition newScalePosition;
        int scale = slot/2;
        GameObject weighttext1;
        GameObject weighttext2;

        for (int x = 0; x < width; x++)
        {
            scaleDisplay[x].GetComponent<WeightDisplayFlicker>().StopWeightWarning();
        }

        if ((slot % 2) == 0)
        {
            newScalePosition = WeightToScalePosition(slotweight[slot] + addWeight, slotweight[slot + 1]);
            weighttext1 = scaleDisplay[slot];
            weighttext2 = scaleDisplay[slot + 1];
        }
        else
        {
            newScalePosition = WeightToScalePosition(slotweight[slot - 1], slotweight[slot] + addWeight);
            weighttext1 = scaleDisplay[slot - 1];
            weighttext2 = scaleDisplay[slot];
        }

        if(newScalePosition != scalePositions[scale])
        {
            weighttext1.GetComponent<WeightDisplayFlicker>().TweenWeightWarning();
            weighttext2.GetComponent<WeightDisplayFlicker>().TweenWeightWarning();
        }
    }

    private void TowerCollapseCheck()
    {
        for (int x = 0; x < width; x++)
        {
            bool ballFound = false;
            int balltyp = 0;
            int ballsSuccession = 0;
            int maxy=0;
            for (int y = 0; y < height; y++)
            {
                /// 5 B?lle finden
                /// 
                if (scaleGrid[x, y] == null)
                    break;

                maxy = y;

                if (!ballFound)
                {
                    // First ball found
                    ballFound = true;
                    balltyp = scaleGrid[x, y].GetComponent<Ball>().GetBallType();
                    ballsSuccession = 1;
                    continue;
                }
                int nextballtype = scaleGrid[x, y].GetComponent<Ball>().GetBallType();

                if(balltyp == nextballtype)
                {
                    ballsSuccession += 1;
                }
                else
                {
                    if(ballsSuccession >= collapseHeight)
                    {
                        TowerCollapse(x,y, ballsSuccession);
                    }
                    ballsSuccession = 1;
                    balltyp = nextballtype;
                }
            }
            if (ballsSuccession >= collapseHeight)
            {
                TowerCollapse(x, maxy, ballsSuccession);
            }
            CollapseScaleGridSlot(x);
        }
    }

    private void TowerCollapse(int x, int y, int height)
    {
        //Debug.Log("Tower Found + x: " + x + " y: " + y + " height: " + height);
        int totalweight = 0;
        Vector3 collapsetarget = new Vector3(x, y - height + 1, 0);
        for (int i = y; i > y-height; i--)
        {
            Ball curBall = scaleGrid[x, i].GetComponent<Ball>();
            totalweight += curBall.GetWeight();

            if(i == y - height + 1)
            {
                curBall.Expanding();
                curBall.SetWeight(totalweight);
            }
            else
            {
                curBall.Collapse(collapsetarget);
                scaleGrid[x, i] = null;
            }
        }
    }

    public void SetTweeningState(bool state)
    {
        tweening = state;
    }

    private void GameOver()
    {
        //Debug.Log("Game Over!");
        leveldisplay.text = $"Level: {level} - GAME OVER";
        gameover = true;
    }

    public void ResetBalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(scaleGrid[x,y] != null)
                {
                    scaleGrid[x, y].transform.position = new Vector3(x, y, 0);
                }
            }
        }
    }

    public int GetGridWidth()
    {
        return width;
    }
}
