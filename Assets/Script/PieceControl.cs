using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshCollider))]
public class PieceControl : MonoBehaviour {

    private static bool IS_ENABLE_GRAB_OFFSET = true;

    private GameObject obj_camera;

    public PuzzleControl script_puzzle_control = null;
    public GameControl script_game_control = null;

    private static float HEIGHT_OFFSET_BASE = 0.1f;
    private static float SNAP_SPEED = 0.1f * 60.0f;
    private static float SNAP_DISTANCE = 0.5f;

    enum STEP
    {
        NONE = -1,

        IDLE = 0,
        DRAGING,
        FINISH,
        RESTART,
        SNAPPING,

        NUM
    };

    private STEP step_now = STEP.NONE;
    private STEP step_next = STEP.NONE;


    private Vector3 grab_offset = Vector3.zero;
    private bool is_dragging = false;

    public Vector3 pos_begin;
	public Vector3 pos_finish;
    private Vector3 snap_target;

    public float height_offset = 0.0f;
    public float _roll = 0.0f;

	void Awake(){
		this.pos_begin = this.transform.position;
		this.pos_finish = this.pos_begin;
	}

	// Use this for initialization
	void Start () {
        this.obj_camera = GameObject.FindGameObjectWithTag("MainCamera");
        this.script_game_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();
	}
	
	// Update is called once per frame
	void Update () {
        Color _color = Color.white;

        //state change
        switch (this.step_now)
        {
            case STEP.NONE:
                this.step_next = STEP.RESTART;
                break;
            case STEP.IDLE:
                if (this.is_dragging)
                {
                    this.step_next = STEP.DRAGING;
                }
                break;
            case STEP.DRAGING:
                if (this.is_in_snap_range())
                {
                    if (!this.is_dragging)
                    {
                        this.step_next = STEP.SNAPPING;
                        this.snap_target = this.pos_finish;
                        this.script_game_control.playSE(GameControl.SE.ATTACH);
                    }
                }
                else
                {
                    if (!this.is_dragging)
                    {
                        this.step_next = STEP.IDLE;
                        this.script_game_control.playSE(GameControl.SE.RELEASE);
                    }
                }
                break;
            case STEP.SNAPPING:
                if ((this.transform.position - this.snap_target).magnitude < 0.001f)
                {
                    this.step_next = STEP.FINISH;
                }
                break;
        }

        //do action when  state changed
        while (this.step_next != STEP.NONE)
        {
            this.step_now = this.step_next;
            this.step_next = STEP.NONE;
            switch (this.step_now)
            {
                case STEP.IDLE:
                    this.setHeightOffset(this.height_offset);
                    break;
                case STEP.DRAGING:
                    this.beginDrag();
                    this.script_puzzle_control.pickPiece(this);
                    this.script_game_control.playSE(GameControl.SE.GRAB);
                    break;
                case STEP.RESTART:
                    this.transform.position = this.pos_begin;
                    this.setHeightOffset(this.height_offset);
                    this.step_next = STEP.IDLE;
                    break;
                case STEP.FINISH:
                    this.transform.position = this.pos_finish;
                    this.script_puzzle_control.finishPiece(this);
                    break;
            }
        }

        this.transform.localScale = Vector3.one;

        //do action when state continue
        switch (this.step_now)
        {
            case STEP.DRAGING:
                this.continueDrag();
                if (this.is_in_snap_range())
                {
                    _color *= 1.5f;
                }
                this.transform.localScale = Vector3.one * 1.1f;
                break;
            case STEP.SNAPPING:
                Vector3 _distance, _next_pos;
                _distance = this.snap_target - this.transform.position;
                _distance *= 0.25f * (60.0f * Time.deltaTime);
                _next_pos = this.transform.position + _distance;
                if (_distance.magnitude < PieceControl.SNAP_SPEED * Time.deltaTime)
                {
                    this.transform.position = this.snap_target;
                }
                else
                {
                    this.transform.position = _next_pos;
                }
                break;
        }
		this.GetComponent<Renderer> ().material.color = _color;
	}

    public Bounds getBounds(Vector3 center)
    {
        Bounds _bounds = this.GetComponent<MeshFilter>().mesh.bounds;
        _bounds.center = center;
        return _bounds;
    }
    
    private bool is_in_snap_range()
    {
        bool ret = false;
        if (Vector3.Distance(this.transform.position, this.pos_finish) < PieceControl.SNAP_DISTANCE)
        {
            ret = true;
        }
        return ret;
    }

    public bool unproject_mouse_position(out Vector3 world_pos, Vector3 mouse_pos)
    {
        bool ret = false;
        float depth = 0;

        Plane _plane = new Plane(Vector3.up, new Vector3(0, this.transform.position.y, 0));
        Ray _ray = this.obj_camera.GetComponent<Camera>().ScreenPointToRay(mouse_pos);

        if (_plane.Raycast(_ray, out depth))
        {
            world_pos = _ray.origin + _ray.direction * depth;
            ret = true;
        }
        else
        {
            world_pos = Vector3.zero;
            ret = false;
        }
        return ret;
    }

    public void setHeightOffset(float height_offset)
    {
        Vector3 _pos = this.transform.position;

        if (this.step_now != STEP.FINISH || this.step_next != STEP.FINISH)
        {
            this.height_offset = height_offset;

            _pos.y = this.pos_finish.y + PieceControl.HEIGHT_OFFSET_BASE;
            _pos.y += this.height_offset;

            this.transform.position = _pos;
        }
    }

    public void beginDrag()
    {
        Vector3 _world_pos;
        if (!this.unproject_mouse_position(out _world_pos, Input.mousePosition))
        {
            return;
        }
        if (PieceControl.IS_ENABLE_GRAB_OFFSET)
        {
            this.grab_offset = this.transform.position - _world_pos;
        }
    }

    public void continueDrag()
    {
        Vector3 _world_pos;
        if (!this.unproject_mouse_position(out _world_pos, Input.mousePosition))
        {
            return;
        }
        this.transform.position = _world_pos + this.grab_offset;
    }

    void OnMouseDown()
    {
        this.is_dragging = true;
    }

    void OnMouseUp()
    {
        this.is_dragging = false;
    }

    public void restart()
    {
        this.step_next = STEP.RESTART;
    }
}
