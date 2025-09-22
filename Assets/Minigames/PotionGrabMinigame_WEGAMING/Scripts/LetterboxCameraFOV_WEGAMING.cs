// Author: Tony Giang
// Retrieved from https://tonysgiang.blogspot.com/2021/09/a-better-way-to-get-letterboxing-in.html

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This component keeps a target orthographic camera view fit for UGUI
/// letterboxing in accordance to the aspect ratio of the fitter. If the new
/// aspect ratio is wider than the fitter's aspect ratio, the target camera's
/// vertical orthographic size will be kept consistent. If the new aspect ratio
/// is taller than the fitter's aspect ratio, the target camera's vertical FOV
/// will be stretched.
/// </summary>
[ExecuteAlways]
public class AutoLetterboxCameraOrthographicSizeFitter : MonoBehaviour
{
  public AspectRatioFitter Fitter = null;
  public Camera TargetCamera = null;
  public static float CurrentRatio => (float)Screen.width / Screen.height;
  public float ReferenceVerticalOrthographicSize;

  void OnRectTransformDimensionsChange()
  {
    if (CurrentRatio < Fitter.aspectRatio)
      TargetCamera.orthographicSize = ReferenceVerticalOrthographicSize
        * Fitter.aspectRatio / CurrentRatio;
    else TargetCamera.orthographicSize = ReferenceVerticalOrthographicSize;
  }
}
