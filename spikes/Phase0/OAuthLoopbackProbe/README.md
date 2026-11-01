# Phase 0 Microsoft OAuth Loopback Probe

This probe validates a system-browser authorization-code flow with PKCE, a localhost callback, and an in-memory token response check. It is not the launcher login implementation. Tokens are never printed or persisted.

## Prerequisites

- Register your own Microsoft public-client app that supports personal Microsoft accounts (consumer authority).
- Add the exact loopback redirect `http://localhost:8400/` under the **Mobile and desktop applications** platform. Ensure the app supports public-client authorization-code flow.
- Allow the delegated `User.Read` permission if required by the app.
- Build with .NET 10 SDK.

Do not use a client ID copied from an unrelated third-party app. Client IDs are public identifiers, but only use an app registration you control or are authorized to test. Never paste passwords, authorization codes, access tokens, or refresh tokens into chat or logs.

## Run in the Ubuntu desktop session

```bash
dotnet run --project spikes/Phase0/OAuthLoopbackProbe/OAuthLoopbackProbe.csproj -c Release
```

In the same desktop terminal, set the public client ID without committing it:

```bash
export PHASE0_MS_CLIENT_ID='YOUR-APP-CLIENT-ID'
export PHASE0_MS_REDIRECT_URI='http://localhost:8400/'
dotnet run --project spikes/Phase0/OAuthLoopbackProbe/OAuthLoopbackProbe.csproj -c Release
```

The probe listens on the exact redirect URI `http://localhost:8400/` by default, opens the system browser, validates the OAuth `state`, exchanges the authorization code using PKCE, and reports only whether the token response was valid and its lifetime. If your app registration uses another loopback port, set `PHASE0_MS_REDIRECT_URI` to that exact registered URI (including the trailing slash) before running. It requests `openid profile offline_access User.Read` as a generic Microsoft identity smoke test; it does not validate Minecraft/Xbox token exchange. The token response body is never printed. Tokens remain in memory and are not persisted, so this validates callback and exchange—not the production token-store integration.

The callback listener times out after three minutes. The browser sign-in must be initiated by the user in the VM desktop; do not run the interactive flow over a headless SSH shell.

### Troubleshooting `invalid_request` for `redirect_uri`

- In the app registration identified by `PHASE0_MS_CLIENT_ID`, open **Authentication** and verify `http://localhost:8400/` is listed under **Mobile and desktop applications**, not under **Web**.
- Confirm the probe is using that same client ID and that `PHASE0_MS_REDIRECT_URI` is either unset or exactly `http://localhost:8400/`.
- If the app registration uses a different loopback URI, set `PHASE0_MS_REDIRECT_URI` to that exact value and register the same URI for the app's mobile/desktop platform.
- Retry after saving the registration. Do not share the full authorization URL or any callback URL containing an authorization code.

## Run the deployed Ubuntu build

The current build is at `/home/user/phase0-oauth-loopback-probe-20261101/OAuthLoopbackProbe`. In a terminal opened from the Ubuntu GNOME desktop, register the exact redirect URI in your app registration, then run:

```bash
cd /home/user/phase0-oauth-loopback-probe-20261101
export PHASE0_MS_CLIENT_ID='YOUR-APP-CLIENT-ID'
export PHASE0_MS_REDIRECT_URI='http://localhost:8400/'
./OAuthLoopbackProbe
```

Complete sign-in in the opened browser. The probe reports callback and token-exchange status only; do not copy credentials, authorization codes, or tokens into chat.
