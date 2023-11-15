using TMPro;
using UnityEngine;

public class VersionShower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;

    private void Start()
    {
        versionText.text = Application.version;
    }
}