using R3;
using UnityEngine;

namespace NestopiSystem;

public class NativeProxy : MonoBehaviour
{
	private readonly Subject<string> onGoogleSignOut = new Subject<string>();

	private readonly Subject<string> onGetIDToken = new Subject<string>();

	private readonly Subject<string> onGetIDTokenFailed = new Subject<string>();

	public Observable<string> OnGoogleSignOut => onGoogleSignOut;

	public Observable<string> OnGetIDToken => onGetIDToken;

	public Observable<string> OnGetIDTokenFailed => onGetIDTokenFailed;

	public void GoogleSignOut(string errorCode)
	{
		UnityEngine.Debug.Log("[NativeProxy] GoogleSignOut: errorCode" + errorCode);
		onGoogleSignOut.OnNext(errorCode);
	}

	public void GetIDToken(string idToken)
	{
		UnityEngine.Debug.Log("[NativeProxy] GetIDToken: idToken" + idToken);
		onGetIDToken.OnNext(idToken);
	}

	public void GetIDTokenFailed(string errorCode)
	{
		UnityEngine.Debug.Log("[NativeProxy] GetIDTokenFailed: errorCode" + errorCode);
		onGetIDTokenFailed.OnNext(errorCode);
	}
}
