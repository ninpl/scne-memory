//                                  ┌∩┐(◣_◢)┌∩┐
//                                                                              \\
// SceneStreamer.cs (27/06/2017)												\\
// Autor: Antonio Mateo (Moon Antonio) 									        \\
// Descripcion:		Manager de las escenas										\\
// Fecha Mod:		27/06/2017													\\
// Ultima Mod:	    Version Inicial												\\
//******************************************************************************\\

#region Librerias
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#endregion

namespace MoonAntonio.SceneMemory
{
	#region Clases Eventos
	[System.Serializable]
	public class StringEvent : UnityEvent<string> { }
	[System.Serializable]
	public class StringAsyncEvent : UnityEvent<string, AsyncOperation> { }
	#endregion

	/*	NOTAS
	Scene Streamer es un Singleton MonoBehaviour usado para cargar y descargar escenas que contienen
	piezas del mundo del juego. Se puede usar para implementar mundos continuos. La pieza
	del mundo que contiene el jugador se llama la "escena actual". SceneStreamer 
	Carga automaticamente las escenas vecinas hasta una distancia que se especifique y descarga
	escenas mas alla de esa distancia.
	*/

	/// <summary>
	/// <para>Manager de las escenas</para>
	/// </summary>
	[AddComponentMenu("Scene Memory/Scene Streamer")]
    public class SceneStreamer : MonoBehaviour
    {
		#region Variables Publicas
		/// <summary>
		/// <para>Numero maximo de vecinos a cargar de la escena actual.</para>
		/// </summary>
		public int maxNeighborDistance = 1;								// Numero maximo de vecinos a cargar de la escena actual
		/// <summary>
		/// <para>Un maximo de tiempo en caso de que la carga se cuelgue. Despues de estos segundos, el SceneStreamer dejara de esperar a que la escena se cargue.</para>
		/// </summary>
		public float maxLoadWaitTime = 10f;								// Un maximo de tiempo en caso de que la carga se cuelgue
		/// <summary>
		/// <para>Un evento de cargando</para>
		/// </summary>
		public StringAsyncEvent onLoading = new StringAsyncEvent();		// Un evento de cargando
		/// <summary>
		/// <para>Un evento de cargado</para>
		/// </summary>
		public StringEvent onLoaded = new StringEvent();				// Un evento de cargado
		/// <summary>
		/// <para>Un Auxiliar de debug</para>
		/// </summary>
		public bool debug = false;										// Un Auxiliar de debug
		/// <summary>
		/// <para>Informacion del debug</para>
		/// </summary>
		public bool logDebugInfo { get { return debug && Debug.isDebugBuild; } }// Informacion del debug
		#endregion

		#region Variables Privadas
		/// <summary>
		/// <para>El nombre de la escena actual del jugador.</para>
		/// </summary>
		private string m_currentSceneName = null;                       // El nombre de la escena actual del jugador.
		/// <summary>
		/// <para>Los nombres de todas las escenas cargadas.</para>
		/// </summary>
		private HashSet<string> m_loaded = new HashSet<string>();       // Los nombres de todas las escenas cargadas.
		/// <summary>
		/// <para>Los nombres de todas las escenas que estan en proceso de ser cargadas.</para>
		/// </summary>
		private HashSet<string> m_loading = new HashSet<string>();      // Los nombres de todas las escenas que estan en proceso de ser cargadas
		/// <summary>
		/// <para>Los nombres de todas las escenas dentro de maxNeighborDistance de la escena actual.</para>
		/// </summary>
		private HashSet<string> m_near = new HashSet<string>();			// Los nombres de todas las escenas dentro de maxNeighborDistance de la escena actual.
		#endregion

		#region Singleton
		/// <summary>
		/// <para>Bloqueador singleton</para>
		/// </summary>
		private static object s_lock = new object();					// Bloqueador singleton
		/// <summary>
		/// <para>Instancia de <see cref="SceneStreamer"/></para>
		/// </summary>
		private static SceneStreamer s_instance = null;					// Instancia de SceneStreamer
		/// <summary>
		/// <para>Instancia de <see cref="SceneStreamer"/></para>
		/// </summary>
		private static SceneStreamer instance							// Instancia de SceneStreamer
		{
			get
			{
				lock (s_lock)
				{
					if (s_instance == null)
					{
						s_instance = FindObjectOfType(typeof(SceneStreamer)) as SceneStreamer;
						if (s_instance == null)
						{
							s_instance = new GameObject("Scene Loader", typeof(SceneStreamer)).GetComponent<SceneStreamer>();
						}
					}
					return s_instance;
				}
			}
			set
			{
				s_instance = value;
			}
		}
		#endregion

