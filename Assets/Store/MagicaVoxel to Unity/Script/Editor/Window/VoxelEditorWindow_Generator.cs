namespace VoxeltoUnity {
	using Moenen.Saving;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	// Generator Part
	public partial class VoxelEditorWindow {





		#region --- SUB ---



		private enum GeneratorMode {
			Character = 0,
			Map = 1,
		}



		#endregion




		#region --- VAR ---



		// Global
		private static readonly string[] MAP_STEP_LABELS = new string[] {
			"{0}/{1} Iterating {2}%",
			"{0}/{1} Generating ground {2}%",
			"{0}/{1} Generating cave {2}%",
			"{0}/{1} Generating water {2}%",
			"{0}/{1} Fixing water {2}%",
		};
		private static readonly string[] CHARACTER_STEP_LABELS = new string[] {
			"{0}/{1} Iterating {2}%",
		};



		// Short Cut
		private bool IsGenerating {
			get {
				return GeneratorEditingRoot && GeneratorEditingRoot.gameObject.activeSelf;
			}
		}



		// Data
		private GeneratorMode CurrentGeneratorMode = GeneratorMode.Character;
		private Core_MapGeneration.Preset MGConfig = new Core_MapGeneration.Preset();
		private Core_CharacterGeneration.Preset CGConfig = new Core_CharacterGeneration.Preset();





		#endregion




		#region --- GUI ---



		private void GeneratorGUI () {
			if (!IsGenerating) { return; }
			switch (CurrentGeneratorMode) {
				case GeneratorMode.Character:
					CharacterGeneratorGUI();
					break;
				case GeneratorMode.Map:
					MapGeneratorGUI();
					break;
			}
		}



		private void GeneratorPanelGUI () {

			const int WIDTH = 158;
			const int HEIGHT = 30;
			const int GAP = 3;

			var buttonStyle = new GUIStyle(EditorStyles.miniButtonLeft) {
				fontSize = 11,
			};
			var dotStyle = new GUIStyle(GUI.skin.label) {
				richText = true,
				alignment = TextAnchor.MiddleLeft,
			};

			Rect rect = new Rect(
				ViewRect.x + ViewRect.width - WIDTH,
				ViewRect.y + ViewRect.height - HEIGHT - GAP,
				WIDTH,
				HEIGHT
			);

			if (GUI.Button(rect, "   Generate", buttonStyle)) {
				Generate();
			}
			rect.y -= HEIGHT + GAP;


			if (GUI.Button(rect, "   Generate And Save", buttonStyle)) {
				Generate();
				SaveData();
			}
			GUI.Label(rect, "  <color=#cc66ff>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;


			if (GUI.Button(rect, "   Export Preset", buttonStyle)) {
				ExportPreset_Dialog();
			}
			GUI.Label(rect, "  <color=#cccccc>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;

			if (GUI.Button(rect, "   Import Preset", buttonStyle)) {
				ImportPreset_Dialog();
			}
			GUI.Label(rect, "  <color=#cccccc>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;
		}




		private void CharacterGeneratorGUI () {
			LayoutV(() => {

				//const int HEIGHT = 16;
				//const int LABEL_WIDTH = 82;
				//const int GAP_WIDTH = 22;
				//const int GAP_HEIGHT = 6;

				bool changed = false;








				if (changed) {
					CGConfig.FixGenerationValues();
					Save_Generation();
					SetDataDirty();
				}


			}, false, new GUIStyle(GUI.skin.box) {
				padding = new RectOffset(9, 6, 4, 4),
				margin = new RectOffset(14, 20, 20, 4),
			});
		}





		private void MapGeneratorGUI () {
			LayoutV(() => {

				const int HEIGHT = 16;
				const int LABEL_WIDTH = 82;
				const int GAP_WIDTH = 22;
				const int GAP_HEIGHT = 6;

				bool changed = false;
				Space(GAP_HEIGHT);

				// Size 
				LayoutH(() => {
					changed = IntField(LABEL_WIDTH, HEIGHT, "Map Size", ref MGConfig.SizeX) || changed;
					Space(2);
					changed = IntField(16, HEIGHT, "×", ref MGConfig.SizeY) || changed;
				});
				Space(GAP_HEIGHT);

				LayoutH(() => {
					// Iteration
					changed = IntField(LABEL_WIDTH, HEIGHT, "Iteration", ref MGConfig.Iteration) || changed;
					Space(GAP_WIDTH);
					// Radius
					changed = FloatField(LABEL_WIDTH, HEIGHT, "Iter Radius", ref MGConfig.IterationRadius) || changed;
					Space(GAP_WIDTH);
					// Lerp
					changed = FloatField(LABEL_WIDTH, HEIGHT, "Iter Lerp", ref MGConfig.IterationLerp) || changed;

				});
				Space(GAP_HEIGHT - 1);


				LayoutH(() => {
					// Island
					var newIsland = EditorGUI.ToggleLeft(GUIRect(LABEL_WIDTH - 8, HEIGHT), " Island", MGConfig.Island);
					if (newIsland != MGConfig.Island) {
						MGConfig.Island = newIsland;
						changed = true;
					}
					Space(GAP_WIDTH);

					// Tint
					var newTint = EditorGUI.ToggleLeft(GUIRect(LABEL_WIDTH - 8, HEIGHT), " Tint", MGConfig.Tint);
					if (newTint != MGConfig.Tint) {
						MGConfig.Tint = newTint;
						changed = true;
					}
					Space(GAP_WIDTH);

					// Land
					var newLand = EditorGUI.ToggleLeft(GUIRect(LABEL_WIDTH - 8, HEIGHT), " Land", MGConfig.Land);
					if (newLand != MGConfig.Land) {
						MGConfig.Land = newLand;
						changed = true;
					}
					Space(GAP_WIDTH);

					// Water
					var newWater = EditorGUI.ToggleLeft(GUIRect(LABEL_WIDTH - 8, HEIGHT), " Water", MGConfig.Water);
					if (newWater != MGConfig.Water) {
						MGConfig.Water = newWater;
						changed = true;
					}
					Space(GAP_WIDTH);

					// Cave
					var newCave = EditorGUI.ToggleLeft(GUIRect(LABEL_WIDTH - 8, HEIGHT), " Cave", MGConfig.Cave);
					if (newCave != MGConfig.Cave) {
						MGConfig.Cave = newCave;
						changed = true;
					}
				});
				Space(GAP_HEIGHT - 1);


				LayoutH(() => {
					// Land Height
					changed = MinMaxField(LABEL_WIDTH, 18, 0, HEIGHT, "Land Height", "", " -", ref MGConfig.GroundHeight, MGConfig.Land) || changed;
					Space(GAP_WIDTH);
					// Land Bump
					changed = FloatField(LABEL_WIDTH, HEIGHT, "Land Bump", ref MGConfig.GroundBump, MGConfig.Land) || changed;
				});
				Space(GAP_HEIGHT);


				LayoutH(() => {
					// Water Height
					changed = MinMaxField(LABEL_WIDTH, 18, 0, HEIGHT, "Water Height", "", " -", ref MGConfig.WaterHeight, MGConfig.Water) || changed;
					Space(GAP_WIDTH);
					// Water Bump
					changed = FloatField(LABEL_WIDTH, HEIGHT, "Water Bump", ref MGConfig.WaterBump, MGConfig.Water) || changed;
				});
				Space(GAP_HEIGHT);


				LayoutH(() => {
					// Cave Height
					changed = MinMaxField(LABEL_WIDTH, 18, 0, HEIGHT, "Cave Height", "", " -", ref MGConfig.CaveHeight, MGConfig.Cave) || changed;
					Space(GAP_WIDTH);
					// Cave Bump
					changed = FloatField(LABEL_WIDTH, HEIGHT, "Cave Bump", ref MGConfig.CaveBump, MGConfig.Cave) || changed;
					Space(GAP_WIDTH);
					// Cave Radius 
					changed = FloatField(LABEL_WIDTH, HEIGHT, "Cave Radius", ref MGConfig.CaveRadius, MGConfig.Cave) || changed;
				});
				Space(GAP_HEIGHT);




				// Colors
				Space(GAP_HEIGHT);
				LayoutH(() => {
					// Label
					GUI.Label(GUIRect(LABEL_WIDTH - 8, HEIGHT), "Land");
					// Add Ground Color
					if (Button(24, HEIGHT, "+", MGConfig.GroundColors.Count < Core_MapGeneration.Preset.MAX_COLOR_COUNT, EditorStyles.miniButtonLeft)) {
						MGConfig.GroundColors.Add(MGConfig.GroundColors.Count > 0 ? MGConfig.GroundColors[MGConfig.GroundColors.Count - 1] : Color.grey);
						changed = true;
						Repaint();
					}
					// Ground Color
					for (int i = 0; i < MGConfig.GroundColors.Count; i++) {
						var color = ColorField(0, HEIGHT, "", MGConfig.GroundColors[i]);
						if (color != MGConfig.GroundColors[i]) {
							MGConfig.GroundColors[i] = color;
							changed = true;
						}
					}
					// Remove Ground Color
					if (Button(24, HEIGHT, "-", MGConfig.GroundColors.Count > 0, EditorStyles.miniButtonRight)) {
						MGConfig.GroundColors.RemoveAt(MGConfig.GroundColors.Count - 1);
						changed = true;
						Repaint();
					}
				});
				Space(GAP_HEIGHT);

				LayoutH(() => {
					// Label
					GUI.Label(GUIRect(LABEL_WIDTH - 8, HEIGHT), "Grass");
					// Add Grass Color
					if (Button(24, HEIGHT, "+", MGConfig.GrassColors.Count < Core_MapGeneration.Preset.MAX_COLOR_COUNT, EditorStyles.miniButtonLeft)) {
						MGConfig.GrassColors.Add(MGConfig.GrassColors.Count > 0 ? MGConfig.GrassColors[MGConfig.GrassColors.Count - 1] : Color.green);
						changed = true;
						Repaint();
					}
					// Grass Color
					for (int i = 0; i < MGConfig.GrassColors.Count; i++) {
						var color = ColorField(0, HEIGHT, "", MGConfig.GrassColors[i]);
						if (color != MGConfig.GrassColors[i]) {
							MGConfig.GrassColors[i] = color;
							changed = true;
						}
					}
					// Remove Grass Color
					if (Button(24, HEIGHT, "-", MGConfig.GrassColors.Count > 0, EditorStyles.miniButtonRight)) {
						MGConfig.GrassColors.RemoveAt(MGConfig.GrassColors.Count - 1);
						changed = true;
						Repaint();
					}
				});
				Space(GAP_HEIGHT);

				LayoutH(() => {
					GUI.Label(GUIRect(LABEL_WIDTH - 8, HEIGHT), "Water");
					var color = ColorField(0, HEIGHT, "", MGConfig.WaterColor);
					if (color != MGConfig.WaterColor) {
						MGConfig.WaterColor = color;
						changed = true;
					}
				});
				Space(GAP_HEIGHT);



				// Random Button
				Space(GAP_HEIGHT);
				LayoutH(() => {
					// Random
					if (GUI.Button(GUIRect(LABEL_WIDTH + 30, HEIGHT), "Random", EditorStyles.miniButton)) {
						MGConfig.CreateSeed();
						EditorApplication.delayCall += Generate;
						changed = true;
					}
					Space(GAP_WIDTH);
					// Seed
					string seedLabel = "(Null)";
					if (!string.IsNullOrEmpty(MGConfig.Seeds)) {
						seedLabel = "Seed:" + MGConfig.Seeds.Substring(0, Mathf.Min(16, MGConfig.Seeds.Length)) + "...";
					}
					GUI.Label(GUIRect(220, HEIGHT), seedLabel);
				});
				Space(GAP_HEIGHT);


				if (changed) {
					MGConfig.FixGenerationValues();
					Save_Generation();
					SetDataDirty();
				}

			}, false, new GUIStyle(GUI.skin.box) {
				padding = new RectOffset(9, 6, 4, 4),
				margin = new RectOffset(14, 20, 20, 4),
			});
		}



		#endregion




		#region --- LGC ---



		private void SwitchGeneratorMode (GeneratorMode mode) {
			CurrentModelIndex = 0;
			VoxelFilePath = "";
			CurrentGeneratorMode = mode;
			Data = VoxelData.CreateNewData();
			// Node Open
			if (Data) {
				foreach (var gp in Data.Groups) {
					if (!NodeOpen.ContainsKey(gp.Key)) {
						NodeOpen.Add(gp.Key, false);
					}
				}
			}
			// Switch Model
			SwitchModel(CurrentModelIndex);
			Load_Generation();
			DataDirty = false;
		}



		private void Generate () {
			try {
				switch (CurrentGeneratorMode) {
					case GeneratorMode.Character:
						CGConfig.FixGenerationValues();
						Data = Core_CharacterGeneration.Generate(CGConfig, ProgressStep_Cha);
						break;
					case GeneratorMode.Map:
						MGConfig.FixGenerationValues();
						Data = Core_MapGeneration.Generate(MGConfig, ProgressStep_Map);
						break;
				}

				SwitchModel(CurrentModelIndex, true);

				SetDataDirty();
			} catch (System.Exception ex) {
				Debug.LogWarning("[Voxel Generator] " + ex.Message);
			}
		}



		private void ImportPreset_Dialog () {
			try {
				string path = EditorUtility.OpenFilePanel("Import Generation Preset", "Assets", "json");
				if (!string.IsNullOrEmpty(path)) {
					var json = Util.Read(path);
					if (!string.IsNullOrEmpty(json)) {
						switch (CurrentGeneratorMode) {
							case GeneratorMode.Character:
								CGConfig.LoadFromJson(json);
								break;
							case GeneratorMode.Map:
								MGConfig.LoadFromJson(json);
								break;
						}
					}
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex.Message);
			}
		}



		private void ExportPreset_Dialog () {
			try {
				var json = "";
				switch (CurrentGeneratorMode) {
					case GeneratorMode.Character:
						json = CGConfig.ToJson();
						break;
					case GeneratorMode.Map:
						json = MGConfig.ToJson();
						break;
				}
				if (!string.IsNullOrEmpty(json)) {
					string path = EditorUtility.SaveFilePanelInProject("Export Generation Preset", "Generation Preset", "json", "");
					if (!string.IsNullOrEmpty(path)) {
						Util.Write(json, path);
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
					}
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex.Message);
			}
		}



		private void Load_Generation () {
			try {
				string rootPath = Util.GetRootPath(this);
				if (!string.IsNullOrEmpty(rootPath)) {
					string path = Util.CombinePaths(rootPath, "Data", "Current " + CurrentGeneratorMode.ToString() + " Generation Preset.json");
					if (Util.FileExists(path)) {
						switch (CurrentGeneratorMode) {
							case GeneratorMode.Map:
								MGConfig.LoadFromJson(Util.Read(path));
								break;
							case GeneratorMode.Character:
								CGConfig.LoadFromJson(Util.Read(path));
								break;
						}
					}
				}
			} catch { };
		}



		private void Save_Generation () {
			try {
				string rootPath = Util.GetRootPath(this);
				if (!string.IsNullOrEmpty(rootPath)) {
					string json = "";
					switch (CurrentGeneratorMode) {
						case GeneratorMode.Map:
							json = MGConfig.ToJson();
							break;
						case GeneratorMode.Character:
							json = CGConfig.ToJson();
							break;
					}
					if (!string.IsNullOrEmpty(json)) {
						Util.Write(json, Util.CombinePaths(rootPath, "Data", "Current " + CurrentGeneratorMode.ToString() + " Generation Preset.json"));
					}
				}
			} catch { };
		}



		#endregion



		#region --- UTL ---



		private void ProgressStep_Map (float progress01, float step01) {
			ProgressStep(progress01, step01, GeneratorMode.Map);
		}



		private void ProgressStep_Cha (float progress01, float step01) {
			ProgressStep(progress01, step01, GeneratorMode.Character);
		}



		private void ProgressStep (float progress01, float step01, GeneratorMode mod) {
			if (step01 > 1.001f) {
				Util.ClearProgressBar();
				return;
			}
			string[] labels = mod == GeneratorMode.Map ? MAP_STEP_LABELS : CHARACTER_STEP_LABELS;
			int step = Mathf.Clamp((int)(step01 * (labels.Length + 1)), 0, labels.Length - 1);
			Util.ProgressBar(
				"Generating...",
				string.Format(
					labels[step],
					(step + 1).ToString(),
					labels.Length.ToString(),
					(progress01 * 100).ToString("00")
				),
				progress01 / labels.Length + step01
			);
		}




		#endregion

	}
}