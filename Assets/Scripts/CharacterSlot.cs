using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;

    public void Setup(Sprite thumbnail, string nombre)
    {
        if (iconImage != null && thumbnail != null)
            iconImage.sprite = thumbnail;

        if (nameText != null)
            nameText.text = nombre.ToUpper();
    }
}