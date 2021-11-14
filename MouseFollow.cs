using UnityEngine;
using System.Collections;
public class MouseFollow : MonoBehaviour {
    private Vector3 mousePosition;
    public float moveSpeed = 1f;
    // Use this for initialization
    void Start () {
 
    }
 
    // Update is called once per frame
    void Update () {
        if(Camera.main == null){ return;}
        mousePosition = Input.mousePosition;
        mousePosition.z = 38.1f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        GrubTrain.GrubTrain.Instance.Log(Input.mousePosition);
        GrubTrain.GrubTrain.Instance.Log(mousePosition);
        transform.position = Vector2.Lerp(transform.position, mousePosition, moveSpeed);
        GrubTrain.GrubTrain.Instance.Log(transform.position);
    }
}