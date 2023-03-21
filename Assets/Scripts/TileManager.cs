using UnityEngine;

using System;
using System.Collections;

public class TileManager : MonoBehaviour
{
	[SerializeField]
	private Settings _settings;

	[SerializeField]
	private Transform target;

	[SerializeField]
	private Texture2D texture;
	private GameObject tile;

	private float oldLat = 0f, oldLon = 0f;
	private float lat = 0f, lon = 0f;

	public PokemonManager pokeManager;

	public float getLat
	{
		get
		{
			return lat;
		}
	}

	public float getLon
	{
		get
		{
			return lon;
		}
	}

	IEnumerator Start()
	{
		while (!Input.location.isEnabledByUser)
		{
			print("Activate gps");

			yield return new WaitForSeconds(1f);
		}

		Input.location.Start(10f, 5f);

		int maxWait = 20;

		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			yield return new WaitForSeconds(1);

			maxWait--;
		}

		if (maxWait < 1)
		{
			print("Timed out");

			yield break;
		}

		if (Input.location.status == LocationServiceStatus.Failed)
		{
			print("Unable to determine device location");

			yield break;
		}
		else
		{
			print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
		}

		lat = Input.location.lastData.latitude;

		lon = Input.location.lastData.longitude;

		StartCoroutine(loadTiles(_settings.zoom));

		while (Input.location.isEnabledByUser)
		{
			yield return new WaitForSeconds(1f);
		}

		yield return StartCoroutine(Start());

