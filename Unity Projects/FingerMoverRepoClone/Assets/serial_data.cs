using UnityEngine;
using System.IO.Ports;
using System.Text;
using UnityEngine.InputSystem;

public class serial_data : MonoBehaviour
{
    SerialPort data_stream = new SerialPort("COM6", 9600);
    public string receivedstring;

    // Reference to the parent object's children
    public GameObject index;
    public GameObject indexTip;

    public GameObject thumb;
    public GameObject thumbTip;

    public GameObject middle;
    public GameObject ring;
    public GameObject middleTip;
    public GameObject ringTip;
    public GameObject pinky;
    public GameObject pinkyTip;

    public float sensitivity = 1;

    private StringBuilder serialBuffer = new StringBuilder();
    private bool readingMessage = false; // Tracks if we're inside a message

    public virtual void Start()
    {
        data_stream.Open();
        Debug.Log("Start");
    }

    void Update()
    {
        if (data_stream.IsOpen)
        {
            try
            {
                while (data_stream.BytesToRead > 0)  // Actively read available bytes
                {
                    char incomingChar = (char)data_stream.ReadChar(); // Read character by character

                    if (incomingChar == '#')  // Start of a new message
                    {
                        serialBuffer.Clear();
                        readingMessage = true;
                    }
                    else if (incomingChar == ';' && readingMessage) // Match Arduino
                    {
                        readingMessage = false;
                        ProcessMessage(serialBuffer.ToString()); // Process full message
                    }
                    else if (readingMessage)
                    {
                        serialBuffer.Append(incomingChar); // Append valid characters
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

     public static float Map(float analogValue, float analogMin, float analogMax, float rotationalMin, float rotationalMax)//custom map function
    {
        return (analogValue - analogMin) * (rotationalMax - rotationalMin) / (analogMax - analogMin) + rotationalMin;
    }

    void ProcessMessage(string message)
    {
        string[] datas = message.Split(',');
        float thumbread = 0;
        float indexread = 0;
        float middleread = 0;
        float ringread = 0;
        float pinkyread = 0;
        float mappedThumbRead = 0;
        float mappedIndexRead = 0;
        float mappedMiddleRead = 0;
        float mappedRingRead = 0;
        float mappedPinkyRead = 0;



        if (datas.Length >=5) // Ensure valid data
        {
             thumbread = float.Parse(datas[0]) ;
             mappedThumbRead = Map(thumbread, 0, 1023, 0, 90);


             indexread = float.Parse(datas[1]) ;
             mappedIndexRead = Map(indexread, 0, 1023, 0, 90);

             middleread = float.Parse(datas[2]) ;
             mappedMiddleRead = Map(middleread, 0, 1023, 0, 90);


             ringread = float.Parse(datas[3]) ;
             mappedRingRead = Map(ringread, 0, 1023, 0, 90);


             pinkyread = float.Parse(datas[4]) ;
             mappedPinkyRead = Map(pinkyread, 0, 1023, 0, 90);

        }
            // Apply rotation to finger (X-axis) and thumb (Y-axis)
            if (mappedIndexRead >= 2)
            {       
                Vector3 currentIndexRotation = index.transform.eulerAngles; // Get current rotation of index

              index.transform.rotation    = Quaternion.Euler(mappedIndexRead, currentIndexRotation.y, currentIndexRotation.z);
              indexTip.transform.rotation =  Quaternion.Euler(mappedIndexRead*2, currentIndexRotation.y, currentIndexRotation.z);
            }
            if (mappedThumbRead >= 2)
            {
                Vector3 currentThumbRotation = thumb.transform.eulerAngles; // Get current rotation of index

                thumb.transform.rotation = Quaternion.Euler(mappedThumbRead, currentThumbRotation.y, currentThumbRotation.z);
                thumbTip.transform.rotation = Quaternion.Euler(mappedThumbRead* 2, currentThumbRotation.y, currentThumbRotation.z);

            }
            if (mappedMiddleRead >= 2)
            {
                Vector3 currentMiddleRotation = middle.transform.eulerAngles; // Get current rotation of index

              middle.transform.rotation    = Quaternion.Euler(mappedMiddleRead, currentMiddleRotation.y, currentMiddleRotation.z);
              middleTip.transform.rotation =  Quaternion.Euler(mappedMiddleRead*2, currentMiddleRotation.y, currentMiddleRotation.z);
            }
            if (mappedRingRead >= 2)
            {
                Vector3 currentRingRotation = ring.transform.eulerAngles; // Get current rotation of Ring

              ring.transform.rotation    = Quaternion.Euler(mappedRingRead, currentRingRotation.y, currentRingRotation.z);
              ringTip.transform.rotation =  Quaternion.Euler(mappedRingRead*2, currentRingRotation.y, currentRingRotation.z);
            }
            if (mappedPinkyRead >= 2)
            {
                Vector3 currentPinkyRotation = index.transform.eulerAngles; // Get current rotation of index

              pinky.transform.rotation    = Quaternion.Euler(mappedPinkyRead, currentPinkyRotation.y, currentPinkyRotation.z);
              pinkyTip.transform.rotation =  Quaternion.Euler(mappedPinkyRead*2, currentPinkyRotation.y, currentPinkyRotation.z);
            }

            float actualAngle = Mathf.Abs(index.transform.eulerAngles.x) % 360;

            // Map rotation angle (0� = Off, 90� = Full Brightness)
            float brightness = Mathf.Clamp((actualAngle / 90f) * 255, 0, 750);

            // Send brightness value to Arduino with start and stop characters
            string brightnessMessage = "#" + brightness.ToString("F0") + ";";
            data_stream.Write(brightnessMessage);

            // Debugging
            Debug.Log("Joystick Data: " + datas[0] + ", " + datas[1] 
            /*+ ", " + datas[2]+ ", " + datas[3]+ ", " + datas[4]*/
            );
        
     }

    private void OnApplicationQuit()
    {
        if (data_stream.IsOpen)
        {
            data_stream.Close();
        }
    }
}
