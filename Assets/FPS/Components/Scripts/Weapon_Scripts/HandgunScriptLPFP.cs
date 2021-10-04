using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

// ----- Low Poly FPS Pack Free Version -----
public class HandgunScriptLPFP : MonoBehaviour {

	//Animator component attached to weapon
	Animator anim;
	private bool infinateAmmo = false;
	public GameObject obj;

	[Header("PauseUI")]
	public GameObject PauseUI;
	public Text infinitAmmoText;

	[Header("EndUI")]
	public GameObject EndUI;
	public Text endCurrentScoreText;
	public Text endBestScoreText;
	public Text endDifficultyText;
	public Text endInfinitAmmoText;
	private float bestScore;
	private float endSec;
	[SerializeField] private Text endSecText;
	[NonSerialized] public bool gameEnd = false;
	[SerializeField] private string mainMenuName;

	[Header("CrossAir")]
	[SerializeField] private GameObject crossAir;

	[Header("Gun Camera")]
	//Main gun camera
	public Camera gunCamera;

	[Header("Gun Camera Options")]
	//How fast the camera field of view changes when aiming 
	[Tooltip("How fast the camera field of view changes when aiming.")]
	public float fovSpeed = 15.0f;
	//Default camera field of view
	[Tooltip("Default value for camera field of view (40 is recommended).")]
	public float defaultFov = 40.0f;

	public float aimFov = 15.0f;

	[Header("UI Weapon Name")]
	[Tooltip("Name of the current weapon, shown in the game UI.")]
	public string weaponName;
	private string storedWeaponName;

	[Header("Weapon Sway")]
	[Tooltip("Toggle weapon sway.")]
	public bool weaponSway;

	public float swayAmount = 0.02f;
	public float maxSwayAmount = 0.06f;
	public float swaySmoothValue = 4.0f;

	private Vector3 initialSwayPosition;

	[Header("Weapon Settings")]

	public float sliderBackTimer = 1.58f;
	private bool hasStartedSliderBack;

	//Eanbles auto reloading when out of ammo
	[Tooltip("Enables auto reloading when out of ammo.")]
	public bool autoReload;
	public float autoReloadDelay;
	private bool isReloading;
	private bool isAiming;
	private bool isInspecting;

	//How much ammo is currently left
	private int currentAmmo;
	[Tooltip("How much ammo the weapon should have.")]
	public int ammo;
	private bool outOfAmmo;
	private bool stopTime = false;

	[Header("Bullet Settings")]
	//Bullet
	[Tooltip("How much force is applied to the bullet when shooting.")]
	public float bulletForce = 400;
	[Tooltip("How long after reloading that the bullet model becomes visible " +
		"again, only used for out of ammo reload aniamtions.")]
	public float showBulletInMagDelay = 0.6f;
	[Tooltip("The bullet model inside the mag, not used for all weapons.")]
	public SkinnedMeshRenderer bulletInMagRenderer;

	[Header("Muzzleflash Settings")]
	public bool randomMuzzleflash = false;
	//min should always bee 1
	private int minRandomValue = 1;

	[Range(2, 25)]
	public int maxRandomValue = 5;

	private int randomMuzzleflashValue;

	public bool enableMuzzleflash = true;
	public ParticleSystem muzzleParticles;
	public bool enableSparks = true;
	public ParticleSystem sparkParticles;
	public int minSparkEmission = 1;
	public int maxSparkEmission = 7;

	[Header("Muzzleflash Light Settings")]
	public Light muzzleflashLight;
	public float lightDuration = 0.02f;

	[Header("Audio Source")]
	//Main audio source
	public AudioSource mainAudioSource;
	//Audio source used for shoot sound
	public AudioSource shootAudioSource;

	[Header("UI Components")]
	public float scoreCount = 0.0f;
	[SerializeField] private float scoreMultiplier = 50.0f;
	private float timer = 0.0f;
	private float time = 0.0f;
	public Text timeText;
	public Text endTimeText;
	public Text currentPoint;
	public Text currentWeaponText;
	public Text currentAmmoText;
	public Text totalAmmoText;

	[System.Serializable]
	public class prefabs
	{  
		[Header("Prefabs")]
		public Transform bulletPrefab;
		public Transform casingPrefab;
	}
	public prefabs Prefabs;
	
	[System.Serializable]
	public class spawnpoints
	{  
		[Header("Spawnpoints")]
		//Array holding casing spawn points 
		//Casing spawn point array
		public Transform casingSpawnPoint;
		//Bullet prefab spawn from this point
		public Transform bulletSpawnPoint;
		//Grenade prefab spawn from this point
		public Transform grenadeSpawnPoint;
	}
	public spawnpoints Spawnpoints;

