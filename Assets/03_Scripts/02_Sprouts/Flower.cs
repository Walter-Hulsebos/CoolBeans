using System.Collections;

using UnityEngine;
using Random = UnityEngine.Random;

using JetBrains.Annotations;

using ExtEvents;

using DG.Tweening;
using UnityEngine.Serialization;
using F32  = System.Single;
using F32x3 = Unity.Mathematics.float3;
using I32  = System.Int32;

using Bool = System.Boolean;

namespace CoolBeans.Sprouts
{
    public class Flower : MonoBehaviour
    {
        [SerializeField] private GameObject visuals;

        [SerializeField] private GameObject sproutPrefab;

        [SerializeField] private Vector2 spawnTimeMinMax = new(x: 0.2f, 1f);

        [SerializeField] private ExtEvent onSpawn;

        [SerializeField] private ExtEvent onBlossom;

        private F32 _spawnTime;

        private IEnumerator Start()
        {
            visuals.SetActive(false);
            visuals.transform.localScale = Vector3.zero;
            
            yield return new WaitForSeconds(Random.Range(spawnTimeMinMax.x, spawnTimeMinMax.y));
            
            visuals.SetActive(true);
            visuals.transform.DOScale(Vector3.one * 0.5f, duration: 0.5f).SetEase(Ease.OutBack).onComplete += () => onSpawn.Invoke();
        }

        [PublicAPI]
        public void Blossom()
        {
            Instantiate(original: sproutPrefab, position: transform.position, rotation: transform.rotation);
            
            onBlossom.Invoke();
            
            Destroy(gameObject);
        }
    }
}
