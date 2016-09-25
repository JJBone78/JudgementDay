using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Strategic Camera")]
public class StrategicCameraManager : MonoBehaviour
{
    // scroll, zoom, rotate and incline speeds
    public float _scroll_speed = 2.5f;
    public float _zoom_speed = 4f;
    public float _rotate_speed = 1f;
    public float _incline_speed = 2.5f;

    // speed factors for key-controlled zoom, rotate and incline
    public float _key_zoom_speed_factor = 2f;
    public float _key_rotate_speed_factor = 1.5f;
    public float _key_incline_speed_factor = 0.5f;

    // scroll, zoom, rotate and incline smoothnesses (higher values make the camera take more time to reach its intended position)
    public float _scroll_and_zoom_smooth = 0.08f;
    public float _rotate_and_incline_smooth = 0.03f;

    // initital values for lookAt, zoom, rotation and inclination
    public float _initial_rotation = 0f; // 1 is one revolution
    public float _initial_inclination = 0.3f; // 0 is top-down, 1 is parallel to the x-z-plane
    public float _initial_zoom = 10f; // distance between camera position and the point on the x-z-plane the camera looks at
    public Vector3 _initial_look_at = new Vector3(0f, 0f, 0f);

    // zoom to or from cursor modes
    public bool _zoom_out_from_cursor = false;
    public bool _zoom_in_to_cursor = true;

    // snapping rotation or inclination back to initial values after rotation or inclination keys have been released
    public bool _snap_back_rotation = false;
    public bool _snap_back_inclination = false;

    // snapping speed (for rotation and inclination)
    public float _snap_back_speed = 6f;

    // zooming in increases the inclination (from minInclination to maxInclination)
    public bool _inclination_by_zoom = false;

    // scroll, zoom, and inclination boundaries
    public float _min_x = -10f;
    public float _max_x = 10f;
    public float _min_z = -10f;
    public float _max_z = 10f;
    public float _min_zoom = 0.4f;
    public float _max_zoom = 40f;
    public float _min_inclination = 0f;
    public float _max_inclination = 0.9f;
    public float _min_rotation = -1f;
    public float _max_rotation = 1f;

    // x-wrap and z-wrap (when reaching the boundaries the cam will jump to the other side, good for continuous maps)
    //public bool xWrap = false;
    //public bool zWrap = false;

    // keys for different controls
    public KeyCode _key_scroll_forward = KeyCode.UpArrow;
    public KeyCode _key_scroll_left = KeyCode.LeftArrow;
    public KeyCode _key_scroll_back = KeyCode.DownArrow;
    public KeyCode _key_scroll_right = KeyCode.RightArrow;
    public KeyCode _key_rotate_and_incline = KeyCode.Space;
    public KeyCode _key_zoom_in = KeyCode.None;
    public KeyCode _key_zoom_out = KeyCode.None;
    public KeyCode _key_rotate_left = KeyCode.None;
    public KeyCode _key_rotate_right = KeyCode.None;
    public KeyCode _key_incline_up = KeyCode.None;
    public KeyCode _key_incline_down = KeyCode.None;

    // width of the scroll sensitive area at the screen boundaries in px
    private int _scroll_boundaries = 2;

    // target camera values (approached asymptotically for smoothing)
    private Vector3 _target_look_at = new Vector3(0f, 0f, 0f);
    private float _target_zoom = 10f;
    private float _target_rotation = 0f;
    private float _target_inclination = 0f;

    // current camera values
    private Vector3 _current_look_at = new Vector3(0f, 0f, 0f);
    private float _current_zoom = 10f;
    private float _current_rotation = 0f;
    private float _current_inclination = 0.3f;

    // snapping takes constant time (regardless of snapping distance), these values store where the snapping began (when the rotate and incline button was released)
    private float _snap_rotation = 0f;
    private float _snap_inclination = 0f;

    // for calculations
    private Vector3 _view_direction = new Vector3(0f, 0f, 0f);
    private float _zoom_request = 0f;
    private float _rotate_request = 0f;
    private float _incline_request = 0f;
    private float _target_snap = 0f;
    private Vector3 _x_direction = new Vector3(0f, 0f, 0f);
    private Vector3 _y_direction = new Vector3(0f, 0f, 0f);
    private float _old_target_zoom = 0f;
    private float _zoom_part = 0f;
    private Vector3 _cursor_position = new Vector3(0f, 0f, 0f);
    private Vector3 _cursor_difference = new Vector3(0f, 0f, 0f);

