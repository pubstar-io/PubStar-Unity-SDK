// using UnityEngine;

// public class PlayerController : MonoBehaviour
// {
//     // Private variables
//     private float speed = 7.0f;
//     private float turnSpeed = 1;
//     private float horizontalInput;
//     private float forwardInput;


//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // Get player input
//          turnSpeed = 45.0f;
//         horizontalInput = Input.GetAxis("Horizontal");
//         forwardInput = Input.GetAxis("Vertical");

//         // Move the vehicle forward/backward and turn
//         transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);

//         // transform.Translate(Vector3.right * Time.deltaTime * turnSpeed * horizontalInput);
//         transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * horizontalInput);
//     }
// }

using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 20f;
    public float backwardSpeed = 10f;

    // Khu vực màn hình dùng để tiến (ví dụ: nửa trên)
    private Rect forwardArea;
    // Khu vực màn hình dùng để lùi (ví dụ: nửa dưới)
    private Rect backwardArea;

    void Start()
    {
        // Định nghĩa vùng chạm:
        // Nửa trên màn hình: (0, 0.5) đến (1, 1) theo tỷ lệ 0-1
        forwardArea = new Rect(0, 0.5f, 1, 0.5f); 
        // Nửa dưới màn hình: (0, 0) đến (1, 0.5)
        backwardArea = new Rect(0, 0, 1, 0.5f);
    }

    void Update()
    {
        // Kiểm tra xem có bất kỳ ngón tay nào đang chạm vào màn hình không
        // if (Input.touchCount > 0)
        // {
        //     // Lặp qua tất cả các điểm chạm hiện tại
        //     foreach (Touch touch in Input.touches)
        //     {
        //         // Chuyển tọa độ điểm chạm từ pixel sang tỷ lệ 0-1
        //         Vector2 normalizedTouchPos = new Vector2(
        //             touch.position.x / Screen.width,
        //             touch.position.y / Screen.height
        //         );

        //         // Nếu chạm vào khu vực TIẾN
        //         if (forwardArea.Contains(normalizedTouchPos))
        //         {
        //             // Di chuyển nhân vật TIẾN theo trục Z (hoặc trục forward của nhân vật)
        //             transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        //         }
        //         // Nếu chạm vào khu vực LÙI
        //         else if (backwardArea.Contains(normalizedTouchPos))
        //         {
        //             // Di chuyển nhân vật LÙI
        //             transform.Translate(Vector3.back * backwardSpeed * Time.deltaTime);
        //         }
        //     }
        // }
    }
}