using Unity.VisualScripting;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject playerGameObject;
    [SerializeField]
    private float desiredTimeX =3f;
    [SerializeField]
    private float YDeadZone =1.5f;
    [SerializeField]
    private float desiredTimeY =3f;
    enum state
    {
        TrackingX,
        TrackingXY
    }
    state currentState = state.TrackingX;
    void Update()
    {
        switch(currentState)
        {
            case state.TrackingX:
                TrackXZone();
                break;
            case state.TrackingXY:
                TrackXYZone();
                break;
        }    
    }

    void TrackXZone()
    {
      
        if(Mathf.Abs(transform.position.y - playerGameObject.transform.position.y) > YDeadZone)
        {
            currentState = state.TrackingXY;
        }

        transform.position = new Vector3(Mathf.MoveTowards(transform.position.x,playerGameObject.transform.position.x,desiredTimeX),transform.position.y,transform.position.z);
    }

    void TrackXYZone()
    {
        if(transform.position.y == playerGameObject.transform.position.y)
        {
            currentState = state.TrackingX;
        }
        transform.position = new Vector3(Mathf.MoveTowards(transform.position.x,playerGameObject.transform.position.x,desiredTimeX),
                                         Mathf.MoveTowards(transform.position.y,playerGameObject.transform.position.y,desiredTimeY),
                                         transform.position.z);
    }    
}
