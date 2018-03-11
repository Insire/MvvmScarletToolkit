using System;

namespace MvvmScarletToolkit
{
    public sealed class SnakeOptions : ObservableObject
    {
        public int FieldSize { get; set; } = 16;
        public int FieldCountX { get; set; } = 100;
        public int FieldCountY { get; set; } = 100;

        public int SpeedMultiplier { get; set; } = 1;

        public int FoodInterval { get; set; } = 3;
        public int MaxFoodCount { get; set; } = 5;

        public int StepWidth => FieldSize;
        public int MaxWidth => FieldCountX * FieldSize;
        public int MaxHeight => FieldCountY * FieldSize;

        public int GlobalTickRate => 1000 * SpeedMultiplier;
        public int FoodTickRate => 1000 * SpeedMultiplier * FoodInterval;

        public Position GetStartingPosition()
        {
            return new Position()
            {
                X = Convert.ToInt32(Math.Round(((double)MaxWidth / 2), MidpointRounding.AwayFromZero)),
                Y = Convert.ToInt32(Math.Round(((double)MaxWidth / 2), MidpointRounding.AwayFromZero)),
            };
        }
    }
}
