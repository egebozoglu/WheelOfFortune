using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

public class WheelHandler : MonoBehaviour
{
    #region Variables
    [Header("Wheel Setup")]
    [SerializeField] private Transform PickerWheelTransform;
    [SerializeField] private Transform wheelCircle;
    [SerializeField] private GameObject wheelContentPrefab;
    [SerializeField] private Transform wheelContentsParent;
    public List<WheelContent> wheelContents = new List<WheelContent>();

    [Header("Wheel Settings")]
    [Range(1, 20)] public int spinTime;
    string wheelType;

    private UnityAction spinStart;
    private UnityAction<WheelContent> spinEnd;

    // Spinning boolean
    private bool isSpinning = false;
    public bool IsSpinning { get { return isSpinning; } }

    // Content Size
    private Vector2 contentMinSize = new Vector2(81f, 146f);
    private Vector2 contentMaxSize = new Vector2(144f, 213f);
    private int contentsMin = 2;
    private int contentsMax = 8;

    // Angle
    private float contentAngle;
    private float halfcontentAngle;
    private float halfcontentAngleWithPaddings;

    // Weight
    private double accumulatedWeight;
    private System.Random rand = new System.Random();

    // Sound
    private AudioSource audioSource;

    private List<int> nonZeroDropRate = new List<int>();
    #endregion

    private void Awake()
    {
        wheelType = this.transform.gameObject.tag;
        SetWheelContentObjectList();

        audioSource = transform.GetComponent<AudioSource>();
    }
    private void Start()
    {
        contentAngle = 360 / wheelContents.Count;
        halfcontentAngle = contentAngle / 2f;
        halfcontentAngleWithPaddings = halfcontentAngle - (halfcontentAngle / 4f);

        Create();
        WeightsAndIndices();
    }

    #region Pick Wheel Contents
    private void SetWheelContentObjectList()
    {
        // Get all contents in order to pick randomly for put them into wheel
        wheelContents.Clear();
        List<WheelContent> contentObjects = new List<WheelContent>();
        Object[] objects = Resources.LoadAll("WheelContentObjects", typeof(ScriptableObject));
        foreach (WheelContent content in objects)
        {
            contentObjects.Add(content);
        }

        // Choose 8 prizes according to WheelType => Bronze, Silver or Gold. Don't forget to add bomb to common class prizes.
        if (wheelType=="Bronze")
        {
            var commonRewards = contentObjects.Where(x => x.RewardClass.ToString() == "Common").Where(x=>x.Active == true).ToList();
            Shuffle(commonRewards);
            commonRewards = commonRewards.Take(7).ToList();
            commonRewards.Add(contentObjects.Where(x => x.RewardClass.ToString() == "Bomb").FirstOrDefault());
            Shuffle(commonRewards);
            wheelContents = commonRewards;
        }
        else if (wheelType == "Silver")
        {
            var normalPrizes = contentObjects.Where(x => x.RewardClass.ToString() == "Normal").Where(x => x.Active == true).ToList();
            Shuffle(normalPrizes);
            normalPrizes = normalPrizes.Take(8).ToList();
            wheelContents = normalPrizes;
        }
        else // Gold
        {
            var rareRewards = contentObjects.Where(x => x.RewardClass.ToString() == "Rare").Where(x => x.Active == true).ToList();
            Shuffle(rareRewards);
            rareRewards = rareRewards.Take(8).ToList();
            wheelContents = rareRewards;
        }
    }

    void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    #endregion

    #region Actions
    public void SpinStartAction(UnityAction action)
    {
        spinStart = action;
    }

    public void SpinEndAction(UnityAction<WheelContent> action)
    {
        spinEnd = action;
    }
    #endregion

    #region Set Contents
    private void Create()
    {
        wheelContentPrefab = InstantiateContent();
        RectTransform rect = wheelContentPrefab.transform.GetChild(0).GetComponent<RectTransform>();
        float contentWidth = Mathf.Lerp(contentMinSize.x, contentMaxSize.x, 1f - Mathf.InverseLerp(contentsMin, contentsMax, wheelContents.Count));
        float contentHeight = Mathf.Lerp(contentMinSize.y, contentMaxSize.y, 1f - Mathf.InverseLerp(contentsMin, contentsMax, wheelContents.Count));
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

        for (int i = 0; i < wheelContents.Count; i++)
            PlaceContent(i);

        Destroy(wheelContentPrefab);
    }

    private void PlaceContent(int index)
    {
        WheelContent content = wheelContents[index];
        Transform contentTransform = InstantiateContent().transform.GetChild(0);

        contentTransform.GetChild(0).GetComponent<Image>().sprite = content.ContentIcon;
        if (content.RewardCount!=0)
        {
            contentTransform.GetChild(1).GetComponent<Text>().text = content.RewardCount.ToString();
        }

        contentTransform.RotateAround(wheelContentsParent.position, Vector3.back, contentAngle * index);
    }

    private GameObject InstantiateContent()
    {
        return Instantiate(wheelContentPrefab, wheelContentsParent.position, Quaternion.identity, wheelContentsParent);
    }
    

    private int GetContentIndex()
    {
        double r = rand.NextDouble() * accumulatedWeight;

        for (int i = 0; i < wheelContents.Count; i++)
            if (wheelContents[i].weight >= r)
                return i;

        return 0;
    }

    private void WeightsAndIndices()
    {
        for (int i = 0; i < wheelContents.Count; i++)
        {
            WheelContent content = wheelContents[i];

            // Add weights
            accumulatedWeight += content.DropRate;
            content.weight = accumulatedWeight;

            // Add index
            content.Index = i;

            // Save non zero drop rates
            if (content.DropRate > 0)
                nonZeroDropRate.Add(i);
        }
    }
    #endregion

    public void SpinWheel()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            if (spinStart != null)
                spinStart.Invoke();

            int index = GetContentIndex();
            WheelContent content = wheelContents[index];

            if (content.DropRate == 0 && nonZeroDropRate.Count != 0)
            {
                index = nonZeroDropRate[Random.Range(0, nonZeroDropRate.Count)];
                content = wheelContents[index];
            }

            float angle = -(contentAngle * index);

            float rightOffset = (angle - halfcontentAngleWithPaddings) % 360;
            float leftOffset = (angle + halfcontentAngleWithPaddings) % 360;

            float randomAngle = Random.Range(leftOffset, rightOffset);

            Vector3 targetRotation = Vector3.back * (randomAngle + 2 * 360 * spinTime);

            float prevAngle, currentAngle;
            prevAngle = currentAngle = wheelCircle.eulerAngles.z;

            bool isIndicatorOnTheLine = false;

            wheelCircle.DORotate(targetRotation, spinTime, RotateMode.Fast).SetEase(Ease.InOutQuart)
                .OnUpdate(() => {
                float diff = Mathf.Abs(prevAngle - currentAngle);
                if (diff >= halfcontentAngle)
                {
                    prevAngle = currentAngle;
                    isIndicatorOnTheLine = !isIndicatorOnTheLine;
                }
                currentAngle = wheelCircle.eulerAngles.z;
                if (audioSource.isPlaying)
                {
                    audioSource.pitch -= Time.deltaTime / spinTime;
                }
            })
            .OnComplete(() => {
                audioSource.Pause();
                isSpinning = false;
                if (spinEnd != null)
                    spinEnd.Invoke(content);

                spinStart = null;
                spinEnd = null;
            });
            audioSource.PlayDelayed(1f);
        }
    }

    private void OnValidate()
    {
        if (PickerWheelTransform != null)
            PickerWheelTransform.localScale = new Vector3(1f, 1f, 1f);
    }
}
