# Privacy — QR King

Last updated: 7 November 2026.

QR King is a non-commercial, open-source Windows utility maintained by Boris Vacula. This describes the application's behaviour, not GitHub's or Windows' privacy practices.

## Clipboard and text

While Live updates is enabled, the app reads clipboard text on startup and when the clipboard changes, displays it, and generates a QR code locally. Turning Live updates off stops automatic reads and lets you edit text manually. Text and QR images remain in the running application's memory until replaced or the application closes. Clear removes the displayed text and image; it does not clear the Windows clipboard.

The app does not transmit clipboard or template content to the maintainer or a remote service. It has no built-in analytics, telemetry, accounts, or automatic crash-upload feature. Saving an image writes the selected file; Copy image puts the image on the Windows clipboard. Other apps and Windows clipboard history or synchronization can retain data independently.

## Preferences and optional saved text (v0.2.0 and later)

Window position, QR options and other preferences are stored at:

%LOCALAPPDATA%\QR King\settings.json

Remember text is off by default. If you enable it in Advanced and apply the change, the current text may be saved in that file when preferences are saved or the app closes. It is restored on startup in manual mode. This is unencrypted text, which can include passwords, Wi-Fi credentials, or contact details. People or software able to read the file can read it. The app does not store a separate clipboard history.

To remove the app's saved text, use Advanced > Delete saved text. This immediately removes the saved value and switches Remember text off, even if you later cancel the dialog. Alternatively, uncheck Remember text and press Apply. A failure to save/delete is reported; check the local file if an error appears. The currently displayed text and Windows clipboard are unchanged. Clear also removes the current saved value when preferences can be written, but does not disable future remembering.

To remove all preferences and stored text manually, close all running copies of QR King and delete the QR King folder under %LOCALAPPDATA%. It will be recreated with defaults on the next settings save. Removal is not a secure erase and cannot remove copies from backups, filesystem recovery, exported images, screenshots, other apps, or Windows clipboard history. A second running copy can write its own settings; close other copies before deleting.

The v0.1.0 application does not have persistent preferences or Remember text. A corrected v0.1.0 package only adds documentation and licenses; it does not add the newer controls.

## External websites and reports

Opening the GitHub link launches your default browser. GitHub and your browser have their own privacy practices. Reading this document through About > Privacy uses the bundled local file and does not open a website.

If you submit a public GitHub issue, its text and attachments are public. Do not include passwords, private clipboard contents or personal information. Questions about this document can be raised at https://github.com/Boriz0/QR_King/issues without sharing sensitive data.
