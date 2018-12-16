namespace MoenenGames.LowpolyParticle {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class Demo : MonoBehaviour {



		// SUB
		public enum Movement {
			None = 0,
			PingPong = 1,
			Repeat = 2,
			Shoot = 3,
			RandomSpawn = 4,
		}



		[System.Serializable]
		public class ParticleData {


			public Transform this[int index] {
				get {
					return ParticleRoot[Mathf.Clamp(index, 0, Count - 1)];
				}
			}

			public int Count {
				get {
					return ParticleRoot.Length;
				}
			}

			[SerializeField]
			private string Editor_Label = "";
			[SerializeField]
			private Transform[] ParticleRoot;
			public bool UsingBlackRoom = true;
			public Movement Movement;
			public float MovementDistance = 4f;
			public float MovementDuration = 1.4f;
			public float LerpRate = 2f;
			public float Random = 0f;


			private void EditorWarningKiller () {
				string a = Editor_Label;
				Editor_Label = a;
			}


		}




		// VAR
		private Transform ShowingParticleRoot {
			get {
				if (!m_ShowingParticleRoot) {
					m_ShowingParticleRoot = new GameObject("Showing Particle Root").transform;
					m_ShowingParticleRoot.SetParent(null);
					m_ShowingParticleRoot.position = Vector3.zero;
					m_ShowingParticleRoot.rotation = Quaternion.identity;
					m_ShowingParticleRoot.localScale = Vector3.one;
				}
				return m_ShowingParticleRoot;
			}
		}


		[SerializeField]
		private Text NameLabel;
		[SerializeField]
		private Transform SwitchPanelRoot;
		[SerializeField]
		private Toggle SwitchTGTemplate;
		[SerializeField]
		private Image Bar;
		[SerializeField]
		private Transform RoomRoot;
		[SerializeField]
		private Color BlackRoomColor;
		[SerializeField]
		private Color WhiteRoomColor;
		[SerializeField]
		private List<ParticleData> Data;



		private int CurrentParticleIndex = 0;
		private int CurrentSubIndex = 1;
		private bool UsingBlackTheme = true;
		private Transform m_ShowingParticleRoot = null;
		private Coroutine ParticleMovementCor = null;
		private Material[] RoomMats = null;




		// MSG
		private void Awake () {
			SwitchParticle();
			MeshRenderer[] mrs = RoomRoot.GetComponentsInChildren<MeshRenderer>(true);
			RoomMats = new Material[mrs.Length];
			for (int i = 0; i < mrs.Length; i++) {
				RoomMats[i] = mrs[i].material;
			}
		}



		private void Update () {

			if (Input.GetKeyDown(KeyCode.LeftArrow) ||
				Input.GetKeyDown(KeyCode.DownArrow) ||
				Input.GetKeyDown(KeyCode.Backspace)
			) {
				SpawnPrevParticle();
			}


			if (Input.GetKeyDown(KeyCode.RightArrow) ||
				Input.GetKeyDown(KeyCode.UpArrow) ||
				Input.GetKeyDown(KeyCode.Space) ||
				Input.GetKeyDown(KeyCode.Return)
			) {
				SpawnNextParticle();
			}


			if (Input.GetKeyDown(KeyCode.Tab)) {
				ChangeSubIndex((CurrentSubIndex + 1) % Data[CurrentParticleIndex].Count);
				RespawnToggles();
				ResetName();
			}


			// Room Theme
			for (int i = 0; i < RoomMats.Length; i++) {
				RoomMats[i].color = Color.Lerp(
					RoomMats[i].color,
					UsingBlackTheme ? BlackRoomColor : WhiteRoomColor,
					Time.deltaTime * 5f
				);
			}

		}



		private void SwitchParticle () {

			// Check
			if (Data[CurrentParticleIndex][CurrentSubIndex] == null) {
				NameLabel.text = "";
				return;
			}

			// Name
			ResetName();

			// Switch Panel
			RespawnToggles();

			// Play
			RespawnParticle(CurrentParticleIndex, CurrentSubIndex);

		}




		public void RespawnParticle (int index, int subIndex) {

			// Delete Old
			if (m_ShowingParticleRoot) {
				DestroyImmediate(m_ShowingParticleRoot.gameObject, false);
			}

			// Stop Movement
			if (ParticleMovementCor != null) {
				StopCoroutine(ParticleMovementCor);
			}

			// Bar
			FreshBar();

			// Add New
			ParticleData data = Data[index];
			Transform tf = Instantiate(data[subIndex]);
			tf.SetParent(ShowingParticleRoot);
			tf.localPosition = Vector3.zero;
			tf.localRotation = Quaternion.identity;
			tf.localScale = Vector3.one;

			switch (data.Movement) {
				case Movement.PingPong:
					ParticleMovementCor = StartCoroutine(PingPongMovement(tf, data));
					break;
				case Movement.Repeat:
					ParticleMovementCor = StartCoroutine(RepeatMovement(tf, data));
					break;
				case Movement.Shoot:
					Destroy(tf.gameObject);
					ParticleMovementCor = StartCoroutine(ShootMovement(data));
					break;
				case Movement.RandomSpawn:
					Destroy(tf.gameObject);
					ParticleMovementCor = StartCoroutine(RandomSpawnMovement(data));
					break;
			}

			// Room
			UsingBlackTheme = data.UsingBlackRoom;

		}



		// API
		public void UGUI_NextParticle () {
			SpawnNextParticle();
		}



		public void UGUI_PrevParticle () {
			SpawnPrevParticle();
		}



		public void UGUI_FiveStar () {
			Application.OpenURL(@"http://u3d.as/Mq0");
		}






		// Logic
		private IEnumerator PingPongMovement (Transform particleRoot, ParticleData data) {
			while (particleRoot) {

				Vector3 aimPos = data.MovementDistance * (
					Mathf.PingPong(Time.time, data.MovementDuration) > data.MovementDuration * 0.5f ?
					Vector3.right :
					Vector3.left
				);

				particleRoot.localPosition = Vector3.Lerp(
					particleRoot.localPosition,
					aimPos,
					Time.deltaTime * data.LerpRate
				);

				yield return new WaitForEndOfFrame();
			}
		}



		private IEnumerator RepeatMovement (Transform particleRoot, ParticleData data) {

			float lastShootTime = Time.time;

			while (particleRoot) {

				if (Time.time > lastShootTime + data.MovementDuration) {
					lastShootTime = Time.time;
					particleRoot.localPosition = Vector3.left * data.MovementDistance;
				}

				particleRoot.localPosition = Vector3.Lerp(
					particleRoot.localPosition,
					Vector3.right * data.MovementDistance,
					Time.deltaTime * data.LerpRate
				);

				yield return new WaitForEndOfFrame();
			}
		}



		private IEnumerator ShootMovement (ParticleData data) {
			float lastShootTime = Time.time - data.MovementDuration;
			float currentDirY = 0f;
			while (true) {
				if (Time.time > lastShootTime + data.MovementDuration) {
					lastShootTime = Time.time;

					// Shoot
					Transform tf = Instantiate(data[CurrentSubIndex]);
					tf.SetParent(ShowingParticleRoot);
					tf.localPosition = Vector3.zero;
					tf.localRotation = Quaternion.identity;
					tf.localScale = Vector3.one;

					float maxDuration = 0f;
					ParticleSystem[] ps = tf.GetComponentsInChildren<ParticleSystem>(true);
					for (int i = 0; i < ps.Length; i++) {

						maxDuration = Mathf.Max(
							maxDuration,
							ps[i].main.duration
						);
					}
					Destroy(tf.gameObject, maxDuration);

					Rigidbody rig = tf.GetComponent<Rigidbody>();
					if (rig) {
						rig.velocity = Quaternion.Euler(0f, currentDirY, 0f) * Vector3.forward * data.MovementDistance;
					}
					currentDirY += Time.deltaTime * (data.LerpRate + (2f * Random.value - 1f) * data.Random);
					currentDirY %= 360f;

				}
				yield return new WaitForEndOfFrame();
			}
		}



		private IEnumerator RandomSpawnMovement (ParticleData data) {
			float lastShootTime = Time.time - data.MovementDuration;
			while (true) {
				if (Time.time > lastShootTime + data.MovementDuration) {
					lastShootTime = Time.time;
					Vector3 pos = Random.insideUnitSphere * data.MovementDistance;
					pos.y = 0f;

					// Spawn
					Transform tf = Instantiate(data[CurrentSubIndex]);
					tf.SetParent(ShowingParticleRoot);
					tf.localPosition = pos;
					tf.localRotation = Quaternion.identity;
					tf.localScale = Vector3.one;

					float maxDuration = 0f;
					ParticleSystem[] ps = tf.GetComponentsInChildren<ParticleSystem>(true);
					for (int i = 0; i < ps.Length; i++) {
						maxDuration = Mathf.Max(
							maxDuration,
							ps[i].main.duration
						);
					}
					Destroy(tf.gameObject, maxDuration);



				}
				yield return new WaitForEndOfFrame();
			}
		}



		private void SpawnNextParticle () {
			CurrentParticleIndex = (int)Mathf.Repeat(CurrentParticleIndex + 1, Data.Count);
			SwitchParticle();
		}


		private void SpawnPrevParticle () {
			CurrentParticleIndex = (int)Mathf.Repeat(CurrentParticleIndex - 1, Data.Count);
			SwitchParticle();
		}


		private void ChangeSubIndex (int index) {
			CurrentSubIndex = index;
			RespawnParticle(CurrentParticleIndex, CurrentSubIndex);
		}


		private void RespawnToggles () {
			// Clear
			Toggle[] tgs = SwitchPanelRoot.GetComponentsInChildren<Toggle>(true);
			for (int i = 0; i < tgs.Length; i++) {
				tgs[i].onValueChanged.RemoveAllListeners();// I did this in demo code....
				DestroyImmediate(tgs[i].gameObject, false);
			}
			// Add
			for (int i = 0; i < Data[CurrentParticleIndex].Count; i++) {
				Toggle tg = Instantiate(SwitchTGTemplate);
				if (!tg.gameObject.activeSelf) {
					tg.gameObject.SetActive(true);
				}
				tg.transform.SetParent(SwitchPanelRoot);
				tg.transform.localScale = Vector3.one;
				tg.transform.localRotation = Quaternion.identity;
				int index = i;
				int count = Data[CurrentParticleIndex].Count;
				tg.isOn = i == Mathf.Clamp(CurrentSubIndex, 0, count - 1);
				tg.onValueChanged.AddListener((isOn) => {
					if (isOn) {
						ChangeSubIndex(index % count);
						ResetName();
					}
				});
				Text text = tg.GetComponentInChildren<Text>(true);
				if (text) {
					string _subName = Data[CurrentParticleIndex][i].name;
					if (!string.IsNullOrEmpty(_subName)) {
						text.text = _subName[_subName.Length - 1].ToString();
					}
				}
			}
		}


		private void ResetName () {
			string _name = Data[CurrentParticleIndex][CurrentSubIndex].name;
			NameLabel.text = _name + "\n<size=18>" + (CurrentParticleIndex + 1).ToString("00") + " / " + Data.Count.ToString("00") + "</size>";
		}


		private void FreshBar () {
			Bar.fillAmount = ((CurrentParticleIndex + 0.1f) / (Data.Count - 1f));
		}

	}
}