namespace VoxeltoUnity {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;




	public enum Direction {
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3,
		Front = 4,
		Back = 5,
	}



	public class VoxelData {



		#region --- SUB ---


		[System.Serializable]
		public class MaterialData {
			public enum MaterialType {
				Diffuse = 0,
				Metal = 1,
				Glass = 2,
				Emit = 3,
			}


			public MaterialType Type;
			public int Index;
			public float Weight;
			public float Rough;
			public float Spec;
			public float Ior;
			public float Att;
			public float Flux;
			public float Glow;
			public int Plastic;


			public static MaterialType GetTypeFromString (string voxStr) {
				switch (voxStr) {
					default:
					case "_diffuse":
						return MaterialType.Diffuse;
					case "_metal":
						return MaterialType.Metal;
					case "_glass":
						return MaterialType.Glass;
					case "_emit":
						return MaterialType.Emit;
				}
			}
			public static string GetStringFromType (MaterialType type) {
				switch (type) {
					default:
					case MaterialType.Diffuse:
						return "_diffuse";
					case MaterialType.Metal:
						return "_metal";
					case MaterialType.Glass:
						return "_glass";
					case MaterialType.Emit:
						return "_emit";
				}
			}
		}


		[System.Serializable]
		public class TransformData {

			[System.Serializable]
			public class FrameData {
				public Vector3 Rotation;
				public Vector3 Position;
				public Vector3 Scale;
			}

			public int ChildID;
			public int LayerID;
			public string Name;
			public bool Hidden;
			public int Reserved;
			public FrameData[] Frames;

		}


		public class GroupData {
			public Dictionary<string, string> Attributes;
			public int[] ChildNodeId;
		}


		public class ShapeData {
			public Dictionary<string, string> Attributes;
			public KeyValuePair<int, Dictionary<string, string>>[] ModelData;
		}

		[System.Serializable]
		public class RigData {
			[System.Serializable]
			public class Bone {

				[System.NonSerialized] public Bone Parent = null;
				[System.NonSerialized] public int ChildCount = 0;

				public int ParentIndex;// root >> -1
				public string Name;
				public int PositionX;
				public int PositionY;
				public int PositionZ;

				public static implicit operator bool (Bone bone) {
					return bone != null;
				}

			}
			[System.Serializable]
			public class Weight {

				public int X;
				public int Y;
				public int Z;
				public int BoneIndexA = -1;
				public int BoneIndexB = -1;



				public Weight () {
					X = 0;
					Y = 0;
					Z = 0;
					BoneIndexA = -1;
					BoneIndexB = -1;
				}



				public Weight (int indexA, int indexB) {
					X = 0;
					Y = 0;
					Z = 0;
					BoneIndexA = indexA;
					BoneIndexB = indexB;
				}



				public float GetWeight (int boneIndex) {
					float weight = 0f;
					if (boneIndex == BoneIndexA || boneIndex == BoneIndexB) {
						weight = 0.5f;
						if (BoneIndexA == -1 || BoneIndexB == -1) {
							weight = 1f;
						}
					}
					return weight;
				}



				public void SetWeight (int boneIndex) {
					if (BoneIndexA == boneIndex || BoneIndexB == boneIndex) {
						return;
					} else if (BoneIndexA == -1) {
						BoneIndexA = boneIndex;
					} else if (BoneIndexB == -1) {
						BoneIndexB = boneIndex;
					} else {
						BoneIndexB = BoneIndexA;
						BoneIndexA = boneIndex;
					}
				}



				public bool IndexEqualsTo (Weight other) {
					return (BoneIndexA == other.BoneIndexA && BoneIndexB == other.BoneIndexB) ||
						(BoneIndexA == other.BoneIndexB && BoneIndexB == other.BoneIndexA);
				}




			}

			public List<Bone> Bones;
			public List<Weight> Weights;

		}



		#endregion




		#region --- VAR ---



		public int Version = -1;
		public List<int[,,]> Voxels = new List<int[,,]>();
		public List<Color> Palette = new List<Color>();
		public List<MaterialData> Materials = new List<MaterialData>();
		public Dictionary<int, TransformData> Transforms = new Dictionary<int, TransformData>();
		public Dictionary<int, GroupData> Groups = new Dictionary<int, GroupData>();
		public Dictionary<int, ShapeData> Shapes = new Dictionary<int, ShapeData>();
		public Dictionary<int, RigData> Rigs = new Dictionary<int, RigData>();