	[System.Serializable]
	public class soundClips
	{
		public AudioClip shootSound;
		public AudioClip takeOutSound;
		public AudioClip holsterSound;
		public AudioClip reloadSoundOutOfAmmo;
		public AudioClip reloadSoundAmmoLeft;
		public AudioClip aimSound;
	}
	public soundClips SoundClips;

	private bool soundHasPlayed = false;

	private void Awake () 
	{
		//Set the animator component
		anim = GetComponent<Animator>();
		PauseUI.SetActive(false);
		EndUI.SetActive(false);
		//Set current ammo to total ammo value
		currentAmmo = ammo;

		muzzleflashLight.enabled = false;
	}

	private void Start () {
		//Save the weapon name
		storedWeaponName = weaponName;
		//Get weapon name from string to text
		currentWeaponText.text = weaponName;
		//Set total ammo text from total ammo int
		totalAmmoText.text = ammo.ToString();

		//Weapon sway
		initialSwayPosition = transform.localPosition;

		//Set the shoot sound to audio source
		shootAudioSource.clip = SoundClips.shootSound;

		if (infinateAmmo == true) infinitAmmoText.text = "Infinate Ammo : APPLIED";
		else infinitAmmoText.text = "Infinate Ammo : UNAPPLIED";
	}

	private void LateUpdate () {
		currentPoint.text = ("Score : " + scoreCount * (-1 * scoreMultiplier * (obj.GetComponent<Target>().difficulty - 5))).ToString();
		//Weapon sway
		if (weaponSway == true) {
			float movementX = -Input.GetAxis ("Mouse X") * swayAmount;
			float movementY = -Input.GetAxis ("Mouse Y") * swayAmount;
			//Clamp movement to min and max values
			movementX = Mathf.Clamp 
				(movementX, -maxSwayAmount, maxSwayAmount);
			movementY = Mathf.Clamp 
				(movementY, -maxSwayAmount, maxSwayAmount);
			//Lerp local pos
			Vector3 finalSwayPosition = new Vector3 
				(movementX, movementY, 0);
			transform.localPosition = Vector3.Lerp 
				(transform.localPosition, finalSwayPosition + 
				initialSwayPosition, Time.deltaTime * swaySmoothValue);
		}
	}
	
	private void Update () {
		if (infinateAmmo == true) infinitAmmoText.text = "Infinate Ammo : APPLIED";
		else infinitAmmoText.text = "Infinate Ammo : UNAPPLIED";
		timer += Time.deltaTime;
		time = (Mathf.Round(timer * 100f) * 0.01f);
		timeText.text = "Time : " + time.ToString();

		//Aiming
		//Toggle camera FOV when right click is held down
		if (Input.GetButton("Fire2") && !isReloading && !isInspecting && !stopTime) 
		{
			
			gunCamera.fieldOfView = Mathf.Lerp (gunCamera.fieldOfView,
				aimFov, fovSpeed * Time.deltaTime);
			
			isAiming = true;

			crossAir.SetActive(false);
			anim.SetBool ("Aim", true);
			if (!soundHasPlayed) 
			{
				mainAudioSource.clip = SoundClips.aimSound;
				mainAudioSource.Play ();
	
				soundHasPlayed = true;
			}
		} 
		else 
		{
			//When right click is released
			gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
				defaultFov,fovSpeed * Time.deltaTime);

			isAiming = false;
			crossAir.SetActive(true);
			anim.SetBool ("Aim", false);
		}
		//Aiming end

		//If randomize muzzleflash is true, genereate random int values
		if (randomMuzzleflash == true) {
			randomMuzzleflashValue = UnityEngine.Random.Range(minRandomValue, maxRandomValue);
		}
			
