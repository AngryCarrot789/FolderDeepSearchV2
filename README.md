# FolderDeepSearchV2
A simple application for searching folders for items (files, folders or file contents), with recursive search options, string options, etc.

I made this as a replacement for windows explorer's dreadful search feature which takes forever just to find a file (as it's indexing while searching. Even after turning that off, it's still slow). This app does no indexing

It's possible to add a few registry options to open the app in the folder you're currently in (in windows explorer).
- Go to `Computer\HKEY_CLASSES_ROOT\Directory\Background\shell`
- Create a new key/folder called `FolderDeepSearch` (or anything you'd like)
- Inside that, set the (Default)'s contents to the text you want in the windows context menu (e.g `Search folder...`)
- Optionally, create a new string called "Icon" and set the path to this app's .exe (you have to compile it obviously to get the exe file)
- Create a sub-key called `command`
- Set the (Default) contents to `"path to the app exe" "%V"` (make sure to include the quotes)
After doing that, it should appear in your context menu, and when you click it, it will set the app's start folder to which ever folder you opened the app in (This is processed in Application_Startup() in App.xaml.cs using StartupEventArgs)
