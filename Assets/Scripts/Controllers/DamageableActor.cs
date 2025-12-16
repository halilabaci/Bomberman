using UnityEngine;

namespace DPBomberman.Controllers
{
    public class DamageableActor : MonoBehaviour
    {
        [Header("Actor")]
        public bool isPlayer = false;

        public bool IsDead { get; private set; }

        public void Kill()
        {
            if (IsDead) return;
            IsDead = true;

            if (isPlayer)
            {
                Debug.Log("[DEATH] Player died.");
                // Faz 1 için minimum: player'ý kapat
                gameObject.SetActive(false);

                // Ýstersen burada State'e dönersin (MainMenu/GameOver) ama þimdilik log yeter
            }
            else
            {
                Debug.Log("[DEATH] Enemy died.");
                Destroy(gameObject);
            }
        }
    }
}
