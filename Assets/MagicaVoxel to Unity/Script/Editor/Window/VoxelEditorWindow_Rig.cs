namespace VoxeltoUnity {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Moenen.Saving;


	// Rigging Part
	public partial class VoxelEditorWindow {




		#region --- SUB ---



		private enum BrushType {
			Voxel = 0,
			Box = 1,
			Rect = 2,
		}




		#endregion




		#region --- VAR ---



		// Short
		private bool IsRigging {
			get {
				return RigEditingRoot && RigEditingRoot.gameObject.activeSelf;
			}
		}

		private List<VoxelData.RigData.Bone> TheBones {
			get {
				if (!RigEditingRoot) {
					return null;
				}
				var rigData = Data.Rigs.ContainsKey(CurrentModelIndex) ? Data.Rigs[CurrentModelIndex] : null;
				if (rigData == null) {
					return null;
				}
				return rigData.Bones;
			}
		}

		private BrushType CurrentBrushType {
			get {
				return (BrushType)BrushTypeIndex.Value;
			}
			set {
				BrushTypeIndex.Value = (int)value;
			}
		}



		// Data
		private Dictionary<VoxelData.RigData.Bone, bool> BoneOpen = new Dictionary<VoxelData.RigData.Bone, bool>();
		private Dictionary<VoxelData.RigData.Bone, Transform> BoneMap = new Dictionary<VoxelData.RigData.Bone, Transform>();
		private VoxelData.RigData.Weight[,,] TheWeights = null;
		private Vector3 MovingBonePosition;
		private Int3 DragBeginPosition = null;
		private Vector2? DragBeginGUIPos = null;
		private int PaintingBoneIndex = -1;
		private int SelectingBoneIndex = -1;
		private int RenamingBoneIndex = -1;
		private int PrevHightlightBoneAxis = -1;
		private int PrevHightlightBone = -1;
		private int DraggingAxisIndex = -1;
		private bool WeightPointQuadDirty = true;
		private bool AttachBrush = true;


		// Saving
		private EditorSavingBool ShowBoneName = new EditorSavingBool("VEditor.ShowBoneName", false);
		private EditorSavingInt BrushTypeIndex = new EditorSavingInt("VEditor.BrushTypeIndex", 1);


		#endregion




		#region --- GUI ---



		private void RigEditingGUI () {

			if (IsRigging && Data.Rigs.ContainsKey(CurrentModelIndex)) {

				var bones = TheBones;
				if (bones == null) { return; }

				// Bone Panel
				LayoutV(() => {

					Space(2);
					BonePanelGUI(null);
					Space(8);

					bool oldE = GUI.enabled;

					LayoutH(() => {

						GUI.enabled = PaintingBoneIndex == -1;

						// Add Bone
						int maxIndex = Mathf.Max(SelectingBoneIndex, PaintingBoneIndex);
						VoxelData.RigData.Bone pBone = maxIndex == -1 ? null : bones[maxIndex];
						if (GUI.Button(GUIRect(80, 22), pBone ? "+ Child Bone" : "+ New Bone", EditorStyles.miniButtonLeft)) {
							AddNewChildBoneFor(pBone);
							Repaint();
						}

						// Add Human Bone
						if (GUI.Button(GUIRect(100, 22), "+ Human Bones", EditorStyles.miniButtonMid)) {
							int id = Util.DialogComplex("Add Human Bones", "Which type of bones do you want?", "Major Bones", "Full Bones", "Cancel");
							if (id != 2) {
								bones.AddRange(GetHumanBones(Data.GetModelSize(CurrentModelIndex), id == 1));
								SpawnBoneTransforms();
								SetDataDirty();
								Repaint();
							}
						}

						// Paint
						GUI.enabled = maxIndex != -1;
						if (GUI.Button(GUIRect(80, 22), PaintingBoneIndex == -1 ? "Paint" : "Stop Paint", EditorStyles.miniButtonRight)) {
							if (PaintingBoneIndex == -1) {
								StartPaintBone(SelectingBoneIndex);
							} else {
								int oldIndex = PaintingBoneIndex;
								StopPaintBone();
								SelectBone(oldIndex);
							}
							Repaint();
						}



					});
					GUI.enabled = oldE;
					Space(4);

					// Brush
					if (PaintingBoneIndex != -1) {

						Space(4);

						// Brush Type
						LayoutH(() => {
							GUI.enabled = oldE;
							GUI.Label(GUIRect(60, 18), "Brush");
							Space(2);

							GUI.enabled = CurrentBrushType != BrushType.Voxel;
							if (GUI.Button(GUIRect(47.5f, 18), BrushType.Voxel.ToString(), EditorStyles.miniButtonLeft)) {
								SetBrushType(BrushType.Voxel);
								Repaint();
							}

							GUI.enabled = CurrentBrushType != BrushType.Box;
							if (GUI.Button(GUIRect(47.5f, 18), BrushType.Box.ToString(), EditorStyles.miniButtonMid)) {
								SetBrushType(BrushType.Box);
								Repaint();
							}

							GUI.enabled = CurrentBrushType != BrushType.Rect;
							if (GUI.Button(GUIRect(47.5f, 18), BrushType.Rect.ToString(), EditorStyles.miniButtonRight)) {
								SetBrushType(BrushType.Rect);
								Repaint();
							}


							Space(6);
							GUI.enabled = !AttachBrush;
							if (GUI.Button(GUIRect(47.5f, 18), "Attach", EditorStyles.miniButtonLeft)) {
								AttachBrush = true;
								Repaint();
							}

							GUI.enabled = AttachBrush;
							if (GUI.Button(GUIRect(47.5f, 18), "Erase", EditorStyles.miniButtonRight)) {
								AttachBrush = false;
								Repaint();
							}


						});
						Space(4);



					}
					GUI.enabled = oldE;
					Space(4);

				}, false, new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(9, 6, 4, 4),
					margin = new RectOffset(14, 20, 20, 4),
				});


				Space(2);

				// Hint
				if (bones.Count == 0) {
					var oldC = GUI.color;
					GUI.color = Color.green;
					GUI.Label(GUIRect(180, 18), "      ▲  Click Here to Add Bones");
					GUI.color = oldC;
				}

				// Top Edit Bone Button
				if (SelectingBoneIndex != -1 || PaintingBoneIndex != -1) {

					var richButton = new GUIStyle(GUI.skin.button) {
						richText = true,
					};

					if (GUI.Button(new Rect(
							ViewRect.x + ViewRect.width * 0.5f - 110f * 0.5f, ViewRect.y + 4, 132f, 28f
						), PaintingBoneIndex == -1 ? "  Paint Weight  <b><size=8>(P)</size></b> " : "   Stop Painting  <b><size=8>(ESC)</size></b>  ", richButton
					)) {
						if (PaintingBoneIndex == -1) {
							StartPaintBone(SelectingBoneIndex);
						} else {
							int index = PaintingBoneIndex;
							StopPaintBone();
							SelectBone(index);
						}
					}
				}

			}
		}



		private void RigPanelGUI () {

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

			// Rig
			if (GUI.Button(rect, "   Create Rig Prefab", buttonStyle)) {
				CreateRigPrefab(false);
				Repaint();
			}
			GUI.Label(rect, "  <color=#33ccff>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;
			if (GUI.Button(rect, "   Create Avatar Prefab", buttonStyle)) {
				CreateRigPrefab(true);
				Repaint();
			}
			GUI.Label(rect, "  <color=#33ccff>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;
			if (GUI.Button(rect, "   Export Rig Json", buttonStyle)) {
				ExportRigData();
				Repaint();
			}
			GUI.Label(rect, "  <color=#cccccc>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;
			if (GUI.Button(rect, "   Import Rig Json", buttonStyle)) {
				ImportRigData();
				Repaint();
			}
			GUI.Label(rect, "  <color=#cccccc>●</color>", dotStyle);
			rect.y -= HEIGHT + GAP;
		}



		private void BonePanelGUI (VoxelData.RigData.Bone parent) {

			var bones = TheBones;
			if (bones == null) { return; }

			for (int i = 0; i < bones.Count; i++) {

				const int HEIGHT = 18;
				var bone = bones[i];
				bool selecting = SelectingBoneIndex == i;
				bool painting = PaintingBoneIndex == i;
				bool renaming = RenamingBoneIndex == i;
				Rect gapRect = new Rect();

				// Check Parent Index
				if (bone.Parent != parent) { continue; }

				LayoutH(() => {

					gapRect = GUIRect(18, HEIGHT);
					Rect buttonRect = GUIRect(200, HEIGHT);

					// Right Click
					if (Event.current.button == 1 && Event.current.type == EventType.MouseDown) {
						if (buttonRect.Contains(Event.current.mousePosition)) {
							ShowBoneMenu("— " + bone.Name + " —", i);
						}
					}

					// Name
					if (selecting || painting) {

						Rect rect;

						if (renaming) {
							// Rename Field
							var newName = GUI.TextField(buttonRect, bone.Name);
							if (newName != bone.Name) {
								bone.Name = newName;
								SetDataDirty();
							}
						} else {
							// Selectable Button
							if (GUI.Button(buttonRect, bone.Name, GUI.skin.label) && Event.current.button == 0) {
								RenamingBoneIndex = i;
								Repaint();
							}
						}

						// Selecting Tint
						rect = buttonRect;
						rect.width = rect.x + rect.width - 14;
						rect.x = 14;
						var oldC = GUI.color;
						GUI.color = PaintingBoneIndex == -1 ? new Color(0.5f, 1f, 1f, 0.08f) : new Color(1f, 0.5f, 1f, 0.08f);
						GUI.DrawTexture(rect, Texture2D.whiteTexture);
						GUI.color = oldC;

					} else {
						// Selectable Button
						if (GUI.Button(buttonRect, bone.Name, GUI.skin.label) && Event.current.button == 0) {
							SelectBone(i);
							Repaint();
						}
					}




				});

				// Children
				bool boneOpen = GetBoneOpen(bone);
				bool hasChild = HasChildBone(bone);

				if (hasChild) {
					LayoutV(() => {
						boneOpen = GUI.Toggle(
							new Rect(gapRect.x, gapRect.y, 18, 18),
							boneOpen, "",
							GUI.skin.GetStyle("foldout")
						);
						if (boneOpen) {
							BonePanelGUI(bone);
						}
					}, false, new GUIStyle() { padding = new RectOffset(18, 0, 0, 0) });
				}

				SetBoneOpen(bone, boneOpen);

			}



		}



		private void BoneMouseGUI () {

			if (!IsRigging || !Data.Rigs.ContainsKey(CurrentModelIndex)) { return; }

			// Not Select Bone
			if (PaintingBoneIndex == -1) {

				var type = Event.current.type;
				if ((type == EventType.MouseDown && Event.current.button == 0) ||
					type == EventType.MouseMove ||
					type == EventType.MouseUp
				) {

					ViewCastHitAll((hits, len) => {

						RaycastHit? theHit = null;

						for (int i = 0; i < len; i++) {
							var hName = hits[i].transform.name;
							if (hName == "Cone") {
								theHit = hits[i];
							} else if (hits[i].transform.parent == MoveBoneAsixRootTF) {
								theHit = hits[i];
								break;
							}
						}

						// Click On Bone to Start Move
						if (theHit.HasValue) {
							if (type == EventType.MouseDown) {
								// Mouse Down
								if (theHit.Value.transform.name == "Cone") {
									int boneIndex = theHit.Value.transform.parent.GetSiblingIndex();
									SelectBone(boneIndex);
								} else {
									int highlightingAxis = theHit.Value.transform.GetSiblingIndex();
									DraggingAxisIndex = highlightingAxis;
								}
							} else {
								// Mouse Move
								int highlightboneIndex = -1;
								int highlightingAxis = -1;
								if (theHit.Value.transform.name == "Cone") {
									highlightboneIndex = theHit.Value.transform.parent.GetSiblingIndex();
								} else {
									highlightingAxis = theHit.Value.transform.GetSiblingIndex();
								}

								if (HighlightBone(highlightboneIndex)) { Repaint(); }
								if (HighlightMovingBoneAxis(highlightingAxis)) { Repaint(); }
							}
						} else {
							if (HighlightMovingBoneAxis(-1)) { Repaint(); }
							if (HighlightBone(-1)) { Repaint(); }
							if (SelectingBoneIndex != -1 && type == EventType.MouseDown && Event.current.button == 0) {
								// Cancel
								DeselectBone();
							}
						}

					});
				}

				// Moving Bone
				if (SelectingBoneIndex != -1) {

					var bones = TheBones;
					if (bones == null) { return; }

					var bone = bones[SelectingBoneIndex];
					var bTF = BoneMap[bone];

					var eventType = Event.current.type;
					if (eventType == EventType.MouseUp || eventType == EventType.MouseLeaveWindow || eventType == EventType.MouseEnterWindow) {
						DraggingAxisIndex = -1;
						HighlightMovingBoneAxis(-1);
						Repaint();
					}

					// Dragging
					if (DraggingAxisIndex != -1) {

						// Keep Highlight
						if (HighlightMovingBoneAxis(DraggingAxisIndex)) {
							Repaint();
						}

						// Move Bone
						if (eventType == EventType.MouseDrag) {

							var oldScreenPos = GetGUIScreenPosition(Event.current.mousePosition - Event.current.delta);
							var screenPos = GetGUIScreenPosition(Event.current.mousePosition);
							Vector3 worldDelta = Camera.ScreenToWorldPoint(screenPos) - Camera.ScreenToWorldPoint(oldScreenPos);
							worldDelta.x = DraggingAxisIndex == 0 ? worldDelta.x : 0f;
							worldDelta.y = DraggingAxisIndex == 1 ? worldDelta.y : 0f;
							worldDelta.z = DraggingAxisIndex == 2 ? worldDelta.z : 0f;

							MovingBonePosition += worldDelta;
							bone.PositionX = Mathf.RoundToInt(MovingBonePosition.x);
							bone.PositionY = Mathf.RoundToInt(MovingBonePosition.y);
							bone.PositionZ = Mathf.RoundToInt(MovingBonePosition.z);

							FreshAllBonesTransform();

							SetDataDirty();

						}

						Repaint();

					}

					// Fresh Transform
					MoveBoneAsixRootTF.localPosition = bTF.localPosition;
					MoveBoneAsixRootTF.localScale = Vector3.one * Mathf.Clamp(
						Camera.orthographicSize * 0.18f, 0.05f, 3f
					);


				}

			}
		}



		private void SceneBoneNameGUI () {
			if (!IsRigging || !ShowBoneName) { return; }
			var bones = TheBones;
			if (bones == null) { return; }

			var oldC = GUI.color;
			GUI.color = Color.black;
			if (PaintingBoneIndex == -1) {
				for (int i = 0; i < bones.Count; i++) {
					var pos = BoneMap[bones[i]].position;

					var screenPos = Camera.WorldToScreenPoint(pos);
					var guiPos = GetGUIPosition(screenPos);

					if (ViewRect.Contains(new Vector2(guiPos.x, guiPos.y))) {
						GUI.Label(new Rect(guiPos.x, guiPos.y - 9, 120, 18), "  " + bones[i].Name);
					}
				}
			} else {
				var pos = BoneMap[bones[PaintingBoneIndex]].position;

				var screenPos = Camera.WorldToScreenPoint(pos);
				var guiPos = GetGUIPosition(screenPos);

				if (ViewRect.Contains(new Vector2(guiPos.x, guiPos.y))) {
					GUI.Label(new Rect(guiPos.x, guiPos.y - 9, 120, 18), "  " + bones[PaintingBoneIndex].Name);
				}
			}
			GUI.color = oldC;
		}



		private void WeightPointGUI () {


			// Painting
			if (IsRigging && PaintingBoneIndex != -1) {
				var type = Event.current.type;
				switch (type) {
					case EventType.MouseDown:
					case EventType.MouseDrag:

						if (Event.current.button != 0) { break; }

						// Begin
						if (type == EventType.MouseDown) {
							if (CurrentBrushType == BrushType.Rect) {
								DragBeginGUIPos = Event.current.mousePosition;
							}
							if (HoveringVoxelPosition) {
								DragBeginPosition = HoveringVoxelPosition;
							} else {
								break;
							}
						}

						// Dragging
						if ((DragBeginPosition && HoveredVoxelPosition) || DragBeginGUIPos.HasValue) {
							if (CurrentBrushType == BrushType.Voxel) {
								if (Util.InRange(HoveredVoxelPosition.x, HoveredVoxelPosition.y, HoveredVoxelPosition.z, ModelSize.x, ModelSize.y, ModelSize.z)) {
									var weight = TheWeights[HoveredVoxelPosition.x, HoveredVoxelPosition.y, HoveredVoxelPosition.z];
									weight = PaintWeight(weight);
									TheWeights[HoveredVoxelPosition.x, HoveredVoxelPosition.y, HoveredVoxelPosition.z] = weight;
									SetDataDirty();
								}
							}
							SetWeightPointQuadDirty();
							Repaint();
						}


						break;
					case EventType.MouseUp:

						if (Event.current.button != 0) { break; }

						if (DragBeginPosition || DragBeginGUIPos.HasValue) {
							if (CurrentBrushType == BrushType.Box || CurrentBrushType == BrushType.Rect) {
								// Paint To TheWeights
								ForAllWeightPoints((x, y, z, weight) => {
									if (CheckVoxelInSelection(x, y, z)) {
										weight = PaintWeight(weight);
									}
									return weight;
								});
								SetDataDirty();
							}
							DragBeginPosition = null;
							DragBeginGUIPos = null;
							SetWeightPointQuadDirty();
							Repaint();
						}

						break;
					case EventType.MouseEnterWindow:
					case EventType.MouseLeaveWindow:
						DragBeginPosition = null;
						DragBeginGUIPos = null;
						Repaint();
						break;
				}




			}


			// All Point Dirty
			if (WeightPointQuadDirty) {
				WeightPointQuadDirty = false;
				if (IsRigging) {

					int index = Mathf.Max(PaintingBoneIndex, SelectingBoneIndex);
					bool painting = PaintingBoneIndex != -1;

					if (index != -1) {

						// Painting || Selecting
						bool attach = AttachBrush != Event.current.shift;
						SetAllQuadsColor((x, y, z, color) => {
							if (CheckVoxelInSelection(x, y, z)) {
								// Selection
								color = GetWeightColor(attach ? 1f : 0f);
							} else {
								// Data
								var w = TheWeights[x, y, z];
								float weight = w != null ? w.GetWeight(index) : 0f;
								if (weight > 0.01f) {
									color = GetWeightColor(weight);
								}
							}

							color.a = painting ? 1f : 0f;
							return color;
						});



					} else {
						// Not Painting
						SetAllQuadsColorToNormal(true);
					}
				} else {
					// Not Rigging
					SetAllQuadsColorToNormal(false);
				}
				Rerender();
			}


			// Rect
			if (DragBeginGUIPos.HasValue && CurrentBrushType == BrushType.Rect) {
				var oldC = GUI.color;
				GUI.color = new Color(1, 1, 1, 0.1f);
				GUI.DrawTexture(new Rect(
					Vector2.Min(DragBeginGUIPos.Value, Event.current.mousePosition),
					Util.Vector2Abs(DragBeginGUIPos.Value - Event.current.mousePosition)
				), Texture2D.whiteTexture);
				GUI.color = oldC;
				Repaint();
			}

		}



		#endregion




		#region --- TSK ---


		private void CreateRigPrefab (bool withAvatar) {
			if (!Data || string.IsNullOrEmpty(VoxelFilePath)) { return; }
			string path = Util.FixPath(EditorUtility.SaveFilePanel("Select Export Path", "Assets", Util.GetNameWithoutExtension(VoxelFilePath), "prefab"));
			if (!string.IsNullOrEmpty(path)) {
				path = Util.FixedRelativePath(path);
				if (!string.IsNullOrEmpty(path)) {
					try {

						// Resources
						float scale = EditorPrefs.GetFloat("V2U.ModelScale", 0.1f);
						var lightmap = (Core_Voxel.LightMapSupportType)EditorPrefs.GetInt("V2U.LightMapSupportTypeIndex", 0);
						var shader = Shader.Find(EditorPrefs.GetString("V2U.ShaderPath", "Mobile/Diffuse"));
						bool mergeF = EditorPrefs.GetBool("V2U.OptimizeFront", true);
						bool mergeB = EditorPrefs.GetBool("V2U.OptimizeBack", true);
						bool mergeU = EditorPrefs.GetBool("V2U.OptimizeUp", true);
						bool mergeD = EditorPrefs.GetBool("V2U.OptimizeDown", true);
						bool mergeL = EditorPrefs.GetBool("V2U.OptimizeLeft", true);
						bool mergeR = EditorPrefs.GetBool("V2U.OptimizeRight", true);

						Core_Voxel.SetMergeInDirection(mergeF, mergeB, mergeU, mergeD, mergeL, mergeR);

						var result = Core_Voxel.CreateModel(Data, scale, lightmap, true, Vector3.one * 0.5f);
						if (!result || result.VoxelModels == null || result.VoxelModels.Length == 0) { return; }

						var vModel = result.VoxelModels[0];
						if (vModel.Meshs == null || vModel.Meshs.Length <= CurrentModelIndex) { return; }
						if (result.VoxelModels.Length > 1) {
							result.VoxelModels = new Core_Voxel.Result.VoxelModel[1] { vModel };
						}

						var uMesh = vModel.Meshs[CurrentModelIndex];
						if (vModel.Meshs.Length > 1) {
							vModel.Meshs = new UnlimitiedMesh[1] { uMesh };
						}
						var mesh = uMesh != null && uMesh.Count > 0 ? uMesh[0] : null;
						if (!mesh) { return; }

						var texture = vModel.Textures != null && vModel.Textures.Length > 0 ? vModel.Textures[0] : null;
						if (!texture) { return; }

						// Reset Transform Layout
						vModel.RootNode = new Core_Voxel.Result.VoxelNode() {
							Model = uMesh,
							Texture = texture,
							Active = true,
							Children = null,
							Name = "",
							Position = new Core_Voxel.Int3(0, 0, 0),
							Rotation = Quaternion.identity,
							Scale = new Core_Voxel.Int3(1, 1, 1),
							Size = new Core_Voxel.Int3(
								(int)vModel.ModelSize[0].x,
								(int)vModel.ModelSize[0].y,
								(int)vModel.ModelSize[0].z
							),
						};

						// File
						result.ExportRoot = Util.GetParentPath(path);
						result.ExportSubRoot = "";
						result.FileName = Util.GetNameWithoutExtension(path);
						result.Extension = ".prefab";
						result.IsRigged = true;
						result.WithAvatar = withAvatar;

						Core_File.CreateFileForResult(new List<Core_Voxel.Result>() { result }, shader, scale, Vector3.one * 0.5f);

					} catch (System.Exception ex) {
						Debug.LogError(ex.Message);
					}
				} else {
					Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
				}
			}
		}



		private void ImportRigData () {
			string path = Util.FixPath(EditorUtility.OpenFilePanel("Open Rig Data", "Assets", "json"));
			if (!string.IsNullOrEmpty(path)) {
				path = Util.FixedRelativePath(path);
				if (!string.IsNullOrEmpty(path)) {
					if (Util.Dialog("Warning", "Overlap rig data for current sub model?", "OK", "Cancel")) {
						try {
							string json = Util.Read(path);
							if (!string.IsNullOrEmpty(json)) {
								var rig = JsonUtility.FromJson<VoxelData.RigData>(json);
								if (rig != null && ((rig.Bones != null && rig.Bones.Count > 0) || (rig.Weights != null && rig.Weights.Count > 0))) {
									if (Data.Rigs.ContainsKey(CurrentModelIndex)) {
										Data.Rigs[CurrentModelIndex] = rig;
										FreshAllBonesParentByParentIndex();
										SetDataDirty();
										StopPaintBone();
										DeselectBone();
										ClearRigCacheIndexs();
										GetWeightsFromData();
										SpawnBoneTransforms();
									}
								} else {
									Debug.LogWarning("Rig data is empty, cancel import.");
								}
							}
						} catch (System.Exception ex) {
							Debug.LogError(ex.Message);
						}
					}
				} else {
					Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
				}
			}
		}



		private void ExportRigData () {
			string path = Util.FixPath(EditorUtility.SaveFilePanel("Select Export Path", "Assets", Util.GetNameWithoutExtension(VoxelFilePath) + "_rigData", "json"));
			if (!string.IsNullOrEmpty(path)) {
				path = Util.FixedRelativePath(path);
				if (!string.IsNullOrEmpty(path)) {
					if (Data.Rigs.ContainsKey(CurrentModelIndex)) {
						try {
							MakeWeightIntoData();
							var rig = Data.Rigs[CurrentModelIndex];
							FreshAllBonesParentIndexByParent();
							rig.Bones = TheBones;
							string json = JsonUtility.ToJson(rig, false);
							Util.Write(json, path);
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
						} catch (System.Exception ex) {
							Debug.LogError(ex.Message);
						}
					}
				} else {
					Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
				}
			}
		}




		#endregion




		#region --- LGC ---



		// Bone System
		private void StartPaintBone (int index) {
			DeselectBone();
			PaintingBoneIndex = index;
			SetAllVoxelCollidersEnable(index != -1);
			HighLightBoneAt(index);
			SetWeightPointQuadDirty();
		}



		private void StopPaintBone (bool forceStop = false) {
			if (forceStop || PaintingBoneIndex != -1) {
				StartPaintBone(-1);
			}
		}



		private void SelectBone (int index) {
			if (PaintingBoneIndex != -1) {
				index = -1;
			}
			if (index != -1) {
				StopPaintBone();
			}
			SelectingBoneIndex = index;
			RenamingBoneIndex = -1;
			MoveBoneAsixRootTF.gameObject.SetActive(index != -1);
			if (index != -1) {
				var bones = TheBones;
				if (bones != null) {
					var bone = bones[index];
					MovingBonePosition = new Vector3(bone.PositionX, bone.PositionY, bone.PositionZ);
				}
			}
			SetWeightPointQuadDirty();
		}



		private void DeselectBone (bool forceDeselect = false) {
			if (forceDeselect || SelectingBoneIndex != -1) {
				SelectBone(-1);
			}
		}



		private void DeleteBoneDialog (int index) {
			if (index != -1 && Util.Dialog("Warning", string.Format("Delete the bone and all it's child bones?"), "Delete", "Cancel")) {
				var bones = TheBones;
				if (bones == null) { return; }
				RemoveBonesWithParent(ref bones, bones[index]);
				bones.RemoveAt(index);
				DeselectBone();
				SpawnBoneTransforms();
				FixAllBoneIndexInWeightPoints(index, -1, -1);
				SetDataDirty();
				Repaint();
			}
		}



		private void AddNewChildBoneFor (VoxelData.RigData.Bone parentBone) {
			var bones = TheBones;
			if (bones == null) { return; }
			SetBoneOpen(parentBone, true);
			bones.Add(GetNewBone("Bone " + bones.Count.ToString(), parentBone, 0, parentBone ? 1 : 0, 0, Vector3.one));
			SpawnBoneTransforms();
			SetDataDirty();
		}



		private void ClearRigCacheIndexs () {
			PaintingBoneIndex = -1;
			SelectingBoneIndex = -1;
			PrevHightlightBoneAxis = -1;
			PrevHightlightBone = -1;
			DraggingAxisIndex = -1;
			RenamingBoneIndex = -1;
		}



		private void SetWeightPointQuadDirty () {
			WeightPointQuadDirty = true;
		}




		// Bone Transform
		private void SpawnBoneTransforms () {

			var bones = TheBones;
			if (bones == null) { return; }

			if (!BoneRoot) {
				BoneRoot = new GameObject("Bone Root").transform;
				BoneRoot.SetParent(RigEditingRoot);
				BoneRoot.localPosition = Vector3.zero;
				BoneRoot.localRotation = Quaternion.identity;
				BoneRoot.localScale = Vector3.one;
			}

			// Delete Transforms
			int len = BoneRoot.childCount;
			for (int i = 0; i < len; i++) {
				DestroyImmediate(BoneRoot.GetChild(0).gameObject, false);
			}
			BoneMap.Clear();

			// Refresh Bone Child Count
			RefreshAllBonesChildCount(bones);

			// Create Bone Transforms
			for (int i = 0; i < bones.Count; i++) {

				var boneTF = new GameObject("Bone").transform;
				boneTF.SetParent(BoneRoot);
				boneTF.SetAsLastSibling();
				boneTF.gameObject.layer = LAYER_ID;
				boneTF.localRotation = Quaternion.identity;
				boneTF.localScale = Vector3.one;

				var boneMat = new Material(QUAD_SHADER);
				boneMat.SetColor(COLOR_SHADER_ID, new Color(0.8f, 0.8f, 0.8f, 0.5f));

				int childCount = Mathf.Max(1, bones[i].ChildCount);

				for (int j = 0; j < childCount; j++) {
					var coneTF = new GameObject("Cone", typeof(MeshRenderer), typeof(MeshFilter), typeof(CapsuleCollider)).transform;
					coneTF.gameObject.layer = LAYER_ID;
					coneTF.SetParent(boneTF);
					coneTF.GetComponent<MeshRenderer>().material = boneMat;
					coneTF.GetComponent<MeshFilter>().mesh = ConeMesh;
					var col = coneTF.GetComponent<CapsuleCollider>();
					col.radius = 1f;
					col.height = 1f;
					col.center = Vector3.up * 0.5f;
					coneTF.gameObject.SetActive(false);
				}

				// Add
				BoneMap.Add(bones[i], boneTF);
			}

			// Refresh Bones Position
			FreshAllBonesTransform();

			// Refresh Bone Color
			HighLightBoneAt(PaintingBoneIndex);

			// Fresh All Bones Active
			ForAllTransformsIn(BoneRoot, (boneTF) => {
				// By doing this, the colliders will in the right position. God knows why.
				bool oldActive = boneTF.gameObject.activeSelf;
				boneTF.gameObject.SetActive(!oldActive);
				boneTF.gameObject.SetActive(oldActive);
			});


			SetWeightPointQuadDirty();

		}



		private void FreshAllBonesTransform () {
			var bones = TheBones;
			if (bones != null) {

				// Pos
				for (int i = 0; i < bones.Count; i++) {
					FreshBonePosition(bones[i], false);
					ForAllTransformsIn(BoneMap[bones[i]], (coneTF) => {
						coneTF.gameObject.SetActive(false);
					});
				}

				// Reset Rot Scale
				for (int i = 0; i < bones.Count; i++) {
					var boneTF = BoneMap[bones[i]];
					if (bones[i].Parent) {
						if (bones[i].ChildCount == 0) {
							var bonePTF = BoneMap[bones[i].Parent];
							var bonePos = boneTF.localPosition;
							var bonePPos = bonePTF.localPosition;
							ForAllTransformsIn(boneTF, (coneTF) => {
								coneTF.gameObject.SetActive(true);
								coneTF.localRotation = bonePos != bonePPos ?
									Quaternion.LookRotation(bonePos - bonePPos) * Quaternion.Euler(90, 0, 0) :
									Quaternion.identity;
								coneTF.localScale = new Vector3(0.309f, 0.409f, 0.309f);
							});
						}
					} else {
						if (bones[i].ChildCount == 0) {
							boneTF.GetChild(0).gameObject.SetActive(true);
						}
						ForAllTransformsIn(boneTF, (coneTF) => {
							coneTF.localRotation = Quaternion.identity;
							coneTF.localScale = new Vector3(0.309f, 0.409f, 0.309f);
						});
					}
				}

				// Scale Rot
				for (int i = 0; i < bones.Count; i++) {
					var boneTF = BoneMap[bones[i]];
					if (bones[i].Parent) {
						var bonePTF = BoneMap[bones[i].Parent];
						var bonePos = boneTF.localPosition;
						var bonePPos = bonePTF.localPosition;
						float disY = Vector3.Distance(boneTF.position, bonePTF.position);
						ForAllTransformsIn(bonePTF, (coneTF) => {
							if (coneTF.gameObject.activeSelf) { return false; }
							coneTF.gameObject.SetActive(true);
							coneTF.localRotation = bonePos != bonePPos ?
							   Quaternion.LookRotation(bonePos - bonePPos) * Quaternion.Euler(90, 0, 0) :
							   Quaternion.identity;
							coneTF.localScale = new Vector3(0.618f * 0.618f, disY, 0.618f * 0.618f);
							return true;
						});
					}
				}

			}
		}



		private void FreshBonePosition (VoxelData.RigData.Bone sourceBone, bool freshChildren = true) {
			var bone = sourceBone;
			var tf = BoneMap[bone];
			int safeIndex = 32;
			var pos = Vector3.zero;
			while (bone && safeIndex >= 0) {
				var p = new Vector3(bone.PositionX, bone.PositionY, bone.PositionZ);
				pos = p + pos;
				bone = bone.Parent;
				safeIndex--;
			}
			tf.localPosition = pos;

			// Children
			if (freshChildren) {
				var bones = TheBones;
				if (bones != null) {
					for (int i = 0; i < bones.Count; i++) {
						var b = bones[i];
						if (b.Parent == bone) {
							FreshBonePosition(b);
						}
					}
				}
			}

		}



		private void RefreshAllBonesChildCount (List<VoxelData.RigData.Bone> bones) {
			for (int i = 0; i < bones.Count; i++) {
				bones[i].ChildCount = 0;
			}
			for (int i = 0; i < bones.Count; i++) {
				var bone = bones[i];
				if (bone.Parent) {
					bone.Parent.ChildCount++;
				}
			}
		}



		private void HighLightBoneAt (int index = -1) {
			var bones = TheBones;
			if (bones == null) { return; }
			for (int i = 0; i < bones.Count; i++) {
				var bone = bones[i];
				if (bone && BoneMap.ContainsKey(bone)) {
					if (BoneMap[bone].gameObject) {
						BoneMap[bone].gameObject.SetActive(index == -1 || i == index);
					}
				}
			}
		}




		// Weight
		private void SetBrushType (BrushType type) {
			CurrentBrushType = type;
		}



		private void GetWeightsFromData () {

			if (!IsRigging) { return; }

			TheWeights = new VoxelData.RigData.Weight[ModelSize.x, ModelSize.y, ModelSize.z];

			if (!Data || !Data.Rigs.ContainsKey(CurrentModelIndex)) { return; }
			var rig = Data.Rigs[CurrentModelIndex];

			var weightList = Data.Rigs[CurrentModelIndex].Weights;

			int test = 0;

			if (weightList != null) {
				for (int i = 0; i < weightList.Count; i++) {
					int x = weightList[i].X;
					int y = weightList[i].Y;
					int z = weightList[i].Z;
					if (Util.InRange(x, y, z, ModelSize.x, ModelSize.y, ModelSize.z)) {
						TheWeights[x, y, z] = weightList[i];
						test++;
					}
				}
			}

		}



		private void MakeWeightIntoData () {

			if (!IsRigging || TheWeights == null || !Data || !Data.Rigs.ContainsKey(CurrentModelIndex) || CurrentModelIndex > Data.Voxels.Count) { return; }

			// Delete Empty Voxel Weight Data	
			int sizeX = TheWeights.GetLength(0);
			int sizeY = TheWeights.GetLength(1);
			int sizeZ = TheWeights.GetLength(2);
			ForAllWeightPoints((x, y, z, weight) => {
				if (weight != null) {
					bool exposed = false;
					for (int d = 0; d < 6; d++) {
						if (Data.IsExposed(CurrentModelIndex, x, y, z, sizeX, sizeY, sizeZ, (Direction)d)) {
							exposed = true;
							break;
						}
					}
					if (!exposed) {
						weight = null;
					}
				}
				return weight;
			});


			// Weight to Data
			var rig = Data.Rigs[CurrentModelIndex];

			if (rig.Weights != null) {
				rig.Weights.Clear();
			} else {
				rig.Weights = new List<VoxelData.RigData.Weight>();
			}

			ForAllWeightPoints((x, y, z, weight) => {
				if (weight != null) {
					weight.X = x;
					weight.Y = y;
					weight.Z = z;
					rig.Weights.Add(weight);
				}
				return weight;
			});

			Data.Rigs[CurrentModelIndex] = rig;

		}



		private bool CheckVoxelInSelection (int x, int y, int z) {
			switch (CurrentBrushType) {
				case BrushType.Voxel:
					return DragBeginPosition && HoveredVoxelPosition && x == HoveredVoxelPosition.x && y == HoveredVoxelPosition.y && z == HoveredVoxelPosition.z;
				case BrushType.Box:
					if (!DragBeginPosition || !HoveredVoxelPosition) { return false; }
					var min = Int3.Min(DragBeginPosition, HoveredVoxelPosition);
					var max = Int3.Max(DragBeginPosition, HoveredVoxelPosition);
					return x <= max.x && x >= min.x && y <= max.y && y >= min.y && z <= max.z && z >= min.z;
				case BrushType.Rect:
					return DragBeginGUIPos.HasValue ? new Rect(
						Vector2.Min(DragBeginGUIPos.Value, Event.current.mousePosition),
						Util.Vector2Abs(DragBeginGUIPos.Value - Event.current.mousePosition)
					).Contains(GetGUIPosition(Camera.WorldToScreenPoint(new Vector3(x, y, z) + ContainerTF.position))) : false;
			}
			return false;
		}



		private VoxelData.RigData.Weight PaintWeight (VoxelData.RigData.Weight weight) {
			int index = AttachBrush == Event.current.shift ? -1 : PaintingBoneIndex;
			if (index != -1) {
				if (weight == null) {
					weight = new VoxelData.RigData.Weight();
				}
				weight.SetWeight(PaintingBoneIndex);
			} else if (weight != null) {
				if (weight.BoneIndexA == PaintingBoneIndex) {
					weight.BoneIndexA = -1;
				}
				if (weight.BoneIndexB == PaintingBoneIndex) {
					weight.BoneIndexB = -1;
				}
				if (weight.BoneIndexA == -1 && weight.BoneIndexB == -1) {
					weight = null;
				}
			}
			return weight;
		}



		#endregion




		#region --- UTL ---



		// Bones
		private List<VoxelData.RigData.Bone> GetAllBonesWithParent (List<VoxelData.RigData.Bone> bones, VoxelData.RigData.Bone parent) {
			var resultList = new List<VoxelData.RigData.Bone>();
			for (int i = 0; i < bones.Count; i++) {
				var bone = bones[i];
				if (bone != bone.Parent && bone.Parent == parent) {
					resultList.Add(bone);
					resultList.AddRange(GetAllBonesWithParent(bones, bone));
				}
			}
			return resultList;
		}



		private void RemoveBonesWithParent (ref List<VoxelData.RigData.Bone> bones, VoxelData.RigData.Bone parent) {
			var list = GetAllBonesWithParent(bones, parent);
			for (int i = 0; i < list.Count; i++) {
				FixAllBoneIndexInWeightPoints(bones.IndexOf(list[i]), -1, -1);
				bones.Remove(list[i]);
			}
		}



		private bool GetBoneOpen (VoxelData.RigData.Bone bone) {
			if (!bone) { return false; }
			if (BoneOpen.ContainsKey(bone)) {
				return BoneOpen[bone];
			} else {
				bool defaultOpen = bone.Parent == null;
				BoneOpen.Add(bone, defaultOpen);
				return defaultOpen;
			}
		}



		private void SetBoneOpen (VoxelData.RigData.Bone bone, bool open) {
			if (!bone) { return; }
			if (BoneOpen.ContainsKey(bone)) {
				BoneOpen[bone] = open;
			} else {
				BoneOpen.Add(bone, open);
			}
		}



		private bool HasChildBone (VoxelData.RigData.Bone bone) {
			if (Data.Rigs.ContainsKey(CurrentModelIndex)) {
				var bones = Data.Rigs[CurrentModelIndex].Bones;
				if (bones != null) {
					for (int i = 0; i < bones.Count; i++) {
						if (bones[i].Parent == bone) {
							return true;
						}
					}
				}
			}
			return false;
		}



		private List<VoxelData.RigData.Bone> GetHumanBones (Vector3 size, bool fullSize) {


			var bones = new List<VoxelData.RigData.Bone>();


			// Body
			var hip = GetNewBone("Hips", null, 0.5f - 1f / size.x, 0.5f - 1f / size.y, 0.5f - 1f / size.z, size);
			var spine = GetNewBone("Spine", hip, 0, 0.05f, 0, size);
			var chest = GetNewBone("Chest", spine, 0, 0.05f, 0, size);
			var _upperChest = GetNewBone("UpperChest", chest, 0, 0.05f, 0, size);
			var neck = GetNewBone("Neck", fullSize ? _upperChest : chest, 0, 0.15f, 0, size);
			var head = GetNewBone("Head", neck, 0, 0.05f, 0, size);


			// Head
			var _eyeL = GetNewBone("LeftEye", head, -0.1f, 0.1f, 0, size);
			var _eyeR = GetNewBone("RightEye", head, 0.1f, 0.1f, 0, size);
			var _jaw = GetNewBone("Jaw", head, 0, 0.05f, 0, size);


			// Arm
			var _leftShoulder = GetNewBone("LeftShoulder", _upperChest, -0.05f, 0, 0, size);
			var _rightShoulder = GetNewBone("RightShoulder", _upperChest, 0.05f, 0, 0, size);

			var armUL = GetNewBone("LeftUpperArm", fullSize ? _leftShoulder : chest, -0.1f, 0.05f, 0, size);
			var armUR = GetNewBone("RightUpperArm", fullSize ? _rightShoulder : chest, 0.1f, 0.05f, 0, size);

			var armDL = GetNewBone("LeftLowerArm", armUL, -0.2f, 0, 0, size);
			var armDR = GetNewBone("RightLowerArm", armUR, 0.2f, 0, 0, size);

			var handL = GetNewBone("LeftHand", armDL, -0.2f, 0, 0, size);
			var handR = GetNewBone("RightHand", armDR, 0.2f, 0, 0, size);


			// Leg
			var legUL = GetNewBone("LeftUpperLeg", hip, -0.05f, 0, 0, size);
			var legUR = GetNewBone("RightUpperLeg", hip, 0.05f, 0, 0, size);

			var legDL = GetNewBone("LeftLowerLeg", legUL, 0, -0.25f, 0, size);
			var legDR = GetNewBone("RightLowerLeg", legUR, 0, -0.25f, 0, size);

			var footL = GetNewBone("LeftFoot", legDL, 0, -0.25f, 0, size);
			var footR = GetNewBone("RightFoot", legDR, 0, -0.25f, 0, size);

			var _toeL = GetNewBone("LeftToes", footL, 0, 0, -0.05f, size);
			var _toeR = GetNewBone("RightToes", footR, 0, 0, -0.05f, size);



			// Hand   
			// Proximal > Intermediate > Distal
			// Thumb Index Middle Ring Little
			var _thumbProximalL = GetNewBone("Left Thumb Proximal", handL, -0.05f, 0, 0, size);
			var _thumbProximalR = GetNewBone("Right Thumb Proximal", handR, 0.05f, 0, 0, size);
			var _thumbIntermediateL = GetNewBone("Left Thumb Intermediate", _thumbProximalL, -0.05f, 0, 0, size);
			var _thumbIntermediateR = GetNewBone("Right Thumb Intermediate", _thumbProximalR, 0.05f, 0, 0, size);
			var _thumbDistalL = GetNewBone("Left Thumb Distal", _thumbIntermediateL, -0.05f, 0, 0, size);
			var _thumbDistalR = GetNewBone("Right Thumb Distal", _thumbIntermediateR, 0.05f, 0, 0, size);

			var _indexProximalL = GetNewBone("Left Index Proximal", handL, -0.05f, 0, 0, size);
			var _indexProximalR = GetNewBone("Right Index Proximal", handR, 0.05f, 0, 0, size);
			var _indexIntermediateL = GetNewBone("Left Index Intermediate", _indexProximalL, -0.05f, 0, 0, size);
			var _indexIntermediateR = GetNewBone("Right Index Intermediate", _indexProximalR, 0.05f, 0, 0, size);
			var _indexDistalL = GetNewBone("Left Index Distal", _indexIntermediateL, -0.05f, 0, 0, size);
			var _indexDistalR = GetNewBone("Right Index Distal", _indexIntermediateR, 0.05f, 0, 0, size);

			var _middleProximalL = GetNewBone("Left Middle Proximal", handL, -0.05f, 0, 0, size);
			var _middleProximalR = GetNewBone("Right Middle Proximal", handR, 0.05f, 0, 0, size);
			var _middleIntermediateL = GetNewBone("Left Middle Intermediate", _middleProximalL, -0.05f, 0, 0, size);
			var _middleIntermediateR = GetNewBone("Right Middle Intermediate", _middleProximalR, 0.05f, 0, 0, size);
			var _middleDistalL = GetNewBone("Left Middle Distal", _middleIntermediateL, -0.05f, 0, 0, size);
			var _middleDistalR = GetNewBone("Right Middle Distal", _middleIntermediateR, -0.05f, 0, 0, size);

			var _ringProximalL = GetNewBone("Left Ring Proximal", handL, -0.05f, 0, 0, size);
			var _ringProximalR = GetNewBone("Right Ring Proximal", handR, 0.05f, 0, 0, size);
			var _ringIntermediateL = GetNewBone("Left Ring Intermediate", _ringProximalL, -0.05f, 0, 0, size);
			var _ringIntermediateR = GetNewBone("Right Ring Intermediate", _ringProximalR, 0.05f, 0, 0, size);
			var _ringDistalL = GetNewBone("Left Ring Distal", _ringIntermediateL, -0.05f, 0, 0, size);
			var _ringDistalR = GetNewBone("Right Ring Distal", _ringIntermediateR, -0.05f, 0, 0, size);

			var _littleProximalL = GetNewBone("Left Little Proximal", handL, -0.05f, 0, 0, size);
			var _littleProximalR = GetNewBone("Right Little Proximal", handR, 0.05f, 0, 0, size);
			var _littleIntermediateL = GetNewBone("Left Little Intermediate", _littleProximalL, -0.05f, 0, 0, size);
			var _littleIntermediateR = GetNewBone("Right Little Intermediate", _littleProximalR, 0.05f, 0, 0, size);
			var _littleDistalL = GetNewBone("Left Little Distal", _littleIntermediateL, -0.05f, 0, 0, size);
			var _littleDistalR = GetNewBone("Right Little Distal", _littleIntermediateR, -0.05f, 0, 0, size);


			bones.Add(hip);
			bones.Add(spine);
			bones.Add(chest);
			bones.Add(neck);
			bones.Add(head);
			bones.Add(legUL);
			bones.Add(legUR);
			bones.Add(legDL);
			bones.Add(legDR);
			bones.Add(footL);
			bones.Add(footR);
			bones.Add(armUL);
			bones.Add(armUR);
			bones.Add(armDL);
			bones.Add(armDR);
			bones.Add(handL);
			bones.Add(handR);

			if (fullSize) {

				bones.Add(_upperChest);
				bones.Add(_eyeL);
				bones.Add(_eyeR);
				bones.Add(_jaw);
				bones.Add(_leftShoulder);
				bones.Add(_rightShoulder);
				bones.Add(_toeL);
				bones.Add(_toeR);


				bones.Add(_thumbProximalL);
				bones.Add(_thumbProximalR);
				bones.Add(_thumbIntermediateL);
				bones.Add(_thumbIntermediateR);
				bones.Add(_thumbDistalL);
				bones.Add(_thumbDistalR);

				bones.Add(_indexProximalL);
				bones.Add(_indexProximalR);
				bones.Add(_indexIntermediateL);
				bones.Add(_indexIntermediateR);
				bones.Add(_indexDistalL);
				bones.Add(_indexDistalR);

				bones.Add(_middleProximalL);
				bones.Add(_middleProximalR);
				bones.Add(_middleIntermediateL);
				bones.Add(_middleIntermediateR);
				bones.Add(_middleDistalL);
				bones.Add(_middleDistalR);

				bones.Add(_ringProximalL);
				bones.Add(_ringProximalR);
				bones.Add(_ringIntermediateL);
				bones.Add(_ringIntermediateR);
				bones.Add(_ringDistalL);
				bones.Add(_ringDistalR);

				bones.Add(_littleProximalL);
				bones.Add(_littleProximalR);
				bones.Add(_littleIntermediateL);
				bones.Add(_littleIntermediateR);
				bones.Add(_littleDistalL);
				bones.Add(_littleDistalR);





			}

			return bones;
		}



		private VoxelData.RigData.Bone GetNewBone (string name, VoxelData.RigData.Bone parent, float x = 0, float y = 0, float z = 0, Vector3 size = default(Vector3)) {
			return new VoxelData.RigData.Bone() {
				Name = name,
				Parent = parent,
				PositionX = x > 0 ? Mathf.CeilToInt(x * size.x) : Mathf.FloorToInt(x * size.x),
				PositionY = y > 0 ? Mathf.CeilToInt(y * size.y) : Mathf.FloorToInt(y * size.y),
				PositionZ = z > 0 ? Mathf.CeilToInt(z * size.z) : Mathf.FloorToInt(z * size.z),
			};
		}



		private bool HighlightMovingBoneAxis (int index) {
			if (PrevHightlightBoneAxis != index) {
				var mrs = MoveBoneAsixRootTF.GetComponentsInChildren<MeshRenderer>(true);
				for (int i = 0; i < mrs.Length; i++) {
					int axisIndex = mrs[i].transform.parent.GetSiblingIndex();
					var color = mrs[i].sharedMaterial.GetColor(COLOR_SHADER_ID);
					color.a = index == axisIndex ? 1f : 0.3f;
					mrs[i].sharedMaterial.SetColor(COLOR_SHADER_ID, color);
				}
				PrevHightlightBoneAxis = index;
				return true;
			}
			return false;
		}



		private bool HighlightBone (int index) {
			if (index != PrevHightlightBone) {
				int len = BoneRoot.childCount;
				if (PrevHightlightBone >= 0 && PrevHightlightBone < len) {
					var prevTF = BoneRoot.GetChild(PrevHightlightBone);
					var mat = prevTF.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
					mat.SetColor(COLOR_SHADER_ID, new Color(0.8f, 0.8f, 0.8f, 0.5f));
				}
				if (index >= 0 && index < len) {
					var tf = BoneRoot.GetChild(index);
					var mat = tf.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
					mat.SetColor(COLOR_SHADER_ID, new Color(0.8f, 0.8f, 0.8f, 1f));
				}

				PrevHightlightBone = index;
				return true;
			}
			return false;
		}



		private void ShowBoneMenu (string title, int index) {

			if (PaintingBoneIndex != -1) { return; }

			var menu = new GenericMenu();

			if (!string.IsNullOrEmpty(title)) {

				menu.AddDisabledItem(new GUIContent(title));

			}

			menu.AddItem(new GUIContent("Paint Weight"), false, () => {
				StartPaintBone(index);
				Repaint();
			});

			menu.AddItem(new GUIContent("Rename"), false, () => {
				SelectBone(index);
				RenamingBoneIndex = index;
				Repaint();
			});

			menu.AddItem(new GUIContent("Add Child"), false, () => {
				if (index != -1) {
					var bones = TheBones;
					if (bones != null) {
						AddNewChildBoneFor(bones[index]);
						Repaint();
					}
				}
			});

			menu.AddItem(new GUIContent("Delete"), false, () => {
				DeleteBoneDialog(index);
			});

			menu.ShowAsContext();
		}



		private void FreshAllBonesParentIndexByParent () {
			var bones = TheBones;
			if (bones != null) {
				for (int i = 0; i < bones.Count; i++) {
					bones[i].ParentIndex = bones[i].Parent ? bones.IndexOf(bones[i].Parent) : -1;
				}
			}
		}



		private void FreshAllBonesParentByParentIndex () {
			var bones = TheBones;
			if (bones != null) {
				for (int i = 0; i < bones.Count; i++) {
					var pIndex = bones[i].ParentIndex;
					bones[i].Parent = pIndex >= 0 && pIndex < bones.Count ? bones[pIndex] : null;
				}
			}
		}



		// Weights
		private Color GetWeightColor (float weight) {
			var color = Color.Lerp(
				weight < 0.5f ? Color.green : Color.yellow,
				weight < 0.5f ? Color.yellow : new Color(1f, 0.4f, 0.4f),
				weight < 0.5f ? weight * 2f : (weight - 0.5f) * 2f
			);
			color.a = weight * 0.5f + 0.5f;
			return color;
		}



		private void ForAllWeightPoints (System.Func<int, int, int, VoxelData.RigData.Weight, VoxelData.RigData.Weight> func) {
			if (TheWeights != null) {
				int sizeX = TheWeights.GetLength(0);
				int sizeY = TheWeights.GetLength(1);
				int sizeZ = TheWeights.GetLength(2);
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						for (int z = 0; z < sizeZ; z++) {
							TheWeights[x, y, z] = func(x, y, z, TheWeights[x, y, z]);
						}
					}
				}
			}
		}




		private void FixAllBoneIndexInWeightPoints (int changingIndex, int equalIndex, int offset) {
			if (changingIndex < 0) { return; }
			ForAllWeightPoints((x, y, z, weight) => {
				if (weight != null) {
					if (weight.BoneIndexA == changingIndex) {
						weight.BoneIndexA = equalIndex;
					} else if (weight.BoneIndexA > changingIndex) {
						weight.BoneIndexA += offset;
					}
					if (weight.BoneIndexB == changingIndex) {
						weight.BoneIndexB = equalIndex;
					} else if (weight.BoneIndexB > changingIndex) {
						weight.BoneIndexB += offset;
					}
					if (weight.BoneIndexA == -1 && weight.BoneIndexB == -1) {
						weight = null;
					}
				}
				return weight;
			});
		}


		#endregion



	}
}
