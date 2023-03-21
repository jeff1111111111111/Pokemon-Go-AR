using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class PokemonManager : MonoBehaviour
{
	private TileManager tileManager;

	[SerializeField]
	private float waitSpawnTime, minIntervalTime, maxIntervalTime;

	private List<Pokemon> pokemons = new List<Pokemon>();

	void Start()
	{
		tileManager = GameObject.FindGameObjectWithTag("TileManager").GetComponent<TileManager>();
	}

	void Update()
	{
		if (waitSpawnTime < Time.time)
		{
			waitSpawnTime = Time.time + UnityEngine.Random.Range(minIntervalTime, maxIntervalTime);

			SpawnPokemon();
		}

		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Stationary)
		{
			RaycastHit hit;

			Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

			if (Physics.Raycast(ray, out hit, 100f))
			{
				if (hit.transform.tag == "Pokemon")
				{
					Pokemon pokemon = hit.transform.GetComponent<Pokemon>();

					PokemonBattle(pokemon.pokeType);
				}
			}
		}
	}

	void PokemonBattle(PokemonType type)
	{
		string t = type.ToString();

		PlayerPrefs.SetString("POKEMON_KEY", t);

		SceneManager.LoadScene("Catch");
	}

	void SpawnPokemon()
	{
		PokemonType type = (PokemonType)(int)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PokemonType)).Length);

		float newLat = tileManager.getLat + UnityEngine.Random.Range(-0.0001f, 0.0001f);

		float newLon = tileManager.getLon + UnityEngine.Random.Range(-0.0001f, 0.0001f);

		Pokemon prefab = Resources.Load("MapPokemon/" + type.ToString(), typeof(Pokemon)) as Pokemon;

		Pokemon pokemon = Instantiate(prefab, Vector3.zero, Quaternion.identity) as Pokemon;

		pokemon.tileManager = tileManager;

		pokemon.Init(newLat, newLon);

		pokemons.Add(pokemon);
	}

	public void UpdatePokemonPosition()
	{
		if (pokemons.Count == 0)

			return;
		Pokemon[] pokemon = pokemons.ToArray();

		for (int i = 0; i < pokemon.Length; i++)
		{
			pokemon[i].UpdatePosition();
		}
	}
}