		//Timescale settings
		if (Input.GetKeyDown(KeyCode.Escape) && !stopTime)		//Pause game when ESC key is pressed
        {
            Time.timeScale = 0.0f;
			PauseUI.SetActive(true);
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
			stopTime = true;
        }
        else if (Input.GetKeyDown (KeyCode.Escape) && stopTime)         //Change timescale to normal when ESC key is pressed again
        {
			Time.timeScale = 1.0f;
			PauseUI.SetActive(false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			stopTime = false;
		}
		


		//Set current ammo text from ammo int
		currentAmmoText.text = currentAmmo.ToString ();

		//Continosuly check which animation 
		//is currently playing
		AnimationCheck ();



		//If out of ammo
		if (currentAmmo == 0) 
		{
			//Show out of ammo text
			currentWeaponText.text = "OUT OF AMMO";
			//Toggle bool
			outOfAmmo = true;
			//Auto reload if true
			if (autoReload == true && !isReloading) 
			{
				StartCoroutine (AutoReload ());
			}
				
			//Set slider back
			anim.SetBool ("Out Of Ammo Slider", true);
			//Increase layer weight for blending to slider back pose
			anim.SetLayerWeight (1, 1.0f);
		} 
		else 
		{
			//When ammo is full, show weapon name again
			currentWeaponText.text = storedWeaponName.ToString ();
			//Toggle bool
			outOfAmmo = false;
			//anim.SetBool ("Out Of Ammo", false);
			anim.SetLayerWeight (1, 0.0f);
		}

		//Shooting 
		if (Input.GetMouseButtonDown (0) && !outOfAmmo && !isReloading && !isInspecting && !stopTime) 
		{
			anim.Play ("Fire", 0, 0f);
	
			muzzleParticles.Emit (1);
				
			//Remove 1 bullet from ammo
			if(infinateAmmo == false) currentAmmo -= 1;
			shootAudioSource.clip = SoundClips.shootSound;
			shootAudioSource.Play ();

			StartCoroutine(MuzzleFlashLight());

			if (!isAiming) //not aiming
			{
				anim.Play ("Fire", 0, 0f);
		
				muzzleParticles.Emit (1);

				if (enableSparks == true) 
				{
					//spawn random amount of spark particles
					sparkParticles.Emit (UnityEngine.Random.Range (1, 6));
				}
			} 
			else //aiming
			{
				anim.Play ("Aim Fire", 0, 0f);
					
				//if random muzzle is false
				if (!randomMuzzleflash) {
					muzzleParticles.Emit (1);
					//if random muzzle is true
				} 
				else if (randomMuzzleflash == true) 
				{
					//Only emit if random value is 1
					if (randomMuzzleflashValue == 1) 
					{
						if (enableSparks == true) 
						{
							//spawn random amount of spark particles
							sparkParticles.Emit(UnityEngine.Random.Range (1, 6));
						}
						if (enableMuzzleflash == true) 
						{
							muzzleParticles.Emit (1);
							//muzzle flash effect
							StartCoroutine (MuzzleFlashLight ());
						}
					}
				}
			}
				
			//Spawn bullet at bullet spawnpoint
			var bullet = (Transform)Instantiate (
				Prefabs.bulletPrefab,
				Spawnpoints.bulletSpawnPoint.transform.position,
				Spawnpoints.bulletSpawnPoint.transform.rotation);

			//fire bulit
			bullet.GetComponent<Rigidbody>().velocity = 
			bullet.transform.forward * bulletForce;

			//Spawn casing prefab at spawnpoint
			Instantiate (Prefabs.casingPrefab, 
				Spawnpoints.casingSpawnPoint.transform.position, 
				Spawnpoints.casingSpawnPoint.transform.rotation);
		}

		//Reload 
		if (Input.GetKeyDown(KeyCode.R) && !isReloading && !isInspecting && (currentAmmo < ammo)) 
		{
			//Reload
			Reload();

			if (!hasStartedSliderBack) 
			{
				hasStartedSliderBack = true;
				StartCoroutine(HandgunSliderBackDelay());
			}
		}

		//Walk animation when move
		if (Input.GetKey(KeyCode.W) || 
			Input.GetKey(KeyCode.A) || 
			Input.GetKey(KeyCode.S) || 
			Input.GetKey(KeyCode.D) ) 
		{
			anim.SetBool("Walk", true);
		} else {
			anim.SetBool("Walk", false);
		}


		EndGame();
		endTimeText.text = "SetEndSec  : " + endSec.ToString();
	}

	private IEnumerator HandgunSliderBackDelay () {
		//Wait set amount of time
		yield return new WaitForSeconds (sliderBackTimer);
		//Set slider back
		anim.SetBool ("Out Of Ammo Slider", false);
		//Increase layer weight for blending to slider back pose
		anim.SetLayerWeight (1, 0.0f);

		hasStartedSliderBack = false;
	}

	private IEnumerator AutoReload () {

		if (!hasStartedSliderBack) 
		{
			hasStartedSliderBack = true;

			StartCoroutine (HandgunSliderBackDelay());
		}
		//Wait for set amount of time
		yield return new WaitForSeconds (autoReloadDelay);

		if (outOfAmmo == true) {
			//Play diff anim if out of ammo
			anim.Play ("Reload Out Of Ammo", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
			mainAudioSource.Play ();

			//If out of ammo, hide the bullet renderer in the mag
			//Do not show if bullet renderer is not assigned in inspector
			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = false;
				//Start show bullet delay
				StartCoroutine (ShowBulletInMag ());
			}
		} 
		//Restore ammo when reloading
		currentAmmo = ammo;
		outOfAmmo = false;
	}

