using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StarFieldBehavior : MonoBehaviour {
    
    private List<Star> Stars = new List<Star>();
    private List<Asterism> Asterisms = new List<Asterism>();

    private ParticleSystem.Particle[] points;
    private GameObject starFieldAnchor;

    public ParticleSystem starParticleSystem;
    public TextAsset starsTextAsset;
    public TextAsset asterismsTextAsset;
    public Material asterismsMaterial;


    private void CreateStars()
    {
        starFieldAnchor = GameObject.Find("StarFieldAnchor");
        Stars.Clear();
        
        char[] splitChars = new char[] { ' ' };

        using (var memStream = new MemoryStream(starsTextAsset.bytes))
        {
            using (var reader = new StreamReader(memStream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] lineArr = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (lineArr.Length < 6) { continue; }

                    var star = new Star();

                    star.Index = Convert.ToInt32(lineArr[0]);
                    star.Ra = Mathf.Deg2Rad * Convert.ToSingle(lineArr[1]);
                    star.Dec = Mathf.Deg2Rad * Convert.ToSingle(lineArr[2]);
                    star.Distance = 500; //Convert.ToSingle(lineArr[3]);
                    star.Mag = Convert.ToSingle(lineArr[4]);
                    star.type = lineArr[5];

                    //if (star.Mag < 3)
                    //{
                    Stars.Add(star);
                    //}
                }
            }
        }

        using (var memStream = new MemoryStream(asterismsTextAsset.bytes))
        {
            using (var reader = new StreamReader(memStream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] lineArr = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (lineArr.Length < 4) { continue; }

                    var asterism = new Asterism();
                    Asterisms.Add(asterism);

                    asterism.Name = lineArr[0];

                    asterism.NumSegments = Convert.ToInt32(lineArr[1]);
                    asterism.StarsA = new Star[asterism.NumSegments];
                    asterism.StarsB = new Star[asterism.NumSegments];

                    int index = 2;
                    for (int i = 0; i < asterism.NumSegments; i++)
                    {
                        int a = Convert.ToInt32(lineArr[index++]);
                        int b = Convert.ToInt32(lineArr[index++]);

                        asterism.StarsA[i] = Stars.Find(s => s.Index == a);
                        asterism.StarsB[i] = Stars.Find(s => s.Index == b);
                    }

                    Debug.Log("Processed asterism: " + asterism.Name);
                }
            }
        }


        points = new ParticleSystem.Particle[Stars.Count];

        for (int i = 0; i < points.Length; i++)
        {
            var star = Stars[i];

            points[i].position = new Vector3(star.Distance * Mathf.Cos(star.Dec) * Mathf.Cos(star.Ra),
                star.Distance * Mathf.Cos(star.Dec) * Mathf.Sin(star.Ra),
                star.Distance * Mathf.Sin(star.Dec));


            //points[i].startSize = 30f;

            float size =  15f - star.Mag * 2;
            if (size < 5f) { size = 5f; }
            //points[i].startColor = new Color(1, 1, 1, 1);
            points[i].startSize3D = new Vector3(size, size, size);

            star.particle = points[i];
        }
        
        starParticleSystem.SetParticles(points, points.Length);



        foreach (var asterism in Asterisms)
        {
            
            for (int i = 0; i < asterism.NumSegments; i++)
            {
                if (asterism.StarsA[i] == null || asterism.StarsB[i] == null)
                {
                    continue;
                }
                
                var lineObj = new GameObject();
                lineObj.transform.parent = starFieldAnchor.transform;

                var lineRenderer = lineObj.AddComponent<LineRenderer>();

                lineRenderer.material = asterismsMaterial;
                lineRenderer.startWidth = 1f;
                lineRenderer.endWidth = 1f;
                lineRenderer.SetPosition(0, asterism.StarsA[i].particle.position);
                lineRenderer.SetPosition(1, asterism.StarsB[i].particle.position);
                lineRenderer.startColor = new Color(0f, 0.2f, 0.7f);
                lineRenderer.endColor = new Color(0f, 0.2f, 0.7f);

                Debug.Log("Adding line: " + asterism.StarsA[i].particle.position + "  :  " + asterism.StarsB[i].particle.position);

            }
        }

    }


    // Use this for initialization
    void Start () {
        CreateStars();

    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public class Star
    {
        public int Index;
        public float Ra;
        public float Dec;
        public float Distance;
        public float Mag;
        public string type;

        public ParticleSystem.Particle particle;
    }


    public class Asterism
    {
        public string Name;

        public int NumSegments;
        public int[] IndexA;
        public int[] IndexB;

        public Star[] StarsA;
        public Star[] StarsB;
    }
}
