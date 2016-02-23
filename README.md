Odnoklassniki Unity SDK
=============

This SDK allows you to connect your Unity **Android** and **iOS** with Odnoklassniki.

Application Requirements
-------

An application registered within OK platform should have:

1. Target platform checked (like ANDROID or IOS)
2. EXTERNAL platform checked
3. Client OAUTH checkbox checked
4. A VALUABLE_ACCESS permission being checked or requested

Setup
-------

1. Fill in application parameters in `OdnoklassnikiSettings.asset`
2. Add the following entries to your `android.manifest` 

* within **application** tag
```
<activity android:name="ru.odnoklassniki.unity.OKAndroidPlugin"
    android:label="@string/app_name"
    android:hardwareAccelerated="true"
    android:windowSoftInputMode="adjustResize"
    android:configChanges="fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen">
    <intent-filter>
        <data android:scheme="okauth" android:host="INSERT_OK_APPLICATION_ID" />
    </intent-filter>
    <meta-data android:name="android.app.lib_name" android:value="unity" />
    <meta-data android:name="unityplayer.ForwardNativeEventsToDalvik" android:value="true" />
</activity>
<activity android:name="ru.odnoklassniki.unity.webview.OKWVActivity"
    android:label="@string/app_name"
    android:hardwareAccelerated="true"
    android:windowSoftInputMode="adjustResize"
    android:configChanges="fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen">
    <meta-data android:name="android.app.lib_name" android:value="unity" />
    <meta-data android:name="unityplayer.ForwardNativeEventsToDalvik" android:value="true" />
</activity>
```
where **INSERT_OK_APPLICATION_ID** should be in format: `ok1234567890`

* within **manifest** tag
```
<activity android:name="ru.odnoklassniki.unity.auth.AppAuthorization"/>
```

Initialization
-------
```
OK.Init(success =>
{
    if (success) {
        //Proceed to authorization
    }
});
```

Authorization
-------
```
OK.Auth(success =>
{
    if (success) {
        //Authorization successful, you can now use Odnoklassniki API
    }
});
```

There are 2 types of authorization based on whether native Odnoklassniki application ([AppStore](https://itunes.apple.com/app/odnoklassniki/id398465290) / [Google Play](https://play.google.com/store/apps/details?id=ru.ok.android)) is installed: authorization via native application & authorization via webview.

Once authorized, you will receive an access token lasting for 30 minutes
```
OK.AccessToken
```

If you authorized via native Odnoklassniki application, you will also receive refresh token lasting for 30 days, which can be used to revalidate access token
```
OK.IsRefreshTokenValid
```

You need to take care of refreshing the access token
```
if (OK.isInitialized && OK.AccessTokenExpiresAt < DateTime.Now) {
    if (OK.IsRefreshTokenValid) {
        OK.RefreshAccessToken(success =>
        {
            //Token refreshed
        });
    } else {
        OK.RefreshOAuth(success => {
            //Token refreshed
        }
    }
}
```

Using widgets
-------

#### Invite Widget

```
OK.OpenInviteDialog(response => {
    //Will be called after Invite API call
}, "Invite Message");
```

#### Suggest Widget

```
OK.OpenSuggestDialog(response => {
    //Will be called after Suggest API call
}, "Suggest Message");
```

#### Photo Upload Widget

```
OK.OpenPhotoDialog(response => {
    //Will be called after Upload API call
}, texture, "Description");
```

#### Publish Widget

```
OK.OpenPublishDialog(response => {
    //Will be called after Publish API call
}, OKMedia.Photo(texture, "Description"));
```

#### Handling errors

```
response => {
    if (response.Object != null && response.Object.ContainsKey("error_code")) {
        string errorCode = response.Object["error_code"].ToString();
        string errorMessage = response.Object["error_msg"].ToString();
        //Debug.Log or show alert?
    } else {
        //Success
    }
}
```

F.A.Q.
-------
Is there a convenient way to see if Odnoklassniki widget is shown?
> OKWidgets.HasActiveWidget() method does that.


