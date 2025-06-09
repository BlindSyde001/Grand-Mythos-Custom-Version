using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QTE
{
    public class QTEInterface : MonoBehaviour
    {
        [Required] public TMP_Text Text;
        [Required] public Image QTETimer, InputProgress;
    }
}