		#endregion




		#region --- API ---




		public Color GetColorFromPalette (int index) {
			index--;
			return index >= 0 && index < Palette.Count ? Palette[index] : Color.clear;
		}



		public Vector3 GetModelSize (int index) {
			return new Vector3(
				Voxels[index].GetLength(0),
				Voxels[index].GetLength(1),
				Voxels[index].GetLength(2)
			);
		}



		public Vector3 GetFootPoint (int index) {

			var voxels = Voxels[index];

			int sizeX = voxels.GetLength(0);
			int sizeY = voxels.GetLength(1);
			int sizeZ = voxels.GetLength(2);

			for (int y = 0; y < sizeY; y++) {
				Vector3 foot = Vector3.zero;
				int footCount = 0;
				for (int x = 0; x < sizeX; x++) {
					for (int z = 0; z < sizeZ; z++) {
						var voxel = voxels[x, y, z];
						if (voxel != 0) {
							foot += new Vector3(x, y, z);
							footCount++;
						}
					}
				}
				if (footCount > 0) {
					return foot / footCount;
				}
			}
			return Vector3.zero;
		}



		public Vector3 GetBounds () {
			Vector3 bounds = Vector3.zero;
			for (int i = 0; i < Voxels.Count; i++) {
				var size = GetModelSize(i);
				bounds = Vector3.Max(bounds, size);
			}
			return bounds;
		}



		public VoxelData GetCopy () {
			return new VoxelData() {
				Version = Version,
				Voxels = new List<int[,,]>(Voxels),
				Palette = new List<Color>(Palette),
				Transforms = new Dictionary<int, TransformData>(Transforms),
				Groups = new Dictionary<int, GroupData>(Groups),
				Shapes = new Dictionary<int, ShapeData>(Shapes),
				Materials = new List<MaterialData>(Materials),
				Rigs = new Dictionary<int, RigData>(Rigs),
			};
		}



		public void GetModelTransform (int index, out Vector3 pos, out Vector3 rot, out Vector3 scale) {
			pos = Vector3.zero;
			rot = Vector3.zero;
			scale = Vector3.one;
			foreach (var tf in Transforms) {
				bool success = false;
				int id = tf.Value.ChildID;
				if (Shapes.ContainsKey(id)) {
					var shape = Shapes[id];
					for (int i = 0; i < shape.ModelData.Length; i++) {
						if (shape.ModelData[i].Key == index) {
							if (tf.Value.Frames.Length > 0) {
								var frame = tf.Value.Frames[0];
								pos = frame.Position;
								rot = frame.Rotation;
								scale = frame.Scale;
							}
							success = true;
							break;
						}
					}
				}
				if (success) {
					break;
				}
			}
		}



		public bool IsExposed (int index, int x, int y, int z, int sizeX, int sizeY, int sizeZ, Direction dir) {

			return IsExposed(Voxels[index], x, y, z, sizeX, sizeY, sizeZ, dir);
		}



		public void ResetToDefaultNode () {
			Transforms.Clear();
			Groups.Clear();
			Shapes.Clear();
			Transforms.Add(0, new TransformData() {
				ChildID = 1,
				Hidden = false,
				LayerID = -1,
				Name = "",
				Reserved = -1,
				Frames = new TransformData.FrameData[1] {
					new TransformData.FrameData() {
						Position = Vector3.zero,
						Rotation = Vector3.zero,
						Scale = Vector3.zero,
					},
				},
			});
			Shapes.Add(1, new ShapeData() {
				Attributes = new Dictionary<string, string>(),
				ModelData = new KeyValuePair<int, Dictionary<string, string>>[1] {
					new KeyValuePair<int, Dictionary<string, string>>(0, new Dictionary<string, string>())
				},
			});
		}



