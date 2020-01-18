# MvvmScarletToolkit.Commands

## Goals

This library aims to provide easy to use [ICommand](https://docs.microsoft.com/en-gb/dotnet/api/system.windows.input.icommand) implementations for use in viewmodels.

## Contents

|class|summary|
|---|---|
|``AsyncCommand<TArgument>``|Task based ICommand implementation.|
|``ConcurrentCommand<TArgument>``|An advanced Task based ICommand implementation supporting fluent configuration.|
|``RelayComand`` and ``RelayComand<TArgument>``|Popular synchronous implementation of ICommand.|

## Credits

``AsyncCommand<TArgument>`` and ``ConcurrentCommand<TArgument>`` are largely based on Stephen Clearlys blog post: [Async Programming : Patterns for Asynchronous MVVM Applications](https://msdn.microsoft.com/en-us/magazine/dn630647.aspx?f=255&MSPPError=-2147217396)

[back](../readme.md)
