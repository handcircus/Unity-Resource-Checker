// Resource Checker
// (c) 2012 Simon Oliver / HandCircus / hello@handcircus.com
// Public domain, do with whatever you like, commercial or not
// This comes with no warranty, use at your own risk!
// https://github.com/handcircus/Unity-Resource-Checker

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


public class TextureDetails
{
	public bool isCubeMap;
	public int memSizeKB;
	public Texture texture;
	public TextureFormat format;
	public List<Material> FoundInMaterials=new List<Material>();
	public List<Renderer> FoundInRenderers=new List<Renderer>();
	public TextureDetails()
	{
		
	}
};

public class MaterialDetails
{
	
	public Material material;

	public List<Renderer> FoundInRenderers=new List<Renderer>();

	public MaterialDetails()
	{
		
	}
};

public class MeshDetails
{
	
	public Mesh mesh;

	public List<MeshFilter> FoundInMeshFilters=new List<MeshFilter>();

	public MeshDetails()
	{
		
	}
};

public class ResourceChecker : EditorWindow {
	
	
	string[] inspectToolbarStrings = {"Textures", "Materials","Meshes"};
	
	enum InspectType 
	{
		Textures,Materials,Meshes
	};
	
	InspectType ActiveInspectType=InspectType.Textures;
	
	float ThumbnailWidth=40;
	float ThumbnailHeight=40;
	
	List<TextureDetails> ActiveTextures=new List<TextureDetails>();
	List<MaterialDetails> ActiveMaterials=new List<MaterialDetails>();
	List<MeshDetails> ActiveMeshDetails=new List<MeshDetails>();
	
	Vector2 textureListScrollPos=new Vector2(0,0);
	Vector2 materialListScrollPos=new Vector2(0,0);
	Vector2 meshListScrollPos=new Vector2(0,0);
	
	int TotalTextureMemory=0;
	int TotalMeshVertices=0;
    
    [MenuItem ("Window/Resource Checker")]
    static void Init ()
	{  
        ResourceChecker window = (ResourceChecker) EditorWindow.GetWindow (typeof (ResourceChecker));
		window.CheckResources();
		window.minSize=new Vector2(435,300);
    }
    
    void OnGUI ()
	{
		if (GUILayout.Button("Refresh")) CheckResources();
		
		GUILayout.Label("Total materials "+ActiveMaterials.Count);
		GUILayout.Label("Total textures "+ActiveTextures.Count+" - "+FormatSizeString(TotalTextureMemory));
		GUILayout.Label("Total meshes "+ActiveMeshDetails.Count+" - "+TotalMeshVertices+" verts");
		
		ActiveInspectType=(InspectType)GUILayout.Toolbar((int)ActiveInspectType,inspectToolbarStrings);
		
		switch (ActiveInspectType)
		{
			case InspectType.Textures:
				ListTextures();
				break;
			case InspectType.Materials:
				ListMaterials();
				break;
			case InspectType.Meshes:
				ListMeshes();
				break;	
			
			
		}
	}
	
	
	int GetBitsPerPixel(TextureFormat format)
	{
		switch (format)
		{
			case TextureFormat.Alpha8: //	 Alpha-only texture format.
				return 8;
			case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
				return 16;
			case TextureFormat.RGB24:	// A color texture format.
				return 24;
			case TextureFormat.RGBA32:	//Color with an alpha channel texture format.
				return 32;
			case TextureFormat.ARGB32:	//Color with an alpha channel texture format.
				return 32;
			case TextureFormat.RGB565:	//	 A 16 bit color texture format.
				return 16;
			case TextureFormat.DXT1:	// Compressed color texture format.
				return 4;
			case TextureFormat.DXT5:	// Compressed color with alpha channel texture format.
				return 4;
			/*
			case TextureFormat.WiiI4:	// Wii texture format.
			case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
			case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
			case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
			case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
			case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
			case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
			case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
				return 0;  //Not supported yet
			*/
			case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
				return 2;
			case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
				return 2;
			case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
				return 4;
			case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
				return 4;
			case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
				return 4;
			case TextureFormat.ATC_RGB4://	 ATC (ATITC) 4 bits/pixel compressed RGB texture format.
				return 4;
			case TextureFormat.ATC_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
				return 8;
			case TextureFormat.BGRA32://	 Format returned by iPhone camera
				return 32;
			case TextureFormat.ATF_RGB_DXT1://	 Flash-specific RGB DXT1 compressed color texture format.
			case TextureFormat.ATF_RGBA_JPG://	 Flash-specific RGBA JPG-compressed color texture format.
			case TextureFormat.ATF_RGB_JPG://	 Flash-specific RGB JPG-compressed color texture format.
				return 0; //Not supported yet
		}
		return 0;
	}
	
