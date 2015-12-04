using UnityEngine;
using System.Collections;


public class PuzzleControl : MonoBehaviour {

	public GameControl script_game_control = null;

	private int piece_num;
	private int piece_num_finished;

	enum STEP{
		NONE = -1,
		PLAY = 0,
		CLEAR,
		NUM
	}

	private STEP step_now = STEP.NONE;
	private STEP step_next = STEP.NONE;

	private float step_time = .0f;
	private float step_time_prev = .0f;

	private PieceControl[] pieces_all;
	private PieceControl[] pieces_active;

	private float puzzle_rotation = 37.0f;
	
	private Bounds shuffle_zone;
	private int shuffle_grid_num = 1;

	private bool is_clear_show = false;

	// Use this for initialization
	void Start () {
		this.script_game_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();

		this.piece_num = 0;

		for(int i=0; i < this.transform.childCount; i++){
			GameObject _piece = this.transform.GetChild(i).gameObject;
			if(this.isPiece(_piece)){
				this.piece_num++;
			}
		}

		this.pieces_all = new PieceControl[this.piece_num];
		this.pieces_active = new PieceControl[this.piece_num];

		for(int i=0, n =0; i < this.transform.childCount; i++){
			GameObject _piece = this.transform.GetChild(i).gameObject;
			if(!this.isPiece(_piece)){
				continue;
			}
			_piece.AddComponent<PieceControl>();
			_piece.GetComponent<PieceControl>().script_puzzle_control = this;
			this.pieces_all[n++] = _piece.GetComponent<PieceControl>();
		}

		this.piece_num_finished = 0;

		this.setPiecesHeightOffset();

		for(int i=0; i<this.transform.childCount; i++){
			GameObject _baseObj = this.transform.GetChild(i).gameObject;
			if(this.isPiece(_baseObj)){
				continue;
			}
			_baseObj.GetComponent<Renderer>().material.renderQueue = this.getDrawPriorityBase();
		}

		this.calcShuffleZone();
		this.is_clear_show = false;
	}
	
	// Update is called once per frame
	void Update () {
		this.step_time_prev = this.step_time;
		this.step_time += Time.deltaTime;

		switch(this.step_now){
		case STEP.NONE:
			this.step_next = STEP.PLAY;
			break;
		case STEP.PLAY:
			if(this.piece_num_finished >= this.piece_num){
				this.step_next = STEP.CLEAR;
			}
			break;
		}

		while(this.step_next != STEP.NONE){
			this.step_now = this.step_next;
			this.step_next = STEP.NONE;
			this.step_time = .0f;
			switch(this.step_now){
			case STEP.PLAY:
				for(int i=0; i<this.pieces_all.Length; i++){
					this.pieces_active[i] = this.pieces_all[i];
				}
				this.piece_num_finished = 0;
				this.shufflePieces();
				foreach(PieceControl _piece in this.pieces_all){
					_piece.restart();
				}
				this.setPiecesHeightOffset();
				break;
			case STEP.CLEAR:
				break;
			}
		}

		switch(this.step_now){
		case STEP.CLEAR:
			const float clear_delay = 0.5f;
			if(this.step_time_prev < clear_delay && this.step_time >= clear_delay){
				this.script_game_control.playSE(GameControl.SE.COMPLETE);
				this.is_clear_show = true;
			}
			break;
		}

		PuzzleControl.drawBounds(this.shuffle_zone);
	}

    public void pickPiece(PieceControl _piece) { 
        
    }

    public void finishPiece(PieceControl _piece)
    {

    }

	private bool isPiece(GameObject obj){
		bool ret = false;
		if(obj.name.Contains("base")){
			ret = false;
		}
		else{
			ret = true;
		}
		return ret;
	}

	private void setPiecesHeightOffset(){
		float offset = 0.01f;
		int n = 0; 
		foreach(PieceControl _piece in this.pieces_all){
			if(_piece == null){
				continue;
			}
			_piece.GetComponent<Renderer>().material.renderQueue = this.getDrawPriorityPiece(n);
			offset -= 0.01f/this.piece_num;
			_piece.setHeightOffset(offset);
			n++;
		}
	}

	private int getDrawPriorityBase(){
		return 0;
	}

	private int getDrawPriorityFinishPiece(){
		int priority = 0;
		priority = this.getDrawPriorityBase() + 1;
		return priority;
	}

	private int getDrawPriorityRetryButton(){
		int priority = 0;
		priority = this.getDrawPriorityFinishPiece() + 1;
		return priority;
	}

	private int getDrawPriorityPiece(int priorityInPieces){
		int priority = 0;
		priority = this.getDrawPriorityRetryButton() + 1;
		priority += this.piece_num - 1 - priorityInPieces;
		return priority;
	}

