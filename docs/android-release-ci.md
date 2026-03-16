# Android release CI

The repository now contains `.github/workflows/android-release.yml`.

It does the following when you run the workflow manually from GitHub Actions:

- creates and pushes a git tag named `v<version>`
- restores the MAUI Android toolchain
- builds a signed Android App Bundle (`.aab`)
- creates a GitHub release
- uploads the signed bundle, its SHA-256 checksum, and the ProGuard/R8 `mapping.txt` file when available

## Required GitHub secrets

- `ANDROID_KEYSTORE_BASE64`: your signing keystore encoded in base64
- `ANDROID_KEYSTORE_PASSWORD`: the keystore password
- `ANDROID_KEY_ALIAS`: the key alias inside the keystore
- `ANDROID_KEY_PASSWORD`: optional, only if the key password differs from the keystore password

## Manual workflow inputs

- `version`: release version without the `v` prefix, for example `1.2.3`
- `version_code`: optional Android version code. If omitted, the workflow uses the GitHub Actions run number
- `release_notes`: optional text prepended to GitHub generated release notes

## Keystore base64 examples

PowerShell:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("keystore.p12")) | Set-Clipboard
```

Bash:

```bash
base64 -w 0 keystore.p12
```
