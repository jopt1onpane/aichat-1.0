using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Firebase.Internal;

internal static class HttpHelpers
{
	internal static async Task SetRequestHeaders(HttpRequestMessage request, FirebaseApp firebaseApp)
	{
		request.Headers.Add("x-goog-api-key", firebaseApp.Options.ApiKey);
		string versionInfoSdkVersion = FirebaseInterops.GetVersionInfoSdkVersion();
		request.Headers.Add("x-goog-api-client", "gl-csharp/8.0 fire/" + versionInfoSdkVersion);
		if (FirebaseInterops.GetIsDataCollectionDefaultEnabled(firebaseApp))
		{
			request.Headers.Add("X-Firebase-AppId", firebaseApp.Options.AppId);
			request.Headers.Add("X-Firebase-AppVersion", Application.version);
		}
		await FirebaseInterops.AddFirebaseTokensAsync(request, firebaseApp);
	}

	internal static async Task ValidateHttpResponse(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
		{
			return;
		}
		string text = "No error content available.";
		if (response.Content != null)
		{
			try
			{
				text = await response.Content.ReadAsStringAsync();
			}
			catch (Exception ex)
			{
				text = "Failed to read error content: " + ex.Message;
			}
		}
		HttpRequestException ex2 = new HttpRequestException($"HTTP request failed with status code: {(int)response.StatusCode} ({response.ReasonPhrase}).\n" + "Error Content: " + text, null);
		ex2.Data["StatusCode"] = response.StatusCode;
		throw ex2;
	}
}
