using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkController : MonoBehaviour
{
    public float speed = 5;
    [SerializeField] [MinMaxSlider(1, 60)] private Vector2 rotateTimeConstraint;
    [SerializeField] [MinMaxSlider(0, 360)] private Vector2 rotateAngleConstraints;
    private bool rotateGenerated = false;
    private float timer;
    [SerializeField] private float rotationTime = 3f;
    private float currentAngle;

    // Start is called before the first frame update
    void Start()
    {
        currentAngle = GenerateRotateAngle();
    }

    // Update is called once per frame
    void Update()
    {
        //Always move the shark forward
        transform.position += transform.up * speed * Time.deltaTime;
        //Start the coroutine when the player finishes a rotation
        if(!rotateGenerated)
            StartCoroutine(RotateShark(GenerateRotateTime()));
    }

    private float GenerateRotateTime()
    {
        rotateGenerated = true;
        return Random.Range(rotateTimeConstraint.x, rotateTimeConstraint.y);
    }

    private float GenerateRotateAngle()
    {
        return Random.Range(rotateAngleConstraints.x, rotateAngleConstraints.y);
    }

    IEnumerator RotateShark(float rotateTime)
    {
        //Pause in between rotations
        yield return new WaitForSeconds(rotateTime);

        Debug.Log("Starting Rotation...");

        Quaternion startRotation = transform.rotation;
        float startTime = Time.time;
        float lerpSpeed = currentAngle / rotationTime;

        float distanceCovered = (Time.time - startTime) * lerpSpeed;
        float progress = distanceCovered / currentAngle;

        //Lerp rotation of shark
        while (progress < 1)
        {
            distanceCovered = (Time.time - startTime) * lerpSpeed;
            progress = distanceCovered / currentAngle;
            transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(new Vector3(90, 0, transform.rotation.z + currentAngle)), progress);
            timer = Mathf.Lerp(timer, 1f, rotationTime * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Rotation Done");

        rotateGenerated = false;
        //Get a new angle and add to the current angle (not 100% accurate but close enough?)
        currentAngle = transform.eulerAngles.y + GenerateRotateAngle();
        Debug.Log("Angle: "+ currentAngle);
    }
}
