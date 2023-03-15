﻿using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Age_Of_Nothing.Events;
using Age_Of_Nothing.Sprites;

namespace Age_Of_Nothing.SpritesUi
{
    /// <summary>
    /// Logique d'interaction pour VillagerUi.xaml
    /// </summary>
    public partial class VillagerUi : BaseSpriteUi<Villager>
    {
        private const int IndexZ = 2;
        private const double FocusStroke = 1;
        private const double SpaceBetween = 1;

        private static double StrokeAndSpace => FocusStroke + SpaceBetween;
        private static double TotalStrokeSize => StrokeAndSpace * 2;

        private static readonly Brush _defaultBrush = Brushes.SandyBrown;
        private static readonly Brush _defaultBrushHover = Brushes.PeachPuff;

        private Brush _goldBrush;
        private Brush _woodBrush;
        private Brush _rockBrush;
        private Brush _goldBrushHover;
        private Brush _woodBrushHover;
        private Brush _rockBrushHover;

        private Brush GoldBrush => _goldBrush ??= GetImageFill(_defaultBrush, "gold");
        private Brush WoodBrush => _woodBrush ??= GetImageFill(_defaultBrush, "wood");
        private Brush RockBrush => _rockBrush ??= GetImageFill(_defaultBrush, "rock");
        private Brush GoldBrushHover => _goldBrushHover ??= GetImageFill(_defaultBrushHover, "gold");
        private Brush WoodBrushHover => _woodBrushHover ??= GetImageFill(_defaultBrushHover, "wood");
        private Brush RockBrushHover => _rockBrushHover ??= GetImageFill(_defaultBrushHover, "rock");

        private readonly Shape _surround;
        private readonly Shape _visual;

        public VillagerUi(Villager villager)
            : base(villager)
        {
            InitializeComponent();

            SetValue(Panel.ZIndexProperty, IndexZ);

            _visual = new Ellipse
            {
                Width = Sprite.Surface.Width,
                Height = Sprite.Surface.Height,
                Fill = _defaultBrush
            };
            MainCanvas.Children.Add(_visual);

            _surround = new Ellipse
            {
                Stroke = Brushes.Black,
                StrokeThickness = FocusStroke,
                Width = Sprite.Surface.Width + TotalStrokeSize,
                Height = Sprite.Surface.Height + TotalStrokeSize,
                Fill = Brushes.Transparent
            };
            
            // do not move this line above the _visual definition
            SetControlDimensionsAndPosition();

            MouseEnter += (a, b) => _visual.Fill = GetFill();
            MouseLeave += (a, b) => _visual.Fill = GetFill();
            MouseLeftButtonDown += (a, b) => Sprite.ToggleFocus();

            Sprite.PropertyChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e is SpriteFocusChangedEventArgs)
                    {
                        SetControlDimensionsAndPosition();
                        if (Sprite.Focused)
                            MainCanvas.Children.Add(_surround);
                        else
                            MainCanvas.Children.Remove(_surround);
                    }
                    else if (e is SpritePositionChangedEventArgs)
                        SetControlDimensionsAndPosition();
                    else if (e.PropertyName == FocusableSprite.ResourcesChanged)
                        _visual.Fill = GetFill();
                }));
            };
        }

        private Brush GetFill()
        {
            return Sprite.IsCarrying() switch
            {
                PrimaryResources.Gold => IsMouseOver ? GoldBrushHover : GoldBrush,
                PrimaryResources.Wood => IsMouseOver ? WoodBrushHover : WoodBrush,
                PrimaryResources.Rock => IsMouseOver ? RockBrushHover : RockBrush,
                _ => IsMouseOver ? _defaultBrushHover : _defaultBrush
            };
        }

        private void SetControlDimensionsAndPosition()
        {
            MainCanvas.Width = Sprite.Surface.Width + (Sprite.Focused ? TotalStrokeSize : 0);
            MainCanvas.Height = Sprite.Surface.Height + (Sprite.Focused ? TotalStrokeSize : 0);
            SetValue(Canvas.LeftProperty, Sprite.Surface.Left - (Sprite.Focused ? StrokeAndSpace : 0));
            SetValue(Canvas.TopProperty, Sprite.Surface.Top - (Sprite.Focused ? StrokeAndSpace : 0));
            _visual.SetValue(Canvas.LeftProperty, Sprite.Focused ? StrokeAndSpace : double.NaN);
            _visual.SetValue(Canvas.TopProperty, Sprite.Focused ? StrokeAndSpace : double.NaN);
        }
    }
}