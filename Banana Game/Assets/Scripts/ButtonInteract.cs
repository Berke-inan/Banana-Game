using UnityEngine;

public class ButtonInteract : MonoBehaviour
{
    [Header("Hangi Kapıyı Açacak?")]
    public DoorController bagliKapi; // Inspector'dan kapıyı seçeceğiz

    public void ButonaBasildi()
    {
        Debug.Log("Butona basıldı!");
        // Burada butonun kendisine basılma animasyonu eklenebilir

        if (bagliKapi != null)
        {
            bagliKapi.kapiyiTetikle();
        }
    }
}