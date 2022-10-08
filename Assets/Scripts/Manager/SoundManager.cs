using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Manager
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

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
            AddressablesManager.Instance.SoundtrackAssetReference.LoadAssetAsync<AudioClip>().Completed += SoundManager_Completed;
        }

        private void SoundManager_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<AudioClip> obj)
        {
            // Create audio source and set properties
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = 0.1f;
            audioSource.clip = obj.Result;
            audioSource.Play();
        }
    }
}