		#region Inicializadores
		/// <summary>
		/// <para>Inicializador de <see cref="SceneStreamer"/></para>
		/// </summary>
		public void Awake()// Inicializador de SceneStreamer
		{
			// Singleton
			if (s_instance)
			{
				Destroy(this);
			}
			else
			{
				s_instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
		}
		#endregion

		#region Metodos
		/// <summary>
		/// <para>Establece la escena actual, la carga y gestiona los vecinos.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		public void SetCurrent(string sceneName)// Establece la escena actual, la carga y gestiona los vecinos.
		{
			if (string.IsNullOrEmpty(sceneName) || string.Equals(sceneName, m_currentSceneName)) return;

			if (logDebugInfo) Debug.Log("Scene Streamer: Ajuste de la escena actual a " + sceneName + ".");

			StartCoroutine(LoadCurrentScene(sceneName));
		}

		/// <summary>
		/// <para>Carga una escena como escena actual y gestiona vecinos</para>
		/// </summary>
		/// <returns>Escena actual.</returns>
		/// <param name="sceneName">Nombre escena.</param>
		private IEnumerator LoadCurrentScene(string sceneName)// Carga una escena como escena actual y gestiona vecinos
		{
			// Primero cargar la escena actual
			m_currentSceneName = sceneName;

			if (!IsLoaded(m_currentSceneName)) Load(sceneName);

			float failsafeTime = Time.realtimeSinceStartup + maxLoadWaitTime;

			while ((m_loading.Count > 0) && (Time.realtimeSinceStartup < failsafeTime))
			{
				yield return null;
			}

			if (Time.realtimeSinceStartup >= failsafeTime && Debug.isDebugBuild) Debug.LogWarning("Scene Streamer: Tiempo de espera agotado " + sceneName + ".");

			// Carga vecinos hasta maxNeighborDistance
			if (logDebugInfo) Debug.Log("Scene Streamer: Cargando " + maxNeighborDistance + " vecinos mas cercanos de " + sceneName + ".");

			m_near.Clear();

			LoadNeighbors(sceneName, 0);

			failsafeTime = Time.realtimeSinceStartup + maxLoadWaitTime;

			while ((m_loading.Count > 0) && (Time.realtimeSinceStartup < failsafeTime))
			{
				yield return null;
			}

			if (Time.realtimeSinceStartup >= failsafeTime && Debug.isDebugBuild) Debug.LogWarning("Scene Streamer: Tiempo de espera para cargar a los vecinos de " + sceneName + ".");

			// Finalmente descargar cualquier escena que no este en la lista cercana
			UnloadFarScenes();
		}

		/// <summary>
		/// <para>Carga las escenas vecinas dentro de maxNeighborDistance, agregandolas a la lista cercana.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		/// <param name="distance">Distancia.</param>
		private void LoadNeighbors(string sceneName, int distance)// Carga las escenas vecinas dentro de maxNeighborDistance, agregandolas a la lista cercana
		{
			if (m_near.Contains(sceneName)) return;

			m_near.Add(sceneName);

			if (distance >= maxNeighborDistance) return;

			GameObject scene = GameObject.Find(sceneName);

			NeighboringScenes neighboringScenes = (scene) ? scene.GetComponent<NeighboringScenes>() : null;

			if (!neighboringScenes) neighboringScenes = CreateNeighboringScenesList(scene);

			if (!neighboringScenes) return;

			for (int i = 0; i < neighboringScenes.sceneNames.Length; i++)
			{
				Load(neighboringScenes.sceneNames[i], LoadNeighbors, distance + 1);
			}
		}

		/// <summary>
		/// <para>Crea la lista de escenas vecinas</para>
		/// </summary>
		/// <returns>La lista de escenas vecinas.</returns>
		/// <param name="scene">Escena actual.</param>
		private NeighboringScenes CreateNeighboringScenesList(GameObject scene)// Crea la lista de escenas vecinas
		{
			if (!scene) return null;

			NeighboringScenes neighboringScenes = scene.AddComponent<NeighboringScenes>();
			HashSet<string> neighbors = new HashSet<string>();
			var sceneEdges = scene.GetComponentsInChildren<SceneEdge>();

			for (int i = 0; i < sceneEdges.Length; i++)
			{
				neighbors.Add(sceneEdges[i].nextSceneName);
			}

			neighboringScenes.sceneNames = new string[neighbors.Count];
			neighbors.CopyTo(neighboringScenes.sceneNames);

			return neighboringScenes;
		}

		/// <summary>
		/// <para>Determina si se ha cargado una escena.</para>
		/// </summary>
		/// <returns><c>true</c> si se ha cargado; sino, <c>false</c>.</returns>
		/// <param name="sceneName">Nombre escena.</param>
		public bool IsLoaded(string sceneName)// Determina si se ha cargado una escena
		{
			return m_loaded.Contains(sceneName);
		}

		/// <summary>
		/// <para>Cargar escena</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		public void Load(string sceneName)// Cargar escena
		{
			Load(m_currentSceneName, null, 0);
		}

		/// <summary>
		/// <para>Carga una escena y llama a un delegado interno cuando se hace.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		/// <param name="loadedHandler">Delegado.</param>
		/// <param name="distance">Distancia.</param>
		private void Load(string sceneName, InternalLoadedHandler loadedHandler, int distance)// Carga una escena y llama a un delegado interno cuando se hace
		{
			if (IsLoaded(sceneName))
			{
				if (loadedHandler != null) loadedHandler(sceneName, distance);
				return;
			}

			m_loading.Add(sceneName);
			if (logDebugInfo && distance > 0) Debug.Log("Scene Streamer: Cargando " + sceneName + ".");

			StartCoroutine(LoadAdditiveAsync(sceneName, loadedHandler, distance));
		}

		/// <summary>
		/// <para>Ejecuta Application.LoadLevelAdditiveAsync() y llama a FinishLoad() cuando se realiza.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		/// <param name="loadedHandler">Delegado.</param>
		/// <param name="distance">Distancia.</param>
		private IEnumerator LoadAdditiveAsync(string sceneName, InternalLoadedHandler loadedHandler, int distance)// Ejecuta Application.LoadLevelAdditiveAsync() y llama a FinishLoad() cuando se realiza
		{
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

			onLoading.Invoke(sceneName, asyncOperation);

			yield return asyncOperation;

			FinishLoad(sceneName, loadedHandler, distance);
		}

		/// <summary>
		/// <para>Ejecuta Application.LoadLevelAdditiveAsync() y llama a FinishLoad() cuando se realiza.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		/// <param name="loadedHandler">Delegado.</param>
		/// <param name="distance">Distancia.</param>
		private IEnumerator LoadAdditive(string sceneName, InternalLoadedHandler loadedHandler, int distance)// Ejecuta Application.LoadLevelAdditiveAsync() y llama a FinishLoad() cuando se realiza
		{
			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

			onLoading.Invoke(sceneName, null);

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			FinishLoad(sceneName, loadedHandler, distance);
		}

		/// <summary>
		/// <para>Se llama cuando se hace un nivel de carga.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		/// <param name="loadedHandler">Delegado.</param>
		/// <param name="distance">Distancia.</param>
		private void FinishLoad(string sceneName, InternalLoadedHandler loadedHandler, int distance)// Se llama cuando se hace un nivel de carga
		{
			GameObject scene = GameObject.Find(sceneName);
			if (scene == null && Debug.isDebugBuild) Debug.LogWarning("Scene Streamer: No se puede encontrar la escena cargada llamada '" + sceneName + "'.");
			m_loading.Remove(sceneName);
			m_loaded.Add(sceneName);
			onLoaded.Invoke(sceneName);
			if (loadedHandler != null) loadedHandler(sceneName, distance);
		}

		/// <summary>
		/// <para>Descarga escenas mas alla de maxNeighborDistance. Supone que la lista cercana ya se ha poblado.</para>
		/// </summary>
		private void UnloadFarScenes()// Descarga escenas más allá de maxNeighborDistance. Supone que la lista cercana ya se ha poblado.
		{
			HashSet<string> far = new HashSet<string>(m_loaded);
			far.ExceptWith(m_near);
			if (logDebugInfo && far.Count > 0) Debug.Log("Scene Streamer: Descargar escenas mas de " + maxNeighborDistance + " metros de la escena actual " + m_currentSceneName + ".");
			foreach (var sceneName in far)
			{
				Unload(sceneName);
			}
		}

		/// <summary>
		/// <para>Descarga una escena.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		public void Unload(string sceneName)//Descarga una escena.
		{
			if (logDebugInfo) Debug.Log("Scene Streamer: Descargando escena " + sceneName + ".");
			Destroy(GameObject.Find(sceneName));
			m_loaded.Remove(sceneName);
			SceneManager.UnloadSceneAsync(sceneName);
		}
		#endregion

		#region API
		/// <summary>
		/// <para>Establece la escena actual.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		public static void SetCurrentScene(string sceneName)// Establece la escena actual
		{
			instance.SetCurrent(sceneName);
		}

		/// <summary>
		/// <para>Determina si se ha cargado una escena</para>
		/// </summary>
		/// <returns><c>true</c> si esta cargada; sino, <c>false</c>.</returns>
		/// <param name="sceneName">Nombre escena.</param>
		public static bool IsSceneLoaded(string sceneName)// Determina si se ha cargado una escena
		{
			return instance.IsLoaded(sceneName);
		}

		/// <summary>
		/// <para>Carga una escena.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena.</param>
		public static void LoadScene(string sceneName)// Carga una escena
		{
			instance.Load(sceneName);
		}

		/// <summary>
		/// <para>Descarga una escena.</para>
		/// </summary>
		/// <param name="sceneName">Nombre escena</param>
		public static void UnloadScene(string sceneName)// Descarga una escena
		{
			instance.Unload(sceneName);
		}
		#endregion

		#region Eventos
		private delegate void InternalLoadedHandler(string sceneName, int distance);
		#endregion
	}
}