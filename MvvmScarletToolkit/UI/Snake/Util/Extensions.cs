namespace MvvmScarletToolkit
{
    public static class Extensions
    {
        /// <summary>
        /// Determines if any space of the left <see cref="IPositionable"/> overlaps with a right
        /// <see cref="IPositionable"/>
        /// </summary>
        public static bool Intersect(this IPositionable left, IPositionable right)
        {
            // XH1 < XH2 YH1 < YH2
            var XH1 = left.CurrentPosition.X - left.Size.Width;
            var YH1 = left.CurrentPosition.Y - left.Size.Height;
            var XH2 = left.CurrentPosition.X + left.Size.Width;
            var YH2 = left.CurrentPosition.Y + left.Size.Height;

            // XA1 < XA2 YA1 < YA2
            var XA1 = right.CurrentPosition.X - right.Size.Width;
            var YA1 = right.CurrentPosition.Y - right.Size.Height;
            var XA2 = right.CurrentPosition.X + right.Size.Width;
            var YA2 = right.CurrentPosition.Y + right.Size.Height;

            return (XA2 >= XH1 && XA1 <= XH2) && (YA2 >= YH1 && YA1 <= YH2);
        }
    }
}
