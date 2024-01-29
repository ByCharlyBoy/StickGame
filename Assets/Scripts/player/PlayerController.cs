using UnityEngine;

namespace SupanthaPaul
{
	public class PlayerController : MonoBehaviour
	{
		[SerializeField] private float speed;
		[Header("Jumping")]
		[SerializeField] private float jumpForce;
		[SerializeField] private float fallMultiplier;
		[SerializeField] private Transform groundCheck;
		[SerializeField] private float groundCheckRadius;
		[SerializeField] private LayerMask whatIsGround;
		[SerializeField] private int extraJumpCount = 1;
		[Header("Dashing")]
		[SerializeField] private float dashSpeed = 30f;
		[Tooltip("Amount of time (in seconds) the player will be in the dashing speed")]
		[SerializeField] private float startDashTime = 0.1f;
		[Tooltip("Time (in seconds) between dashes")]
		[SerializeField] private float dashCooldown = 0.2f;

		// Access needed for handling animation in Player script and other uses
		[HideInInspector] public bool isGrounded;
		[HideInInspector] public float moveInput;
		[HideInInspector] public bool canMove = true;
		[HideInInspector] public bool isDashing = false;

		// controls whether this instance is currently playable or not
		[HideInInspector] public bool isCurrentlyPlayable = false;

		[Header("Wall grab & jump")]
		[Tooltip("Right offset of the wall detection sphere")]
		public Vector2 grabRightOffset = new Vector2(0.16f, 0f);
		public Vector2 grabLeftOffset = new Vector2(-0.16f, 0f);
		public float grabCheckRadius = 0.24f;
		public float slideSpeed = 2.5f;
		public Vector2 wallJumpForce = new Vector2(10.5f, 18f);
		public Vector2 wallClimbForce = new Vector2(4f, 14f);

		private Rigidbody2D m_rb;

		private bool m_facingRight = true;
		private readonly float m_groundedRememberTime = 0.25f;
		private float m_groundedRemember = 0f;
		private int m_extraJumps;
		private float m_extraJumpForce;
		private float m_dashTime;
		private bool m_hasDashedInAir = false;
		private bool m_onWall = false;
		private bool m_onRightWall = false;
		private bool m_onLeftWall = false;
		private bool m_wallGrabbing = false;
		private readonly float m_wallStickTime = 0.25f;
		private float m_wallStick = 0f;
		private bool m_wallJumping = false;
		private float m_dashCooldown;

		// 0 -> none, 1 -> right, -1 -> left
		private int m_onWallSide = 0;
		private int m_playerSide = 1;

		void Start()
		{

			if (transform.CompareTag("Player"))
				isCurrentlyPlayable = true;

			m_extraJumps = extraJumpCount;
			m_dashTime = startDashTime;
			m_dashCooldown = dashCooldown;
			m_extraJumpForce = jumpForce * 0.7f;

			m_rb = GetComponent<Rigidbody2D>();

		}
		
