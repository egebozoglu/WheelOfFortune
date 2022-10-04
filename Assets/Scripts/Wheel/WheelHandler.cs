using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

namespace Wheel
{
    public class WheelHandler : MonoBehaviour
    {
        #region Variables
        [Header("Wheel Setup")]
        [SerializeField] private Transform pickerWheelTransform;
        [SerializeField] private Transform wheelCircle;
        [SerializeField] private GameObject wheelContentPrefab;
        [SerializeField] private Transform wheelContentsParent;
        public List<WheelContent> WheelContents = new List<WheelContent>();

        [Header("Wheel Settings")]
        [Range(1, 20)] public int SpinTime;
        private string wheelType;

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
        private int contentsAmount = 8;

        // Angle
        private float contentAngle;
        private float halfcontentAngle;
        private float halfcontentAngleWithPaddings;
        private int circleAngle = 360;

        // Weight
        private double accumulatedWeight;
        private System.Random rand = new System.Random();

        // Sound
        private AudioSource audioSource;

        private List<int> nonZeroDropRate = new List<int>();

        // Rarity Properties
        private string common = "Common";
        private string normal = "Normal";
        private string rare = "Rare";
        private string bomb = "Bomb";
        #endregion

        private void Awake()
        {
            wheelType = this.transform.gameObject.tag;
            SetWheelContentObjectList();

            audioSource = transform.GetComponent<AudioSource>();
        }
        private void Start()
        {
            contentAngle = circleAngle / WheelContents.Count;
            halfcontentAngle = contentAngle / 2f;
            halfcontentAngleWithPaddings = halfcontentAngle - (halfcontentAngle / 4f);

            Create();
            WeightsAndIndices();
        }

        #region Pick Wheel Contents
        private void SetWheelContentObjectList()
        {
            // Get all contents in order to pick randomly for put them into wheel
            WheelContents.Clear();
            List<WheelContent> contentObjects = new List<WheelContent>();
            Object[] objects = Resources.LoadAll("WheelContentObjects", typeof(ScriptableObject));
            foreach (WheelContent content in objects)
            {
                contentObjects.Add(content);
            }

            // Choose 8 prizes according to WheelType => Bronze, Silver or Gold. Don't forget to add bomb to common class prizes.
            if (wheelType == "Bronze")
            {
                var commonRewards = contentObjects.Where(x => x.RarityProperty == common).Where(x => x.Active == true).ToList();
                Shuffle(commonRewards);
                commonRewards = commonRewards.Take(contentsAmount-1).ToList();
                commonRewards.Add(contentObjects.Where(x => x.RarityProperty == bomb).FirstOrDefault());
                Shuffle(commonRewards);
                WheelContents = commonRewards;
            }
            else if (wheelType == "Silver")
            {
                var normalRewards = contentObjects.Where(x => x.RarityProperty == normal).Where(x => x.Active == true).ToList();
                Shuffle(normalRewards);
                normalRewards = normalRewards.Take(contentsAmount).ToList();
                WheelContents = normalRewards;
            }
            else // Gold
            {
                var rareRewards = contentObjects.Where(x => x.RarityProperty == rare).Where(x => x.Active == true).ToList();
                Shuffle(rareRewards);
                rareRewards = rareRewards.Take(contentsAmount).ToList();
                WheelContents = rareRewards;
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
            float contentWidth = Mathf.Lerp(contentMinSize.x, contentMaxSize.x, 1f - Mathf.InverseLerp(contentsMin, contentsMax, WheelContents.Count));
            float contentHeight = Mathf.Lerp(contentMinSize.y, contentMaxSize.y, 1f - Mathf.InverseLerp(contentsMin, contentsMax, WheelContents.Count));
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

            for (int i = 0; i < WheelContents.Count; i++)
                PlaceContent(i);

            Destroy(wheelContentPrefab);
        }

        private void PlaceContent(int index)
        {
            WheelContent content = WheelContents[index];
            Transform contentTransform = InstantiateContent().transform.GetChild(0);

            contentTransform.GetChild(0).GetComponent<Image>().sprite = content.ContentIcon;
            if (content.RewardCount != 0)
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

            for (int i = 0; i < WheelContents.Count; i++)
                if (WheelContents[i].Weight >= r)
                    return i;

            return 0;
        }

        private void WeightsAndIndices()
        {
            for (int i = 0; i < WheelContents.Count; i++)
            {
                WheelContent content = WheelContents[i];

                // Add weights
                accumulatedWeight += content.DropRate;
                content.Weight = accumulatedWeight;

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
                WheelContent content = WheelContents[index];

                if (content.DropRate == 0 && nonZeroDropRate.Count != 0)
                {
                    index = nonZeroDropRate[Random.Range(0, nonZeroDropRate.Count)];
                    content = WheelContents[index];
                }

                float angle = -(contentAngle * index);

                float rightOffset = (angle - halfcontentAngleWithPaddings) % circleAngle;
                float leftOffset = (angle + halfcontentAngleWithPaddings) % circleAngle;

                float randomAngle = Random.Range(leftOffset, rightOffset);

                Vector3 targetRotation = Vector3.back * (randomAngle + 2 * circleAngle * SpinTime);

                float prevAngle, currentAngle;
                prevAngle = currentAngle = wheelCircle.eulerAngles.z;

                bool isIndicatorOnTheLine = false;

                wheelCircle.DORotate(targetRotation, SpinTime, RotateMode.Fast).SetEase(Ease.InOutQuart)
                    .OnUpdate(() =>
                    {
                        float diff = Mathf.Abs(prevAngle - currentAngle);
                        if (diff >= halfcontentAngle)
                        {
                            prevAngle = currentAngle;
                            isIndicatorOnTheLine = !isIndicatorOnTheLine;
                        }
                        currentAngle = wheelCircle.eulerAngles.z;
                        if (audioSource.isPlaying)
                        {
                            audioSource.pitch -= Time.deltaTime / SpinTime;
                        }
                    })
                .OnComplete(() =>
                {
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
            if (pickerWheelTransform != null)
                pickerWheelTransform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
