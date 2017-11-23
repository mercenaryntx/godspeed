# godspeed
GODspeed - FTP client for JTAG/RGH Xbox 360 consoles

-= Project Description =-

GODspeed is a Total Commander like FTP client designed to fasten and clarify file management of JTAG/RGH/DevKit Xbox 360 consoles. 

-= Main features =-

- FTP connection to Xbox 360 with Freestyle Dash, XeXMenu, DLi and/or Aurora
- Instead of cumbersome IDs real names and thumbnails are displayed to see who is who and what is what
- Automatic Gamertag and Gamer picture extraction from profiles
- Automatic game information gathering from Game folders and Xbox Unity
- Manual game information gathering from profiles
- Automatic SVOD package recognition (DLCs, Title Updates, gamesaves, etc.)
- STFS package browsing, content extraction/injection
- Folder size calculation on FTP (most annoying deficiency of Total Commander)
- Content-aware folder creation
- Remote Copy support between NAS and FTP (Telnet and LFTP required)
- Compressed file support
- Extended FSD/F3 support:
   - Automatic Content scan triggering
   - File hash verification
   - Game launch
   - Xex launch
   - Shutdown
   - Database checking and disk clean up
- Limited support to PS3 (multiMAN)

-= Version History =-

[UNRELEASED] v1.2

* ADDED: DashLaunch FTPdll plugin support (ftpdll.xex)
* ADDED: Large icons view
* ADDED: Title/Name column mode switch (Ctrl+Click on the Title/Name column)
* ADDED: Title bar to the context menu (Open with Explorer action for local contents)
* ADDED: File operation options to the context menu
* ADDED: View and sorting options to the context menu
* ADDED: FATX file name and path limitations awareness
* ADDED: "Overwrite all older" has been replaced with "Overwrite all smaller"
* ADDED: Write error dialog refactor
* ADDED: Renaming a file now supports overwrite (using the same write error dialog that can appear during transfer)
* ADDED: Favorite folders
* ADDED: UI improvements
* ADDED: System default icons for well-known extensions
* ADDED: Recognize game demos
* ADDED: Signed in profiles can be recognized now using F3Plugin WebUI (supported by F3 and Aurora)
* ADDED: Ctrl+R (Refresh) refactor - Now it refreshes cached contents too
* ADDED: ISO to GOD conversion (Partial rebuild is not available yet)
* ADDED: Auto updater for quick hotfixes
* FIXED: Active pane behavior refactor
* FIXED: Transfer timer and speedometer refactor
* FIXED: name.txt content validation and text overflow
* FIXED: More fail-safe compressed file support (Please note that password protected archives are not supported yet!)
* FIXED: Navigation into AvatarAssets folder within a profile package
* FIXED: File existence check cache to speed up transfer
* FIXED: File date issues in F3
* FIXED Issue #509: Null reference exception in FTP CloseDataStream
* FIXED Issue #512: Directory listing refactor to support folders with large number of files
* FIXED Issue #513: Null reference exception in TransferManagerViewModel.ProcessError
* FIXED Issue #516: Null file item name property in ExecuteChangeDirectoryCommand
* FIXED Issue #538: Null reference exception in GetCorrespondingScanFolder
* FIXED Issue #562: File does not exists in the package: Account
* FIXED Issue #585: Error getting StorageInfo in Aurora 0.1a
* FIXED Issue #606: Handle HTTP GET errors
* FIXED Issue #611: Error when pressing Pause button during delete
* FIXED Issue #727: Collection was modified; Main window was hit test visible during population
* REMOVED: Facebook and Codeplex notifications

[2014-09-04] v1.1

