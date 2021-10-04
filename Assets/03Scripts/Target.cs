using System;
using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
	[Header("Difficulty")]
	[Tooltip("0 : insane, 1 : difficult, 2 : normal, 3 : easy")]
	public float difficulty = 2.0f;

	[Header("HitChangeMaterial")]
	//when hit, change object's material
	[SerializeField] private Material hitMaterial;

	[Header("Collider")]
	//when hit, disable object's collider
	[SerializeField] private Collider m_Collider;

	[Header("Time")]
	//when hit, wait for a second
	[SerializeField] private float hitTime = 0.8f;

	[Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioSource audioSource;

	private bool routineStarted = false;
	[NonSerialized] public bool isHit = false;
	private bool afterSec = false;


    private void Start()
    {
		StartCoroutine(ExpiredTimer());
		m_Collider = GetComponent<Collider>();
	}

    private void Update()
	{
		if (isHit == true)
		{
			if (routineStarted == false)
			{
				//Set the downSound as current sound, and play it
				audioSource.GetComponent<AudioSource>().clip = hitSound;
				audioSource.Play();

				//Start the timer
				StartCoroutine(DelayTimer());
				routineStarted = true;
				gameObject.GetComponent<MeshRenderer>().material = hitMaterial;
				m_Collider.enabled = false;
			}
			else if(routineStarted == true && afterSec == true)
            {
				Destroy(gameObject);
            }
		}
	}
	private IEnumerator DelayTimer()
	{
		yield return new WaitForSeconds(hitTime);
		afterSec = true;
	}
	private IEnumerator ExpiredTimer()
	{
		yield return new WaitForSeconds((difficulty + 2) * 0.25f);
		if (isHit != true) Destroy(gameObject);
	}

	public void DifficultEasy()
    {
		difficulty = 3f;
	}
	public void DifficultNormal()
	{
		difficulty = 2f;
	}
	public void DifficultDifficult()
	{
		difficulty = 1f;
	}
	public void DifficultInsane()
	{
		difficulty = 0f;
	}
}
