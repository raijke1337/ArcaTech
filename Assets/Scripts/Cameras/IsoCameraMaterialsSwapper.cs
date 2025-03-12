using Arcatech.Level;
using Arcatech.Units.Inputs;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;
namespace Arcatech.Scenes.Cameras
{
    public class IsoCameraMaterialsSwapper : MonoBehaviour
    {

        [Header("Fade out Settings")]
        [SerializeField, Tooltip("vert Offset from player")] float verticalOffset = 10f;        
        [SerializeField, Tooltip("radius of fade sphere")] float _fadeCastRad = 5f;
        
        [SerializeField] Material _fadedMaterial;
        [SerializeField] float _fadeSpeed = 0.5f;


        [Header("Fade out info")]
        [SerializeField] private List<FadingDecorComponent> _currentlyFadingList = new List<FadingDecorComponent>();
        private RaycastHit[] _hitsThisFrame;

        AimingComponent comp;
        Ray ray;
        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(ray);
        }

        
        private void SphereCastForHiding()
        {
            Vector3 dir = comp.transform.position - transform.position;
            ray = new Ray(transform.position,dir);

            int hits = Physics.SphereCastNonAlloc(ray, _fadeCastRad, _hitsThisFrame, verticalOffset-(_fadeCastRad/2));
            
            //add relevant
            if (hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    FadingDecorComponent fading = GetFadingDecorFromHit(_hitsThisFrame[i]);
                    // see if there are objects hitting the raycast target from camera

                    if (fading != null)
                    {
                        //Debug.Log($"Raycast on relevant object {fading.gameObject.name}");
                        if (!_currentlyFadingList.Contains(fading))
                        {
                            _currentlyFadingList.Add(fading);
                            fading.Fade(_fadeSpeed, _fadedMaterial);
                        }
                    }
                }
                //remove irrelevant
                List<FadingDecorComponent> toRemove = new List<FadingDecorComponent>(_currentlyFadingList.Count);

                foreach (FadingDecorComponent decor in _currentlyFadingList)
                {
                    if (decor != null)
                    {
                        bool isDecorInFrameResults = false;
                        for (int i = 0; i < _hitsThisFrame.Length; i++)
                        {
                            FadingDecorComponent compThisFrame = GetFadingDecorFromHit(_hitsThisFrame[i]);
                            if (decor == compThisFrame)
                            {
                                isDecorInFrameResults = true;
                                break;
                                // comp in list still hit
                            }
                        }

                        if (!isDecorInFrameResults)
                        {
                            toRemove.Add(decor);
                        }
                    }
                }
                foreach (FadingDecorComponent decor in toRemove)
                {
                    // Debug.Log($"Remove from fading list {decor}");
                    _currentlyFadingList.Remove(decor);
                    decor.Unfade();
                }


                //clear hits storage
                System.Array.Clear(_hitsThisFrame, 0, _hitsThisFrame.Length);

            }
        }

        private FadingDecorComponent GetFadingDecorFromHit(RaycastHit hit)
        {
            return hit.collider != null ? hit.collider.GetComponent<FadingDecorComponent>() : null;
        }
        private void Start()
        {
            comp = FindObjectOfType<AimingComponent>();
            transform.position = new Vector3(comp.transform.position.x, comp.transform.position.y + verticalOffset, comp.transform.position.z);
            transform.LookAt(comp.transform.position);
            transform.SetParent(comp.transform, true);

            _hitsThisFrame = new RaycastHit[50];
        }
        private void Update()
        {
            if (_hitsThisFrame == null || comp == null) return;
            SphereCastForHiding();
        }

    }


}