FIXED Issue #61: Null reference when showing non-closable notification message
FIXED Issue #135: DialogResult can be set only after Window is created and shown as dialog.
FIXED Issue #168: Application crashes if item cannot be deleted
FIXED Issue #171: Uploading and empty folder browsing with XeXMenu
FIXED Issue #173: Do not let multiple instances to run
FIXED Issue #177: Throwing the cache of a non-cached item
FIXED Issue #181: Application crashes if parent folder doesn't exist anymore but I call an UpDirectory
FIXED Issue #182: Application crashes if folder cannot be renamed
FIXED Issue #187: Application crashes if reparse points cannot be accessed
FIXED Issue #220: Handle errors on the right thread during changing directory
FIXED Issue #237: STFS DateTime parsing
FIXED Issue #244: Special characters in FTP item names
FIXED Issue #267: No application is associated with URLs
FIXED Issue #283: Handle permission denied errors on FTP
FIXED: Black Window on Windows 7 Aero theme
FIXED: Misbehaving .. folder on local file system
FIXED: Stored participation answer isn't displayed if I open the User Participation Window again
FIXED: Editing connection highlight name as an already existing one
FIXED: Drive selector combobox shows wrong drive letter after unsuccessful drive change
FIXED: Can't open profile by right clicking on its profile
FIXED: Can't interact (open, recognize, etc.) with a profile if the cache was just erased before
FIXED: Trimming of long connection names
FIXED: Cannot abort second transfer if the first one was aborted too
FIXED: Title recognition after rename
FIXED: Folder creation within Xbox folder structure
FIXED: Folder name validation
FIXED: Skip All doesn't remember decision
FIXED: Move deletes skipped files
ADDED: DashLaunch FTP (DLiFTPD) support
ADDED: XeXMenu and DashLaunch FTP cannot be accessed in PASV anymore
ADDED: Better support of reparse points
ADDED: Completely rewritten copy mechanism
ADDED: Completely rewritten caching mechanism
ADDED: FTP stream handling refactor
ADDED: Aurora support
ADDED: FSD/F3 Content Scan Trigger
ADDED: File hash verification after upload/download
ADDED: Launch games via HTTP
ADDED: Launch .xex file via FTP
ADDED: FSD Database Checker and Clean up
ADDED: Shutdown PC and/or Xbox after transfer completed
ADDED: Displaying version number of Title Updates
ADDED: FTP Log Viewer
ADDED: Hungarian language
ADDED: New About window
ADDED: Grid splitter presets

[2014-03-24] v1.0 RC3

FIXED: File existence check on FTP

[2014-03-21] v1.0 RC2

FIXED Issue #9: Polish month abbreviations in LIST results
FIXED Issue #18: "No error" exception when calling GetDrives()
FIXED Issue #28: Null reference exception after DeleteError
FIXED Issue #33: Folder creation on a device with no space left throws no exception
FIXED Issue #35: Null reference exception in FtpConnectError
FIXED Issue #36: Null reference exception during the disposal of a non-connected FTP ViewModel
FIXED Issue #40: Copying a grid row to clipboard
FIXED Issue #44: Lost connection can screw up directory navigation
FIXED Issue #45: .NET version checker doesn't do its thing
FIXED Issue #48: Null reference exception after CopyError
FIXED Issue #62: Can't connect back to an FTP where the last visited folder doesn't exist anymore
FIXED Issue #63: Remove server has not initiated the connection exception
FIXED Issue #64: Null reference exception in FTP file renaming
FIXED Issue #84: Null reference exception during Title Recognition
FIXED: Recognize standalone SVOD packages
FIXED: Clear Cache UI freeze
FIXED: PASV usage
FIXED: UTF-8 error reporting
FIXED: Special characters in path screws up Remote Copy
FIXED: Recognition can freeze if an unexpected error happens
FIXED: Remember last used sort order
FIXED: Close FTP pane if connection cannot be reestablished or user decides to do so
FIXED: Progress indication fix in case of skipping/retrying partially transferred files
FIXED: Access of read-only files
FIXED: Something went wrong exception doesn't have a stack trace
ADDED: New FTP client library (Based on the work of J.P. Trosclair, https://netftp.codeplex.com/), the dependency to Limilabs' Ftp.dll has been finally removed
ADDED: User Notification Service
ADDED: Sanity checker - checks dependencies, migrates cached data from an old version if needed
ADDED: User Statistics
ADDED: Partial recognition notification
ADDED: New new version detection
ADDED: Full FSD 1.x (MinFTPD) support for the old fashioned ones :)
ADDED: Pause/Continue buttons in the Windows 7 taskbar thumbnail
ADDED: Select drive in dropdown by initial letter key press
ADDED: Shows "?" in Size column while calculating
ADDED: Size calculation can now aborted by pressing the Esc key or changing directory

[2014-02-08] v1.0 RC1

FIXED Issue #4: Handle connection callback errors
FIXED Issue #10: Connection name existance check
FIXED Issue #12: Handle lost connection when calling up directory
FIXED Issue #24: Null reference exception in FTP connection error callback
ADDED: Indicate inaccessible files
ADDED: Indicate inaccessible profiles
ADDED: Cache item won't be invalidated if refreshing fails
ADDED: Invalid items won't be cached anymore
ADDED: Sending screenshot attached to error reporting

[2014-01-27] v1.0 Beta 4

