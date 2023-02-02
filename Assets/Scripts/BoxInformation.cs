using System;
using Boxes;
using UnityEngine;

public class BoxInformation : MonoBehaviour
{
    private ForkliftMain _forklift;
    public int floorLayer = 7;
    public int shelfLayer = 8;
    public Boxes.Types boxType;

    private void Start()
    {
        _forklift = ForkliftMain.GetInstance();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == floorLayer)
            _forklift.ForkHitGround();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != shelfLayer) return;

        var shelfZone = other.GetComponent<ShelfZone>();
        var same = (int)shelfZone.zoneType - 1 == (int)boxType;
        if (same)
            shelfZone.AlertSame(this);
        else if (shelfZone.zoneType == Zones.Mixed)
            shelfZone.AlertMixed(this);
        else 
            shelfZone.AlertWrong(this);
    }
}
