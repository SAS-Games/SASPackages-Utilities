using SAS.SceneManagement;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UILoadingScreen : MonoBehaviour, ILoadingScreen
{
    [Inject] private ISceneLoader _sceneLoader;
    [SerializeField] Image m_LoadingBar;
    [SerializeField] float m_FillSpeed = 0.5f;
    [SerializeField] Canvas m_LoadingCanvas;

    private float _currentProgress;
    public Action OnFadeInComplete{get;set;}
    public Action OnFadeOutComplete{get;set;}

    private void Awake()
    {
        this.InjectFieldBindings();
    }

    public void HandleProgressUpdated(float targetProgress)
    {
        _currentProgress = targetProgress;
    }

    public void SetActive(bool active)
    {
        if (active)
            _sceneLoader.OnProgressUpdated += HandleProgressUpdated;
        else
            _sceneLoader.OnProgressUpdated -= HandleProgressUpdated;

        m_LoadingBar.fillAmount = 0f;
        gameObject.SetActive(active);

    }

    public void Update()
    {
        float currentFillAmount = m_LoadingBar.fillAmount;
        float progressDifference = Mathf.Abs(currentFillAmount - _currentProgress);
        float dynamicFillSpeed = progressDifference * m_FillSpeed;

        m_LoadingBar.fillAmount = Mathf.Lerp(currentFillAmount, _currentProgress, Time.deltaTime * dynamicFillSpeed);
    }
}
