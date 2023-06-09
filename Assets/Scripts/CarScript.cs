using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScript : MonoBehaviour
{
    private float turn, speed;

    [SerializeField] float speedMultiplier, turnMultiplier;
    [SerializeField] Rigidbody rb;

    private float distance = 0f;
    private float duration = 0f;

    private Vector3 firstPosition;
    private Vector3 firstRotation;
    private Vector3 lastPosition;

    private int updatesToNewPrediction = 0;

    public float Fitness { get; private set; }
    public bool RideEnded { get; private set; } = false;
    public NeuralNetwork NeuralNet { get; set; }

    private int sensorCount;
    private float maxRayDistance;
    [SerializeField] GameObject rayVisualizer;
    private List<LineRenderer> rayVisualizers;
    private List<Vector3> rayDirections;
    [SerializeField] LayerMask layerMask;

    private void Start()
    {       
        lastPosition = transform.position;
        firstPosition = transform.position;
        firstRotation = transform.eulerAngles;
        sensorCount = Setting.sensorCount;
        maxRayDistance = Setting.sensorLength;
        NeuralNet = new NeuralNetwork(sensorCount, Setting.hiddenLayersCount, Setting.hiddenLayerSize);
        GenerateRay();
    }

    private void GenerateRay()
    {
        rayVisualizers = new List<LineRenderer>();
        for (int i = 0; i < sensorCount; i++)
        {
            GameObject visualizer = Instantiate(rayVisualizer);
            visualizer.transform.parent = gameObject.transform;
            visualizer.transform.position = gameObject.transform.position;
            LineRenderer lr = visualizer.GetComponent<LineRenderer>();
            rayVisualizers.Add(lr);
        }

        rayDirections = new List<Vector3>();
        if (sensorCount == 1)
        {
            rayDirections.Add(transform.forward);
        }
        else if (sensorCount == 3)
        {
            rayDirections.Add((transform.forward - transform.right).normalized);
            rayDirections.Add(transform.forward);
            rayDirections.Add((transform.forward + transform.right).normalized);
        }
        else if (sensorCount == 5)
        {
            rayDirections.Add((transform.forward - transform.right).normalized);
            rayDirections.Add((2 * transform.forward - transform.right).normalized);
            rayDirections.Add(transform.forward);
            rayDirections.Add((2 * transform.forward + transform.right).normalized);
            rayDirections.Add((transform.forward + transform.right).normalized);
        }
        else if (sensorCount == 7)
        {
            rayDirections.Add((transform.forward - 2 * transform.right).normalized);
            rayDirections.Add((transform.forward - transform.right).normalized);
            rayDirections.Add((2 * transform.forward - transform.right).normalized);
            rayDirections.Add(transform.forward);
            rayDirections.Add((2 * transform.forward + transform.right).normalized);
            rayDirections.Add((transform.forward + transform.right).normalized);
            rayDirections.Add((transform.forward + 2 * transform.right).normalized);
        }
    }

    private void FixedUpdate()
    {
        if (!RideEnded)
        {           
            float[] sensorValues = new float[sensorCount];
            for (int i = 0; i < sensorValues.Length; i++)
            {
                sensorValues[i] = SensorScanning(i);
            }

            if(updatesToNewPrediction == 12)
            {
                NeuralNet.Predict(sensorValues, out turn, out speed);                
                updatesToNewPrediction = 0;
            }
            else
            {
                updatesToNewPrediction++;
            }
            Move();    
            UpdateFitness();        
        }        
    }

    private void Move()
    {
        //Vector3 currentPosition = transform.position;
        //transform.position = currentPosition + transform.forward * speed * speedMultiplier * Time.fixedDeltaTime;
        //transform.position = Vector3.Lerp(currentPosition, currentPosition + transform.forward * speed * speedMultiplier, Time.fixedDeltaTime);  //19
        rb.velocity = transform.forward * speed * speedMultiplier * Time.fixedDeltaTime * 100f; //9
        //transform.position = transform.TransformDirection(Vector3.Lerp(Vector3.zero, transform.forward * speed * speedMultiplier, Time.fixedDeltaTime));

        transform.eulerAngles += new Vector3(0f, turn * turnMultiplier, 0f); //6
        //Quaternion desiredRotation = Quaternion.Euler(0f, turn * turnMultiplier, 0f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            StopCar();
            UpdateFitness();
        }
    }

    private void UpdateFitness()
    {
        Vector3 currentPosition = transform.position;
        float distanceOfPositions = Vector3.Distance(currentPosition, lastPosition);
        distance += distanceOfPositions;
        lastPosition = currentPosition;
        duration += Time.fixedDeltaTime;

        if(duration > 7 && duration < 10 && Vector3.Distance(currentPosition, firstPosition) < 10)
        {
            distance = 0f;
            StopCar();
        }else if(duration > Setting.maxTime)
        {
            StopCar();
        }

        Fitness = distance;
    }

    private void StopCar()
    {
        RideEnded = true;
        rb.isKinematic = true;
        foreach (LineRenderer lr in rayVisualizers)
        {
            lr.enabled = false;
        }
    }


    private float SensorScanning(int rayIndex)
    {
        //nastaveni, aby pocatecni bod snimani nebyl uprostred auta, ale byl v predni casti auta
        Vector3 from = transform.position + (transform.forward * 2f) + (transform.up);
        Vector3 direction = transform.TransformDirection(rayDirections[rayIndex]);

        Ray ray = new Ray(from, direction);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, maxRayDistance, layerMask))
        {
            if (hit.distance > (3f / 4f * maxRayDistance))
            {
                VisualizeRay(rayIndex, Color.yellow, from, hit.point);
            }
            else
            {
                VisualizeRay(rayIndex, Color.red, from, hit.point);
            }
        }
        else
        {
            if(Physics.Raycast(ray, maxRayDistance + 3f, layerMask))
            {
                VisualizeRay(rayIndex, Color.yellow, from, from + direction * maxRayDistance);
            }
            else
            {
                VisualizeRay(rayIndex, Color.green, from, from + direction * maxRayDistance);
            }
            
        }
        return hit.distance / maxRayDistance;
    }

    private void VisualizeRay(int visualizerIndex, Color c, Vector3 from, Vector3 to)
    {        
        rayVisualizers[visualizerIndex].SetPosition(0, from);
        rayVisualizers[visualizerIndex].SetPosition(1, to);
        rayVisualizers[visualizerIndex].endColor = c;
        rayVisualizers[visualizerIndex].startColor = c;
    }

    public void ResetCar()
    {
        transform.position = firstPosition;
        transform.eulerAngles = firstRotation;
        lastPosition = firstPosition;

        rb.isKinematic = false;

        foreach (LineRenderer lr in rayVisualizers)
        {
            lr.enabled = true;
        }

        Fitness = 0f;
        duration = 0f;
        distance = 0f;
        RideEnded = false;
    }
}
