using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SaveFile : MonoBehaviour
{
	public static SaveFile current;

	public PlayerInfo playerInfo;

	public bool AutoSave = true;

	private string file = "/savedata.json";

	private float timer = 0f;
	private float interval = 30f;

	void Start()
	{
		current = this;

		file = Application.persistentDataPath + file;

		playerInfo = new PlayerInfo("Ashe");

		ReadFromJson();
	}

	void Update()
	{
		if (AutoSave)
		{
			if (timer < Time.time)
			{
				timer = Time.time + interval;

				SaveToJson();
			}
		}
	}

	public void SaveToJson()
	{
		playerInfo.Prepare();

		string json = JsonUtility.ToJson(playerInfo);

		try
		{
			File.WriteAllText(file, json);
			Log(file + " has been saved!");
		}
		catch (System.Exception e)
		{
			Log(e.ToString());
		}
	}

	public void ReadFromJson()
	{
		string f = File.ReadAllText(file);

		f.Trim();

		playerInfo = JsonUtility.FromJson<PlayerInfo>(f);

		playerInfo.Unzip();

		Log(file + " has been loaded!");

		Log("Contents: \n" + JsonUtility.ToJson(playerInfo));
	}

	string GetJsonFileLocation()
	{
		return file;
	}

	void Log(string t)
	{
		GetComponent<UnityEngine.UI.Text>().text += "\n" + t;
	}
}

public class PlayerInfo
{
	private List<PokemonInfo> pokemons;

	public string name;

	public int level;

	public float experience;

	public float expNeeded;

	public string pstring;

	public PlayerInfo(string name, int level = 1, float experience = 0f)
	{
		this.name = name;

		this.level = level;

		this.experience = experience;

		this.expNeeded = (this.level + 1) * 50f;

		this.pokemons = new List<PokemonInfo>();
	}

	public void Prepare()
	{
		pstring = "";

		PokemonInfo[] p = this.pokemons.ToArray();

		for (int i = 0; i < p.Length; i++)
		{
			pstring += "/" + JsonUtility.ToJson(p[i]);
		}
	}

	public void Unzip()
	{
		string[] s = pstring.Split('/');

		this.pokemons = new List<PokemonInfo>();

		for (int i = 1; i < s.Length; i++)
		{
			PokemonInfo p = JsonUtility.FromJson<PokemonInfo>(s[i]);

			this.pokemons.Add(p);
		}
	}

	public void AddPokemon(PokemonType type, int level, float experience = 0f, int cookies = 3)
	{
		PokemonInfo p = new PokemonInfo(type, level, cookies, experience);

		PokemonInfo r = this.GetPokemonByType(type);

		if (r == null)
		{
			this.pokemons.Add(p);
		}
		else
		{
			r.AddCookies(cookies + 1);

			if (r.level < level)
			{
				r.level = level;
			}
		}

		this.Prepare();
	}

	public PokemonInfo GetPokemonByType(PokemonType type)
	{
		PokemonInfo[] p = this.pokemons.ToArray();

		for (int i = 0; i < p.Length; i++)
		{
			if (p[i].type == type)
			{
				return p[i];
			}
		}

		return null;
	}
}

public class PokemonInfo
{
	public PokemonType type;

	public int level;

	public int cookies;

	public float experience;

	public float expNeeded;

	public PokemonInfo(PokemonType type, int level, int cookies = 3, float experience = 0f)
	{
		this.type = type;

		this.level = level;

		this.cookies = cookies;

		this.experience = experience;

		this.expNeeded = (this.level + 1) * 50f;
	}

	public void AddExperience(float exp)
	{
		experience += exp;

		if (experience > expNeeded)
		{
			float rest = experience - expNeeded;

			experience = rest;

			level++;
		}
	}

	public void AddCookies(int cookies)
	{
		this.cookies += cookies;
	}
}
