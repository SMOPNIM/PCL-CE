# Phase 0 Secret Store Probe

This isolated test probe exercises Secret Service access and an AES-256-GCM file fallback. It is not production code and is not part of the launcher. It defines a local `ISecretStore` interface because the production repository does not yet have one.

The probe stores only a generated synthetic value. It never accepts or prints a real password/token. The fallback's random key and ciphertext are both user-readable files under a `0700` directory, with each file set to `0600`; this validates the planned permission and round-trip behavior, but is not production-grade key isolation.

## Build on Windows

```powershell
dotnet publish spikes/Phase0/SecretStoreProbe/SecretStoreProbe.csproj `
  -c Release -r linux-x64 --self-contained true `
  -o artifact/phase0/secret-store-probe-linux-x64
```

Transfer the publish output to Ubuntu and run the executable from its directory.

## Secret Service path

Install `libsecret-tools` on Ubuntu if `secret-tool` is missing:

```bash
sudo apt install libsecret-tools
```

In the logged-in GNOME desktop session, run:

```bash
./SecretStoreProbe store
./SecretStoreProbe read
./SecretStoreProbe clear
```

`store` and `read` should both report `backend=SecretService` and the same fingerprint. `clear` removes the test item from the keyring.

The probe was tested against the Ubuntu 22.04.5 GNOME session bus on 2026-11-01. The first call waited while the VM was locked; after unlocking, a repeated store/read/clear cycle succeeded.

## Fallback path

From an SSH shell, run with a deliberately unavailable session bus. This does not stop or alter the active desktop keyring:

```bash
DBUS_SESSION_BUS_ADDRESS=unix:path=/tmp/pcl-phase0-no-session-bus ./SecretStoreProbe store
DBUS_SESSION_BUS_ADDRESS=unix:path=/tmp/pcl-phase0-no-session-bus ./SecretStoreProbe read
DBUS_SESSION_BUS_ADDRESS=unix:path=/tmp/pcl-phase0-no-session-bus ./SecretStoreProbe clear
stat -c '%a %n' ~/.local/share/pcl-phase0-secret-store-probe \
  ~/.local/share/pcl-phase0-secret-store-probe/*
```

Expected: the first two commands warn that Secret Service is unavailable, report `backend=AesFallback`, and show matching fingerprints. `stat` should show `700` for the directory and `600` for both files while the fallback data exists. The final `clear` removes the probe files.

This isolated-bus fallback path was also tested on Ubuntu 22.04.5 on 2026-11-01.

If running from a shell that already has the desktop session bus, explicitly set the unavailable address as shown so the test cannot silently use the desktop keyring. Do not stop `gnome-keyring-daemon` in the active desktop session.
