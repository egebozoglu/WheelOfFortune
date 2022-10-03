using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Manager
{

    [Serializable]
    public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
    {
        public AssetReferenceAudioClip(string guid) : base(guid) { }
    }

    public class AddressablesManager : MonoBehaviour
    {
        #region Variables

        public static AddressablesManager Instance;

        public AssetReference[] WheelAssetReferences;

        public AssetReference ObjectRewardPrefabAssetReference;

        public AssetReference ObjectRewardAnimationPrefabAssetReference;

        [SerializeField] private AssetReferenceAudioClip soundtrackAssetReference;

        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
        }

        private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
        {
            Debug.Log("Addressables Initialized.");

            soundtrackAssetReference.LoadAssetAsync<AudioClip>().Completed += (op) =>
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = true;
                audioSource.volume = 0.1f;
                audioSource.clip = op.Result;
                audioSource.Play();
            };
        }

        // Update is called once per frame
        void Update()
        {
            // Check ound property to adjust volume
            if (PlayerPrefs.GetString("Sound") == "Off")
            {
                AudioListener.volume = 0;
            }
            else
            {
                AudioListener.volume = 1;
            }
        }
    }
}