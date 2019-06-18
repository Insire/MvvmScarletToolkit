﻿using MvvmScarletToolkit.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MvvmScarletToolkit
{
    public sealed class LocalizationProvider : ILocalizationProvider
    {
        public IEnumerable<CultureInfo> Languages { get; } = Enumerable.Empty<CultureInfo>();

        public string Translate(string key, CultureInfo culture)
        {
            return key;
        }
    }
}
