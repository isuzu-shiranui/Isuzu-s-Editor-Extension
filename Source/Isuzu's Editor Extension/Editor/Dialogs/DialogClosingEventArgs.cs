using System;

namespace IsuzuEditorExtension.Dialogs
{
    /// <inheritdoc />
    /// <summary />
    public class DialogCloseEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        public DialogCloseEventArgs(DialogResult dialogResult)
        {
            this.DialogResult = dialogResult;
        }

        /// <summary>
        ///     ダイアログの結果
        /// </summary>
        public DialogResult DialogResult { get; private set; }
    }
}