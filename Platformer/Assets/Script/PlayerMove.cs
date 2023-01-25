  using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public float maxSpeed;
    public float jumpPower;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    BoxCollider2D boxCollider;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    // 실질적인 Update(1초에 60frame) -> 단발적인 키 입력
    void Update()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping")) {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        // Stop Speed
        // rigid.velocity.normalized : 단위벡터(방향을 구하고 싶을 때 사용)
        if (Input.GetButtonUp("Horizontal"))
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.1f , rigid.velocity.y);

        // Direction Sprite
        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // Animation
        // Mathf.Abs(x) : x의 절댓값
        if (Mathf.Abs(rigid.velocity.x) < 0.3f)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    // 물리적인 Update(1초에 50frame)
    void FixedUpdate()
    {
        // Move Speed
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        // Max Speed
        if (rigid.velocity.x > maxSpeed) // Right Max Speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        if (rigid.velocity.x < maxSpeed * (-1)) // Left Max Speed
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        // Landing Platform
        // 점프할때 이미 Platform과 닿는 경우를 방지하기 위해 떨어질때만 코드적용
        if (rigid.velocity.y < 0)
        {
            // 자신 밑에 있는 Ray
            Debug.DrawRay(rigid.position, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            // 자신 양 옆에 있는 Ray
            Vector2 frontVec1 = new Vector2(rigid.position.x + 0.5f, rigid.position.y);
            Vector2 frontVec2 = new Vector2(rigid.position.x - 0.5f, rigid.position.y);
            Debug.DrawRay(frontVec1, Vector3.down, new Color(1, 0, 0));
            Debug.DrawRay(frontVec2, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D rayHit2 = Physics2D.Raycast(frontVec1, Vector3.down, 2, LayerMask.GetMask("Platform"));
            RaycastHit2D rayHit3 = Physics2D.Raycast(frontVec2, Vector3.down, 2, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null || rayHit2.collider != null || rayHit3.collider != null)
            {
                if (rayHit.distance < 1 || rayHit2.distance < 1 || rayHit3.distance < 1)
                    anim.SetBool("isJumping", false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어가 적과 닿았을 경우
        if (collision.gameObject.tag == "Enemy")
        {
            // Attack
            // rigid.velocity.y < 0 : 물리적인 상하 속도값이 음수이다(내려오고 있다)
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            // Damaged
            else
            {
                // 맞은 적의 위치값을 넘겨줌
                OnDamaged(collision.transform.position);
            }
        }
        // 플레이어가 가시와 닿았을 경우
        else if (collision.gameObject.tag == "Spike")
        {
            OnDamaged(collision.transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            // Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            else if (isSilver)
                gameManager.stagePoint += 100;
            else if (isGold)
                gameManager.stagePoint += 200;

            // Deactive Item
            collision.gameObject.SetActive(false);
            // Sound
            PlaySound("ITEM");
        }
        else if (collision.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.NextStage();
            // Sound
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        // Point
        gameManager.stagePoint += 100;

        // Reaction Force
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        // Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();

        // Sound
        PlaySound("ATTACK");
    }

    // 무적 상태
    void OnDamaged(Vector2 targetPos)
    {
        // Health Down
        gameManager.HealthDown();

        // Change Layer (Immortal Active)
        gameObject.layer = 9;

        // VIew Alpha(투명도)
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Reaction Force
        // 플레이어의 x값과 적의 x값의 차이가 양수면(적이 더 왼쪽일 경우) 오른쪽방향
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc,1) * 10 ,ForceMode2D.Impulse);

        // Animation
        // 애니메이션에서 Damaged에서 1초 뒤에 Exit로 넘어가는 부분(Has Exit Time)
        anim.SetTrigger("doDamaged");

        // 무적시간을 준 뒤 다시 OffDamaged 호출
        Invoke("OffDamaged", 1);

        // Sound
        PlaySound("DAMAGED");
    }

    // 무적 상태 해제
    void OffDamaged()
    {
        gameObject.layer = 8;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // Sprite Flip Y
        spriteRenderer.flipY = true;
        // Collider Disable
        boxCollider.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // Sound
        PlaySound("DIE");
        // Player Control Rock
    }

    // 시간 멈추기
    //IEnumerator ControlRock()
    //{
    //    yield return new WaitForSeconds(2.0f);
    //    Time.timeScale = 0;
    //}

    public void VelocityZero()
    {
        // 낙하 속도를 0으로 만들어줌
        rigid.velocity = Vector2.zero;
    }

    // 사운드 관리 함수
    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
        }

        audioSource.Play();
    }
}