    // for calculations (making camera behaviour framerate-independent)
    private float _time = 0f;

    // to determine if camera is currently being rotated by the user
    private bool _rotating = false;
    private Vector3 _last_mouse_position = new Vector3(0f, 0f);

    // for trigonometry
    private static float TAU = Mathf.PI * 2;


    // access methods for camera values for "jumps"
    private float _desired_zoom = 0f;
    private bool _change_zoom = false;

    private float _desired_rotation = 0f;
    private bool _change_rotation = false;

    private float _desired_inclination = 0f;
    private bool _change_inclination = false;

    private Vector3 _desired_look_at = new Vector3(0f, 0f, 0f);
    private bool _change_look_at = false;

    public void _set_zoom(float _zoom)
    {
        if (_zoom < _min_zoom)
            _zoom = _min_zoom;
        if (_zoom > _max_zoom)
            _zoom = _max_zoom;
        _desired_zoom = _zoom;
        _change_zoom = true;
    }

    public void _set_rotation(float _rotation)
    {
        if (_rotation < _min_rotation)
            _rotation = _min_rotation;
        if (_rotation > _max_rotation)
            _rotation = _max_rotation;
        _desired_rotation = _rotation;
        _change_rotation = true;
    }

    public void _set_inclination(float _inclination)
    {
        if (_inclination < _min_inclination)
            _inclination = _min_inclination;
        if (_inclination > _max_inclination)
            _inclination = _max_inclination;
        _desired_inclination = _inclination;
        _change_inclination = true;
    }

    public void _set_look_at(Vector3 _look_at)
    {
        if (_look_at.x < _min_x)
            _look_at.x = _min_x;
        if (_look_at.x > _max_x)
            _look_at.x = _max_x;
        if (_look_at.z < _min_z)
            _look_at.z = _min_z;
        //if (lookAt.z > maxZ) lookAt.z = maxZ; //automatically set the z axis to top of the map
        _look_at.y = 0f;
        _desired_look_at = _look_at;
        _change_look_at = true;
    }

    void Start()
    {
        //enforce sanity values (preventing weird effects or NaN results due to division by 0
        if (_scroll_and_zoom_smooth < 0.01f)
            _scroll_and_zoom_smooth = 0.01f;
        if (_rotate_and_incline_smooth < 0.01f)
            _rotate_and_incline_smooth = 0.01f;

        if (_min_inclination < 0.001f)
            _min_inclination = 0.001f;

        //enforce that initial values are within boundaries
        if (_initial_inclination < _min_inclination)
            _initial_inclination = _min_inclination;
        if (_initial_inclination > _max_inclination)
            _initial_inclination = _max_inclination;

        if (_initial_rotation < _min_rotation)
            _initial_rotation = _min_rotation;
        if (_initial_rotation > _max_rotation)
            _initial_rotation = _max_rotation;

        if (_initial_zoom < _min_zoom)
            _initial_zoom = _min_zoom;
        if (_initial_zoom > _max_zoom)
            _initial_zoom = _max_zoom;

        _initial_look_at.y = 0f;
        if (_initial_look_at.x > _max_x)
            _initial_look_at.x = _max_x;
        if (_initial_look_at.x < _min_x)
            _initial_look_at.x = _min_x;
        if (_initial_look_at.z > _max_z)
            _initial_look_at.z = _max_z;
        if (_initial_look_at.z < _min_z)
            _initial_look_at.z = _min_z;

        //initialise current camera values
        _current_zoom = _initial_zoom;
        _current_inclination = _initial_inclination;
        _current_rotation = _initial_rotation;
        _current_look_at = _initial_look_at;

        //initialise target camera values (to current camera values)
        _target_look_at = _current_look_at;
        _target_inclination = _current_inclination;
        _target_zoom = _current_zoom;
        _target_rotation = _current_rotation;
    }