		// Global API
		public static VoxelData CreateNewData () {
			var data = new VoxelData() {
				Version = 150,
				Materials = new List<VoxelData.MaterialData>(),
				Palette = new List<Color>() { Color.white },
				Rigs = new Dictionary<int, VoxelData.RigData>(),
				Voxels = new List<int[,,]>() {
					new int[1,1,1]{ { { 1 } } },
				},
			};
			data.ResetToDefaultNode();
			return data;
		}



		public static bool IsExposed (int[,,] voxels, int x, int y, int z, int sizeX, int sizeY, int sizeZ, Direction dir) {
			if (voxels[x, y, z] == 0) { return false; }
			switch (dir) {
				case Direction.Up:
					return y == sizeY - 1 || voxels[x, y + 1, z] == 0;
				case Direction.Down:
					return y == 0 || voxels[x, y - 1, z] == 0;
				case Direction.Left:
					return x == 0 || voxels[x - 1, y, z] == 0;
				case Direction.Right:
					return x == sizeX - 1 || voxels[x + 1, y, z] == 0;
				case Direction.Front:
					return z == sizeZ - 1 || voxels[x, y, z + 1] == 0;
				case Direction.Back:
					return z == 0 || voxels[x, y, z - 1] == 0;
			}
			return false;
		}



		public static implicit operator bool (VoxelData data) {
			return data != null;
		}



