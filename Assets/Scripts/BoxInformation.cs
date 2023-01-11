using UnityEngine;

public class BoxInformation : MonoBehaviour
{
    private ForkliftMain _forklift;
    public int floorLayer = 7;
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
}
