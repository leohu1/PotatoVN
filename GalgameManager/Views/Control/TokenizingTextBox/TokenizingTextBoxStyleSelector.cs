﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GalgameManager.Views.Control.TokenizingTextBox
{
    /// <summary>
    /// <see cref="StyleSelector"/> used by <see cref="GalgameManager.Views.Control.TokenizingTextBox.TokenizingTextBox"/> to choose the proper <see cref="GalgameManager.Views.Control.TokenizingTextBox.TokenizingTextBoxItem"/> container style (text entry or token).
    /// </summary>
    public class TokenizingTextBoxStyleSelector : StyleSelector
    {
        /// <summary>
        /// Gets or sets the <see cref="Style"/> of a token item.
        /// </summary>
        public Style TokenStyle { get; set; } = null!;

        /// <summary>
        /// Gets or sets the <see cref="Style"/> of a text entry item.
        /// </summary>
        public Style TextStyle { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizingTextBoxStyleSelector"/> class.
        /// </summary>
        public TokenizingTextBoxStyleSelector()
        {
        }

        /// <inheritdoc/>
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item is ITokenStringContainer)
            {
                return TextStyle;
            }

            return TokenStyle;
        }
    }
}