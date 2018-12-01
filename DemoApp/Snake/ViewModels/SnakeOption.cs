using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit
{
    public sealed class SnakeOption : ObservableObject
    {
        public static SnakeOption Easy()
        {
            return new SnakeOption()
            {
                SpeedMultiplier = 0.1,
                IsDefault = true,
                Name = "Easy",
            };
        }

        public static SnakeOption Normal()
        {
            return new SnakeOption()
            {
                SpeedMultiplier = 0.075,
                IsDefault = true,
                Name = "Normal",
            };
        }

        public static SnakeOption Hard()
        {
            return new SnakeOption()
            {
                SpeedMultiplier = 0.035,
                IsDefault = true,
                Name = "Hard",
            };
        }

        /// <summary>
        /// indicates whether this instance should be modified via the UI
        /// </summary>
        public bool IsDefault { get; private set; }

        public string Name { get; private set; }

        private int _fieldSize;
        /// <summary>
        /// Size of a BoardTile
        /// </summary>
        public int FieldSize
        {
            get { return _fieldSize; }
            set { SetValue(ref _fieldSize, value); }
        }

        private int _fieldCountX;
        /// <summary>
        /// max amount of fields on x-axis
        /// </summary>
        public int FieldCountX
        {
            get { return _fieldCountX; }
            set { SetValue(ref _fieldCountX, value); }
        }

        private int _fieldCountY;
        /// <summary>
        /// max amount of fields on y-axis
        /// </summary>
        public int FieldCountY
        {
            get { return _fieldCountY; }
            set { SetValue(ref _fieldCountY, value); }
        }

        private double _speedMultiplier;
        /// <summary>
        /// a value between 0 and 1
        /// </summary>
        public double SpeedMultiplier
        {
            get { return _speedMultiplier; }
            set { SetValue(ref _speedMultiplier, value); }
        }

        /// <summary>
        /// How often food should be generated
        /// </summary>
        public int FoodInterval { get; set; }

        /// <summary>
        /// maximum amount of food units
        /// </summary>
        public int MaxFoodCount { get; set; }

        /// <summary>
        /// determines how many units a board piece can move per tick
        /// </summary>
        public int StepWidth => FieldSize;

        public int BoardPieceSize => FieldSize - 1;

        /// <summary>
        /// amount of horizontal units
        /// </summary>
        public int MaxWidth => FieldCountX * FieldSize;

        /// <summary>
        /// amount of vertical units
        /// </summary>
        public int MaxHeight => FieldCountY * FieldSize;

        public int MinWidth => 0;

        public int MinHeight => 0;

        /// <summary>
        /// interval of which the snake gets updated
        /// </summary>
        public int GlobalTickRate => Convert.ToInt32(Math.Round(100 * SpeedMultiplier, MidpointRounding.AwayFromZero));

        /// <summary>
        /// interval in milliseconds of which apples can be generated
        /// </summary>
        public int FoodTickRate => Convert.ToInt32(Math.Round(1000 * SpeedMultiplier * FoodInterval, MidpointRounding.AwayFromZero));

        public SnakeOption()
        {
            FieldSize = 16;

            FieldCountX = 100;
            FieldCountY = 100;

            SpeedMultiplier = 0.075;
            FoodInterval = 3;
            MaxFoodCount = 5;

            IsDefault = false;
            Name = "Custom";
        }

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
