using UnityEngine;

public class OlumculNesne : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Çarpan şey oyuncu mu?
        PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

        if (player != null)
        {
            Debug.Log("Dolap altında kaldın!");
            player.AnindaOldur(); // Tek at!
        }
    }
}