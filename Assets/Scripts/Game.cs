using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance
    {
        get;
        protected set;
    }

    // events
    public event Action OnPackageScoreTickDown = delegate { };
    public event Action<float> OnRoundTimerTick = delegate { };
    public event Action<Package, int> OnPackageDelivered = delegate { };
    public event Action<int> OnUpdateTotalScore = delegate { };
    public event Action<bool, bool, int, int, int, int, int, int> OnGameOver = delegate { };
    public event Action<int, bool> OnNewHighScore = delegate { };

    // UI
    public MenuManager menuManager;

    // game prefabs
    public GameObject packagePrefab;
    public GameObject packageDiamondPrefab;
    public GameObject truckPrefab;
    public GameObject scorePipPrefab;

    // locations
    public GameObject packageSpawn;
    public GameObject packageEnd1;
    public GameObject packageEnd2;
    public GameObject packageEnd3;
    public GameObject packageEnd4;
    public GameObject packageEnd5;
    private List<GameObject> packageEnds;

    public GameObject truckSpawn;
    public GameObject truckEnd1;
    public GameObject truckEnd2;
    public GameObject truckEnd3;
    public GameObject truckDeliverySpot;
    private List<GameObject> truckEnds;

    // game variables
    public float conveyorSpeed = 0.5f;
    public float roundLength = 60f * 4f; // each round 4 minutes
    public float roundLengthTick = 1f;  // 1 second
    public float roundLengthCurrent;
    int currentPackages;    // the packages in play
    int maxPackages = 5;

    public int packagesToDeliver = 100;    //  the packages to be delivered this round
    int packagesLeft;

    public float truckSpeed = 0.5f;
    int currentTrucks;
    int maxTrucks = 3;

    float goodLuckChance = 30f;
    float diamondPackageChance;
    public int diamondPackagePerGame = 2;
    int diamondPackageLeft;

    RangeAttribute packageDim = new RangeAttribute(0.2f, 2.0f);
    public float[] packageAllowedSizes = new float[] { 0.5f, 1, 1.5f, 2};

    GameObject[] packagesOnConveyor;
    GameObject[] trucksWaiting;
    List<GameObject> trucksDelivering;

    List<GameObject> scorePip = new List<GameObject>();

    // state
    bool gameStarted = false;
    bool gameRunning = false;
    public bool paused = false;

    bool showHelpFirstTime = true;

    // score stuff
    int subtractPackages;
    int packagesDelivered;
    int gameScore;
    int gameScoreFinal;
    bool allDeliveredBonus = false;
    bool deliveredOnTimeBonus = false;

    int sessionHighScore = 0;

    private GameObject heldPackage;
    private int heldPackageIndex;

    // timers
    float packageSpawnTimer = 0.0f;
    float truckSpawnTimer = 0.0f;
    public float spawnWaitTime = 1.0f;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        menuManager.openPage(MenuPage.Page.main);

        packageEnds = new List<GameObject> { packageEnd1, packageEnd2, packageEnd3, packageEnd4, packageEnd5 };

        truckEnds = new List<GameObject> { truckEnd1, truckEnd2, truckEnd3 };

        diamondPackageChance = ((float)diamondPackagePerGame / (float)packagesToDeliver) * 100;

        // setup 5 score pippers
        for (int i = 0; i < 5; i++)
        {
            GameObject newPip = Instantiate(scorePipPrefab);
            newPip.name = "pip" + i;
            scorePip.Add(newPip);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            gameStarted = false;
            gameRunning = true;
            startGameRoutines();
        }

        if(gameRunning)
        {
            if(showHelpFirstTime)
            {
                showHelpFirstTime = false;
                toggleMenu();
            }
            if (!paused)
            {
                if (heldPackage != null)
                {
                    Vector3 screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    heldPackage.transform.position = new Vector3(screenPos.x, screenPos.y, heldPackage.transform.position.z);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (heldPackage != null)
                    {
                        // drop package
                        dropHeldPackage();
                    }
                }
            }

            if (packagesDelivered >= packagesToDeliver)
            {
                // end game, show score
                gameOver();
            }

            if (roundLengthCurrent < 0)
            {
                // end game, show score
                gameOver();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                toggleMenu();
            }
        }
    }

    public void toggleMenu()
    {
        if (menuManager.currentPage == MenuPage.Page.gameMenu)
        {
            unpauseGame();
            menuManager.openPage(MenuPage.Page.game);
        }
        else
        {
            pauseGame();
            menuManager.openPage(MenuPage.Page.gameMenu);
        }
    }

    public void pauseGame()
    {
        paused = true;
    }

    public void unpauseGame()
    {
        paused = false;
    }

    private void gameOver()
    {
        StopAllCoroutines();
        calculateGameOverBonuses();
        gameRunning = false;
        OnGameOver(allDeliveredBonus, deliveredOnTimeBonus, packagesToDeliver, subtractPackages, packagesLeft, packagesDelivered, gameScore, gameScoreFinal);
        menuManager.openPage(MenuPage.Page.gameover);

        // check if new high score
        if(gameScoreFinal > sessionHighScore)
        {
            sessionHighScore = gameScoreFinal;
            OnNewHighScore(gameScoreFinal, true);
        } else
        {
            OnNewHighScore(gameScoreFinal, false);
        }
    }

    private void initNewGame()
    {
        StopAllCoroutines();
        // remove old gameobjects from previous games
        if (packagesOnConveyor != null)
        {
            for (int i = 0; i < packagesOnConveyor.Length; i++)
            {
                Destroy(packagesOnConveyor[i]);
            }
        }
        if (trucksWaiting != null)
        {
            for (int i = 0; i < trucksWaiting.Length; i++)
            {
                Destroy(trucksWaiting[i]);
            }
        }
        if (trucksDelivering != null)
        {
            for (int i = 0; i < trucksDelivering.Count; i++)
            {
                Destroy(trucksDelivering[i]);
            }
        }
        packagesOnConveyor = new GameObject[5];
        trucksWaiting = new GameObject[3];
        trucksDelivering = new List<GameObject>();

        currentPackages = 0;
        currentTrucks = 0;
        packagesDelivered = 0;
        gameScore = 0;
        gameScoreFinal = 0;
        roundLengthCurrent = roundLength;
        packagesLeft = packagesToDeliver;

        diamondPackageLeft = diamondPackagePerGame;

        packageSpawnTimer = spawnWaitTime/2;
        truckSpawnTimer = spawnWaitTime/2;

        OnRoundTimerTick(roundLengthCurrent);
        OnPackageDelivered(null, packagesLeft);
        OnUpdateTotalScore(gameScore);

        paused = false;
    }

    private void startGameRoutines()
    {
        StartCoroutine(runGame());
        StartCoroutine(roundTimer());
        StartCoroutine(updatePackageScore());
        StartCoroutine(spawnPackages());
        StartCoroutine(spawnTrucks());
    }

    IEnumerator runGame()
    {
        while (gameRunning)
        {
            while (!paused)
            {
                // package movement
                for (int i = 0; i < packagesOnConveyor.Length; i++)
                {
                    if (packagesOnConveyor[i] != null)
                    {
                        // package is there, move it forward.
                        packagesOnConveyor[i].transform.position = Vector3.MoveTowards(packagesOnConveyor[i].transform.position, packageEnds[i].transform.position, conveyorSpeed * Time.deltaTime);

                        // if package is stopped on conveyor belt, flag it and start timer count down
                        if (packagesOnConveyor[i].transform.position == packageEnds[i].transform.position)
                        {
                            packagesOnConveyor[i].GetComponent<Package>().waiting = true;
                        }
                    }
                }

                // truck movement
                for (int i = 0; i < trucksWaiting.Length; i++)
                {
                    if (trucksWaiting[i] != null)
                    {
                        // truck is there, move it forward.
                        trucksWaiting[i].transform.position = Vector3.MoveTowards(trucksWaiting[i].transform.position, truckEnds[i].transform.position, truckSpeed * Time.deltaTime);

                        if (trucksWaiting[i].GetComponent<Truck>().full)
                        {
                            // truck is already full, move it off screen to 'deliver'
                            skipTruck(trucksWaiting[i].GetComponent<Truck>());
                        }
                    }
                }

                foreach (GameObject truck in trucksDelivering)
                {
                    truck.transform.position = Vector3.MoveTowards(truck.transform.position, truckDeliverySpot.transform.position, truckSpeed * Time.deltaTime);
                }

                // truck removal
                trucksDelivering.RemoveAll(t => t.transform.position == truckDeliverySpot.transform.position);
                yield return null;
            }
            yield return null;
        }
    }

    IEnumerator roundTimer()
    {
        while (gameRunning)
        {
            while (!paused)
            {
                roundLengthCurrent -= roundLengthTick;
                OnRoundTimerTick(roundLengthCurrent);
                yield return new WaitForSecondsRealtime(roundLengthTick);
            }
            yield return null;
        }
    }

    IEnumerator spawnPackages()
    {
        while (gameRunning)
        {
            while (!paused)
            {
                packageSpawnTimer += Time.deltaTime;
                if (packageSpawnTimer > spawnWaitTime)
                {
                    Debug.Log("this is package spawner");
                    if (currentPackages < maxPackages && packagesLeft > 0)
                    {
                        spawnPackage();
                    }
                    packageSpawnTimer = packageSpawnTimer - spawnWaitTime;
                }

                //yield return new WaitForSecondsRealtime(packageSpawnRate);
                yield return null;
            }

            yield return null;
        }
    }

    IEnumerator spawnTrucks()
    {
        while (gameRunning)
        {
            while (!paused)
            {
                truckSpawnTimer += Time.deltaTime;
                if (truckSpawnTimer > spawnWaitTime)
                {
                    Debug.Log("this is truck spawner");
                    if (currentTrucks < maxTrucks)
                    {
                        spawnTruck();
                    }
                    truckSpawnTimer = truckSpawnTimer - spawnWaitTime;
                }
                //yield return new WaitForSecondsRealtime(truckSpawnRate);
                yield return null;
            }

            yield return null;
        }
    }

    IEnumerator updatePackageScore()
    {
        while (gameRunning)
        {
            while (!paused)
            {
                OnPackageScoreTickDown();
                yield return new WaitForSecondsRealtime(1);
            }

            yield return null;
        }
    }

    private void spawnPackage()
    {
        // random dimensions
        //float size = UnityEngine.Random.Ra nge(packageDim.min, packageDim.max);
        int sizeIndex = UnityEngine.Random.Range(0, packageAllowedSizes.Length);
        float size = packageAllowedSizes[sizeIndex];

        GameObject prefab = packagePrefab;
        if ((UnityEngine.Random.Range(0, 100) < diamondPackageChance || packagesLeft <= diamondPackagePerGame)
            && diamondPackageLeft > 0)
        {
            prefab = packageDiamondPrefab;
            diamondPackageLeft--;
        } 
        GameObject newPackage = Instantiate(prefab, packageSpawn.transform.position, packageSpawn.transform.rotation);
        newPackage.transform.localScale = new Vector3(size, size, size);

        
        Package package = newPackage.GetComponent<Package>();
        package.index = currentPackages;
        package.size = newPackage.transform.localScale.x;
        package.weight = 1;

        packagesOnConveyor[currentPackages] = newPackage;
        currentPackages++;
        packagesLeft--;
    }

    private void spawnTruck()
    {
        // random dimensions
        int sizeIndex = UnityEngine.Random.Range(0, packageAllowedSizes.Length);
        float size = packageAllowedSizes[sizeIndex];

        // every 'goodLuckChance', if no current truck sizes fit, spawn one that does
        if (UnityEngine.Random.Range(0, 100) < goodLuckChance)
        {
            float maxSizeTruck = 0;
            for (int i = 0; i < currentTrucks; i++)
            {
                if (trucksWaiting[i].GetComponent<Truck>().maxSize > maxSizeTruck)
                {
                    maxSizeTruck = trucksWaiting[i].GetComponent<Truck>().maxSize;
                }
            }
            float maxSizePackage = 0;
            for (int i = 0; i < currentPackages; i++)
            {
                if (packagesOnConveyor[i].GetComponent<Package>().size > maxSizePackage)
                {
                    maxSizePackage = packagesOnConveyor[i].GetComponent<Package>().size;
                }
            }
            if (maxSizePackage > maxSizeTruck)
            {
                size = maxSizePackage;
            }
        }

        GameObject newTruck = Instantiate(truckPrefab, truckSpawn.transform.position, truckSpawn.transform.rotation);
        Truck truckScript = newTruck.GetComponent<Truck>();
        truckScript.index = currentTrucks;
        truckScript.box.transform.localScale = new Vector3(size, size, size);
        truckScript.maxSize = truckScript.box.transform.localScale.x;
        truckScript.maxWeight = 100;

        trucksWaiting[currentTrucks] = newTruck;
        currentTrucks++;
    }

    public void startNewGame()
    {
        menuManager.openPage(MenuPage.Page.game);

        // do init stuff
        initNewGame();

        gameStarted = true;
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void quitToMenu()
    {
        menuManager.openPage(MenuPage.Page.main);
    }

    public void skipTruck(Truck truck)
    {
        if (trucksWaiting[truck.index] != null)
        {
            Debug.Log("skip truck: " + truck.index);
            trucksDelivering.Add(trucksWaiting[truck.index]);
            bumpUpInArray(truck.index, trucksWaiting, maxTrucks);
            currentTrucks--;
        }
    }

    public void setHeldPackage(int packageIndex)
    {
        dropHeldPackage();
        heldPackage = packagesOnConveyor[packageIndex];
        heldPackage.GetComponent<Collider2D>().enabled = false;
        heldPackageIndex = packageIndex;
    }

    public void placeHeldPackageOnTruck(GameObject truck)
    {
        if (heldPackage != null)
        {
            // check if truck can carry package
            Truck truckScript = truck.GetComponent<Truck>();
            if (!packageFits(heldPackage.GetComponent<Package>(), truckScript))
            {
                dropHeldPackage();
                return;
            }

            // move all other packages up in array
            bumpUpInArray(heldPackageIndex, packagesOnConveyor, maxPackages);
            currentPackages--;

            heldPackage.transform.position = truck.GetComponent<Truck>().packageHolder.transform.position;
            heldPackage.transform.position = new Vector3(heldPackage.transform.position.x, heldPackage.transform.position.y, -0.1f);
            heldPackage.transform.SetParent(truck.transform);
            deliverPackage(heldPackage.GetComponent<Package>(), truck.GetComponent<Truck>());
            heldPackage = null;

            truckScript.full = true;
        }
    }

    public void deliverPackage(Package package, Truck truck)
    {
        package.setDelivered();
        // calculate score adjustment
        int sizePenalty = (int)((truck.maxSize - package.size) * 10)/5;
        int packageScore = package.scoreValue - Math.Abs(sizePenalty);    // this can be negative

        packagesDelivered++;
        OnPackageDelivered(package, packagesToDeliver - packagesDelivered);
        gameScore += packageScore;
        OnUpdateTotalScore(gameScore);

        List<Func<ScorePip>> pips = new List<Func<ScorePip>>();
        pips.Add(() => getScorePip().score(truck.packageHolder.gameObject.transform.position, package.scoreValue.ToString()).run());
        if (sizePenalty != 0)
        {
            string diff = sizePenalty < 0 ? "oversize" : "undersize";
            pips.Add(() => getScorePip().score(truck.packageHolder.gameObject.transform.position, "-" + sizePenalty.ToString() + ", " + diff).run());
        }

        StartCoroutine(scorePippers(pips));
    }

    IEnumerator scorePippers(List<Func<ScorePip>> pips)
    {
        for(int i = 0; i < pips.Count; i++)
        {
            pips[i]();
            yield return new WaitForSecondsRealtime(0.5f);
        }

        yield return null;
    }

    public ScorePip getScorePip()
    {
        foreach(GameObject pip in scorePip)
        {
            if(!pip.GetComponent<ScorePip>().running)
            {
                return pip.GetComponent<ScorePip>();
            }
        }
        return scorePip[0].GetComponent<ScorePip>();
    }

    public void dropHeldPackage()
    {
        if (heldPackage != null)
        {
            heldPackage.GetComponent<Collider2D>().enabled = true;
            heldPackage = null;
        }
    }

    private void bumpUpInArray(int indexRemoved, GameObject[] arr, int max)
    {
        // move all other packages up in array
        for (int i = indexRemoved; i < arr.Length - 1; i++)
        {
            arr[i] = arr[i + 1];
            if (arr[i] != null)
            {
                arr[i].GetComponent<Indexed>().index = i;
            }
        }
        arr[max - 1] = null;
    }

    public bool packageFits(Package package, Truck truck)
    {
        if (package.size <= truck.maxSize
            && package.weight <= truck.maxWeight
            && !truck.full)
        {
            return true;
        }
        return false;
    }

    private void calculateGameOverBonuses()
    {
        // subtract any packages sitting on conveyor belt that were in negative score
        subtractPackages = 0;
        for (int i = 0; i < currentPackages; i++)
        {
            if (packagesOnConveyor[i].GetComponent<Package>().scoreValue < 0)
            {
                subtractPackages += Math.Abs(packagesOnConveyor[i].GetComponent<Package>().scoreValue);
            }
        }

        gameScoreFinal = gameScore - subtractPackages;
        // bonus for delivering all packages
        if(packagesLeft >= 0)
        {
            gameScoreFinal *= 2;
            allDeliveredBonus = true;
        }

        // bonus for delivering all packages before the round ends
        if(roundLengthCurrent > 0)
        {
            gameScoreFinal *= 2;
            deliveredOnTimeBonus = true;
        }

        // bonus for no package going into negatives?  (need to track this)
    }
}
