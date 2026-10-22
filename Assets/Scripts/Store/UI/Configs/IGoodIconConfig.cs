using Store.Models;
using UnityEngine;

namespace Store.UI.Configs
{
	public interface IGoodIconConfig
	{
		Sprite GetIcon(GoodType type);
	}
}
