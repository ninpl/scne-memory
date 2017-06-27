//                                  ┌∩┐(◣_◢)┌∩┐
//                                                                              \\
// NeighboringScenes.cs (27/06/2017)											\\
// Autor: Antonio Mateo (Moon Antonio) 									        \\
// Descripcion:		Enumera los vecinos de la escena.							\\
// Fecha Mod:		27/06/2017													\\
// Ultima Mod:	    Version Inicial												\\
//******************************************************************************\\

#region Libreria
using UnityEngine;
#endregion

namespace MoonAntonio.SceneMemory
{
	/*	NOTAS
		Añada esto al objeto raiz de la escena. Enumera los vecinos de la escena.
		SceneStreamer lo usa para determinar que vecinos cargar y descargar.
		Si el objeto raiz de la escena no tiene este componente, SceneStreamer
		lo generara automaticamente en tiempo de carga, lo que tarda un poco de tiempo.
	*/

	/// <summary>
	/// <para>Enumera los vecinos de la escena.</para>
	/// </summary>
	[AddComponentMenu("Scene Memory/Neighboring Scenes")]
	public class NeighboringScenes : MonoBehaviour
	{
		#region Variables Publicas
		/// <summary>
		/// <para>Las proximas escenas</para>
		/// </summary>
		[Tooltip("Las proximas escenas")]
		public string[] sceneNames;											// Las proximas escenas
		#endregion
	}
}