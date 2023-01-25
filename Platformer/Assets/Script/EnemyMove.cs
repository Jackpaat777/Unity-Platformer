using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    public int nextMove;
    float nextThinkTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        // Invoke : 지정된 시간 뒤에 함수 실행
        nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void FixedUpdate()
    {
        // Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y );
        Debug.DrawRay(frontVec, Vector3.down, new Color(1, 0, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
            Turn();
    }

    void Think()
    {
        // Set Next Active
        // Range에서 최소값은 포함되지만 최대값은 포함되지 않음
        nextMove = Random.Range(-1,2);

        // Sprite Animation
        anim.SetInteger("walkSpeed", nextMove);

        // Flip Sprite
        // 서있을때 무조건 왼쪽을 보는 것을 방지
        if (nextMove != 0)
            spriteRenderer.flipX = nextMove == 1;

        // Recursive
        nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;
        CancelInvoke(); // 현재 작동중인 Invoke를 중지(시간 다시세기)
        nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    public void OnDamaged()
    {
        nextMove = 0;

        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Sprite Flip Y
        spriteRenderer.flipY = true;

        // Collider Disable
        capsuleCollider.enabled = false;

        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // Destroy
        StartCoroutine(DeActive());
    }

    IEnumerator DeActive()
    {
        yield return new WaitForSeconds(5.0f);
        gameObject.SetActive(false);
    }
}
