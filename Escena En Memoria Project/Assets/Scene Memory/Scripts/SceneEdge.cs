//                                  ┌∩┐(◣_◢)┌∩┐
//                                                                              \\
// SceneEdge.cs (27/06/2017)													\\
// Autor: Antonio Mateo (Moon Antonio) 									        \\
// Descripcion:		Control que carga la escena siguiente						\\
// Fecha Mod:		27/06/2017													\\
// Ultima Mod:	    Version Inicial												\\
//******************************************************************************\\

#region Librerias
using UnityEngine;
using System.Collections.Generic;
#endregion

namespace MoonAntonio.SceneMemory
{
	/*	NOTAS
		Este controlador informa a SceneStreamer acerca de una escena vecina. Generalmente
		agregar a un trigger en el borde de una escena. Cuando el jugador entra en el borde,
		por ejemplo al entrar en el borde de una escena vecina, la escena del borde cargara la escena.
	*/

	/// <summary>
	/// <para>Control que carga la escena siguiente.</para>
	/// </summary>
	[AddComponentMenu("Scene Memory/Scene Edge")]
	public class SceneEdge : MonoBehaviour
	{
		#region Variables Publicas
		/// <summary>
		/// <para>La actual escena raiz.</para>.
		/// </summary>
		[Tooltip("El root gameobject de la escena")]
		public GameObject currentSceneRoot;                                     // La actual escena raiz
		/// <summary>
		/// <para>El nombre de la siguiente escena en el otro lado</para>
		/// </summary>
		[Tooltip("El nombre de la siguiente escena en el otro lado")]
		public string nextSceneName;											// El nombre de la siguiente escena en el otro lado
		/// <summary>
		/// <para>Las tags que acepta para cargar a la siguiente escena.</para>
		/// </summary>
		public List<string> acceptedTags = new List<string>() { "Player" };		// Las tags que acepta para cargar a la siguiente escena
		#endregion

		#region Eventos Triggers
		/// <summary>
		/// <para>Cuando el tag entra en el trigger, carga la siguiente escena</para>
		/// </summary>
		/// <param name="other">Colisionador</param>
		public void OnTriggerEnter(Collider other)// Cuando el tag entra en el trigger, carga la siguiente escena
		{
			CheckEdge(other.tag);
		}

		/// <summary>
		/// <para>Cuando el tag entra en el trigger, carga la siguiente escena</para>
		/// </summary>
		/// <param name="other">Colisionador</param>
		public void OnTriggerEnter2D(Collider2D other)// Cuando el tag entra en el trigger, carga la siguiente escena
		{
			CheckEdge(other.tag);
		}
		#endregion

		#region Metodos
		/// <summary>
		/// <para>Comprueba si se puede cargar la escena</para>
		/// </summary>
		/// <param name="otherTag">Tags aceptadas</param>
		private void CheckEdge(string otherTag)// Comprueba si se puede cargar la escena
		{
			if (acceptedTags == null || acceptedTags.Count == 0 || acceptedTags.Contains(otherTag))
			{
				SetCurrentScene();
			}
		}

		/// <summary>
		/// <para>Seleccionar la escena actual</para>
		/// </summary>
		private void SetCurrentScene()// Seleccionar la escena actual
		{
			if (currentSceneRoot) SceneStreamer.SetCurrentScene(currentSceneRoot.name);
		}
		#endregion
	}
}