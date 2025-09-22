using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;

public static class SpatialAwarenessExtensions
{
    public static IMixedRealitySpatialAwarenessMeshObserver GetActiveObserver(this IMixedRealitySpatialAwarenessSystem system)
    {
        var castedSpatialAwareness = (MixedRealitySpatialAwarenessSystem)system;
        var observers = castedSpatialAwareness.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
        return observers.FirstOrDefault(o => o is BaseSpatialObserver baseObs && baseObs.IsRunning);
    }

    public static void SuspendAllMeshObservers(this IMixedRealitySpatialAwarenessSystem spatialAwarenessSystem)
    {
        if (spatialAwarenessSystem == null)
        {
            Debug.LogError("Spatial awareness system is null.");
            return;
        }

        var castedSpatialAwareness = (MixedRealitySpatialAwarenessSystem)spatialAwarenessSystem;

        var meshObservers = castedSpatialAwareness.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
        
        foreach (var observer in meshObservers)
        {
            if (observer is BaseSpatialObserver baseObs && baseObs.IsRunning)
            {
                observer.Suspend();
                Debug.Log($"Suspended observer: {observer.Name}");
            }
        }
    }

}
