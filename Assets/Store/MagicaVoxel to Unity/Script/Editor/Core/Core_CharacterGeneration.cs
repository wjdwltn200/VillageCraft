namespace VoxeltoUnity {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public static class Core_CharacterGeneration {




		#region --- SUB ---




		[System.Serializable]
		public class Preset {



			public int HeadIndex = 0;
			public int NeckIndex = 0;
			public int BodyIndex = 0;
			public int ArmIndex = 0;
			public int LegIndex = 0;
			public int FootIndex = 0;


			

			public void LoadFromJson (string json) {
				try {
					if (string.IsNullOrEmpty(json)) { return; }
					JsonUtility.FromJsonOverwrite(json, this);
				} catch { }
			}



			public string ToJson () {
				return JsonUtility.ToJson(this, true);
			}

			


			public void FixGenerationValues () {





			}



		}



		#endregion




		public static VoxelData Generate (Preset preset, System.Action<float, float> onProgress = null) {
			try {
				var data = VoxelData.CreateNewData();
				if (preset == null) { return data; }








				if (onProgress != null) {
					onProgress(1f, 2f);
				}
				return data;
			} catch (System.Exception ex) {
				if (onProgress != null) {
					onProgress(1f, 2f);
				}
				throw ex;
			}
		}



	}
}