using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using Wheel;
using Manager;
using Model;

namespace Handler
{
    public class GameHandler : MonoBehaviour
    {
        #region Variables
        public static GameHandler Instance;

        [Header("Zone Section")]
        public int ZoneLevel = 1;
        public GameObject ZonePrefab;
        public GameObject ZoneContainer;
        public Sprite GreenZone;
        private int minZone = 1;
        public bool ZoneAnimation = false;
        private float nextTargetRectX = -140f;
        private int safeZone = 5;
        private int superZone = 30;

        [Header("Reward Section")]
        private List<Reward> gainedRewards = new List<Reward>();
        public GameObject RewardContainer;
        private Text animatedText;
        private int targetrewardText;
        private bool textAnimationActive;

        [Header("Wheel Section")]
        private WheelHandler wheelHandler;
        private List<GameObject> instantiatedWheels = new List<GameObject>();
        [SerializeField] private Transform wheelPanel;
        public RectTransform IndicatorPosition;

        [Header("Buttons Section")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button collectButton;
        [SerializeField] private Button collectPopUpCollectButton;
        [SerializeField] private Button collectPopUpGoBackButton;
        [SerializeField] private Button soundButton;

        [Header("Bomb Section")]
        public GameObject BombPopUp;
        public Button PopUpRestartButton;

        [Header("Collect Section")]
        public GameObject CollectPopUp;

        [Header("Sound Button Section")]
        [SerializeField] private Text localizeSoundText;
        [SerializeField] private Text soundPropertyText;
        public string LocalizeSound;
        #endregion

        private void Awake()
        {
            // For reach this class from another class staticly
            if (Instance == null)
            {
                Instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            localizeSoundText.text = LocalizeSound;
            if (AudioListener.volume == 0)
            {
                soundPropertyText.text = "Off";
            }
            else
            {
                soundPropertyText.text = "On";
            }

            ButtonListeners();

            // Check zone level and set the wheel type
            SetWheelHandler();

            // Set top panel
            SetZonePanel();

            InvokeRepeating("TextAnimation", 0.0f, 0.000015f);
        }

        void ButtonListeners()
        {
            spinButton.onClick.AddListener(SpinClick);
            collectButton.onClick.AddListener(CollectClick);
            collectPopUpCollectButton.onClick.AddListener(CollectPopUpCollectClick);
            collectPopUpGoBackButton.onClick.AddListener(CollectPopUpGoBackClick);
            PopUpRestartButton.onClick.AddListener(RestartClick);
            soundButton.onClick.AddListener(SoundClick);
        }

        #region Button Voids
        private void SpinClick()
        {
            // Start Spinning
            wheelHandler.SpinStartAction(() =>
            {
                Debug.Log("Spin Started");
                spinButton.gameObject.SetActive(false);
            });
            wheelHandler.SpinEndAction(async wheelContent =>
            {
                Debug.Log("Spin Ended: " + wheelContent.Name + ", Count: " + wheelContent.RewardCount);

                // Check if the reward is bomb or not
                if (wheelContent.RarityProperty== "Bomb")
                {
                    collectButton.gameObject.SetActive(false);
                    BombPopUp.SetActive(true);
                    BombPopUp.GetComponent<AudioSource>().Play();
                }
                else
                {
                    await AddReward(wheelContent.ContentIcon, wheelContent.RewardCount, wheelContent.Name);
                    ZoneLevel++;
                    SetWheelHandler();
                    ZonePanelSlide();
                }
            });
            wheelHandler.SpinWheel();
        }

        private void CollectClick()
        {
            // Show Collect Pop Up
            CollectPopUp.SetActive(true);
        }

        private void CollectPopUpCollectClick()
        {
            // Take to the collection scene
            GainedRewardsHandler.Instance.GainedRewards = gainedRewards;
            SceneManager.LoadScene("CollectingScene");
        }

        private void CollectPopUpGoBackClick()
        {
            // Close Pop Up
            CollectPopUp.SetActive(false);
        }

        private void RestartClick()
        {
            // Load Scene Again
            SceneManager.LoadScene("GameScene");
        }

        private void SoundClick()
        {
            if (AudioListener.volume == 0)
            {
                AudioListener.volume = 1;
                soundPropertyText.text = "On";
            }
            else
            {
                AudioListener.volume = 0;
                soundPropertyText.text = "Off";
            }
        }
        #endregion


        // Update is called once per frame
        void Update()
        {
            // If is no gained reward, deactivate the collect button
            if (gainedRewards.Count != 0)
            {
                collectButton.gameObject.SetActive(true);
            }
        }

        void SetWheelHandler()
        {
            // Clear old wheels
            if (instantiatedWheels.Count != 0)
            {
                foreach (GameObject wheelObject in instantiatedWheels)
                {
                    Destroy(wheelObject, 0f);
                }
                instantiatedWheels.Clear();
            }

            int index = 0;
            // Check zone level
            if (ZoneLevel % superZone == 0)
            {
                index = 2;

            }
            else if (ZoneLevel % safeZone == 0 || ZoneLevel == 1)
            {
                index = 1;
            }

            
            // Set wheelprefab
            AddressablesManager.Instance.WheelAssetReferences[index].InstantiateAsync(wheelPanel, false).Completed += WheelPrefabCompleted;
        }

        private void WheelPrefabCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
        {
            GameObject wheel;
            wheel = obj.Result;
            wheel.transform.SetAsFirstSibling();
            wheel.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            instantiatedWheels.Add(wheel);
            wheelHandler = wheel.transform.GetComponent<WheelHandler>();
            spinButton.gameObject.SetActive(true);
        }

        #region Zone Panel

        // Change zone level panel according to zoneLevel variable
        void SetZonePanel()
        {
            // As the zone progresses, new zone cards will be created according to ranges
            for (int i = minZone; i < minZone + 20; i++)
            {
                GameObject zone;

                zone = Instantiate(ZonePrefab);
                zone.transform.SetParent(ZoneContainer.transform, false);
                zone.transform.GetComponentInChildren<Text>().text = i.ToString();
                if (i == 1 || i % safeZone == 0)
                {
                    zone.gameObject.GetComponent<Image>().sprite = GreenZone;
                }
            }
            minZone += 20;
        }

        public void ZonePanelSlide()
        {
            // In terms of optimization, new zone cards are created as the zone progresses.
            if (ZoneLevel % 10 == 0)
            {
                SetZonePanel();
            }
            ZoneContainer.transform.GetComponent<RectTransform>().DOAnchorPosX(nextTargetRectX, 0.5f); // As the zone advances, the panel will slide to the left.
            nextTargetRectX -= 140f;
        }

        #endregion

        #region Reward Panel

        public async Task AddReward(Sprite rewardSprite, int rewardCount, string rewardName)
        {
            // Check existence of reward
            var reward = gainedRewards.Where(x => x.Name == rewardName).FirstOrDefault();
            if (reward != null)
            {
                await RewardAnimation(RewardContainer.transform.Find(rewardName).transform.GetChild(1).GetComponent<RectTransform>().position, rewardSprite);
                var count = reward.Count;
                reward.Count = count + rewardCount;
                animatedText = RewardContainer.transform.Find(rewardName).GetComponentInChildren<Text>();
                targetrewardText = reward.Count;
                textAnimationActive = true;
            }
            // If not exists, add now
            else
            {
                AddressablesManager.Instance.ObjectRewardPrefabAssetReference.InstantiateAsync(RewardContainer.transform, false).Completed += (op) =>
                {
                    RewardCompleted(op, rewardSprite, rewardCount, rewardName);
                };
                await Task.Delay(1200);
            }
        }

        private async void RewardCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj, Sprite rewardSprite, int rewardCount, string rewardName)
        {
            GameObject newReward = obj.Result;
            newReward.GetComponentInChildren<Image>().sprite = rewardSprite;
            newReward.GetComponentInChildren<Text>().text = "0";
            newReward.name = rewardName;
            await RewardAnimation(RewardContainer.transform.Find(rewardName).transform.GetChild(1).transform.GetComponent<RectTransform>().position, rewardSprite);
            animatedText = newReward.GetComponentInChildren<Text>();
            targetrewardText = rewardCount;
            textAnimationActive = true;
            Reward newRewardItem = new Reward();
            newRewardItem.Name = rewardName; newRewardItem.Sprite = rewardSprite; newRewardItem.Count = rewardCount;
            gainedRewards.Add(newRewardItem);
        }

        #endregion

        #region Basic Code Animations
        async Task RewardAnimation(Vector3 targetPos, Sprite rewardSprite)
        {
            // Get object from addressables
            AddressablesManager.Instance.ObjectRewardAnimationPrefabAssetReference.InstantiateAsync(IndicatorPosition).Completed +=  (op) =>
            {
                RewardAnimationCompleted(op, rewardSprite, targetPos);
            };
            transform.GetComponent<AudioSource>().Play();

            // Await task until the wheel changing
            await Task.Delay(1200);
        }

        private async void RewardAnimationCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj, Sprite rewardSprite, Vector3 targetPos)
        {
            GameObject rewardAnimation = obj.Result;
            rewardAnimation.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            rewardAnimation.GetComponent<Image>().sprite = rewardSprite;
            await Task.Delay(200);
            rewardAnimation.GetComponent<RectTransform>().DOMove(targetPos, 1f);
            await Task.Delay(1000);
            Destroy(rewardAnimation);
        }

        void TextAnimation()
        {
            if (textAnimationActive)
            {
                animatedText.text = (int.Parse(animatedText.text) + 10).ToString();

                if (int.Parse(animatedText.text) >= targetrewardText)
                {
                    animatedText.text = targetrewardText.ToString();
                    textAnimationActive = false;
                }
            }
        }

        #endregion
    }
}