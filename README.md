MediaDownloader
===============

C# and WinForms wrapper for youtube-dl and ffmpeg for easy audio/video downloading.

Requirements
-----
- Youtube-dl **Standalone EXE**: https://rg3.github.io/youtube-dl/download.html
- ffmpeg/ffmprobe **32-bit Static Version**: http://ffmpeg.zeranoe.com/builds/

Both of these programs must be in your PATH variable.

Note
-----

URL input is currently not sanitized. This means that if you mean to download only a single video from Youtube, but paste a playlist URL you will end up downloading the entire playlist with no confirmation. 

