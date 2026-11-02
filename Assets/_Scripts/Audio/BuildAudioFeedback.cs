using UnityEngine;

/// <summary>
/// Placement controller calls exactly one method AFTER validating/committing placement.
/// Do not play success in Tower.Start: preplaced/preview towers are not purchases.
/// </summary>
public class BuildAudioFeedback : MonoBehaviour
{
    public void PlacementSucceeded() => AudioManager.Instance?.PlayTowerPlace();
    public void PlacementRejected() => AudioManager.Instance?.PlayBuildFailed();
}