		private void FixedUpdate()
		{
			isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
			var position = transform.position;
			m_onWall = Physics2D.OverlapCircle((Vector2)position + grabRightOffset, grabCheckRadius, whatIsGround)
			          || Physics2D.OverlapCircle((Vector2)position + grabLeftOffset, grabCheckRadius, whatIsGround);
			m_onRightWall = Physics2D.OverlapCircle((Vector2)position + grabRightOffset, grabCheckRadius, whatIsGround);
			m_onLeftWall = Physics2D.OverlapCircle((Vector2)position + grabLeftOffset, grabCheckRadius, whatIsGround);

			CalculateSides();

			if((m_wallGrabbing || isGrounded) && m_wallJumping)
			{
				m_wallJumping = false;
			}
			if (isCurrentlyPlayable)
			{
				if(m_wallJumping)
				{
					m_rb.velocity = Vector2.Lerp(m_rb.velocity, (new Vector2(moveInput * speed, m_rb.velocity.y)), 1.5f * Time.fixedDeltaTime);
				}
				else
				{
					if(canMove && !m_wallGrabbing)
						m_rb.velocity = new Vector2(moveInput * speed, m_rb.velocity.y);
					else if(!canMove)
						m_rb.velocity = new Vector2(0f, m_rb.velocity.y);
				}
				if (m_rb.velocity.y < 0f)
				{
					m_rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
				}

				if (!m_facingRight && moveInput > 0f)
					Flip();
				else if (m_facingRight && moveInput < 0f)
					Flip();

				if (isDashing)
				{
					if (m_dashTime <= 0f)
					{
						isDashing = false;
						m_dashCooldown = dashCooldown;
						m_dashTime = startDashTime;
						m_rb.velocity = Vector2.zero;
					}
					else
					{
						m_dashTime -= Time.deltaTime;
						if(m_facingRight)
							m_rb.velocity = Vector2.right * dashSpeed;
						else
							m_rb.velocity = Vector2.left * dashSpeed;
					}
				}

				if(m_onWall && !isGrounded && m_rb.velocity.y <= 0f && m_playerSide == m_onWallSide)
				{

					m_wallGrabbing = true;
					m_rb.velocity = new Vector2(moveInput * speed, -slideSpeed);
					m_wallStick = m_wallStickTime;
				} else
				{
					m_wallStick -= Time.deltaTime;

					if (m_wallStick <= 0f)
						m_wallGrabbing = false;
				}
				if (m_wallGrabbing && isGrounded)
					m_wallGrabbing = false;

				// enable/disable dust particles
				float playerVelocityMag = m_rb.velocity.sqrMagnitude;

			}
		}

		private void Update()
		{
			// horizontal input
			moveInput = InputSystem.HorizontalRaw();

			if (isGrounded)
			{
				m_extraJumps = extraJumpCount;
			}

			m_groundedRemember -= Time.deltaTime;
			if (isGrounded)
				m_groundedRemember = m_groundedRememberTime;

			if (!isCurrentlyPlayable) return;
			if (!isDashing && !m_hasDashedInAir && m_dashCooldown <= 0f)
			{
				// dash input (left shift)
				if (InputSystem.Dash())
				{
					isDashing = true;

					if(!isGrounded)
					{
						m_hasDashedInAir = true;
					}

				}
			}
			m_dashCooldown -= Time.deltaTime;
			
			if (m_hasDashedInAir && isGrounded)
				m_hasDashedInAir = false;
			
			// Jumping
			if(InputSystem.Jump() && m_extraJumps > 0 && !isGrounded && !m_wallGrabbing)	// extra jumping
			{
				m_rb.velocity = new Vector2(m_rb.velocity.x, m_extraJumpForce); ;
				m_extraJumps--;
			}
			else if(InputSystem.Jump() && (isGrounded || m_groundedRemember > 0f))	// normal single jumping
			{
				m_rb.velocity = new Vector2(m_rb.velocity.x, jumpForce);

			}
			else if(InputSystem.Jump() && m_wallGrabbing && moveInput!=m_onWallSide )		// wall jumping off the wall
			{
				m_wallGrabbing = false;
				m_wallJumping = true;
				Debug.Log("Wall jumped");
				if (m_playerSide == m_onWallSide)
					Flip();
				m_rb.AddForce(new Vector2(-m_onWallSide * wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
			}
			else if(InputSystem.Jump() && m_wallGrabbing && moveInput != 0 && (moveInput == m_onWallSide))      // wall climbing jump
			{
				m_wallGrabbing = false;
				m_wallJumping = true;
				Debug.Log("Wall climbed");
				if (m_playerSide == m_onWallSide)
					Flip();
				m_rb.AddForce(new Vector2(-m_onWallSide * wallClimbForce.x, wallClimbForce.y), ForceMode2D.Impulse);
			}

		}

		void Flip()
		{
			m_facingRight = !m_facingRight;
			Vector3 scale = transform.localScale;
			scale.x *= -1;
			transform.localScale = scale;
		}

		void CalculateSides()
		{
			if (m_onRightWall)
				m_onWallSide = 1;
			else if (m_onLeftWall)
				m_onWallSide = -1;
			else
				m_onWallSide = 0;

			if (m_facingRight)
				m_playerSide = 1;
			else
				m_playerSide = -1;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
			Gizmos.DrawWireSphere((Vector2)transform.position + grabRightOffset, grabCheckRadius);
			Gizmos.DrawWireSphere((Vector2)transform.position + grabLeftOffset, grabCheckRadius);
		}
	}
}
