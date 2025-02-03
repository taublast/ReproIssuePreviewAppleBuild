# ReproIssuePreviewAppleBuild
Repro Issue Latest VS Preview multiple-build for Apple

Version 17.13.0 Preview 4.0

XCode 16.2
Simulator iPhone 15 18.2

## Issue

Main difference between real device and simulator for the repro project is that on real device app closes all the time when you stop debugging from VS, on simulator it does not for every case.

When building for Apple, gettings following symptoms:

A - Using Simulator

1 Start without debugging

2 Open CommunityToolkit.Maui.Core / Options.cs, change `BuildNumber` to a new number, click Start without debugging.

3 You wiill see the app displaying your old number at the "Debug Label".

4 Make new changes, this time click "Debug"

5 You will get stuck. App will not deploy until you force-quit app on simulator.

6 Force-kill app on simulator, your app will deploy and report "successfully launched". You still see an old debug number.

7 Force-quit app on simulator. A new debug session will launch. With the same old number.

8 Click Stop. App will close. Edit any file in main app project. Click Debug.

9 App will start with changes finally picked up for DebugLabel.

10 Force-close app on simulator. Notice VS stuck at "INFO: Closing debug session after launching on simulator..." still debugging.

11 Click Stop. Edit MainPage.xaml. Click Debug. You get "Could not open port for debugger. Another process may be using the port."


Okay these are just some repro steps, if you really work on a real-case project you should get even more fun with a similar repro solution.

Main workaround to this mess is to do a "Rebuild" of the project you just changed, only then click "Debug" or Start witthout debugging"".  

So the worst case is when you changed the root project that references like 10 other projects you need to rebuild it otherwise for debugging you risk a nice  "Could not open port for debugger. Another process may be using the port.".

This all is reproducible with or without preview options checked, my actual setup is on the picture:

![image](https://github.com/user-attachments/assets/c4623ff1-8cb3-4949-be4d-097250c58606)

What i also have:

![image](https://github.com/user-attachments/assets/c3b000df-af17-4d00-a37b-b034ee71021a)

and

![image](https://github.com/user-attachments/assets/0131784e-e08d-4b80-a016-5ea2ff0ea996)