		public static VoxelData GetSplitedData (VoxelData source) {


			const int SPLIT_SIZE = 126;
			if (source.Voxels.Count == 0) {
				return source;
			}

			var size = source.GetModelSize(0);
			int sizeX = Mathf.RoundToInt(size.x);
			int sizeY = Mathf.RoundToInt(size.y);
			int sizeZ = Mathf.RoundToInt(size.z);
			if (sizeX <= SPLIT_SIZE && sizeY <= SPLIT_SIZE && sizeZ <= SPLIT_SIZE) {
				return source;
			}

			int splitCountX = (sizeX / SPLIT_SIZE) + 1;
			int splitCountY = (sizeY / SPLIT_SIZE) + 1;
			int splitCountZ = (sizeZ / SPLIT_SIZE) + 1;

			var data = new VoxelData();

			// Nodes
			var childNodeId = new int[splitCountX * splitCountY * splitCountZ];
			for (int i = 0; i < childNodeId.Length; i++) {
				childNodeId[i] = i * 2 + 2;
			}

			data.Voxels = new List<int[,,]>();
			data.Transforms = new Dictionary<int, TransformData>() {
				{ 0, new TransformData(){
					Name = "",
					ChildID = 1,
					Hidden = false,
					LayerID = 0,
					Reserved = 0,
					Frames = new TransformData.FrameData[1]{
						new TransformData.FrameData(){
							Position = Vector3.zero,
							Rotation = Vector3.zero,
							Scale = Vector3.one,
						},
					},
				}},
			};
			data.Groups = new Dictionary<int, GroupData>() {
				{1, new GroupData(){
					Attributes = new Dictionary<string, string>(),
					ChildNodeId = childNodeId,
				}},
			};
			data.Shapes = new Dictionary<int, ShapeData>();
			data.Palette = new List<Color>(source.Palette);
			data.Version = source.Version;

			int _i = 0;
			for (int x = 0; x < splitCountX; x++) {
				for (int y = 0; y < splitCountY; y++) {
					for (int z = 0; z < splitCountZ; z++) {
						int splitSizeX = x < splitCountX - 1 ? SPLIT_SIZE : (sizeX - x * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeY = y < splitCountY - 1 ? SPLIT_SIZE : (sizeY - y * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeZ = z < splitCountZ - 1 ? SPLIT_SIZE : (sizeZ - z * SPLIT_SIZE) % SPLIT_SIZE;
						int childID = childNodeId[_i];
						data.Transforms.Add(childID, new TransformData() {
							Name = "Splited_Model_" + _i,
							Reserved = 0,
							LayerID = 0,
							Hidden = false,
							ChildID = childID + 1,
							Frames = new TransformData.FrameData[1] {
								new TransformData.FrameData(){
									Position = new Vector3(
										x * SPLIT_SIZE + splitSizeX / 2,
										y * SPLIT_SIZE + splitSizeY / 2,
										z * SPLIT_SIZE + splitSizeZ / 2
									),
									Rotation = Vector3.zero,
									Scale = Vector3.one,
								},
							},
						});
						data.Shapes.Add(childID + 1, new ShapeData() {
							Attributes = new Dictionary<string, string>(),
							ModelData = new KeyValuePair<int, Dictionary<string, string>>[] {
								new KeyValuePair<int, Dictionary<string, string>>(_i, new Dictionary<string, string>()),
							},
						});
						_i++;
					}
				}
			}


			// Split
			var sourceVoxels = source.Voxels[0];
			for (int x = 0; x < splitCountX; x++) {
				for (int y = 0; y < splitCountY; y++) {
					for (int z = 0; z < splitCountZ; z++) {
						int splitSizeX = x < splitCountX - 1 ? SPLIT_SIZE : (sizeX - x * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeY = y < splitCountY - 1 ? SPLIT_SIZE : (sizeY - y * SPLIT_SIZE) % SPLIT_SIZE;
						int splitSizeZ = z < splitCountZ - 1 ? SPLIT_SIZE : (sizeZ - z * SPLIT_SIZE) % SPLIT_SIZE;
						var voxels = new int[splitSizeX, splitSizeY, splitSizeZ];
						for (int i = 0; i < splitSizeX; i++) {
							for (int j = 0; j < splitSizeY; j++) {
								for (int k = 0; k < splitSizeZ; k++) {
									voxels[i, j, k] = sourceVoxels[
										x * SPLIT_SIZE + i,
										y * SPLIT_SIZE + j,
										z * SPLIT_SIZE + k
									];
								}
							}
						}
						data.Voxels.Add(voxels);
					}
				}
			}

			return data;
		}





		#endregion




		#region --- LOD ---



		public void LodIteration (int lodLevel) {


			Rigs.Clear();

			for (int index = 0; index < Voxels.Count; index++) {
				var size = GetModelSize(index);
				int sizeX = (int)size.x;
				int sizeY = (int)size.y;
				int sizeZ = (int)size.z;
				int range = lodLevel + Mathf.Max(1, (sizeX + sizeY + sizeZ) / 42);
				bool allZero = true;
				int[,,] voxels = new int[sizeX, sizeY, sizeZ];
				int[,,] sourceVoxels = Voxels[index];
				for (int x = 0; x < sizeX - range; x++) {
					for (int y = 0; y < sizeY - range; y++) {
						for (int z = 0; z < sizeZ - range; z++) {
							if (sourceVoxels[x, y, z] == 0) { continue; }
							if (allZero) { allZero = false; }
							SetRange(
								ref voxels, sourceVoxels[x, y, z],
								Mathf.Max(0, x - range), Mathf.Min(sizeX - 1, x + range),
								Mathf.Max(0, y - range), Mathf.Min(sizeY - 1, y + range),
								Mathf.Max(0, z - range), Mathf.Min(sizeZ - 1, z + range)
							);
						}
					}
				}
				if (allZero) {
					voxels[0, 0, 0] = 1;
				}
				Voxels[index] = voxels;
			}

		}



		private void SetRange (ref int[,,] voxels, int v, int l, int r, int d, int u, int b, int f) {
			for (int x = l; x < r; x++) {
				for (int y = d; y < u; y++) {
					for (int z = b; z < f; z++) {
						voxels[x, y, z] = v;
					}
				}
			}
		}





		#endregion


	}





	public class QbData {


		public struct QbMatrix {

			public string Name;
			public int SizeX, SizeY, SizeZ;
			public int PosX, PosY, PosZ;
			public int[,,] Voxels;

		}



		public uint Version;
		public uint ColorFormat; // 0->RGBA 1->BGRA
		public uint ZAxisOrientation; // 0->Left Handed  1->Right Handed
		public uint Compressed; // 0->Normal  1->WithNumbers
		public uint NumMatrixes;
		public uint VisibleMask;



		public List<QbMatrix> MatrixList;



		public VoxelData GetVoxelData () {
			var data = new VoxelData {
				Version = 150,
				Palette = new List<Color>(),
				Transforms = new Dictionary<int, VoxelData.TransformData>() {
					{ 0, new VoxelData.TransformData(){
						ChildID = 1,
						Hidden = false,
						LayerID = -1,
						Name = "",
						Reserved = -1,
						Frames = new VoxelData.TransformData.FrameData[1] {new VoxelData.TransformData.FrameData() {
							 Position = Vector3.zero,
							 Rotation = Vector3.zero,
							 Scale = Vector3.one,
						}},
					}},
				},
				Shapes = new Dictionary<int, VoxelData.ShapeData>(),
				Groups = new Dictionary<int, VoxelData.GroupData>() {
					{1, new VoxelData.GroupData(){
						 Attributes = new Dictionary<string, string>(),
						 ChildNodeId = new int[MatrixList.Count],
					}}
				},
				Materials = new List<VoxelData.MaterialData>(),
				Voxels = new List<int[,,]>(),
			};

			bool leftHanded = ZAxisOrientation == 0;

			var palette = new Dictionary<Color, int>();
			for (int index = 0; index < MatrixList.Count; index++) {

				var m = MatrixList[index];
				int[,,] voxels = new int[m.SizeX, m.SizeY, m.SizeZ];

				for (int x = 0; x < m.SizeX; x++) {
					for (int y = 0; y < m.SizeY; y++) {
						for (int z = 0; z < m.SizeZ; z++) {
							int colorInt = m.Voxels[x, y, z];
							if (colorInt == 0) {
								if (leftHanded) {
									voxels[x, y, m.SizeZ - z - 1] = 0;
								} else {
									voxels[x, y, z] = 0;
								}
							} else {
								var color = GetColor(colorInt);
								int cIndex;
								if (palette.ContainsKey(color)) {
									cIndex = palette[color];
								} else {
									cIndex = palette.Count;
									palette.Add(color, cIndex);
									data.Palette.Add(color);
								}
								if (leftHanded) {
									voxels[x, y, m.SizeZ - z - 1] = cIndex + 1;
								} else {
									voxels[x, y, z] = cIndex + 1;
								}
							}
						}
					}
				}

				data.Voxels.Add(voxels);
				data.Groups[1].ChildNodeId[index] = index * 2 + 2;
				data.Transforms.Add(index * 2 + 2, new VoxelData.TransformData() {
					ChildID = index * 2 + 3,
					Hidden = false,
					LayerID = 0,
					Reserved = 0,
					Name = "",
					Frames = new VoxelData.TransformData.FrameData[1] {new VoxelData.TransformData.FrameData() {
						Position = leftHanded ?
							new Vector3(m.PosX + m.SizeX / 2, m.PosY + m.SizeY / 2, -(m.PosZ + Mathf.CeilToInt(m.SizeZ / 2f))) :
							new Vector3(m.PosX + m.SizeX / 2, m.PosY + m.SizeY / 2, m.PosZ + m.SizeZ / 2),
						Rotation = Vector3.zero,
						Scale = Vector3.one,
					}},
				});
				data.Shapes.Add(index * 2 + 3, new VoxelData.ShapeData() {
					Attributes = new Dictionary<string, string>(),
					ModelData = new KeyValuePair<int, Dictionary<string, string>>[1] {
						new KeyValuePair<int, Dictionary<string, string>>(index, new Dictionary<string, string>())
					},
				});
			}

			return data;
		}



		private Color GetColor (int color) {
			if (ColorFormat == 0) {
				int r = 0xFF & color;
				int g = 0xFF00 & color;
				g >>= 8;
				int b = 0xFF0000 & color;
				b >>= 16;
				return new Color(r / 255f, g / 255f, b / 255f, 1f);
			} else {
				int b = 0xFF & color;
				int g = 0xFF00 & color;
				g >>= 8;
				int r = 0xFF0000 & color;
				r >>= 16;
				return new Color(r / 255f, g / 255f, b / 255f, 1f);
			}
		}



	}




	[System.Serializable]
	public class VoxelJsonData {


		[System.Serializable]
		public class IntArray3 {
			public int[] Value;
			public int SizeX;
			public int SizeY;
			public int SizeZ;

			public IntArray3 (int sizeX, int sizeY, int sizeZ) {
				SizeX = sizeX;
				SizeY = sizeY;
				SizeZ = sizeZ;
				Value = new int[sizeX * sizeY * sizeZ];
			}

			public void Set (int x, int y, int z, int value) {
				Value[z * SizeX * SizeY + y * SizeX + x] = value;
			}

			public int Get (int x, int y, int z) {
				return Value[z * SizeX * SizeY + y * SizeX + x];
			}

		}


		[System.Serializable]
		public class SerGroupData {
			public int Index;
			public string[] AttKeys;
			public string[] Attributes;
			public int[] ChildNodeId;
		}


		[System.Serializable]
		public class SerShapeData {

			[System.Serializable]
			public class ModelMap {
				public string[] Key;
				public string[] Value;
			}

			public int Index;
			public string[] AttKeys;
			public string[] Attributes;
			public int[] ModelDataIndexs;
			public ModelMap[] ModelDatas;
		}


		public int Version;
		public IntArray3[] Voxels;
		public Color[] Palette;
		public VoxelData.MaterialData[] Materials;
		public int[] TransformIndexs;
		public VoxelData.TransformData[] Transforms;
		public SerGroupData[] Groups;
		public SerShapeData[] Shapes;
		public int[] RigIndexs;
		public VoxelData.RigData[] Rigs;




		public VoxelJsonData (VoxelData source) {

			// Version
			Version = source.Version;

			// Voxels
			Voxels = new IntArray3[source.Voxels.Count];
			for (int i = 0; i < Voxels.Length; i++) {
				int sizeX = source.Voxels[i].GetLength(0);
				int sizeY = source.Voxels[i].GetLength(1);
				int sizeZ = source.Voxels[i].GetLength(2);
				var voxels = new IntArray3(sizeX, sizeY, sizeZ);
				var sourceVoxels = source.Voxels[i];
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						for (int z = 0; z < sizeZ; z++) {
							voxels.Set(x, y, z, sourceVoxels[x, y, z]);
						}
					}
				}
				Voxels[i] = voxels;
			}

			// Palette
			Palette = source.Palette.ToArray();

			// Materials
			Materials = source.Materials.ToArray();

			// Transforms
			var tfMap = source.Transforms;
			TransformIndexs = new int[tfMap.Count];
			Transforms = new VoxelData.TransformData[tfMap.Count];
			int index = 0;
			foreach (var tf in tfMap) {
				TransformIndexs[index] = tf.Key;
				Transforms[index] = tf.Value;
				index++;
			}

			// Groups
			var gpMap = source.Groups;
			Groups = new SerGroupData[gpMap.Count];
			index = 0;
			foreach (var gp in gpMap) {
				Groups[index] = new SerGroupData() {
					Index = gp.Key,
					ChildNodeId = gp.Value.ChildNodeId,
					AttKeys = new string[gp.Value.Attributes.Count],
					Attributes = new string[gp.Value.Attributes.Count],
				};
				var attMap = gp.Value.Attributes;
				int i = 0;
				foreach (var att in attMap) {
					Groups[index].AttKeys[i] = att.Key;
					Groups[index].Attributes[i] = att.Value;
					i++;
				}
				index++;
			}

			// Shapes
			var shMap = source.Shapes;
			Shapes = new SerShapeData[shMap.Count];
			index = 0;
			foreach (var sh in shMap) {
				Shapes[index] = new SerShapeData {
					Index = sh.Key,
					ModelDataIndexs = new int[sh.Value.ModelData.Length],
					ModelDatas = new SerShapeData.ModelMap[sh.Value.ModelData.Length],
					AttKeys = new string[sh.Value.Attributes.Count],
					Attributes = new string[sh.Value.Attributes.Count],
				};
				var attMap = sh.Value.Attributes;
				int i = 0;
				foreach (var att in attMap) {
					Shapes[index].AttKeys[i] = att.Key;
					Shapes[index].Attributes[i] = att.Value;
					i++;
				}
				for (i = 0; i < sh.Value.ModelData.Length; i++) {
					Shapes[index].ModelDataIndexs[i] = sh.Value.ModelData[i].Key;
					Shapes[index].ModelDatas[i] = new SerShapeData.ModelMap() {
						Key = new string[sh.Value.ModelData[i].Value.Count],
						Value = new string[sh.Value.ModelData[i].Value.Count],
					};
					int j = 0;
					var map = sh.Value.ModelData[i].Value;
					foreach (var aj in map) {
						Shapes[index].ModelDatas[i].Key[j] = aj.Key;
						Shapes[index].ModelDatas[i].Value[j] = aj.Value;
						j++;
					}
				}
				index++;
			}

			// Rigs
			var rigMap = source.Rigs;
			RigIndexs = new int[rigMap.Count];
			Rigs = new VoxelData.RigData[rigMap.Count];
			index = 0;
			foreach (var rig in rigMap) {
				RigIndexs[index] = rig.Key;
				Rigs[index] = rig.Value;
				var bones = Rigs[index].Bones;
				for (int i = 0; i < bones.Count; i++) {
					bones[i].ParentIndex = bones.IndexOf(bones[i].Parent);
				}
				index++;
			}


		}



		public VoxelData GetVoxelData () {

			var data = new VoxelData() {
				Version = Version,
				Voxels = new List<int[,,]>(),
				Palette = new List<Color>(Palette),
				Materials = new List<VoxelData.MaterialData>(Materials),
				Transforms = new Dictionary<int, VoxelData.TransformData>(),
				Groups = new Dictionary<int, VoxelData.GroupData>(),
				Shapes = new Dictionary<int, VoxelData.ShapeData>(),
				Rigs = new Dictionary<int, VoxelData.RigData>(),
			};

			// Voxels
			for (int i = 0; i < Voxels.Length; i++) {
				var sourceV = Voxels[i];
				int[,,] vs = new int[sourceV.SizeX, sourceV.SizeY, sourceV.SizeZ];
				for (int x = 0; x < sourceV.SizeX; x++) {
					for (int y = 0; y < sourceV.SizeY; y++) {
						for (int z = 0; z < sourceV.SizeZ; z++) {
							vs[x, y, z] = sourceV.Get(x, y, z);
						}
					}
				}
				data.Voxels.Add(vs);
			}

			// Transforms
			for (int i = 0; i < Transforms.Length; i++) {
				data.Transforms.Add(TransformIndexs[i], Transforms[i]);
			}

			// Groups
			for (int i = 0; i < Groups.Length; i++) {
				var groupData = new VoxelData.GroupData() {
					ChildNodeId = Groups[i].ChildNodeId,
					Attributes = new Dictionary<string, string>(),
				};
				for (int j = 0; j < Groups[i].AttKeys.Length; j++) {
					groupData.Attributes.Add(Groups[i].AttKeys[j], Groups[i].Attributes[j]);
				}
				data.Groups.Add(Groups[i].Index, groupData);
			}

			// Shapes
			for (int i = 0; i < Shapes.Length; i++) {
				var sourceShape = Shapes[i];
				var shapeData = new VoxelData.ShapeData() {
					Attributes = new Dictionary<string, string>(),
					ModelData = new KeyValuePair<int, Dictionary<string, string>>[sourceShape.ModelDatas.Length],
				};
				for (int j = 0; j < sourceShape.Attributes.Length; j++) {
					shapeData.Attributes.Add(sourceShape.AttKeys[j], sourceShape.Attributes[j]);
				}
				for (int j = 0; j < sourceShape.ModelDatas.Length; j++) {
					shapeData.ModelData[j] = new KeyValuePair<int, Dictionary<string, string>>(
						sourceShape.ModelDataIndexs[j],
						new Dictionary<string, string>()
					);
					var sourceData = sourceShape.ModelDatas[j];
					for (int k = 0; k < sourceData.Value.Length; k++) {
						shapeData.ModelData[j].Value.Add(sourceData.Key[k], sourceData.Value[k]);
					}
				}
				data.Shapes.Add(sourceShape.Index, shapeData);
			}

			// Rig
			for (int i = 0; i < Rigs.Length; i++) {
				var bones = Rigs[i].Bones;
				for (int j = 0; j < bones.Count; j++) {
					bones[j].Parent = null;
					int pIndex = bones[j].ParentIndex;
					if (pIndex >= 0 && pIndex < bones.Count) {
						bones[j].Parent = bones[pIndex];
					}
				}
				data.Rigs.Add(RigIndexs[i], Rigs[i]);
			}

			return data;
		}


	}



}