---
title: "Install ALX"
description: "Get ALX running on your Windows PC."
sidebar:
  order: 1
---

ALX runs on Windows today. Linux and Mac come later.

## What you need

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). This builds ALX on your PC.
- [Git](https://git-scm.com/) to download ALX.
- [Visual Studio Code](https://code.visualstudio.com/) is recommended for writing code.

:::note[One-click installs are coming]
From **0.8.0** every release will publish a ready Windows zip, so you will not need .NET to try ALX. Until then, follow the steps below.
:::

## Install

```powershell
git clone https://github.com/Real-devalex/alx.git
cd alx
.\install.ps1
```

The installer builds ALX and puts `alx` in `%LOCALAPPDATA%\ALX`. Open a new terminal and check it worked:

```bash
alx version
```

## Add VS Code colours

Run `install-vscode.ps1` from the same folder. It installs the ALX extension, which gives you colours and a play button for `.alx` files.

## Update later

```bash
alx update
```
