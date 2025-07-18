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
        [SerializeField, Tooltip("cast ray length")] float rayLength = 7f;        
        [SerializeField, Tooltip("radius of fade sphere")] float fadeCastSphereRadius = 2f;
        [SerializeField,Tooltip("vertical offset for sphere casted, defaults to 1/2 of radius")] float fadeCastSphereVerticalOffset = 3f;
       // [SerializeField] bool aimAtCursor = false;
        
        [SerializeField] Material _fadedMaterial;
     //   [SerializeField] float _fadeSpeed = 0.5f;


        [Header("Fade out info")]
        [SerializeField] private List<FadingDecorComponent> _currentlyFadingList = new List<FadingDecorComponent>();
        private RaycastHit[] _hitsThisFrame;

        AimingComponent comp;
        IsoCameraController cam;

        Ray ray;
        float _rayL;
        float _castRad;
        Vector3 _sphereOffset;

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(ray.origin, ray.origin + (ray.direction * rayLength));
            Gizmos.DrawWireSphere(ray.origin + (ray.direction * rayLength), fadeCastSphereRadius);
            foreach (var fading in _currentlyFadingList)
            {
                Gizmos.DrawWireCube(fading.transform.position, Vector3.one);
            }
        }

        
        private void SphereCastForHiding()
        {
            Vector3 dir = comp.transform.position + _sphereOffset - transform.position;
            ray = new Ray(transform.position,dir);

            int hits = Physics.SphereCastNonAlloc(ray, _castRad, _hitsThisFrame, _rayL);
            
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
                            fading.Fade(0f, _fadedMaterial);
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
            cam = FindObjectOfType<IsoCameraController>();

            _rayL = rayLength;
            _castRad = fadeCastSphereRadius;
           // fadeCastSphereVerticalOffset = _castRad / 2;
            _sphereOffset = new Vector3(0, fadeCastSphereVerticalOffset, 0);

            //transform.localPosition = new Vector3(comp.transform.position.x, comp.transform.position.y + _curOfset, comp.transform.position.z);


            transform.SetParent(cam.transform, false);
            _hitsThisFrame = new RaycastHit[50];
        }
        private void Update()
        {
            if (_hitsThisFrame == null || comp == null || cam == null) return;
// update serialized settings
            if (_castRad != fadeCastSphereRadius) { _castRad = fadeCastSphereRadius; }
            if (_rayL != rayLength) { _rayL = rayLength; }
            if (_sphereOffset.y != fadeCastSphereVerticalOffset) { _sphereOffset.y = fadeCastSphereVerticalOffset; }
           // if (aimAtCursor) { transform.LookAt(comp.GetLookTarget); }
            SphereCastForHiding();
        }

    }


}