	int CalculateTextureSizeBytes(Texture tTexture)
	{
		
		int tWidth=tTexture.width;
		int tHeight=tTexture.height;
		if (tTexture is Texture2D)
		{
			Texture2D tTex2D=tTexture as Texture2D;
		 	int bitsPerPixel=GetBitsPerPixel(tTex2D.format);
			return tWidth*tHeight*bitsPerPixel/8;
		}
		
		if (tTexture is Cubemap)
		{
			Cubemap tCubemap=tTexture as Cubemap;
		 	int bitsPerPixel=GetBitsPerPixel(tCubemap.format);
			return tWidth*tHeight*6*bitsPerPixel/8;
		}
		return 0;
	}
	
	void ListTextures()
	{
		textureListScrollPos = EditorGUILayout.BeginScrollView(textureListScrollPos);
		
		foreach (TextureDetails tDetails in ActiveTextures)
		{			
			
			GUILayout.BeginHorizontal ();
			GUILayout.Box(tDetails.texture, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));
			
			if(GUILayout.Button(tDetails.texture.name,GUILayout.Width(150)))
			{
				Selection.activeObject=tDetails.texture;
			}
			
			string sizeLabel=""+tDetails.texture.width+"x"+tDetails.texture.height;
			if (tDetails.isCubeMap) sizeLabel+="x6";
			sizeLabel+="\n"+FormatSizeString(tDetails.memSizeKB)+" - "+tDetails.format+"";
			
			GUILayout.Label (sizeLabel,GUILayout.Width(100));
					
			if(GUILayout.Button(tDetails.FoundInMaterials.Count+" Mat",GUILayout.Width(50)))
			{
				Selection.objects=tDetails.FoundInMaterials.ToArray();
			}
			
			if(GUILayout.Button(tDetails.FoundInRenderers.Count+" GO",GUILayout.Width(50)))
			{
				List<GameObject> FoundObjects=new List<GameObject>();
				foreach (Renderer renderer in tDetails.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
				Selection.objects=FoundObjects.ToArray();
			}
			
			GUILayout.EndHorizontal();	
		}
		EditorGUILayout.EndScrollView();		
    }
	
