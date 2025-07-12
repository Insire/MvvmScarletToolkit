using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Avalonia.Samples.Views;
using MvvmScarletToolkit.Core.Samples.Features.Busy;
using MvvmScarletToolkit.Core.Samples.Features.Enums;
using System;
using System.Collections.Generic;

namespace MvvmScarletToolkit.Avalonia.Samples
{
    public sealed class ViewLocator : IDataTemplate
    {
        private readonly Dictionary<Type, Type> _viewMap;

        public ViewLocator()
        {
            _viewMap = new Dictionary<Type, Type>()
            {
                // [typeof(DataEntriesViewModel)]=typeof(),
                // [typeof(AsyncStateListViewModel)]=typeof(),
                // [typeof(ProgressViewModel)]=typeof(),
                [typeof(BusyViewModel)] = typeof(BusyView),
                // [typeof(PasswordViewModel)]=typeof(),
                // [typeof(ProcessViewModel)]=typeof(),
                // [typeof(ContextMenuViewModels)]=typeof(),
                [typeof(EnumViewModel)] = typeof(EnumsView),
                // [typeof(FormViewModel)]=typeof(),
                // [typeof(ObservableDictionaryViewModel)]=typeof(),
            };
        }

        public Control? Build(object? viewmodel)
        {
            if (viewmodel is null)
            {
                return new TextBlock { Text = "NULL" };
            }

            var viewmodelType = viewmodel.GetType();
            if (!_viewMap.TryGetValue(viewmodelType, out var viewType))
            {
                return new TextBlock { Text = "Not Found: " + viewmodelType };
            }

            return (Control)Activator.CreateInstance(viewType)!;
        }

        public bool Match(object? data)
        {
            return data is ObservableObject;
        }
    }
}
