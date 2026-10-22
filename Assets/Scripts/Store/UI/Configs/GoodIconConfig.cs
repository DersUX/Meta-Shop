using System.Collections.Generic;
using Store.Models;
using UnityEngine;

namespace Store.UI.Configs
{
	[CreateAssetMenu(fileName = "GoodIconConfig", menuName = "Store/Good Icon Config")]
	public class GoodIconConfig : ScriptableObject, IGoodIconConfig
	{
		[SerializeField] private List<GoodIconEntry> _icons = new List<GoodIconEntry>();

		public Sprite GetIcon(GoodType type)
		{
			foreach (GoodIconEntry entry in _icons)
			{
				if (entry.Type == type)
					return entry.Icon;
			}

			return null;
		}
	}
}
