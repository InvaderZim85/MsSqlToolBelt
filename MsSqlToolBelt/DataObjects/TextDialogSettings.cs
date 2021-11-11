﻿using System;
using System.Threading.Tasks;
using MsSqlToolBelt.DataObjects.Types;

namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Provides the settings for the text dialog
    /// </summary>
    public sealed class TextDialogSettings
    {
        /// <summary>
        /// Gets or sets the title of the <see cref="View.TextDialog"/> window
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the caption of the window (short text below the title)
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the text which should be shown
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the text which should be shown when the "option" is true
        /// </summary>
        public string TextOption { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the option check box should be shown
        /// </summary>
        public bool ShowOption { get; set; }

        /// <summary>
        /// Gets or sets the text of the check box
        /// </summary>
        public string CheckboxText { get; set; }

        /// <summary>
        /// Gets or sets the code type
        /// </summary>
        public CodeType CodeType { get; set; } = CodeType.None;

        /// <summary>
        /// Gets or sets the value which indicates if the validate button should be shown
        /// </summary>
        public bool ShowValidateButton { get; set; }

        /// <summary>
        /// Gets or sets the function wich validates the input
        /// </summary>
        public Func<string, Task<(bool valid, string message)>> ValidationFunc;
    }
}
