using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

    enum STEP
    {
        NONE = -1,

        PLAYING = 0,
        CLEAR,

        NUM
    };

    public enum SE
    {

        NONE = -1,

        GRAB = 0,		
        RELEASE,		

        ATTACH,			

        COMPLETE,		

        BUTTON,			

        NUM,
    };

    private STEP current_step = STEP.PLAYING;
    private STEP next_step = STEP.NONE;
    private float step_timer = .0f;

    public GameObject prefab_puzzle = null;
    public PuzzleControl script_puzzle_control = null;
    public Texture texture_finish = null;

    private Sprite sprite_finish = null;

    public AudioClip[] audio_clips;

	// Use this for initialization
	void Start () {
        this.script_puzzle_control = (Instantiate(this.prefab_puzzle) as GameObject).GetComponent<PuzzleControl>();
        //this.sprite_finish = Sprite.Create(this.texture_finish, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(200, 100,300,300), this.texture_finish, ScaleMode.ScaleToFit, false, .0f);
    }

    public void playSE(SE se)
    {
        this.GetComponent<AudioSource>().PlayOneShot(this.audio_clips[(int)se]);
    }
}
