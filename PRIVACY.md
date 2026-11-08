# Privacy — QR King

Last updated: 7 November 2026.

QR King is a non-commercial, open-source Windows utility maintained by Boris Vacula. This describes the application's behaviour, not GitHub's or Windows' privacy practices.

## Clipboard and text

While Live updates is enabled, the app reads clipboard text on startup and when the clipboard changes, displays it, and generates a QR code locally. Turning Live updates off stops automatic reads and lets you edit text manually. Dropping a .txt file reads that local file and switches to manual mode; it does not modify the file or clipboard. Imported file paths and contents are not saved to preferences.

Text and QR images remain in the running application's memory until replaced or the application closes. Clear removes the displayed text and image; it does not clear the Windows clipboard.

The app does not transmit clipboard or template content to the maintainer or a remote service. It has no built-in analytics, telemetry, accounts, or automatic crash-upload feature. Saving an image writes the selected file; Copy image puts the image on the Windows clipboard. Other apps and Windows clipboard history or synchronization can retain data independently.

## Preferences (v0.2.1 and later)

The application saves only window position, QR options and other non-content preferences at:

%LOCALAPPDATA%\QR King\settings.json

Clipboard contents, imported text, manually entered text, and template data are never saved to preferences or restored between sessions. There is no Remember text or Delete saved text control. Saved images are still written when you explicitly choose Save image.

## Upgrading from v0.2.0

The previously published v0.2.0 release had an opt-in Remember text feature that could save unencrypted content. When this version loads an older settings file, it removes the legacy text fields while retaining valid non-content preferences. An abandoned settings.json.tmp file is overwritten with non-content settings and removed by the atomic save. Invalid settings are replaced with defaults. Text from an older settings file is never displayed or restored.

If access or file locking prevents cleanup, the app warns you that old data may remain. Close all older/running copies of QR King, then delete settings.json and settings.json.tmp under %LOCALAPPDATA%\QR King, or fix permissions and restart. To reset every preference, close the app and delete that folder. Older running versions may write their own text back, so close them before upgrading.

Removing files or rewriting settings is not secure erasure and cannot remove copies from backups, filesystem recovery, screenshots, exported images, other apps, or Windows clipboard history. The v0.1.0 application did not persist preferences or text. The privacy changes described here apply to v0.2.1 and later, not retroactively to older executables.

## External websites and reports

Opening the GitHub link launches your default browser. GitHub and your browser have their own privacy practices. Reading this document through About > Privacy uses the bundled local file and does not open a website.

If you submit a public GitHub issue, its text and attachments are public. Do not include passwords, private clipboard contents or personal information. Questions about this document can be raised at https://github.com/Boriz0/QR_King/issues without sharing sensitive data.
