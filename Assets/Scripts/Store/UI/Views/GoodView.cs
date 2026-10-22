using System;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Store.UI.Views
{
	public class GoodView : MonoBehaviour
	{
		private const string PriceTextFormat = "<sprite name=\"icon_coin\"> {0}";

		[SerializeField] private TMP_Text _headerText;
		[SerializeField] private TMP_Text _descriptionText;
		[SerializeField] private TMP_Text _priceText;
		[SerializeField] private Image _image;
		[SerializeField] private Button _buyButton;

		private int _id;

		public event Action<int> BuyClicked;

		private void Awake()
		{
			Utils.AssertIsNotNull(_headerText);
			Utils.AssertIsNotNull(_descriptionText);
			Utils.AssertIsNotNull(_priceText);
			Utils.AssertIsNotNull(_image);
			Utils.AssertIsNotNull(_buyButton);

			_buyButton.onClick.AddListener(Buy);
		}

		private void OnDestroy()
		{
			if (_buyButton != null)
				_buyButton.onClick.RemoveListener(Buy);
		}

		public void SetPurchaseInteractable(bool interactable) => _buyButton.interactable = interactable;

		public void SetData(int id, string header, string description, string price, Sprite icon)
		{
			_id = id;

			_headerText.text = header;
			_descriptionText.text = description;
			_priceText.text = string.Format(PriceTextFormat, price);
			_image.sprite = icon;
		}

		private void Buy()
		{
			if (_buyButton.interactable)
				BuyClicked?.Invoke(_id);
		}
	}
}
