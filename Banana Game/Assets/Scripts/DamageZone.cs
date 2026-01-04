using UnityEngine;

public class DamageZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Çarpan şey oyuncu mu? (PlayerController scripti var mı?)
        PlayerHealth playerCan = other.GetComponent<PlayerHealth>();

        if (playerCan != null)
        {
            // Oyuncuyu bulduk!
            // Darbe yönü hesapla: (Oyuncu Pozisyonu - Tuzak Pozisyonu)
            // Bu sayede oyuncu tuzağın neresindeyse tam tersi yöne uçar.
            Vector3 darbeYonu = other.transform.position - transform.position;

            // Oyuncuya hasar ver
            playerCan.HasarAl(darbeYonu);
        }
    }
}