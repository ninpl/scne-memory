//                                  ┌∩┐(◣_◢)┌∩┐
//                                                                              \\
// SetStartScene.cs (27/06/2017)												\\
// Autor: Antonio Mateo (Moon Antonio) 									        \\
// Descripcion:		Fija la escena actual en el Start							\\
// Fecha Mod:		27/06/2017													\\
// Ultima Mod:	    Version Inicial												\\
//******************************************************************************\\

#region Libreria
using UnityEngine;
#endregion

namespace MoonAntonio.SceneMemory
{
	/// <summary>
	/// <para>Fija la escena actual en el Start</para>
	/// </summary>
	[AddComponentMenu("Scene Memory/Set Start Scene")]
	public class SetStartScene : MonoBehaviour 
	{
		#region Variables Publicas
		/// <summary>
		/// <para>El nombre de la escena a cargar en Inicio.</para>
		/// </summary>
		[Tooltip("Carga esta escena en el Start")]
		public string startSceneName = "Scene 1";                               // El nombre de la escena a cargar en Inicio
		#endregion

		#region Inicializadores
		/// <summary>
		/// <para>Inicializador de <see cref="SetStartScene"/>.</para>
		/// </summary>
		public void Start()// Inicializador de SetStartScene
		{
			SceneStreamer.SetCurrentScene(startSceneName);
			Destroy(this);
		}
		#endregion
	}
}