	void ListMaterials()
	{
		materialListScrollPos = EditorGUILayout.BeginScrollView(materialListScrollPos);
		
		foreach (MaterialDetails tDetails in ActiveMaterials)
		{			
			if (tDetails.material!=null)
			{
				GUILayout.BeginHorizontal ();
				
					if (tDetails.material.mainTexture!=null) GUILayout.Box(tDetails.material.mainTexture, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));
				else	
				{
					GUILayout.Box("n/a",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));
				}
				
				if(GUILayout.Button(tDetails.material.name,GUILayout.Width(150)))
				{
					Selection.activeObject=tDetails.material;
				}
				
				if(GUILayout.Button(tDetails.FoundInRenderers.Count+" GO",GUILayout.Width(50)))
				{
					List<GameObject> FoundObjects=new List<GameObject>();
					foreach (Renderer renderer in tDetails.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
					Selection.objects=FoundObjects.ToArray();
				}
				
				
				GUILayout.EndHorizontal();	
			}
		}
		EditorGUILayout.EndScrollView();		
    }
	
	void ListMeshes()
	{
		meshListScrollPos = EditorGUILayout.BeginScrollView(meshListScrollPos);
		
		foreach (MeshDetails tDetails in ActiveMeshDetails)
		{			
			if (tDetails.mesh!=null)
			{
				GUILayout.BeginHorizontal ();
				/*
				if (tDetails.material.mainTexture!=null) GUILayout.Box(tDetails.material.mainTexture, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));
				else	
				{
					GUILayout.Box("n/a",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));
				}
				*/
				
				if(GUILayout.Button(tDetails.mesh.name,GUILayout.Width(150)))
				{
					Selection.activeObject=tDetails.mesh;
				}
				string sizeLabel=""+tDetails.mesh.vertexCount+" vert";
				
				GUILayout.Label (sizeLabel,GUILayout.Width(100));
				
				
				if(GUILayout.Button(tDetails.FoundInMeshFilters.Count+" GO",GUILayout.Width(50)))
				{
					List<GameObject> FoundObjects=new List<GameObject>();
					foreach (MeshFilter meshFilter in tDetails.FoundInMeshFilters) FoundObjects.Add(meshFilter.gameObject);
					Selection.objects=FoundObjects.ToArray();
				}
				
				
				GUILayout.EndHorizontal();	
			}
		}
		EditorGUILayout.EndScrollView();		
    }
	
	string FormatSizeString(int memSizeKB)
	{
		if (memSizeKB<1024) return ""+memSizeKB+"k";
		else
		{
			float memSizeMB=((float)memSizeKB)/1024.0f;
			return memSizeMB.ToString("0.00")+"Mb";
		}
	}
	
	
	TextureDetails FindTextureDetails(Texture tTexture)
	{
		foreach (TextureDetails tTextureDetails in ActiveTextures)
		{
			if (tTextureDetails.texture==tTexture) return tTextureDetails;
		}
		return null;
		
	}
	
	MaterialDetails FindMaterialDetails(Material tMaterial)
	{
		foreach (MaterialDetails tMaterialDetails in ActiveMaterials)
		{
			if (tMaterialDetails.material==tMaterial) return tMaterialDetails;
		}
		return null;
		
	}
	
	MeshDetails FindMeshDetails(Mesh tMesh)
	{
		foreach (MeshDetails tMeshDetails in ActiveMeshDetails)
		{
			if (tMeshDetails.mesh==tMesh) return tMeshDetails;
		}
		return null;
		
	}
		
	
	void CheckResources()
	{
		ActiveTextures.Clear();
		ActiveMaterials.Clear();
		ActiveMeshDetails.Clear();
		
		Renderer[] renderers = (Renderer[]) FindObjectsOfType(typeof(Renderer));
		//Debug.Log("Total renderers "+renderers.Length);
		foreach (Renderer renderer in renderers)
		{
			//Debug.Log("Renderer is "+renderer.name);
			foreach (Material material in renderer.sharedMaterials)
			{
				
				MaterialDetails tMaterialDetails=FindMaterialDetails(material);
				if (tMaterialDetails==null)
				{
					tMaterialDetails=new MaterialDetails();
					tMaterialDetails.material=material;
					ActiveMaterials.Add(tMaterialDetails);
				}
				tMaterialDetails.FoundInRenderers.Add(renderer);
			}
		}
		
		foreach (MaterialDetails tMaterialDetails in ActiveMaterials)
		{
			Material tMaterial=tMaterialDetails.material;
			foreach (Object obj in EditorUtility.CollectDependencies(new UnityEngine.Object[] {tMaterial}))
		    {
				if (obj is Texture)
				{
					Texture tTexture=obj as Texture;
					TextureDetails tTextureDetails=FindTextureDetails(tTexture);
					if (tTextureDetails==null)
					{
						tTextureDetails=new TextureDetails();
						tTextureDetails.texture=tTexture;
						tTextureDetails.isCubeMap=tTexture is Cubemap;
						
						int memSize=CalculateTextureSizeBytes(tTexture);
			
						tTextureDetails.memSizeKB=memSize/1024;
						TextureFormat tFormat=TextureFormat.RGBA32;
						
						if (tTexture is Texture2D) tFormat=(tTexture as Texture2D).format;
						if (tTexture is Cubemap) tFormat=(tTexture as Cubemap).format;
				
						tTextureDetails.format=tFormat;
						ActiveTextures.Add(tTextureDetails);
					}
					tTextureDetails.FoundInMaterials.Add(tMaterial);
					foreach (Renderer renderer in tMaterialDetails.FoundInRenderers)
					{
						if (!tTextureDetails.FoundInRenderers.Contains(	renderer)) tTextureDetails.FoundInRenderers.Add(renderer);
					}
				}
			}
		}
		
		
		MeshFilter[] meshFilters = (MeshFilter[]) FindObjectsOfType(typeof(MeshFilter));
		
		foreach (MeshFilter tMeshFilter in meshFilters)
		{
			Mesh tMesh=tMeshFilter.sharedMesh;
			if (tMesh!=null)
			{
				MeshDetails tMeshDetails=FindMeshDetails(tMesh);
				if (tMeshDetails==null)
				{
					tMeshDetails=new MeshDetails();
					tMeshDetails.mesh=tMesh;
					ActiveMeshDetails.Add(tMeshDetails);
				}
				tMeshDetails.FoundInMeshFilters.Add(tMeshFilter);
			}
		}
		
	
		TotalTextureMemory=0;
		foreach (TextureDetails tTextureDetails in ActiveTextures) TotalTextureMemory+=tTextureDetails.memSizeKB;
		
		TotalMeshVertices=0;
		foreach (MeshDetails tMeshDetails in ActiveMeshDetails) TotalMeshVertices+=tMeshDetails.mesh.vertexCount;
		
		// Sort by size, descending
		ActiveTextures.Sort(delegate(TextureDetails details1, TextureDetails details2) {return details2.memSizeKB-details1.memSizeKB;});
		ActiveMeshDetails.Sort(delegate(MeshDetails details1, MeshDetails details2) {return details2.mesh.vertexCount-details1.mesh.vertexCount;});
		
	}
	
	
}