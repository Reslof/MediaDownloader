
@echo off
echo Waiting for file handle to be closed ...
ping 127.0.0.1 -n 5 -w 1000 > NUL
move /Y "F:\repos\MediaDownloader\Release\youtube-dl.exe.new" "F:\repos\MediaDownloader\Release\youtube-dl.exe" > NUL
echo Updated youtube-dl to version 2021.12.17.
start /b "" cmd /c del "%~f0"&exit /b"
                
