using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{

    #region Class definitions

    [System.Serializable]
    internal class PoolObject
    {
        internal GameObject _object;
        internal PooledObject objectScript;
        internal PoolObjectSettings objectSettings = new PoolObjectSettings();
        internal string type = "";

        internal void ActivatePoolObject()
        {
            if (this._object != null)
            {
                if (this.objectSettings != null)
                {
                    if (string.IsNullOrEmpty(this.objectSettings.objectName) == false)
                    {
                        this._object.name = this.objectSettings.objectName;
                    }
                    if (this.objectSettings.parentToSet != null)
                    {
                        this._object.transform.SetParent(this.objectSettings.parentToSet.transform);
                    }

                    if (this.objectSettings.setLocalPositionAndRotation && this.objectSettings.parentToSet != null)
                    {
                        this._object.transform.localPosition = this.objectSettings.positionToSet;
                        this._object.transform.localRotation = this.objectSettings.rotationToSet;
                    }
                    else if (!this.objectSettings.setLocalPositionAndRotation)
                    {
                        this._object.transform.position = this.objectSettings.positionToSet;
                        this._object.transform.rotation = this.objectSettings.rotationToSet;
                    }
                }

                this._object.SetActive(true);

                if (this.objectSettings != null)
                {
                    Rigidbody objectBody = this._object.GetComponent<Rigidbody>();

                    if (this.objectSettings.setRigidBodyForce && objectBody != null)
                    {
                        objectBody.velocity = new Vector3();
                        objectBody.angularVelocity = new Vector3();
                        objectBody.AddForce(this.objectSettings.rigidbodyForceToSet, this.objectSettings.forceModeToSend);
                    }

                    if (this.objectSettings.actionToCallWhenActivated != null)
                    {
                        this.objectSettings.actionToCallWhenActivated();
                    }

                    if (this.objectScript != null)
                    {
                        this.objectScript.SetPoolSettings(this.objectSettings);
                    }
                }
            }
        }

        internal void DeActivatePoolObject(GameObject defaultParent)
        {
            if (this._object != null)
            {
                if (this._object.transform.parent != defaultParent)
                {
                    if (defaultParent != null)
                    {
                        this._object.transform.SetParent(defaultParent.transform);
                    }
                    else
                    {
                        this._object.transform.SetParent(null);
                    }
                }

                this._object.SetActive(false);
            }
            else
            {
                Debug.Log("_object was null!");
            }

            this.objectSettings = null;
        }

    }

    [System.Serializable]
    public class PoolPrefab
    {
        public string type = "";
        public GameObject prefab;
        public int objectsToPreSpawnInBattle = 0;
    }

    public class TypePoolManager
    {
        public GameObject defaultParent;
        public string typeName;
        public PoolPrefab poolPrefabToUse;
        private List<PoolObject> _activePool = new List<PoolObject>();
        private List<PoolObject> _waitingForActivationPool = new List<PoolObject>();
        private List<PoolObject> _idlePool = new List<PoolObject>();

        internal bool CheckIfPreSpawnIsEnabled()
        {
            if (this.poolPrefabToUse != null)
            {
                return this.poolPrefabToUse.objectsToPreSpawnInBattle > 0;
            }

            return false;
        }

        internal void PreSpawnPoolObjects()
        {
            if (this.poolPrefabToUse != null && this.poolPrefabToUse.objectsToPreSpawnInBattle > 0 && this._idlePool.Count < this.poolPrefabToUse.objectsToPreSpawnInBattle)
            {
                for (int i = 0; i <= this.poolPrefabToUse.objectsToPreSpawnInBattle; i++)
                {
                    InstantiateNewPooledObject();
                }
            }
        }

        internal void ClearPool()
        {
            for (int i = 0; i < this._activePool.Count; i++)
            {
                GameObject objectToCheck = this._activePool[i]._object;
                if (objectToCheck != null)
                {
                    Destroy(objectToCheck);
                }
            }

            this._activePool = new List<PoolObject>();

            for (int i = 0; i < this._waitingForActivationPool.Count; i++)
            {
                GameObject objectToCheck = this._waitingForActivationPool[i]._object;
                if (objectToCheck != null)
                {
                    Destroy(objectToCheck);
                }
            }

            this._waitingForActivationPool = new List<PoolObject>();

            for (int i = 0; i < this._idlePool.Count; i++)
            {
                GameObject objectToCheck = this._idlePool[i]._object;
                if (objectToCheck != null)
                {
                    Destroy(objectToCheck);
                }
            }

            this._idlePool = new List<PoolObject>();
        }

        internal void ReturnAllObjectsToPool()
        {
            for (int i = 0; i < this._activePool.Count; i++)
            {
                PoolObject poolObject = this._activePool[i];

                if (poolObject != null && poolObject._object != null)
                {
                    poolObject.DeActivatePoolObject(this.defaultParent);
                    this._idlePool.Add(poolObject);
                }
            }

            this._activePool = new List<PoolObject>();

            for (int i = 0; i < this._waitingForActivationPool.Count; i++)
            {
                PoolObject poolObject = this._waitingForActivationPool[i];

                if (poolObject != null && poolObject._object != null)
                {
                    poolObject.DeActivatePoolObject(this.defaultParent);
                    this._idlePool.Add(poolObject);
                }
            }

            this._waitingForActivationPool = new List<PoolObject>();
        }

        internal GameObject GetObjectFromPool(PoolObjectSettings settings)
        {
            if (settings != null)
            {
                PoolObject poolObject = TakeObjectFromPool();

                if (poolObject != null)
                {
                    poolObject.objectSettings = settings;
                    settings.startTime = Time.time;

                    if (settings != null && settings.appearanceTime <= 0)
                    {
                        this._idlePool.Remove(poolObject);
                        this._activePool.Add(poolObject);
                        poolObject.ActivatePoolObject();
                    }
                    else
                    {
                        this._idlePool.Remove(poolObject);
                        this._waitingForActivationPool.Add(poolObject);
                    }

                    return poolObject._object;
                }
            }

            return null;
        }

        internal GameObject GetAndActivateObjectFromPool(PoolObjectSettings settings)
        {
            if (settings != null)
            {
                PoolObject poolObject = TakeObjectFromPool();

                if (poolObject != null)
                {
                    poolObject.objectSettings = settings;
                    settings.startTime = Time.time;
                    this._idlePool.Remove(poolObject);
                    poolObject.ActivatePoolObject();
                    this._activePool.Add(poolObject);
                    return poolObject._object;
                }
            }

            Debug.Log("Got null from pool!");
            return null;
        }

        internal void ReturnObjectBackToPool(GameObject returnedObject)
        {
            if (this._activePool != null && returnedObject != null)
            {
                PoolObject objectToReturnBack = null;
                for (int i = 0; i < this._activePool.Count; i++)
                {
                    if (this._activePool[i]._object == returnedObject)
                    {
                        objectToReturnBack = this._activePool[i];
                        break;
                    }
                }

                if (objectToReturnBack != null)
                {
                    this._idlePool.Add(objectToReturnBack);
                    this._activePool.Remove(objectToReturnBack);
                    objectToReturnBack.DeActivatePoolObject(this.defaultParent);

                    if (this.defaultParent != null && objectToReturnBack._object != null)
                    {
                        objectToReturnBack._object.transform.SetParent(this.defaultParent.transform);
                    }
                }
                else
                {
                    Debug.Log("failed to get objectToReturn!");
                }
            }
        }

        internal void CheckForActivationsInPool()
        {
            if (this._waitingForActivationPool != null)
            {
                List<PoolObject> moveToActiveList = new List<PoolObject>();

                for (int i = 0; i < this._waitingForActivationPool.Count; i++)
                {
                    PoolObject objectToCheck = this._waitingForActivationPool[i];
                    if (objectToCheck != null)
                    {
                        float timeToAppear = objectToCheck.objectSettings.startTime + objectToCheck.objectSettings.appearanceTime - Time.time;
                        if (timeToAppear < 0)
                        {
                            moveToActiveList.Add(objectToCheck);
                        }
                    }
                }

                for (int i = 0; i < moveToActiveList.Count; i++)
                {
                    PoolObject objectToMove = moveToActiveList[i];
                    this._waitingForActivationPool.Remove(objectToMove);
                    this._activePool.Add(objectToMove);
                    objectToMove.ActivatePoolObject();
                }
            }
        }

        private PoolObject TakeObjectFromPool()
        {
            PoolObject objectToTake = null;
            if (this._idlePool.Count > 0)
            {
                for (int i = this._idlePool.Count - 1; i >= 0; i--)
                {
                    if (this._idlePool[i] == null || this._idlePool[i]._object == null)
                    {
                        this._idlePool.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        objectToTake = this._idlePool[i];
                        break;
                    }
                }
            }

            if (objectToTake == null)
            {
                objectToTake = InstantiateNewPooledObject();
            }

            return objectToTake;
        }

        private PoolObject InstantiateNewPooledObject()
        {
            PoolObject objectToTake = new PoolObject();
            GameObject gameObjectToCreate = Instantiate(this.poolPrefabToUse.prefab);
            objectToTake._object = gameObjectToCreate;
            objectToTake.type = this.typeName;

            PooledObject pooledObjectScript = gameObjectToCreate.GetComponent<PooledObject>();

            if (pooledObjectScript == null)
            {
                pooledObjectScript = gameObjectToCreate.AddComponent<PooledObject>();
            }

            objectToTake.objectScript = pooledObjectScript;

            if (!string.IsNullOrEmpty(this.typeName))
            {
                pooledObjectScript.poolTypeString = this.typeName;
            }
            else
            {
                pooledObjectScript.poolBasePrefab = this.poolPrefabToUse.prefab;
            }

            if (this.defaultParent == null)
            {
                this.defaultParent = new GameObject();

                if (!string.IsNullOrEmpty(this.typeName))
                {
                    this.defaultParent.name = this.typeName + "_Parent";
                }
                else if (this.poolPrefabToUse != null && this.poolPrefabToUse.prefab != null)
                {
                    this.defaultParent.name = this.poolPrefabToUse.prefab.name + "_Parent";
                }
            }

            objectToTake.DeActivatePoolObject(this.defaultParent);
            this._idlePool.Add(objectToTake);

            return objectToTake;
        }

    }

    #endregion

    private static PoolManager _instance;
    internal static PoolManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public List<PoolPrefab> poolPrefabList = new List<PoolPrefab>();
    public List<PoolPrefab> poolNetworkPrefabList = new List<PoolPrefab>();

    private Dictionary<string, TypePoolManager> poolTypeManagerDictionary = new Dictionary<string, TypePoolManager>();
    private Dictionary<GameObject, TypePoolManager> poolPrefabManagerDictionary = new Dictionary<GameObject, TypePoolManager>();

    private Dictionary<NetworkHash128, TypePoolManager> networkedPoolTypeManagerDictionary = new Dictionary<NetworkHash128, TypePoolManager>();

    private List<TypePoolManager> activePoolManagers = new List<TypePoolManager>();

    private void Awake()
    {
        if (!CreateSingleton())
        {
            return;
        }
        else
        {
            if (this.poolTypeManagerDictionary == null)
            {
                this.poolTypeManagerDictionary = new Dictionary<string, TypePoolManager>();
            }

            if (this.poolTypeManagerDictionary.Count == 0)
            {
                for (int i = 0; i < this.poolPrefabList.Count; i++)
                {
                    PoolPrefab prefabToAdd = this.poolPrefabList[i];

                    if (prefabToAdd != null && prefabToAdd.prefab != null && !string.IsNullOrEmpty(prefabToAdd.type))
                    {
                        if (!this.poolTypeManagerDictionary.ContainsKey(prefabToAdd.type))
                        {
                            TypePoolManager poolManager = new TypePoolManager()
                            {
                                poolPrefabToUse = prefabToAdd,
                                typeName = prefabToAdd.type
                            };

                            this.poolTypeManagerDictionary.Add(prefabToAdd.type, poolManager);
                            this.activePoolManagers.Add(poolManager);
                        }
                    }
                }
            }

            if (this.networkedPoolTypeManagerDictionary == null)
            {
                this.networkedPoolTypeManagerDictionary = new Dictionary<NetworkHash128, TypePoolManager>();
            }

            if (this.networkedPoolTypeManagerDictionary.Count == 0)
            {
                for (int i = 0; i < this.poolNetworkPrefabList.Count; i++)
                {
                    PoolPrefab prefabToAdd = this.poolNetworkPrefabList[i];

                    if (prefabToAdd != null && prefabToAdd.prefab != null)
                    {
                        NetworkIdentity networkIdentity = prefabToAdd.prefab.GetComponent<NetworkIdentity>();

                        if (networkIdentity != null)
                        {
                            NetworkHash128 assetId = networkIdentity.assetId;

                            if (!this.networkedPoolTypeManagerDictionary.ContainsKey(assetId))
                            {
                                TypePoolManager poolManager = new TypePoolManager()
                                {
                                    poolPrefabToUse = prefabToAdd,
                                };

                                this.networkedPoolTypeManagerDictionary.Add(assetId, poolManager);
                                this.activePoolManagers.Add(poolManager);

                                ClientScene.RegisterSpawnHandler(assetId, GetNetworkedPooledObject, ReturnNetworkedObjectToPool);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += CheckForPrespawningObjects;
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= CheckForPrespawningObjects;
        }
    }

    private void Update()
    {
        if (this.activePoolManagers != null)
        {
            for (int i = 0; i < this.activePoolManagers.Count; i++)
            {
                TypePoolManager manager = this.activePoolManagers[i];
                if (manager != null)
                {
                    manager.CheckForActivationsInPool();
                }
            }
        }
    }

    private bool CreateSingleton()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return false;
        }
        if (!transform.parent)
        {
            DontDestroyOnLoad(gameObject);
        }

        _instance = this;
        return true;
    }

    private void CheckForPrespawningObjects(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(waitAndCheckIfPreSpawn());
    }

    public GameObject GetPooledObject(string type)
    {
        return GetPooledObject(type, new PoolObjectSettings());
    }

    public GameObject GetPooledObject(string type, PoolObjectSettings settings)
    {
        if (this.poolTypeManagerDictionary.ContainsKey(type))
        {
            return this.poolTypeManagerDictionary[type].GetObjectFromPool(settings);
        }

        Debug.Log("Call to non existant type of Pool Manager with string: " + type);
        return null;
    }

    public GameObject GetPooledObject(GameObject prefabToUse, PoolObjectSettings settings)
    {
        if (prefabToUse != null)
        {
            if (this.poolPrefabManagerDictionary.ContainsKey(prefabToUse))
            {
                return this.poolPrefabManagerDictionary[prefabToUse].GetObjectFromPool(settings);
            }
            else
            {
                TypePoolManager typePoolManager = new TypePoolManager();
                typePoolManager.poolPrefabToUse = new PoolPrefab()
                {
                    prefab = prefabToUse
                };
                this.activePoolManagers.Add(typePoolManager);
                this.poolPrefabManagerDictionary.Add(prefabToUse, typePoolManager);
                return typePoolManager.GetObjectFromPool(settings);
            }
        }
        else
        {
            Debug.Log("Call to PoolManager with null prefab!");
        }

        return null;
    }

    public GameObject GetNetworkedPooledObject(Vector3 position, NetworkHash128 assetId)
    {
        if (this.networkedPoolTypeManagerDictionary.ContainsKey(assetId))
        {
            return this.networkedPoolTypeManagerDictionary[assetId].GetAndActivateObjectFromPool(new PoolObjectSettings() { positionToSet = position });
        }
        else
        {
            Debug.Log("Call to non existant type of Pool Manager!");
            return null;
        }
    }

    public bool ReturnObjectToPool(string type, GameObject objectToReturn)
    {
        if (this.poolTypeManagerDictionary.ContainsKey(type))
        {
            this.poolTypeManagerDictionary[type].ReturnObjectBackToPool(objectToReturn);
            return true;
        }
        else
        {
            Debug.Log("Call to non existant type of Pool Manager!");
            return false;
        }
    }

    public bool ReturnObjectToPool(GameObject prefabToUse, GameObject objectToReturn)
    {
        if (this.poolPrefabManagerDictionary.ContainsKey(prefabToUse))
        {
            this.poolPrefabManagerDictionary[prefabToUse].ReturnObjectBackToPool(objectToReturn);
            return true;
        }
        else
        {
            Debug.Log("Call to non existant type of Pool Manager!");
            return false;
        }
    }

    public void ReturnNetworkedObjectToPool(GameObject objectToReturn)
    {
        if (objectToReturn != null)
        {
            NetworkIdentity networkIdentity = objectToReturn.GetComponent<NetworkIdentity>();

            if (networkIdentity != null)
            {
                if (this.networkedPoolTypeManagerDictionary.ContainsKey(networkIdentity.assetId))
                {
                    this.networkedPoolTypeManagerDictionary[networkIdentity.assetId].ReturnObjectBackToPool(objectToReturn);
                }
                else
                {
                    Debug.Log("Call to non existant type of Pool Manager!");
                }
            }
        }
    }

    public void ReturnAllObjectsToPools()
    {
        if (this.activePoolManagers != null)
        {
            for (int i = 0; i < this.activePoolManagers.Count; i++)
            {
                TypePoolManager poolManager = this.activePoolManagers[i];
                if (poolManager != null)
                {
                    poolManager.ReturnAllObjectsToPool();
                }
            }
        }
    }

    public void ClearAllPools()
    {
        if (this.activePoolManagers != null)
        {
            for (int i = 0; i < this.activePoolManagers.Count; i++)
            {
                TypePoolManager poolManager = this.activePoolManagers[i];
                if (poolManager != null)
                {
                    poolManager.ClearPool();
                }
            }
        }
    }

    private IEnumerator waitAndCheckIfPreSpawn()
    {
        yield return new WaitForSeconds(1f);

        if (true) // Modified to this since we dont have menus or anything currently.
        {
            for (int i = 0; i < this.activePoolManagers.Count; i++)
            {
                TypePoolManager poolManager = this.activePoolManagers[i];
                if (poolManager != null)
                {
                    if (poolManager.CheckIfPreSpawnIsEnabled())
                    {
                        poolManager.ClearPool();
                        poolManager.PreSpawnPoolObjects();
                    }
                    else
                    {
                        poolManager.ClearPool();
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < this.activePoolManagers.Count; i++)
            {
                TypePoolManager poolManager = this.activePoolManagers[i];
                if (poolManager != null)
                {
                    poolManager.ClearPool();
                }
            }
        }
    }

    /// <summary>
    /// Tries to return the object to their pool, if they are part of a pool. Otherwise, destroys the gameobject.
    /// </summary>
    /// <param name="gameObject">Object that we want to try and return to the pool.</param>
    public static void ReturnObjectToPoolOrDestroyIt(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        if (PoolManager.Instance == null)
        {
            Destroy(gameObject);
            return;
        }

        PooledObject pooledObjectScript = gameObject.GetComponent<PooledObject>();

        if (pooledObjectScript)
        {
            pooledObjectScript.ReturnPooledObjectBackToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

}

[System.Serializable]
public class PoolObjectSettings
{
    internal string objectName;
    internal float startTime;
    internal float appearanceTime = 0f;
    internal Vector3 positionToSet = new Vector3();
    internal Quaternion rotationToSet = new Quaternion();
    internal GameObject parentToSet = null;
    internal bool setLocalPositionAndRotation = false;
    internal float timeBeforeReturningToPool = 0; // If this is zero or lower, it is never automatically returned to the pool, but has to be manually done.
    internal Action actionToCallWhenActivated;
    internal bool setRigidBodyForce = false;
    internal Vector3 rigidbodyForceToSet = new Vector3();
    internal ForceMode forceModeToSend = ForceMode.Impulse;

}