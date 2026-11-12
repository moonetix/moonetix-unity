# Offerwall demo

1. Open `OfferwallDemo.unity`.
2. Select the "Moonetix Offerwall Demo" object and fill in:
   - **App Id**: your Moonetix app ID from the Integration tab of your app.
   - **Token Url**: an endpoint on your own server that creates a session with
     `POST https://api.moonetix.com/v1/session` (with your API key) and returns `{"token":"sst_..."}`.
     The demo sends `user_id` as a form field.
3. Build and run on an Android phone or an iPhone, and tap **Earn coins**.

You can also paste a token that you created with curl into the text field and tap
**Open with this token**. A token works once and for 10 minutes.

In the Editor the buttons only check the app ID and token and report `UNSUPPORTED`: the offerwall
opens on Android and iOS devices.

The demo uses the built-in IMGUI and UnityWebRequest modules.
