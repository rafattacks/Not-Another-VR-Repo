﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// Keep track of all grabbables within this trigger
    /// </summary>
    public class GrabbablesInTrigger : MonoBehaviour {

        /// <summary>
        /// All grabbables in trigger that are considered valid
        /// </summary>
        public Dictionary<Collider, Grabbable> NearbyGrabbables;

        /// <summary>
        /// All nearby Grabbables that are considered valid. I.e. Not being held, within range, etc.
        /// </summary>
        public Dictionary<Collider, Grabbable> ValidGrabbables;

        /// <summary>
        /// The closest valid grabbable. If grab button is pressed this is the object that will be grabbed.
        /// </summary>
        public Grabbable ClosestGrabbable;

        /// <summary>
        /// All grabbables in trigger that are considered valid
        /// </summary>
        public Dictionary<Collider, Grabbable> ValidRemoteGrabbables;

        /// <summary>
        /// Closest Valid Remote Grabbable may be highlighted
        /// </summary>
        public Grabbable ClosestRemoteGrabbable;

        /// <summary>
        /// Should we call events on grabbables
        /// </summary>
        public bool FireGrabbableEvents = true;

        // Cache these variables for GC
        private Grabbable _closest;
        private float _lastDistance;
        private float _thisDistance;
        private Dictionary<Collider, Grabbable> _valids;
        private Dictionary<Collider, Grabbable> _filtered;

        void Start() {
            NearbyGrabbables = new Dictionary<Collider, Grabbable>();
            ValidGrabbables = new Dictionary<Collider, Grabbable>();
            ValidRemoteGrabbables = new Dictionary<Collider, Grabbable>();
        }

        void Update() {
            // Sort Grabbales by Distance so we can use that information later if we need it
            updateClosestGrabbable();
            updateClosestRemoteGrabbables();
        }

        void updateClosestGrabbable() {

            // Remove any Grabbables that may have been destroyed, deactivated, etc.
            NearbyGrabbables = sanitizeGrabbables(NearbyGrabbables);

            // Find any grabbables that can potentially be picked up
            ValidGrabbables = getValidGrabbables(NearbyGrabbables);

            // Assign closest grabbable
            ClosestGrabbable = GetClosestGrabbable(ValidGrabbables);
        }

        void updateClosestRemoteGrabbables() {

            // Assign closest remote grabbable
            ClosestRemoteGrabbable = GetClosestGrabbable(ValidRemoteGrabbables, true);

            // We can't have a closest remote grabbable if we are over a grabbable.
            // The closestGrabbable always takes precedent of the closestRemoteGrabbable
            if (ClosestGrabbable != null) {
                ClosestRemoteGrabbable = null;
            }
        }

        public Grabbable GetClosestGrabbable(Dictionary<Collider, Grabbable> grabbables, bool remoteOnly = false) {
            _closest = null;
            _lastDistance = 9999f;

            if(grabbables == null) {
                return null;
            }

            foreach (var kvp in grabbables) {                

                if (kvp.Value == null || !kvp.Value.IsValidGrabbable()) {
                    continue;
                }

                // Use Collider transform as position
                _thisDistance = Vector3.Distance(kvp.Value.transform.position, transform.position);
                if (_thisDistance < _lastDistance && kvp.Value.isActiveAndEnabled && _thisDistance < kvp.Value.RemoteGrabDistance) {
                    // Not a valid option
                    if (remoteOnly && !kvp.Value.RemoteGrabbable) {
                        continue;
                    }

                    // This is now our closest grabbable
                    _lastDistance = _thisDistance;
                    _closest = kvp.Value;
                }
            }

            return _closest;
        }

        Dictionary<Collider, Grabbable> getValidGrabbables(Dictionary<Collider, Grabbable> grabs) {
            _valids = new Dictionary<Collider, Grabbable>();

            if (grabs == null) {
                return _valids;
            }

            // Check for objects that need to be removed from RemoteGrabbables
            foreach (var kvp in grabs) {
                if (isValidGrabbale(kvp.Key, kvp.Value) && !_valids.ContainsKey(kvp.Key)) {
                    _valids.Add(kvp.Key, kvp.Value);
                }
            }

            return _valids;
        }

        bool isValidGrabbale(Collider col, Grabbable grab) {

            // Object has been deactivated. Remove it
            if (col == null || grab == null || !grab.isActiveAndEnabled || !col.enabled) {
                return false;
            }
            // Not considered grabbable any longer. May have been picked up, marked, etc.
            else if (!grab.IsValidGrabbable()) {
                return false;
            }
            // Snap Zone without an item isn't a valid grab. Want to skip this unless something is inside
            else if(grab.GetComponent<SnapZone>() != null && grab.GetComponent<SnapZone>().HeldItem == null) {
                return false;
            }
            // Position was manually set outside of break distance
            // No longer possible for it to be the closestGrabbable
            else if (grab == ClosestGrabbable) {
                if (Vector3.Distance(grab.transform.position, transform.position) > grab.BreakDistance) {
                    return false;
                }
            }

            return true;
        }

        Dictionary<Collider, Grabbable> sanitizeGrabbables(Dictionary<Collider, Grabbable> grabs) {
            _filtered = new Dictionary<Collider, Grabbable>();

            if (grabs == null) {
                return _filtered;
            }

            foreach (var g in grabs) {
                if (g.Key != null && g.Key.enabled && g.Value.isActiveAndEnabled) {

                    // If outside of distance then this collider may have been disabled / re-enabled. Scrub from Nearby
                    if (Vector3.Distance(g.Key.transform.position, transform.position) > g.Value.BreakDistance) {
                        continue;
                    }

                    _filtered.Add(g.Key, g.Value);
                }
            }

            return _filtered;
        }

        public void AddNearbyGrabbable(Collider col, Grabbable grabObject) {

            if (grabObject != null && !NearbyGrabbables.ContainsKey(col)) {
                NearbyGrabbables.Add(col, grabObject);
            }
        }

        public void RemoveNearbyGrabbable(Collider col, Grabbable grabObject) {
            if (grabObject != null && NearbyGrabbables.ContainsKey(col)) {
                NearbyGrabbables.Remove(col);
            }
        }

        public void RemoveNearbyGrabbable(Grabbable grabObject) {
            if (grabObject != null) {

                foreach (var x in NearbyGrabbables) {
                    if (x.Value == grabObject) {
                        NearbyGrabbables.Remove(x.Key);
                        break;
                    }
                }
            }
        }

        public void AddValidRemoteGrabbable(Collider col, Grabbable grabObject) {
            if (grabObject != null && grabObject.RemoteGrabbable && !ValidRemoteGrabbables.ContainsKey(col)) {
                ValidRemoteGrabbables.Add(col, grabObject);
            }
        }

        public void RemoveValidRemoteGrabbable(Collider col, Grabbable grabObject) {
            if (grabObject != null && ValidRemoteGrabbables != null && ValidRemoteGrabbables.ContainsKey(col)) {
                ValidRemoteGrabbables.Remove(col);
            }
        }

        void OnTriggerEnter(Collider other) {
            Grabbable g = other.GetComponent<Grabbable>();
            if (g != null) {
                AddNearbyGrabbable(other, g);
            }
        }

        void OnTriggerExit(Collider other) {
            Grabbable g = other.GetComponent<Grabbable>();
            if (g != null) {
                RemoveNearbyGrabbable(other, g);
            }
        }
    }
}

