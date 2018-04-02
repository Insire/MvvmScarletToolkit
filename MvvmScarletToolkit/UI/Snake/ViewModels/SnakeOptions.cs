using System;

namespace MvvmScarletToolkit
{
    public sealed class SnakeOptions : ObservableObject
    {
        public static SnakeOptions Easy()
        {
            return new SnakeOptions()
            {
                SpeedMultiplier = 0.1,

            };
        }

        public static SnakeOptions Normal()
        {
            return new SnakeOptions()
            {
                SpeedMultiplier = 0.075,

            };
        }

        public static SnakeOptions Hard()
        {
            return new SnakeOptions()
            {
                SpeedMultiplier = 0.035,

            };
        }

        public int FieldSize { get; set; } = 16;
        /// <summary>
        /// max amount of fields on x-axis
        /// </summary>
        public int FieldCountX { get; set; } = 100;
        /// <summary>
        /// max amount of fields on y-axis
        /// </summary>
        public int FieldCountY { get; set; } = 100;

        /// <summary>
        /// a value between 0 and 1
        /// </summary>
        public double SpeedMultiplier { get; private set; } = 0.075;

        public int FoodInterval { get; set; } = 3;
        public int MaxFoodCount { get; set; } = 5;

        public int StepWidth => FieldSize;
        public int MaxWidth => FieldCountX * FieldSize;
        public int MaxHeight => FieldCountY * FieldSize;

        public int GlobalTickRate => Convert.ToInt32(Math.Round(1000 * SpeedMultiplier, MidpointRounding.AwayFromZero));
        public int FoodTickRate => Convert.ToInt32(Math.Round(1000 * SpeedMultiplier * FoodInterval, MidpointRounding.AwayFromZero));

        public Position GetStartingPosition()
        {
            return new Position()
            {
                X = Convert.ToInt32(Math.Round(((double)MaxWidth / 2), MidpointRounding.AwayFromZero)),
                Y = Convert.ToInt32(Math.Round(((double)MaxHeight / 2), MidpointRounding.AwayFromZero)),
            };
        }
    }
}
