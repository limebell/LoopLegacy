using UnityEngine;

namespace LoopLegacy.Manager
{
    public class GameEssentials : MonoBehaviour
    {
        public static GameEssentials Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void DestroyEssentials()
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
}