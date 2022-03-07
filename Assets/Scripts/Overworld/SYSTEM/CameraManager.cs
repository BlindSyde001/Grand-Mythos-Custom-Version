using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    // VARIABLES
    [SerializeField]
    private GameObject FadeScreen;
    [SerializeField]
    internal Camera _Camera;

    public GameObject player;

    public List<Transform> ReferenceDirections;
    public List<CameraBase> myCameraSetups;

    [SerializeField]
    internal CameraBase currentCameraSetup;
    [SerializeField]
    internal CameraBase previousCameraSetup;

    // UPDATES
    private void Start()
    {
        StartCoroutine(BlackToFull());
    }

    // METHODS
    private IEnumerator BlackToFull()
    {
        FadeScreen.GetComponent<Image>().DOFade(0f, .5f);
        yield return null;
    }
    private IEnumerator FadeToBlack()
    {
        FadeScreen.GetComponent<Image>().DOFade(1f, 1f);
        yield return null;
    }
}
