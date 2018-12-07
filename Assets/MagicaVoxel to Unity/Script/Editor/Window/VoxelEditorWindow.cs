namespace VoxeltoUnity {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Moenen.Saving;
	using Moenen;
	using UnityEngine.SceneManagement;
	using UnityEditor.SceneManagement;


	// Main Part
	public partial class VoxelEditorWindow : MoenenEditorWindow {



		#region --- SUB ---



		public enum EditorMode {
			Rigging = 0,
			Sprite = 1,
			Generator = 2,
		}



		#endregion




		#region --- VAR ---


		// Global
		private const string RIGGING_TITLE = "Rigging Editor";
		private const string SPRITE_TITLE = "Sprite Editor";
		private const string GENERATOR_TITLE = "Voxel Generator";
		private const string RIGGING_TITLE_RICH = "<color=#ff3333>R</color><color=#ffcc00>i</color><color=#ffff33>g</color><color=#ffff33>g</color><color=#33ff33>i</color><color=#33ffff>n</color><color=#33cccc>g</color><color=#eeeeee> Editor</color>";
		private const string SPRITE_TITLE_RICH = "<color=#ff3333>S</color><color=#ffcc00>p</color><color=#ffff33>r</color><color=#33ff33>i</color><color=#33ccff>t</color><color=#33ffff>e</color><color=#eeeeee> Editor</color>";
		private const string GENERATOR_TITLE_RICH = "<color=#ff3333>V</color><color=#ffcc00>o</color><color=#ffff33>x</color><color=#33ff33>e</color><color=#33ccff>l</color><color=#eeeeee> Generator</color>";
		private const int RENDER_TEXTURE_HEIGHT = 512;
		private static VoxelEditorWindow Main = null;
		private static readonly string[] EDITOR_TITLE = new string[3] { "Rigging Editor", "Sprite Editor", "Vox Generator" };


		// Short
		private bool RigAvailable {
			get {
				return !string.IsNullOrEmpty(VoxelFilePath) && Util.GetExtension(VoxelFilePath) == ".vox";
			}
		}


		// UI Data
		private VoxelData Data = null;
		private Texture2D FileIcon = null;
		private Int3 ModelSize = new Int3(0, 0, 0);
		private string VoxelFilePath = "";
		private int CurrentModelIndex = 0;
		private bool DataDirty = false;


		// Data
		private EditorMode CurrentEditorMode;
		private Vector2 MasterScrollPosition;
		private Rect ViewRect;
		private Dictionary<int, bool> NodeOpen = new Dictionary<int, bool>();
		private bool ColorfulTitle = true;
		private bool DataPanelOpen = false;
		private bool DataPalettePanelOpen = false;
		private bool DataNodePanelOpen = false;
		private bool DataMaterialPanelOpen = false;
		private bool DataRigPanelOpen = false;
		private bool VoxelTintColorChanged = false;

		// Save
		private EditorSavingBool ShowStateInfo = new EditorSavingBool("VEditor.ShowStateInfo", true);


		#endregion




		#region --- MSG ---


		[MenuItem("Tools/MagicaVoxel to Unity/Rigging Editor")]
		public static void OpenRiggingEditorWindow () {
			OpenWindow(EditorMode.Rigging, EditorPrefs.GetBool("V2U.EditorDockToScene", true));
		}


		[MenuItem("Tools/MagicaVoxel to Unity/Sprite Editor")]
		public static void OpenSpriteEditorWindow () {
			OpenWindow(EditorMode.Sprite, EditorPrefs.GetBool("V2U.EditorDockToScene", true));
		}


		[MenuItem("Tools/MagicaVoxel to Unity/Voxel Generator")]
		public static void OpenVoxelGeneratorWindow () {
			OpenWindow(EditorMode.Generator, EditorPrefs.GetBool("V2U.EditorDockToScene", true));
		}



		public static void OpenWindow (EditorMode mode, bool dockToScene) {
			if (Main) {
				if (Main.Data && !Main.CloseTarget()) {
					return;
				}
				Main.Close();
			}
			var window = dockToScene ?
				GetWindow<VoxelEditorWindow>("Voxel Editor", true, typeof(SceneView)) :
				GetWindow<VoxelEditorWindow>("Voxel Editor", true);
			window.CurrentEditorMode = mode;
			window.minSize = new Vector2(900, 800);
			window.maxSize = new Vector2(1200, 1000);
			window.titleContent = new GUIContent(EDITOR_TITLE[(int)mode]);
			if (!dockToScene) {
				window.position = new Rect(window.position) {
					width = window.minSize.x,
					height = window.minSize.y,
				};
			}
			window.CloseTarget(true);
		}



		[InitializeOnLoadMethod]
		private static void EditorInit () {
			var oldRoot = GameObject.Find(ROOT_NAME);
			if (oldRoot) {
				DestroyImmediate(oldRoot, false);
			}
			EditorSceneManager.sceneSaved += (scene) => {
				if (Main) {
					Main.SaveData();
					Main.Repaint();
				}
			};
		}



		private void OnEnable () {
			Main = this;
			wantsMouseMove = true;
			wantsMouseEnterLeaveWindow = true;
			DataPanelOpen = false;
			QUAD_SHADER = Shader.Find("Sprites/Default");
			GROUND_SHADER = Shader.Find("Unlit/Color");
			MESH_SHADER = Shader.Find("VoxelToUnity/VoxelMesh");
			COLOR_SHADER_ID = Shader.PropertyToID("_Color");
			LAYER_ID = LayerMask.NameToLayer("Water");
			LAYER_ID_ALPHA = LayerMask.NameToLayer("TransparentFX");
			LAYER_MASK = LayerMask.GetMask("Water");
			LAYER_MASK_ALPHA = LayerMask.GetMask("TransparentFX");
			EditorLoad();
			titleContent = new GUIContent(EDITOR_TITLE[(int)CurrentEditorMode]);
			Resources.UnloadUnusedAssets();
		}



		private void OnFocus () {
			EditorLoad();
		}



		private void OnDestroy () {
			RemoveRoot();
			Main = null;
		}



		private void OnGUI () {

			if (Data) {

				HeaderGUI();
				ToolbarGUI();

				MasterScrollPosition = GUILayout.BeginScrollView(MasterScrollPosition, GUI.skin.scrollView);

				ViewGUI();
				StateInfoGUI();
				EditToolGUI();

				CubeGUI();
				HighlightGUI();
				SceneBoneNameGUI();

				if (IsSpriting) {
					SpritePanelGUI();
				} else if (IsRigging) {
					RigPanelGUI();
				} else if (IsGenerating) {
					GeneratorPanelGUI();
				}

				RotationSliderGUI();

				RigEditingGUI();
				BoneMouseGUI();
				SpriteEditingGUI();

				WeightPointGUI();

				GeneratorGUI();

				DataGUI();

				KeyboardGUI();

				GUILayout.EndScrollView();
			} else {
				PickGUI();
			}

			DragInFileGUI();

			if (GUI.changed) {
				EditorSave();
			}

			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl(null);
				Repaint();
			}

		}




		#endregion




		#region --- GUI ---



		private void HeaderGUI () {

			LayoutH(() => {

				// Icon
				if (FileIcon) {
					GUI.DrawTexture(GUIRect(24, 24), FileIcon);
					Space(4);
				}

				// Label
				GUI.Label(GUIRect(56 + 80, 24), Util.GetNameWithExtension(VoxelFilePath), new GUIStyle(GUI.skin.label) {
					alignment = TextAnchor.MiddleLeft,
				});

				// Title
				TitleGUI(GUIRect(0, 18));

				GUIRect(80, 24);

				// Save Data Button
				bool showSaveButton = !IsSpriting;
				if (ColorfulButton(GUIRect(56, 24), "Save", !showSaveButton ? Color.clear : DataDirty ? new Color(0.6f, 1f, 0.7f, 1) : Color.white, new GUIStyle(EditorStyles.miniButtonLeft) { fontSize = 11 }) && showSaveButton) {
					SaveData();
					Repaint();
				}

				// Close Button
				if (ColorfulButton(GUIRect(24, 24), "×", new Color(1, 0.6f, 0.6f, 1), new GUIStyle(showSaveButton ? EditorStyles.miniButtonMid : EditorStyles.miniButtonLeft) { fontSize = 14 })) {
					EditorApplication.delayCall += () => { CloseTarget(); };
				}

			}, false, new GUIStyle() {
				padding = new RectOffset(0, 0, 0, 0),
				margin = new RectOffset(28, 0, 12, 0),
			});
			Space(4);
		}



		private void PickGUI () {

			if (Data) { return; }

			Space(10);

			TitleGUI(GUIRect(0, 18));

			{// Switch Button
				const int BUTTON_WIDTH = 62;
				Rect rect = new Rect(position.width - BUTTON_WIDTH, 6, 0, 18);

				rect.width += CurrentEditorMode == EditorMode.Rigging ? BUTTON_WIDTH + 20 : BUTTON_WIDTH;
				rect.x = position.width - rect.width;
				if (GUI.Button(rect, "Rigging", EditorStyles.miniButtonLeft)) {
					if (CurrentEditorMode != EditorMode.Rigging) {
						OpenWindow(EditorMode.Rigging, EditorPrefs.GetBool("V2U.EditorDockToScene", true));
					}
				}
				rect.y += rect.height + 2;

				rect.width = CurrentEditorMode == EditorMode.Sprite ? BUTTON_WIDTH + 20 : BUTTON_WIDTH;
				rect.x = position.width - rect.width;
				if (GUI.Button(rect, "Sprite", EditorStyles.miniButtonLeft)) {
					if (CurrentEditorMode != EditorMode.Sprite) {
						OpenWindow(EditorMode.Sprite, EditorPrefs.GetBool("V2U.EditorDockToScene", true));
					}
				}
				rect.y += rect.height + 2;

				rect.width = CurrentEditorMode == EditorMode.Generator ? BUTTON_WIDTH + 20 : BUTTON_WIDTH;
				rect.x = position.width - rect.width;
				if (GUI.Button(rect, "Generator", EditorStyles.miniButtonLeft)) {
					if (CurrentEditorMode != EditorMode.Generator) {
						OpenWindow(EditorMode.Generator, EditorPrefs.GetBool("V2U.EditorDockToScene", true));
					}
				}
				rect.y += rect.height + 2;
			}

			LayoutV(() => {
				if (CurrentEditorMode == EditorMode.Generator) {
					GUI.Label(GUIRect(0, 48), "Select the type of voxel model to generate.", new GUIStyle(GUI.skin.label) {
						alignment = TextAnchor.MiddleCenter,
						fontSize = 12,
					});
					Space(12);
					LayoutH(() => {
						GUIRect(0, 36);

						GUI.enabled = false;

						if (GUI.Button(GUIRect(220, 36), "Character(Coming soon...)")) {
							EditorApplication.delayCall += () => {
								SwitchGeneratorMode(GeneratorMode.Character);
								CGConfig.FixGenerationValues();
							};
						}

						GUI.enabled = true;

						Space(12);
						if (GUI.Button(GUIRect(220, 36), "Map")) {
							EditorApplication.delayCall += () => {
								SwitchGeneratorMode(GeneratorMode.Map);
								MGConfig.FixGenerationValues();
							};
						}
						GUIRect(0, 36);
					});
				} else {

					GUI.Label(GUIRect(0, 48), "Pick a vox/qb file to " + (CurrentEditorMode == EditorMode.Rigging ? "rig it" : "create sprite") + ".\nDrag vox/qb file from project view to this window or use the buttons below.", new GUIStyle(GUI.skin.label) {
						alignment = TextAnchor.MiddleCenter,
						fontSize = 12,
					});

					Space(12);

					LayoutH(() => {

						GUIRect(0, 36);

						if (GUI.Button(GUIRect(240, 36), "Pick VOX File")) {
							EditorApplication.delayCall += () => {
								PickTarget(true);
							};
						}

						Space(12);

						if (GUI.Button(GUIRect(240, 36), "Pick QB File")) {
							EditorApplication.delayCall += () => {
								PickTarget(false);
							};
						}

						GUIRect(0, 36);

					});
				}
			}, false, new GUIStyle() {
				padding = new RectOffset(120, 120, 180, 60),
				margin = new RectOffset(28, 20, 8, 4),
			});


			GUI.Label(
				GUIRect(0, 280),
				"<size=100></</size><size=280>" + CurrentEditorMode.ToString()[0].ToString() + "</size><size=100>></size>",
				new GUIStyle(GUI.skin.label) {
					richText = true,
					alignment = TextAnchor.MiddleCenter,
					normal = new GUIStyleState() {
						textColor = new Color(0.5f, 0.5f, 0.5f, 0.08f),
					},
				}
			);

		}



		private void DragInFileGUI () {
			if (IsGenerating) { return; }
			if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) {
				if (DragAndDrop.paths.Length > 0) {
					var path = DragAndDrop.paths[0];
					if (!string.IsNullOrEmpty(path)) {
						var ex = Util.GetExtension(path);
						if (ex == ".vox" || ex == ".qb") {
							if (Event.current.type == EventType.DragPerform) {
								bool set = true;
								if (Data) {
									set = Util.Dialog("Warning", "Open another voxel model?", "Open", "Cancel");
								}
								if (set) {
									SetTargetAt(path);
								}
								Repaint();
								DragAndDrop.AcceptDrag();
							} else {
								DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
							}
						}
					}
				}
			}
		}



		private void ViewGUI () {
			if (!Data) { return; }
			if (CameraRoot && Camera && Camera.targetTexture) {
				var targetTexture = Camera.targetTexture;
				int textureWidth = targetTexture.width;
				int currentViewWidth = (int)EditorGUIUtility.currentViewWidth - 2;

				// Width Check
				if (targetTexture.width != currentViewWidth) {
					Camera.targetTexture = new RenderTexture(currentViewWidth, RENDER_TEXTURE_HEIGHT, 24) { antiAliasing = 2, };
					AlphaCamera.targetTexture = new RenderTexture(currentViewWidth, RENDER_TEXTURE_HEIGHT, 24) { antiAliasing = 2, };
					textureWidth = currentViewWidth;
					RefreshCubeTransform();
					Camera.Render();
					Repaint();
				}

				Space(4);

				// View
				ViewRect = GUIRect(textureWidth, RENDER_TEXTURE_HEIGHT);
				GUI.DrawTexture(ViewRect, Camera.targetTexture);
				var oldC = GUI.color;
				GUI.color = new Color(1, 1, 1, 0.309f);
				GUI.DrawTexture(ViewRect, AlphaCamera.targetTexture);
				GUI.color = oldC;

				// Camera
				if (Event.current.type == EventType.MouseDrag) {
					// Mosue Right Drag
					if (Event.current.button == 1) {
						if (Event.current.alt) {
							// Drag Zoom
							SetCameraSize(Mathf.Clamp(
								Camera.orthographicSize * (1f + Event.current.delta.y * 0.01f),
								CameraSizeMin, CameraSizeMax
							));
							HighlightTF.gameObject.SetActive(false);
							RefreshCubeTransform();
							Repaint();
						} else {
							// Rotate
							Vector2 del = Event.current.delta * 0.4f;
							float angle = Camera.transform.rotation.eulerAngles.x + del.y;
							angle = angle > 89 && angle < 180 ? 89 : angle;
							angle = angle > 180 && angle < 271 ? 271 : angle;
							CameraRoot.rotation = Quaternion.Euler(angle, CameraRoot.rotation.eulerAngles.y + del.x, 0f);
							HighlightTF.gameObject.SetActive(false);
							RefreshCubeTransform();
							Repaint();
						}
					} else if (Event.current.button == 2 || (Event.current.button == 0 && Event.current.alt)) {
						// Move
						Vector3 del = Event.current.delta * Camera.orthographicSize * 0.005f;
						del = CameraRoot.rotation * new Vector3(-del.x, del.y, 0f);
						CameraRoot.localPosition += del;
						HighlightTF.gameObject.SetActive(false);
						Repaint();
					}
				} else if (Event.current.isScrollWheel) {
					// Wheel Zoom
					if (ViewRect.Contains(Event.current.mousePosition)) {
						SetCameraSize(Mathf.Clamp(
							Camera.orthographicSize * (1f + Event.current.delta.y * 0.04f),
							CameraSizeMin, CameraSizeMax
						));
						HighlightTF.gameObject.SetActive(false);
						Event.current.Use();
						RefreshCubeTransform();
						Repaint();
					}
				} else if (Event.current.type == EventType.MouseDown) {
					if (Event.current.button == 1 && Event.current.shift) {
						// Focus
						ViewCastHit((hit) => {
							if (hit.transform.name != "Cube") {
								CameraRoot.position = hit.point;
								Repaint();
							}
						});
					}
				}

			} else {
				EditorApplication.delayCall += () => {
					CloseTarget(true);
				};
			}
		}



		private void EditToolGUI () {

			const int LABEL_WIDTH = 64;
			const int ITEM_WIDTH = 24;
			var rect = new Rect(ViewRect.x + 12, ViewRect.y + ViewRect.height - 18 - 6, LABEL_WIDTH, 18);

			// Flip
			GUI.Label(rect, "Flip");
			rect.x += rect.width;
			rect.width = ITEM_WIDTH;
			if (GUI.Button(rect, "X", EditorStyles.miniButtonLeft)) {
				Flip(0);
			}
			rect.x += rect.width;
			if (GUI.Button(rect, "Y", EditorStyles.miniButtonMid)) {
				Flip(1);
			}
			rect.x += rect.width;
			if (GUI.Button(rect, "Z", EditorStyles.miniButtonRight)) {
				Flip(2);
			}
			rect.x = ViewRect.x + 12;
			rect.y -= rect.height + 6;

			// Rot
			rect.width = LABEL_WIDTH;
			GUI.Label(rect, "Rotate");
			rect.x += rect.width;
			rect.width = ITEM_WIDTH;
			if (GUI.Button(rect, "X", EditorStyles.miniButtonLeft)) {
				Rotate(0);
			}
			rect.x += rect.width;
			if (GUI.Button(rect, "Y", EditorStyles.miniButtonMid)) {
				Rotate(1);
			}
			rect.x += rect.width;
			if (GUI.Button(rect, "Z", EditorStyles.miniButtonRight)) {
				Rotate(2);
			}
			rect.x = ViewRect.x + 12;
			rect.y -= rect.height + 6;


		}



		private void ToolbarGUI () {
			// Toolbar
			LayoutH(() => {

				// Data Button
				var tempRect = new Rect();
				if (GUI.Button(tempRect = GUIRect(70, 18), "Debug Data", EditorStyles.toolbarButton)) {
					DataPanelOpen = !DataPanelOpen;
				}
				if (DataPanelOpen) {
					ColorBlock(tempRect);
				}


				// State Info Button
				if (GUI.Button(tempRect = GUIRect(45, 18), "State", EditorStyles.toolbarButton)) {
					ShowStateInfo.Value = !ShowStateInfo;
				}
				if (ShowStateInfo) {
					ColorBlock(tempRect);
				}


				if (IsRigging) {
					// Bone Name Button
					if (GUI.Button(tempRect = GUIRect(75, 18), "Bone Name", EditorStyles.toolbarButton)) {
						ShowBoneName.Value = !ShowBoneName;
					}
					if (ShowBoneName) {
						ColorBlock(tempRect);
					}
				}

				Space(16);

				// Tag
				if (Data.Voxels.Count > 1) {
					LayoutH(() => {
						Rect barRect = GUIRect(0, 18);
						int len = Data.Voxels.Count;
						if (IsGenerating) {
							if (GUI.Button(
									new Rect(barRect.x, barRect.y, 60, 18), "Map",
									CurrentModelIndex == 0 ? new GUIStyle(EditorStyles.toolbarButton) {
										normal = EditorStyles.toolbarButton.active,
									} : EditorStyles.toolbarButton
								)) {
								SwitchModel(0, true);
							}
							if (Data.Voxels.Count > 1 && GUI.Button(
									new Rect(barRect.x + 60, barRect.y, 60, 18), "Cave",
									CurrentModelIndex == 1 ? new GUIStyle(EditorStyles.toolbarButton) {
										normal = EditorStyles.toolbarButton.active,
									} : EditorStyles.toolbarButton
								)) {
								SwitchModel(1, true);
							}
						} else {
							float tagWidth = Mathf.Min(40f, barRect.width / len);
							for (int i = 0; i < len; i++) {
								if (GUI.Button(
									new Rect(barRect.x + i * tagWidth, barRect.y, tagWidth, 18),
									i.ToString(),
									i == CurrentModelIndex ? new GUIStyle(EditorStyles.toolbarButton) {
										normal = EditorStyles.toolbarButton.active,
									} : EditorStyles.toolbarButton
								)) {
									SwitchModel(i);
								}
							}
						}
					});
					Space(60);
				} else {
					GUIRect(0, 18);
				}

				// Help
				if (GUI.Button(GUIRect(18, 18), "?", EditorStyles.toolbarButton)) {
					Util.Dialog(
						"Help",
						"[Right drag] Rotate camera.\n" +
						"[Middle drag] or [Alt + Left drag] Move camera\n" +
						"[Mouse wheel] or [Alt + Right drag] Zoom camera\n" +
						"[Shift + right click] Focus camera\n" +
						"[Shift + S] Show/Hide state info\n" +
						"[P] Start/stop paint selecting bone\n" +
						"[ESC] Stop painting weight point\n" +
						"[V] [B] [N] Set brush type\n" +
						"[T] [R] Set attach or erase\n" +
						"[(Hold)Shift] Switch between attach and erase\n" +
						"[C] Reset Camera\n" +
						"[G] Show background box or not",
						"OK"
					);
				}

				Space(12);

			}, false, EditorStyles.toolbar);

		}



		private void RotationSliderGUI () {

			// Slider
			const int LABEL_WIDTH = 64;
			const int ITEM_WIDTH = 24;
			const int WIDTH = 158;
			const int OFFSET_X = -12;
			float left = ViewRect.x + ViewRect.width - WIDTH + 48 + OFFSET_X;

			Rect rect = new Rect(left, ViewRect.y + 60, WIDTH - 48, 18);

			if (CameraRoot) {

				// Reset Camera Button
				rect.x = ViewRect.x + ViewRect.width - 28 + OFFSET_X;
				rect.y = ViewRect.y + 6;
				rect.width = 22;
				rect.height = 22;
				if (GUI.Button(rect, "C", EditorStyles.miniButton)) {
					ResetCamera();
					Repaint();
				}
				rect.y += 24;

				// Box Button
				if (GUI.Button(rect, "G", EditorStyles.miniButton)) {
					SetBoxBackgroundActive(!ShowBackgroundBox);
					Repaint();
				}
				rect.y += 24;



				// Slider
				Vector3 oldAngel = CameraRoot.rotation.eulerAngles;
				Vector3 newAngel = oldAngel;
				rect.x = left;
				rect.width = WIDTH - 48;
				rect.height = 18;

				newAngel.x = Mathf.Repeat(GUI.HorizontalSlider(rect, Mathf.Repeat(CameraRoot.rotation.eulerAngles.x + 90f, 360f), 0f, 180f) - 90f, 360f);
				rect.y += 20;
				newAngel.y = GUI.HorizontalSlider(rect, CameraRoot.rotation.eulerAngles.y, 0f, 360f);

				if (newAngel != oldAngel) {
					if (newAngel.x != oldAngel.x) {
						newAngel.x = Mathf.Round(newAngel.x / 18f) * 18f;
					}
					if (newAngel.y != oldAngel.y) {
						newAngel.y = Mathf.Round(newAngel.y / 36f) * 36f;
					}
					CameraRoot.rotation = Quaternion.Euler(newAngel);
					RefreshCubeTransform();
					Repaint();
				}

				// Label
				rect.x -= 30;
				rect.y -= 20;
				GUI.Label(rect, newAngel.x.ToString("00"));
				rect.y += 20;
				GUI.Label(rect, newAngel.y.ToString("00"));
				rect.y += 20;


			}


			// Voxel Color Tint
			left = ViewRect.x + ViewRect.width - ITEM_WIDTH * 3 + OFFSET_X;

			rect.x = left - LABEL_WIDTH;
			rect.y += 4;
			rect.width = LABEL_WIDTH;
			GUI.Label(rect, "Tint");

			rect.width = ITEM_WIDTH;
			rect.x = left;

			// F
			var newColorF = ColorField(rect, VoxelFaceColorF);
			EditorGUI.DropShadowLabel(rect, " F", EditorStyles.boldLabel);
			VoxelTintColorChanged = VoxelFaceColorF.Value != newColorF || VoxelTintColorChanged;
			VoxelFaceColorF.Value = newColorF;
			rect.x += rect.width;

			// R
			var newColorR = ColorField(rect, VoxelFaceColorR);
			EditorGUI.DropShadowLabel(rect, " R", EditorStyles.boldLabel);
			VoxelTintColorChanged = VoxelFaceColorR.Value != newColorR || VoxelTintColorChanged;
			VoxelFaceColorR.Value = newColorR;
			rect.x += rect.width;

			// U
			var newColorU = ColorField(rect, VoxelFaceColorU);
			EditorGUI.DropShadowLabel(rect, " U", EditorStyles.boldLabel);
			VoxelTintColorChanged = VoxelFaceColorU.Value != newColorU || VoxelTintColorChanged;
			VoxelFaceColorU.Value = newColorU;




			rect.x = left;
			rect.y += rect.height + 6;

			// B
			var newColorB = ColorField(rect, VoxelFaceColorB);
			EditorGUI.DropShadowLabel(rect, " B", EditorStyles.boldLabel);
			VoxelTintColorChanged = VoxelFaceColorB.Value != newColorB || VoxelTintColorChanged;
			VoxelFaceColorB.Value = newColorB;
			rect.x += rect.width;

			// L
			var newColorL = ColorField(rect, VoxelFaceColorL);
			EditorGUI.DropShadowLabel(rect, " L", EditorStyles.boldLabel);
			VoxelTintColorChanged = VoxelFaceColorL.Value != newColorL || VoxelTintColorChanged;
			VoxelFaceColorL.Value = newColorL;
			rect.x += rect.width;

			// D
			var newColorD = ColorField(rect, VoxelFaceColorD);
			EditorGUI.DropShadowLabel(rect, " D", EditorStyles.boldLabel);
			VoxelTintColorChanged = VoxelFaceColorD.Value != newColorD || VoxelTintColorChanged;
			VoxelFaceColorD.Value = newColorD;
			rect.y += rect.height + 6;

			// Change Button
			rect.x = left;
			rect.width = ITEM_WIDTH * 3;
			if (ColorfulButton(rect, "Change", VoxelTintColorChanged ? Color.white : Color.clear)) {
				if (VoxelTintColorChanged) {
					SwitchModel(CurrentModelIndex, true);
				}
			}

		}



		private void DataGUI () {

			if (!Data || !DataPanelOpen) { return; }

			LayoutV(() => {

				// Palette
				AltLayoutF(() => {
					bool oldE = GUI.enabled;
					GUI.enabled = false;
					const int COLUMN_COUNT = 32;
					int rowCount = Mathf.CeilToInt(Data.Palette.Count / (float)COLUMN_COUNT);
					for (int i = 0; i < rowCount; i++) {
						LayoutH(() => {
							for (int j = 0; j < COLUMN_COUNT; j++) {
								int index = i * COLUMN_COUNT + j;
								if (index >= Data.Palette.Count) {
									break;
								}
#if UNITY_2018
								EditorGUI.ColorField(GUIRect(18, 18), GUIContent.none, Data.Palette[index], false, false, false);
#else
								EditorGUI.ColorField(GUIRect(18, 18), GUIContent.none, Data.Palette[index], false, false, false, null);
#endif
							}
						});
						Space(2);
					}
					GUI.enabled = oldE;
				}, string.Format("Palette [{0}]", Data.Palette.Count), ref DataPalettePanelOpen, false, new GUIStyle() {
					padding = new RectOffset(18, 0, 0, 0),
				});

				// Node
				AltLayoutF(() => {
					NodGUI(0);
				}, string.Format("Node [{0}]", Data.Transforms.Count), ref DataNodePanelOpen, false, new GUIStyle() {
					padding = new RectOffset(18, 0, 0, 0),
				});

				// Material
				AltLayoutF(() => {
					// Containt
					for (int i = 0; i < Data.Materials.Count; i++) {
						if (i % 20 == 0) {
							// Labels
							LayoutH(() => {
								GUIRect(36, 18);
								GUI.Label(GUIRect(62, 18), "Type");
								Space(12);
								GUI.Label(GUIRect(48, 18), "Weight");
								GUI.Label(GUIRect(48, 18), "Rough");
								GUI.Label(GUIRect(48, 18), "Spec");
								GUI.Label(GUIRect(48, 18), "Ior");
								GUI.Label(GUIRect(48, 18), "Att");
								GUI.Label(GUIRect(48, 18), "Flux");
								GUI.Label(GUIRect(48, 18), "Glow");
								GUI.Label(GUIRect(48, 18), "Plastic");
							});
						}
						MaterialGUI(Data.Materials[i]);
					}
				}, string.Format("Material [{0}]", Data.Materials.Count), ref DataMaterialPanelOpen, false, new GUIStyle() {
					padding = new RectOffset(18, 0, 0, 0),
				});

				// Rig
				AltLayoutF(() => {
					foreach (var r in Data.Rigs) {
						RigDataGUI(r.Key, r.Value);
					}
				}, string.Format("Rigging [{0}]", Data.Rigs.Count), ref DataRigPanelOpen, false, new GUIStyle() {
					padding = new RectOffset(18, 0, 0, 0),
				});

			}, false, new GUIStyle(GUI.skin.box) {
				padding = new RectOffset(9, 6, 4, 4),
				margin = new RectOffset(14, 20, 20, 4),
			});


		}



		private void StateInfoGUI () {
			if (!ShowStateInfo) { return; }

			var rect = ViewRect;
			rect.x += 12;
			rect.y += 12;
			rect.width = 220;
			rect.height = 18;

			if (!IsRigging) {
				// Normal && Spriting
				GUI.Label(rect, "Name\t" + Util.GetNameWithExtension(VoxelFilePath));
				rect.y += rect.height + 2;

				if (Data.Voxels.Count > 1) {
					GUI.Label(rect, "Model\t" + CurrentModelIndex + "/" + (Data.Voxels.Count - 1));
					rect.y += rect.height + 2;
				}

				GUI.Label(rect, "Size\t" + string.Format("{0}×{1}×{2}", ModelSize.x, ModelSize.y, ModelSize.z));
				rect.y += rect.height + 2;




			} else {

				// Rigging

				int boneIndex = Mathf.Max(SelectingBoneIndex, PaintingBoneIndex);
				var bone = TheBones != null && boneIndex != -1 ? TheBones[boneIndex] : null;

				// Bone Name
				GUI.Label(rect, "Name\t" + (bone ? bone.Name : "---"));
				rect.y += rect.height + 2;

				// Bone Index
				GUI.Label(rect, "Index\t" + (bone ? boneIndex.ToString() : "---"));
				rect.y += rect.height + 2;

				// Bone Position
				GUI.Label(rect, "X\t" + (bone ? bone.PositionX.ToString() : "---"));
				rect.y += rect.height + 2;
				GUI.Label(rect, "Y\t" + (bone ? bone.PositionY.ToString() : "---"));
				rect.y += rect.height + 2;
				GUI.Label(rect, "Z\t" + (bone ? bone.PositionZ.ToString() : "---"));
				rect.y += rect.height + 2;

				if (PaintingBoneIndex != -1) {

					// Cursor
					GUI.Label(rect, "Cursor\t" + (HoveringVoxelPosition != null ? HoveringVoxelPosition.ToString() : "---"));
					rect.y += rect.height + 2;

					// Bones
					string bonesLabelA = "---";
					string bonesLabelB = "---";
					var weight = TheWeights != null && HoveringVoxelPosition && HoveredVoxelPosition && Util.InRange(HoveredVoxelPosition.x, HoveredVoxelPosition.y, HoveredVoxelPosition.z, ModelSize.x, ModelSize.y, ModelSize.z) ? TheWeights[HoveredVoxelPosition.x, HoveredVoxelPosition.y, HoveredVoxelPosition.z] : null;
					if (weight != null && TheBones != null) {
						bonesLabelA = (weight.BoneIndexA != -1 ? TheBones[weight.BoneIndexA].Name : "---");
						bonesLabelB = (weight.BoneIndexB != -1 ? TheBones[weight.BoneIndexB].Name : "---");
					}
					GUI.Label(rect, "Bones A\t" + bonesLabelA);
					rect.y += rect.height + 2;
					GUI.Label(rect, "Bones B\t" + bonesLabelB);
					rect.y += rect.height + 2;


				}

			}

		}



		private void KeyboardGUI () {
			if (Event.current.isKey && Event.current.type == EventType.KeyDown && !Util.IsTypingInGUI()) {
				var key = Event.current.keyCode;
				switch (key) {
					case KeyCode.Escape:
						if (PaintingBoneIndex != -1) {
							int index = PaintingBoneIndex;
							StopPaintBone();
							SelectBone(index);
							SetWeightPointQuadDirty();
							Repaint();
						} else if (SelectingBoneIndex != -1) {
							DeselectBone();
							SetWeightPointQuadDirty();
							Repaint();
						}
						break;
					case KeyCode.N:
						if (PaintingBoneIndex != -1) {
							SetBrushType(BrushType.Rect);
							Repaint();
						}
						Repaint();
						break;
					case KeyCode.S:
						if (Event.current.shift) {
							ShowStateInfo.Value = !ShowStateInfo;
							Repaint();
						}
						break;
					case KeyCode.Delete:
						if (SelectingBoneIndex != -1) {
							DeleteBoneDialog(SelectingBoneIndex);
						}
						break;
					case KeyCode.RightArrow:
						int index0 = Mathf.Max(SelectingBoneIndex, PaintingBoneIndex);
						if (index0 != -1) {
							var bones = TheBones;
							if (bones != null) {
								SetBoneOpen(bones[index0], true);
								Repaint();
							}
						}
						break;
					case KeyCode.LeftArrow:
						int index1 = Mathf.Max(SelectingBoneIndex, PaintingBoneIndex);
						if (index1 != -1) {
							var bones = TheBones;
							if (bones != null) {
								SetBoneOpen(bones[index1], false);
								Repaint();
							}
						}
						break;
					case KeyCode.V:
						if (PaintingBoneIndex != -1) {
							SetBrushType(BrushType.Voxel);
							Repaint();
						}
						break;
					case KeyCode.B:
						if (PaintingBoneIndex != -1) {
							SetBrushType(BrushType.Box);
							Repaint();
						}
						break;
					case KeyCode.P:
						if (PaintingBoneIndex == -1 && SelectingBoneIndex != -1) {
							StartPaintBone(SelectingBoneIndex);
							Repaint();
						} else if (PaintingBoneIndex != -1) {
							int oldPaintingBone = PaintingBoneIndex;
							StopPaintBone();
							SelectBone(oldPaintingBone);
							Repaint();
						}
						break;
					case KeyCode.C:
						ResetCamera();
						Repaint();
						break;
					case KeyCode.G:
						SetBoxBackgroundActive(!ShowBackgroundBox);
						Repaint();
						break;
					case KeyCode.T:
						AttachBrush = true;
						Repaint();
						break;
					case KeyCode.R:
						AttachBrush = false;
						Repaint();
						break;
				}
			}
		}




		#region --- Data GUI ---



		private void NodGUI (int id) {
			if (Data) {
				if (Data.Transforms.ContainsKey(id)) {
					TransformNodGUI(id, Data.Transforms[id]);
				} else if (Data.Groups.ContainsKey(id)) {
					GroupNodGUI(id, Data.Groups[id]);
				} else if (Data.Shapes.ContainsKey(id)) {
					ShapeNodGUI(id, Data.Shapes[id]);
				}
			}
		}



		private void TransformNodGUI (int id, VoxelData.TransformData tfData) {

			const int ITEM_HEIGHT = 18;

			LayoutV(() => {

				Space(2);
				LayoutH(() => {

					// Name
					LayoutH(() => {
						GUI.Label(GUIRect(60, ITEM_HEIGHT), "Name");
						EditorGUI.LabelField(GUIRect(42, ITEM_HEIGHT), tfData.Name);
					}, true);
					Space(4);

					// LayerID
					LayoutH(() => {
						GUI.Label(GUIRect(60, ITEM_HEIGHT), "Layer");
						EditorGUI.LabelField(GUIRect(42, ITEM_HEIGHT), tfData.LayerID.ToString());
					}, true);
					Space(4);

					// Reserved
					LayoutH(() => {
						GUI.Label(GUIRect(60, ITEM_HEIGHT), "Reserved");
						EditorGUI.LabelField(GUIRect(42, ITEM_HEIGHT), tfData.Reserved.ToString());
					}, true);
					Space(4);

					// Hidden
					LayoutH(() => {
						GUI.Label(GUIRect(60, ITEM_HEIGHT), "Hidden");
						EditorGUI.LabelField(GUIRect(42, ITEM_HEIGHT), tfData.Hidden.ToString());
					}, true);

				});

				Space(2);

				// Frames
				for (int i = 0; i < tfData.Frames.Length; i++) {

					LayoutH(() => {

						var frame = tfData.Frames[i];

						// Pos
						LayoutH(() => {
							GUI.Label(GUIRect(60, ITEM_HEIGHT), "Position");
							EditorGUI.LabelField(GUIRect(100, ITEM_HEIGHT), frame.Position.ToString());
						}, true);
						Space(2);

						// Rot
						LayoutH(() => {
							GUI.Label(GUIRect(60, ITEM_HEIGHT), "Rotation");
							EditorGUI.LabelField(GUIRect(100, ITEM_HEIGHT), frame.Rotation.ToString());
						}, true);
						Space(2);

						// Scale
						LayoutH(() => {
							GUI.Label(GUIRect(40, ITEM_HEIGHT), "Scale");
							EditorGUI.LabelField(GUIRect(100, ITEM_HEIGHT), frame.Scale.ToString());
						}, true);

					});
				}

				Space(8);

				// Child Node
				NodGUI(tfData.ChildID);

			}, true);

		}



		private void GroupNodGUI (int id, VoxelData.GroupData gpData) {

			bool open = false;
			if (!NodeOpen.ContainsKey(id)) {
				NodeOpen.Add(id, false);
			}
			open = NodeOpen[id];
			AltLayoutF(() => {
				for (int i = 0; i < gpData.ChildNodeId.Length; i++) {
					NodGUI(gpData.ChildNodeId[i]);
					Space(6);
				}
			}, "Child", ref open, false, new GUIStyle() {
				margin = new RectOffset(0, 0, 0, 0),
				padding = new RectOffset(24, 0, 0, 0),
			});

			NodeOpen[id] = open;
		}



		private void ShapeNodGUI (int id, VoxelData.ShapeData spData) {
			LayoutH(() => {
				string labelText = "Model ID [ ";
				for (int i = 0; i < spData.ModelData.Length; i++) {
					labelText += spData.ModelData[i].Key.ToString() + (i < spData.ModelData.Length - 1 ? ", " : "");
				}
				labelText += " ]";
				GUI.Label(GUIRect(0, 18), labelText);
			});
		}



		private void MaterialGUI (VoxelData.MaterialData matData) {

			const int LABEL_WIDTH = 36;
			const int SPACE_WIDTH = 12;

			LayoutH(() => {

				GUI.Label(GUIRect(LABEL_WIDTH, 18), matData.Index.ToString());

				// Type
				EditorGUI.LabelField(GUIRect(62, 18), matData.Type.ToString());
				Space(SPACE_WIDTH);

				// Weight
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Weight.ToString());
				Space(SPACE_WIDTH);

				// Rough
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Rough.ToString());
				Space(SPACE_WIDTH);

				// Spec
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Spec.ToString());
				Space(SPACE_WIDTH);

				// Ior
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Ior.ToString());
				Space(SPACE_WIDTH);

				// Att
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Att.ToString());
				Space(SPACE_WIDTH);

				// Flux
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Flux.ToString());
				Space(SPACE_WIDTH);

				// Glow
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Glow.ToString());
				Space(SPACE_WIDTH);

				// Plastic
				EditorGUI.LabelField(GUIRect(LABEL_WIDTH, 18), matData.Plastic.ToString());
				Space(SPACE_WIDTH);

			});
			Space(2);

		}



		private void RigDataGUI (int id, VoxelData.RigData rigData) {
			LayoutH(() => {
				GUI.Label(GUIRect(36, 18), id.ToString());
				Space();
				GUI.Label(GUIRect(80, 18), "Bones x " + rigData.Bones.Count);
				Space();
				GUI.Label(GUIRect(80, 18), "Weights x " + rigData.Weights.Count);
			});
			Space(2);
		}




		#endregion




		#endregion




		#region --- LGC ---




		private void EditorLoad () {
			ColorfulTitle = EditorPrefs.GetBool("V2U.ColorfulTitle", true);
			ShowBackgroundBox.Load();
			ShowBoneName.Load();
			BrushTypeIndex.Load();
			ShowStateInfo.Load();

			VoxelFaceColorU.Load();
			VoxelFaceColorD.Load();
			VoxelFaceColorL.Load();
			VoxelFaceColorR.Load();
			VoxelFaceColorF.Load();
			VoxelFaceColorB.Load();

			Load_Generation();
			Load_Sprite();
		}



		private void EditorSave () {
			ShowBackgroundBox.TrySave();
			ShowBoneName.TrySave();
			BrushTypeIndex.TrySave();
			ShowStateInfo.TrySave();
			VoxelFaceColorU.TrySave();
			VoxelFaceColorD.TrySave();
			VoxelFaceColorL.TrySave();
			VoxelFaceColorR.TrySave();
			VoxelFaceColorF.TrySave();
			VoxelFaceColorB.TrySave();

			Save_Sprite();
		}



		private void TitleGUI (Rect rect) {
			GUIStyle style = new GUIStyle() {
				alignment = TextAnchor.LowerCenter,
				fontSize = 12,
				fontStyle = FontStyle.Bold
			};
			style.normal.textColor = Color.white;
			style.richText = true;

			GUIStyle shadowStyle = new GUIStyle(style) {
				richText = false
			};

			string title = CurrentEditorMode == EditorMode.Rigging ? RIGGING_TITLE : CurrentEditorMode == EditorMode.Sprite ? SPRITE_TITLE : GENERATOR_TITLE;
			string titleRich = CurrentEditorMode == EditorMode.Rigging ? RIGGING_TITLE_RICH : CurrentEditorMode == EditorMode.Sprite ? SPRITE_TITLE_RICH : GENERATOR_TITLE_RICH;
			EditorGUI.DropShadowLabel(rect, title, shadowStyle);
			GUI.Label(rect, ColorfulTitle ? titleRich : title, style);
		}



		// Target
		private void PickTarget (bool pickVox) {
			if (Data) { return; }
			SetTargetAt(Util.FixedRelativePath(EditorUtility.OpenFilePanel("Pick Voxel Target", "Assets", pickVox ? "vox" : "qb")));
			Repaint();
		}



		private void SetTargetAt (string path) {
			Data = null;
			FileIcon = null;
			TheWeights = null;
			VoxelFilePath = "";
			CurrentModelIndex = 0;
			DataPanelOpen = false;
			NodeOpen.Clear();
			BoneOpen.Clear();
			RemoveRoot();
			if (!string.IsNullOrEmpty(path)) {
				try {
					string ex = Util.GetExtension(path);
					Util.ProgressBar("", "Importing...", 0f);
					Data = VoxelFile.GetVoxelData(Util.FileToByte(path), ex == ".vox");
					Util.ProgressBar("", "Importing...", 0.333f);
					VoxelFilePath = path;
					var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
					if (obj) {
						FileIcon = AssetPreview.GetMiniThumbnail(obj);
					}
					// Node Open
					if (Data) {
						foreach (var gp in Data.Groups) {
							if (!NodeOpen.ContainsKey(gp.Key)) {
								NodeOpen.Add(gp.Key, false);
							}
						}
					}
					// Faces
					SwitchModel(CurrentModelIndex);
					DataDirty = false;
					Repaint();
				} catch (System.Exception ex) {
					Util.ClearProgressBar();
					Debug.LogWarning("[Voxel Editor] Fail to open voxel file.\n" + ex.Message);
					Util.Dialog("", "Fail to open vox file.", "OK");
				}
			}
		}



		private bool CloseTarget (bool forceClose = false) {
			if (forceClose || Util.Dialog("", "Close the current editor?", "Close", "Don't Close")) {
				Data = null;
				VoxelFilePath = "";
				NodeOpen.Clear();
				RemoveRoot();
				DataDirty = false;
				BoneOpen.Clear();
				Repaint();
				return true;
			} else {
				return false;
			}
		}



		private void ResetCamera () {
			CameraRoot.localPosition = Vector3.zero;
			CameraRoot.localRotation = Quaternion.Euler(33.5f, 33.5f, 0f);
			SetCameraSize((CameraSizeMin + CameraSizeMax) * 0.5f);
			RefreshCubeTransform();
		}



		private void SetDataDirty () {
			DataDirty = true;
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}



		private void SaveData () {
			try {
				if (IsGenerating) {
					if (!Data) { Data = VoxelData.CreateNewData(); }
					if (string.IsNullOrEmpty(VoxelFilePath) || !Util.FileExists(VoxelFilePath)) {
						VoxelFilePath = Util.FixedRelativePath(EditorUtility.SaveFilePanel("Create Vox", "Assets", "New Generated Data", "vox"));
					}
					if (string.IsNullOrEmpty(VoxelFilePath)) { return; }
					if (!FileIcon && !string.IsNullOrEmpty(VoxelFilePath)) {
						var obj = AssetDatabase.LoadAssetAtPath<Object>(VoxelFilePath);
						if (obj) { FileIcon = AssetPreview.GetMiniThumbnail(obj); }
					}
				} else if (!Data || string.IsNullOrEmpty(VoxelFilePath)) {
					return;
				}
				if (IsRigging) {
					MakeWeightIntoData();
					FreshAllBonesParentIndexByParent();
				}
				var bytes = VoxelFile.GetVoxelByte(
					VoxelData.GetSplitedData(Data),
					Util.GetExtension(VoxelFilePath) == ".vox",
					(progress) => { Util.ProgressBar("Saving...", string.Format("Saving vox file {0}%", (int)(progress * 100)), progress); }
				);
				if (bytes == null) { return; }
				Util.ByteToFile(bytes, VoxelFilePath);
				DataDirty = false;
				AssetDatabase.Refresh();
			} catch (System.Exception ex) {
				Debug.LogError(ex.Message);
			}
			Util.ClearProgressBar();
		}



		private void Rerender () {
			if (Camera) {
				Camera.Render();
			}
			if (AlphaCamera) {
				AlphaCamera.Render();
			}
			Repaint();
		}



		private void SwitchModel (int index, bool keepCameraAngle = false) {

			// Save Weight Data
			MakeWeightIntoData();

			// Switch Model
			index = Mathf.Clamp(index, 0, Data.Voxels.Count - 1);

			bool hasCamera = CameraRoot && Camera;
			var oldPos = hasCamera ? CameraRoot.localPosition : Vector3.zero;
			var oldRot = hasCamera ? CameraRoot.localRotation : Quaternion.identity;
			var oldSize = hasCamera ? Camera.orthographicSize : 1f;

			RemoveRoot();
			CurrentModelIndex = index;

			bool success = SpawnRoot();
			if (!success) {
				CloseTarget(true);
				return;
			}

			if (keepCameraAngle && hasCamera) {
				CameraRoot.localPosition = oldPos;
				CameraRoot.localRotation = oldRot;
				SetCameraSize(oldSize);
			}

			ModelSize = Data.GetModelSize(CurrentModelIndex);
			VoxelTintColorChanged = false;

			// Edit Mode
			ClearRigCacheIndexs();

			switch (CurrentEditorMode) {

				case EditorMode.Rigging:
					// Rig
					if (RigAvailable) {
						if (RigEditingRoot) { RigEditingRoot.gameObject.SetActive(true); }
						if (SpriteEditingRoot) { SpriteEditingRoot.gameObject.SetActive(false); }
						if (GeneratorEditingRoot) { GeneratorEditingRoot.gameObject.SetActive(false); }
						if (!Data.Rigs.ContainsKey(CurrentModelIndex)) {
							Data.Rigs.Add(CurrentModelIndex, new VoxelData.RigData() {
								Bones = new List<VoxelData.RigData.Bone>(),
								Weights = new List<VoxelData.RigData.Weight>(),
							});
							SetDataDirty();
						}
						SetWeightPointQuadDirty();
						SpawnBoneTransforms();
						SetAllVoxelCollidersEnable(false);

						StopPaintBone(true);
						DeselectBone(true);
					}
					break;

				case EditorMode.Sprite:
					if (RigEditingRoot) { RigEditingRoot.gameObject.SetActive(false); }
					if (SpriteEditingRoot) { SpriteEditingRoot.gameObject.SetActive(true); }
					if (GeneratorEditingRoot) { GeneratorEditingRoot.gameObject.SetActive(false); }
					SetAllVoxelCollidersEnable(false);
					break;

				case EditorMode.Generator:
					if (RigEditingRoot) { RigEditingRoot.gameObject.SetActive(false); }
					if (SpriteEditingRoot) { SpriteEditingRoot.gameObject.SetActive(false); }
					if (GeneratorEditingRoot) { GeneratorEditingRoot.gameObject.SetActive(true); }

					SetAllVoxelCollidersEnable(false);

					break;
			}

			// Fix Weight Data
			GetWeightsFromData();

			BoneOpen.Clear();
			EditorApplication.delayCall += () => { if (Camera) Camera.Render(); };
			EditorApplication.delayCall += () => { if (AlphaCamera) AlphaCamera.Render(); };
			EditorApplication.delayCall += Repaint;
		}





		// Misc
		private void Flip (int axis) {

			var voxels = Data.Voxels[CurrentModelIndex];

			int sizeX = (int)(axis == 0 ? ModelSize.x * 0.5f : ModelSize.x);
			int sizeY = (int)(axis == 1 ? ModelSize.y * 0.5f : ModelSize.y);
			int sizeZ = (int)(axis == 2 ? ModelSize.z * 0.5f : ModelSize.z);

			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					for (int z = 0; z < sizeZ; z++) {
						int i = axis == 0 ? ModelSize.x - x - 1 : x;
						int j = axis == 1 ? ModelSize.y - y - 1 : y;
						int k = axis == 2 ? ModelSize.z - z - 1 : z;
						int temp = voxels[x, y, z];
						voxels[x, y, z] = voxels[i, j, k];
						voxels[i, j, k] = temp;
					}
				}
			}

			if (Data.Rigs.ContainsKey(CurrentModelIndex)) {
				var rig = Data.Rigs[CurrentModelIndex];
				if (rig != null) {
					// Bones
					var bones = TheBones;
					if (bones != null) {
						for (int i = 0; i < bones.Count; i++) {
							var bone = bones[i];
							if (bone) {
								if (bone.Parent) {
									bone.PositionX = axis == 0 ? -bone.PositionX : bone.PositionX;
									bone.PositionY = axis == 1 ? -bone.PositionY : bone.PositionY;
									bone.PositionZ = axis == 2 ? -bone.PositionZ : bone.PositionZ;
								} else {
									bone.PositionX = axis == 0 ? ModelSize.x - bone.PositionX - 1 : bone.PositionX;
									bone.PositionY = axis == 1 ? ModelSize.y - bone.PositionY - 1 : bone.PositionY;
									bone.PositionZ = axis == 2 ? ModelSize.z - bone.PositionZ - 1 : bone.PositionZ;
								}
								bones[i] = bone;
							}
						}
					}
					// Weights
					var weights = rig.Weights;
					if (weights != null) {
						for (int i = 0; i < weights.Count; i++) {
							var weight = weights[i];
							if (weight != null) {
								weight.X = axis == 0 ? ModelSize.x - weight.X - 1 : weight.X;
								weight.Y = axis == 1 ? ModelSize.y - weight.Y - 1 : weight.Y;
								weight.Z = axis == 2 ? ModelSize.z - weight.Z - 1 : weight.Z;
								weights[i] = weight;
							}
						}
					}

				}
			}

			SetDataDirty();
			GetWeightsFromData();
			SwitchModel(CurrentModelIndex);

			Repaint();
		}



		private void Rotate (int axis) {

			var voxels = Data.Voxels[CurrentModelIndex];
			var newVoxels = new int[
				axis == 0 ? ModelSize.x : axis == 1 ? ModelSize.z : ModelSize.y,
				axis == 0 ? ModelSize.z : axis == 1 ? ModelSize.y : ModelSize.x,
				axis == 0 ? ModelSize.y : axis == 1 ? ModelSize.x : ModelSize.z
			];

			for (int x = 0; x < ModelSize.x; x++) {
				for (int y = 0; y < ModelSize.y; y++) {
					for (int z = 0; z < ModelSize.z; z++) {
						int i = x, j = y, k = z;
						if (axis == 0) {
							j = ModelSize.z - z - 1;
							k = y;
						} else if (axis == 1) {
							i = ModelSize.z - z - 1;
							k = x;
						} else {
							i = ModelSize.y - y - 1;
							j = x;
						}
						newVoxels[i, j, k] = voxels[x, y, z];
					}
				}
			}

			Data.Voxels[CurrentModelIndex] = newVoxels;


			if (Data.Rigs.ContainsKey(CurrentModelIndex)) {
				var rig = Data.Rigs[CurrentModelIndex];
				if (rig != null) {
					// Bones
					var bones = TheBones;
					if (bones != null) {
						for (int i = 0; i < bones.Count; i++) {
							var bone = bones[i];
							if (bone) {
								int x = bone.PositionX;
								int y = bone.PositionY;
								int z = bone.PositionZ;
								if (bone.Parent) {
									if (axis == 0) {
										bone.PositionY = -z;
										bone.PositionZ = y;
									} else if (axis == 1) {
										bone.PositionX = -z;
										bone.PositionZ = x;
									} else {
										bone.PositionX = -y;
										bone.PositionY = x;
									}
								} else {
									if (axis == 0) {
										bone.PositionY = ModelSize.z - z - 1;
										bone.PositionZ = y;
									} else if (axis == 1) {
										bone.PositionX = ModelSize.z - z - 1;
										bone.PositionZ = x;
									} else {
										bone.PositionX = ModelSize.y - y - 1;
										bone.PositionY = x;
									}
								}
								bones[i] = bone;
							}
						}
					}
					// Weights
					var weights = rig.Weights;
					if (weights != null) {
						for (int i = 0; i < weights.Count; i++) {
							var weight = weights[i];
							if (weight != null) {
								int x = weight.X;
								int y = weight.Y;
								int z = weight.Z;
								if (axis == 0) {
									weight.Y = ModelSize.z - z - 1;
									weight.Z = y;
								} else if (axis == 1) {
									weight.X = ModelSize.z - z - 1;
									weight.Z = x;
								} else {
									weight.X = ModelSize.y - y - 1;
									weight.Y = x;
								}
								weights[i] = weight;
							}
						}
					}

				}
			}

			ModelSize.x = newVoxels.GetLength(0);
			ModelSize.y = newVoxels.GetLength(1);
			ModelSize.z = newVoxels.GetLength(2);

			SetDataDirty();
			GetWeightsFromData();
			SwitchModel(CurrentModelIndex);
			Repaint();
		}




		#endregion




	}





}