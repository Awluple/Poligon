using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SearchingArea {

    public Vector3 Position;
    public bool AreaChecked;
    public bool IsLargeArea;

    public SearchingArea(Vector3 position, bool areaChecked, bool isLargeArea) {
        Position = position;
        AreaChecked = areaChecked;
        IsLargeArea = isLargeArea;
    }
}