	private static float	SHUFFLE_ZONE_OFFSET_X = -5.0f;
	private static float	SHUFFLE_ZONE_OFFSET_Y =  1.0f;
	private static float	SHUFFLE_ZONE_SCALE =  1.1f;

	private void calcShuffleZone(){
		Vector3 center = Vector3.zero;
		foreach(PieceControl _piece in this.pieces_all){
			center += _piece.pos_finish;
		}
		center /= (float)this.pieces_all.Length;
		center.x += SHUFFLE_ZONE_OFFSET_X;
		center.z += SHUFFLE_ZONE_OFFSET_Y;

		this.shuffle_grid_num = Mathf.CeilToInt(Mathf.Sqrt((float)this.pieces_all.Length));
		Bounds piece_bounds_max = new Bounds(Vector3.zero, Vector3.zero);
		foreach(PieceControl _piece in this.pieces_all){
			Bounds bounds = _piece.getBounds(Vector3.zero);
			piece_bounds_max.Encapsulate(bounds);
		}
		piece_bounds_max.size *= SHUFFLE_ZONE_SCALE;
		this.shuffle_zone = new Bounds(center, piece_bounds_max.size*this.shuffle_grid_num);
	}

	private void shufflePieces(){
#if true
		int[] pieces_index = new int[this.shuffle_grid_num*this.shuffle_grid_num];
		for(int i=0; i<pieces_index.Length; i++){
			if(i<this. pieces_all.Length){
				pieces_index[i] = i;
			}
			else{
				pieces_index[i] = -1;
			}
		}
		for(int i=0; i<pieces_index.Length-1; i++){
			int j = Random.Range(i+1, pieces_index.Length);
			int temp = pieces_index[i];
			pieces_index[i] = pieces_index[j];
			pieces_index[j] = temp;
		}

		Vector3 pitch = this.shuffle_zone.size/(float)this.shuffle_grid_num;

		for(int i=0; i<pieces_index.Length; i++){
			if(pieces_index[i] < 0){
				continue;
			}
			PieceControl _piece = this.pieces_all[pieces_index[i]];
			int ix = i%this.shuffle_grid_num;
			int iz = i/this.shuffle_grid_num;
			Vector3 _position = new Vector3(ix*pitch.x, _piece.pos_finish.y, iz*pitch.z);
			_position.x += this.shuffle_zone.center.x - pitch.x*(this.shuffle_grid_num/2.0f - 0.5f);
			_position.z += this.shuffle_zone.center.z - pitch.z*(this.shuffle_grid_num/2.0f - 0.5f);
			_piece.pos_begin = _position;
		}

		for(int i=0; i<pieces_index.Length; i++){
			if(pieces_index[i] < 0){
				continue;
			}
			Vector3 piece_offset = Vector3.zero;
			piece_offset.x = pitch.x*Random.Range(-0.4f,0.4f);
			piece_offset.x = pitch.z*Random.Range(-0.4f,0.4f);
			PieceControl _piece = this.pieces_all[pieces_index[i]];
			_piece.pos_begin += piece_offset;
		}

		foreach(PieceControl _piece in this.pieces_all){
			Vector3 _position = _piece.pos_begin;
			_position -= this.shuffle_zone.center;
			_position = Quaternion.AngleAxis(this.puzzle_rotation, Vector3.up)*_position;
			_position += this.shuffle_zone.center;
			_piece.pos_begin = _position;
		}

		this.puzzle_rotation += 90;
#else
		foreach(PieceControl _piece in this.pieces_all){
			Vector3 _position;
			Bounds piece_bounds = _piece.getBounds(Vector3.zero);
			_position.x = Random.Range(this.shuffle_zone.min.x - piece_bounds.min.x, this.shuffle_zone.max.x - piece_bounds.max.x);
			_position.z = Random.Range(this.shuffle_zone.min.z - piece_bounds.min.z, this.shuffle_zone.max.z - piece_bounds.max.z);
			_position.y = _piece.pos_begin.y;
			_piece.pos_begin = _position;
		}
#endif
	}

	public static void drawBounds(Bounds bounds){
		Vector3[] square = new Vector3[4];
		square[0] = new Vector3(bounds.min.x, .0f, bounds.min.z);
		square[1] = new Vector3(bounds.max.x, .0f, bounds.min.z);
		square[2] = new Vector3(bounds.max.x, .0f, bounds.max.z);
		square[3] = new Vector3(bounds.min.x, .0f, bounds.max.z);
		for(int i = 0;i < 4;i++) {
			Debug.DrawLine(square[i], square[(i + 1)%4], Color.white, 0.0f, false);
		}
	}
}