		yield break;
	}
	//the map dont work
	public IEnumerator loadTiles(int zoom)
	{
		int size = _settings.size;

		string key = _settings.key;

		string style = _settings.style;

		lat = Input.location.lastData.latitude;

		lon = Input.location.lastData.longitude;

		string url = String.Format("https://api.mapbox.com/v4/mapbox.{5}/{0},{1},{2}/{3}x{3}@2x.png?access_token={4}", lon, lat, zoom, size, key, style);

		WWW www = new WWW(url);

		yield return www;

		texture = www.texture;

		if (tile == null)
		{
			tile = GameObject.CreatePrimitive(PrimitiveType.Plane);

			tile.name = "Tile " + lat + "" + lon;

			tile.transform.localScale = Vector3.one * _settings.scale;

			tile.GetComponent<Renderer>().material = _settings.material;

			tile.transform.parent = transform;
		}

		if (oldLat != 0 && oldLon != 0)
		{
			float x, y;

			Vector3 position = Vector3.zero;

			geodeticOffsetInv(lat * Mathf.Deg2Rad, lon * Mathf.Deg2Rad, oldLat * Mathf.Deg2Rad, oldLon * Mathf.Deg2Rad, out x, out y);

			if ((oldLat - lat) < 0 && (oldLon - lon) > 0 || (oldLat - lat) > 0 && (oldLon - lon) < 0)
			{
				position = new Vector3(x, .5f, y);
			}
			else
			{
				position = new Vector3(-x, .5f, -y);
			}

			position.x *= 0.300122f;

			position.z *= 0.123043f;

			target.position = position;



			float ps = 10 * _settings.scale / _settings.size;

			x *= ps;
			y *= ps;

			Debug.Log(x + " / " + y);
		}

		pokeManager.UpdatePokemonPosition();

		tile.GetComponent<Renderer>().material.mainTexture = texture;

		yield return new WaitForSeconds(1f);

		oldLat = lat;
		oldLon = lon;

		while (oldLat == lat && oldLon == lon)
		{
			lat = Input.location.lastData.latitude;
			lon = Input.location.lastData.longitude;

			yield return new WaitForSeconds(0.5f);
		}

		yield return new WaitUntil(() => (oldLat != lat || oldLon != lon));

		yield return StartCoroutine(loadTiles(_settings.zoom));

		yield break;
	}

	float[] px(float lat, float lon, int zoom)
	{
		float d = (_settings.size / 2 * Mathf.Pow(2, _settings.zoom));

		float f = Mathf.Min(Mathf.Max(Mathf.Sin(Mathf.Deg2Rad * lat), -0.9999f), 0.9999f);

		float x = Mathf.Round(d + lon * (_settings.size / 360 * Mathf.Pow(2, _settings.zoom)));

		float y = Mathf.Round(d + 0.5f * Mathf.Log((1 + f) / (1 - f)) * (-(_settings.size / (2 * Mathf.PI) * Mathf.Pow(2, _settings.zoom))));

		float[] result = new float[2];

		result[0] = x;
		result[1] = y;

		return result;
	}

	float semiMajorAxis = 6378137.000000f;
	float TranMercB = 6356752.314245f;
	float geocentF = 0.003352810664f;

	void geodeticOffsetInv(float refLat, float refLon,
		float lat, float lon,
		out float xOffset, out float yOffset)
	{
		float a = semiMajorAxis;
		float b = TranMercB;
		float f = geocentF;

		float L = lon - refLon;

		float U1 = Mathf.Atan((1 - f) * Mathf.Tan(refLat));

		float U2 = Mathf.Atan((1 - f) * Mathf.Tan(lat));

		float sinU1 = Mathf.Sin(U1);

		float cosU1 = Mathf.Cos(U1);

		float sinU2 = Mathf.Sin(U2);

		float cosU2 = Mathf.Cos(U2);

		float lambda = L;

		float lambdaP;

		float sinSigma;

		float sigma;

		float cosSigma;

		float cosSqAlpha;

		float cos2SigmaM;

		float sinLambda;

		float cosLambda;

		float sinAlpha;

		int iterLimit = 100;

		do
		{
			sinLambda = Mathf.Sin(lambda);

			cosLambda = Mathf.Cos(lambda);

			sinSigma = Mathf.Sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +
				(cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) *
				(cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));

			if (sinSigma == 0)
			{
				xOffset = 0.0f;
				yOffset = 0.0f;

				return;
			}
			cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;

			sigma = Mathf.Atan2(sinSigma, cosSigma);

			sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;

			cosSqAlpha = 1 - sinAlpha * sinAlpha;

			cos2SigmaM = cosSigma - 2 * sinU1 * sinU2 / cosSqAlpha;

			if (cos2SigmaM != cos2SigmaM)
			{
				cos2SigmaM = 0;
			}
			float C = f / 16 * cosSqAlpha * (4 + f * (4 - 3 * cosSqAlpha));

			lambdaP = lambda;

			lambda = L + (1 - C) * f * sinAlpha *
				(sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));

		}
		while (Mathf.Abs(lambda - lambdaP) > 1e-12 && --iterLimit > 0);

		if (iterLimit == 0)
		{
			xOffset = 0.0f;
			yOffset = 0.0f;

			return;
		}

		float uSq = cosSqAlpha * (a * a - b * b) / (b * b);

		float A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));

		float B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));

		float deltaSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) -
			B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));

		float s = b * A * (sigma - deltaSigma);

		float bearing = Mathf.Atan2(cosU2 * sinLambda, cosU1 * sinU2 - sinU1 * cosU2 * cosLambda);

		xOffset = Mathf.Sin(bearing) * s;

		yOffset = Mathf.Cos(bearing) * s;
	}

	void Update()
	{
		target.position = Vector3.Lerp(target.position, new Vector3(0, .5f, 0), 2.0f * Time.deltaTime);
	}

	[Serializable]
	public class Settings
	{
		[SerializeField]
		public Vector2 centerPosition;

		[SerializeField]
		public Material material;

		[SerializeField]
		public int zoom = 18;

		[SerializeField]
		public int size = 640;

		[SerializeField]
		public float scale = 1f;

		[SerializeField]
		public int range = 1;

		[SerializeField]
		public string key = "KEY_HERE";

		[SerializeField]
		public string style = "emerald";
	}
}