    void Update()
    {
        // handle the "jumps"
        if (_change_zoom)
        {
            _target_zoom = _desired_zoom;
            _change_zoom = false;
        }
        if (_change_rotation)
        {
            _target_rotation = _desired_rotation;
            _change_rotation = false;
        }
        if (_change_look_at)
        {
            _target_look_at = _desired_look_at;
            _change_look_at = false;
        }
        if (_change_inclination)
        {
            _target_inclination = _desired_inclination;
            _change_inclination = false;
        }

        _time = 0.02f; //Time.deltaTime;

        _y_direction.x = transform.up.x; //determine directions the camera should move in when scrolling
        _y_direction.y = 0;
        _y_direction.z = transform.up.z;
        _y_direction = _y_direction.normalized;
        _x_direction.x = transform.right.x;
        _x_direction.y = 0;
        _x_direction.z = transform.right.z;
        _x_direction = _x_direction.normalized;

        // scrolling when the cursor touches the screen boundaries or when arrows keys are used
        if (Input.mousePosition.x >= Screen.width - _scroll_boundaries || Input.GetKey(_key_scroll_right))
        {
            _target_look_at.x = _target_look_at.x + _x_direction.x * _scroll_speed * _time * _target_zoom;
            _target_look_at.z = _target_look_at.z + _x_direction.z * _scroll_speed * _time * _target_zoom;
        }
        if (Input.mousePosition.x <= _scroll_boundaries || Input.GetKey(_key_scroll_left))
        {
            _target_look_at.x = _target_look_at.x - _x_direction.x * _scroll_speed * _time * _target_zoom;
            _target_look_at.z = _target_look_at.z - _x_direction.z * _scroll_speed * _time * _target_zoom;
        }
        if (Input.mousePosition.y >= Screen.height - _scroll_boundaries || Input.GetKey(_key_scroll_forward))
        {
            _target_look_at.x = _target_look_at.x + _y_direction.x * _scroll_speed * _time * _target_zoom;
            _target_look_at.z = _target_look_at.z + _y_direction.z * _scroll_speed * _time * _target_zoom;
        }
        if (Input.mousePosition.y <= _scroll_boundaries || Input.GetKey(_key_scroll_back))
        {
            _target_look_at.x = _target_look_at.x - _y_direction.x * _scroll_speed * _time * _target_zoom;
            _target_look_at.z = _target_look_at.z - _y_direction.z * _scroll_speed * _time * _target_zoom;
        }

        // zooming when the mousewheel or the zoom keys are used
        _zoom_request = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(_key_zoom_in))
            _zoom_request += _key_zoom_speed_factor * _time;
        if (Input.GetKey(_key_zoom_out))
            _zoom_request -= _key_zoom_speed_factor * _time;
        if (_zoom_request != 0)
        {
            _old_target_zoom = _target_zoom; //needed for zoom to cursor behaviour
            _target_zoom = _target_zoom - _zoom_speed * _target_zoom * _zoom_request; //zoom
            if (_target_zoom > _max_zoom)
                _target_zoom = _max_zoom; //enforce zoom boundaries
            if (_target_zoom < _min_zoom)
                _target_zoom = _min_zoom;
            if (_inclination_by_zoom) //zoom-dependent inclination behaviour
            {
                _zoom_part = 1f - ((_target_zoom - _min_zoom) / _max_zoom); //determine where the current targetZoom is in the range of the zoomBoundaries
                _zoom_part = _zoom_part * _zoom_part * _zoom_part * _zoom_part * _zoom_part * _zoom_part * _zoom_part * _zoom_part; //make sure the inclination increase mostly happens at the lowest zoom levels (when the camera is closest to the x-z-plane)
                _target_inclination = _min_inclination + _zoom_part * (_max_inclination - _min_inclination); //apply the inclination
            }
            if ((_zoom_request > 0 && _zoom_in_to_cursor) || (_zoom_request < 0 && _zoom_out_from_cursor)) //zoom to and from cursor behaviour
            {
                _cursor_position = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).direction; //determine the point on the x-z-plane the cursor hovers over
                _cursor_position = transform.position + _cursor_position * (transform.position.y / -_cursor_position.y);
                _target_look_at = _target_look_at - ((_old_target_zoom - _target_zoom) / (_old_target_zoom)) * (_target_look_at - _cursor_position); //move closer to that point (in the same proportion the zoom has just changed)
            }
        }

        _rotate_request = 0f; //rotating and inclining when the middle mouse button or Space is pressed
        _incline_request = 0f;
        if (Input.GetMouseButton(2) || Input.GetKey(_key_rotate_and_incline))
        {
            if (_rotating)
            {
                _cursor_difference = Input.mousePosition - _last_mouse_position; //how far the cursor has travelled on screen
                _rotate_request += _rotate_speed * 0.001f * _cursor_difference.x; //determine the rotation
                _incline_request -= _incline_speed * 0.001f * _cursor_difference.y; //determine the inclination
            }
            else
                _rotating = true; //rotation has just started
            _last_mouse_position = Input.mousePosition; //store cursor position
        }
        else
            _rotating = false;
        //key controlled rotation and inclination
        if (Input.GetKey(_key_rotate_left))
            _rotate_request += _key_rotate_speed_factor * _rotate_speed * _time;
        if (Input.GetKey(_key_rotate_right))
            _rotate_request -= _key_rotate_speed_factor * _rotate_speed * _time;
        if (Input.GetKey(_key_incline_up))
            _incline_request += _key_incline_speed_factor * _incline_speed * _time;
        if (Input.GetKey(_key_incline_down))
            _incline_request -= _key_incline_speed_factor * _incline_speed * _time;

        if (_rotate_request != 0f)
        {
            _target_rotation += _rotate_request; //apply rotation
            if (_target_rotation > _max_rotation)
                _target_rotation = _max_rotation; //enforce boundaries
            if (_target_rotation < _min_rotation)
                _target_rotation = _min_rotation;
            // make sure rotation stays in the interval between -0.5 and 0.5;
            if (_target_rotation > 0.5f)
            {
                _target_rotation -= 1f;
                _current_rotation -= 1f;
            }
            else if (_target_rotation < -0.5f)
            {
                _target_rotation += 1f;
                _current_rotation += 1f;
            }
            // in case inclination stops afterwards store the last inclination (for snapping)
            _snap_rotation = _target_rotation;
            _rotate_request = 0f;
        }
        else if (!_rotating)
        {
            if (_snap_back_rotation) //snap back
            {
                _target_snap = _target_rotation + _time * _snap_back_speed * (_initial_rotation - _snap_rotation); //determine the next rotation value assuming constant snap speed
                if (Mathf.Abs(_target_snap - _initial_rotation) > Mathf.Abs(_target_rotation - _initial_rotation))
                    _target_snap = _initial_rotation; //finish the snap when it would diverge again (this means it has reached or overshot the initial rotation)
                _target_rotation = _target_snap; //apply the snap
            }
        }

        if (!_inclination_by_zoom && _incline_request != 0f)
        {
            _target_inclination += _incline_request; //apply inclination
            if (_target_inclination > _max_inclination) //enforce boundaries
                _target_inclination = _max_inclination;
            if (_target_inclination < _min_inclination)
                _target_inclination = _min_inclination;
            //in case inclination stops afterwards store the last inclination (for snapping)
            _snap_inclination = _target_inclination;
            _incline_request = 0f;
        }
        else if (!_rotating)
        {
            if (_snap_back_inclination && !_inclination_by_zoom) //snap back
            {
                _target_snap = _target_inclination + _time * _snap_back_speed * (_initial_inclination - _snap_inclination); //determine the next inclination value assuming constant snap speed
                if (Mathf.Abs(_target_snap - _initial_inclination) > Mathf.Abs(_target_inclination - _initial_inclination))
                    _target_snap = _initial_inclination; //finish the snap when it would diverge again (this means it has reached or overshot the initial inclination)
                _target_inclination = _target_snap; //apply the snap
            }
        }

        //enforce scroll boundaries
        if (_target_look_at.x > _max_x)
            _target_look_at.x = _max_x;
        if (_target_look_at.x < _min_x)
            _target_look_at.x = _min_x;

        if (_target_look_at.z > _max_z)
            _target_look_at.z = _max_z;
        if (_target_look_at.z < _min_z)
            _target_look_at.z = _min_z;

        //calculate the current values (let them approach target values asymptotically - this is the actual smoothing)
        _current_look_at = _current_look_at - (_time * (_current_look_at - _target_look_at)) / _scroll_and_zoom_smooth;
        _current_zoom = _current_zoom - (_time * (_current_zoom - _target_zoom)) / _scroll_and_zoom_smooth;
        _current_rotation = _current_rotation - (_time * (_current_rotation - _target_rotation)) / _rotate_and_incline_smooth;
        if (_inclination_by_zoom) //zoom-dependent inclination means the smoothing must be the smoothing when zooming
            _current_inclination = _current_inclination - (_time * (_current_inclination - _target_inclination)) / _scroll_and_zoom_smooth;
        else
            _current_inclination = _current_inclination - (_time * (_current_inclination - _target_inclination)) / _rotate_and_incline_smooth;

        //calculate the viewDirection of the camera from inclination and rotation
        _view_direction = (Vector3.down * (1.0f - _current_inclination) + new Vector3(Mathf.Sin(_current_rotation * TAU), 0, Mathf.Cos(_current_rotation * TAU)) * (_current_inclination)).normalized;
        //apply the current camera values to move and rotate the camera
        transform.position = _current_look_at + _current_zoom * (-_view_direction);
        transform.LookAt(_current_look_at);
    }
}