	//Reload
	private void Reload () {
		
		if (outOfAmmo == true) 
		{
			//Play diff anim if out of ammo
			anim.Play ("Reload Out Of Ammo", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundOutOfAmmo;
			mainAudioSource.Play ();

			//If out of ammo, hide the bullet renderer in the mag
			//Do not show if bullet renderer is not assigned in inspector
			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = false;
				//Start show bullet delay
				StartCoroutine (ShowBulletInMag ());
			}
		} 
		else 
		{
			//Play diff anim if ammo left
			anim.Play ("Reload Ammo Left", 0, 0f);

			mainAudioSource.clip = SoundClips.reloadSoundAmmoLeft;
			mainAudioSource.Play ();

			//If reloading when ammo left, show bullet in mag
			//Do not show if bullet renderer is not assigned in inspector
			if (bulletInMagRenderer != null) 
			{
				bulletInMagRenderer.GetComponent
				<SkinnedMeshRenderer> ().enabled = true;
			}
		}
		//Restore ammo when reloading
		currentAmmo = ammo;
		outOfAmmo = false;
	}

	//Enable bullet in mag renderer after set amount of time
	private IEnumerator ShowBulletInMag () {
		//Wait set amount of time before showing bullet in mag
		yield return new WaitForSeconds (showBulletInMagDelay);
		bulletInMagRenderer.GetComponent<SkinnedMeshRenderer> ().enabled = true;
	}

	//Show light when shooting, then disable after set amount of time
	private IEnumerator MuzzleFlashLight () 
	{
		muzzleflashLight.enabled = true;
		yield return new WaitForSeconds (lightDuration);
		muzzleflashLight.enabled = false;
	}

	//Check current animation playing
	private void AnimationCheck () 
	{
		//Check if reloading
		//Check both animations
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Out Of Ammo") || 
			anim.GetCurrentAnimatorStateInfo (0).IsName ("Reload Ammo Left")) 
		{
			isReloading = true;
		} 
		else 
		{
			isReloading = false;
		}

		//Check if inspecting weapon
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Inspect")) 
		{
			isInspecting = true;
		} 
		else 
		{
			isInspecting = false;
		}
	}
	
	public void InfiniteAmmo()
    {
		if(infinateAmmo == false) infinateAmmo = true;
		else infinateAmmo = false;
    }

	private void EndGame()
    {
        if (time >= endSec && endSec != 0) { 
			Time.timeScale = 0.0f;
			gameEnd = true;
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = true;
			EndUI.SetActive(true);
			if (scoreCount * scoreMultiplier > bestScore)
			{
				bestScore = scoreCount * (-1 * scoreMultiplier * (obj.GetComponent<Target>().difficulty - 5));
				PlayerPrefs.SetFloat("BestScore", bestScore);
			}
			endBestScoreText.text = "BestScore : " + bestScore.ToString();
			endCurrentScoreText.text = "CurrentScore : " + (scoreCount * (-1 * scoreMultiplier * (obj.GetComponent<Target>().difficulty - 5))).ToString();
			if (obj.GetComponent<Target>().difficulty == 0) endDifficultyText.text = "Difficulty : Insane";
			if(obj.GetComponent<Target>().difficulty ==1) endDifficultyText.text = "Difficulty : Difficult";
			if (obj.GetComponent<Target>().difficulty ==2) endDifficultyText.text = "Difficulty : Normal";
			if (obj.GetComponent<Target>().difficulty ==3) endDifficultyText.text = "Difficulty : Easy";
			if (infinateAmmo == true) endInfinitAmmoText.text = "Infinate Ammo : APPLIED";
			else endInfinitAmmoText.text = "Infinate Ammo : UNAPPLIED";
		}
    }

	public void ResetGame()
    {
		ResetScore();
		ResetGameSec();
		endSec = 0.0f;
		EndUI.SetActive(false);
		Time.timeScale = 1.0f;
	}

	private void ResetGameSec() {timer = 0.0f;}

	private void ResetScore() {scoreCount = 0f;}

	public void AddEndSec()
    {
		ResetScore();
		ResetGameSec();
		endSec +=1;
		endSecText.text = "EndSecondd : " + endSec.ToString();
	}
	public void ReduceEndSec()
	{
		if (endSec <= 0) return;
		endSec -= 1;
	}

	public void ReturntoMainMenu()		//enter MainMenuName name in inspector view
    {
		SceneManager.LoadScene(mainMenuName, LoadSceneMode.Single);
    }
}
// ----- Low Poly FPS Pack Free Version -----