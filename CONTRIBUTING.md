# Contribution guidelines

**Content**

<!-- TOC -->

- [General](#general)
- [Coding guidelines](#coding-guidelines)
- [Principles](#principles)
- [New *stuff*](#new-stuff)
- [Architecture](#architecture)
- [WPF & MVVM](#wpf--mvvm)
- [Branch](#branch)
- [Summary](#summary)

<!-- /TOC -->

---

## General

First of all, thank you very much for participating in this project.

Here are a few important points that you should look at / read through before you start.

## Coding guidelines

This project follows the following coding guidelines / conventions: [Common C# code conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

## Principles

The following principles should/must be observed:

1. **DRY** (*Don't repeat yourself*) - [Click here for more information](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself)
2. **YAGNI** (*You aren't gonna need it*) - [Click here for more information](https://en.wikipedia.org/wiki/You_aren%27t_gonna_need_it)
3. **KISS** (*Keep it simple, stupid*) - [Click here for more information](https://en.wikipedia.org/wiki/KISS_principle)
4. **SOLID** - [Click here for more information](https://en.wikipedia.org/wiki/SOLID)

## New *stuff*

If you add something, make sure that a corresponding function / class / etc. does not yet exist.

## Architecture

This project uses a simple, horizontal layered multitir architecture.

If you are not familiar with it, please read the following: [Multitier architecture (Wikipedia)](https://en.wikipedia.org/wiki/Multitier_architecture).

This architecture must not be *broken*. If this is still the case, please contact us beforehand.

## WPF & MVVM

This project uses WPF with MVVM and the UI framework MahApps. If you are not familiar with this, please read the following:

- [WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/getting-started/introduction-to-wpf-in-vs?view=netframeworkdesktop-4.8&viewFallbackFrom=netdesktop-8.0)
- [MVVM](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel)
- [MahApps](https://mahapps.com/)

The NuGet package *CommunityToolkit.MVVM* is used for the use of MVVM. You can find out more about this here: [MVVM Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

## Branch

Every change must be made in a branch. Changes in the *Main* branch are **not** permitted!

## Summary

Once you have familiarized yourself with everything, you can get started.

Have fun developing and if you have any questions, don't hesitate to ask (you can use the [Discussions](https://github.com/InvaderZim85/MsSqlToolBelt/discussions))
