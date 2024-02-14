using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DRC.RPG.Utils
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

        private GameObject _objectPoolEmptyHolder;
        private static GameObject _gameObjectsEmpty;
        public enum PoolType
        {
            GameObject,
            None,
            Ghost,
            Agent
        }

        public static PoolType PoolingType;
        private void Awake()
        {
            SetupEntities();
        }

        private void SetupEntities()
        {
            _objectPoolEmptyHolder = new GameObject("Pool Objects");

            _gameObjectsEmpty = new GameObject("GameObject");
            _gameObjectsEmpty.transform.SetParent(_objectPoolEmptyHolder.transform);
        }

        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.None)
        {
            PooledObjectInfo pool = ObjectPools.Find(p => p.LookUpString == objectToSpawn.name);

            if (pool == null)
            {
                pool = new PooledObjectInfo() { LookUpString = objectToSpawn.name };
                ObjectPools.Add(pool);
            }

            GameObject spawnableObject = pool.InactiveObjects.FirstOrDefault();
            if (spawnableObject == null)
            {
                GameObject parentObject = SetParentObject(poolType);

                spawnableObject = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

                if (parentObject != null)
                {
                    spawnableObject.transform.SetParent(parentObject.transform);
                }
            }
            else
            {
                spawnableObject.transform.position = spawnPosition;
                spawnableObject.transform.rotation = spawnRotation;
                pool.InactiveObjects.Remove(spawnableObject);
                spawnableObject.SetActive(true);
            }
            return spawnableObject;
        }
        public static void ReturnObjectToPool(GameObject obj)
        {
            string goName = obj.name.Substring(0, obj.name.Length - 7); // Se elimina el (Clone)

            PooledObjectInfo pool = ObjectPools.Find(p => p.LookUpString == goName);

            if (pool == null)
            {
                Debug.LogWarning("Trying to release an object that is not pooled: " + obj.name);
            }
            pool.InactiveObjects.Add(obj);
            obj.SetActive(false);
        }

        private static GameObject SetParentObject(PoolType poolType)
        {
            switch (poolType)
            {
                case PoolType.GameObject:
                    return _gameObjectsEmpty;
                case PoolType.None:
                    return null;
                default:
                    return null;
            }
        }
    }

    public class PooledObjectInfo
    {
        public string LookUpString;
        public List<GameObject> InactiveObjects = new List<GameObject>();
    }
}