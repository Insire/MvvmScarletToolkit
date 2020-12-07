﻿namespace MvvmScarletToolkit.Wpf.Samples
{
    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class GenericScarletMessage<TContent> : ScarletMessageBase
    {
        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; }

        public GenericScarletMessage(in object sender, in TContent content)
            : base(sender)
        {
            Content = content;
        }
    }
}