FIXED: Title caching (Directory change speed up 500-1000%)
FIXED: Displaying wrong folder size before copy
FIXED: Profile detection error
FIXED: Last visited path per connection
FIXED: New version detection
FIXED: Executing the Rename command from Context menu
FIXED: Title recognition in STFS packages
FIXED: STFS package saving
FIXED: Refreshing directory shows random percentage
FIXED: Speedmeter using Remote Copy
FIXED: Remote Copying files with "&" in path
FIXED: PS3 contents aren't treated as Xbox anymore
ADDED: Compressed file support (Zip, Rar, Tar, GZip, 7Zip)
ADDED: Invert Selection (Num *)
ADDED: Quick Search
ADDED: File/Directory rename
ADDED: .NET version detection (Quick Search and Renaming requires .NET 4.0.30319.18408 or newer)
ADDED: XeXMenu support
ADDED: Active mode enabled and became default for FTPs (Passive Mode is still available but cannot be used with XeXMenu)
ADDED: Warning messages can be ignored with "Don't show this message again" checkbox
ADDED: NTFS Junction Point support
ADDED: File transfer notification during Indirect Copy
ADDED: Clear Cache command
ADDED: Navigate to parent folder and closing nested pane (FTP, STFS, Compressed file) by pressing the Backspace button
ADDED: PS3 Free space indication
ADDED: Removable device support (can handle the insertion and removal of USB devices)
ADDED: Error reporting of unhandled exceptions

[2013-11-15] v1.0 Beta 3

FIXED: Upload to FTP (was malfunctioning with FSD 2.2)
FIXED: FTP upload error handling
FIXED: FTP download progress indication
FIXED: UI freeze caused by FTP download
FIXED: Displaying selection size after queue population
FIXED: Main window is not hit test visible any more during transfer
FIXED: Free space update
FIXED: Slim E icon was missing
ADDED: Progress indication in taskbar
ADDED: Recognition progress indication
ADDED: Elapsed time indication, Remaining time calculation
ADDED: Transfer speed calculation
ADDED: Local to local progress indication
ADDED: Local to local file transfer abortion
ADDED: Pause & Resume file transfer
ADDED: Context-aware "New folder" dropdown
ADDED: Save and restore locations and sort settings
ADDED: User settings (more options will be added soon)
ADDED: Custom Window Chrome now can be disabled
ADDED: Remote Copy support between NAS and FTP (Telnet and LFTP required)
ADDED: Delete existings connection
ADDED: Anonymous login
ADDED: Minor PS3 FTP support

[2013-10-23] v1.0 Beta 2

NEW: Open command introduced in the profile item's context menu to ease STFS access
NEW: Version checker
FIXED: Keyvault resources were missing from Core
FIXED: Move command now works properly
FIXED: File existense detection during FTP download
Slight performance tweak
Minor UI related bugfixes

[2013-10-21] v1.0 Beta 1

Limitations

- The left pane can be used for local file system browsing only
- The right pane can be used for FTP browsing only
- Limited configuration possibilities
- Edit action works on FTP connection settings only

Usage

- Double click or Enter on the New connection... item in the Connections pane to create a new connection.
- Double click or Enter on a connection item to connect to an FTP.
- Browse into your Content folder and wait for your profiles to be recognized. Result will be cached.
- Browse into your Games folder and wait for your games to be recognized. Result will be cached.
- If you see a game with a green Xbox icon and - probably - trimmed title, it means the game isn't installed on your console. As a fallback GODspeed checked the title on covers.jqe360.com and found the displayed title.
- If you see a game with the name Unknown game it means GODspeed couldn't find it on covers.jqe360.com either. (Or jqe360.com lookup has been switched off)
- Right click and select Rename if you want to change an item's title.
- Right click on a profile and select Recognize Titles from Profile to update your cached game information based on the selected profile.u It's much more accurate than covers.jqe360.com but significantly slower.
- To browse profile content select Open profile, or if it's a file Double click or Enter on it. Every change you make on that profile is in-memory and will be saved only if you press the Save and Close button. If you open a profile via FTP a temp file is used and it will be uploaded back automatically after Save and Close.
  USE IT WITH CAUTION!
- Network shares are not supported. If you want to connect to a NAS map a network drive first.
- If you have a NAS and you want copy files between it and your Xbox, you can speed up your transfer by turning the Remote Copy feature on. However this is going to work only if your NAS supports Telnet and LFTP.
- You can use the Total Commander's most common hotkeys in GODspeed as well like the F keys, Alt+F1/F2 for drive change, Space and Ctrl+L for space consumption calculation, etc.
- If the application stops working and throws an unexpected error please press the Report button to report the error to help to fix it ASAP. Thank you!
