using UnityEngine;
using UnityEngine.UI;

public class GachaTypeController : MonoBehaviour
{
    [Header("表示するImage")]
    public Image backgroundImage;
    public Image gachaMachineImage;
    public Image pickupImage;
    public Image gachaTitleImage;

    [Header("ノーマルガチャ")]
    public Sprite normalBackground;
    public Sprite normalGachaMachine;
    public Sprite normalPickup;
    public Sprite normalTitle;

    [Header("レアガチャ")]
    public Sprite rareBackground;
    public Sprite rareGachaMachine;
    public Sprite rarePickup;
    public Sprite rareTitle;

    [Header("コラボガチャ")]
    public Sprite collabBackground;
    public Sprite collabGachaMachine;
    public Sprite collabPickup;
    public Sprite collabTitle;

    public void ShowNormalGacha()
    {
        backgroundImage.sprite = normalBackground;
        gachaMachineImage.sprite = normalGachaMachine;
        pickupImage.sprite = normalPickup;
        gachaTitleImage.sprite = normalTitle;
    }

    public void ShowRareGacha()
    {
        backgroundImage.sprite = rareBackground;
        gachaMachineImage.sprite = rareGachaMachine;
        pickupImage.sprite = rarePickup;
        gachaTitleImage.sprite = rareTitle;
    }

    public void ShowCollabGacha()
    {
        backgroundImage.sprite = collabBackground;
        gachaMachineImage.sprite = collabGachaMachine;
        pickupImage.sprite = collabPickup;
        gachaTitleImage.sprite = collabTitle;
    }
}