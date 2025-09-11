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
}
