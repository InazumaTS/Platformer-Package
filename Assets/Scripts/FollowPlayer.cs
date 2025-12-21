using Unity.VisualScripting;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject playerGameObject;
    [SerializeField]
    private float desiredTime =3f;
    [SerializeField]
    private float YDeadZone =1.5f;
    private float elapsedTime = 0f;
    void Update()
    {
        elapsedTime += Time.deltaTime;
        float percentageComplete = elapsedTime / desiredTime;
        transform.position = new Vector3(Vector3.Lerp(transform.position, playerGameObject.transform.position,Mathf.SmoothStep(0,1, percentageComplete)).x,
                                         transform.position.y,
                                         transform.position.z);

    }
}
