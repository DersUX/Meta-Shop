using Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Logon.UI.Views
{
	public class LoadingBarView : MonoBehaviour
	{
		private const string ProgressTextFormat = "{0}%";

		[SerializeField] private RectTransform _fillImage;
		[SerializeField] private RectMask2D _fillMask;
		[SerializeField] private TMP_Text _progressLabel;

		private float _progress;

		private void Awake()
		{
			Utils.AssertIsNotNull(_fillImage);
			Utils.AssertIsNotNull(_fillMask);
			Utils.AssertIsNotNull(_progressLabel);

			SetProgress(0f);
		}

		public void SetProgress(float progress)
		{
			_progress = Mathf.Clamp01(progress);

			UpdateFillMask();

			_progressLabel.text = string.Format(ProgressTextFormat, Mathf.FloorToInt(_progress * 100f));
		}

		private void OnEnable()
		{
			if (_fillImage != null && _fillMask != null)
				UpdateFillMask();
		}

		private void OnRectTransformDimensionsChange()
		{
			if (_fillImage != null && _fillMask != null)
				UpdateFillMask();
		}

		private void UpdateFillMask()
		{
			float fill = _fillImage.rect.width * (1f - _progress);
			_fillMask.padding = new Vector4(0f, 0f, fill, 0f);

			_fillImage.gameObject.SetActive(_progress > 0f);
		}
	}
}
