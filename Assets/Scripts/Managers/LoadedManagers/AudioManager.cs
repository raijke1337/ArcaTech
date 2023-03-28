using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    #region singleton
    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    #endregion
    private void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        _fx = new List<AudioSource>();
    }

    [SerializeField] private AudioSource _audioPrefab;
    [SerializeField] private AudioSource _musicPrefab;
    private List<AudioSource> _fx;
    private AudioSource _m;

    public void PlaySound(AudioClip clip, Vector3 place)
    {
        var s = Instantiate(_audioPrefab, place, Quaternion.identity,transform);
        s.clip = clip;
        s.Play();
        _fx.Add(s);
        StartCoroutine(TerminateObject(s));
    }
    public void PlayMusic(AudioClip clip)
    {
        _m = Instantiate(_musicPrefab);
        _m.clip = clip;
        _m.loop = true;
        _m.Play();
    }

    private IEnumerator TerminateObject(AudioSource s)
    {
        yield return new WaitForSeconds(s.clip.length);
        _fx.ToList().Remove(s);
        Destroy(s.gameObject);
    }

    public void CleanUpOnSceneChange()
    {
        StopAllCoroutines();
        if (_fx == null) return;
        foreach (var fx in _fx.ToList())
        {
            if (fx == null) return;
            fx.Stop();
            Destroy(fx.gameObject);
            Destroy(_m.gameObject);
        }
    }

}
