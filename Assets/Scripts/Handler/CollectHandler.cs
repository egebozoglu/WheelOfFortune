using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Manager;
using Model;

namespace Handler
{

    public class CollectHandler : MonoBehaviour
    {
        #region Variables
        private List<Reward> gainedRewards = new List<Reward>();

        [Header("Reward Values")]
        [SerializeField] private GameObject rewardObject;
        [SerializeField] private Text rewardName;
        [SerializeField] private Text rewardCount;
        [SerializeField] private Image rewardImage;

        [Header("Reward Panel")]
        [SerializeField] private RectTransform rewardGardient;
        [SerializeField] private GameObject rewardContainer;
        private Reward reward;

        [Header("Others")]
        // Reward Animation
        [SerializeField] private RectTransform rewardBG;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button soundButton;
        private bool firstLoad = true;
        private int rewardIndex = 0;
        private float rewardTime = 0f;
        private float rewardRate = 2.4f;
        private bool animationOn = true;

        [Header("Sound Button Section")]
        [SerializeField] private Text localizeSoundText;
        [SerializeField] private Text soundPropertyText;
        public string LocalizeSound;
        #endregion

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

            gainedRewards = GainedRewardsHandler.Instance.GainedRewards;

            ButtonListeners();
        }

        void ButtonListeners()
        {
            restartButton.onClick.AddListener(RestartClick);
            soundButton.onClick.AddListener(SoundClick);
        }

        #region Button Voids
        private void RestartClick()
        {
            // Load Main Game Scene
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
            // Star Animation
            rewardBG.Rotate(new Vector3(0f, 0f, 100f) * Time.deltaTime);

            StartCoroutine(RewardAnimation());
        }

        private IEnumerator RewardAnimation()
        {
            // Wait a little bit on load
            if (firstLoad)
            {
                yield return new WaitForSeconds(0.5f);
                firstLoad = false;
                if (rewardObject != null)
                    rewardObject.SetActive(true);
            }

            if (animationOn && rewardName != null && rewardCount != null && rewardImage != null)
            {
                // Set Reward Values
                reward = gainedRewards[rewardIndex];
                rewardName.text = reward.Name;
                rewardCount.text = reward.Count.ToString();
                rewardImage.sprite = reward.Sprite;
                ShowRewardsInOrder();
            }
            else
            {
                rewardObject.SetActive(false);
            }
        }

        void ShowRewardsInOrder()
        {
            rewardTime += Time.deltaTime;
            if (rewardTime > rewardRate)
            {
                rewardTime = 0f;

                // Avoid from out of range error, check index value
                if (rewardIndex + 1 >= gainedRewards.Count)
                {
                    animationOn = false;
                    rewardGardient.DOAnchorPosX(0f, 2f);
                }
                else
                {
                    rewardIndex++;
                }

                AddReward(reward.Sprite, reward.Count);
            }
        }

        void AddReward(Sprite rewardSprite, int rewardCount)
        {
            // Add Reward to panel after show on the center
            AddressablesManager.Instance.ObjectRewardPrefabAssetReference.InstantiateAsync(rewardContainer.transform, false).Completed += (op) =>
            {
                RewardCompleted(op, rewardSprite, rewardCount);
            };
        }

        private void RewardCompleted(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj, Sprite rewardSprite, int rewardCount)
        {
            GameObject newReward = obj.Result;
            newReward.transform.SetAsFirstSibling();
            newReward.GetComponentInChildren<Image>().sprite = rewardSprite;
            newReward.GetComponentInChildren<Text>().text = rewardCount.ToString();
            transform.GetComponent<AudioSource>().Play();
        }
        
    }
}
