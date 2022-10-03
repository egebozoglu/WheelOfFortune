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

        [SerializeField] public AssetReferenceAudioClip SoundtrackAssetReference;

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
        }
    }
}