using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.Network;
using UnityEngine;

namespace Store.Services
{
	public class FilePendingPurchaseStore : IPendingPurchaseStore
	{
		private const string EmptyPurchaseText = "Saved purchase is empty.";
		private const string PurchaseOwnerRequiredText = "Purchase owner is required.";

		private readonly string _purchasesUrl;
		private readonly string _directoryPath;

		public FilePendingPurchaseStore(IBackendConfig backend)
		{
			_purchasesUrl = (backend ?? throw new ArgumentNullException(nameof(backend))).PurchasesUrl;
			_directoryPath = Path.Combine(Application.persistentDataPath, "PendingPurchases");
		}

		public PendingPurchase Load(int playerId, string login)
		{
			string path = GetPurchasePath(playerId, login);
			string json;

			try
			{
				json = File.ReadAllText(path);
			}
			catch (FileNotFoundException)
			{
				return null;
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}

			PendingPurchase purchase = JsonUtility.FromJson<PendingPurchase>(json);

			if (purchase == null)
				throw new InvalidOperationException(EmptyPurchaseText);

			purchase.Validate();

			return purchase;
		}

		public void Save(int playerId, string login, PendingPurchase purchase)
		{
			if (purchase == null)
				throw new ArgumentNullException(nameof(purchase));

			purchase.Validate();

			string path = GetPurchasePath(playerId, login);
			byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(purchase));
			WriteAtomically(path, bytes);
		}

		public void Clear(int playerId, string login) => File.Delete(GetPurchasePath(playerId, login));

		private void WriteAtomically(string path, byte[] bytes)
		{
			Directory.CreateDirectory(_directoryPath);
			string temporaryPath = path + ".tmp";

			using (FileStream stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				stream.Write(bytes, 0, bytes.Length);
				stream.Flush(true);
			}

			if (File.Exists(path))
				File.Replace(temporaryPath, path, null);
			else
				File.Move(temporaryPath, path);
		}

		private string GetPurchasePath(int playerId, string login)
		{
			if (playerId <= 0 || string.IsNullOrWhiteSpace(login))
				throw new ArgumentException(PurchaseOwnerRequiredText);

			using SHA256 sha256 = SHA256.Create();

			string key = _purchasesUrl + "\n" + playerId + "\n" + login;
			byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
			string fileName = BitConverter.ToString(hash).Replace("-", string.Empty) + ".json";

			return Path.Combine(_directoryPath, fileName);
		}
	}
}
