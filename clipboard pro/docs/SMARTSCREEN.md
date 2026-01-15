# Windows SmartScreen Issue

If you see a "Windows protected your PC" window when installing or running **Clipboard Pro** for the first time, do not worry. This is a common occurrence for new applications that do not have an expensive digital certificate.

## Why Does This Happen?

Microsoft Defender SmartScreen blocks unrecognized apps from starting to protect users. Since **Clipboard Pro** is a new, independent open-source project, it hasn't established a "reputation" with Microsoft yet, and it is not signed with a paid Code Signing Certificate (which can cost hundreds of dollars per year).

## How to Proceed (Safe)

1.  Click **"More info"** text on the blue popup.
2.  Click the **"Run anyway"** button that appears.

This will only happen once. After you do this, Windows will trust the application on your PC.

## For Developers: How to Fix This

To completely remove this warning for all users, the application needs to be signed with a Trusted Code Signing Certificate.

1.  **Purchase a Certificate**: Buy a Code Signing Certificate from a CA like Sectigo, DigiCert, or GoDaddy.
2.  **Sign the Exe**: Use `signtool.exe` (part of Windows SDK) to sign the build output.
    ```powershell
    signtool sign /f "YourCert.pfx" /p "YourPassword" /tr http://timestamp.digicert.com /td sha256 /fd sha256 "ClipboardPro.exe"
    ```
