# Implementation Log - Fix Battle Transition Crash

## Step 1: Modify GameManager.cs for Persistence and Duplication Cleanup
- Awake: Detach and always DontDestroyOnLoad.
- OnSceneLoaded: Identify and destroy duplicate EventSystems/AudioListeners.
- LoadScene: Use LoadSceneAsync.

## Step 2: Ensure QuestManager uses Destroy(gameObject)
- Fix Singleton logic.

## Step 3: Ensure ProceduralSlash Shader Safety
- Add fallback shader logic to prevent null materials.

## Step 4: Stabilize TempleIntroController
- Disable CinemachineBrain before scene change.
