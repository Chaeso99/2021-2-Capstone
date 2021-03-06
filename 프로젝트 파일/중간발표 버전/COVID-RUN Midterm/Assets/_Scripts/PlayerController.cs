using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public SoundManager soundManager; // SoundManager의 오디오 클립
    public GameObject PlayerObject;  // 플레이어 오브젝트의 색상 정보를 저장

    public float jumpForce = 500f; // 점프 힘

    public CapsuleCollider2D playerRun; // 기본 콜라이더
    public BoxCollider2D playerSlide; // 슬라이더, 사망 전용 콜라이더

    private int jumpCount = 0; // 누적 점프 횟수
    private bool isDead = false; // 사망 상태
    private bool isSlide = false; // 슬라이딩 상태
    private bool isOnDamaged = false; // 무적 상태
    private Rigidbody2D playerRigidbody; // 사용할 리지드바디 컴포넌트
    private Animator playerAnimator; // 사용할 애니메이터 컴포넌트
    private SpriteRenderer spriteRenderer; // 캐릭터 무적시, 색상을 변경하기 위한 변수

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        spriteRenderer = PlayerObject.GetComponent<SpriteRenderer>();

        playerSlide.enabled = false; // 기본 콜라이더만 활성화
    }

    void Update()
    {
        // 게임 시작 상태가 아니면 키 입력을 받지 않도록 함
        if (!GameManager.instance.isGameStart)
        {
            return;
        }
        
        // 플레이어 캐릭터가 죽으면 메서드 실행 종료
        if (isDead)
        {
            return;
        }

        // 게임이 일시정지 상태이면 키 입력을 받지 않도록 함
        if (GameManager.instance.isPause)
        {
            return;
        }
        
        // 점프 (위쪽 방향키, Space 바 사용)
        if (Input.GetKeyDown(KeyCode.UpArrow) && jumpCount < 2 && !isSlide)
        {
            jumpCount++;
            playerAnimator.SetInteger("Jump", jumpCount);
            playerAnimator.SetBool("Run", false);

            playerRigidbody.velocity = Vector2.zero; // new Vector2(0, 0);
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            soundManager.PlayJumpSound();
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2 && !isSlide)
        {
            jumpCount++;
            playerAnimator.SetInteger("Jump", jumpCount);
            playerAnimator.SetBool("Run", false);

            playerRigidbody.velocity = Vector2.zero; // new Vector2(0, 0);
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            soundManager.PlayJumpSound();
        }

        /*
        else if (Input.GetKeyDown(KeyCode.UpArrow) && playerRigidbody.velocity.y > 0)
        {
            playerRigidbody.velocity = playerRigidbody.velocity * 0.5f;
        }
        */

        // 슬라이딩 (아래쪽 방향키, 왼쪽 Shift 키 사용)
        if (Input.GetKeyDown(KeyCode.DownArrow) && jumpCount == 0)
        {
            playerAnimator.SetBool("Slide", true);
            isSlide = true;

            playerSlide.enabled = true;
            playerRun.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && jumpCount == 0)
        {
            playerAnimator.SetBool("Slide", true);
            isSlide = true;

            playerSlide.enabled = true;
            playerRun.enabled = false;
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            playerAnimator.SetBool("Slide", false);
            isSlide = false;

            playerRun.enabled = true;
            playerSlide.enabled = false;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerAnimator.SetBool("Slide", false);
            isSlide = false;

            playerRun.enabled = true;
            playerSlide.enabled = false;
        }
    }

    // 플레이어 캐릭터가 죽으면 실행
    public void Die()
    {
        soundManager.PlayDeathSound();

        playerAnimator.SetTrigger("Die"); 
        playerSlide.enabled = true;
        playerRun.enabled = false;

        playerRigidbody.velocity = Vector3.zero;
        isDead = true;

        GameManager.instance.OnPlayerDead();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 코인, 아이템 코드 (미구현)
        /*
        if (other.tag == "Coin" && !isDead)
        {
            soundManager.PlayCoinSound();
            // 점수 상승 코드
        }

        if (other.tag == "Item" && !isDead)
        {
            soundManager.PlayItemSound();
            // 피 회복 코드
            HPBar.HP += 0.1f;

            // 사람 장애물 제거 코드 (플레이어 위치로 부터 몇 거리 안에 있으면 제거)
        }
        */

        // 장애물과 부딪히면 HP가 0.1 감소
        if (other.tag == "Obstacle" && !isDead)
        {
            // 무적상태이면 데미지를 받지 않음
            if (isOnDamaged)
            {
                return;
            }

            soundManager.PlayHitSound();
            HpBar.Hp -= 0.1f;
            OnDamaged();  // 무적상태 돌입
        }

        // 낙사하면 사망 메소드 실행
        if (other.tag == "Dead" && !isDead)
        {
            Die();
        }
    }
    
    // 점프 카운트 측정 (1단 점프, 2단 점프 구분)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            jumpCount = 0;
            playerAnimator.SetInteger("Jump", jumpCount);
            playerAnimator.SetBool("Run", true);
        }

        /*
        if (collision.gameObject.tag == "Platform")
        {
            jumpCount = 0;
            playerAnimator.SetInteger("Jump", jumpCount);
            playerAnimator.SetBool("Run", true);
        }
        */
    }
    void OnDamaged() // 피격시
    {
        gameObject.layer = 8;
        spriteRenderer.material.color = new Color(255.0f / 255.0f, 200.0f / 255.0f, 200.0f / 255.0f, 200.0f / 255.0f);
        // 빨간 투명색
        isOnDamaged = true;

        Invoke("DamagedOff", 1.0f);
    }

    void DamagedOff() // 무적상태 풀림
    {
        gameObject.layer = 20; // 플레이어 레이어로
        spriteRenderer.material.color = Color.white;
        isOnDamaged = false;
    }
}
