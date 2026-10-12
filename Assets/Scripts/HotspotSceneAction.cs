using UnityEngine;

public class HotspotSceneAction : MonoBehaviour
{
    public enum ActionType { NextScene, FirstScene, ExitGame, RestartScene }
    [SerializeField] private ActionType _actionToTrigger;

    public void ExecuteAction()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance not found in scene!");
            return;
        }

        switch (_actionToTrigger)
        {
            case ActionType.NextScene:
                GameManager.Instance.NextScene();
                break;
            case ActionType.FirstScene:
                GameManager.Instance.FirstScene();
                break;
            case ActionType.RestartScene:
                GameManager.Instance.RestartScene();
                break;
            case ActionType.ExitGame:
                GameManager.Instance.ExitGame();
                break;
        }
    }
}