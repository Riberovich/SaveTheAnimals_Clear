using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BalloonSpawner : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private BalloonSimUI sim;
    [SerializeField] private RectTransform balloonsRoot;
    [SerializeField] private RectTransform animal;
    [SerializeField] private RectTransform ropeStartPoint;
    [SerializeField] private SaveTheAnimalController controller;

    [Header("Prefabs")]
    [SerializeField] private RectTransform balloonPrefab;
    [SerializeField] private RopeImageUI ropePrefab;

    [Header("Visual Data")]
    [SerializeField] private BalloonVisualSetSO visualSet;
    [SerializeField] private BalloonPatternPresetSO[] patternPresets;

    [Header("Pattern Selection")]
    [SerializeField] private bool pickRandomPreset = true;
    [SerializeField] private BalloonPatternPresetSO fallbackPreset;

    [Header("Bouquet Placement")]
    [SerializeField] private Vector2 bouquetCenter = new Vector2(0f, 260f);
    [SerializeField] private bool clearExisting = true;

    [Header("Pop Audio")]
    [SerializeField] private AudioSource popSfxSource;
    [SerializeField] private AudioClip[] popClips;

    [Header("Pop VFX")]
    [SerializeField] private GameObject popVfxPrefab;
    [SerializeField] private Canvas vfxCanvas;
    [SerializeField] private Camera worldCameraForSprites;

    [Header("Test Auto Spawn")]
    [SerializeField] private bool autoSpawnOnStart = true;
    [SerializeField] private int startCount = 6;

    private void Start()
    {
        if (autoSpawnOnStart)
            StartCoroutine(SpawnRoutine(startCount));
    }

    public void Spawn(int count)
    {
        StartCoroutine(SpawnRoutine(count));
    }

    private IEnumerator SpawnRoutine(int count)
    {
        if (balloonsRoot == null) balloonsRoot = transform as RectTransform;
        if (sim == null) sim = GetComponent<BalloonSimUI>();

        if (balloonPrefab == null || ropePrefab == null)
        {
            Debug.LogError("BalloonSpawner: Assign balloonPrefab and ropePrefab.");
            yield break;
        }

        BalloonPatternPresetSO preset = GetPresetForCount(count);
        if (preset == null || !preset.IsValid())
        {
            Debug.LogError($"BalloonSpawner: No valid pattern preset found for count = {count}.");
            yield break;
        }

        if (clearExisting)
        {
            for (int i = balloonsRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = balloonsRoot.GetChild(i);
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            yield return null;
        }

        Sprite[] chosenSprites = BuildSpriteSequence(count);

        for (int i = 0; i < preset.localPositions.Length; i++)
        {
            Vector2 pos = bouquetCenter + preset.localPositions[i];

            pos.x += Random.Range(-preset.jitterX, preset.jitterX);
            pos.y += Random.Range(-preset.jitterY, preset.jitterY);

            Sprite sprite = (chosenSprites != null && i < chosenSprites.Length) ? chosenSprites[i] : null;
            float scaleMul = GetScaleMultiplier(preset);

            CreatePair(i, pos, sprite, scaleMul);
        }

        yield return null;

        if (sim != null)
        {
            sim.animal = animal;
            sim.ropeStartPoint = ropeStartPoint;
            sim.RebuildFromChildren();
        }

        if (controller != null)
        {
            controller.BindBalloonsFromRoot(balloonsRoot);
        }
    }

    private void CreatePair(int idx, Vector2 pos, Sprite balloonSprite, float scaleMul)
    {
        var rope = Instantiate(ropePrefab, balloonsRoot);
        rope.name = $"Rope_{idx + 1}";
        rope.startPoint = ropeStartPoint;

        var balloon = Instantiate(balloonPrefab, balloonsRoot);
        balloon.name = idx == 0 ? "Balloon" : $"Balloon ({idx})";
        balloon.anchoredPosition = pos;
        balloon.localRotation = Quaternion.identity;
        balloon.localScale = Vector3.one * scaleMul;

        var end = balloon.Find("RopeEndPoint");
        if (end == null)
        {
            Debug.LogError("BalloonSpawner: RopeEndPoint not found under Balloon prefab.");
        }
        else
        {
            rope.endPoint = end as RectTransform;
        }

        ApplySprite(balloon, balloonSprite);
        ConfigureBalloonTap(balloon);
    }

    private void ApplySprite(RectTransform balloon, Sprite sprite)
    {
        if (sprite == null) return;

        var img = balloon.GetComponent<Image>();
        if (img == null) return;

        img.sprite = sprite;
        img.preserveAspect = true;
    }

    private void ConfigureBalloonTap(RectTransform balloon)
    {
        var tap = balloon.GetComponent<BalloonTap>();
        if (tap == null) return;

        tap.sfxSource = popSfxSource;
        tap.popClips = popClips;
        tap.popVfxPrefab = popVfxPrefab;
        tap.vfxCanvas = vfxCanvas;
        tap.worldCameraForSprites = worldCameraForSprites;
    }

    private BalloonPatternPresetSO GetPresetForCount(int count)
    {
        List<BalloonPatternPresetSO> matches = new List<BalloonPatternPresetSO>();

        if (patternPresets != null)
        {
            foreach (var p in patternPresets)
            {
                if (p == null) continue;
                if (!p.IsValid()) continue;
                if (p.balloonCount != count) continue;

                matches.Add(p);
            }
        }

        if (matches.Count > 0)
        {
            if (!pickRandomPreset || matches.Count == 1)
                return matches[0];

            return matches[Random.Range(0, matches.Count)];
        }

        if (fallbackPreset != null && fallbackPreset.IsValid() && fallbackPreset.balloonCount == count)
            return fallbackPreset;

        return null;
    }

    private Sprite[] BuildSpriteSequence(int count)
    {
        if (visualSet == null) return null;
        if (visualSet.balloonSprites == null || visualSet.balloonSprites.Length == 0) return null;

        Sprite[] result = new Sprite[count];

        if (visualSet.allowRepeats)
        {
            for (int i = 0; i < count; i++)
            {
                result[i] = visualSet.balloonSprites[Random.Range(0, visualSet.balloonSprites.Length)];
            }

            return result;
        }

        List<Sprite> pool = new List<Sprite>();
        foreach (var s in visualSet.balloonSprites)
        {
            if (s != null) pool.Add(s);
        }

        if (pool.Count == 0) return null;

        for (int i = 0; i < count; i++)
        {
            int pickIndex = Mathf.Min(i, pool.Count - 1);

            if (i < pool.Count)
            {
                int randomIndex = Random.Range(i, pool.Count);
                (pool[i], pool[randomIndex]) = (pool[randomIndex], pool[i]);
                result[i] = pool[i];
            }
            else
            {
                // not enough unique sprites -> reuse last random from available pool
                result[i] = pool[Random.Range(0, pool.Count)];
            }
        }

        return result;
    }

    private float GetScaleMultiplier(BalloonPatternPresetSO preset)
    {
        if (preset == null || !preset.useScaleVariation)
            return 1f;

        float min = Mathf.Min(preset.scaleRange.x, preset.scaleRange.y);
        float max = Mathf.Max(preset.scaleRange.x, preset.scaleRange.y);

        return Random.Range(min, max);
    }
}