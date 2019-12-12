Internal Accessible Compiler
===

This package generates an 'internal accessible' dll.

In other words, you can internally access to other assemblies **without reflection**.

![](https://user-images.githubusercontent.com/12690315/70728190-4b81c980-1d44-11ea-856c-b05332d88ca0.png)
![](https://user-images.githubusercontent.com/12690315/70616819-a804bc00-1c52-11ea-8ea3-e24f94f6467d.gif)

[![](https://img.shields.io/github/release/mob-sakai/InternalAccessibleCompilerForUnity.svg?label=latest%20version)](https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/InternalAccessibleCompilerForUnity.svg)](https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/releases)
![](https://img.shields.io/badge/unity-2018.3%20or%20later-green.svg)
[![](https://img.shields.io/github/license/mob-sakai/InternalAccessibleCompilerForUnity.svg)](https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/blob/upm/LICENSE.txt)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)

<< [Description](#description) | [Install](#install) | [Usage](#usage) >>

### What's new? [See changelog ![](https://img.shields.io/github/release-date/mob-sakai/InternalAccessibleCompilerForUnity.svg?label=last%20updated)](https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/blob/upm/CHANGELOG.md)
### Do you want to receive notifications for new releases? [Watch this repo ![](https://img.shields.io/github/watchers/mob-sakai/InternalAccessibleCompilerForUnity.svg?style=social&label=Watch)](https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/subscription)
### Support me on GitHub!  
[![become_a_sponsor_on_github](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)


<br><br><br><br>
## Description

About `IgnoresAccessChecksToAttribute`  
[No InternalsVisibleTo, no problem – bypassing C# visibility rules with Roslyn](https://www.strathweb.com/2018/10/no-internalvisibleto-no-problem-bypassing-c-visibility-rules-with-roslyn/)



<br><br><br><br>
## Install

Find `Packages/manifest.json` in your project and edit it to look like this:
```js
{
  "dependencies": {
    "com.coffee.internal-accessible-compiler": "https://github.com/mob-sakai/InternalAccessibleCompilerForUnity.git",
    ...
  },
}
```

To update the package, add/change prefix `#version` to the target version.  
Or, use [UpmGitExtension](https://github.com/mob-sakai/UpmGitExtension).


### Requirement

* Unity 2018.3 or later
* Dot Net 2.1 or later



<br><br><br><br>
## Usage

### Compile AssemblyDefinitionFile to an 'internal accessible' dll

1. Select `*.asmdef` in project view.
2. Click Right button and select `Internal Accessible Compiler > Setting` in context menu.  
![](https://user-images.githubusercontent.com/12690315/70728182-49b80600-1d44-11ea-9ef7-9f2709702b81.png)
3. Open `Internal Accessible Compiler Setting` and configure compile setting.  
![](https://user-images.githubusercontent.com/12690315/70728190-4b81c980-1d44-11ea-856c-b05332d88ca0.png)
   * **Assembly Names To Access:** Target assembly names separated by semicolons to access internally (eg. UnityEditor;UnityEditor.UI) 
   * **OutputDllPath:** Output dll path (eg. Assets/Editor/SomeAssembly.dll)  
4. Press `Compile` button to start compiling. After compilation, the dll will be automatically imported.
5. Enjoy!



<br><br><br><br>
## Demo

A demo project that dynamically changes the text displayed in UnityEditor's title bar.　(This package is used in Solution 3.)
https://github.com/mob-sakai/MainWindowTitleModifierForUnity



<br><br><br><br>
## License

* MIT



## Author

[mob-sakai](https://github.com/mob-sakai)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)  
[![become_a_sponsor_on_github](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)



## See Also

* GitHub page : https://github.com/mob-sakai/InternalAccessibleCompilerForUnity
* Releases : https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/releases
* Issue tracker : https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/issues
* Change log : https://github.com/mob-sakai/InternalAccessibleCompilerForUnity/blob/upm/CHANGELOG.md
* [No InternalsVisibleTo, no problem – bypassing C# visibility rules with Roslyn](https://www.strathweb.com/2018/10/no-internalvisibleto-no-problem-bypassing-c-visibility-rules-with-roslyn/)