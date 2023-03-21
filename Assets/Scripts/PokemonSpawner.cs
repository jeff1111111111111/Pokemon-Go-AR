using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PokemonSpawner : MonoBehaviour
{
	void Start()
	{
		string t = PlayerPrefs.GetString("POKEMON_KEY");

		GameObject prefab = Resources.Load("CatchPokemon/" + t, typeof(GameObject)) as GameObject;

		GameObject pokemon = Instantiate(prefab, transform.position, Quaternion.identity) as GameObject;

		pokemon.transform.SetParent(transform);

		pokemon.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);

		PlayerPrefs.DeleteKey("POKEMON_KEY");
	}

	public static void Run()
	{
		SceneManager.LoadScene("World");
	}
}
